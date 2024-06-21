using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CrizGames.Vour
{
    //[CreateAssetMenu(menuName = "Vour Settings", fileName = "VourSettings", order = 1000)]
    public class VourSettings : ScriptableObject
    {
        public const string AssetName = "VourSettings";

        private static VourSettings _instance;
        public static VourSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<VourSettings>(AssetName);
#if UNITY_EDITOR
                    if (_instance == null)
                    {
                        AssetDatabase.CreateAsset(CreateInstance<VourSettings>(), $"Assets/Resources/{AssetName}.asset");
                        AssetDatabase.SaveAssets();

                        Debug.LogError("No VourSettings object was found in \"Resources\" folder! Created a new empty one. You need to configure it!");
                    }
#endif
                }

                return _instance;
            }
        }

        [Header("Default Placeables")]
        public GameObject defaultTeleportPoint;
        public GameObject defaultInfoPoint;
        public GameObject defaultInfoPanel;
        public GameObject defaultVideoPoint;
        public GameObject defaultVideoPanel;
        
        [Header("Info Panels")]
        public GameObject infoPanelTextOnly;
        public GameObject infoPanelLeftImage;
        public GameObject infoPanelRightImage;
        
        [Header("Misc")]
        public GameObject locationManagerPrefab;
        public GameObject playerPrefab;
        public GameObject locationPrefab;
        
        [Space]
        [Tooltip("The loading texture is displayed at video locations as a placeholder until the video starts.")]
        public Texture2D loadingTexture;

        [Space] 
        [Tooltip("When enabled, the blink animation will stay black until the location has finished loading.")]
        public bool blinkWaitUntilLocationIsReady = true;
    }
}