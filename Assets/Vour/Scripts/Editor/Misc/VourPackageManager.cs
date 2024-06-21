using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager;
#if VOUR_JSON
using Newtonsoft.Json.Linq;
#endif

namespace CrizGames.Vour.Editor
{
    public struct PackageIdUrl
    {
        public string ID;
        public string VersionOrURL;

        public static implicit operator PackageIdUrl(string p)
        {
            var split = p.Split(':');
            if (split.Length == 1)
            {
                Debug.LogError($"Package {split[0]} has no version specified!");
                return new PackageIdUrl
                {
                    ID = split[0].Trim(),
                    VersionOrURL = "1.0.0"
                };
            } 
            else
            {
                return new PackageIdUrl
                {
                    ID = split[0].Trim(),
                    VersionOrURL = split[1].Trim()
                };
            }
        }
    }
    
    /// <summary>
    /// Install and uninstall packages
    /// Based on XRPackageMetadataStore from XR Plugin Management
    /// </summary>
    [InitializeOnLoad]
    public class VourPackageManager
    {
        const string k_RebuildCache = "Vour Setup Rebuilding Cache";
        const string k_CachedMDStoreKey = "Vour Setup Metadata Store";

        static float k_TimeOutDelta = 30f;
        
        static readonly string ManifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");

        [Serializable]
        struct CachedPackageInfo
        {
            public string[] installedPackages;
        }

        static CachedPackageInfo _cachedPackageInfo = new CachedPackageInfo
        {
            installedPackages = { },
        };

        static void LoadCachedMDStoreInformation()
        {
            string data = SessionState.GetString(k_CachedMDStoreKey, "{}");
            _cachedPackageInfo = JsonUtility.FromJson<CachedPackageInfo>(data);
        }

        static void StoreCachedMDStoreInformation()
        {
            SessionState.EraseString(k_CachedMDStoreKey);
            string data = JsonUtility.ToJson(_cachedPackageInfo, true);
            SessionState.SetString(k_CachedMDStoreKey, data);
        }

        enum LogLevel
        {
            Info,
            Warning,
            Error
        }

        [Serializable]
        struct PackageRequest
        {
            [SerializeField]
            public string packageId;
            [SerializeField]
            public ListRequest packageListRequest;
            [SerializeField]
            public float timeOut;
            [SerializeField]
            public string logMessage;
            [SerializeField]
            public LogLevel logLevel;
        }

        [Serializable]
        struct PackageRequests
        {
            [SerializeField]
            public List<PackageRequest> activeRequests;
        }

        const string k_DefaultSessionStateString = "DEFAULTSESSION";
        static bool SessionStateHasStoredData(string queueName)
        {
            return SessionState.GetString(queueName, k_DefaultSessionStateString) != k_DefaultSessionStateString;
        }

        public static bool IsRebuildingCache => SessionStateHasStoredData(k_RebuildCache);

        public static bool IsPackageInstalled(PackageIdUrl package)
        {
            return _cachedPackageInfo.installedPackages?.Contains(package.ID) ?? false;
        }
        
        public static bool AllPackagesInstalled(IEnumerable<PackageIdUrl> packages)
        {
            return packages.All(IsPackageInstalled);
        }

        public static void InstallPackages(IEnumerable<PackageIdUrl> packages)
        {
#if VOUR_JSON
            var manifest = JObject.Parse(File.ReadAllText(ManifestPath));
            
            foreach (var package in packages)
                manifest["dependencies"]![package.ID] = package.VersionOrURL;
            
            File.WriteAllText(ManifestPath, manifest.ToString());
            Client.Resolve();
#endif
        }
        
        public static void UninstallPackages(IEnumerable<PackageIdUrl> packages)
        {
#if VOUR_JSON
            var manifest = JObject.Parse(File.ReadAllText(ManifestPath));
            
            foreach (var package in packages)
                manifest["dependencies"]![package.ID]?.Remove();
            
            File.WriteAllText(ManifestPath, manifest.ToString());
            Client.Resolve();
#endif
        }

        public static string GetCurrentStatusDisplayText()
        {
            if (IsRebuildingCache)
                return "Querying Package Manager for currently installed packages...";

            return "";
        }

        static VourPackageManager()
        {
            EditorApplication.playModeStateChanged -= PlayModeStateChanged;
            EditorApplication.playModeStateChanged += PlayModeStateChanged;

            if (IsEditorInPlayMode())
                return;

            AssemblyReloadEvents.afterAssemblyReload -= AfterAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += AfterAssemblyReload;
        }

        static void Refresh()
        {
            if (!IsRebuildingCache)
                AfterAssemblyReload();
        }

        static void AfterAssemblyReload()
        {
            LoadCachedMDStoreInformation();

            if (!IsEditorInPlayMode())
            {
                RebuildInstalledCache();
                StartAllQueues();
            }
        }

        static bool IsEditorInPlayMode()
        {
            return EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorApplication.isPlaying ||
                EditorApplication.isPaused;
        }

        static void PlayModeStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    StopAllQueues();
                    StoreCachedMDStoreInformation();
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    LoadCachedMDStoreInformation();
                    StartAllQueues();
                    break;
            }
        }

        static void StopAllQueues()
        {
            EditorApplication.update -= RebuildCache;
        }

        static void StartAllQueues()
        {
            EditorApplication.update += RebuildCache;
        }

        static void AddRequestToQueue(PackageRequest request, string queueName)
        {
            PackageRequests reqs;

            if (SessionStateHasStoredData(queueName))
            {
                string fromJson = SessionState.GetString(queueName, k_DefaultSessionStateString);
                reqs = JsonUtility.FromJson<PackageRequests>(fromJson);
            }
            else
            {
                reqs = new PackageRequests();
                reqs.activeRequests = new List<PackageRequest>();
            }

            reqs.activeRequests.Add(request);
            string json = JsonUtility.ToJson(reqs);
            SessionState.SetString(queueName, json);
        }

        static void SetRequestsInQueue(PackageRequests reqs, string queueName)
        {
            string json = JsonUtility.ToJson(reqs);
            SessionState.SetString(queueName, json);
        }

        static PackageRequests GetAllRequestsInQueue(string queueName)
        {
            var reqs = new PackageRequests();
            reqs.activeRequests = new List<PackageRequest>();

            if (SessionStateHasStoredData(queueName))
            {
                string fromJson = SessionState.GetString(queueName, k_DefaultSessionStateString);
                reqs = JsonUtility.FromJson<PackageRequests>(fromJson);
                SessionState.EraseString(queueName);
            }

            return reqs;
        }

        static void RebuildInstalledCache()
        {
            if (IsRebuildingCache)
                return;

            var req = new PackageRequest();
            req.packageListRequest = Client.List(true, true);
            req.timeOut = Time.realtimeSinceStartup + k_TimeOutDelta;
            AddRequestToQueue(req, k_RebuildCache);
            EditorApplication.update += RebuildCache;
        }

        static void RebuildCache()
        {
            EditorApplication.update -= RebuildCache;

            if (IsEditorInPlayMode())
                return; // Use the cached data that should have been passed in the play state change.

            PackageRequests reqs = GetAllRequestsInQueue(k_RebuildCache);

            if (reqs.activeRequests == null || reqs.activeRequests.Count == 0)
                return;

            var req = reqs.activeRequests[0];
            reqs.activeRequests.Remove(req);

            if (req.timeOut < Time.realtimeSinceStartup)
            {
                req.logMessage = $"Timeout trying to get package list after {k_TimeOutDelta}s.";
                req.logLevel = LogLevel.Warning;
                Log(req);
                Refresh();
            }
            else if (req.packageListRequest.IsCompleted)
            {
                if (req.packageListRequest.Status == StatusCode.Success)
                {
                    var installedPackages = new List<string>();

                    foreach (var packageInfo in req.packageListRequest.Result)
                        installedPackages.Add(packageInfo.name);
                    
                    _cachedPackageInfo.installedPackages = installedPackages.ToArray();
                    //Debug.Log($"Installed packages: {string.Join(", ", installedPackages)}");
                }

                StoreCachedMDStoreInformation();
            }
            else if (!req.packageListRequest.IsCompleted)
            {
                AddRequestToQueue(req, k_RebuildCache);
                EditorApplication.update += RebuildCache;
            }
            else
            {
                req.logMessage = "Unable to rebuild installed package cache. Some state may be missing or incorrect.";
                req.logLevel = LogLevel.Warning;
                Log(req);
            }

            if (reqs.activeRequests.Count > 0)
            {
                SetRequestsInQueue(reqs, k_RebuildCache);
                EditorApplication.update += RebuildCache;
            }
        }

        static void Log(PackageRequest req)
        {
            /*const string header = "Vour";
            switch(req.logLevel)
            {
                case LogLevel.Info:
                    Debug.Log($"{header}: {req.logMessage}");
                    break;

                case LogLevel.Warning:
                    Debug.LogWarning($"{header} Warning: {req.logMessage}");
                    break;

                case LogLevel.Error:
                    Debug.LogError($"{header} error. Failure reason: {req.logMessage}.\n Check if there are any other errors in the console and make sure they are corrected before trying again.");
                    break;
            }*/
        }
    }
}