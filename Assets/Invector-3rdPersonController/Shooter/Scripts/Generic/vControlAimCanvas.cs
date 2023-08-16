using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vShooter
{
    using vCharacterController;
    public class vControlAimCanvas : MonoBehaviour
    {
        public static vControlAimCanvas instance;
        public RectTransform canvas;
        public List<vAimCanvas> aimCanvasCollection = new List<vAimCanvas>();
        public bool useAimCorrectionSmooth = true;
        public float aimCorrectionSmooth=20f;
        public Camera scopeBackgroundCamera;
        public bool isScopeCameraActive { get => scopeBackgroundCamera && scopeBackgroundCamera.gameObject.activeInHierarchy; set { if (scopeBackgroundCamera) scopeBackgroundCamera.gameObject.SetActive(value); } }
        public bool isValid { get { if (!currentAimCanvas) return false; return currentAimCanvas.isValid; } set { currentAimCanvas.isValid = value; } }
        public bool isAimActive { get { if (!currentAimCanvas) return false; return currentAimCanvas.isAimActive; } set { currentAimCanvas.isAimActive = value; } }
        public bool isScopeUIActive { get { if (!currentAimCanvas) return false; return currentAimCanvas.isScopeUIActive; } set { currentAimCanvas.isScopeUIActive = value; } }
        public bool useScopeTransition { get { if (!currentAimCanvas) return false; return currentAimCanvas.useScopeTransition; } set { currentAimCanvas.useScopeTransition = value; } }
        protected bool scaleAimWithMovement { get { if (!currentAimCanvas) return false; return currentAimCanvas.scaleAimWithMovement; } }
        protected float movementSensibility { get { return currentAimCanvas.movementSensibility; } }
        protected float scaleWithMovement { get { return currentAimCanvas.scaleWithMovement; } }
        protected float smoothChangeScale { get { return currentAimCanvas.smoothChangeScale; } }
        protected float smoothTransitionIn { get { return currentAimCanvas.smoothTransitionIn; } }
        protected float smoothTransitionOut { get { return currentAimCanvas.smoothTransitionOut; } }
        protected RectTransform aimTarget { get { return currentAimCanvas.aimTarget; } }
        protected RectTransform aimCenter { get { return currentAimCanvas.aimCenter; } }
        protected Vector2 sizeDeltaTarget { get { return currentAimCanvas.sizeDeltaTarget; } }
        protected Vector2 sizeDeltaCenter { get { return currentAimCanvas.sizeDeltaCenter; } }

        protected vThirdPersonController cc;

        protected UnityEvent onEnableAim { get { return currentAimCanvas.onEnableAim; } }
        protected UnityEvent onDisableAim { get { return currentAimCanvas.onDisableAim; } }
        protected UnityEvent onCheckvalidAim { get { return currentAimCanvas.onCheckvalidAim; } }
        protected UnityEvent onCheckInvalidAim { get { return currentAimCanvas.onCheckInvalidAim; } }
        public UnityEvent onEnableScopeCamera { get { return currentAimCanvas.onEnableScopeCamera; } }
        public UnityEvent onDisableScopeCamera { get { return currentAimCanvas.onDisableScopeCamera; } }
        protected UnityEvent onEnableScopeUI { get { return currentAimCanvas.onEnableScopeUI; } }
        protected UnityEvent onDisableScopeUI { get { return currentAimCanvas.onDisableScopeUI; } }

        public vAimCanvas currentAimCanvas;

        protected int currentCanvasID;

        protected virtual float scopeCameraTransformWeight { get; set; }
        protected virtual bool scopeActive { get; set; }
        float scopeCameraTargetZoom;
        float scopeCameraOriginZoom => mainCamera.fieldOfView;
        Vector3 scopeCameraTargetDir;
        Vector3 scopeCameraUpDir;
        Quaternion scopeCameraOriginRot => mainCamera.transform.rotation;
        Vector3 scopeCameraTargetPos;
        Vector3 scopeCameraOriginPos => mainCamera.transform.position;
        public Camera mainCamera;

        public virtual void Init(vThirdPersonController cc)
        {
            if (scopeBackgroundCamera == null)
                scopeBackgroundCamera = GetComponentInChildren<Camera>(true);
            if (scopeBackgroundCamera == null)
            {
                Debug.LogWarning("Could not find Scope Background Camera. Please assign ScopeBackgroundCamera of Control aim canvas", gameObject);
            }
            mainCamera = Camera.main;
            instance = this;
            this.cc = cc;
            currentAimCanvas = aimCanvasCollection[currentCanvasID];
            isValid = true;
        }

        public void UpdateScopeCameraTransition()
        {
            scopeBackgroundCamera.transform.position = Vector3.Lerp(scopeCameraOriginPos, scopeCameraTargetPos, scopeCameraTransformWeight);

            if (scopeCameraTargetDir.magnitude > 0.01f)
                scopeBackgroundCamera.transform.rotation = Quaternion.Lerp(scopeCameraOriginRot, Quaternion.LookRotation(scopeCameraTargetDir,scopeCameraUpDir), scopeCameraTransformWeight);

            scopeBackgroundCamera.fieldOfView = Mathf.Lerp(scopeCameraOriginZoom, scopeCameraTargetZoom, scopeCameraTransformWeight);

            if (useScopeTransition)
            {
                if (scopeActive)
                {
                    scopeCameraTransformWeight = Mathf.Clamp01(scopeCameraTransformWeight += smoothTransitionIn * Time.fixedDeltaTime);
                }
                else
                {
                    scopeCameraTransformWeight = Mathf.Clamp01(scopeCameraTransformWeight -= smoothTransitionOut * Time.fixedDeltaTime);
                }
            }
            else
            {
                scopeCameraTransformWeight = scopeActive ? 1f : 0f;
            }

            if (scopeActive && scopeCameraTransformWeight > 0 && isScopeCameraActive == false)
            {
                isScopeCameraActive = true;
                mainCamera.enabled = false;
                onEnableScopeCamera.Invoke();
            }
            else if (!scopeActive && scopeCameraTransformWeight == 0 && isScopeCameraActive == true)
            {
                mainCamera.enabled = true;
                isScopeCameraActive = false;
                onDisableScopeCamera.Invoke();
            }
        }

        /// <summary>
        /// Set Current Aim to Stay in Center
        /// </summary>
        /// <param name="validPoint">Set if Aim is valid</param>
        public void SetAimToCenter(bool validPoint = true)
        {
            if (currentAimCanvas == null) return;
            if (validPoint != isValid)
            {
                isValid = validPoint;
                if (isValid) onCheckvalidAim.Invoke();
                else onCheckInvalidAim.Invoke();
            }

            if (aimTarget)
            {
                aimTarget.anchoredPosition = Vector2.Lerp(aimTarget.anchoredPosition,Vector2.zero, useAimCorrectionSmooth ? aimCorrectionSmooth * Time.fixedDeltaTime : 1f);
            }
            if (currentAimCanvas.aimCenterToAimTarget && aimCenter)
            {
                aimCenter.anchoredPosition = Vector2.Lerp(aimCenter.anchoredPosition, Vector2.zero, useAimCorrectionSmooth ? aimCorrectionSmooth * Time.fixedDeltaTime : 1f);
            }
            UpdateAimSize();
        }

        /// <summary>
        /// Set WordPosition of TargetAim 
        /// </summary>
        /// <param name="wordPosition">Word Position</param>
        /// <param name="validPoint">Set if Aim is Valid</param>
        public void SetWordPosition(Vector3 wordPosition, bool validPoint = true)
        {
            if (currentAimCanvas == null) return;

            if (validPoint != isValid)
            {
                isValid = validPoint;
                if (isValid) onCheckvalidAim.Invoke();
                else onCheckInvalidAim.Invoke();
            }

        

            Vector2 ViewportPosition = mainCamera.WorldToViewportPoint(wordPosition);
            Vector2 WorldObject_ScreenPosition = new Vector2(
            ((ViewportPosition.x * canvas.sizeDelta.x) - (canvas.sizeDelta.x * 0.5f)),
            ((ViewportPosition.y * canvas.sizeDelta.y) - (canvas.sizeDelta.y * 0.5f)));
         
            if (currentAimCanvas.aimCenterToAimTarget && aimCenter)
            {                
                aimCenter.anchoredPosition = Vector2.Lerp(aimCenter.anchoredPosition, WorldObject_ScreenPosition, useAimCorrectionSmooth?aimCorrectionSmooth * Time.fixedDeltaTime:1f);
            }
            if (aimTarget)
            {
                aimTarget.anchoredPosition = Vector2.Lerp(aimTarget.anchoredPosition, WorldObject_ScreenPosition, useAimCorrectionSmooth ? aimCorrectionSmooth * Time.fixedDeltaTime : 1f);
            }
            UpdateAimSize();
        }


        /// <summary>
        /// Set WordPosition of TargetAim 
        /// </summary>
        /// <param name="wordPosition">Word Position</param>
        /// <param name="validPoint">Set if Aim is Valid</param>
        public void SetWordPosition(Vector3 centerPosition, Vector3 targetPosition, bool validPoint = true)
        {
            if (currentAimCanvas == null) return;

            if (validPoint != isValid)
            {
                isValid = validPoint;
                if (isValid) onCheckvalidAim.Invoke();
                else onCheckInvalidAim.Invoke();
            }

            if (aimCenter)
            {
                Vector2 ViewportPosition = mainCamera.WorldToViewportPoint(centerPosition);
                Vector2 WorldObject_ScreenPosition = new Vector2(
                ((ViewportPosition.x * canvas.sizeDelta.x) - (canvas.sizeDelta.x * 0.5f)),
                ((ViewportPosition.y * canvas.sizeDelta.y) - (canvas.sizeDelta.y * 0.5f)));
                aimCenter.anchoredPosition = Vector2.Lerp(aimCenter.anchoredPosition, WorldObject_ScreenPosition, useAimCorrectionSmooth ? aimCorrectionSmooth * Time.fixedDeltaTime : 1f);
            }
            if (aimTarget)
            {
                Vector2 ViewportPosition = mainCamera.WorldToViewportPoint(targetPosition);
                Vector2 WorldObject_ScreenPosition = new Vector2(
                ((ViewportPosition.x * canvas.sizeDelta.x) - (canvas.sizeDelta.x * 0.5f)),
                ((ViewportPosition.y * canvas.sizeDelta.y) - (canvas.sizeDelta.y * 0.5f)));
                aimTarget.anchoredPosition = Vector2.Lerp(aimTarget.anchoredPosition, WorldObject_ScreenPosition, useAimCorrectionSmooth ? aimCorrectionSmooth * Time.fixedDeltaTime : 1f);
            }
            UpdateAimSize();
        }

        private void UpdateAimSize()
        {
            float scale = 1f;
            if (scaleAimWithMovement && (cc.input.magnitude > movementSensibility || Mathf.Abs(Input.GetAxis("Mouse X")) > movementSensibility || Mathf.Abs(Input.GetAxis("Mouse Y")) > movementSensibility))
            {
                scale = scaleWithMovement;               
            }
           if(aimCenter) aimCenter.sizeDelta = Vector2.Lerp(aimCenter.sizeDelta, sizeDeltaCenter * Mathf.Abs(scale), smoothChangeScale * Time.fixedDeltaTime);
           if(aimTarget) aimTarget.sizeDelta = Vector2.Lerp(aimTarget.sizeDelta, sizeDeltaTarget * Mathf.Abs(scale), smoothChangeScale * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Enable or Disable the current Aim
        /// </summary>
        /// <param name="value"> active value</param>
        public void SetActiveAim(bool value)
        {
            if (currentAimCanvas == null) return;
            if (value != isAimActive)
            {

                isAimActive = value;
                if (value)
                {                  
                    isValid = true;                   
                    onEnableAim.Invoke();
                }
                else
                {                  
                    onDisableAim.Invoke();                   
                }
            }
        
        }

        public void DisableScopeCamera()
        {
            scopeActive = false;
            scopeCameraTransformWeight = 0;
            isScopeCameraActive = false;
            mainCamera.enabled = true;
        }

        public void DisableAim()
        {
            SetActiveAim(false);
            DisableScopeCamera();
        }

        /// <summary>
        /// Enable or Disable the current Scope
        /// </summary>
        /// <param name="value">active value</param>
        /// <param name="useUI">set if scope camera use the Scope UI </param>
        public void SetActiveScopeCamera(bool value, bool useUI = false)
        {
            if (currentAimCanvas == null || !scopeBackgroundCamera) return;

         
            if (scopeActive != value || isScopeUIActive != useUI)
            {
                isScopeUIActive = useUI;
                if (value)
                {
                    scopeActive = true;

                    if (value && useUI)
                    {
                        onEnableScopeUI.Invoke();
                        isScopeUIActive = true;
                    }
                    else
                    {                       
                        onDisableScopeUI.Invoke();                     
                        isScopeUIActive = false;
                    }
                }
                else
                {
                    scopeActive = false;
                    isScopeUIActive = false;
                    onDisableScopeUI.Invoke();  
                }
            }
        }

        /// <summary>
        /// Update Word properties and zoom ("FieldOfView") of the Scope Camera
        /// </summary>
        /// <param name="position">word position</param>
        /// <param name="lookDirection">Word target LookAt</param>
        /// <param name="zoom">FieldOfView</param>
        public void UpdateScopeCamera(Vector3 position, Vector3 lookDirection, Vector3 upDirection, float zoom = 60, bool isAiming = false)
        {
            if (currentAimCanvas == null || !scopeBackgroundCamera) return;

            var _zoom = Mathf.Clamp(60 - zoom, 1, 179);

            if (isAiming)
            {
                scopeCameraTargetPos = position;
                scopeCameraTargetDir = lookDirection;
                scopeCameraUpDir = upDirection;
                scopeCameraTargetZoom = _zoom;
            }

            UpdateScopeCameraTransition();
        }

        /// <summary>
        /// Set AimCanvas ID
        /// if id do not exist,this change to defaultAimCanvas id 0
        /// </summary>
        /// <param name="id">index of AimCanvasCollection</param>
        public void SetAimCanvasID(int id)
        {
            if (aimCanvasCollection.Count > 0 && currentCanvasID != id)
            {
                if (currentAimCanvas != null) currentAimCanvas.DisableAll();
                if (id < aimCanvasCollection.Count)
                {
                    currentAimCanvas = aimCanvasCollection[id];
                    currentCanvasID = id;
                }
                else
                {
                    currentAimCanvas = aimCanvasCollection[0];
                    currentCanvasID = 0;
                }
            }
        }
    }
}