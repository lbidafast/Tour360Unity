using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif
#if VOUR_WEBXR
using WebXR;
#endif

namespace CrizGames.Vour
{
    public class DesktopPlayer : PlayerBase
    {
        public float MouseSensitivity = 50;
        public bool canMoveCam = true;
        public float yOffset = 1.7f;

        private Vector3 pointerPos;
        private Vector2 startPos;
        private Vector3 camRot;

        private IInteractable currentInteractable;

#if ENABLE_INPUT_SYSTEM
        private Mouse mouse => Mouse.current;
        private TouchControl primaryTouch => Touchscreen.current.primaryTouch;
#endif
        
        public override void Init()
        {
            base.Init();
            
            currentInteractable = null;
            
            camRot = cam.transform.eulerAngles;
        }

#if ENABLE_INPUT_SYSTEM
        private void Update()
        {
#if VOUR_WEBXR
            if (WebXRManager.Instance != null && WebXRManager.Instance.subsystem != null && WebXRManager.Instance.XRState != WebXRState.NORMAL)
                return;
#endif
            if (EventSystem.current != null)
                pointerOverUI = EventSystem.current.IsPointerOverGameObject();

            // Get Interactable
            var mousePos = Application.isMobilePlatform switch
            {
                true => primaryTouch.position.ReadValue(),
                false => mouse.position.ReadValue()
            };
            var ray = cam.ScreenPointToRay(mousePos);
            var i = RaycastInteractable(ray.origin, ray.direction);
            UpdateInteractable(ref currentInteractable, i);

            // Click and Interact
            var buttonDown = Application.isMobilePlatform switch
            {
                true => primaryTouch.press.wasPressedThisFrame,
                false => mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame
            };
            if (buttonDown)
                startPos = cam.ScreenToViewportPoint(mousePos);

            if (IsClick())
            {
                var buttonUp = Application.isMobilePlatform switch
                {
                    true => primaryTouch.press.wasReleasedThisFrame,
                    false => mouse.leftButton.wasReleasedThisFrame
                };
                if (buttonUp)
                    i?.Interact();
            }
            // Look
            else
            {
                Look();
            }
        }
        
        protected override IInteractable RaycastInteractable(Vector3 pos, Vector3 dir)
        {
            if (pointerOverUI)
                return null;

            if (Physics.Raycast(pos, dir, out RaycastHit hit))
            {
                pointerPos = hit.point;
                return hit.collider.GetComponentInParent<IInteractable>();
            }

            pointerPos = pos + dir * 100f;
            return null;
        }

        private bool IsClick()
        {
            var pos = Application.isMobilePlatform switch
            {
                true => primaryTouch.position.ReadValue(),
                false => mouse.position.ReadValue()
            };
            return Vector2.Distance(startPos, cam.ScreenToViewportPoint(pos)) < 0.02f;
        }

        private void Look()
        {
            if (!canMoveCam)
                return;

            var press = Application.isMobilePlatform switch
            {
                true => primaryTouch.press.isPressed,
                false => mouse.middleButton.isPressed || mouse.rightButton.isPressed
            };

            if(press)
            {
                var delta = Application.isMobilePlatform switch
                {
                    true => primaryTouch.delta.ReadValue(),
                    false => mouse.delta.ReadValue()
                };
                var lookVec = new Vector3(delta.y, -delta.x) / Screen.height / Screen.dpi * 1000f;
                camRot += lookVec * MouseSensitivity;
                camRot.x = Mathf.Clamp(camRot.x, -90f, 90f);
                cam.transform.eulerAngles = camRot;
            }
        }
#endif

        public override void OnNewLocation(Location l)
        {
            base.OnNewLocation(l);

            if (Application.isPlaying)
                canMoveCam = !l.lockCamera || l.displayType.Is360();
        }

        public override void ResetRotation()
        {
            cam.transform.eulerAngles = camRot = Vector3.zero;
        }

        public override void SetCenterCam(bool center)
        {
            base.SetCenterCam(center);
            if (!center)
                cam.transform.localPosition = new Vector3(0, yOffset, 0);
        }

        public override Vector3 GetPointerPos() => pointerPos;
    }
}