using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrizGames.Vour
{
    public abstract class PlayerBase : MonoBehaviour
    {
        public static PlayerBase Instance;
        
        protected static readonly int CustomEyeIndexProperty = Shader.PropertyToID("_CustomEyeIndex");
        
        public GazePointer gazePointer;
        public bool forceEnableGazePointer = false;

        [HideInInspector] public Camera cam;

        [Header("Player Settings")]
        [SerializeField] private protected bool centerCam = true;
        public bool CenterCamera { get { return centerCam; } }

        private protected float startYPos;

        protected bool initialized = false;

        protected bool pointerOverUI = false;

        private void Awake()
        {
            Init();
        }

        public virtual void Init()
        {
            startYPos = transform.position.y;
            
            if (initialized)
                return;
            
            Shader.SetGlobalInt(CustomEyeIndexProperty, -1);

            cam = GetComponentInChildren<Camera>();
            
            SetCenterCam(CenterCamera);

            Instance = this;
            initialized = true;
        }
        
        protected virtual IInteractable RaycastInteractable(Vector3 pos, Vector3 dir)
        {
            if (pointerOverUI)
                return null;

            if (Physics.Raycast(pos, dir, out RaycastHit hit))
                return hit.collider.GetComponentInParent<IInteractable>();

            return null;
        }
        
        protected virtual void UpdateInteractable(ref IInteractable currentInteractable, IInteractable i)
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

        public abstract void ResetRotation();

        public virtual void SetCenterCam(bool center)
        {
            centerCam = center;

            if (center)
                cam.transform.position = Vector3.zero;
            else
                transform.position = new Vector3(transform.position.x, startYPos, transform.position.z);
        }

        public virtual void OnNewLocation(Location l)
        {
            // Reset player rotation if this is a non-360 location and not empty
            if (Application.isPlaying && !l.displayType.Is360() && l.locationType != Location.LocationType.Empty)
                ResetRotation();
        }

        protected virtual void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public abstract Vector3 GetPointerPos();
    }
}