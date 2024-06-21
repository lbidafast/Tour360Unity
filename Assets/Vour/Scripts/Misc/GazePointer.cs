using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if VOUR_XRI
using UnityEngine.XR.Interaction.Toolkit;
#endif

namespace CrizGames.Vour
{
    
#if VOUR_XRI
    public class GazePointer : XRBaseController
    {
        private XRRayInteractor rayInteractor;
        
        [SerializeField] private SpriteRenderer gazeCursor;
        private Transform gazeCursorT;

        public Color gazeInvalidColor = new Color(0.6f, 0.6f, 0.6f);
        public Color gazeValidColor = Color.white;

        private Transform camT;

        [Header("Gaze Settings")]
        [HideInInspector] public bool startedClick = false;
        private float startTime;
        private float startGazeSize;
        public float clickTime = 2;
        //public float clickAnimDelay = 0.5f;

        private bool click = false;

        private IInteractable currentInteractable;

        private void Start()
        {
            gazeCursorT = gazeCursor.transform;
            camT = GetComponentInParent<PlayerBase>().cam.transform;
            startGazeSize = gazeCursorT.localScale.x;

            rayInteractor = GetComponent<XRRayInteractor>();
        }

        protected override void UpdateController()
        {
            base.UpdateController();
            
            if (!rayInteractor.TryGetHitInfo(out var cursorPos, out _, out _, out _))
            {
                // Set default values if not hit
                cursorPos = camT.forward * rayInteractor.maxRaycastDistance;
            }
            
            IInteractable i = rayInteractor.IsOverUIGameObject() ? null : RaycastInteractable(rayInteractor.transform.position, rayInteractor.transform.forward);

            // If hit interactable
            if (i != null)
            {
                // Check if interactable changed
                if (i != currentInteractable)
                    StartClick(i); // Restart click timer
            }
            else
            {
                CancelClick();
            }
            
            UpdateInteractable(i);

            SetCursor(cursorPos, currentInteractable != null);

            UpdateTimerAndAnimation();
        }

        private void UpdateTimerAndAnimation()
        {
            if(startedClick)
            {
                float timeDiff = Time.time - startTime;
                if(timeDiff > 1f)
                {
                    float size = (clickTime - timeDiff + 1f) / clickTime * startGazeSize; 
                    gazeCursorT.localScale = new Vector3(size, size, 1f);

                    if (timeDiff - 1f > clickTime)
                    {
                        click = true;
                        startedClick = false;
                    }
                }
            }
            else
            {
                gazeCursorT.localScale = new Vector3(startGazeSize, startGazeSize, 1f);
            }
        }

        private void SetCursor(Vector3 cursorPos, bool isValidTarget)
        {
            gazeCursorT.position = cursorPos;
            gazeCursorT.LookAt(camT);
            gazeCursor.color = isValidTarget ? gazeValidColor : gazeInvalidColor;
        }

        private void StartClick(IInteractable interactable)
        {
            startedClick = true;
            startTime = Time.time;
            currentInteractable = interactable;
        }

        private void CancelClick()
        {
            startedClick = false;
            currentInteractable = null;
        }

        protected override void UpdateTrackingInput(XRControllerState controllerState)
        {
            transform.SetPositionAndRotation(camT.position, camT.rotation);
        }

        protected override void UpdateInput(XRControllerState controllerState)
        {
            if (controllerState == null)
                return;

            controllerState.activateInteractionState.SetFrameState(click, click ? 1f : 0f);

            controllerState.uiPressInteractionState.SetFrameState(click, click ? 1f : 0f);
            
            if (click)
                currentInteractable?.Interact();

            if (click)
                click = false;
        }

        public void Enable()
        {
            enabled = true;
            gazeCursor.gameObject.SetActive(true);
        }

        public void Disable()
        {
            CancelClick();
            enabled = false;
            gazeCursor.gameObject.SetActive(false);
        }
        
        protected virtual IInteractable RaycastInteractable(Vector3 pos, Vector3 dir)
        {
            if (rayInteractor.IsOverUIGameObject())
                return null;

            if (Physics.Raycast(pos, dir, out RaycastHit hit))
                return hit.collider.GetComponentInParent<IInteractable>();

            return null;
        }
        
        protected virtual void UpdateInteractable(IInteractable i)
        {
            if (i != null)
            {
                if (i != currentInteractable)
                    i.OnPointerHoverEnter();

                if (currentInteractable != null && i != currentInteractable)
                    currentInteractable?.OnPointerHoverExit();
            }
            else
            {
                currentInteractable?.OnPointerHoverExit();
            }
            currentInteractable = i;
        }
    }
#else
    public class GazePointer : MonoBehaviour {}
#endif
}
