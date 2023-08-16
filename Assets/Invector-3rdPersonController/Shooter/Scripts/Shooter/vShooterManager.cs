using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vShooter
{
    using vCharacterController;
    using vItemManager;

    [vClassHeader("SHOOTER MANAGER", iconName = "shooterIcon")]
    public class vShooterManager : vMonoBehaviour, IWeaponEquipmentListener
    {
        #region Variables

        [System.Serializable]
        public class WeaponEvent : UnityEngine.Events.UnityEvent<vShooterWeapon> { }
        /// <summary>
        /// Event called when equip or unequip a weapon, (Weapon, isLeftWeapon)
        /// </summary>
        [System.Serializable]
        public class EquipWeaponEvent : UnityEngine.Events.UnityEvent<vShooterWeapon,bool> { }
        public delegate void AmmoHandle(int ammoID, ref int ammo);

        [vEditorToolbar("Melee Overrides")]

        [vHelpBox("Behaviour when Shooter Weapon is Disabled (equipped but disabled)")]
        public bool canUseMeleeBlock_H = true;
        public bool canUseMeleeWeakAttack_H = true;
        public bool canUseMeleeStrongAttack_H = true;
        [vHelpBox("Behaviour when Shooter Weapon is Enabled (equipped and enabled)")]
        public bool canUseMeleeBlock_E = false;
        public bool canUseMeleeWeakAttack_E = true;
        public bool canUseMeleeStrongAttack_E = false;
        public bool canUseMeleeAiming = false;

        [vEditorToolbar("Damage Layers")]

        [Tooltip("Layer to aim and apply damage")]
        public LayerMask damageLayer = 1 << 0;
        [Tooltip("Tags to ignore (auto add this gameObject tag to avoid damage your self)")]
        public vTagMask ignoreTags = new vTagMask("Player");
        [Tooltip("Layer to block aim")]
        public LayerMask blockAimLayer = 1 << 0;

        [vEditorToolbar("Cancel Reload")]

        [vHelpBox("You can call the <b>CancelReload</b> method using events to interrupt the reload routine and animation, for example, when doing an Custom Action or receiving a specific hitReaction ID")]
        [Tooltip("It will always automatically use the CancelReload")]
        public bool useCancelReload = true;
        [Tooltip("This is a list of HitReaction ID that will be ignored by the CancelReload routine")]
        public List<int> ignoreReacionIDList = new List<int>() { -1 };

        [vEditorToolbar("Aim")]
        [Tooltip("The min distance that Hit point can be, the min distance will be used to indicate the target point in the crosshair")]
        public float maxAimHitPointIndicator=30f;
        [Tooltip("The min distance that Hit point can be, the min distance will be used to calculate the point using camera position and camera forward relative to muzzle position")]
        public float minAimHitPointDistance;
        [Tooltip("If the Aim Hit point is behind weapon muzzle, the aim point will be the default point")]
        public bool ignoreBackAimPoint = true;
        [Tooltip("Offset for check if aim point is behind weapon muzzle")]
        public float backAimPointOffset = 0;

        [vSeparator("Float Values")]
        public bool useCheckAim = true;
        public float checkAimRadius = 0.1f;
        public float checkAimOffsetZ = 0;
        public float checkAimOffsetSmooth = 2f;

        [vSeparator("Standing")]
        public float checkAimStandingOffsetStartY = 0;
        public float checkAimStandingOffsetStartX = 0.2f;
        public float checkAimStandingOffsetEndY = 0;
        public float checkAimStandingOffsetEndX = 0f;

        [vSeparator("Crouching")]
        public float checkAimCrouchedOffsetStartY = 0;
        public float checkAimCrouchedOffsetStartX = 0.2f;
        public float checkAimCrouchedOffsetEndY = 0;
        public float checkAimCrouchedOffsetEndX = 0;

        [vSeparator("Shooter Settings")]

        [Tooltip("The Aim stays active when reload, including animator parameter and camera state")]
        public bool keepAimingWhenReload;
        [Tooltip("Check true to make the character always aim and walk on strafe mode")]
        public bool alwaysAiming;
        public bool onlyWalkWhenAiming = true;
        public bool useDefaultMovesetWhenNotAiming = true;

        [vEditorToolbar("IK Adjust")]

        public float armIKSmoothIn = 10, armIKSmoothOut = 25f;
        public AnimationCurve armIKCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        [Tooltip("Control the speed of the Animator Layer OnlyArms Weight")]
        public float onlyArmsSpeed = 25f;
        [Tooltip("Alignment smooth method\nIf Yes. The smooth will be applied to the target alignment point (Aim Position) of the Arm \nIf NOT. the smooth will be applied to the additive rotation  based on target alignment point (Aim Position)")]
        public bool smoothIKAlignmentPoint = false;
        [Tooltip("smooth of the right hand when correcting the aim")]
        public float smoothArmIKRotation = 30f;
        [Tooltip("Limit the maxAngle for the arm Alignment")]
        public float maxVerticalAimAngle = 60f;
        [Tooltip("Limit the maxAngle for the arm Alignment")]
        public float maxHorizontalAimAngle = 20f;
        [Tooltip("smooth of the right arm when correcting the aim")]
        public float smoothArmWeight = 24f;
        [Tooltip("Sync the weapon aim to the camera aim")]
        public bool raycastAimTarget = true;
        [Tooltip("rotate arm ik to aim hit point, if false the arms will rotate to  camera forward distance 100")]
        public bool alignArmToHitPoint = true;     
        [Tooltip("Check this to use IK on the left hand")]
        public bool useLeftIK = true, useRightIK = true;
        [vSeparator("--- Start PlayMode to edit the IK Adjust ---")]
        public vWeaponIKAdjustList weaponIKAdjustList;
        public float ikAdjustSmooth = 20;
        [vEditorToolbar("Ammo")]
        [SerializeField] protected bool allAmmoInfinity;
        [Tooltip("Use the vAmmoDisplay to shot ammo count")]
        public bool useAmmoDisplay = true;
        [Tooltip("ID to find ammoDisplay for leftWeapon")]
        public int leftWeaponAmmoDisplayID = -1;
        [Tooltip("ID to find ammoDisplay for rightWeapon")]
        public int rightWeaponAmmoDisplayID = 1;

        [vEditorToolbar("Recoil")]

        [Tooltip("Move camera angle when shot using recoil properties of weapon")]
        public bool applyRecoilToCamera = true;
        [Tooltip("The camera recoil stability"), Range(0, 1)]
        public float cameraRecoilStability = 0f;

        [vEditorToolbar("LockOn")]

        [vSeparator("LockOn (need the shooter lockon component)")]
        [Tooltip("Allow the use of the LockOn or not")]
        public bool useLockOn = false;
        [Tooltip("Allow the use of the LockOn only with a Melee Weapon")]
        public bool useLockOnMeleeOnly = true;

        [vEditorToolbar("HipFire")]

        [vSeparator("HipFire Options")]
        [Tooltip("If enable, remember to change your weak attack input to other input - this allows shot without aim")]
        public bool hipfireShot = false;
        [Tooltip("Precision of the weapon when shooting using hipfire (without aiming)")]
        public float hipfireDispersion = 0.5f;
        [Tooltip("Time to keep aiming after shot")]
        [SerializeField] public float hipfireAimTime = 2f;
        [vEditorToolbar("Camera Sway")]
        [vSeparator("Camera Sway Settings")]
        [Tooltip("Camera Sway movement while aiming")]
        public float cameraMaxSwayAmount = 2f;
        [Tooltip("Camera Sway Speed while aiming")]
        public float cameraSwaySpeed = .5f;

        [vEditorToolbar("Weapons")]

        [SerializeField] protected vShooterWeapon _rWeapon, _lWeapon;
        public virtual vShooterWeapon rWeapon { get { return _rWeapon; } set { _rWeapon = value; } }
        public virtual vShooterWeapon lWeapon { get { return _lWeapon; } set { _lWeapon = value; } }
        public int reloadAnimatorLayer = 4;
        public WeaponEvent onEnableAim;
        public WeaponEvent onDisableAim;
        public WeaponEvent onEnableScopeView;
        public WeaponEvent onDisableScopeView;
        public WeaponEvent onShot;
        public WeaponEvent onEmptyClipShot;
        public WeaponEvent onFinishAmmo;
        public WeaponEvent onStartReloadWeapon;
        public WeaponEvent onFinishReloadWeapon;
        public EquipWeaponEvent onEquipWeapon;
        public EquipWeaponEvent onUnequipWeapon;
        public virtual vAmmoManager ammoManager { get; set; }
        public virtual vAmmoDisplay ammoDisplayR { get; set; }
        public virtual vAmmoDisplay ammoDisplayL { get; set; }
        public virtual AmmoHandle ammoHandle { get; set; }
        public virtual vCamera.vThirdPersonCamera tpCamera { get; set; }
        public virtual bool isReloadingWeapon { get; set; }

        protected virtual vWeaponIKAdjust currentWeaponIKAdjust { get; set; }
        protected virtual Animator animator { get; set; }
        protected virtual bool usingThirdPersonController { get; set; }
        protected virtual bool cancelReload { get; set; }
        protected virtual bool isReloading { get; set; }
        protected virtual float hipfirePrecisionAngle { get; set; }
        protected virtual float hipfirePrecision { get; set; }
        protected virtual float reloadStartTime { get; set; }

        /// <summary>
        /// The Shot layer of the animation
        /// </summary>
        public virtual int ShotLayer { get; protected set; }
        /// <summary>
        /// Animator Hash for IsShoot parameter 
        /// </summary>
        internal readonly int IsShoot = Animator.StringToHash("Shoot");
        /// <summary>
        /// Animator Hash for Reload parameter 
        /// </summary>
        internal readonly int Reload = Animator.StringToHash("Reload");
        /// <summary>
        /// Animator Hash for ReloadID parameter 
        /// </summary>
        internal readonly int ReloadID = Animator.StringToHash("ReloadID");

        protected int extraAmmo;

        public virtual int ExtraAmmo => extraAmmo;      
      
        [vEditorToolbar("Debug")]
        public bool showCheckAimGizmos;

        #endregion

        public virtual void Start()
        {
            animator = GetComponent<Animator>();
            if (applyRecoilToCamera)
            {
                tpCamera = FindObjectOfType<vCamera.vThirdPersonCamera>();
            }
            ammoManager = GetComponent<vAmmoManager>();
            if (ammoManager != null)
            {
                ammoManager.updateTotalAmmo = new vAmmoManager.OnUpdateTotalAmmo(AmmoManagerWasUpdated);
            }

            var tpInput = GetComponent<vThirdPersonController>();
            usingThirdPersonController = tpInput;

            if (usingThirdPersonController && useCancelReload)
            {
                tpInput.onReceiveDamage.AddListener(CancelReload);
            }

            if (useAmmoDisplay)
            {
                GetAmmoDisplays();
            }

            if (animator)
            {
                var _rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
                var _lefttHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                var weaponR = _rightHand.GetComponentInChildren<vShooterWeapon>();
                var weaponL = _lefttHand.GetComponentInChildren<vShooterWeapon>();
                ShotLayer = animator.GetLayerIndex("Shot");
                if (weaponR != null)
                {
                    SetRightWeapon(weaponR.gameObject);
                }

                if (weaponL != null)
                {
                    SetLeftWeapon(weaponL.gameObject);
                }
            }

            if (!ignoreTags.Contains(gameObject.tag))
            {
                ignoreTags.Add(gameObject.tag);
            }

            if (useAmmoDisplay)
            {
                if (ammoDisplayR)
                {
                    ammoDisplayR.UpdateDisplay("");
                }

                if (ammoDisplayL)
                {
                    ammoDisplayL.UpdateDisplay("");
                }
            }
            UpdateTotalAmmo();
        }

        public virtual bool AllAmmoInfinity
        {
            get
            {
                return allAmmoInfinity;
            }
            set
            {
                allAmmoInfinity = value;
            }
        }

        public virtual void SetLeftWeapon(GameObject weapon)
        {
            if (weapon != null)
            {
                var w = weapon.GetComponent<vShooterWeapon>();
                SetLeftWeapon(w);
            }
            else
            {
                if (lWeapon) onUnequipWeapon.Invoke(lWeapon, true);
                lWeapon = null;
            }
        }

        protected virtual void SetLeftWeapon(vShooterWeapon weapon)
        {
         
            if (weapon)
            {
                lWeapon = weapon;
                lWeapon.IsEquipped = true;
                lWeapon.ignoreTags = ignoreTags;
                lWeapon.hitLayer = damageLayer;
                lWeapon.root = transform;
                lWeapon.onDisable.RemoveListener(HideLeftAmmoDisplay);
                lWeapon.onDisable.AddListener(HideLeftAmmoDisplay);
                lWeapon.onDestroy.RemoveListener(OnDestroyWeapon);
                lWeapon.onDestroy.AddListener(OnDestroyWeapon);

                CollectExtraAmmo(weapon);

                if (lWeapon.dontUseReload)
                {
                    LoadAllAmmo(lWeapon);
                }

                if (usingThirdPersonController)
                {
                    if (useAmmoDisplay && !ammoDisplayL)
                    {
                        GetAmmoDisplays();
                    }

                    if (useAmmoDisplay && ammoDisplayL)
                    {
                        ammoDisplayL.Show();
                    }

                    UpdateLeftAmmo();
                }

                UpdateWeaponIK();
                onEquipWeapon.Invoke(weapon, true);
            }
            else
            {
                if (lWeapon) onUnequipWeapon.Invoke(lWeapon, true);
                lWeapon = null;
            }
        }

        public virtual void SetRightWeapon(GameObject weapon)
        {
            if (weapon != null)
            {
                var w = weapon.GetComponent<vShooterWeapon>();
                SetRightWeapon(w);
             
            }
            else
            {
                if(rWeapon) onUnequipWeapon.Invoke(rWeapon, false);
                rWeapon = null;
            }
        }

        protected virtual void SetRightWeapon(vShooterWeapon weapon)
        {
          
            if (weapon)
            {
                rWeapon = weapon;
                rWeapon.IsEquipped = true;
                rWeapon.ignoreTags = ignoreTags;
                rWeapon.hitLayer = damageLayer;
                rWeapon.root = transform;
                rWeapon.onDisable.RemoveListener(HideRightAmmoDisplay);
                rWeapon.onDisable.AddListener(HideRightAmmoDisplay);
                rWeapon.onDestroy.RemoveListener(OnDestroyWeapon);
                rWeapon.onDestroy.AddListener(OnDestroyWeapon);
                if (rWeapon.dontUseReload)
                {
                    LoadAllAmmo(rWeapon);
                }

                CollectExtraAmmo(weapon);

                if (usingThirdPersonController)
                {
                    if (useAmmoDisplay && !ammoDisplayR)
                    {
                        GetAmmoDisplays();
                    }

                    if (useAmmoDisplay && ammoDisplayR)
                    {
                        ammoDisplayR.Show();
                    }

                    UpdateRightAmmo();
                }

                UpdateWeaponIK();
                onEquipWeapon.Invoke(weapon, false);
            }
            else
            {                
                if (rWeapon) onUnequipWeapon.Invoke(rWeapon, false);
                rWeapon = null;
                
            }
        }

        protected virtual void CollectExtraAmmo(vShooterWeapon weapon)
        {
            if (weapon.ammoCount > weapon.clipSize)
            {
                var ammocount = weapon.ammo - weapon.clipSize;
                weapon.ammo = weapon.ammo - ammocount;
                if (ammoManager)
                {
                    ammoManager.AddAmmo(weapon.ammoID, ammocount);
                }
            }
        }

        protected virtual void HideLeftAmmoDisplay()
        {
            HideAmmoDisplay(ammoDisplayL);
        }

        protected virtual void HideRightAmmoDisplay()
        {
            HideAmmoDisplay(ammoDisplayR);
        }

        protected virtual void HideAmmoDisplay(vAmmoDisplay ammoDisplay)
        {
            if (useAmmoDisplay && ammoDisplay)
            {
                ammoDisplay.UpdateDisplay("");
                ammoDisplay.Hide();
            }
        }

        public virtual void OnDestroyWeapon(GameObject otherGameObject)
        {
            if (usingThirdPersonController)
            {
                var ammoDisplay = rWeapon != null && otherGameObject == rWeapon.gameObject ?
                    ammoDisplayR : lWeapon != null && otherGameObject == lWeapon.gameObject ? ammoDisplayL : null;

                HideAmmoDisplay(ammoDisplay);
            }

        }

        protected virtual void GetAmmoDisplays()
        {
            var ammoDisplays = FindObjectsOfType<vAmmoDisplay>();
            if (ammoDisplays.Length > 0)
            {
                if (!ammoDisplayL)
                {
                    ammoDisplayL = ammoDisplays.vToList().Find(d => d.displayID == leftWeaponAmmoDisplayID);
                }

                if (!ammoDisplayR)
                {
                    ammoDisplayR = ammoDisplays.vToList().Find(d => d.displayID == rightWeaponAmmoDisplayID);
                }
            }
        }

        public virtual int GetMoveSetID()
        {
            int id = 0;

            if (rWeapon && rWeapon.gameObject.activeInHierarchy)
            {
                id = (int)rWeapon.moveSetID;
            }
            else if (lWeapon && lWeapon.gameObject.activeInHierarchy)
            {
                id = (int)lWeapon.moveSetID;
            }

            return id;
        }

        public virtual int GetUpperBodyID()
        {
            int id = 0;

            if (rWeapon && rWeapon.gameObject.activeInHierarchy)
            {
                id = (int)rWeapon.upperBodyID;
            }
            else if (lWeapon && lWeapon.gameObject.activeInHierarchy)
            {
                id = (int)lWeapon.upperBodyID;
            }

            return id;
        }

        public virtual int GetShotID()
        {
            int id = 0;

            if (rWeapon && rWeapon.gameObject.activeInHierarchy)
            {
                id = (int)rWeapon.shotID;
            }
            else if (lWeapon && lWeapon.gameObject.activeInHierarchy)
            {
                id = (int)lWeapon.shotID;
            }

            return id;
        }

        public virtual int GetEquipID()
        {
            int id = 0;
            if (rWeapon && rWeapon.gameObject.activeInHierarchy)
            {
                id = rWeapon.equipID;
            }
            else if (lWeapon && lWeapon.gameObject.activeInHierarchy)
            {
                id = lWeapon.equipID;
            }

            return id;
        }

        public virtual int GetReloadID()
        {
            int id = 0;
            if (rWeapon && rWeapon.gameObject.activeInHierarchy)
            {
                id = rWeapon.reloadID;
            }
            else if (lWeapon && lWeapon.gameObject.activeInHierarchy)
            {
                id = lWeapon.reloadID;
            }

            return id;
        }

        public virtual float HipfireAimTime
        {
            get
            {
                return hipfireAimTime + (CurrentWeapon ? CurrentWeapon.shootFrequency : 0);

            }
        }

        public virtual bool WeaponHasLoadedAmmo()
        {
            var hasAmmo = (CurrentWeapon ? CurrentWeapon.ammoCount : 0) > 0;
            return hasAmmo;
        }

        public virtual bool WeaponHasUnloadedAmmo()
        {
            var hasAmmo = extraAmmo > 0;
            return hasAmmo;
        }

        public virtual bool isShooting
        {
            get { return CurrentWeapon && !CurrentWeapon.CanDoShot; }
        }

        public virtual bool isShootingEmptyClip
        {
            get { return CurrentWeapon && !CurrentWeapon.CanDoEmptyClip; }
        }

        public virtual void ReloadWeapon()
        {
            var weapon = rWeapon ? rWeapon : lWeapon;

            if (!weapon || !weapon.gameObject.activeInHierarchy || isReloading)
            {
                return;
            }

            UpdateTotalAmmo();

            if (weapon.ammoCount < weapon.clipSize && ((weapon.isInfinityAmmo || AllAmmoInfinity) || WeaponHasUnloadedAmmo()) && !weapon.dontUseReload)
            {
                onStartReloadWeapon.Invoke(weapon);

                if (animator)
                {
                    animator.SetInteger(ReloadID, GetReloadID());
                    animator.SetTrigger(Reload);
                }
                if (CurrentWeapon && CurrentWeapon.gameObject.activeInHierarchy)
                {
                    StartCoroutine(AddAmmoToWeapon(CurrentWeapon, CurrentWeapon.reloadTime));
                }
            }
        }

        protected virtual IEnumerator AddAmmoToWeapon(vShooterWeapon weapon, float delayTime)
        {
            isReloading = true;
            isReloadingWeapon = true;
            reloadStartTime = Time.time;
            if (weapon.ammoCount < weapon.clipSize && ((weapon.isInfinityAmmo || AllAmmoInfinity) || WeaponHasUnloadedAmmo()) && !weapon.dontUseReload && !cancelReload)
            {
                weapon.ReloadEffect();
                yield return new WaitForSeconds(delayTime);

                if (!cancelReload)
                {
                    var needAmmo = weapon.reloadOneByOne ? 1 : weapon.clipSize - weapon.ammoCount;

                    if (!weapon.reloadOneByOne && (weapon.isInfinityAmmo || AllAmmoInfinity))
                    {
                        weapon.AddAmmo(needAmmo);
                    }
                    else
                    {
                        if (!(weapon.isInfinityAmmo || AllAmmoInfinity)&& WeaponAmmo(weapon).count < needAmmo)
                        {
                            needAmmo = WeaponAmmo(weapon).count;
                        }

                        weapon.AddAmmo(needAmmo);
                        WeaponAmmo(weapon).Use(needAmmo);
                    }

                    if (weapon.reloadOneByOne && weapon.ammoCount < weapon.clipSize && WeaponHasUnloadedAmmo())
                    {
                        if (weapon.isInfinityAmmo == false && WeaponAmmo(weapon).count == 0)
                        {
                            weapon.FinishReloadEffect();
                            isReloadingWeapon = false;
                            onFinishReloadWeapon.Invoke(weapon);
                        }
                        else
                        {

                            isReloadingWeapon = true;

                            if (!cancelReload)
                            {
                                animator.SetInteger(ReloadID, weapon.reloadID);
                                animator.SetTrigger(Reload);
                                StartCoroutine(AddAmmoToWeapon(weapon, delayTime));
                            }
                        }
                    }
                    else
                    {
                        weapon.FinishReloadEffect();
                        isReloadingWeapon = false;
                        onFinishReloadWeapon.Invoke(weapon);
                    }
                }
                UpdateTotalAmmo();
            }
            isReloading = false;
        }

        public virtual void CancelReload()
        {
            if (isReloading)
            {
                StartCoroutine(CancelReloadRoutine());
            }
        }

        public virtual void CancelReload(vDamage damage)
        {
            if (!ignoreReacionIDList.Contains(damage.reaction_id) && isReloading)
            {
                StartCoroutine(CancelReloadRoutine());
            }
        }

        protected virtual IEnumerator CancelReloadRoutine()
        {
            if (CurrentWeapon != null /*&& (Time.time - reloadStartTime) >= Mathf.Min(0.5f, CurrentWeapon.reloadTime * 0.5f)*/)
            {
                animator.SetTrigger("CancelReload");
                animator.ResetTrigger("Reload");
                cancelReload = true;
                StopCoroutine("AddAmmoToWeapon");
                if (CurrentWeapon)
                {
                    CurrentWeapon.CancelReload();
                }
                yield return new WaitForSeconds(CurrentWeapon.reloadTime + 0.1f);
                cancelReload = false;
                if (isReloadingWeapon)
                {
                    isReloadingWeapon = false;
                    if (CurrentWeapon)
                    {
                        onFinishReloadWeapon.Invoke(CurrentWeapon);
                    }
                }
                animator.ResetTrigger("CancelReload");
                UpdateTotalAmmo();
            }
        }

        public virtual void LoadAllAmmo(vShooterWeapon weapon)
        {
            if (!weapon)
            {
                return;
            }

            UpdateTotalAmmo();
            if (weapon.ammoCount < weapon.clipSize && ((weapon.isInfinityAmmo || AllAmmoInfinity) || WeaponHasUnloadedAmmo()))
            {
                var needAmmo = weapon.clipSize - weapon.ammoCount;
                if ((weapon.isInfinityAmmo || AllAmmoInfinity))
                {
                    weapon.AddAmmo(needAmmo);
                }
                else
                {
                    if (WeaponAmmo(weapon).count < needAmmo)
                    {
                        needAmmo = WeaponAmmo(weapon).count;
                    }

                    weapon.AddAmmo(needAmmo);
                    WeaponAmmo(weapon).Use(needAmmo);
                }
                weapon.onReload.Invoke();
            }
        }

        public virtual vAmmo WeaponAmmo(vShooterWeapon weapon)
        {
            if (!weapon)
            {
                return null;
            }

            var ammo = new vAmmo();
            if (ammoManager && ammoManager.ammos != null && ammoManager.ammos.Count > 0)
            {
                ammo = ammoManager.GetAmmo(weapon.ammoID);
            }
            return ammo;
        }

        public virtual vShooterWeapon CurrentActiveWeapon
        {
            get
            {
                var weapon = CurrentWeapon;
                if (weapon && weapon.IsEquipped) return weapon;
                return null;
            }
        }

        public virtual vShooterWeapon CurrentWeapon
        {
            get
            {
                var _weapon = rWeapon ?
                    rWeapon :
                    lWeapon ?
                    lWeapon : null;
                return _weapon != null ? _weapon : null;
            }
        }

        public virtual void SetIKAdjustList(vWeaponIKAdjustList weaponIKAdjustList)
        {
            this.weaponIKAdjustList = weaponIKAdjustList;
            if (CurrentWeapon)
            {
                currentWeaponIKAdjust = weaponIKAdjustList.GetWeaponIK(CurrentWeapon.weaponCategory);
            }
        }

        public virtual vWeaponIKAdjust CurrentWeaponIK
        {
            get
            {
                return currentWeaponIKAdjust;
            }
        }

        public virtual void UpdateWeaponIK()
        {
            if (weaponIKAdjustList && CurrentWeapon)
            {
                currentWeaponIKAdjust = weaponIKAdjustList.GetWeaponIK(CurrentWeapon.weaponCategory);
            }
        }

        public virtual bool IsLeftWeapon
        {
            get
            {
                var isLeftWp = (rWeapon == null) ?
                    (lWeapon) : rWeapon.isLeftWeapon;
                return isLeftWp;
            }
        }

        public virtual void AmmoManagerWasUpdated()
        {
            bool needUpdateAmmo = true;
            if (CurrentWeapon)
            {
                if (CurrentWeapon.dontUseReload)
                {
                    LoadAllAmmo(CurrentWeapon);
                    needUpdateAmmo = false;
                }

            }
            if (needUpdateAmmo)
            {
                UpdateTotalAmmo();
            }
        }

        public virtual void UpdateTotalAmmo()
        {
            UpdateLeftAmmo();
            UpdateRightAmmo();
        }

        public virtual void UpdateLeftAmmo()
        {
            if (!lWeapon)
            {
                return;
            }

            UpdateTotalAmmo(lWeapon, ref extraAmmo, -1);
        }

        public virtual void UpdateRightAmmo()
        {
            if (!rWeapon)
            {
                return;
            }

            UpdateTotalAmmo(rWeapon, ref extraAmmo, 1);
        }

        protected virtual void UpdateTotalAmmo(vShooterWeapon weapon, ref int targetTotalAmmo, int displayId)
        {
            if (!weapon)
            {
                return;
            }

            var ammoCount = 0;
            if ((weapon.isInfinityAmmo || AllAmmoInfinity))
            {
                ammoCount = 9999;
            }
            else
            {
                var ammo = WeaponAmmo(weapon);
                if (ammo != null)
                {
                    ammoCount += ammo.count;
                }
            }
            targetTotalAmmo = ammoCount;
            UpdateAmmoDisplay(displayId);
        }

        protected virtual void UpdateAmmoDisplay(int displayId)
        {
            if (!useAmmoDisplay)
            {
                return;
            }

            var weapon = displayId == 1 ? rWeapon : lWeapon;
            if (!ammoDisplayR || !ammoDisplayL)
            {
                GetAmmoDisplays();
            }

            var ammoDisplay = displayId == 1 ? ammoDisplayR : ammoDisplayL;

            if (useAmmoDisplay && ammoDisplay)
            {
                string textA = weapon.dontUseReload ? (weapon.isInfinityAmmo || AllAmmoInfinity) ? "∞" : (weapon.ammoCount + extraAmmo).ToString("00") : weapon.ammoCount.ToString("00"); ;
                string textB = weapon.dontUseReload && (weapon.isInfinityAmmo || AllAmmoInfinity) ? "" : !weapon.dontUseReload && (weapon.isInfinityAmmo || AllAmmoInfinity) ? "∞" : weapon.dontUseReload && !(weapon.isInfinityAmmo && AllAmmoInfinity) ? "" : (extraAmmo).ToString("00");
                ammoDisplay.UpdateDisplay(textA, textB, weapon.ammoID);
            }
        }

        public virtual void Shoot(Vector3 aimPosition, bool applyHipfirePrecision = false, bool scopeViewMode = false)
        {
            if (isShooting)
            {
                return;
            }

            var weapon = rWeapon ? rWeapon : lWeapon;
            if (!weapon || !weapon.gameObject.activeInHierarchy)
            {
                return;
            }

            if (weapon.dontUseReload)
            {
                LoadAllAmmo(weapon);
            }
            else if (weapon.autoReload && weapon.ammoCount <= 0 && WeaponHasUnloadedAmmo())
            {
                ReloadWeapon();
                return;
            }

            var _aimPos = applyHipfirePrecision ? aimPosition + HipFirePrecision(aimPosition) : aimPosition;
            var sucessfulShot = false;
            weapon.Shoot(weapon.muzzle.position, _aimPos, transform, (bool sucessful) => { sucessfulShot = sucessful; });

            if (sucessfulShot)
            {
                ApplyRecoil();
                onShot.Invoke(weapon);
            }
            else
            {
                onEmptyClipShot.Invoke(weapon);
            }

            UpdateAmmoDisplay(rWeapon ? 1 : -1);

            if (weapon.dontUseReload)
            {
                LoadAllAmmo(weapon);
            }

            if (extraAmmo <= 0)
            {
                onFinishAmmo.Invoke(weapon);
                weapon.onFinishAmmo.Invoke();
            }
        }

        protected virtual Vector3 HipFirePrecision(Vector3 _aimPosition)
        {
            var weapon = rWeapon ? rWeapon : lWeapon;
            if (!weapon)
            {
                return Vector3.zero;
            }

            hipfirePrecisionAngle = UnityEngine.Random.Range(-1000, 1000);
            hipfirePrecision = Random.Range(-hipfireDispersion, hipfireDispersion);
            var dir = (Quaternion.AngleAxis(hipfirePrecisionAngle, _aimPosition - weapon.muzzle.position) * (Vector3.up)).normalized * hipfirePrecision;
            return dir;
        }

        public virtual void CameraSway()
        {
            var weapon = rWeapon ? rWeapon : lWeapon;
            if (!weapon)
            {
                return;
            }

            float bx = (Mathf.PerlinNoise(0, Time.time * cameraSwaySpeed) - 0.5f);
            float by = (Mathf.PerlinNoise(0, (Time.time * cameraSwaySpeed) + 100)) - 0.5f;

            var swayAmount = cameraMaxSwayAmount * (1f - weapon.cameraStability);
            if (swayAmount == 0)
            {
                return;
            }

            bx *= swayAmount;
            by *= swayAmount;

            float tx = (Mathf.PerlinNoise(0, Time.time * cameraSwaySpeed) - 0.5f);
            float ty = ((Mathf.PerlinNoise(0, (Time.time * cameraSwaySpeed) + 100)) - 0.5f);

            tx *= -(swayAmount * 0.25f);
            ty *= (swayAmount * 0.25f);

            if (tpCamera != null)
            {
                tpCamera.offsetMouse.x = bx + tx;
                tpCamera.offsetMouse.y = by + ty;
            }
        }


        #region Recoil        

        protected Vector3 targetPosition, currentPosition;
        protected Vector3 currentRotation, targetRotation;
        protected float targetSpinekickBack, currentspinekickBack;
        protected virtual float cameraRecoilPower => (1f - cameraRecoilStability);

        /// <summary>
        /// Weapon recoil offset position 
        /// </summary>
        public virtual Vector3 recoilPositionOffset { get; protected set; }
        /// <summary>
        /// Weapon recoil offset angle
        /// </summary>
        public virtual Vector3 recoilRotationOffset { get; protected set; }
        /// <summary>
        /// Spine recoil offset angle
        /// </summary>
        public virtual float recoilSpineVerticalOffset { get; protected set; }

        /// <summary>
        /// Set the target recoil values values
        /// </summary>
        public virtual void ApplyRecoil()
        {
            ApplyAnimationRecoil();
            ApplyCameraRecoil();
        }

        protected virtual void ApplyCameraRecoil()
        {
            if (!applyRecoilToCamera || CurrentWeapon == null || tpCamera == null) return;
            var cameraRecoilHorizontal = Random.Range(CurrentWeapon.recoilLeft, CurrentWeapon.recoilRight) * cameraRecoilPower;
            var cameraRecoilUp = Random.Range(0, CurrentWeapon.recoilUp) * cameraRecoilPower;

            if (tpCamera != null)
            {
                tpCamera.RotateCamera(cameraRecoilHorizontal, cameraRecoilUp);
            }
        }

        protected virtual void ApplyAnimationRecoil()
        {
            if (animator)
            {
                animator.SetTrigger(IsShoot);
            }
        }

        #endregion
    }
}