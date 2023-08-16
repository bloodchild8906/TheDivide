using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.Throw
{
    using Invector.vCharacterController;
    public abstract class vThrowManagerBase : vMonoBehaviour
    {
        [System.Serializable]
        public class VisualEvent : UnityEngine.Events.UnityEvent<vThrowVisualSettings> { }
        [System.Serializable]
        public class SettingsEvent : UnityEngine.Events.UnityEvent<vThrowSettings> { }
        [System.Serializable]
        public class ActiveEvent : UnityEngine.Events.UnityEvent<bool> { }

        #region Variables

        public enum CameraStyle
        {
            ThirdPerson, TopDown, SideScroll
        }

        [vEditorToolbar("Settings")]
        public CameraStyle cameraStyle;

        [Tooltip("Use this to enable/disable if you can or not enter ThrowMode")]
        public bool canUseThrow;

        [Tooltip("Check this to use the Line that draws the Throw Trajectory")]
        public bool drawTrajectory = true;
        public LineRenderer lineRenderer;

        [Tooltip("This is the Starting point of the Throw")]
        public Transform throwStartPoint;

        [Tooltip("This is the Ending point (landing) of the Throw")]
        public GameObject throwEnd;
        public LayerMask obstacles = 1 << 0;
        public vThrowSettings defaultSettings;
        public vThrowVisualSettings defaultVisualSettings;

        [Tooltip("Set ignore collision to the grenade to not collide with the Player")]
        public bool setIgnoreCollision;
        public bool debugMode;

        [vSeparator("Only for ThirdPerson Camera Style")]
        [Tooltip("The Third person camera right will be applied as offset to throw start point")]
        public bool useThrowStartRightOffset;

        [Tooltip("Sets a Offset right for the Throw Start Point"), vHideInInspector("useThrowStartRightOffset")]
        public float throwStartRightOffsetMultiplier = 1f;

        [vSeparator("Controller Settings")]

        [Tooltip("Force the controller to walk while aiming")]
        public bool walkWhileAiming;

        [Tooltip("Rotate to aim point while aiming")]
        public bool rotateWhileAiming = true;
        public bool rotateWhileThrowing = true;
        public bool strafeWhileAiming = true;

        [vEditorToolbar("Inputs")]
        public GenericInput throwInput = new GenericInput("Mouse0", "RB", "RB");
        public GenericInput aimThrowInput = new GenericInput("G", "LB", "LB");
        public bool aimHoldingButton = true;

        [vEditorToolbar("Events")]
        public UnityEngine.Events.UnityEvent onEquipThrowable;
        public UnityEngine.Events.UnityEvent onEnableAim;
        public UnityEngine.Events.UnityEvent onCancelAim;
        public UnityEngine.Events.UnityEvent onStartThrowObject;
        public UnityEngine.Events.UnityEvent onThrowObject;
        public UnityEngine.Events.UnityEvent onCollectObject;
        public UnityEngine.Events.UnityEvent onFinishThrow;

        [vEditorToolbar("Visual Effect Events")]

        [vSeparator("Line")]
        public ActiveEvent onSetActiveLine;

        [vSeparator("Indicator")]
        public ActiveEvent onSetActiveIndicator;
        public SettingsEvent onChangeSettings;
        public VisualEvent onChangeVisualSettings;

        public virtual int MaxThrowObjects { get; }
        public abstract int CurrentThrowAmount { get; }
        public virtual Sprite CurrentThrowableSprite { get; }

        protected virtual Collider[] selfColliders { get; set; }
        /// <summary>
        /// Enter Aiming ThrowMode 
        /// </summary>
        protected virtual bool isAiming { get; set; }
        /// <summary>
        /// "ThrowObject" animation is playing
        /// </summary>
        protected virtual bool isThrowing { get; set; }
        /// <summary>
        /// The moment you press the throwInput
        /// </summary>
        protected virtual bool pressThrowInput { get; set; }
        protected virtual bool wasAiming { get; set; }
        protected virtual bool inEnterThrowMode { get; set; }

        protected virtual Vector3 surfaceNormal { get; set; }

        protected virtual Transform rightUpperArm { get; set; }
        protected virtual vThirdPersonInput tpInput { get; set; }
        public abstract vThrowableObject ObjectToThrow { get; }
        protected virtual vThrowableObject lastThrowObject { get; set; }

        #endregion

        #region Settings     
        public virtual vThrowSettings CurrentSettings
        {
            get
            {
                var settings = ObjectToThrow && ObjectToThrow.throwSettings ? ObjectToThrow.throwSettings : defaultSettings;
                if (settings != lastSettings)
                {
                    lastSettings = settings;
                    onChangeSettings.Invoke(lastSettings);
                }
                return settings;
            }
        }

        protected virtual vThrowSettings lastSettings { get; set; }

        public virtual string cameraState => tpInput.cc.isCrouching ? CurrentSettings.cameraStateCrouching : CurrentSettings.cameraStateStanding;
        public virtual bool alignToSurfaceNormal => CurrentSettings.alignToSurfaceNormal;
        public virtual bool alignToViewWhenNotHit => CurrentSettings.alignToViewWhenNotHit;
        public virtual bool disableEndPointWhenNotHit => CurrentSettings.disableEndPointWhenNotHit;
        public virtual float alignmentSmooth => CurrentSettings.alignmentSmooth;
        public virtual float metersPerSeconds => CurrentSettings.metersPerSeconds;
        public virtual Vector2 minMaxTime => CurrentSettings.minMaxTime;
        public virtual float maxDistance => CurrentSettings.maxDistance;
        public virtual float maxVelocity => CurrentSettings.maxVelocity;
        public virtual float throwMaxForce => CurrentSettings.throwMaxForce;
        public virtual float throwDelayTime => CurrentSettings.throwDelayTime;
        public virtual float lineStepPerTime => CurrentSettings.lineStepPerTime;
        public virtual float lineMaxLength => CurrentSettings.maxLineLength;
        public virtual float exitThrowModeDelay => CurrentSettings.exitThrowModeDelay;
        public virtual string throwAnimation => CurrentSettings.throwAnimation;
        public virtual string holdingAnimation => CurrentSettings.holdingAnimation;
        public virtual string cancelAnimation => CurrentSettings.cancelAnimation;
        #endregion

        #region Visual        
        public virtual vThrowVisualSettings CurrentVisualSettings
        {
            get
            {
                var settings = ObjectToThrow && ObjectToThrow.throwVisualSettings ? ObjectToThrow.throwVisualSettings : defaultVisualSettings;
                if (settings != lastSettings)
                {
                    lastVisualSettings = settings;
                    onChangeVisualSettings.Invoke(lastVisualSettings);
                }
                return settings;
            }
        }
        protected virtual vThrowVisualSettings lastVisualSettings { get; set; }

        #endregion

        protected virtual IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            if (lineRenderer == null) lineRenderer = GetComponentInChildren<LineRenderer>(true);

            if (lineRenderer)
            {
                lineRenderer.useWorldSpace = false;
            }
            if (lineRenderer && lineRenderer.gameObject.activeInHierarchy)
            {
                lineRenderer.gameObject.SetActive(false);
            }


            if (throwEnd && throwEnd.activeSelf)
            {
                throwEnd.SetActive(false);
            }

            if (transform.root.TryGetComponent(out vDrawHideMeleeWeapons drawHide))
            {
                onEquipThrowable.AddListener(() => drawHide.HideWeapons(true));
            }

            canUseThrow = true;

            tpInput = GetComponentInParent<vThirdPersonInput>();

            if (tpInput)
            {
                selfColliders = tpInput.GetComponentsInChildren<Collider>(true);
                tpInput.onUpdate -= UpdateThrowInput;
                tpInput.onUpdate += UpdateThrowInput;
                tpInput.onFixedUpdate -= UpdateThrowBehavior;
                tpInput.onFixedUpdate += UpdateThrowBehavior;
                rightUpperArm = tpInput.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

                if (cameraStyle == CameraStyle.SideScroll)
                {
                    rotateWhileAiming = true;
                }
               
            }
            if (cameraStyle != CameraStyle.ThirdPerson)
            {
                useThrowStartRightOffset = false;
                rotateWhileAiming = true;
                strafeWhileAiming = true;
            }
        }

        public virtual void CanUseThrow(bool value)
        {
            canUseThrow = value;
        }

        protected virtual bool ThrowConditions => !(ObjectToThrow == null || CurrentThrowAmount <= 0 || !tpInput.enabled || tpInput.cc.customAction || canUseThrow == false);

        protected virtual void UpdateThrowBehavior()
        {
            CheckThrowObjectChanges();
            UpdateThrow();
            MoveAndRotate();
        }

        protected virtual void CheckThrowObjectChanges()
        {
            if (ObjectToThrow != lastThrowObject)
            {
                lastThrowObject = ObjectToThrow;
                onSetActiveLine.Invoke(CurrentVisualSettings.useLine);
                onSetActiveIndicator.Invoke(CurrentVisualSettings.useIndicator);
            }
        }

        protected virtual void UpdateThrow()
        {

            if (isAiming)
            {
                tpInput.CrouchInput();
                wasAiming = true;
                tpInput.SetWalkByDefault(walkWhileAiming);
                if (string.IsNullOrEmpty(cameraState) == false && tpInput.customCameraState != cameraState) tpInput.ChangeCameraStateWithLerp(cameraState);
                CalculateAimPoint();
                if (drawTrajectory) DrawTrajectory();
                if (debugMode) Debug.DrawLine(startPoint, aimPoint, Color.cyan);
            }
            else if (!inEnterThrowMode)
            {
                if (wasAiming)
                {
                    tpInput.ResetWalkByDefault();
                    if (string.IsNullOrEmpty(cameraState) == false && tpInput.customCameraState == cameraState) tpInput.ResetCameraState();
                    if (lineRenderer && lineRenderer.gameObject.activeInHierarchy)
                    {
                        lineRenderer.gameObject.SetActive(false);
                    }

                    if (throwEnd && throwEnd.activeSelf)
                    {
                        throwEnd.SetActive(false);
                    }
                }
                wasAiming = false;
            }

            if (pressThrowInput)
            {
                isThrowing = true;
                pressThrowInput = false;
                tpInput.animator.SetBool("IsAiming", false);
                tpInput.animator.CrossFadeInFixedTime(throwAnimation, 0.2f);
            }
        }

        protected virtual void MoveAndRotate()
        {
            if (isAiming || isThrowing || inEnterThrowMode)
            {
                tpInput.MoveInput();

                switch (cameraStyle)
                {
                    case CameraStyle.ThirdPerson:
                        if (rotateWhileAiming && !isThrowing || (isThrowing && rotateWhileThrowing))
                        {

                            tpInput.cc.RotateToDirection(tpInput.cameraMain.transform.forward);
                        }
                        break;
                    case CameraStyle.TopDown:
                        var dir = aimDirection;
                        dir.y = 0;
                        if (isThrowing || rotateWhileAiming) tpInput.cc.RotateToDirection(dir);
                        break;
                    case CameraStyle.SideScroll:
                        ///
                        break;
                }
            }
        }

        protected virtual void UpdateThrowInput()
        {
            if (!ThrowConditions)
            {
                return;
            }

            if (aimThrowInput.GetButtonDown() && !inEnterThrowMode && !isThrowing && !isAiming)
            {
                EnterThrowMode();
                return;
            }

            if (aimThrowInput.GetButtonUp() && aimHoldingButton && (isAiming || inEnterThrowMode) && !isThrowing)
            {
                ExitThrowMode();
            }

            if (isAiming && !isThrowing && !pressThrowInput)
            {
                if (throwInput.GetButtonDown())
                {
                    pressThrowInput = true;
                }
            }

            if (!aimHoldingButton && aimThrowInput.GetButtonDown() && !pressThrowInput && (isAiming || inEnterThrowMode) && !isThrowing)
            {
                ExitThrowMode();
            }
        }

        protected virtual vThrowableObject GetInstanceOfThrowable()
        {
            return Instantiate(ObjectToThrow, startPoint, Quaternion.identity);
        }

        protected virtual void Throw()
        {
            LaunchObject(GetInstanceOfThrowable());
            ExitThrowMode();
        }

        protected virtual void LaunchObject(vThrowableObject throwable)
        {

            if (setIgnoreCollision)
            {
                var _colliders = throwable.GetComponentsInChildren<Collider>();
                if (_colliders.Length > 0)
                {
                    foreach (var _collider in _colliders)
                        for (int i = 0; i < selfColliders.Length; i++)
                        {
                            Physics.IgnoreCollision(_collider, selfColliders[i], true);
                        }
                }
            }
            throwable.transform.position = startPoint;
            throwable.isKinematic = false;
            throwable.transform.parent = null;
            throwable.onThrow.Invoke(tpInput.transform);
            throwable.selfRigidbody.velocity = StartVelocity;
            onThrowObject.Invoke();
        }

        protected virtual void DrawTrajectory()
        {
            float time = Vector3.Distance(startPoint, aimPoint) * metersPerSeconds;
            time = Mathf.Clamp(time, minMaxTime.x, minMaxTime.y);
            Vector3? hitPoint = null;

            var points = GetTrajectoryPoints(startPoint, StartVelocity, lineStepPerTime, lineMaxLength, ref hitPoint);
            if (lineRenderer)
            {
                lineRenderer.transform.rotation = Quaternion.LookRotation(Quaternion.AngleAxis(90, transform.right) * (points[points.Count - 1] - startPoint).normalized);
                if (!lineRenderer.gameObject.activeInHierarchy)
                {
                    lineRenderer.gameObject.SetActive(true);
                }

                lineRenderer.positionCount = points.Count;
                for (int i = 0; i < lineRenderer.positionCount; i++)
                {
                    lineRenderer.SetPosition(i, lineRenderer.transform.InverseTransformPoint(points[i]));
                }

            }
            if (throwEnd)
            {
                if (!throwEnd.activeSelf)
                {
                    if ((!disableEndPointWhenNotHit || hitPoint != null))
                        throwEnd.SetActive(true);
                }
                else if (!(!disableEndPointWhenNotHit || hitPoint != null))
                {
                    throwEnd.SetActive(false);
                }
                if (points.Count > 1)
                {
                    throwEnd.transform.position = points[points.Count - 1];
                    throwEnd.transform.rotation = Quaternion.Lerp(throwEnd.transform.rotation, Quaternion.LookRotation(surfaceNormal, transform.up), alignmentSmooth * Time.deltaTime);

                }
            }
        }

        internal virtual void TriggerAnimatorEvent(vThrowAnimatorEvent.ThrowEventType eventType)
        {
            switch (eventType)
            {
                case vThrowAnimatorEvent.ThrowEventType.EquipThrowable:
                    onEquipThrowable.Invoke();
                    EquipThrowObject();
                    break;
                case vThrowAnimatorEvent.ThrowEventType.EnableAiming:
                    EnableAimMode();
                    break;
                case vThrowAnimatorEvent.ThrowEventType.CancelAiming:
                    DisableAimMode();
                    ExitThrowMode();
                    break;
                case vThrowAnimatorEvent.ThrowEventType.StartLaunch:
                    DisableAimMode();
                    StartThrow();
                    break;
                case vThrowAnimatorEvent.ThrowEventType.FinishLaunch:
                    Throw();
                    break;
            }

        }

        protected virtual void EquipThrowObject()
        {
            onEquipThrowable.Invoke();
        }

        protected virtual void EnableAimMode()
        {
            inEnterThrowMode = false;
            isAiming = true;
            onEnableAim.Invoke();
        }

        protected virtual void DisableAimMode()
        {
            inEnterThrowMode = false;
            isAiming = false;
            onCancelAim.Invoke();
        }

        protected virtual void EnterThrowMode()
        {
            inEnterThrowMode = true;
            tpInput.animator.CrossFadeInFixedTime(holdingAnimation, 0.2f);
            tpInput.SetLockAllInput(true);
            tpInput.SetStrafeLocomotion(strafeWhileAiming);
            if (string.IsNullOrEmpty(cameraState) == false && tpInput.customCameraState != cameraState) tpInput.ChangeCameraStateWithLerp(cameraState);
            tpInput.cc.isSprinting = false;
            tpInput.animator.SetInteger("ActionState", 1);
            if (cameraStyle == CameraStyle.SideScroll)
            {
                tpInput.cc.strafeSpeed.rotateWithCamera = true;
            }
        }

        protected virtual void StartThrow()
        {
            onStartThrowObject.Invoke();
        }

        protected virtual void ExitThrowMode()
        {
            isThrowing = false;
            tpInput.SetLockAllInput(false);
            tpInput.SetStrafeLocomotion(false);
            tpInput.cc.isSprinting = false;
            tpInput.animator.SetInteger("ActionState", 0);
            onFinishThrow.Invoke();
        }

        public virtual bool CanCollectThrowable(string throwableName, out int remainingAmount) { remainingAmount = 0; return false; }

        public virtual void OnCollectThrowable(string throwableName, int amount = 1) { }

        protected virtual Vector3 thirdPersonAimPoint
        {
            get
            {
                Vector3 endPoint = tpInput.cameraMain.transform.position + tpInput.cameraMain.transform.forward * maxDistance;
                if (Physics.Linecast(tpInput.cameraMain.transform.position, endPoint, out RaycastHit hit, obstacles))
                {
                    endPoint = hit.point;
                }
                return endPoint;
            }
        }

        protected virtual Vector3 topDownAimPoint
        {
            get
            {
                var pos = vMousePositionHandler.Instance.WorldMousePosition(obstacles);
                return pos;
            }
        }

        protected virtual Vector3 sideScrollAimPoint
        {
            get
            {
                Vector3 screen = vMousePositionHandler.Instance.mousePosition;
                screen.z = tpInput.cameraMain.WorldToScreenPoint(startPoint).z;
                var world = tpInput.cameraMain.ScreenToWorldPoint(screen, Camera.MonoOrStereoscopicEye.Mono);
                var local = transform.InverseTransformVector(world);
                var localStart = transform.InverseTransformVector(startPoint);
                local.x = localStart.x;
                return transform.TransformVector(local);
            }
        }
        protected virtual Vector3 startPoint
        {
            get
            {
                Vector3 startPosition = throwStartPoint.position;
                var direction = aimPoint - throwStartPoint.position;

                var localPoint = Vector3.zero;
                localPoint.y = throwStartPoint.localPosition.y;
                var localStartPoint = transform.InverseTransformPoint(startPosition);
                Vector3 point = transform.TransformPoint(localPoint) + direction.normalized * localStartPoint.z;
                if (useThrowStartRightOffset && tpInput && tpInput.tpCamera && tpInput.tpCamera.lerpState != null)
                {
                    point += tpInput.tpCamera.transform.right * tpInput.tpCamera.lerpState.right * throwStartRightOffsetMultiplier * tpInput.tpCamera.switchRight;
                }
                var directionFromCenter = point - transform.TransformPoint(localPoint);
                if (Physics.Linecast(transform.TransformPoint(localPoint), point, out RaycastHit hit, obstacles))
                    point = hit.point - directionFromCenter.normalized * 0.2f;
                return point;
            }
        }

        protected virtual Vector3 StartVelocity
        {
            get
            {
                return GetStartVelocity();
                //if (cameraStyle != CameraStyle.SideScroll)
                //{
                //    return GetStartVelocity();                    
                //}
                //else
                //{
                //    var force = throwMaxForce;
                //    return aimDirection.normalized * force;
                //}
            }
        }

        protected virtual void CalculateAimPoint()
        {
            switch (cameraStyle)
            {
                case CameraStyle.ThirdPerson:
                    aimPoint = thirdPersonAimPoint;
                    break;
                case CameraStyle.TopDown:
                    aimPoint = topDownAimPoint;
                    break;
                case CameraStyle.SideScroll:
                    aimPoint = sideScrollAimPoint;
                    break;
            }
        }

        protected virtual Vector3 GetStartVelocity(Vector3? startPoint = null)
        {
            var endPoint = this.aimPoint;
            Vector3 distance = endPoint - (startPoint != null ? (Vector3)startPoint : this.startPoint);
            Vector3 distanceXZ = distance;
            // distanceXZ.y = 0;

            float Sy = distance.y;
            float Sxz = distanceXZ.magnitude;
            float time = Mathf.Clamp(distance.magnitude / metersPerSeconds, minMaxTime.x, minMaxTime.y);
            float Vxz = Sxz / time;
            float Vy = Sy / time + 0.5f * Mathf.Abs(Physics.gravity.y) * time;

            Vector3 result = distanceXZ.normalized;

            result *= Vxz;
            result.y = Vy;
            return result.normalized * Mathf.Min(maxVelocity, result.magnitude);
        }

        protected virtual Vector3 PlotTrajectoryAtTime(Vector3 start, Vector3 startVelocity, float time)
        {
            return start + startVelocity * time + Physics.gravity * time * time * 0.5f;
        }

        protected virtual List<Vector3> GetTrajectoryPoints(Vector3 start, Vector3 startVelocity, float timestep, float maxLength, ref Vector3? hitPoint)
        {
            Vector3 prev = start;
            List<Vector3> points = new List<Vector3>();
            points.Add(prev);

            surfaceNormal = alignToViewWhenNotHit ? (tpInput.cameraMain.transform.position - aimPoint).normalized : Vector3.up;
            float distance = 0;

            for (int i = 1; ; i++)
            {
                float t = timestep * i;
                Vector3 pos = PlotTrajectoryAtTime(start, startVelocity, t);
                RaycastHit hit;
                var dir = (pos - prev).normalized;
                if (Physics.Linecast(prev, pos + dir * 0.1f, out hit, obstacles))
                {
                    points.Add(hit.point);
                    if (alignToSurfaceNormal) surfaceNormal = hit.normal;
                    hitPoint = hit.point;
                    if (debugMode) Debug.DrawLine(prev, hit.point, Color.red);
                    break;
                }
                else
                {
                    if (debugMode) Debug.DrawLine(prev, pos, Color.red);
                    points.Add(pos);
                    distance += (pos - prev).magnitude;
                    prev = pos;
                    if (distance > maxLength) break;
                }
            }
            return points;
        }

        public virtual Vector3 aimPoint
        {
            get; protected set;
        }

        public virtual Vector3 aimDirection
        {
            get
            {
                return aimPoint - startPoint;
            }
        }
    }

}