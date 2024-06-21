using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static CrizGames.Vour.Location;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(VideoPoint))]
    public class VideoPointEditor : UnityEditor.Editor
    {
        private SerializedProperty video;
        private SerializedProperty videoURL;
        private SerializedProperty streamingAssetsVidPath;
        private SerializedProperty videoType;
        private SerializedProperty loopVideo;
        private SerializedProperty videoVolume;
        private SerializedProperty videoUI;
        private SerializedProperty videoUIAudioVolume;
        private SerializedProperty videoUILoopButton;
        private SerializedProperty rotateTowardsPlayer;

        private bool inPrefabView;

        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            VideoPoint p = (VideoPoint)target;
            
            video = serializedObject.FindProperty(nameof(p.video));
            videoURL = serializedObject.FindProperty(nameof(p.videoURL));
            streamingAssetsVidPath = serializedObject.FindProperty(nameof(p.streamingAssetsVidPath));
            videoType = serializedObject.FindProperty(nameof(p.videoLocationType));
            loopVideo = serializedObject.FindProperty(nameof(p.loopVideo));
            videoVolume = serializedObject.FindProperty(nameof(p.videoVolume));
            videoUI = serializedObject.FindProperty(nameof(p.videoUI));
            videoUIAudioVolume = serializedObject.FindProperty(nameof(p.videoUIAudioVolume));
            videoUILoopButton = serializedObject.FindProperty(nameof(p.videoUILoopButton));
            rotateTowardsPlayer = serializedObject.FindProperty(nameof(p.rotateTowardsPlayer));

            if (EditorTools.IsEditorInPlayMode())
                return;

            inPrefabView = EditorTools.IsInPrefabView(p.gameObject);
            if (inPrefabView)
                return;

            p.RotateTowardsPlayer();
        }

        /// <summary>
        /// OnDisable
        /// </summary>
        private void OnDisable()
        {
            if (EditorTools.IsEditorInPlayMode() || inPrefabView)
                return;

            VideoPoint p = (VideoPoint)target;

            if (p == null)
                return;

            p.RotateTowardsPlayer();
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            VideoPoint p = (VideoPoint)target;

            if (inPrefabView)
            {
                DrawDefaultInspector();
                return;
            }

            DrawProperties(p);

            serializedObject.ApplyModifiedProperties();
        }

        void DrawProperties(VideoPoint point)
        {
            EditorGUILayout.PropertyField(videoType, new GUIContent("Video Type"));

            switch (point.videoLocationType)
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

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(videoUI, new GUIContent("Enable Video UI"));
            if (videoUI.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(videoUIAudioVolume, new GUIContent("Show Audio Button"));
                EditorGUILayout.PropertyField(videoUILoopButton, new GUIContent("Show Loop Button"));
                EditorGUI.indentLevel--;
            }
            if (EditorGUI.EndChangeCheck() && !EditorTools.IsEditorInPlayMode())
            {
                var ui = point.panel.GetComponent<VideoUIController>();
                if (videoUI.boolValue)
                    ui.EnableUI(null, videoUIAudioVolume.boolValue, videoUILoopButton.boolValue);
                else
                    ui.DisableUI();
            }

            EditorGUILayout.Space();

            bool prevRot = rotateTowardsPlayer.boolValue;
            EditorGUILayout.PropertyField(rotateTowardsPlayer, new GUIContent("Rotate Panel Towards Player"));
            if (!prevRot && rotateTowardsPlayer.boolValue)
            {
                point.rotateTowardsPlayer = true;
                point.RotateTowardsPlayer();
            }
        }
    }
}