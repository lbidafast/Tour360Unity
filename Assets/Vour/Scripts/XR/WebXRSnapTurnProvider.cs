using UnityEngine;
#if VOUR_WEBXR
using UnityEngine.XR.Interaction.Toolkit;
using WebXR;
#endif

namespace CrizGames.Vour
{
#if VOUR_WEBXR
    public class WebXRSnapTurnProvider : SnapTurnProviderBase
    {
        [SerializeField] private WebXRController leftController;
        [SerializeField] private WebXRController rightController;


        protected override Vector2 ReadInput()
        {
            var leftHandValue = leftController.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);
            var rightHandValue = rightController.GetAxis2D(WebXRController.Axis2DTypes.Thumbstick);

            return leftHandValue + rightHandValue;
        }
    }
#else
    public class WebXRSnapTurnProvider : MonoBehaviour {}
#endif
}