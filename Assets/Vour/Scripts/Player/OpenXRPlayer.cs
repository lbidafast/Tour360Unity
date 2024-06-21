using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if VOUR_XRI
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
#endif

namespace CrizGames.Vour
{
    public class OpenXRPlayer : PlayerBase
    {
#if VOUR_XRI
        public ActionBasedController leftController;
        public ActionBasedController rightController;
        private XRRayInteractor leftRay, rightRay;
        private XRRayInteractor currentRayInteractor;

        private bool _lastLeftActiveState = true;
        private bool _lastRightActiveState = true;
        
        private IInteractable currentInteractableLeft;
        private IInteractable currentInteractableRight;

        public override void Init()
        {
            base.Init();

            leftRay = leftController.GetComponent<XRRayInteractor>();
            rightRay = rightController.GetComponent<XRRayInteractor>();
            currentRayInteractor = rightRay;

            // Disable both controller permanently for gaze pointer
            leftController.gameObject.SetActive(!forceEnableGazePointer);
            rightController.gameObject.SetActive(!forceEnableGazePointer);
            if (forceEnableGazePointer)
                gazePointer.Enable();
            else
                gazePointer.Disable();
        }

        private void Update()
        {
            MonitorControllerStates();

            if (leftController.activateInteractionState.activatedThisFrame)
                currentRayInteractor = leftRay;
            else if (rightController.activateInteractionState.activatedThisFrame)
                currentRayInteractor = rightRay;

            GetInteractable(ref currentInteractableLeft, leftRay);
            GetInteractable(ref currentInteractableRight, rightRay);
            
            if (leftController.activateInteractionState.activatedThisFrame)
                currentInteractableLeft?.Interact();
            if (rightController.activateInteractionState.activatedThisFrame)
                currentInteractableRight?.Interact();
            
            if (centerCam)
                transform.position -= cam.transform.position;
        }

        private void MonitorControllerStates()
        {
            var leftState = (leftController.currentControllerState.inputTrackingState & InputTrackingState.Position) != 0;
            var rightState = (rightController.currentControllerState.inputTrackingState & InputTrackingState.Position) != 0;
            
            if (leftState != _lastLeftActiveState)
                leftController.SetControllerActive(leftState);
            if (rightState != _lastRightActiveState)
                rightController.SetControllerActive(rightState);

            // Enable gaze pointer if both controller are not available
            if (!leftState && !rightState && (_lastLeftActiveState || _lastRightActiveState))
            {
                gazePointer.Enable();
            }
            // Disable gaze pointer if at least one controller available
            else if (!forceEnableGazePointer && !_lastLeftActiveState && !_lastRightActiveState && (leftState || rightState))
            {
                gazePointer.Disable();
            }
            
            _lastLeftActiveState = leftState;
            _lastRightActiveState = rightState;
        }
        
        private void GetInteractable(ref IInteractable currentInteractable, XRRayInteractor interactor)
        {
            pointerOverUI = interactor.IsOverUIGameObject();
            IInteractable i = RaycastInteractable(interactor.transform.position, interactor.transform.forward);
            UpdateInteractable(ref currentInteractable, pointerOverUI ? null : i);
        }
#endif

        public override Vector3 GetPointerPos()
        {
#if VOUR_XRI
            var points = new Vector3[2];
            if (currentRayInteractor.GetLinePoints(ref points, out int numPoints))
                return points[numPoints - 1];
#endif
            // Return player's position if GetLinePoints() failed
            return cam.transform.position;
        }

        public override void ResetRotation()
        {
            transform.eulerAngles = Vector3.up * -cam.transform.localEulerAngles.y;
        }
    }
}