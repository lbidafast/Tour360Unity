using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using static CrizGames.Vour.Location;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(Location))]
    public class LocationEditor : UnityEditor.Editor
    {
        private SerializedProperty locationType;
        private SerializedProperty mediaType;
        private SerializedProperty texture;
        private SerializedProperty video;
        private SerializedProperty videoURL;
        private SerializedProperty streamingAssetsVidPath;
        private SerializedProperty videoLocationType;
        private SerializedProperty scaleToFullscreen;
        private SerializedProperty lockCamera;
        private SerializedProperty loopVideo;
        private SerializedProperty videoVolume;
        private SerializedProperty videoUI;
        private SerializedProperty videoUIAudioVolume;
        private SerializedProperty videoUILoopButton;
        private SerializedProperty scene;
        private SerializedProperty _3DLayout;
        private SerializedProperty rotOffset;


        private static string[] _mediaTypes = {"2D", "3D", "180", "180 3D", "360", "360 3D"};
        private static GUIContent[] _mediaTypesContent = _mediaTypes.Select(x => new GUIContent(x)).ToArray();
        
        private void OnEnable()
        {
            Location l;
            locationType = serializedObject.FindProperty(nameof(l.locationType));
            mediaType = serializedObject.FindProperty(nameof(l.displayType));
            texture = serializedObject.FindProperty(nameof(l.texture));
            video = serializedObject.FindProperty(nameof(l.video));
            videoURL = serializedObject.FindProperty(nameof(l.videoURL));
            streamingAssetsVidPath = serializedObject.FindProperty(nameof(l.streamingAssetsVidPath));
            videoLocationType = serializedObject.FindProperty(nameof(l.videoLocationType));
            scaleToFullscreen = serializedObject.FindProperty(nameof(l.scaleToFullscreen));
            lockCamera = serializedObject.FindProperty(nameof(l.lockCamera));
            loopVideo = serializedObject.FindProperty(nameof(l.loopVideo));
            videoVolume = serializedObject.FindProperty(nameof(l.videoVolume));
            videoUI = serializedObject.FindProperty(nameof(l.videoUI));
            videoUIAudioVolume = serializedObject.FindProperty(nameof(l.videoUIAudioVolume));
            videoUILoopButton = serializedObject.FindProperty(nameof(l.videoUILoopButton));
            scene = serializedObject.FindProperty(nameof(l.scene));
            _3DLayout = serializedObject.FindProperty(nameof(l.layout3D));
            rotOffset = serializedObject.FindProperty(nameof(l.rotOffset));

            //Location l = (Location)target;
            //switch (l.locationType)
            //{
            //    case LocationType.Video:
            //    case LocationType.Video3D:
            //    case LocationType.Video360:
            //    case LocationType.Video3D360:
            //        if (l.videoType == VideoType.URL)
            //            break;

            //        if (previewPlayerObj == null)
            //        {
            //            previewPlayerObj = new GameObject("VideoPreview");
            //            previewPlayerObj.hideFlags = HideFlags.DontSave;
            //            //previewPlayerObj.hideFlags = HideFlags.HideAndDontSave;
            //            previewPlayerObj.AddComponent<VideoPlayer>();
            //        }

            //        previewPlayer = previewPlayerObj.GetComponent<VideoPlayer>();

            //        VideoPlayer.FrameReadyEventHandler frameReadyHandler = null;
            //        bool oldSendFrameReadyEvents = previewPlayer.sendFrameReadyEvents;
            //        frameReadyHandler = (source, index) => {
            //            previewPlayer.sendFrameReadyEvents = oldSendFrameReadyEvents;
            //            previewPlayer.frameReady -= frameReadyHandler;
            //            // Callback
            //            FirstVideoFramePreview();
            //        };

            //        previewPlayer.frameReady += frameReadyHandler;
            //        previewPlayer.sendFrameReadyEvents = true;

            //        if (l.videoType == VideoType.Local)
            //            previewPlayer.clip = l.Video;
            //        else if (l.videoType == VideoType.StreamingAssets)
            //            previewPlayer.url = l.VideoURL;

            //        previewPlayer.Prepare();
            //        previewPlayer.StepForward();

            //        break;
            //}
        }

        
        private void OnDisable()
        {
            if (EditorTools.IsEditorInPlayMode())
                return;

            Location l = (Location)target;

            if (!l)
                return;

            Transform tResult = null;
            if (Selection.activeTransform != null)
                tResult = FindDeepChild(l.transform, Selection.activeTransform.name);

            if (tResult == null)
            {
                LocationManager manager = LocationManager.GetManager();

                if (manager == null)
                    return;

                manager.DeactivateLocationViews();

                if (l != null && l.gameObject.activeSelf)
                    l.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// FindDeepChild (Breadth-first search)
        /// </summary>
        public static Transform FindDeepChild(Transform parent, string name)
        {
            Queue<Transform> queue = new Queue<Transform>();
            queue.Enqueue(parent);
            while (queue.Count > 0)
            {
                Transform child = queue.Dequeue();

                // Return if found
                if (child.name == name)
                    return child;

                // Add childs children to search queue
                foreach (Transform childsChild in child)
                    queue.Enqueue(childsChild);
            }

            return null;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Location l = (Location)target;
            LocationManager manager = LocationManager.GetManager();

            if (manager == null)
            {
                EditorGUILayout.HelpBox("There is no Location Manager! You need to have one in your scene!", MessageType.Error);
                if (GUILayout.Button("Add Location Manager to Scene"))
                    MenuEntries.AddLocationManager();
                return;
            }

            if (!EditorTools.IsEditorInPlayMode())
                ActivateLocation(manager, l);

            EditorGUI.BeginChangeCheck();

            // Location Type
            EditorGUILayout.PropertyField(locationType, new GUIContent("Location Type"));

            // File source
            DrawFileProperties(l);
            serializedObject.ApplyModifiedProperties();

            bool propertiesChanged = EditorGUI.EndChangeCheck();

            // Display media in scene
            if (propertiesChanged || !EditorTools.IsEditorInPlayMode())
            {
                l.SetData();

                if (!EditorTools.IsEditorInPlayMode())
                    manager.SwitchCurrentLocationView(l);
            }

            // Add teleport point & info point buttons
            if (!EditorTools.IsEditorInPlayMode() && l.locationType != LocationType.Scene)
            {
                void InstantiateObj(GameObject obj)
                {
                    Selection.activeGameObject = (GameObject)PrefabUtility.InstantiatePrefab(obj, l.transform);
                }

                EditorGUILayout.Space();

                // Teleport point button
                if (GUILayout.Button("Add Teleport Point"))
                    InstantiateObj(VourSettings.Instance.defaultTeleportPoint);

                // Place buttons next to each other
                GUILayout.BeginHorizontal();

                // Info point button
                if (GUILayout.Button("Add Info Point"))
                    InstantiateObj(VourSettings.Instance.defaultInfoPoint);

                // Info panel button
                if (GUILayout.Button("Add Info Panel"))
                    InstantiateObj(VourSettings.Instance.defaultInfoPanel);

                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();

                // Video point button
                if (GUILayout.Button("Add Video Point"))
                    InstantiateObj(VourSettings.Instance.defaultVideoPoint);

                // Video panel button
                if (GUILayout.Button("Add Video Panel"))
                    InstantiateObj(VourSettings.Instance.defaultVideoPanel);

                GUILayout.EndHorizontal();
            }
        }

        private void DisplayTypeProperty(ref DisplayType value, string strLabel)
        {
            var rect = EditorGUILayout.GetControlRect(true);
            var label = EditorGUI.BeginProperty(rect, new GUIContent(strLabel), mediaType);
            
            EditorGUI.BeginChangeCheck();
            var newIdx = EditorGUI.Popup(rect, label, mediaType.enumValueIndex, _mediaTypesContent);
            if (EditorGUI.EndChangeCheck())
                mediaType.enumValueIndex = newIdx;
            
            EditorGUI.EndProperty();
        }

        private void DrawFileProperties(Location loc)
        {
            // 3D Only
            bool show3DLayout = false;
            switch (loc.displayType)
            {
                case DisplayType._3D:
                case DisplayType._180_3D:
                case DisplayType._3603D:
                    show3DLayout = true;
                    break;
            }

            // IMAGE or VIDEO
            if (loc.locationType.IsImage() || loc.locationType.IsVideo())
            {
                DisplayTypeProperty(ref loc.displayType, "Display Type");
                
                if (show3DLayout)
                    EditorGUILayout.PropertyField(_3DLayout, new GUIContent("3D Layout"));
            }

            // IMAGE
            if (loc.locationType.IsImage())
            {
                EditorGUILayout.PropertyField(texture, new GUIContent("Texture"));
            }
            // VIDEO
            else if (loc.locationType.IsVideo())
            {
                EditorGUILayout.PropertyField(videoLocationType, new GUIContent("Video Location"));

                switch (loc.videoLocationType)
                {
                    case VideoLocationType.Local:
                        EditorGUILayout.PropertyField(video, new GUIContent("Video"));
                        break;

                    case VideoLocationType.StreamingAssets:
                        EditorGUILayout.PropertyField(streamingAssetsVidPath, new GUIContent("Streaming Assets Video Path"));
                        break;

                    case VideoLocationType.URL:
                        EditorGUILayout.PropertyField(videoURL, new GUIContent("Video URL"));
                        break;
                }

                EditorGUILayout.PropertyField(loopVideo, new GUIContent("Loop Video"));

                EditorGUILayout.PropertyField(videoVolume, new GUIContent("Volume"));

                EditorGUILayout.PropertyField(videoUI, new GUIContent("Enable Video UI"));
                if (loc.videoUI)
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(videoUIAudioVolume, new GUIContent("Show Audio Button"));
                    EditorGUILayout.PropertyField(videoUILoopButton, new GUIContent("Show Loop Button"));
                    EditorGUI.indentLevel--;
                }
            }
            // SCENE
            else if (loc.locationType == LocationType.Scene)
            {
                EditorGUILayout.PropertyField(scene, new GUIContent("Scene"));
            }

            // 180/360 Only
            if (loc.displayType.Is180() || loc.displayType.Is360())
            {
                EditorGUILayout.PropertyField(rotOffset, new GUIContent("Rotation"));
            }
            // Not 180/360 only
            else if (loc.locationType != LocationType.Scene)
            {
                if (loc.locationType != LocationType.Empty)
                {
                    EditorGUILayout.PropertyField(scaleToFullscreen, new GUIContent("Scale to Fullscreen"));
                    scaleToFullscreen.AddTooltip();
                }
                EditorGUILayout.PropertyField(lockCamera, new GUIContent("Lock Camera (Non-VR Only)"));
            }
        }

        public static void ActivateLocation(LocationManager manager, Location l)
        {
            manager.DeactivateLocationViews();
            manager.DeactivateLocations();

            // Activate this location
            if (!l.gameObject.activeSelf)
                l.gameObject.SetActive(true);
            manager.SetLocationViewActive(l, true);
        }
    }
}