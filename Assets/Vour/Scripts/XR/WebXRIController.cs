using UnityEngine;
#if VOUR_XRI && VOUR_WEBXR
using UnityEngine.XR.Interaction.Toolkit;
using WebXR;
#endif

namespace CrizGames.Vour
{
#if VOUR_XR && VOUR_WEBXR
    public class WebXRIController : XRBaseController
    {
        private WebXRController webXRController;

        protected override void Awake()
        {
            webXRController = GetComponent<WebXRController>();
            base.Awake();
        }

        protected override void UpdateInput(XRControllerState controllerState)
        {
            base.UpdateInput(controllerState);
            if (controllerState == null)
                return;

            bool grip = webXRController.GetButtonDown(WebXRController.ButtonTypes.Grip);
            controllerState.selectInteractionState.SetFrameState(grip, grip ? 1f : 0f);

            bool trigger = webXRController.GetButtonDown(WebXRController.ButtonTypes.Trigger);
            controllerState.activateInteractionState.SetFrameState(trigger, trigger ? 1f : 0f);

            controllerState.uiPressInteractionState.SetFrameState(trigger, trigger ? 1f : 0f);
        }
    }
#else
    public class WebXRIController : MonoBehaviour {}
#endif
}