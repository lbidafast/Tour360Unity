using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrizGames.Vour
{
    public class Player : MonoBehaviour
    {
        public enum PlayerPlatformType
        {
            VR,
            WebVR,
            Desktop
        }

        public GameObject DesktopPlayer;
        public GameObject OpenXRPlayer;
        public GameObject WebXRPlayer;

        public bool CenterCamera = true;
        [Tooltip("The gaze pointer enables navigation without using VR controllers.")]
        public bool VRGazePointer = false;

        /// <summary>
        /// Awake
        /// </summary>
        private void Awake()
        {
            PlayerBase player = PlayerBase.Instance;
            if (player != null)
            {
                player.transform.SetPositionAndRotation(transform.position, transform.rotation);
                player.ResetRotation();
            }
            else
            {
                player = Instantiate(GetPlayerPrefab(), transform.position, transform.rotation).GetComponent<PlayerBase>();
                DontDestroyOnLoad(player.gameObject);
            }
            
            player.Init();
            player.SetCenterCam(CenterCamera);

            Destroy(gameObject);
        }

        /// <summary>
        /// GetPlayerPrefab
        /// </summary>
        private GameObject GetPlayerPrefab()
        {
            switch (GetPlayerPlatform())
            {
                case PlayerPlatformType.VR:
                    Debug.Log("VR Mode (OpenXR)");
                    return OpenXRPlayer;

                case PlayerPlatformType.WebVR:
                    Debug.Log("VR Mode (WebXR)");
                    return WebXRPlayer;

                case PlayerPlatformType.Desktop:
                default:
                    Debug.Log("PC Mode");
                    return DesktopPlayer;
            }
        }

        public static PlayerPlatformType GetPlayerPlatform()
        {
#if VOUR_WEBXR && UNITY_WEBGL
            if (Application.platform == RuntimePlatform.WebGLPlayer)
                return PlayerPlatformType.WebVR;
#endif
            
            if (Utils.InVR())
                return PlayerPlatformType.VR;

            return PlayerPlatformType.Desktop;
        }
    }
}