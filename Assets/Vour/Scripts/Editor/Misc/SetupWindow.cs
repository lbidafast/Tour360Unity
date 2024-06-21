using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
#if VOUR_OPENXR
using UnityEditor.XR.OpenXR;
using UnityEditor.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.OculusQuestSupport;
#endif

namespace CrizGames.Vour.Editor
{
    public class SetupWindow : EditorWindow
    {
        [System.Serializable]
        private class OptionalFeature
        {
            public string name;
            public BuildTargetGroup buildTargetGroup;
            public BuildTarget[] buildTargets;
            public PackageIdUrl[] packages;
            public string xrLoader;
            public string[] openXRFeatures;
            
            public bool toggled;
            public bool stateChanged;

            public int GetSupportedBuildTarget()
            {
                for (var i = 0; i < buildTargets.Length; i++)
                    if (BuildPipeline.IsBuildTargetSupported(buildTargetGroup, buildTargets[i]))
                        return i;
                return -1;
            }

            public bool IsInstalledAndEnabled() => IsFeatureInstalled() && IsLoaderEnabled();

            public bool IsFeatureInstalled()
            {
                return VourPackageManager.AllPackagesInstalled(packages);
            }

            public bool IsLoaderEnabled() => EditorTools.IsXRLoaderEnabled(xrLoader, buildTargetGroup);

            public void SetupAndEnableLoaderEvent()
            {
                EditorApplication.update -= SetupAndEnableLoaderEvent;
                
#if VOUR_XRPLUGINMANAGEMENT
                // Make sure settings for this platform are available
                var settings = EditorTools.GetOrCreateXRSettingsForBuildTarget();
                if (!settings.HasManagerSettingsForBuildTarget(buildTargetGroup))
                    settings.CreateDefaultManagerSettingsForBuildTarget(buildTargetGroup);
#endif
                bool loaderSet = EditorTools.SetXRLoader(xrLoader, buildTargetGroup, true);

                bool featuresSet = openXRFeatures == null;
                bool xrErrorsFixed = false;
#if VOUR_OPENXR
                if (openXRFeatures != null)
                {
                    foreach (var profile in openXRFeatures)
                    {
                        var feature = FeatureHelpers.GetFeatureWithIdForBuildTarget(buildTargetGroup, profile);
                        if (feature != null)
                            feature.enabled = true;
                    }
                    featuresSet = true;
                }

                var xrErrors = GetValidationErrors(buildTargetGroup);
                xrErrorsFixed = xrErrors.Count == 0;
                FixValidationIssues(xrErrors);
#endif
                if (!loaderSet || !featuresSet || !xrErrorsFixed)
                    EditorApplication.update += SetupAndEnableLoaderEvent;
            } 
            
            public void DisableLoaderEvent()
            {
                EditorTools.SetXRLoader(xrLoader, buildTargetGroup, false);
            }
        }

        private static readonly OptionalFeature[] OptionalFeatures = 
        {
            new OptionalFeature
            {
                name = "Desktop VR",
                buildTargetGroup = BuildTargetGroup.Standalone,
                buildTargets = new[] { BuildTarget.StandaloneWindows64, BuildTarget.StandaloneWindows, BuildTarget.StandaloneOSX, BuildTarget.StandaloneLinux64 },
                packages = new PackageIdUrl[]
                {
                    "com.unity.xr.management:4.2.1",
                    "com.unity.xr.openxr:1.3.1",
                    "com.unity.xr.interaction.toolkit:2.1.1",
                },
                xrLoader = "UnityEngine.XR.OpenXR.OpenXRLoader",
#if VOUR_OPENXR
                openXRFeatures = new []
                {
                    OculusTouchControllerProfile.featureId,
                    MicrosoftMotionControllerProfile.featureId,
                    ValveIndexControllerProfile.featureId,
                    HTCViveControllerProfile.featureId,
                    KHRSimpleControllerProfile.featureId
                }
#endif
            },
            new OptionalFeature
            {
                name = "Oculus Quest",
                buildTargetGroup = BuildTargetGroup.Android,
                buildTargets = new[] {BuildTarget.Android},
                packages = new PackageIdUrl[]
                {
                    "com.unity.xr.management:4.2.1",
                    "com.unity.xr.openxr:1.3.1",
                    "com.unity.xr.interaction.toolkit:2.1.1",
                },
                xrLoader = "UnityEngine.XR.OpenXR.OpenXRLoader",
#if VOUR_OPENXR
                openXRFeatures = new []
                {
                    OculusTouchControllerProfile.featureId,
                    OculusQuestFeature.featureId
                }
#endif
            },
            new OptionalFeature
            {
                name = "Web VR",
                buildTargetGroup = BuildTargetGroup.WebGL,
                buildTargets = new[] {BuildTarget.WebGL},
                packages = new PackageIdUrl[]
                {
                    "com.unity.xr.management:4.2.1",
                    "com.unity.xr.interaction.toolkit:2.1.1",
                    "com.unity.burst:1.6.6",
                    new PackageIdUrl { ID = "com.atteneder.gltfast", VersionOrURL = "https://github.com/atteneder/glTFast.git#v4.8.5" },
                    new PackageIdUrl { ID = "com.de-panther.webxr", VersionOrURL = "https://github.com/De-Panther/unity-webxr-export.git?path=/Packages/webxr#webxr/0.15.0-preview" },
                    new PackageIdUrl { ID = "com.de-panther.webxr-input-profiles-loader", VersionOrURL = "https://github.com/De-Panther/webxr-input-profiles-loader.git?path=/Packages/webxr-input-profiles-loader#0.4.0" },
                },
                xrLoader = "WebXR.WebXRLoader"
            }
        };

        public static readonly PackageIdUrl[] RequiredPackages =
        {
            "com.unity.inputsystem:1.3.0"
        };

        private const string WindowTitle = "Vour Setup Window";
        private const string Title = "Vour Setup";

        private static readonly GUIContent OptionalFeaturesTitle = EditorGUIUtility.TrTextContent("Optional Features");

        private static readonly float WindowWidth = 300f;
        private static readonly float HeaderSizeX = 150;

        private const int WindowPadding = 20;

        private static GUIStyle HeaderLabelStyle => new GUIStyle(GUI.skin.label) {fontStyle = FontStyle.Bold, fontSize = 24, alignment = TextAnchor.MiddleCenter};

        private bool _setupRequired;

        private Texture2D _headerImg;
        private Vector2 _headerSize;

        private ReorderableList _featureList;

        public static void ShowWindow()
        {
            var window = GetWindow<SetupWindow>(true, WindowTitle, true);
            window.ShowUtility();
        }

        private void Init()
        {
            if (_headerImg == null)
            {
                _headerImg = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Vour/Textures/Editor/VourEditorLogo.png");
                _headerSize = new Vector2(HeaderSizeX, _headerImg.height * (HeaderSizeX / _headerImg.width));
            }
            
            CreateFeatureList();
            
            _setupRequired = !VourPackageManager.AllPackagesInstalled(RequiredPackages);
        }

        private void OnEnable()
        {
            Init();
        }

        private void OnGUI()
        {
            if(_setupRequired) 
                _setupRequired = !VourPackageManager.AllPackagesInstalled(RequiredPackages);
                
            var headerRect = new Rect(position.width / 2 - _headerSize.x / 2, 30, _headerSize.x, _headerSize.y);
            GUI.DrawTexture(headerRect, _headerImg);
            GUILayout.Space(_headerSize.y + 40);
            GUILayout.Label(Title, HeaderLabelStyle);
            GUILayout.Space(WindowPadding);

            var lastY = GUILayoutUtility.GetLastRect().y + WindowPadding;

            GUILayout.BeginHorizontal();
            GUILayout.Space(WindowPadding);
            GUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(EditorTools.IsEditorInPlayMode());
            DrawContent(lastY);
            EditorGUI.EndDisabledGroup();
            GUILayout.EndVertical();
            GUILayout.Space(WindowPadding);
            GUILayout.EndHorizontal();
            
            SetWindowSize();
        }

        private void DrawContent(float lastY)
        {
            var windowWidth = position.width;
            var windowWidthPadded = windowWidth - WindowPadding * 2;

            // Toolbar
            // var toolbarRect = new Rect(WindowPadding, lastY, windowWidthPadded, 24);
            // GUI.Toolbar(toolbarRect, 0, new[] {"Setup", "Settings"});
            // GUILayout.Space(24 + 10);

            if (_setupRequired)
            {
                var rect = GUILayoutUtility.GetRect(windowWidthPadded, 24);
                if (GUI.Button(rect, "Install required dependencies"))
                    VourPackageManager.InstallPackages(RequiredPackages);
                GUILayout.Space(10);
            }

            _featureList!.DoLayoutList();

            foreach (var feature in OptionalFeatures)
            {
                if (feature.stateChanged)
                {
                    if (feature.toggled)
                    {
                        feature.SetupAndEnableLoaderEvent();
                    }
                    else
                    {
                        // Uninstall
                        // var choice = EditorUtility.DisplayDialog("Uninstall Packages", "Are you sure you want to uninstall the packages for this feature?", "Yes", "Cancel");
                        // if (choice)
                        //     VourPackageManager.UninstallPackages(FilterOutSharedPackages(li).Select(p => p.id));
                        
                        // Disable loader
                        feature.DisableLoaderEvent();
                    }

                    feature.stateChanged = false;
                }
            }
        }

        private void DrawFeatureCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            var feature = OptionalFeatures[index];

            var isFeatureInstalled = feature.IsFeatureInstalled();

            var activeTargetGroup = BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget);
            var isDifferentPlatform = activeTargetGroup != feature.buildTargetGroup;
            var availableTargetIdx = feature.GetSupportedBuildTarget();
            var isGroupAvailable = availableTargetIdx >= 0;

            var disableAll = _setupRequired || VourPackageManager.IsRebuildingCache;
            
            feature.toggled = isFeatureInstalled && feature.IsLoaderEnabled();
            var preToggledState = feature.toggled;
            
            // Disable if setup required or rebuilding cache
            EditorGUI.BeginDisabledGroup(disableAll);
            
            var installButtonRect = GUILayoutUtility.GetRect(new GUIContent("Install"), EditorStyles.miniButton, GUILayout.ExpandWidth(false));
            installButtonRect = new Rect(rect.x + rect.width - installButtonRect.width, rect.y + 2, installButtonRect.width, installButtonRect.height);
            
            // Install button
            if (isGroupAvailable && !isFeatureInstalled && GUI.Button(installButtonRect, "Install") && !VourPackageManager.AllPackagesInstalled(feature.packages))
                VourPackageManager.InstallPackages(feature.packages);
            
            // Feature toggle/label
            // Disable if build target is not available/installed
            EditorGUI.BeginDisabledGroup(!isGroupAvailable);
            if (!isFeatureInstalled || !isGroupAvailable)
                GUI.Label(rect, feature.name);
            else
                feature.toggled = EditorGUI.ToggleLeft(rect, feature.name, preToggledState);
            EditorGUI.EndDisabledGroup();
            
            // Info icon
            if (!isGroupAvailable)
            {
                var toggleSize = EditorStyles.label.CalcSize(new GUIContent(feature.name));
                var infoBtnRect = new Rect(rect.x + toggleSize.x + 4, rect.y, rect.width - toggleSize.x, rect.height);
                var content = new GUIContent(EditorGUIUtility.IconContent("_Help@2x").image, "The required platform module is not installed.");
                if (GUI.Button(infoBtnRect, content, EditorStyles.label))
                {
                    // Open BuildPlayerWindow with buildTargetGroup selected
                    BuildPlayerWindow.ShowBuildPlayerWindow();
                    EditorUserBuildSettings.selectedBuildTargetGroup = feature.buildTargetGroup;
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.NoTarget);
                }
            }

            // Switch platform button
            /*var buttonRect = GUILayoutUtility.GetRect(new GUIContent("Switch Platform"), EditorStyles.miniButton, GUILayout.ExpandWidth(false));
            buttonRect = new Rect(rect.x + rect.width - buttonRect.width, rect.y + 2, buttonRect.width, buttonRect.height);
            if (feature.toggled && isDifferentPlatform && isGroupAvailable &&
                GUI.Button(buttonRect, "Switch Platform", EditorStyles.miniButton))
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(feature.buildTargetGroup, feature.buildTargets[availableTargetIdx]);
            }*/
            
            EditorGUI.EndDisabledGroup();

            feature.stateChanged = (feature.toggled != preToggledState);
        }

        private void CreateFeatureList()
        {
            _featureList = new ReorderableList(OptionalFeatures, typeof(OptionalFeature), false, true, false, false);
            _featureList.drawHeaderCallback = rect =>
            {
                var labelSize = EditorStyles.label.CalcSize(OptionalFeaturesTitle);
                var labelRect = new Rect(rect);
                labelRect.width = labelSize.x;

                EditorGUI.LabelField(labelRect, OptionalFeaturesTitle, EditorStyles.label);
            };
            _featureList.drawElementCallback = DrawFeatureCallback;
            _featureList.drawElementBackgroundCallback = (rect, index, isActive, isFocused) =>
            {
                var tex = GUI.skin.label.normal.background;
                if (tex == null && GUI.skin.label.normal.scaledBackgrounds.Length > 0)
                    tex = GUI.skin.label.normal.scaledBackgrounds[0];
                if (tex == null) return;

                GUI.DrawTexture(rect, GUI.skin.label.normal.background);
            };
            _featureList.drawFooterCallback = rect =>
            {
                var status = VourPackageManager.GetCurrentStatusDisplayText();
                GUI.Label(rect, EditorGUIUtility.TrTextContent(status), EditorStyles.label);
            };
            _featureList.elementHeightCallback = i => _featureList.elementHeight;
        }

        private void SetWindowSize()
        {
            var windowHeight = (30 + _headerSize.y + 40) + WindowPadding * 2 + (_featureList.elementHeight * _featureList.count + 36);
            if (_setupRequired) windowHeight += 34;
            if (VourPackageManager.GetCurrentStatusDisplayText() != "") windowHeight += 16;
            
            minSize = maxSize = new Vector2(WindowWidth, windowHeight);
        }
        
#if VOUR_OPENXR
        private static List<OpenXRFeature.ValidationRule> GetValidationIssues(BuildTargetGroup targetGroup)
        {
            var issues = new List<OpenXRFeature.ValidationRule>();
            OpenXRProjectValidation.GetCurrentValidationIssues(issues, targetGroup);
            return issues;
        }
        
        private static List<OpenXRFeature.ValidationRule> GetValidationErrors(BuildTargetGroup targetGroup)
        {
            return GetValidationIssues(targetGroup).Where(x => x.error).ToList();
        }
        
        private static void FixValidationIssues(List<OpenXRFeature.ValidationRule> issues)
        {
            foreach (var issue in issues)
            {
                if (issue.fixItAutomatic)
                    issue.fixIt();
            }
        }
#endif

        /// <summary>
        /// Filter out packages shared with other installed features
        /// </summary>
        private PackageIdUrl[] FilterOutSharedPackages(OptionalFeature feature)
        {
            var installedFeatures = OptionalFeatures.Where(f => f != feature && f.IsFeatureInstalled());
            var packages = installedFeatures.Select(f => f.packages).SelectMany(x => x).Distinct();
            return feature.packages.Where(p => !packages.Contains(p)).ToArray();
        }
    }
}