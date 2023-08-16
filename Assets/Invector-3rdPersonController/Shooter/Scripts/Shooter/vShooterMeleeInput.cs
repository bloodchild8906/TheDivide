using UnityEngine;

namespace Invector.vCharacterController
{
    using IK;
    using vShooter;

    [vClassHeader("SHOOTER/MELEE INPUT", iconName = "inputIcon")]
    public class vShooterMeleeInput : vMeleeCombatInput, vIShooterIKController, PlayerController.vILockCamera
    {
        #region Shooter Inputs       

        [vEditorToolbar("Inputs")]
        [Header("Shooter Inputs")]
        public GenericInput aimInput = new GenericInput("Mouse1", false, "LT", true, "LT", false);
        public GenericInput shotInput = new GenericInput("Mouse0", false, "RT", true, "RT", false);
        public GenericInput reloadInput = new GenericInput("R", "LB", "LB");
        public GenericInput switchCameraSideInput = new GenericInput("Tab", "RightStickClick", "RightStickClick");
        public GenericInput scopeViewInput = new GenericInput("Z", "RB", "RB");

        #endregion

        #region Shooter Variables       

        [HideInInspector] public vShooterManager shooterManager;
        public vArmAimAlign leftArmAim, rightArmAim;

        public virtual bool isAimingByInput { get; set; }
        public virtual bool isReloading { get; set; }
        public virtual bool ignoreIK { get; set; }
        public virtual bool aimConditions { get; protected set; }
        public virtual bool allowAttack { get; set; }
        public virtual bool isCameraRightSwitched { get; set; }

        protected virtual bool wasAiming { get; set; }
        protected virtual bool wasAimingWithScope { get; set; }
        protected virtual bool lockShooterInput { get; set; }
        protected virtual bool checkCanAimInit { get; set; }
        protected virtual bool _ignoreIKFromAnimator { get; set; }
        protected virtual bool _walkingByDefaultWasChanged { get; set; }
        protected bool _isUsingScopeView;
        protected virtual bool isUsingScopeView { get { return _isUsingScopeView; } set { _isUsingScopeView = value; } }

        public virtual int onlyArmsLayer { get; set; }
        public virtual int shotLayer { get; set; }
        public virtual int shootCountA { get; set; }

        public virtual float onlyArmsLayerWeight { get; set; }
        public virtual float supportIKWeight { get; set; }
        public virtual float weaponIKWeight { get; set; }

        public virtual float armAlignmentWeight { get; protected set; }

        protected virtual float _aimTiming { get; set; }
        protected virtual float checkCanAimOffsetStartX { get; set; }
        protected virtual float checkCanAimOffsetStartY { get; set; }
        protected virtual float checkCanAimOffsetEndX { get; set; }
        protected virtual float checkCanAimOffsetEndY { get; set; }
        protected virtual float checkCanAimHeight { get; set; }
        protected virtual float scopeDirectionWeight { get; set; }

        protected virtual GameObject aimAngleReference { get; set; }
        protected virtual GameObject lastShooterWeapon { get; set; }
        protected virtual Vector3 localAimPosition { get; set; }
        protected virtual Vector3 aimHitPoint { get; set; }
        protected virtual Vector3 upperArmPosition { get; set; }
        protected virtual Vector3 muzzlePosition { get; set; }
        protected virtual Vector3 muzzleForward { get; set; }
        protected virtual Vector3 ikRotationOffset { get; set; }
        protected virtual Vector3 ikPositionOffset { get; set; }
        protected virtual Vector3 scopeLocalPosition { get; set; }
        protected virtual Vector3 scopeCameraLocalPosition { get; set; }
        protected virtual Vector3 scopeLocalForward { get; set; }
        protected virtual Vector3 scopeCameraLocalForward { get; set; }
        protected virtual Vector3 lastTargetCameraEuler { get; set; }

        protected virtual Quaternion handRotation { get; set; }
        protected virtual Quaternion upperArmRotation { get; set; }
        protected virtual Quaternion upperArmRotationAlignment { get; set; }
        protected virtual Quaternion handRotationAlignment { get; set; }

        protected vHeadTrack headTrack;
        protected vControlAimCanvas _controlAimCanvas;
        protected IKAdjust _currentIKAdjust;

        protected RaycastHit checkCanAimHit;

        protected Transform leftHand, rightHand, rightLowerArm, leftLowerArm, rightUpperArm, leftUpperArm;

        #region IKController Interface Properties

        public virtual vIKSolver LeftIK { get; set; }
        public virtual vIKSolver RightIK { get; set; }

        public virtual vWeaponIKAdjustList WeaponIKAdjustList
        {
            get
            {
                if (shooterManager)
                {
                    return shooterManager.weaponIKAdjustList;
                }


                return null;
            }
            set
            {
                if (shooterManager)
                {
                    shooterManager.weaponIKAdjustList = value;
                }
            }
        }

        public virtual vWeaponIKAdjust CurrentWeaponIK
        {
            get
            {
                if (shooterManager)
                {
                    return shooterManager.CurrentWeaponIK;
                }


                return null;
            }
        }

        public virtual IKAdjust CurrentIKAdjust
        {
            get
            {
                if (CurrentWeaponIK == null) return null;
                if (CurrentIKAdjustStateWithTag != (IKWeaponTag + TargetIKAdjustState) || _currentIKAdjust == null)
                {
                    CurrentIKAdjustStateWithTag = (IKWeaponTag + TargetIKAdjustState);
                    CurrentIKAdjustState = TargetIKAdjustState;
                    _currentIKAdjust = CurrentWeaponIK.GetIKAdjust(CurrentIKAdjustState, CurrentActiveWeapon.isLeftWeapon);

                }
                return _currentIKAdjust;
            }
        }

        public virtual bool EditingIKGlobalOffset
        {
            get; set;
        }

        public virtual string DefaultIKAdjustState => CurrentWeaponIK ? CurrentWeaponIK.GetDefaultStateName(this) : string.Empty;

        protected virtual string TargetIKAdjustState => (!IsUsingCustomIKAdjust ? DefaultIKAdjustState : CustomIKAdjustState);

        protected virtual string IKWeaponTag => CurrentActiveWeapon ? CurrentActiveWeapon.weaponCategory + "@" : "";

        public virtual string CurrentIKAdjustStateWithTag { get; set; }

        public virtual string CurrentIKAdjustState { get; protected set; }

        public virtual bool IsUsingCustomIKAdjust => !string.IsNullOrEmpty(CustomIKAdjustState);

        public string CustomIKAdjustState { get; protected set; }

        public virtual void SetCustomIKAdjustState(string value)
        {
            if (!string.IsNullOrEmpty(value)) CustomIKAdjustState = value;
        }

        public virtual void ResetCustomIKAdjustState()
        {
            if (!string.IsNullOrEmpty(CustomIKAdjustState)) CustomIKAdjustState = string.Empty;
        }

        public virtual bool IsIgnoreIK
        {
            get
            {
                return ignoreIK || _ignoreIKFromAnimator;
            }
        }

        public virtual bool IsIgnoreSupportHandIK
        {
            get
            {
                return cc.IsAnimatorTag("IgnoreSupportHandIK");
            }
        }
        public virtual bool IsSupportHandIKEnabled
        {
            get; protected set;

        }

        public virtual void UpdateWeaponIK()
        {
            if (shooterManager)
            {
                shooterManager.UpdateWeaponIK();
                if (CurrentWeaponIK == null) return;
                _currentIKAdjust = CurrentWeaponIK.GetIKAdjust(CurrentIKAdjustState, CurrentActiveWeapon.isLeftWeapon);
            }
        }

        public event IKUpdateEvent onStartUpdateIK;

        public event IKUpdateEvent onFinishUpdateIK;
        /// <summary>
        /// Aim position including  <seealso cref="vShooterManager.damageLayer"/>.
        /// This is a point calculated from <seealso cref="vThirdPersonInput.cameraMain"/> position and <seealso cref="vThirdPersonInput.cameraMain"/> forward to get the desired aim position.
        /// Used to align the arms and to define the<see cref="AimPosition"/>
        /// </summary>
        public virtual Vector3 DesiredAimPosition { get; protected set; }

        /// <summary>
        /// Aim position including  <seealso cref="vShooterManager.blockAimLayer"/>.
        /// This is a point calculated from <see cref="vShooterWeapon.aimReference"/> of the weapon and <seealso cref="DesiredAimPosition"/>
        /// </summary>
        public virtual Vector3 AimPosition { get; protected set; }

        public virtual bool LockAiming
        {
            get
            {
                return shooterManager && shooterManager.alwaysAiming;
            }
            set
            {
                shooterManager.alwaysAiming = value;
            }
        }

        public virtual bool LockHipFireAiming
        {
            get; set;
        }

        public virtual bool IsCrouching
        {
            get
            {
                return cc.isCrouching;
            }
            set
            {
                cc.isCrouching = value;
            }
        }

        public virtual bool IsLeftWeapon
        {
            get
            {

                return shooterManager && shooterManager.IsLeftWeapon;
            }
        }

        public virtual bool LockCamera
        {
            get
            {
                return tpCamera && tpCamera.LockCamera;
            }
            set
            {
                if (tpCamera)
                {
                    tpCamera.LockCamera = value;
                }
            }
        }
        #endregion

        /// <summary>
        /// Is Aiming by <see cref="isAimingByInput"/> or <see cref="isAimingByHipFire"/>
        /// </summary>
        public virtual bool IsAiming
        {
            get
            {
                return lockShooterInput == false && (!cc.isRolling) && (isAimingByInput || isAimingByHipFire);
            }
        }

        public virtual bool isAimingByHipFire
        {
            get
            {
                if (!shooterManager.hipfireShot && _aimTiming > 0 || (isReloading && !shooterManager.keepAimingWhenReload) || isEquipping || lockShooterInput)
                {
                    _aimTiming = 0;
                    return false;
                }
                return shooterManager.hipfireShot && ((_aimTiming > 0 || (shotInput.GetButton() && shooterManager.CurrentWeapon != null)) || (!isAimingByInput && shootCountA > 0));
            }
        }

        public virtual vControlAimCanvas controlAimCanvas
        {
            get
            {
                if (!_controlAimCanvas)
                {
                    _controlAimCanvas = FindObjectOfType<vControlAimCanvas>();
                    if (_controlAimCanvas)
                    {
                        _controlAimCanvas.Init(cc);
                    }
                }

                return _controlAimCanvas;
            }
        }

        public override bool lockInventory
        {
            get
            {
                return base.lockInventory || isReloading || cc.customAction || cc.isRolling;
            }
        }

        #endregion

        protected override void Start()
        {
            shooterManager = GetComponent<vShooterManager>();

            base.Start();
            checkCanAimHeight = cc._capsuleCollider.height;
            leftHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftLowerArm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            rightLowerArm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            leftUpperArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightUpperArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

            onlyArmsLayer = animator.GetLayerIndex("OnlyArms");
            shotLayer = animator.GetLayerIndex("Shot");
            aimAngleReference = new GameObject("aimAngleReference");
            aimAngleReference.tag = ("Ignore Ragdoll");
            aimAngleReference.transform.rotation = transform.rotation;
            var aimAngleParent = animator.GetBoneTransform(HumanBodyBones.Head);
            aimAngleReference.transform.SetParent(aimAngleParent);
            aimAngleReference.transform.localPosition = Vector3.zero;

            headTrack = GetComponent<vHeadTrack>();
            if (headTrack)
            {
                headTrack.onInitUpdate.AddListener(UpdateAimAngleReference);
            }
            if (!controlAimCanvas)
            {
                Debug.LogWarning("Missing the AimCanvas, drag and drop the prefab to this scene in order to Aim", gameObject);
            }
            muzzlePosition = Vector3.forward * cc._capsuleCollider.radius * 2;
            muzzleForward = Vector3.forward;

        }

        protected override void LateUpdate()
        {
            if ((!updateIK && animator.updateMode == AnimatorUpdateMode.AnimatePhysics))
            {
                return;
            }

            base.LateUpdate();
            UpdateAimBehaviour();
        }


        #region Shooter Inputs    

        protected virtual void Reset()
        {
            // We change the Melee Attack Input for the Shooter because 'Mouse1' is the same input to Shot a Fire Weapon
            weakAttackInput = new GenericInput("Mouse2", "RB", "RB");
            // By default it's disable because it uses the same input as the switchCameraSideInput
            strafeInput.useInput = false;
        }

        /// <summary>
        /// Lock only shooter inputs
        /// </summary>
        /// <param name="value">lock or unlock</param>
        public virtual void SetLockShooterInput(bool value)
        {
            lockShooterInput = value;

            if (value)
            {
                isBlocking = false;
                isAimingByInput = false;
                _aimTiming = 0f;
                if (controlAimCanvas)
                {
                    controlAimCanvas.SetActiveAim(false);
                    controlAimCanvas.SetActiveScopeCamera(false);

                }
            }
        }

        public override void SetLockAllInput(bool value)
        {
            base.SetLockAllInput(value);
            SetLockShooterInput(value);
        }

        /// <summary>
        /// Set Always Aiming
        /// </summary>
        /// <param name="value">value to set aiming</param>
        public virtual void SetAlwaysAim(bool value)
        {
            shooterManager.alwaysAiming = value;
        }

        /// <summary>
        /// Current active weapon (if weapon gameobject is disabled this return null)
        /// </summary>
        public virtual vShooterWeapon CurrentActiveWeapon
        {
            get
            {
                return shooterManager.CurrentActiveWeapon;
            }
        }

        /// <summary>
        /// Handles all the Controller Input 
        /// </summary>
        public override void InputHandle()
        {
            if (cc == null || cc.isDead)
            {
                return;
            }

            #region BasicInput

            if (!cc.ragdolled && !lockInput)
            {
                MoveInput();
                SprintInput();
                CrouchInput();
                StrafeInput();
                JumpInput();
                RollInput();
            }

            #endregion

            #region MeleeInput

            if (MeleeAttackConditions() && !IsAiming && !isReloading && !lockMeleeInput && !CurrentActiveWeapon)
            {
                if (shooterManager.canUseMeleeWeakAttack_H || shooterManager.CurrentWeapon == null)
                {
                    MeleeWeakAttackInput();
                }

                if (shooterManager.canUseMeleeStrongAttack_H || shooterManager.CurrentWeapon == null)
                {
                    MeleeStrongAttackInput();
                }

                if (shooterManager.canUseMeleeBlock_H || shooterManager.CurrentWeapon == null)
                {
                    BlockingInput();
                }
                else
                {
                    isBlocking = false;
                }
            }

            #endregion

            #region ShooterInput

            if (lockShooterInput)
            {
                isAimingByInput = false;
                _aimTiming = 0;
                if (controlAimCanvas != null)
                {
                    if (controlAimCanvas.isAimActive || controlAimCanvas.isScopeCameraActive)
                    {
                        isUsingScopeView = false;
                        controlAimCanvas.DisableAim();
                    }
                }
            }
            else if (shooterManager.CurrentWeapon)
            {
                if (MeleeAttackConditions() && (!IsAiming || shooterManager.canUseMeleeAiming))
                {
                    if (shooterManager.canUseMeleeWeakAttack_E)
                    {
                        MeleeWeakAttackInput();
                    }
                    if (shooterManager.canUseMeleeStrongAttack_E)
                    {
                        MeleeStrongAttackInput();
                    }
                    if (shooterManager.canUseMeleeBlock_E)
                    {
                        BlockingInput();
                    }
                    else
                    {
                        isBlocking = false;
                    }
                }
                else
                {
                    isBlocking = false;
                }

                if (shooterManager == null || CurrentActiveWeapon == null || isEquipping)
                {
                    if (_walkingByDefaultWasChanged)
                    {
                        _walkingByDefaultWasChanged = false;
                        ResetWalkByDefault();
                    }
                    if (IsAiming)
                    {
                        isAimingByInput = false;
                        _aimTiming = 0;
                        if (!cc.lockInStrafe && cc.isStrafing)
                        {
                            cc.Strafe();
                        }

                        if (controlAimCanvas != null)
                        {
                            if (controlAimCanvas.isAimActive || controlAimCanvas.isScopeCameraActive)
                            {
                                isUsingScopeView = false;
                                controlAimCanvas.DisableAim();
                            }
                        }
                        if (shooterManager && shooterManager.CurrentWeapon && shooterManager.CurrentWeapon.chargeWeapon && shooterManager.CurrentWeapon.powerCharge != 0)
                        {
                            CurrentActiveWeapon.powerCharge = 0;
                        }

                        shootCountA = 0;
                    }
                }
                else
                {
                    AimInput();
                    ShotInput();
                    ReloadInput();
                    SwitchCameraSideInput();
                    ScopeViewInput();
                }
            }
            else
            {
                isAimingByInput = false;
                _aimTiming = 0;
                if (!cc.lockInStrafe && cc.isStrafing)
                {
                    cc.Strafe();

                }
                if (_walkingByDefaultWasChanged && !IsAiming)
                {
                    _walkingByDefaultWasChanged = false;
                    ResetWalkByDefault();
                }
                if (controlAimCanvas != null)
                {
                    if (controlAimCanvas.isAimActive || controlAimCanvas.isScopeCameraActive)
                    {
                        isUsingScopeView = false;
                        controlAimCanvas.DisableAim();
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Override the Melee TriggerStrongAttack method to add the call to CancelReload when attacking
        /// </summary>
        public override void TriggerStrongAttack()
        {
            shooterManager.CancelReload();
            base.TriggerStrongAttack();
        }

        /// <summary>
        /// Control Aim Input
        /// </summary>
        public virtual void AimInput()
        {
            //Change Rotation Method While Aiming 
            cc.strafeSpeed.rotateWithCamera = IsAiming ? true : cc.strafeSpeed.defaultRotateWithCamera;
            if (_walkingByDefaultWasChanged && !IsAiming)
            {
                _walkingByDefaultWasChanged = false;
                ResetWalkByDefault();
            }
            if (!shooterManager || isEquipping || isAttacking || (isReloading && (isUsingScopeView || !shooterManager.keepAimingWhenReload)))
            {
                if (!isReloading || (isReloading && !shooterManager.keepAimingWhenReload))
                {
                    isAimingByInput = false;

                    ResetWalkByDefault();
                    _walkingByDefaultWasChanged = false;
                    if (cc.isStrafing && cc.locomotionType != vThirdPersonMotor.LocomotionType.OnlyStrafe)
                    {
                        cc.Strafe();
                    }
                }
                if (controlAimCanvas)
                {
                    if (controlAimCanvas.isAimActive || controlAimCanvas.isScopeCameraActive)
                    {
                        isUsingScopeView = false;
                        controlAimCanvas.DisableAim();
                    }

                }

                return;
            }


            if (LockHipFireAiming) _aimTiming = 1f;

            if (shooterManager.onlyWalkWhenAiming && (!isReloading || shooterManager.keepAimingWhenReload))
            {
                if (isAimingByInput)
                {
                    SetWalkByDefault(true);
                    _walkingByDefaultWasChanged = true;
                }
                else
                {
                    ResetWalkByDefault();
                    _walkingByDefaultWasChanged = false;
                }
            }

            if (cc.locomotionType == vThirdPersonMotor.LocomotionType.OnlyFree)
            {
                Debug.LogWarning("Shooter behaviour needs to be OnlyStrafe or Free with Strafe. \n Please change the Locomotion Type.");
                return;
            }

            if (shooterManager.hipfireShot && !LockHipFireAiming)
            {
                // countdown for the hipfire to reset the aim back to idle
                if (_aimTiming > 0 && CanRotateAimArm())
                {
                    _aimTiming -= Time.deltaTime;
                }

                // reset the aimTimming if you sprint while still aiming through the hipfire
                if (sprintInput.GetButtonDown() && _aimTiming > 0f)
                {
                    _aimTiming = 0f;
                }
            }

            if (!shooterManager || !CurrentActiveWeapon)
            {
                if (controlAimCanvas)
                {
                    if (controlAimCanvas.isAimActive || controlAimCanvas.isScopeCameraActive)
                    {
                        isUsingScopeView = false;
                        controlAimCanvas.DisableAim();
                    }
                }
                isAimingByInput = false;
                if (cc.isStrafing)
                {
                    cc.Strafe();
                }
                return;
            }

            if (!cc.isRolling)
            {
                isAimingByInput = (!isReloading || shooterManager.keepAimingWhenReload) && (aimInput.GetButton() || (shooterManager.alwaysAiming && CurrentActiveWeapon)) && !cc.ragdolled && !cc.customAction
                    || (cc.customAction && cc.isJumping);
            }

            if (aimInput.GetButtonUp() && !shotInput.GetButton())
            {
                _aimTiming = 0f;
            }
            if (headTrack)
            {
                headTrack.alwaysFollowCamera = isAimingByInput;
            }

            if (cc.locomotionType == vThirdPersonMotor.LocomotionType.FreeWithStrafe)
            {
                if (!cc.lockInStrafe)
                {
                    if (IsAiming && !cc.isStrafing)
                    {
                        cc.Strafe();
                    }
                    else if (!IsAiming && cc.isStrafing)
                    {
                        cc.Strafe();
                    }
                }

            }
            if (IsAiming && shooterManager.onlyWalkWhenAiming && cc.isSprinting)
            {
                cc.isSprinting = false;
            }

            if (controlAimCanvas)
            {
                if (!isUsingScopeView)
                {

                    if (IsAiming && !controlAimCanvas.isAimActive)
                    {
                        controlAimCanvas.SetActiveAim(true);
                    }

                    if (!IsAiming && controlAimCanvas.isAimActive)
                    {
                        controlAimCanvas.SetActiveAim(false);
                    }
                }
            }
            if (shooterManager.rWeapon)
            {
                shooterManager.rWeapon.SetActiveAim(IsAiming && aimConditions);
                shooterManager.rWeapon.SetActiveScope(IsAiming && isUsingScopeView);
            }
            else if (shooterManager.lWeapon)
            {
                shooterManager.lWeapon.SetActiveAim(IsAiming && aimConditions);
                shooterManager.lWeapon.SetActiveScope(IsAiming && isUsingScopeView);
            }
        }

        /// <summary>
        /// Control shot inputs (primary and secundary weapons)
        /// </summary>
        public virtual void ShotInput()
        {
            if (!shooterManager || CurrentActiveWeapon == null || cc.isDead || isReloading || isAttacking || isEquipping)
            {
                if (shooterManager && shooterManager.CurrentWeapon.chargeWeapon && shooterManager.CurrentWeapon.powerCharge != 0)
                {
                    CurrentActiveWeapon.powerCharge = 0;
                }

                shootCountA = 0;

                return;
            }

            if (IsAiming && !shooterManager.isShooting && aimConditions)
            {
                if (CurrentActiveWeapon || (shooterManager.CurrentWeapon && shooterManager.hipfireShot))
                {
                    HandleShotCount(shooterManager.CurrentWeapon, shotInput.GetButton());
                }
            }
            else if (!IsAiming)
            {
                if (shooterManager.CurrentWeapon.chargeWeapon && shooterManager.CurrentWeapon.powerCharge != 0)
                {
                    CurrentActiveWeapon.powerCharge = 0;
                }

                shootCountA = 0;
            }
        }

        /// <summary>
        /// Control Shot count
        /// </summary>
        /// <param name="weapon">target weapon</param>
        /// <param name="weaponInput">check input</param>
        public virtual void HandleShotCount(vShooterWeapon weapon, bool weaponInput = true)
        {

            if (weapon.chargeWeapon)
            {
                if (shooterManager.WeaponHasLoadedAmmo() && weapon.powerCharge < 1 && weaponInput)
                {
                    if (shooterManager.hipfireShot)
                    {
                        _aimTiming = shooterManager.HipfireAimTime;
                    }

                    weapon.powerCharge += Time.fixedDeltaTime * weapon.chargeSpeed;

                }
                else if ((weapon.powerCharge >= 1 && weapon.autoShotOnFinishCharge && weaponInput) ||
                    (!weaponInput && IsAiming && weapon.powerCharge > 0f))
                {
                    if (shooterManager.hipfireShot)
                    {
                        _aimTiming = shooterManager.HipfireAimTime;
                    }
                    shootCountA = 1;

                }
                animator.SetFloat(vAnimatorParameters.PowerCharger, weapon.powerCharge);
            }
            else if (weapon.automaticWeapon && weaponInput)
            {
                if (shooterManager.hipfireShot && !isAimingByInput)
                {

                    _aimTiming = shooterManager.HipfireAimTime;

                }
                shootCountA = 1;
            }
            else if (weaponInput)
            {
                if (shooterManager.hipfireShot && !isAimingByInput)
                {
                    _aimTiming = shooterManager.HipfireAimTime;
                }

                if (allowAttack == false)
                {
                    shootCountA = 1;
                    allowAttack = true;
                }
            }
            else
            {
                allowAttack = false;
            }
        }

        /// <summary>
        /// Do Shots by shotcount after Ik behaviour updated
        /// </summary>
        public virtual void DoShots()
        {
            if (CanDoShots())
            {
                animator.SetFloat(vAnimatorParameters.Shot_ID, shooterManager.GetShotID());
                Debug.DrawLine(CurrentActiveWeapon.aimReference.position, AimPosition, Color.red, 10f);
                shooterManager.Shoot(AimPosition, !isAimingByInput);
                if (CurrentActiveWeapon.chargeWeapon) CurrentActiveWeapon.powerCharge = 0;
                shootCountA--;
            }
        }

        /// <summary>
        /// Reload current weapon
        /// </summary>
        public virtual void ReloadInput()
        {
            if (!shooterManager || CurrentActiveWeapon == null || isReloading || cc.customAction || shooterManager.isShooting || cc.ragdolled)
            {
                return;
            }

            if (reloadInput.GetButtonDown())
            {
                shootCountA = 0;
                _aimTiming = 0f;
                shooterManager.ReloadWeapon();
            }
            else
            {
                if (CurrentActiveWeapon.autoReload && !shooterManager.WeaponHasLoadedAmmo() && shooterManager.WeaponHasUnloadedAmmo())
                {
                    switch (CurrentActiveWeapon.autoReloadStyle)
                    {
                        case vShooterWeapon.AutoReloadStyle.WhenAiming:
                            if (IsAiming) shooterManager.ReloadWeapon();
                            break;
                        case vShooterWeapon.AutoReloadStyle.WhenShot:
                            if (shotInput.GetButtonDown()) shooterManager.ReloadWeapon();
                            break;
                        case vShooterWeapon.AutoReloadStyle.WhenAmmoAvailable:
                            shooterManager.ReloadWeapon();
                            break;
                    }
                }

            }
        }

        /// <summary>
        /// Control Switch Camera side Input
        /// </summary>
        public virtual void SwitchCameraSideInput()
        {
            if (tpCamera == null)
            {
                return;
            }

            if (switchCameraSideInput.GetButtonDown())
            {
                SwitchCameraSide();
            }
        }

        /// <summary>
        /// Change side view of the <seealso cref="Invector.vCamera.vThirdPersonCamera"/>
        /// </summary>
        public virtual void SwitchCameraSide()
        {
            if (tpCamera == null)
            {
                return;
            }

            isCameraRightSwitched = !isCameraRightSwitched;
            tpCamera.SwitchRight(isCameraRightSwitched);
        }

        /// <summary>
        /// Reset the Aiming and AimCanvas to false
        /// </summary>
        public virtual void CancelAiming()
        {
            isAimingByInput = false;
            _aimTiming = 0;
            if (controlAimCanvas)
            {
                controlAimCanvas.SetActiveAim(false);
                controlAimCanvas.SetActiveScopeCamera(false);
            }
        }

        /// <summary>
        /// Control Scope view input
        /// </summary>
        public virtual void ScopeViewInput()
        {
            if (!shooterManager || CurrentActiveWeapon == null)
            {
                return;
            }

            if (isAimingByInput && aimConditions && (scopeViewInput.GetButtonDown() || CurrentActiveWeapon.onlyUseScopeUIView))
            {
                if (controlAimCanvas && CurrentActiveWeapon.scopeTarget)
                {
                    if (!isUsingScopeView && CurrentActiveWeapon.onlyUseScopeUIView)
                    {
                        EnableScopeView();
                    }
                    else if (isUsingScopeView && !CurrentActiveWeapon.onlyUseScopeUIView)
                    {
                        DisableScopeView();
                    }
                    else if (!isUsingScopeView)
                    {
                        EnableScopeView();
                    }
                }
            }
            else if (isUsingScopeView && (controlAimCanvas && !isAimingByInput || controlAimCanvas && !aimConditions || cc.isRolling))
            {
                DisableScopeView();
            }
        }

        /// <summary>
        /// Enable scope view (just if is aiming)
        /// </summary>
        public virtual void EnableScopeView()
        {
            if (!isAimingByInput || !controlAimCanvas.scopeBackgroundCamera || isReloading || isEquipping)
            {
                return;
            }

            isUsingScopeView = true;

            controlAimCanvas.SetActiveScopeCamera(true, CurrentActiveWeapon.useUI);
            controlAimCanvas.SetActiveAim(false);
        }

        /// <summary>
        /// Disable scope view
        /// </summary>
        public virtual void DisableScopeView()
        {
            if (!controlAimCanvas.scopeBackgroundCamera) return;
            isUsingScopeView = false;
            var lastState = tpCamera.useSmooth;
            tpCamera.useSmooth = false;
            tpCamera.mouseX = lastTargetCameraEuler.y;
            tpCamera.mouseY = lastTargetCameraEuler.x;
            tpCamera.CameraMovement();
            tpCamera.useSmooth = lastState;
            controlAimCanvas.SetActiveScopeCamera(false);
            controlAimCanvas.SetActiveAim(IsAiming);

        }

        #endregion

        #region Update Animations

        protected override void UpdateMeleeAnimations()
        {
            // disable the onlyarms layer and run the melee methods if the character is not using any shooter weapon
            if (!animator)
            {
                return;
            }

            // find states with the IsEquipping tag
            isEquipping = cc.IsAnimatorTag("IsEquipping");
            // Check if Animator state need to ignore IK
            _ignoreIKFromAnimator = cc.IsAnimatorTag("IgnoreIK");

            if (cc.customAction)
            {
                ResetMeleeAnimations();
                ResetShooterAnimations();
                // reset to the default camera state
                UpdateCameraStates();
                // reset the aiming
                CancelAiming();
                return;
            }
            // update MeleeManager Animator Properties
            if ((shooterManager == null || !CurrentActiveWeapon) && meleeManager)
            {
                base.UpdateMeleeAnimations();
                // set the uppbody id (armsonly layer)
                //animator.SetFloat(vAnimatorParameters.UpperBody_ID, 0, .2f, Time.fixedDeltaTime);
                // turn on the onlyarms layer to aim 
                onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, 0, 6f * vTime.fixedDeltaTime);
                animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
                // reset aiming parameter
                animator.SetBool(vAnimatorParameters.IsAiming, false);
                // animator.SetBool(vAnimatorParameters.IsHipFire, false);
                isReloading = false;
            }
            // update ShooterManager Animator Properties
            else if (shooterManager && CurrentActiveWeapon)
            {
                UpdateShooterAnimations();
            }
            // reset Animator Properties
            else
            {
                ResetMoveSet();
                ResetMeleeAnimations();
                ResetShooterAnimations();
            }
        }

        public virtual void ResetMoveSet()
        {
            cc.animator.SetFloat(vAnimatorParameters.MoveSet_ID, defaultMoveSetID, .2f, Time.fixedDeltaTime);
        }

        public virtual void ResetShooterAnimations()
        {
            if (shooterManager == null || !animator)
            {
                return;
            }
            // set the uppbody id (armsonly layer)
            animator.SetFloat(vAnimatorParameters.UpperBody_ID, 0, .2f, vTime.fixedDeltaTime);
            // set if the character can aim or not (upperbody layer)
            animator.SetBool(vAnimatorParameters.CanAim, false);
            // character is aiming
            animator.SetBool(vAnimatorParameters.IsAiming, false);
            // animator.SetBool(vAnimatorParameters.IsHipFire, false);
            // turn on the onlyarms layer to aim 
            onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, 0, 6f * vTime.fixedDeltaTime);
            animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
        }

        protected virtual void UpdateShooterAnimations()
        {
            if (shooterManager == null)
            {
                return;
            }

            // turn on the onlyarms layer to aim 
            onlyArmsLayerWeight = Mathf.Lerp(onlyArmsLayerWeight, (CurrentActiveWeapon || isEquipping) ? 1f : 0f, shooterManager.onlyArmsSpeed * vTime.fixedDeltaTime);
            animator.SetLayerWeight(onlyArmsLayer, onlyArmsLayerWeight);
            if (CurrentActiveWeapon && IsAiming) animator.SetLayerWeight(shotLayer, isUsingScopeView ? CurrentActiveWeapon.scopeShootAnimationWeight : 1f);
            if (CurrentActiveWeapon && !shooterManager.useDefaultMovesetWhenNotAiming || IsAiming)
            {
                // set the move set id (base layer) 
                animator.SetFloat(vAnimatorParameters.MoveSet_ID, shooterMoveSetID, .1f, vTime.fixedDeltaTime);
            }
            else if (!CurrentActiveWeapon && !shooterManager.useDefaultMovesetWhenNotAiming || shooterManager.useDefaultMovesetWhenNotAiming)
            {
                // set the move set id (base layer) 
                animator.SetFloat(vAnimatorParameters.MoveSet_ID, defaultMoveSetID, .1f, vTime.fixedDeltaTime);
            }
            animator.SetInteger(vAnimatorParameters.DefenseID, DefenseID);
            // set the isBlocking false while using shooter weapons
            animator.SetBool(vAnimatorParameters.IsBlocking, isBlocking);
            // set the uppbody id (armsonly layer)
            animator.SetFloat(vAnimatorParameters.UpperBody_ID, shooterManager.GetUpperBodyID());
            // set if the character can aim or not (upperbody layer)
            animator.SetBool(vAnimatorParameters.CanAim, aimConditions);
            // character is aiming
            animator.SetBool(vAnimatorParameters.IsAiming, IsAiming);
            // animator.SetBool(vAnimatorParameters.IsHipFire, isAimingByHipFire);
            // find states with the Reload tag
            isReloading = cc.IsAnimatorTag("IsReloading") || shooterManager.isReloadingWeapon;


            vAnimatorParameter ap = new vAnimatorParameter(animator, "IsReloading");

        }

        /// <summary>
        /// Current moveset id based if is using weapon or not
        /// </summary>
        public virtual int shooterMoveSetID
        {
            get
            {
                int id = shooterManager.GetMoveSetID();
                if (id == 0 || overrideWeaponMoveSetID)
                {
                    id = defaultMoveSetID;
                }

                return id;
            }
        }

        public override void UpdateCameraStates()
        {
            // CAMERA STATE - you can change the CameraState here, the bool means if you want lerp of not, make sure to use the same CameraState String that you named on TPCameraListData
            if (ignoreTpCamera)
            {
                return;
            }

            if (tpCamera == null)
            {
                tpCamera = FindObjectOfType<vCamera.vThirdPersonCamera>();
                if (tpCamera == null)
                {
                    return;
                }

                if (tpCamera)
                {
                    tpCamera.SetMainTarget(this.transform);
                    tpCamera.Init();
                }
            }

            if (changeCameraState)
            {
                tpCamera.ChangeState(customCameraState, customlookAtPoint, true);
            }
            else if (cc.isCrouching && !isAimingByInput)
            {
                tpCamera.ChangeState("Crouch", true);
            }
            else if (cc.isStrafing && !isAimingByInput)
            {
                tpCamera.ChangeState("Strafing", true);
            }
            else if (isAimingByInput && CurrentActiveWeapon)
            {
                if (isUsingScopeView)
                {
                    if (string.IsNullOrEmpty(CurrentActiveWeapon.customScopeCameraState))
                    {
                        tpCamera.ChangeState(cc.isCrouching ? "CrouchingAiming" : "Aiming", true);
                    }
                    else
                    {
                        tpCamera.ChangeState(CurrentActiveWeapon.customScopeCameraState, true);
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(CurrentActiveWeapon.customAimCameraState))
                    {
                        tpCamera.ChangeState(cc.isCrouching ? "CrouchingAiming" : "Aiming", true);
                    }
                    else
                    {
                        tpCamera.ChangeState(CurrentActiveWeapon.customAimCameraState, true);
                    }
                }
            }
            else
            {
                tpCamera.ChangeState("Default", true);
            }
        }

        #endregion

        #region Update Aim

        protected Ray ray = new Ray();
        /// <summary>
        /// Calculate the <see cref="DesiredAimPosition"/>
        /// </summary>
        protected virtual void UpdateDesiredAimPosition()
        {
            if (!shooterManager)
            {
                return;
            }

            if (CurrentActiveWeapon == null)
            {
                return;
            }
            if (isUsingScopeView)
            {
                DesiredAimPosition = cameraMain.transform.TransformPoint(localAimPosition);
                return;
            }
            var camT = cameraMain.transform;

            var vOrigin = camT.position;
            var desiredAimPoint = camT.position + camT.forward * cameraMain.farClipPlane;
            desiredAimPoint.DebugPoint(Color.green, 0.1f, 0.2f);
            ray.origin = vOrigin;
            ray.direction = camT.forward;
            if (shooterManager.raycastAimTarget && CurrentActiveWeapon.raycastAimTarget)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, cameraMain.farClipPlane, shooterManager.damageLayer))
                {
                    bool canAimToHit = false;
                    var dist = hit.distance;

                    //Check if hit object is child of  character transform
                    if (hit.collider.transform.IsChildOf(transform))
                    {
                        var collider = hit.collider;
                        //Clear last hit infor
                        hit = new RaycastHit();
                        var hits = Physics.RaycastAll(ray, cameraMain.farClipPlane, shooterManager.damageLayer);
                        ///Try to find other hit point next to character transform
                        for (int i = 0; i < hits.Length; i++)
                        {
                            if (hits[i].distance < dist && hits[i].collider.gameObject != collider.gameObject && !hits[i].collider.transform.IsChildOf(transform))
                            {
                                dist = hits[i].distance;
                                hit = hits[i];
                                canAimToHit = true;
                            }
                        }
                    }
                    else canAimToHit = true;

                    if (canAimToHit)
                    {
                        desiredAimPoint = hit.point;

                    }
                }
            }

            var localAimPoint = aimAngleReference.transform.InverseTransformPoint(desiredAimPoint);
            if (!isUsingScopeView)
            {
                if (!shooterManager.ignoreBackAimPoint) localAimPoint.z = Mathf.Max(localAimPoint.z, (muzzlePosition.z + shooterManager.backAimPointOffset + shooterManager.minAimHitPointDistance));
                desiredAimPoint = aimAngleReference.transform.TransformPoint(localAimPoint);
            }
            DesiredAimPosition = desiredAimPoint;

            localAimPosition = cameraMain.transform.InverseTransformPoint(DesiredAimPosition);
            desiredAimPoint.DebugPoint(Color.red, 0.1f, 0.1f);

        }


        /// <summary>
        /// Calculate the <see cref="AimPosition"/>
        /// </summary>
        protected virtual void UpdateAimPosition()
        {
            if (!shooterManager)
            {
                return;
            }

            if (CurrentActiveWeapon == null)
            {
                return;
            }

            Vector3 rayDirection = DesiredAimPosition - CurrentActiveWeapon.aimReference.position;
            ray.origin = CurrentActiveWeapon.aimReference.position;
            ray.direction = rayDirection;
            Vector3 desiredAimPoint = DesiredAimPosition;


            RaycastHit hit;
            var castLayer = shooterManager.blockAimLayer;
            var castDistance = rayDirection.magnitude;
            var castRay = ray;

            if (Physics.Raycast(castRay, out hit, castDistance, castLayer))
            {
                bool canAimToHit = false;
                var dist = cameraMain.farClipPlane;

                //Check if hit object is child of  character transform
                if (hit.collider.transform.IsChildOf(transform))
                {
                    var collider = hit.collider;
                    //Clear last hit infor
                    hit = new RaycastHit();
                    var hits = Physics.RaycastAll(castRay, castDistance, castLayer);
                    ///Try to find other hit point next to character transform
                    for (int i = 0; i < hits.Length; i++)
                    {
                        if (hits[i].distance < dist && hits[i].collider.gameObject != collider.gameObject && !hits[i].collider.transform.IsChildOf(transform))
                        {
                            dist = hits[i].distance;
                            hit = hits[i];
                            canAimToHit = true;
                        }
                    }
                }
                else canAimToHit = true;
                if (hit.collider && canAimToHit)
                {
                    desiredAimPoint = hit.point;
                }

            }
            DesiredAimPosition.DebugPoint(Color.green, 0.01f, 0.5f);
            desiredAimPoint.DebugPoint(Color.red, 0.01f, 0.1f);
            AimPosition = desiredAimPoint;

            if (isUsingScopeView)
            {
                var rotation = Quaternion.LookRotation(AimPosition - cameraMain.transform.position, cameraMain.transform.up);
                lastTargetCameraEuler = rotation.eulerAngles.NormalizeAngle();
            }

            if (isAimingByInput)
            {
                shooterManager.CameraSway();
            }

        }

        public override void ControlRotation()
        {

            base.ControlRotation();
        }

        public override void CameraInput()
        {

            base.CameraInput();
        }

        #endregion

        #region IK behaviour       

        protected virtual void UpdateAimBehaviour()
        {
            if (cc.isDead || cc.ragdolled)
            {
                return;
            }

            UpdateDesiredAimPosition();
            UpdateHeadTrack();
            OnStartUpdateIK();
            CheckAimConditions();
            if (shooterManager && CurrentActiveWeapon)
            {
                UpdateIKAdjust(shooterManager.IsLeftWeapon);
                AlignArmToAimPosition(shooterManager.IsLeftWeapon);
                UpdateArmsIK(shooterManager.IsLeftWeapon);
            }
            UpdateAimPosition();
            OnFinishUpdateIK();
            UpdateAimHud();
            DoShots();
            CheckAimEvents();
        }

        protected virtual void CheckAimEvents()
        {
            if (IsAiming != wasAiming)
            {
                wasAiming = IsAiming;
                if (IsAiming) shooterManager.onEnableAim.Invoke(shooterManager.CurrentWeapon);
                else shooterManager.onDisableAim.Invoke(shooterManager.CurrentWeapon);
            }
            if (isUsingScopeView != wasAimingWithScope)
            {
                wasAimingWithScope = isUsingScopeView;
                if (IsAiming) shooterManager.onEnableScopeView.Invoke(shooterManager.CurrentWeapon);
                else shooterManager.onDisableScopeView.Invoke(shooterManager.CurrentWeapon);
            }
        }

        protected virtual void UpdateAimAngleReference()
        {
            aimAngleReference.transform.rotation = transform.rotation;
            UpdateCheckAimHelpers(shooterManager.IsLeftWeapon);
        }

        protected void UpdateCheckAimHelpers(bool isUsingLeftHand)
        {
            if (!CurrentActiveWeapon) return;
            var weight = (float)System.Math.Round(armAlignmentWeight, 1);
            var upperArm = isUsingLeftHand ? leftUpperArm : rightUpperArm;
            upperArmPosition = Vector3.Lerp(upperArmPosition, aimAngleReference.transform.InverseTransformPoint(upperArm.position), weight);
            muzzlePosition = Vector3.Lerp(muzzlePosition, aimAngleReference.transform.InverseTransformPoint(CurrentActiveWeapon.muzzle.position), weight);
            muzzleForward = Vector3.Lerp(muzzleForward, aimAngleReference.transform.InverseTransformDirection(CurrentActiveWeapon.muzzle.forward), weight);

        }

        public virtual void UpdateCheckAimPoints(ref Vector3 start, ref Vector3 end)
        {
            if (CurrentActiveWeapon)
            {
                float checkAimSmooth = shooterManager.checkAimOffsetSmooth;
                ///Lerp offsets 
                checkCanAimOffsetStartX = Mathf.Lerp(checkCanAimOffsetStartX, IsCrouching ? shooterManager.checkAimCrouchedOffsetStartX : shooterManager.checkAimStandingOffsetStartX, checkAimSmooth * Time.fixedDeltaTime);
                checkCanAimOffsetStartY = Mathf.Lerp(checkCanAimOffsetStartY, IsCrouching ? shooterManager.checkAimCrouchedOffsetStartY : shooterManager.checkAimStandingOffsetStartY, checkAimSmooth * Time.fixedDeltaTime);
                checkCanAimOffsetEndX = Mathf.Lerp(checkCanAimOffsetEndX, IsCrouching ? shooterManager.checkAimCrouchedOffsetEndX : shooterManager.checkAimStandingOffsetEndX, checkAimSmooth * Time.fixedDeltaTime);
                checkCanAimOffsetEndY = Mathf.Lerp(checkCanAimOffsetEndY, IsCrouching ? shooterManager.checkAimCrouchedOffsetEndY : shooterManager.checkAimStandingOffsetEndY, checkAimSmooth * Time.fixedDeltaTime);

                /// Make original points to check aim 
                Vector3 startPoint = aimAngleReference.transform.TransformPoint(upperArmPosition);
                float upperArmToMuzzleDistance = Vector3.Distance(startPoint, aimAngleReference.transform.TransformPoint(muzzlePosition));
                var endPoint = startPoint + (DesiredAimPosition - startPoint).normalized * upperArmToMuzzleDistance;
                Vector3 forward = (DesiredAimPosition - startPoint).normalized;
                ///Apply offsets              
                Vector3 newStartPoint = startPoint + cameraMain.transform.right * (checkCanAimOffsetStartX * (tpCamera.switchRight > 0 ? 1 : -1)) + cameraMain.transform.up * checkCanAimOffsetStartY;
                Vector3 newEndPoint = endPoint + cameraMain.transform.right * (checkCanAimOffsetEndX * (tpCamera.switchRight > 0 ? 1 : -1)) + cameraMain.transform.up * checkCanAimOffsetEndY + forward * shooterManager.checkAimOffsetZ;

                start = newStartPoint;
                end = newEndPoint;
            }
        }

        protected virtual void OnFinishUpdateIK()
        {
            onFinishUpdateIK?.Invoke();
        }

        protected virtual void OnStartUpdateIK()
        {
            onStartUpdateIK?.Invoke();
        }

        protected virtual void UpdateIKAdjust(bool isUsingLeftHand)
        {

            // create left arm ik solver if equal null
            if (LeftIK == null || !LeftIK.isValidBones)
            {
                LeftIK = new vIKSolver(animator, AvatarIKGoal.LeftHand);
                LeftIK.UpdateIK();
            }
            if (RightIK == null || !RightIK.isValidBones)
            {
                RightIK = new vIKSolver(animator, AvatarIKGoal.RightHand);
                RightIK.UpdateIK();
            }


            if (WeaponIKAdjustList == null) return;
            else
            {
                CurrentActiveWeapon.handIKTargetOffset.localPosition = isUsingLeftHand ? WeaponIKAdjustList.ikTargetPositionOffsetL : WeaponIKAdjustList.ikTargetPositionOffsetR;
                CurrentActiveWeapon.handIKTargetOffset.localEulerAngles = isUsingLeftHand ? WeaponIKAdjustList.ikTargetRotationOffsetL : WeaponIKAdjustList.ikTargetRotationOffsetR;
            }

            if (!CurrentWeaponIK || IsIgnoreIK)
            {
                LeftIK.UpdateIK();
                RightIK.UpdateIK();
                RightIK.SetIKWeight(0);
                LeftIK.SetIKWeight(0);
                weaponIKWeight = 0;
                return;
            }
            bool isValidIK = !cc.customAction && !isReloading && !isEquipping && CurrentWeaponIK != null && CurrentIKAdjust != null;
            weaponIKWeight = Mathf.Lerp(weaponIKWeight, isValidIK ? 1 : 0, shooterManager.ikAdjustSmooth);
            if (weaponIKWeight <= 0)
            {
                return;
            }

            if (isUsingLeftHand)
            {
                ApplyOffsets(LeftIK, RightIK, isValidIK);
            }
            else
            {
                ApplyOffsets(RightIK, LeftIK, isValidIK);
            }
        }

        protected virtual void ApplyOffsets(vIKSolver weaponHand, vIKSolver supportHand, bool isValidIK = true)
        {
            if (!weaponHand.isValidBones || !supportHand.isValidBones) return;
            //Apply Offset to Weapon Arm
            weaponHand.SetIKWeight(weaponIKWeight);
            ApplyOffsetToTargetBone(isValidIK ? CurrentIKAdjust.weaponHandOffset : null, weaponHand.endBoneOffset, isValidIK);
            ApplyOffsetToTargetBone(isValidIK ? CurrentIKAdjust.weaponHintOffset : null, weaponHand.middleBoneOffset, isValidIK);
            weaponHand.AnimationToIK();
            //Apply offset to Support Weapon Arm         
            ApplyOffsetToTargetBone(isValidIK ? CurrentIKAdjust.supportHandOffset : null, supportHand.endBoneOffset, !EditingIKGlobalOffset && isValidIK);
            ApplyOffsetToTargetBone(isValidIK ? CurrentIKAdjust.supportHintOffset : null, supportHand.middleBoneOffset, !EditingIKGlobalOffset && isValidIK);
        }

        protected virtual void ApplyOffsetToTargetBone(IKOffsetTransform iKOffset, Transform target, bool isValidIK)
        {
            try
            {
                target.localPosition = Vector3.Lerp(target.localPosition, isValidIK ? iKOffset.position : Vector3.zero, shooterManager.ikAdjustSmooth * vTime.fixedDeltaTime);
                target.localRotation = Quaternion.Lerp(target.localRotation, isValidIK ? Quaternion.Euler(iKOffset.eulerAngles) : Quaternion.Euler(Vector3.zero), shooterManager.ikAdjustSmooth * vTime.fixedDeltaTime);
            }
            catch
            {
                Debug.LogWarning("Can't Get IK Adjust");
            }
        }

        protected virtual void UpdateArmsIK(bool isUsingLeftHand = false)
        {
            // create left arm ik solver if equal null
            if (LeftIK == null || !LeftIK.isValidBones)
            {
                LeftIK = new vIKSolver(animator, AvatarIKGoal.LeftHand);
            }

            if (RightIK == null || !RightIK.isValidBones)
            {
                RightIK = new vIKSolver(animator, AvatarIKGoal.RightHand);
            }

            vIKSolver targetIK = null;

            if (isUsingLeftHand)
            {
                targetIK = RightIK;
            }
            else
            {
                targetIK = LeftIK;
            }
            bool useIK = isUsingLeftHand ? shooterManager.useLeftIK : shooterManager.useRightIK;
            if ((!shooterManager || !CurrentActiveWeapon || !useIK || IsIgnoreIK || isEquipping) ||
                (cc.IsAnimatorTag("Shot Fire") && CurrentActiveWeapon.disableIkOnShot))
            {
                if (supportIKWeight > 0)
                {
                    supportIKWeight = 0;
                    targetIK.SetIKWeight(0);
                }

                // Debug.Log($"Use ik {useIK.ToStringColor()}, IsIgnoreIK {IsIgnoreIK.ToStringColor()} "); // Debug IK conditions
                return;
            }

            bool useIkConditions = false;
            var animatorInput = System.Math.Round(cc.inputMagnitude, 1);
            if (!IsAiming && !isAttacking)
            {
                var locomotionValidation = cc.isStrafing ? CurrentActiveWeapon.strafeIKOptions : CurrentActiveWeapon.freeIKOptions;
                if (locomotionValidation.use)
                {
                    if (animatorInput <= 0.1f)
                    {
                        useIkConditions = locomotionValidation.useOnIdle;
                    }
                    else if (animatorInput <= 0.5f)
                    {
                        useIkConditions = locomotionValidation.useOnWalk;
                    }
                    else if (animatorInput <= 1f)
                    {
                        useIkConditions = locomotionValidation.useOnRun;
                    }
                    else if (animatorInput <= 1.5f)
                    {
                        useIkConditions = locomotionValidation.useOnSprint;
                    }
                }
                else useIkConditions = false;
            }
            else if (IsAiming && !isAttacking)
            {
                useIkConditions = shooterManager.isShooting ? !CurrentActiveWeapon.disableIkOnShot : CurrentActiveWeapon.useIKOnAiming;
            }
            else if (isAttacking)
            {
                useIkConditions = CurrentActiveWeapon.useIkAttacking;
            }


            IsSupportHandIKEnabled = useIkConditions && !IsIgnoreSupportHandIK;

            if (targetIK != null)
            {
                if (shooterManager.weaponIKAdjustList)
                {
                    if (isUsingLeftHand)
                    {
                        ikRotationOffset = shooterManager.weaponIKAdjustList.ikTargetRotationOffsetR;
                        ikPositionOffset = shooterManager.weaponIKAdjustList.ikTargetPositionOffsetR;
                    }
                    else
                    {
                        ikRotationOffset = shooterManager.weaponIKAdjustList.ikTargetRotationOffsetL;
                        ikPositionOffset = shooterManager.weaponIKAdjustList.ikTargetPositionOffsetL;
                    }
                }

                // Debug.Log($"using ik {useIkConditions.ToStringColor()}");//Debug IK Conditions
                // control weight of ik
                if (CurrentActiveWeapon && CurrentActiveWeapon.handIKTargetOffset && !isReloading && !cc.customAction && (cc.isGrounded || (IsAiming)) && useIkConditions)
                {
                    supportIKWeight = Mathf.Lerp(supportIKWeight, 1, shooterManager.armIKSmoothIn * vTime.fixedDeltaTime);
                }
                else
                {
                    supportIKWeight = Mathf.Lerp(supportIKWeight, 0, shooterManager.armIKSmoothOut * vTime.fixedDeltaTime);
                }

                if (supportIKWeight <= 0 || !IsSupportHandIKEnabled)
                {
                    if (!IsSupportHandIKEnabled && shooterManager.CurrentWeaponIK)
                    {
                        targetIK.SetIKWeight(shooterManager.armIKCurve.Evaluate(supportIKWeight));
                        targetIK.AnimationToIK();
                    }
                    return;
                }

                // update IK
                targetIK.SetIKWeight(shooterManager.armIKCurve.Evaluate(supportIKWeight));
                if (shooterManager && CurrentActiveWeapon && CurrentActiveWeapon.handIKTargetOffset)
                {

                    targetIK.SetIKPosition(CurrentActiveWeapon.handIKTargetOffset.position);
                    targetIK.SetIKRotation(CurrentActiveWeapon.handIKTargetOffset.rotation);


                    if (shooterManager.CurrentWeaponIK)
                    {
                        targetIK.AnimationToIK();
                    }
                }
            }
        }

        protected virtual bool CanRotateAimArm()
        {
            return cc.IsAnimatorTag("Upperbody Pose") && cc.animatorStateInfos.GetCurrentNormalizedTime(cc.upperBodyLayer) > 0.5f;
        }

        protected virtual bool CanDoShots()
        {
            return armAlignmentWeight >= 0.5f && cc.IsAnimatorTag("Upperbody Pose") && shootCountA > 0 && !isReloading;
        }

        protected virtual void AlignArmToAimPosition(bool isUsingLeftHand = false)
        {
            if (!shooterManager)
            {
                return;
            }
            ///Init the arms, if needs that
            if (leftArmAim == null) leftArmAim = new vArmAimAlign(leftUpperArm, leftLowerArm, leftHand);
            if (rightArmAim == null) rightArmAim = new vArmAimAlign(rightUpperArm, rightLowerArm, rightHand);
            /// Select the arm
            vArmAimAlign arm = isUsingLeftHand ? leftArmAim : rightArmAim;

            ///Calculate the Alignment weight based on aim conditions.
            armAlignmentWeight = IsAiming && aimConditions && CanRotateAimArm() ? Mathf.Lerp(armAlignmentWeight, Mathf.Clamp(cc.upperBodyInfo.normalizedTime, 0, 1f), shooterManager.smoothArmWeight * (.001f + Time.fixedDeltaTime)) : 0;

            if (CurrentActiveWeapon)
            {
                if (!shooterManager.isShooting)
                {
                    ///Update arm to use default animation rotation while not shooting
                    arm.UpdateDefaultAlignment();
                }
                else
                {
                    ///Use last alignment to ignore shoot animation rotation
                    ///When shooting the arm position and rotation will be changed by the animation
                    ///This method avoid this changes using the last alignment before the shot was performed
                    arm.RestoreToLastAlignment();
                }
                /// Set the Arm aligment values
                arm.smoothIKAlignmentPoint = shooterManager.smoothIKAlignmentPoint;
                arm.aimReference = CurrentActiveWeapon.aimReference;
                arm.smooth = shooterManager.smoothArmIKRotation;
                arm.maxVerticalAligmentAngle = shooterManager.maxVerticalAimAngle;
                arm.maxHorizontalAligmentAngle = shooterManager.maxHorizontalAimAngle;
                if (shooterManager.showCheckAimGizmos) arm.DrawBones(Color.blue);
                ///Align arm to target aim position              
                arm.AlignToArmToPosition(targetArmAlignmentPosition, armAlignmentWeight, CurrentActiveWeapon.alignRightUpperArmToAim, CurrentActiveWeapon.alignRightHandToAim);
                if (shooterManager.showCheckAimGizmos) arm.DrawHelpers(Color.green);
            }
        }

        protected virtual void CheckAimConditions()
        {
            if (!shooterManager)
            {
                return;
            }

            var weaponSide = (tpCamera.switchRight < 0 ? -1 : 1);

            if (CurrentActiveWeapon == null)
            {
                aimConditions = false;
                return;
            }

            if (shooterManager.useCheckAim == false)
            {
                aimConditions = true;
                return;
            }
            Vector3 startPoint = Vector3.zero;
            Vector3 endPoint = Vector3.zero;
            Ray _ray = new Ray(startPoint, ((endPoint) - startPoint).normalized);
            if (animator.IsInTransition(0))
            {
                if (Physics.SphereCast(_ray, shooterManager.checkAimRadius, out checkCanAimHit, (endPoint - startPoint).magnitude, shooterManager.blockAimLayer))
                {
                    aimConditions = false;
                }
                return;
            }


            UpdateCheckAimPoints(ref startPoint, ref endPoint);
            _ray = new Ray(startPoint, ((endPoint) - startPoint).normalized);
            if (Vector3.Distance(startPoint, AimPosition) < Vector3.Distance(startPoint, endPoint))
                aimConditions = false;


            if (Physics.SphereCast(_ray, shooterManager.checkAimRadius, out checkCanAimHit, (endPoint - startPoint).magnitude, shooterManager.blockAimLayer))
            {
                aimConditions = false;
            }
            else
            {
                aimConditions = true;
            }
        }

        protected virtual Vector3 targetArmAlignmentPosition
        {
            get
            {
                return shooterManager.alignArmToHitPoint || isUsingScopeView ? DesiredAimPosition : cameraMain.transform.position + cameraMain.transform.forward * 100;
            }
        }

        protected virtual Transform targetCamera
        {
            get
            {
                var t = controlAimCanvas && controlAimCanvas.isScopeCameraActive && controlAimCanvas.scopeBackgroundCamera ? controlAimCanvas.scopeBackgroundCamera.transform : cameraMain.transform;
                return t;
            }
        }

        protected virtual Vector3 targetArmAlignmentDirection
        {
            get
            {

                return targetCamera.forward;
            }
        }

        protected virtual void UpdateHeadTrack()
        {
            if (headTrack)
            {
                UpdateHeadTrackLookPoint();
            }
            if (!shooterManager || !headTrack)
            {
                if (headTrack)
                {
                    headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, Vector2.zero, weaponIKWeight);
                    headTrack.offsetHead = Vector2.Lerp(headTrack.offsetHead, Vector2.zero, weaponIKWeight);
                }
                return;
            }
            if (!CurrentActiveWeapon || !headTrack || !CurrentWeaponIK || CurrentIKAdjust == null)
            {
                if (headTrack)
                {
                    headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, Vector2.zero, weaponIKWeight);
                    headTrack.offsetHead = Vector2.Lerp(headTrack.offsetHead, Vector2.zero, weaponIKWeight);
                }
                return;
            }
            if (IsAiming)
            {
                var ikAdjust = CurrentIKAdjust;
                var offsetSpine = ikAdjust.spineOffset.spine;
                var offsetHead = ikAdjust.spineOffset.head;
                headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, offsetSpine, weaponIKWeight);
                headTrack.offsetHead = Vector2.Lerp(headTrack.offsetHead, offsetHead, weaponIKWeight);
            }
            else
            {
                var ikAdjust = CurrentIKAdjust;
                var offsetSpine = ikAdjust.spineOffset.spine;
                var offsetHead = ikAdjust.spineOffset.head;
                headTrack.offsetSpine = Vector2.Lerp(headTrack.offsetSpine, offsetSpine, headTrack.Smooth);
                headTrack.offsetHead = Vector2.Lerp(headTrack.offsetHead, offsetHead, headTrack.Smooth);
            }
        }

        protected virtual void UpdateHeadTrackLookPoint()
        {
            if (IsAiming /*&& !isUsingScopeView*/ && CurrentActiveWeapon && aimConditions)
            {
                headTrack.SetTemporaryLookPoint(targetArmAlignmentPosition); //cameraMain.transform.TransformPoint(Vector3.forward * (cameraMain.transform.InverseTransformPoint(CurrentActiveWeapon.muzzle.position).z + 10)), 0.1f);
            }
        }

        protected virtual void UpdateAimHud()
        {

            if (!shooterManager || !controlAimCanvas)
            {
                return;
            }

            if (CurrentActiveWeapon == null)
            {
                return;
            }

            var _aimPoint = AimPosition;

            controlAimCanvas.SetAimCanvasID(CurrentActiveWeapon.scopeID);
            if (controlAimCanvas.scopeBackgroundCamera && controlAimCanvas.scopeBackgroundCamera.gameObject.activeSelf)
            {

                controlAimCanvas.SetAimToCenter(true);

            }
            else if (IsAiming && aimConditions)
            {
                var aimDistance = Vector3.Distance(aimAngleReference.transform.position, _aimPoint);
                var validAim = (aimDistance > shooterManager.maxAimHitPointIndicator || Vector3.Distance(AimPosition, DesiredAimPosition) < 0.1f) && aimConditions;

                if (aimDistance > shooterManager.maxAimHitPointIndicator || Vector3.Distance(AimPosition, DesiredAimPosition) < 0.1f)
                    controlAimCanvas.SetAimToCenter(true);
                else
                    controlAimCanvas.SetWordPosition(_aimPoint, validAim);

            }
            else
            {
                controlAimCanvas.SetAimToCenter(aimConditions);
            }

            if ((controlAimCanvas.scopeBackgroundCamera && CurrentActiveWeapon.scopeTarget))
            {
                if (isUsingScopeView || controlAimCanvas.isScopeCameraActive)
                {
                    vArmAimAlign arm = shooterManager.IsLeftWeapon ? leftArmAim : rightArmAim;
                    if (!shooterManager.isShooting)
                    {
                        scopeLocalForward = arm.aimReferenceHelper.InverseTransformDirection(CurrentActiveWeapon.scopeTarget.forward);
                        scopeLocalPosition = arm.aimReferenceHelper.InverseTransformPoint(CurrentActiveWeapon.scopeTarget.position);

                        scopeCameraLocalForward = aimAngleReference.transform.InverseTransformDirection(controlAimCanvas.scopeBackgroundCamera.transform.forward);
                        scopeCameraLocalPosition = aimAngleReference.transform.InverseTransformPoint(controlAimCanvas.scopeBackgroundCamera.transform.position);
                    }
                    var weight = (shooterManager.isShooting ? (1 - CurrentActiveWeapon.scopeShootAnimationWeight) : 1f);

                    var scopCameraPosition = aimAngleReference.transform.TransformPoint(scopeCameraLocalPosition);
                    var scopCameraForward = aimAngleReference.transform.TransformDirection(scopeCameraLocalForward);
                    var scopeLookDirection = Vector3.Lerp(scopCameraForward, arm.aimReferenceHelper.TransformDirection(scopeLocalForward), scopeDirectionWeight = Mathf.Lerp(scopeDirectionWeight, 1.001f, (10f / CurrentActiveWeapon.shootFrequency) * Time.fixedDeltaTime));
                    var scopePosition = Vector3.Lerp(scopCameraPosition, arm.aimReferenceHelper.TransformPoint(scopeLocalPosition), weight);
                    controlAimCanvas.UpdateScopeCamera(scopePosition, scopeLookDirection, aimAngleReference.transform.up, CurrentActiveWeapon.backGroundScopeZoom, IsAiming);
                }
                else scopeDirectionWeight = 1f;
            }

        }

        #endregion

    }

    public static partial class vAnimatorParameters
    {
        public static int UpperBody_ID = Animator.StringToHash("UpperBody_ID");
        public static int CanAim = Animator.StringToHash("CanAim");
        public static int IsAiming = Animator.StringToHash("IsAiming");
        public static int IsHipFire = Animator.StringToHash("IsHipFire");
        public static int Shot_ID = Animator.StringToHash("Shot_ID");
        public static int PowerCharger = Animator.StringToHash("PowerCharger");
    }
}