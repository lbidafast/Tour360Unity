using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if VOUR_WEBXR
using UnityEngine.XR.Interaction.Toolkit;
using WebXR;
#endif

namespace CrizGames.Vour
{
    public class WebXRPlayer : PlayerBase
    {
        [Header("Hands & Controllers")]
        public Transform leftControllerAnchor;
        public Transform rightControllerAnchor;
        public GameObject controllersParent;

        protected bool rotated = false;
        
        [Header("Cameras")]
        [SerializeField] private Camera pcCamera;
        [SerializeField] private Camera vrCameraL;
        [SerializeField] private Camera vrCameraR;

        private Rect leftRect, rightRect;
        private int viewsCount = 1;
        
        private DesktopPlayer pcPlayer;

#if VOUR_WEBXR
        private WebXRController leftController;
        private WebXRController rightController;
        private WebXRIController leftXRIController, rightXRIController;
        private XRRayInteractor leftRay, rightRay;
        private XRRayInteractor currentRayInteractor;
        
        private bool _lastLeftActiveState = true;
        private bool _lastRightActiveState = true;
        
        private IInteractable currentInteractableLeft;
        private IInteractable currentInteractableRight;
        
        private WebXRState xrState = WebXRState.NORMAL;
        private WebXRState prevXRState = WebXRState.NORMAL;

        public override void Init()
        {
            pcPlayer = GetComponent<DesktopPlayer>();
            pcPlayer.Init(); // Init if it wasn't yet
            
            base.Init();
            
            cam = vrCameraL;

            InitCurrentController();

            // Disable both controller permanently for gaze pointer
            controllersParent.SetActive(!forceEnableGazePointer);
            if (forceEnableGazePointer)
                gazePointer.Enable();
            else
                gazePointer.Disable();

            if (WebXRManager.Instance.subsystem != null)
                xrState = WebXRManager.Instance.XRState;
            
            if (Utils.IsUsingBuiltinRenderPipeline())
                Camera.onPreRender += OnCameraPreRender;
            else
                RenderPipelineManager.beginCameraRendering += (context, camera) => OnCameraPreRender(camera);

#if UNITY_EDITOR
            if (Utils.InVR())
                xrState = WebXRState.VR;
#endif

            SwitchXRState();
        }

        private void OnEnable()
        {
            WebXRManager.OnXRChange += OnXRChange;
            WebXRManager.OnHeadsetUpdate += OnHeadsetUpdate;
        }

        private void OnDisable()
        {
            WebXRManager.OnXRChange -= OnXRChange;
            WebXRManager.OnHeadsetUpdate -= OnHeadsetUpdate;
        }

        private void OnCameraPreRender(Camera camera)
        {
            // Set custom stereo eye index for 3D shaders
            var value = xrState == WebXRState.VR && camera == vrCameraR ? 1 : 0;
            Shader.SetGlobalInt(CustomEyeIndexProperty, value);
        }

        private void SwitchXRState()
        {
            switch (xrState)
            {
                case WebXRState.AR:
                    Debug.LogError("AR is not supported by Vour.");
                    break;

                case WebXRState.VR:
                    pcCamera.enabled = false;
#if UNITY_EDITOR
                    vrCameraL.enabled = true;
                    vrCameraR.enabled = true;
#else
                    // In WebGL, the viewports have to be adjusted manually
                    vrCameraL.enabled = viewsCount > 0;
                    vrCameraL.rect = leftRect;
                    vrCameraR.enabled = viewsCount > 1;
                    vrCameraR.rect = rightRect;
#endif
                    leftControllerAnchor.gameObject.SetActive(true);
                    rightControllerAnchor.gameObject.SetActive(true);
                    
                    if (forceEnableGazePointer)
                        gazePointer.Enable();
                    break;

                case WebXRState.NORMAL:
                    pcCamera.enabled = true;
                    vrCameraL.enabled = false;
                    vrCameraR.enabled = false;

                    leftControllerAnchor.gameObject.SetActive(false);
                    rightControllerAnchor.gameObject.SetActive(false);
                    
                    gazePointer.Disable();

                    pcPlayer.SetCenterCam(pcPlayer.CenterCamera);
                    break;
            }
        }

        private void OnXRChange(WebXRState state, int viewsCount, Rect leftRect, Rect rightRect)
        {
            Debug.Log($"XR state changed from {xrState} to {state}");
            //Debug.Log("Views Count: " + viewsCount);   // VR = 2; NORMAL = 1
            //Debug.Log("Left Eye Rect: " + leftRect);   // VR = (  0, 0, 0.5, 1); NORMAL = (0, 0, 0, 0)
            //Debug.Log("Right Eye Rect: " + rightRect); // VR = (0.5, 0, 0.5, 1); NORMAL = (0, 0, 0, 0)

            this.viewsCount = viewsCount;
            this.leftRect = leftRect;
            this.rightRect = rightRect;

            prevXRState = xrState;
            xrState = state;
            SwitchXRState();
        }

        private void OnHeadsetUpdate(
            Matrix4x4 leftProjectionMatrix, Matrix4x4 rightProjectionMatrix,
            Quaternion leftRotation, Quaternion rightRotation,
            Vector3 leftPosition, Vector3 rightPosition)
        {
            if (xrState == WebXRState.VR)
            {
#if UNITY_EDITOR
                // Eye positions have a difference of ~0.04m
                Vector3 pos = (leftPosition + rightPosition) / 2f;

                vrCameraL.transform.localPosition = pos;
                vrCameraR.transform.localPosition = pos;
#else
                // In WebGL, the eye distance has to be set manually
                vrCameraL.transform.localPosition = leftPosition;
                vrCameraR.transform.localPosition = rightPosition;
#endif
                vrCameraL.transform.localRotation = leftRotation;
                vrCameraR.transform.localRotation = rightRotation;
                vrCameraL.projectionMatrix = leftProjectionMatrix;
                vrCameraR.projectionMatrix = rightProjectionMatrix;
            }
        }

        private void InitCurrentController()
        {
            leftController = leftControllerAnchor.GetComponent<WebXRController>();
            rightController = rightControllerAnchor.GetComponent<WebXRController>();
            leftXRIController = leftControllerAnchor.GetComponent<WebXRIController>();
            rightXRIController = rightControllerAnchor.GetComponent<WebXRIController>();
            leftRay = leftController.GetComponent<XRRayInteractor>();
            rightRay = rightController.GetComponent<XRRayInteractor>();
            
            leftController.OnControllerActive += active => UpdateControllerState(leftXRIController, true, active);
            rightController.OnControllerActive += active => UpdateControllerState(rightXRIController, false, active);
            
            currentRayInteractor = rightRay;
        }

        private void UpdateControllerState(WebXRIController controller, bool isLeftController, bool active)
        {
            controller.SetControllerActive(active);

            var lastControllerState = isLeftController ? _lastLeftActiveState : _lastRightActiveState;
            
            var controllerState = active;
            var otherControllerState = !isLeftController ? _lastLeftActiveState : _lastRightActiveState;

            // Enable gaze pointer if both controller are not available
            if (!controllerState && !otherControllerState && lastControllerState)
            {
                gazePointer.Enable();
            }
            // Disable gaze pointer if at least one controller available
            else if (!forceEnableGazePointer && !lastControllerState && controllerState)
            {
                gazePointer.Disable();
            }
            
            if (isLeftController) _lastLeftActiveState = active;
            else _lastRightActiveState = active;
        }

        private void Update()
        {
            if (xrState == WebXRState.NORMAL)
                return;

            if (leftXRIController.activateInteractionState.activatedThisFrame)
                currentRayInteractor = leftRay;
            else if (rightXRIController.activateInteractionState.activatedThisFrame)
                currentRayInteractor = rightRay;
            
            pointerOverUI = currentRayInteractor.IsOverUIGameObject();

            GetInteractable(ref currentInteractableLeft, leftRay);
            GetInteractable(ref currentInteractableRight, rightRay);
            
            if (leftXRIController.activateInteractionState.activatedThisFrame)
                currentInteractableLeft?.Interact();
            if (rightXRIController.activateInteractionState.activatedThisFrame)
                currentInteractableRight?.Interact();

            if (centerCam)
                transform.position -= cam.transform.position;
        }

        public override void SetCenterCam(bool center)
        {
            base.SetCenterCam(center);
            pcPlayer.SetCenterCam(center);
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
#if VOUR_WEBXR
            if (xrState == WebXRState.NORMAL)
                return pcPlayer.GetPointerPos();
            
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
            pcPlayer.ResetRotation();
        }
    }
}
