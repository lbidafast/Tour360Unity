using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static CrizGames.Vour.Location;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(LocationManager))]
    public class LocationManagerEditor : UnityEditor.Editor
    {
        private static LocationManager manager;

        private SerializedProperty startLocation;

        /// <summary>
        /// OnEnable
        /// </summary>
        public void OnEnable()
        {
            LocationManager m;
            startLocation = serializedObject.FindProperty(nameof(m.startLocation));
        }

        /// <summary>
        /// AddActivateLocationEvent
        /// </summary>
        [UnityEditor.Callbacks.DidReloadScripts]
        public static void AddActivateLocationEvent()
        {
            Selection.selectionChanged -= ActivateLocationIfChildSelected;
            Selection.selectionChanged += ActivateLocationIfChildSelected;
        }

        /// <summary>
        /// ActivateLocationIfChildSelected
        /// </summary>
        public static void ActivateLocationIfChildSelected()
        {
            if (EditorTools.IsEditorInPlayMode())
                return;

            LocationManager manager = LocationManager.GetManager();
            if (manager == null)
                return;

            // Search for parent
            if (Selection.activeGameObject == null)
            {
                manager.DeactivateLocationViews();
                manager.DeactivateLocations();
                return;
            }

            // Search for a parent with Location script
            Transform parent = Selection.activeGameObject.transform;
            Location l = null;
            while (parent != null && l == null)
            {
                parent = parent.parent;
                if (parent)
                    l = parent.GetComponent<Location>();
            }

            if (l != null)
            {
                l.SetData();
                LocationEditor.ActivateLocation(manager, l);
            }
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            manager = (LocationManager)target;

            // If prefab or something else
            GameObject locationManagerObj = manager.gameObject;
            if (EditorTools.IsInPrefabView(locationManagerObj))
            {
                DrawDefaultInspector();
                return;
            }

            EditorGUILayout.PropertyField(startLocation, new GUIContent("Start Location"));
            serializedObject.ApplyModifiedProperties();

            if (manager.startLocation == null)
                EditorGUILayout.HelpBox("Start Location is not assigned.", MessageType.Error);
        }
    }
}