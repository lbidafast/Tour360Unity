using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(Player))]
    public class PlayerEditor : UnityEditor.Editor
    {
        private SerializedProperty centerCam;
        private SerializedProperty vrGazePointer;

        /// <summary>
        /// OnEnable
        /// </summary>
        public void OnEnable()
        {
            Player p;
            centerCam = serializedObject.FindProperty(nameof(p.CenterCamera));
            vrGazePointer = serializedObject.FindProperty(nameof(p.VRGazePointer));
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            GameObject playerGO = ((Player)target).gameObject;
            // Check if in project window / prefab mode
            if (EditorTools.IsInPrefabView(playerGO))
            {
                DrawDefaultInspector();
                return;
            }

            EditorGUILayout.PropertyField(centerCam, new GUIContent("Center Camera"));
            EditorGUILayout.PropertyField(vrGazePointer, new GUIContent("Enable Gaze Pointer (VR only)"));
            vrGazePointer.AddTooltip();
            serializedObject.ApplyModifiedProperties();
        }
    }
}