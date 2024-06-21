using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if VOUR_XRI
using UnityEngine.XR.Interaction.Toolkit;
#endif
#if VOUR_XR
using UnityEngine.XR;
#endif
#if VOUR_XRPLUGINMANAGEMENT
using UnityEngine.XR.Management;
#endif
#if VOUR_WEBXR
using WebXR;
#endif
using static CrizGames.Vour.Location;

namespace CrizGames.Vour
{
#if VOUR_WEBXR
    [Flags]
    public enum WebControllerButtons
    {
        ButtonA = WebXRController.ButtonTypes.ButtonA,
        ButtonB = WebXRController.ButtonTypes.ButtonB,
        Grip = WebXRController.ButtonTypes.Grip,
        Thumbstick = WebXRController.ButtonTypes.Thumbstick,
        Touchpad = WebXRController.ButtonTypes.Touchpad,
        Trigger = WebXRController.ButtonTypes.Trigger
    }
#endif

    public static class Utils
    {
        public static bool IsUsingBuiltinRenderPipeline()
        {
            return QualitySettings.renderPipeline == null;
        }
        
        public static bool VRAvailable()
        {
#if VOUR_XRPLUGINMANAGEMENT
            // https://forum.unity.com/threads/how-to-detect-if-headset-is-available-and-initialize-xr-only-if-true.927134/
            return XRGeneralSettings.Instance.Manager.activeLoader != null || InVR();
#else
            return false;
#endif
        }
        
        public static bool InVR()
        {
#if VOUR_XR
            List<XRDisplaySubsystem> displaySubsystems = new List<XRDisplaySubsystem>();
            SubsystemManager.GetInstances(displaySubsystems);
            // If there are xr displays detected = VR is on
            return displaySubsystems.Count > 0;
#else
            return false;
#endif
        }

#if VOUR_XRI
        /// <summary>
        /// Assumes that there is also a LineRenderer and XRRayInteractor
        /// </summary>
        public static void SetControllerActive(this XRBaseController controller, bool active)
        {
            controller.GetComponent<XRRayInteractor>().enabled = active;
            controller.GetComponent<LineRenderer>().enabled = active;
            
            for (int i = 0; i < controller.transform.childCount; i++)
                controller.transform.GetChild(i).gameObject.SetActive(active);
        }
#endif
        
#if VOUR_WEBXR
        private static WebXRController.ButtonTypes[] GetButtonsFromFlags(WebControllerButtons flags)
        {
            return Enum.GetValues(flags.GetType()).Cast<Enum>().Where(flags.HasFlag).Cast<WebXRController.ButtonTypes>().ToArray();
        }

        /// <summary>
        /// Returns true if any one of the specified buttons are pressed.
        /// </summary>
        public static bool GetAnyButton(this WebXRController controller, WebControllerButtons flags)
        {
            foreach (var value in GetButtonsFromFlags(flags))
                if (controller.GetButton(value))
                    return true;
            return false;
        }

        /// <summary>
        /// Returns true if any one of the specified buttons were pressed down this frame.
        /// </summary>
        public static bool GetAnyButtonDown(this WebXRController controller, WebControllerButtons flags)
        {
            foreach (var value in GetButtonsFromFlags(flags))
                if (controller.GetButtonDown(value))
                    return true;
            return false;
        }

        /// <summary>
        /// Returns true if any one of the specified buttons were pressed up this frame.
        /// </summary>
        public static bool GetAnyButtonUp(this WebXRController controller, WebControllerButtons flags)
        {
            foreach (var value in GetButtonsFromFlags(flags))
                if (controller.GetButtonUp(value))
                    return true;
            return false;
        }
#endif

        /// <summary>
        /// Is it some kind of image location?
        /// </summary>
        public static bool IsImage(this LocationType type) => type == LocationType.Image;

        /// <summary>
        /// Is it some kind of video location?
        /// </summary>
        public static bool IsVideo(this LocationType type) => type == LocationType.Video;

        /// <summary>
        /// Is it some kind of 360 location?
        /// </summary>
        public static bool Is360(this DisplayType type)
        {
            switch (type)
            {
                case DisplayType._360:
                case DisplayType._3603D:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Is it some kind of 180 location?
        /// </summary>
        public static bool Is180(this DisplayType type)
        {
            switch (type)
            {
                case DisplayType._180:
                case DisplayType._180_3D:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Is it some kind of 2D location?
        /// </summary>
        public static bool Is2D(this DisplayType type)
        {
            switch (type)
            {
                case DisplayType._2D:
                case DisplayType._180:
                case DisplayType._360:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Is it some kind of 3D location?
        /// </summary>
        public static bool Is3D(this DisplayType type)
        {
            switch (type)
            {
                case DisplayType._3D:
                case DisplayType._180_3D:
                case DisplayType._3603D:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Is it some kind of VR player?
        /// </summary>
        public static bool IsAnyVR(this Player.PlayerPlatformType platform)
        {
            switch (platform)
            {
                case Player.PlayerPlatformType.VR:
                case Player.PlayerPlatformType.WebVR:
                    return true;
                case Player.PlayerPlatformType.Desktop:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Find child of transform by tag
        /// </summary>
        public static Transform FindChildByTag(this Transform parent, string tag)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                if (child.CompareTag(tag))
                    return child;
            }
            return null;
        }
    }
}
