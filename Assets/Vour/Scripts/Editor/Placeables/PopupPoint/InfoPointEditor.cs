using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(InfoPoint))]
    public class InfoPointEditor : UnityEditor.Editor
    {
        private SerializedProperty customPanel;
        private SerializedProperty customPanelObject;
        private SerializedProperty title;
        private SerializedProperty image;
        private SerializedProperty text;
        private SerializedProperty rotateTowardsPlayer;

        private bool inPrefabView;

        /// <summary>
        /// OnEnable
        /// </summary>
        private void OnEnable()
        {
            InfoPoint p = (InfoPoint)target;
            
            customPanel = serializedObject.FindProperty(nameof(p.CustomPanel));
            customPanelObject = serializedObject.FindProperty(nameof(p.CustomPanelObject));
            title = serializedObject.FindProperty(nameof(p.Title));
            image = serializedObject.FindProperty(nameof(p.Image));
            text = serializedObject.FindProperty(nameof(p.Text));
            rotateTowardsPlayer = serializedObject.FindProperty(nameof(p.rotateTowardsPlayer));

            if (EditorTools.IsEditorInPlayMode())
                return;

            inPrefabView = EditorTools.IsInPrefabView(p.gameObject);
            if (inPrefabView)
                return;

            SetPanel(true);

            if (p.panelParent.childCount > 0)
                p.InitPanel();

            p.RotateTowardsPlayer();
        }

        /// <summary>
        /// OnDisable
        /// </summary>
        private void OnDisable()
        {
            if (EditorTools.IsEditorInPlayMode())
                return;

            InfoPoint p = (InfoPoint)target;

            if (p == null)
                return;

            p.RotateTowardsPlayer();
        }

        /// <summary>
        /// Set Panel Visibility and Update Rotation
        /// </summary>
        private void SetPanel(bool value)
        {
            Transform panelParent = ((InfoPoint)target).panelParent;
            GameObject panel = panelParent.childCount > 0 ? panelParent.GetChild(0).gameObject : null;
            if (panel)
                panel.SetActive(value);
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InfoPoint p = (InfoPoint)target;

            if (inPrefabView)
            {
                DrawDefaultInspector();
                return;
            }

            // Custom panel
            EditorGUILayout.PropertyField(customPanel, new GUIContent("Custom Panel"));
            if (p.CustomPanel)
            {
                EditorGUILayout.PropertyField(customPanelObject, new GUIContent("Custom Panel Object"));

                if (p.CustomPanelObject == null)
                    EditorGUILayout.HelpBox("You need to set Custom Panel Object in order for it to work in play mode!", MessageType.Warning);

                else if (!p.CustomPanelObject.activeSelf)
                    p.CustomPanelObject.SetActive(true);

                EditorGUILayout.PropertyField(rotateTowardsPlayer, new GUIContent("Rotate Panel Towards Player"));
                serializedObject.ApplyModifiedProperties();
                
                // Disable built-in panels
                SetPanel(false);
                return;
            }
            else if (p.CustomPanelObject != null && p.CustomPanelObject.activeSelf)
            {
                p.CustomPanelObject.SetActive(false);
            }

            // Built-in panels
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(title, new GUIContent("Title"));
            EditorGUILayout.PropertyField(image, new GUIContent("Image"));

            GameObject prefab = null;
            if (p.Image != null)
            {
                var prevPanelType = p.PanelType;
                p.PanelType = (InfoPoint.InfoPanelImageType)EditorGUILayout.EnumPopup("Image", p.PanelType);
                if (p.PanelType != prevPanelType)
                    Undo.RecordObject(p, $"Updated Info Point ({p.name}) Panel");

                switch (p.PanelType)
                {
                    case InfoPoint.InfoPanelImageType.LeftImage:
                        prefab = VourSettings.Instance.infoPanelLeftImage;
                        break;

                    case InfoPoint.InfoPanelImageType.RightImage:
                        prefab = VourSettings.Instance.infoPanelRightImage;
                        break;
                }
            }
            else
            {
                prefab = VourSettings.Instance.infoPanelTextOnly;
            }

            EditorGUILayout.PropertyField(text, new GUIContent("Text"));

            EditorGUILayout.Space();
            bool prevRot = rotateTowardsPlayer.boolValue;
            EditorGUILayout.PropertyField(rotateTowardsPlayer, new GUIContent("Rotate Panel Towards Player"));
            if (!prevRot && rotateTowardsPlayer.boolValue)
            {
                p.rotateTowardsPlayer = true;
                p.RotateTowardsPlayer();
            }

            serializedObject.ApplyModifiedProperties();

            // Instantiate
            Transform panelParent = p.panelParent;
            GameObject panel = panelParent.childCount > 0 ? panelParent.GetChild(0).gameObject : null;

            // Enable panel
            if (panel != null && !panel.activeSelf)
                panel.SetActive(true);

            // Add / Update Panel Prefab
            if (panel == null || panel.name != prefab.name)
            {
                if (panel != null)
                    DestroyImmediate(panel);
                panel = PrefabUtility.InstantiatePrefab(prefab, panelParent) as GameObject;
                EditorUtility.SetDirty(panel);
            }

            if (EditorGUI.EndChangeCheck())
            {
                // Update text
                p.InitPanel();

                // Notify objects that they changed
                Undo.RecordObject(panel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>(), "Updated Info Title");
                Undo.RecordObject(panel.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>(), "Updated Info Text");

                if (panel.transform.childCount > 3)
                    Undo.RecordObject(panel.transform.GetChild(3).GetComponent<UnityEngine.UI.Image>(), "Updated Info Image");
            }
        }
    }
}