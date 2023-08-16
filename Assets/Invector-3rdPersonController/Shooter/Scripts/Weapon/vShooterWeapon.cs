using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Invector.vShooter
{
    public static class IKLocomotionOptionsHelper
    {
        public static vShooterWeapon.IKLocomotionOptions Copy(this vShooterWeapon.IKLocomotionOptions options)
        {
            vShooterWeapon.IKLocomotionOptions copy = new vShooterWeapon.IKLocomotionOptions()
            {
                use = options.use,
                useOnIdle = options.useOnIdle,
                useOnWalk = options.useOnWalk,
                useOnRun = options.useOnRun,
                useOnSprint = options.useOnSprint,
            };
            return copy;
        }
    }
    [vClassHeader("Shooter Weapon", openClose = false)]
    public class vShooterWeapon : vShooterWeaponBase
    {
        public enum AutoReloadStyle
        {            
            WhenAiming,WhenShot,WhenAmmoAvailable
        }
        #region variables
        [System.Serializable]
        public class IKLocomotionOptions
        {
            public bool use = true;
            [vHideInInspector("use")]
            public bool useOnIdle = true;
            [vHideInInspector("use")]
            public bool useOnWalk = true;
            [vHideInInspector("use")]
            public bool useOnRun = true;
            [vHideInInspector("use")]
            public bool useOnSprint = true;

            public IKLocomotionOptions()
            {
                use = true;
                useOnIdle = true;
                useOnWalk = true;
                useOnRun = true;
                useOnSprint = true;
            }

        }

        [vEditorToolbar("Weapon Settings")]

        public bool isLeftWeapon = false;
        [Tooltip("Hold Charge Input to charge")]
        public bool chargeWeapon = false;
        [vHideInInspector("chargeWeapon")]
        public bool autoShotOnFinishCharge = false;
        [vHideInInspector("chargeWeapon")]
        public float chargeSpeed = 0.1f;
        [vHideInInspector("chargeWeapon")]
        public float chargeDamageMultiplier = 2;
        [vHideInInspector("chargeWeapon")]
        public bool changeVelocityByCharge = true;
        [vHideInInspector("chargeWeapon")]
        public float chargeVelocityMultiplier = 2;
        [Tooltip("Change between automatic weapon or shot once")]
        [vHideInInspector("chargeWeapon", true)]
        public bool automaticWeapon;

        [vEditorToolbar("Ammo")]
        public float reloadTime = 1f;
        public bool reloadOneByOne;
        [SerializeField, Tooltip("Max clip size of your weapon"), FormerlySerializedAs("clipSize")]
        protected int _clipSize;
        public virtual int clipSize { get { return _clipSize; } set { _clipSize = value; } }
        [Tooltip("Check this to combine extra ammo with the current ammo, the Reload will not be used")]
        public bool dontUseReload;
        [vHideInInspector("dontUseReload", true)]
        [Tooltip("Automatically reload the weapon when it's empty")]
        public bool autoReload;
        [vHideInInspector("autoReload")]
        public AutoReloadStyle autoReloadStyle = AutoReloadStyle.WhenAiming;
        [Tooltip("Ammo ID - make sure your AmmoManager and ItemListData use the same ID"), vHideInInspector("isInfinityAmmo", true)]
        public int ammoID;

        [vEditorToolbar("Weapon ID")]
        [Tooltip("What moveset the underbody will play")]
        public float moveSetID;
        [Tooltip("What moveset the uperbody will play")]
        public float upperBodyID;
        [Tooltip("What shot animation will trigger")]
        public float shotID;
        [Tooltip("What reload animation will play")]
        public int reloadID;
        [Tooltip("What equip animation will play")]
        public int equipID;

        [vEditorToolbar("IK Options")]
        [Tooltip("IK will help the right hand to align where you actually is aiming")]
        public bool alignRightHandToAim = true;
        [Tooltip("IK will help the right hand to align where you actually is aiming")]
        public bool alignRightUpperArmToAim = true;
        public bool raycastAimTarget = true;

        [Tooltip("Left IK on free locomotion")]
        public IKLocomotionOptions freeIKOptions = new IKLocomotionOptions();
        [Tooltip("Left IK on strafe locomotion")]
        public IKLocomotionOptions strafeIKOptions = new IKLocomotionOptions();

        [Tooltip("Left IK while attacking")]
        public bool useIkAttacking = false;
        [Tooltip("Left IK while Shot")]
        public bool disableIkOnShot = false;
        [Tooltip("Left IK while Aiming")]
        public bool useIKOnAiming = true;
        [Tooltip("Left IK Hand Target")]
        public Transform handIKTarget;

        [vEditorToolbar("Projectile")]
        [Tooltip("Assign the aimReference of your weapon")]
        public Transform aimReference;
        [vHelpBox("Only affects the camera from player, when player is aiming")]
        [Tooltip("how much the camera will sway when aiming, 1 means no cameraSway and   means maxCameraSway from the ShooterManager")]
        [Range(0, 1)]
        [UnityEngine.Serialization.FormerlySerializedAs("precision")]
        public float cameraStability = 0.5f;
        [Tooltip("Creates a right recoil on the camera")]
        public float recoilRight = 1;
        [Tooltip("Creates a left recoil on the camera")]
        public float recoilLeft = -1;
        [Tooltip("Creates a up recoil on the camera")]
        public float recoilUp = 1;

        [vEditorToolbar("Audio & VFX")]
        public AudioSource reloadSource;
        public AudioClip reloadClip;
        public AudioClip finishReloadClip;
        [vEditorToolbar("Scope View")]
        [vHelpBox("Third Person Controller Only", vHelpBoxAttribute.MessageType.Info)]
        public bool onlyUseScopeUIView;

        [Tooltip("Check this bool to use an UI image for the scope, ex: snipers")]
        public bool useUI;
        [Tooltip("You can create different Aim sprites and use for different weapons")]
        public int scopeID;
        [Tooltip("The weight of shoot animation when using scope"), Range(0, 1f)]
        public float scopeShootAnimationWeight = .5f;
        [Tooltip("change the FOV of the scope view\n **The calc is default value (60)-scopeZoom**"), Range(-118, 60)]
        public float scopeZoom = 60;
        [Tooltip("Change the FOV of the scope view Background\n **The calc is default value (60)-scopeZoom**"), Range(-118, 60)]
        public float backGroundScopeZoom = 0;
        [Tooltip("Used with the TPCamera to use a custom CameraState while aiming, if it's empty it will use the 'Aiming' CameraState.")]
        public string customAimCameraState;
        [Tooltip("Used with the TPCamera to use a custom CameraState while using scope view mode, if it's empty it will use the 'Aiming' CameraState.")]
        public string customScopeCameraState;
        [Tooltip("assign an empty transform with the pos/rot of your scope view")]
        public Transform scopeTarget;
        public Camera zoomScopeCamera;

        [vHelpBox("Keep Scope Camera Z is used to align z rotation of the zoomScopeCamera to z rotation of the weapon muzzle<color=red> (Projectile toolbar)</color>. if you want to align camera with Vector3.up in z rotation enable this.")]
        public bool keepScopeCameraRotationZ = true;

        [System.Serializable]
        public class OnChangePowerCharger : UnityEvent<float> { }
        [HideInInspector]
        public bool isAiming, usingScope;

        [vEditorToolbar("Events")]
        public UnityEvent onReload, onCancelReload, onFinishReload, onFinishAmmo, onEnableAim, onDisableAim, onEnableScope, onDisableScope, onFullPower;
        [HideInInspector]
        public UnityEvent onDisable;
        public OnChangePowerCharger onPowerChargerChanged;

        [HideInInspector]
        public Transform root;
        [HideInInspector]
        public bool isSecundaryWeapon;

        public delegate bool CheckAmmoHandle(ref bool isValid, ref int totalAmmo);
        public delegate void ChangeAmmoHandle(int value);
        public CheckAmmoHandle checkAmmoHandle;
        public ChangeAmmoHandle changeAmmoHandle;

        /// <summary>
        /// Weapon is in equipped in hand
        /// </summary>
        public virtual bool IsEquipped { get; set; }
        protected virtual float _charge { get; set; }
        protected virtual Transform _handIKTargetOffset
        {
            get; set;
        }

        public virtual Transform handIKTargetOffset
        {
            get
            {
                if (_handIKTargetOffset == null && handIKTarget != null)
                {
                    _handIKTargetOffset = new GameObject("Offset").transform;
                    _handIKTargetOffset.SetParent(handIKTarget);
                    _handIKTargetOffset.localPosition = Vector3.zero;
                    _handIKTargetOffset.localEulerAngles = Vector3.zero;
                }
                return _handIKTargetOffset;
            }
        }
        #endregion

        [System.NonSerialized] private float testTime;

        protected virtual void OnDrawGizmos()
        {

            if (!Application.isPlaying && testShootEffect)
            {
                if (testTime <= 0)
                {
                    Shootest();
                }
                else
                {
                    testTime -= Time.deltaTime;
                }
            }
        }

        protected virtual void OnDisable()
        {
            onDisable.Invoke();
        }

        protected virtual void Start()
        {
            if (!reloadSource)
            {
                reloadSource = source;
            }

            SetScopeZoom(scopeZoom);
        }

        public virtual void Shootest()
        {
            testTime = shootFrequency;
            StartEmitters();
            lightOnShot.enabled = true;
            source.PlayOneShot(fireClip);
            Invoke("StopShootTest", .037f);
        }

        protected virtual void StopShootTest()
        {
            StopEmitters();
            lightOnShot.enabled = false;
        }

        public virtual float powerCharge
        {
            get
            {
                return _charge;
            }
            set
            {
                if (value != _charge)
                {
                    _charge = value;
                    onPowerChargerChanged.Invoke(_charge);
                    if (_charge >= 1)
                    {
                        onFullPower.Invoke();
                    }
                }
            }
        }

        public virtual void SetPrecision(float value)
        {
            cameraStability = Mathf.Clamp(value, 0, 1);
        }

        public override bool HasAmmo()
        {
            if (checkAmmoHandle != null)
            {
                bool isValidAmmo = false;
                int totalAmmo = 0;
                var hasAmmo = checkAmmoHandle.Invoke(ref isValidAmmo, ref totalAmmo);
                if (isValidAmmo)
                {
                    return hasAmmo;
                }
                else
                {
                    return ammo > 0;
                }
            }
            else
            {
                return ammo > 0;
            }
        }

        public virtual int ammoCount
        {
            get
            {
                if (checkAmmoHandle != null)
                {
                    bool isValidAmmo = false;
                    int totalAmmo = 0;
                    checkAmmoHandle.Invoke(ref isValidAmmo, ref totalAmmo);
                    if (isValidAmmo)
                    {
                        return totalAmmo;
                    }
                    else
                    {
                        return ammo;
                    }
                }
                return ammo;
            }
        }

        public virtual void AddAmmo(int value)
        {
            if (checkAmmoHandle != null && changeAmmoHandle != null)
            {

                bool isValidAmmo = false;
                int totalAmmo = 0;
                checkAmmoHandle.Invoke(ref isValidAmmo, ref totalAmmo);
                if (isValidAmmo)
                {
                    changeAmmoHandle(value);
                }
                else
                {
                    ammo += value;
                }
            }
            else
            {
                ammo += value;
            }
        }

        public override void UseAmmo(int count = 1)
        {
            if (checkAmmoHandle != null && changeAmmoHandle != null)
            {

                bool isValidAmmo = false;
                int totalAmmo = 0;
                checkAmmoHandle.Invoke(ref isValidAmmo, ref totalAmmo);
                if (isValidAmmo)
                {
                    changeAmmoHandle(-count);
                }
                else
                {
                    ammo -= count;
                }
            }
            else
            {
                ammo -= count;
            }
        }

        public virtual void ReloadEffect()
        {

            if (reloadSource && reloadClip)
            {
                reloadSource.Stop();
                reloadSource.PlayOneShot(reloadClip);
            }
            onReload.Invoke();
        }

        public virtual void FinishReloadEffect()
        {
            if (reloadSource && finishReloadClip)
            {
                reloadSource.Stop();
                reloadSource.PlayOneShot(finishReloadClip);
            }
            onFinishReload.Invoke();
        }

        protected override float damageMultiplier
        {
            get
            {
                if (!chargeWeapon)
                {
                    return base.damageMultiplier;
                }

                return (float)System.Math.Round((1 + Mathf.Lerp(0, chargeDamageMultiplier, _charge)) + damageMultiplierMod, 1);
            }
        }

        protected override float velocityMultiplier
        {
            get
            {
                if (!chargeWeapon || !changeVelocityByCharge)
                {
                    return base.velocityMultiplier;
                }

                return (1 + Mathf.Lerp(0, chargeVelocityMultiplier, _charge)) + velocityMultiplierMod;
            }
        }

        public virtual void SetScopeZoom(float value)
        {
            if (zoomScopeCamera)
            {
                var zoom = Mathf.Clamp(61 - value, 1, 179);
                zoomScopeCamera.fieldOfView = zoom;
            }
        }

        public virtual void SetActiveAim(bool value)
        {
            if (isAiming != value)
            {
                isAiming = value;
                if (isAiming)
                {
                    onEnableAim.Invoke();
                }
                else
                {
                    onDisableAim.Invoke();
                }
            }
        }

        /// <summary>
        /// Set if Weapon is using scope
        /// </summary>
        /// <param name="value"></param>
        public virtual void SetActiveScope(bool value)
        {
            if (usingScope != value)
            {
                usingScope = value;
                if (usingScope)
                {
                    onEnableScope.Invoke();
                }
                else
                {
                    onDisableScope.Invoke();
                }
            }
        }

        /// <summary>
        /// Set look target point to Zoom scope camera
        /// </summary>
        /// <param name="point"></param>
        public virtual void SetScopeLookTarget(Vector3 point)
        {
            if (zoomScopeCamera)
            {
                var euler = Quaternion.LookRotation(point - zoomScopeCamera.transform.position, Vector3.up).eulerAngles;
                if (keepScopeCameraRotationZ)
                {
                    euler.z = muzzle.transform.eulerAngles.z;
                }

                zoomScopeCamera.transform.eulerAngles = euler;
            }
        }

        public virtual void CancelReload()
        {
            if (reloadSource && reloadSource.isPlaying)
            {
                reloadSource.Stop();
            }

            onCancelReload.Invoke();
        }
    }
}
