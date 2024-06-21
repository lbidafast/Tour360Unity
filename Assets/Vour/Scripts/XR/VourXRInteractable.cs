using UnityEngine;
using UnityEngine.Events;
#if VOUR_XRI
using UnityEngine.XR.Interaction.Toolkit;
#endif

namespace CrizGames.Vour
{
#if false // VOUR_XRI
    public class VourXRInteractable : XRBaseInteractable
    {
        protected override void Awake()
        {
            base.Awake();

            var i = GetComponent<IInteractable>() ?? GetComponentInParent<IInteractable>();
            SetEvent(hoverEntered, i.OnPointerHoverEnter);
            SetEvent(hoverExited, i.OnPointerHoverExit);
            SetEvent(activated, i.Interact);
        }

        private void SetEvent<T>(UnityEvent<T> unityEvent, UnityAction action)
        {
            unityEvent.AddListener(_ => action());
        }
    }
#else
    public class VourXRInteractable : MonoBehaviour {}
#endif
}