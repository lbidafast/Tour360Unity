using System;
using System.Collections;
using System.Collections.Generic;
using System.IO; // For VOUR_JSON
using UnityEditor.PackageManager; // For VOUR_JSON
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_2021_1_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
#if VOUR_XRPLUGINMANAGEMENT
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;
#endif

namespace CrizGames.Vour.Editor
{
    [InitializeOnLoad]
    public static class EditorTools
    {
        private static UnityEngine.Object _tagManagerAsset = AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset");
        
        static EditorTools()
        {
            CreateTags("LocationManager", "Location");
            
#if WEBXR_INPUT_PROFILES && UNITY_WEBGL && !(USING_URP || USING_HDRP)
            AddAlwaysIncludedShaders("glTF/PbrMetallicRoughness", "glTF/PbrSpecularGlossiness", "glTF/Unlit");
#endif
#if !VOUR_JSON
            AddJsonPackageIfNotPresent();
#else
            EditorApplication.update -= OpenSetupIfNeeded;
            EditorApplication.update += OpenSetupIfNeeded;
#endif
            // https://docs.unity3d.com/Manual/webgl-templates.html
            // Bug: preprocessor variables aren't initialized when setting template via code
            // if (PlayerSettings.WebGL.template == "APPLICATION:Default")
            //     PlayerSettings.WebGL.template = "PROJECT:Vour";
        }

#if !VOUR_JSON
        // Add package via text editing when json package isn't yet available
        // JsonUtility doesn't help, System.Text.Json isn't available yet (.NET 7)
        private static void AddJsonPackageIfNotPresent()
        {
            var path = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");
            var json = File.ReadAllText(path);
            if (!json.Contains("com.unity.nuget.newtonsoft-json"))
                json = json.Replace("dependencies\": {", "dependencies\": {\n\t\"com.unity.nuget.newtonsoft-json\": \"2.0.2\",");
            File.WriteAllText(path, json);
            Client.Resolve();
        }
#endif

        private static void OpenSetupIfNeeded()
        {
            if (VourPackageManager.IsRebuildingCache || IsEditorInPlayMode())
                return;
            
            if (!VourPackageManager.AllPackagesInstalled(SetupWindow.RequiredPackages))
                SetupWindow.ShowWindow();

            EditorApplication.update -= OpenSetupIfNeeded;
        }
        
        public static bool IsEditorInPlayMode()
        {
            return EditorApplication.isPlayingOrWillChangePlaymode ||
                   EditorApplication.isPlaying ||
                   EditorApplication.isPaused;
        }
        
        public static bool IsInPrefabView(GameObject obj)
        {
            return !obj.scene.IsValid() || PrefabStageUtility.GetCurrentPrefabStage() != null;
        }

        public static void AddTooltip(this SerializedProperty property)
        {
            GUI.Label(GUILayoutUtility.GetLastRect(), new GUIContent("", property.tooltip));
        }
        
        private static void CreateTags(params string[] tags) {
            if (_tagManagerAsset != null) {
                var so = new SerializedObject(_tagManagerAsset);
                var tagsArray = so.FindProperty("tags");

                var numTags = tagsArray.arraySize;
                bool tagsChanged = false;
                
                foreach (var tag in tags)
                    AddTag(tag);

                if (tagsChanged)
                {
                    so.ApplyModifiedProperties();
                    so.Update();
                }

                void AddTag(string tag)
                {
                    // Do not create duplicates
                    for (int i = 0; i < numTags; i++) {
                        var existingTag = tagsArray.GetArrayElementAtIndex(i);
                        if (existingTag.stringValue == tag) return;
                    }

                    tagsArray.InsertArrayElementAtIndex(numTags);
                    tagsArray.GetArrayElementAtIndex(numTags).stringValue = tag;
                    numTags = tagsArray.arraySize;
                    tagsChanged = true;
                }
            }
        }

        // https://forum.unity.com/threads/modify-always-included-shaders-with-pre-processor.509479/#post-3509413
        private static void AddAlwaysIncludedShaders(params string[] shaderNames)
        {
            var graphicsSettingsObj = AssetDatabase.LoadAssetAtPath<GraphicsSettings>("ProjectSettings/GraphicsSettings.asset");
            if(!graphicsSettingsObj)
                return;

            var serializedObject = new SerializedObject(graphicsSettingsObj);
            var arrayProp = serializedObject.FindProperty("m_AlwaysIncludedShaders");
            
            bool addedShaders = false;
            foreach (var shaderName in shaderNames)
            {
                var shader = Shader.Find(shaderName);
                if (shader == null)
                    continue;
            
                bool hasShader = false;
                for (int i = 0; i < arrayProp.arraySize; ++i)
                {
                    var arrayElem = arrayProp.GetArrayElementAtIndex(i);
                    if (shader == arrayElem.objectReferenceValue)
                    {
                        hasShader = true;
                        break;
                    }
                }

                if (!hasShader)
                {
                    int arrayIndex = arrayProp.arraySize;
                    arrayProp.InsertArrayElementAtIndex(arrayIndex);
                    var arrayElem = arrayProp.GetArrayElementAtIndex(arrayIndex);
                    arrayElem.objectReferenceValue = shader;

                    addedShaders = true;
                }
            }

            if (addedShaders)
            {
                serializedObject.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
            }
        }
        
#if VOUR_XRPLUGINMANAGEMENT
        // Created from XRGeneralSettingsPerBuildTarget.GetOrCreate because it is internal
        public static XRGeneralSettingsPerBuildTarget GetOrCreateXRSettingsForBuildTarget()
        {
            EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey, out var generalSettings);
            if (generalSettings == null)
            {
                string searchText = "t:XRGeneralSettings";
                string[] assets = AssetDatabase.FindAssets(searchText);
                if (assets.Length > 0)
                {
                    string path = AssetDatabase.GUIDToAssetPath(assets[0]);
                    generalSettings = AssetDatabase.LoadAssetAtPath(path, typeof(XRGeneralSettingsPerBuildTarget)) as XRGeneralSettingsPerBuildTarget;
                }
            }

            if (generalSettings == null)
            {
                generalSettings = ScriptableObject.CreateInstance(typeof(XRGeneralSettingsPerBuildTarget)) as XRGeneralSettingsPerBuildTarget;
                string assetPath = GetAssetPathForComponents(new[]{"XR"});
                if (!string.IsNullOrEmpty(assetPath))
                {
                    assetPath = Path.Combine(assetPath, "XRGeneralSettings.asset");
                    AssetDatabase.CreateAsset(generalSettings, assetPath);
                }
            }

            EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, generalSettings, true);

            return generalSettings;
        }
        
        // Created from XRManagement EditorUtilities.GetAssetPathForComponents because it is internal
        private static string GetAssetPathForComponents(string[] pathComponents, string root = "Assets")
        {
            if (pathComponents.Length <= 0)
                return null;

            string path = root;
            foreach( var pc in pathComponents)
            {
                string subFolder = Path.Combine(path, pc);
                bool shouldCreate = true;
                foreach (var f in AssetDatabase.GetSubFolders(path))
                {
                    if (String.Compare(Path.GetFullPath(f), Path.GetFullPath(subFolder), StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        shouldCreate = false;
                        break;
                    }
                }

                if (shouldCreate)
                    AssetDatabase.CreateFolder(path, pc);
                path = subFolder;
            }

            return path;
        }
#endif

        public static bool IsXRLoaderEnabled(string loaderName, BuildTargetGroup group) =>
#if VOUR_XRPLUGINMANAGEMENT
            XRPackageMetadataStore.IsLoaderAssigned(loaderName, group);
#else
            false;
#endif

        public static bool SetXRLoader(string loaderName, BuildTargetGroup group, bool enabled)
        {
#if VOUR_XRPLUGINMANAGEMENT
            XRGeneralSettings target = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(group);

            if (target == null)
                return false;
            
            if (target.AssignedSettings == null)
            {
                XRManagerSettings instance = ScriptableObject.CreateInstance<XRManagerSettings>();
                target.AssignedSettings = instance;
                EditorUtility.SetDirty(target);
            }

            bool loaderEnabled = IsXRLoaderEnabled(loaderName, group);

            if (!loaderEnabled && enabled)
            {
                if (!XRPackageMetadataStore.AssignLoader(target.AssignedSettings, loaderName, group))
                {
                    Debug.LogError($"Unable to assign loader {loaderName} for build target {group}. " +
                                   $"Please try to enable it manually by checking the loader in Project Settings > XR Plug-in Management > {group} > Plug-in Providers.");
                    return false;
                }
            }
            else if(loaderEnabled && !enabled)
            {
                if (!XRPackageMetadataStore.RemoveLoader(target.AssignedSettings, loaderName, group))
                {
                    Debug.LogError($"Unable to remove loader {loaderName} for build target {group}. " +
                                   $"Please try to enable it manually by unchecking the loader in Project Settings > XR Plug-in Management > {group} > Plug-in Providers.");
                    return false;
                }
            }

            return true;
#else
            return false;
#endif
        }
    }
}