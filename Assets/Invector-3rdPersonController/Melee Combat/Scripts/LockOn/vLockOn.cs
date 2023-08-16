using UnityEngine;

namespace Invector.vCharacterController
{
    [vClassHeader("MELEE LOCK-ON")]
    public class vLockOn : vLockOnBehaviour
    {
        #region variables
        [System.Serializable]
        public class LockOnEvent : UnityEngine.Events.UnityEvent<Transform> { }

        [Tooltip("Make sure to disable or change the StrafeInput to a different key at the Player Input component")]
        public bool strafeWhileLockOn = true;
        [Tooltip("Create a Image inside the UI and assign here")]
        public RectTransform aimImagePrefab;
        public Canvas aimImageContainer;
        public Vector2 aimImageSize = new Vector2(30, 30);
        [Tooltip("True: Hide the sprite when not Lock On, False: Always show the Sprite")]
        public bool hideSprite = true;
        [Tooltip("Create a offset for the sprite based at the center of the target")]
        [Range(-0.5f, 0.5f)]
        public float spriteHeight = 0.25f;
        [Tooltip("Offset for the camera height")]
        public float cameraHeightOffset;
        [Tooltip("Transition Speed for the Camera")]
        public float lockSpeed = 0.5f;
        [Header("LockOn Inputs")]
        public GenericInput lockOnInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");
        public GenericInput nexTargetInput = new GenericInput("X", false, false, "RightAnalogHorizontal", true, false, "X", false, false);
        public GenericInput previousTargetInput = new GenericInput("Z", false, false, "RightAnalogHorizontal", true, true, "Z", false, false);

        internal bool isLockingOn;
        public LockOnEvent onLockOnTarget;
        public LockOnEvent onUnLockOnTarget;
        protected Canvas _aimCanvas;
        protected RectTransform _aimImage;

        protected bool _inTarget;
        protected virtual bool inTarget { get { return _inTarget; } set { _inTarget = value; } }
        protected vMeleeCombatInput tpInput;

        #endregion

        protected override void Start()
        {
            base.Start();
            Init();

            tpInput = GetComponent<vMeleeCombatInput>();
            if (tpInput)
            {
                tpInput.onUpdate -= UpdateLockOn;
                tpInput.onUpdate += UpdateLockOn;

                // access the HealthController to Reset the LockOn when Dead
                GetComponent<vHealthController>().onDead.AddListener((GameObject g) =>
                {
                    // action to reset lockOn
                    isLockingOn = false;
                    LockOn(false);
                    UpdateLockOn();
                });
            }

            if (!aimImageContainer)
            {
                aimImageContainer = gameObject.GetComponentInChildren<Canvas>(true);
            }
        }

        public RectTransform aimImage
        {
            get
            {
                if (_aimImage) return _aimImage;
                if (aimImageContainer)
                {
                    _aimImage = Instantiate(aimImagePrefab, Vector2.zero, Quaternion.identity) as RectTransform;
                    _aimImage.SetParent(aimImageContainer.transform);
                    return _aimImage;
                }
                else
                {
                    Debug.LogWarning("Missing UI Canvas in the scene, please add one");
                }
                return null;
            }
        }

        protected virtual void UpdateLockOn()
        {
            if (this.tpInput == null) return;
            LockOnInput();
            SwitchTargetsInput();
            CheckForTargetDistance();
            CheckForCharacterAlive();
            UpdateAimImage();
        }

        protected virtual void LockOnInput()
        {
            if (tpInput.tpCamera == null || tpInput.cc == null) return;

            // lock the camera into a target, if there is any around
            if (lockOnInput.GetButtonDown() && !tpInput.cc.customAction)
            {
                isLockingOn = !isLockingOn;
                LockOn(isLockingOn);
            }
            // unlock the camera if the target is null
            else if (isLockingOn && (tpInput.tpCamera.lockTarget == null) || LostTargetDistance())
            {
                isLockingOn = false;
                LockOn(false);
            }
            // choose to use lock-on with strafe of free movement
            if (strafeWhileLockOn && !tpInput.cc.locomotionType.Equals(vThirdPersonMotor.LocomotionType.OnlyStrafe))
            {
                if (isLockingOn && tpInput.tpCamera.lockTarget != null)
                {
                    tpInput.cc.lockInStrafe = true;
                    tpInput.cc.isStrafing = true;
                }
                else
                {
                    tpInput.cc.lockInStrafe = false;
                    tpInput.cc.isStrafing = false;
                }
            }
        }

        protected virtual bool LostTargetDistance()
        {
            if (!isLockingOn || currentTarget == null) return false;

            float distance = Vector3.Distance(transform.position, currentTarget.position);

            return distance > rangeToExitLockOn ? true : false;
        }

        protected override void SetTarget()
        {
            if (tpInput.tpCamera != null)
            {
                tpInput.tpCamera.SetLockTarget(currentTarget.transform, cameraHeightOffset, lockSpeed);
                onLockOnTarget.Invoke(currentTarget);
            }
        }

        protected virtual void SwitchTargetsInput()
        {
            if (tpInput.tpCamera == null) return;

            if (tpInput.tpCamera.lockTarget)
            {
                // switch between targets using Keyboard
                if (previousTargetInput.GetButtonDown()) PreviousTarget();
                else if (nexTargetInput.GetButtonDown()) NextTarget();
            }
        }

        protected virtual void CheckForTargetDistance()
        {
            if (!isLockingOn || currentTarget == null) return;

            float distance = Vector3.Distance(transform.position, currentTarget.position);

            if (distance > rangeToExitLockOn)
            {
                isLockingOn = false;
                LockOn(false);
            }
        }

        protected virtual void CheckForCharacterAlive()
        {
            if (currentTarget && !isCharacterAlive() && inTarget || (inTarget && !isCharacterAlive()))
            {
                ResetLockOn();
                inTarget = false;
                LockOn(true);
                StopLockOn();
            }
        }

        protected virtual void LockOn(bool value)
        {
            base.UpdateLockOn(value);
            if (!inTarget && currentTarget)
            {
                inTarget = true;
                // send current target if inTarget           
                SetTarget();
            }
            else if (inTarget && !currentTarget)
            {
                inTarget = false;
                // send message to clear current target
                StopLockOn();
            }
        }

        protected virtual void UpdateAimImage()
        {
            if (!aimImageContainer || !aimImage) return;
            if (hideSprite)
            {
                aimImage.sizeDelta = aimImageSize;
                if (currentTarget && !aimImage.transform.gameObject.activeSelf && isCharacterAlive())
                    aimImage.transform.gameObject.SetActive(true);
                else if (!currentTarget && aimImage.transform.gameObject.activeSelf)
                    aimImage.transform.gameObject.SetActive(false);
                else if (_aimImage.transform.gameObject.activeSelf && !isCharacterAlive())
                    aimImage.transform.gameObject.SetActive(false);
            }
            if (currentTarget && aimImage && aimImageContainer)
                aimImage.anchoredPosition = currentTarget.GetScreenPointOffBoundsCenter(aimImageContainer, tpCamera.targetCamera, spriteHeight);
            else if (aimImageContainer)
                aimImage.anchoredPosition = Vector2.zero;
        }

        public virtual void StopLockOn()
        {
            if (currentTarget == null && tpInput.tpCamera != null)
            {
                onUnLockOnTarget.Invoke(tpInput.tpCamera.lockTarget);
                tpInput.tpCamera.RemoveLockTarget();
                isLockingOn = false;
                inTarget = false;
            }
        }

        public virtual void NextTarget()
        {
            base.ChangeTarget(1);
        }

        public virtual void PreviousTarget()
        {
            base.ChangeTarget(-1);
        }
    }
}