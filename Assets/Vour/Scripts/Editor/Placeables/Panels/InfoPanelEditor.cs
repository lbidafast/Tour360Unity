using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrizGames.Vour.Editor
{
    [CustomEditor(typeof(InfoPanel))]
    public class InfoPanelEditor : UnityEditor.Editor
    {
        private SerializedProperty customPanel;
        private SerializedProperty customPanelObject;
        private SerializedProperty title;
        private SerializedProperty image;
        private SerializedProperty text;

        private bool inPrefabView;

        /// <summary>
        /// OnEnable
        /// </summary>
        protected virtual void OnEnable()
        {
            InfoPanel p = (InfoPanel)target;
            
            customPanel = serializedObject.FindProperty(nameof(p.customPanel));
            customPanelObject = serializedObject.FindProperty(nameof(p.customPanelObject));
            title = serializedObject.FindProperty(nameof(p.title));
            image = serializedObject.FindProperty(nameof(p.image));
            text = serializedObject.FindProperty(nameof(p.text));

            if (EditorTools.IsEditorInPlayMode())
                return;

            inPrefabView = EditorTools.IsInPrefabView(p.gameObject);
            if (inPrefabView)
                return;

            SetPanel(true);

            if (p.transform.childCount > 0)
                p.InitPanel();
        }

        /// <summary>
        /// Set Panel Visibility
        /// </summary>
        protected virtual void SetPanel(bool value)
        {
            InfoPanel i = (InfoPanel)target;
            if (i.panel == null)
                return;
            GameObject panel = i.panel.gameObject;
            if (panel)
                panel.SetActive(value);
        }

        /// <summary>
        /// OnInspectorGUI
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            InfoPanel i = (InfoPanel)target;

            if (inPrefabView)
            {
                DrawDefaultInspector();
                return;
            }

            // Custom panel
            EditorGUILayout.PropertyField(customPanel, new GUIContent("Custom Panel"));
            if (i.customPanel)
            {
                EditorGUILayout.PropertyField(customPanelObject, new GUIContent("Custom Panel Object"));
                serializedObject.ApplyModifiedProperties();

                if (i.customPanelObject == null)
                    EditorGUILayout.HelpBox("You need to set Custom Panel Object!", MessageType.Warning);

                else if (!i.customPanelObject.activeSelf)
                    i.customPanelObject.SetActive(true);

                // Disable built-in panels
                SetPanel(false);
                return;
            }
            else if (i.customPanelObject != null && i.customPanelObject.activeSelf)
            {
                i.customPanelObject.SetActive(false);
            }

            // Built-in panels
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(title, new GUIContent("Title"));
            EditorGUILayout.PropertyField(image, new GUIContent("Image"));

            GameObject prefab = null;
            if (i.image != null)
            {
                var prevPanelType = i.panelType;
                i.panelType = (InfoPoint.InfoPanelImageType)EditorGUILayout.EnumPopup("Image", i.panelType);
                if (i.panelType != prevPanelType)
                    Undo.RecordObject(i, $"Updated Info Point ({i.name}) Panel");

                switch (i.panelType)
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

            serializedObject.ApplyModifiedProperties();

            // Instantiate
            Transform panelParent = i.panelParent;
            GameObject panel = i.panel != null ? i.panel.gameObject : null;

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
                i.InitPanel();

                // Notify objects that they changed
                Undo.RecordObject(panel.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>(), "Updated Info Title");
                Undo.RecordObject(panel.transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>(), "Updated Info Text");

                if (panel.transform.childCount > 3)
                    Undo.RecordObject(panel.transform.GetChild(3).GetComponent<UnityEngine.UI.Image>(), "Updated Info Image");
            }
        }
    }
}