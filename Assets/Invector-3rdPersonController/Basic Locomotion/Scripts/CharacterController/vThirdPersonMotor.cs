
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vCharacterController
{
    using UnityEngine.Serialization;
    using vEventSystems;
    public class vThirdPersonMotor : vCharacter, vIAnimatorStateInfoController
    {
        #region Variables               

        #region Stamina       

        [vEditorToolbar("Stamina", order = 2)]
        [SerializeField] protected float _maxStamina = 200f;
        public virtual float maxStamina { get { return _maxStamina; } set { _maxStamina = value; } }
        [SerializeField] protected float _staminaRecovery = 1.2f;
        public virtual float staminaRecovery { get { return _staminaRecovery; } set { _staminaRecovery = value; } }
        internal float currentStamina;
        internal float currentStaminaRecoveryDelay;
        [SerializeField] protected float _sprintStamina = 30f;
        public virtual float sprintStamina { get { return _sprintStamina; } set { _sprintStamina = value; } }
        [SerializeField] protected float _jumpStamina = 30f;
        public virtual float jumpStamina { get { return _jumpStamina; } set { _jumpStamina = value; } }
        [SerializeField] protected float _rollStamina = 25f;
        public virtual float rollStamina { get { return _rollStamina; } set { _rollStamina = value; } }

        [vEditorToolbar("Events", order = 7)]
        public UnityEvent OnExitGround;
        public UnityEvent OnGrounded;
        public UnityEvent OnRoll;
        public UnityEvent OnJump;
        public UnityEvent OnStartSprinting;
        public UnityEvent OnFinishSprinting;
        public UnityEvent OnFinishSprintingByStamina;
        public UnityEvent OnStaminaEnd;

        #endregion

        #region Crouch
        [vEditorToolbar("Crouch", order = 3)]
        [Range(1, 2.5f)]
        public float crouchHeightReduction = 2f;
        [Range(1, 2f)]
        public float crouchColliderRadius = 1.5f;

        [Tooltip("What objects can make the character auto crouch")]
        public LayerMask autoCrouchLayer = 1 << 0;
        [Tooltip("[SPHERECAST] ADJUST IN PLAY MODE - White Spherecast put just above the head, this will make the character Auto-Crouch if something hit the sphere.")]
        public float crouchHeadDetect = 0.95f;
        #endregion

        #region Character Variables       
        [vEditorToolbar("Locomotion", order = 0)]

        [vSeparator("Movement Settings")]
        [Tooltip("Multiply the current speed of the controller rigidbody velocity")]
        [SerializeField] protected float _speedMultiplier = 1;
        public virtual float speedMultiplier { get { return _speedMultiplier; } set { _speedMultiplier = value; } }
        [Tooltip("Use this to rotate the character using the World axis, or false to use the camera axis - CHECK for Isometric Camera")]
        public bool rotateByWorld = false;
        [Tooltip("Use this to move character only in forward direction when in free locomotion, the character will rotate to input direction normaly but the movement will be based on character forward")]
        public bool moveForwardInFree;
        public enum LocomotionType
        {
            FreeWithStrafe,
            OnlyStrafe,
            OnlyFree,
        }

        [vHelpBox("FreeLocomotion: Rotate on any direction regardless of the camera \nStrafeLocomotion: Move always facing forward (extra directional animations)")]

        [SerializeField, FormerlySerializedAs("locomotionType")] protected LocomotionType _locomotionType = LocomotionType.FreeWithStrafe;
        public virtual LocomotionType locomotionType { get { return _locomotionType; } set { _locomotionType = value; } }

        public vMovementSpeed freeSpeed, strafeSpeed;

        [vSeparator("Extra Animation Settings")]

        [Tooltip("Use it for debug purposes")]
        public bool disableAnimations;
        [Tooltip("Turn off if you have 'in place' animations and use this values above to move the character, or use with root motion as extra speed")]
        [vHelpBox("When 'Use RootMotion' is checked, make sure to reset all speeds to zero to use the original root motion velocity.")]
        public bool useRootMotion = false;
        [Tooltip("While in Free Locomotion the character will lean to left/right when steering")]
        public bool useLeanMovementAnim = true;
        [Tooltip("Smooth value for the Lean Movement animation")]
        [Range(0.01f, 0.1f)]
        public float leanSmooth = 0.05f;
        [Tooltip("Check this to use the TurnOnSpot animations while the character is stading still and rotating in place")]
        public bool useTurnOnSpotAnim = true;
        public float turnOnSpotSmooth = 0.01f;
        [Tooltip("Put your Random Idle animations at the AnimatorController and select a value to randomize, 0 is disable.")]
        public float randomIdleTime = 0f;


        /// <summary>
        /// ignore animation root motion when input is zero
        /// </summary>
        internal bool ignoreAnimatorMovement;

        [vSeparator("Extra Movement Settings")]
        [Tooltip("Check This to use sprint on press button to your Character run until the stamina finish or movement stops\nIf uncheck your Character will sprint as long as the SprintInput is pressed or the stamina finishes")]
        public bool useContinuousSprint = true;
        [Tooltip("Check this to sprint always in free movement")]
        public bool sprintOnlyFree = true;

        public enum CustomFixedTimeStep { Default, FPS30, FPS60, FPS75, FPS90, FPS120, FPS144 };

        [vHelpBox("Set the FixedTimeStep to match the FPS of your Game, \nEx: If your game aims to run at 30fps, select FPS30 to match the FixedUpdate Physics")]
        public CustomFixedTimeStep customFixedTimeStep = CustomFixedTimeStep.FPS60;

        [vEditorToolbar("Jump / Airborne", order = 3)]

        [vHelpBox("Jump only works via Rigidbody Physics, if you want Jump that use only RootMotion make sure to use the AnimatorTag 'CustomAction' ")]

        [vSeparator("Jump")]
        [Tooltip("Use the currently Rigidbody Velocity to influence on the Jump Distance")]
        public bool jumpWithRigidbodyForce = false;
        [Tooltip("Rotate or not while airborne")]
        public bool jumpAndRotate = true;
        [Tooltip("How much time the character will be jumping")]
        public float jumpTimer = 0.3f;
        [Tooltip("Delay to match the animation anticipation")]
        public float jumpStandingDelay = 0.25f;
        internal float jumpCounter;
        internal bool inJumpStarted;
        [Tooltip("Add Extra jump height, if you want to jump only with Root Motion leave the value with 0.")]
        [SerializeField] protected float _jumpHeight = 4f;
        public virtual float jumpHeight { get { return _jumpHeight; } set { _jumpHeight = value; } }

        [vSeparator("Falling")]
        [Tooltip("Speed that the character will move while airborne")]
        [SerializeField] protected float _airSpeed = 5f;
        public virtual float airSpeed { get { return _airSpeed; } set { _airSpeed = value; } }

        [Tooltip("Smoothness of the direction while airborne")]
        [SerializeField] protected float _airSmooth = 6f;
        public virtual float airSmooth { get { return _airSmooth; } set { _airSmooth = value; } }
        [Tooltip("Apply extra gravity when the character is not grounded")]
        [SerializeField] protected float _extraGravity = -10f;
        public virtual float extraGravity { get { return _extraGravity; } set { _extraGravity = value; } }
        [Tooltip("Limit of the vertical velocity when Falling")]
        [SerializeField] protected float _limitFallVelocity = -15f;
        public virtual float limitFallVelocity { get { return _limitFallVelocity; } set { _limitFallVelocity = value; } }
        [Tooltip("Turn the Ragdoll On when falling at high speed (check VerticalVelocity) - leave the value with 0 if you don't want this feature")]
        [SerializeField] protected float _ragdollVelocity = -15f;
        public virtual float ragdollVelocity { get { return _ragdollVelocity; } set { _ragdollVelocity = value; } }

        [vSeparator("Fall Damage")]
        [SerializeField] protected float _fallMinHeight = 6f;
        public virtual float fallMinHeight { get { return _fallMinHeight; } set { _fallMinHeight = value; } }
        [SerializeField] protected float _fallMinVerticalVelocity = -10f;
        public virtual float fallMinVerticalVelocity { get { return _fallMinVerticalVelocity; } set { _fallMinVerticalVelocity = value; } }
        [SerializeField] protected float _fallDamage = 10f;
        public virtual float fallDamage { get { return _fallDamage; } set { _fallDamage = value; } }

        [vEditorToolbar("Roll", order = 4)]
        public bool useRollRootMotion = true;
        [Tooltip("Animation Transition from current animation to Roll")]
        public float rollTransition = .25f;
        [Range(1, 2.5f)]
        public float rollHeightReduction = 1.6f;
        [Range(1, 2f)]
        public float rollColliderRadius = 1.5f;
        [Tooltip("Can control the Roll Direction")]
        public bool rollControl = true;
        [Tooltip("Speed of the Roll Movement")]
        [SerializeField] protected float _rollSpeed = 0f;
        public virtual float rollSpeed { get { return _rollSpeed; } set { _rollSpeed = value; } }

        [Tooltip("Speed of the Roll Rotation")]
        [SerializeField] protected float _rollRotationSpeed = 20f;
        public virtual float rollRotationSpeed { get { return _rollRotationSpeed; } set { _rollRotationSpeed = value; } }
        [vHideInInspector("Roll use gravity influence")]
        [SerializeField] protected bool _rollUseGravity = true;
        public virtual bool rollUseGravity { get { return _rollUseGravity; } set { _rollUseGravity = value; } }

        [vHideInInspector("rollUseGravity")]
        [Tooltip("Normalized Time of the roll animation to enable gravity influence")]
        [SerializeField] protected float _rollUseGravityTime = 0.2f;
        public virtual float rollUseGravityTime { get { return _rollUseGravityTime; } set { _rollUseGravityTime = value; } }
        [Tooltip("Use the normalized time of the animation to know when you can roll again")]
        [Range(0, 1)]
        [SerializeField] protected float _timeToRollAgain = 0.75f;
        public virtual float timeToRollAgain { get { return _timeToRollAgain; } set { _timeToRollAgain = value; } }
        [Tooltip("Ignore all damage while is rolling, include Damage that ignore defense")]
        [SerializeField] protected bool _noDamageWhileRolling = true;
        public virtual bool noDamageWhileRolling { get { return _noDamageWhileRolling; } set { _noDamageWhileRolling = value; } }
        [Tooltip("Ignore damage that needs to activate ragdoll")]
        [SerializeField] protected bool _noActiveRagdollWhileRolling = true;
        public virtual bool noActiveRagdollWhileRolling { get { return _noActiveRagdollWhileRolling; } set { _noActiveRagdollWhileRolling = value; } }

        public enum GroundCheckMethod
        {
            Low, High
        }
        public enum StopMoveCheckMethod
        {
            RayCast, SphereCast, CapsuleCast
        }

        [vEditorToolbar("Grounded", order = 3)]

        [vSeparator("Ground")]
        [SerializeField]
        [vReadOnly()] protected bool _isGrounded;
        [Tooltip("Layers that the character can walk on")]
        public LayerMask groundLayer = 1 << 0;
        [Tooltip("Ground Check Method To check ground Distance and ground angle\n*Simple: Use just a single Raycast\n*Normal: Use Raycast and SphereCast\n*Complex: Use SphereCastAll")]
        public GroundCheckMethod groundCheckMethod = GroundCheckMethod.High;
        [Tooltip("The length of the Ray cast to detect ground ")]
        public float groundDetectionDistance = 10f;
        [Tooltip("Snaps the capsule collider to the ground surface, recommend when using complex terrains or inclined ramps")]
        public bool useSnapGround = true;
        [Range(0, 1)]
        public float snapPower = 0.5f;
        [Tooltip("Distance to became not grounded")]
        [Range(0, 10)]
        public float groundMinDistance = 0.1f;
        [Range(0, 10)]
        public float groundMaxDistance = 0.5f;
        [Tooltip("Max angle to walk")]

        [vSeparator("StopMove")]

        public LayerMask stopMoveLayer;
        [vHelpBox("Character will stop moving, ex: walls - set the layer to nothing to not use")]
        public float stopMoveRayDistance = 1f;
        public float stopMoveMaxHeight = 1.6f;
        public StopMoveCheckMethod stopMoveCheckMethod = StopMoveCheckMethod.RayCast;


        [vSeparator("Slope Limit")]
        [SerializeField] protected bool _useSlopeLimit = true;
        public virtual bool useSlopeLimit { get { return _useSlopeLimit; } set { _useSlopeLimit = value; } }
        [Range(30, 80)]
        [SerializeField] protected float _slopeLimit = 75f;
        public virtual float slopeLimit { get { return _slopeLimit; } set { _slopeLimit = value; } }

        [SerializeField] protected float _stopSlopeMargin = 20f;
        public virtual float stopSlopeMargin { get { return _stopSlopeMargin; } set { _stopSlopeMargin = value; } }
        [SerializeField] protected float _SlopeSidewaysSmooth = 2f;
        public virtual float slopeSidewaysSmooth { get { return _SlopeSidewaysSmooth; } set { _SlopeSidewaysSmooth = value; } }
        [SerializeField] protected float _slopeMinDistance = 0f;
        public virtual float slopeMinDistance { get { return _slopeMinDistance; } set { _slopeMinDistance = value; } }
        [SerializeField] protected float _slopeMaxDistance = 1.5f;
        public virtual float slopeMaxDistance { get { return _slopeMaxDistance; } set { _slopeMaxDistance = value; } }
        [SerializeField] protected float _slopeLimitHeight = 0.2f;
        public virtual float slopeLimitHeight { get { return _slopeLimitHeight; } set { _slopeLimitHeight = value; } }


        protected float _slopeSidewaysSmooth;
        [HideInInspector]
        public bool steepSlopeAhead;

        [vSeparator("Slide On Slopes")]
        [SerializeField] protected bool _useSlide = true;
        public virtual bool useSlide { get { return _useSlide; } set { _useSlide = value; } }
        [Tooltip("Velocity to slide down when on a slope limit ramp")]
        [Range(0, 30)]
        [SerializeField] protected float _slideDownVelocity = 10f;
        public virtual float slideDownVelocity { get { return _slideDownVelocity; } set { _slideDownVelocity = value; } }
        [Tooltip("Smooth to slide down the controller")]
        [SerializeField] protected float _slideDownSmooth = 2f;
        public virtual float slideDownSmooth { get { return _slideDownSmooth; } set { _slideDownSmooth = value; } }
        [Tooltip("Velocity to slide sideways when on a slope limit ramp")]
        [Range(0, 1)]
        [SerializeField] protected float _slideSidewaysVelocity = 0.5f;
        public virtual float slideSidewaysVelocity { get { return _slideSidewaysVelocity; } set { _slideSidewaysVelocity = value; } }
        [Range(0f, 1f)]
        [Tooltip("Delay to start sliding once the character is standing on a slope")]
        [SerializeField] protected float _SlidingEnterTime = 0.2f;
        public virtual float slidingEnterTime { get { return _SlidingEnterTime; } set { _SlidingEnterTime = value; } }

        internal float _slidingEnterTime;
        [Range(0f, 1f)]
        [Tooltip("Delay to rotate once the character started sliding")]
        [SerializeField] protected float _RotateSlopeEnterTime = 0.1f;
        public virtual float rotateSlopeEnterTime { get { return _RotateSlopeEnterTime; } set { _RotateSlopeEnterTime = value; } }
        [Tooltip("Smooth to rotate the controller")]
        [SerializeField] protected float _rotateDownSlopeSmooth = 8f;
        public virtual float rotateDownSlopeSmooth { get { return _rotateDownSlopeSmooth; } set { _rotateDownSlopeSmooth = value; } }
        internal float _rotateSlopeEnterTime;

        [vSeparator("Step Offset")]
        [SerializeField] protected bool _useStepOffset = true;
        public virtual bool useStepOffset { get { return _useStepOffset; } set { _useStepOffset = value; } }
        [Tooltip("Layers that the character will perform a StepOffset")]
        public LayerMask stepOffsetLayer = 1 << 0;
        [Tooltip("Offset max height to walk on steps - YELLOW Raycast in front of the legs")]
        [Range(0, 1)]
        [SerializeField] protected float _stepOffsetMaxHeight = 0.5f;
        public virtual float stepOffsetMaxHeight { get { return _stepOffsetMaxHeight; } set { _stepOffsetMaxHeight = value; } }
        [Tooltip("Offset min height to walk on steps. Make sure to keep slight above the floor - YELLOW Raycast in front of the legs")]
        [Range(0, 1)]
        [SerializeField] protected float _stepOffsetMinHeight = 0f;
        public virtual float stepOffsetMinHeight { get { return _stepOffsetMinHeight; } set { _stepOffsetMinHeight = value; } }
        [Tooltip("Offset distance to walk on steps - YELLOW Raycast in front of the legs")]
        [Range(0, 1)]
        [SerializeField] protected float _stepOffsetDistance = 0.1f;
        public virtual float stepOffsetDistance { get { return _stepOffsetDistance; } set { _stepOffsetDistance = value; } }
        internal float _stopMoveWeight;
        internal virtual float stopMoveWeight { get { return _stopMoveWeight; } set { _stopMoveWeight = value; } }
        internal float _sprintWeight;
        internal virtual float sprintWeight { get { return _sprintWeight; } set { _sprintWeight = value; } }
        internal float groundDistance;
        public RaycastHit groundHit;

        [vEditorToolbar("Debug", order = 9)]
        [Header("--- Debug Info ---")]
        public bool debugWindow;
        public vAnimatorStateInfos _animatorStateInfos;
        public vAnimatorStateInfos animatorStateInfos { get => _animatorStateInfos; protected set => _animatorStateInfos = value; }


        #endregion

        #region Actions

        public virtual bool isStrafing
        {
            get
            {
                return sprintOnlyFree && isSprinting ? false : _isStrafing;
            }
            set
            {
                _isStrafing = value;
            }
        }

        // movement bools

        public bool isGrounded
        {
            get
            {
                return _isGrounded;
            }
            set
            {
                if (_isGrounded != value)
                {
                    _isGrounded = value;
                    if (_isGrounded) OnGrounded.Invoke();
                    else OnExitGround.Invoke();
                }
            }
        }
        /// <summary>
        /// use to stop update the Check Ground method and return true for IsGrounded
        /// </summary>
        public bool disableCheckGround { get; set; }
        public bool inCrouchArea { get; protected set; }
        protected bool _isSprinting = false;
        public virtual bool isSprinting { get { return _isSprinting; } set { _isSprinting = value; } }
        public bool isSliding { get; protected set; }
        public bool autoCrouch { get; protected set; }

        // action bools
        internal bool
            isRolling,
            isJumping,
            isInAirborne,
            isTurningOnSpot;

        internal bool customAction;


        protected void RemoveComponents()
        {
            if (!removeComponentsAfterDie)
            {
                return;
            }

            if (_capsuleCollider != null)
            {
                Destroy(_capsuleCollider);
            }

            if (_rigidbody != null)
            {
                Destroy(_rigidbody);
            }

            if (animator != null)
            {
                Destroy(animator);
            }

            var comps = GetComponents<MonoBehaviour>();
            for (int i = 0; i < comps.Length; i++)
            {
                Destroy(comps[i]);
            }
        }

        #endregion      

        #region Components

        internal Rigidbody _rigidbody;                                                      // access the Rigidbody component
        internal PhysicMaterial frictionPhysics, maxFrictionPhysics, slippyPhysics;         // create PhysicMaterial for the Rigidbody
        internal CapsuleCollider _capsuleCollider;                                          // access CapsuleCollider information
        public PhysicMaterial currentMaterialPhysics { get; protected set; }
        #endregion

        #region Hide Variables
        public virtual float defaultSpeedMultiplier { get; set; }
        public virtual float inputMagnitude { get; set; }                     // sets the inputMagnitude to update the animations in the animator controller
        public virtual float rotationMagnitude { get; set; }                   // sets the rotationMagnitude to update the animations in the animator controller
        public virtual float verticalSpeed { get; set; }                       // set the verticalSpeed based on the verticalInput        
        public virtual float horizontalSpeed { get; set; }                     // set the horizontalSpeed based on the horizontalInput
        public virtual bool invertVerticalSpeed { get; set; }
        public virtual bool invertHorizontalSpeed { get; set; }
        public virtual float moveSpeed { get; set; }                           /// set the current moveSpeed for the<seealso cref="MoveCharacter(Vector3)"/> method        
        public virtual float verticalVelocity { get; set; }                    // set the vertical velocity of the rigidbody
        public virtual float colliderRadius { get; set; }
        public virtual float colliderHeight { get; set; }                     // storage capsule collider extra information                       
        public virtual float jumpMultiplier { get; set; }                     // internally used to set the jumpMultiplier
        public virtual float timeToResetJumpMultiplier { get; set; }          // internally used to reset the jump multiplier
        public virtual float heightReached { get; set; }                      // max height that character reached in air;       
        public virtual bool lockMovement { get; set; }                        // lock the movement of the controller (not the animation)
        public virtual bool lockRotation { get; set; }                        // lock the rotation of the controller (not the animation)
        public virtual bool lockSetMoveSpeed { get; set; }                    // locks the method to update the moveset based on the locomotion type, so you can modify externally
        protected bool _isStrafing { get; set; }                              // internally used to set the strafe movement
        public virtual bool lockInStrafe { get; set; }                        // locks the controller to only used the strafe locomotion type        
        public virtual bool forceRootMotion { get; set; }                     // force the controller to use root motion
        public virtual bool keepDirection { get; set; }                       // keeps the character direction even if the camera direction changes
        public virtual bool finishStaminaOnSprint { get; set; }               // used to trigger the OnFinishStamina event
        public virtual bool applyingStepOffset { get; set; }                  // internally used to apply the StepOffset       
        public virtual bool lockAnimMovement { get; set; }                    // internally used with the vAnimatorTag("LockMovement"), use on the animator to lock the movement of a specific animation clip        
        public virtual bool lockAnimRotation { get; set; }                    // internally used with the vAnimatorTag("LockRotation"), use on the animator to lock a rotation of a specific animation clip
        public virtual Vector3 lastCharacterAngle { get; set; }               //Last angle of the character used to calculate rotationMagnitude;
        public virtual Transform rotateTarget { get; set; }
        public virtual Vector3 input { get; set; }                              // generate raw input for the controller
        public virtual Vector3 oldInput { get; set; }                           // used internally to identify oldinput from the current input
        public virtual Vector3 colliderCenter { get; set; }                     // storage the center of the capsule collider info                
        public virtual Vector3 inputSmooth { get; set; }        // generate smooth input based on the inputSmooth value      

        public virtual Vector3 moveDirection { get; set; }

        public RaycastHit stepOffsetHit;
        public RaycastHit slopeHitInfo;

        internal AnimatorStateInfo baseLayerInfo, underBodyInfo, rightArmInfo, leftArmInfo, fullBodyInfo, upperBodyInfo;


        public virtual int baseLayer { get { return animator.GetLayerIndex("Base Layer"); } }
        public virtual int underBodyLayer { get { return animator.GetLayerIndex("UnderBody"); } }
        public virtual int rightArmLayer { get { return animator.GetLayerIndex("RightArm"); } }
        public virtual int leftArmLayer { get { return animator.GetLayerIndex("LeftArm"); } }
        public virtual int upperBodyLayer { get { return animator.GetLayerIndex("UpperBody"); } }
        public virtual int fullbodyLayer { get { return animator.GetLayerIndex("FullBody"); } }
        /// <summary>
        /// Default radius of the Character Capsule
        /// </summary>
        public virtual float colliderRadiusDefault
        {
            get; protected set;
        }
        /// <summary>
        /// Default Height of the Character Capsule
        /// </summary>
        public virtual float colliderHeightDefault
        {
            get; protected set;
        }
        /// <summary>
        /// Default Center of the Character Capsule
        /// </summary>
        public virtual Vector3 colliderCenterDefault
        {
            get; protected set;
        }

        /// <summary>
        ///Check if Can Apply Fall Damage and/or Enable Ragdoll when landing. <see cref="jumpMultiplier"/>  automatically return false if > 1 or if <seealso cref="customAction"/> is true
        /// </summary>
        protected virtual bool _canApplyFallDamage { get { return !blockApplyFallDamage && jumpMultiplier <= 1 && !customAction; } }
        /// <summary>
        /// For movement to walk by default. 
        /// </summary>
        public virtual bool alwaysWalkByDefault { get; set; }
        /// <summary>
        /// Can Apply Fall Damage and/or Enable Ragdoll when landing;
        /// </summary>
        public virtual bool blockApplyFallDamage { get; set; }

        #endregion

        #endregion

        protected virtual void Awake()
        {
            defaultSpeedMultiplier = 1;
            jumpMultiplier = 1;
            heightReached = transform.position.y;
            SetCustomFixedTimeStep();
        }

        protected override void Start()
        {
            base.Start();
        }

        public override void Init()
        {
            base.Init();

            animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            // slides the character through walls and edges
            frictionPhysics = new PhysicMaterial();
            frictionPhysics.name = "frictionPhysics";
            frictionPhysics.staticFriction = .25f;
            frictionPhysics.dynamicFriction = .25f;
            frictionPhysics.frictionCombine = PhysicMaterialCombine.Multiply;

            // prevents the collider from slipping on ramps
            maxFrictionPhysics = new PhysicMaterial();
            maxFrictionPhysics.name = "maxFrictionPhysics";
            maxFrictionPhysics.staticFriction = 1f;
            maxFrictionPhysics.dynamicFriction = 1f;
            maxFrictionPhysics.frictionCombine = PhysicMaterialCombine.Maximum;

            // air physics 
            slippyPhysics = new PhysicMaterial();
            slippyPhysics.name = "slippyPhysics";
            slippyPhysics.staticFriction = 0f;
            slippyPhysics.dynamicFriction = 0f;
            slippyPhysics.frictionCombine = PhysicMaterialCombine.Minimum;

            // rigidbody info
            _rigidbody = GetComponent<Rigidbody>();

            // capsule collider info
            _capsuleCollider = GetComponent<CapsuleCollider>();

            // save your collider preferences 
            colliderCenter = colliderCenterDefault = _capsuleCollider.center;
            colliderRadius = colliderRadiusDefault = _capsuleCollider.radius;
            colliderHeight = colliderHeightDefault = _capsuleCollider.height;

            // avoid collision detection with inside colliders 
            Collider[] AllColliders = this.GetComponentsInChildren<Collider>();
            for (int i = 0; i < AllColliders.Length; i++)
            {
                Physics.IgnoreCollision(_capsuleCollider, AllColliders[i]);
            }

            // health info
            if (fillHealthOnStart)
            {
                currentHealth = maxHealth;
            }

            currentHealthRecoveryDelay = healthRecoveryDelay;
            currentStamina = maxStamina;
            ResetJumpMultiplier();
            isGrounded = true;
            ResetControllerSpeedMultiplier();
            freeSpeed.Init();
            strafeSpeed.Init();
        }

        public virtual void SetCustomFixedTimeStep()
        {
            switch (customFixedTimeStep)
            {
                case CustomFixedTimeStep.Default:
                    break;
                case CustomFixedTimeStep.FPS30:
                    Time.fixedDeltaTime = 0.03333334f;
                    break;
                case CustomFixedTimeStep.FPS60:
                    Time.fixedDeltaTime = 0.01666667f;
                    break;
                case CustomFixedTimeStep.FPS75:
                    Time.fixedDeltaTime = 0.01333333f;
                    break;
                case CustomFixedTimeStep.FPS90:
                    Time.fixedDeltaTime = 0.01111111f;
                    break;
                case CustomFixedTimeStep.FPS120:
                    Time.fixedDeltaTime = 0.008333334f;
                    break;
                case CustomFixedTimeStep.FPS144:
                    Time.fixedDeltaTime = 0.006944444f;
                    break;
            }
        }

        public virtual void UpdateMotor()
        {
            CheckStamina();
            CheckGround();

            SlideMovementBehavior();
            CheckRagdoll();
            ControlCapsuleHeight();
            ControlJumpBehaviour();
            AirControl();
            StaminaRecovery();
            CalculateRotationMagnitude();
        }

        #region Health & Stamina

        public override void TakeDamage(vDamage damage)
        {
            // don't apply damage if the character is rolling, you can add more conditions here
            if (currentHealth <= 0 || (IgnoreDamageRolling()))
            {
                if (damage.activeRagdoll && !IgnoreDamageActiveRagdollRolling())
                {
                    onActiveRagdoll.Invoke(damage);
                }

                return;
            }

            if (damage.activeRagdoll && IgnoreDamageActiveRagdollRolling())
            {
                damage.activeRagdoll = false;
            }

            base.TakeDamage(damage);
        }

        protected virtual bool IgnoreDamageRolling()
        {
            return noDamageWhileRolling == true && isRolling == true;
        }

        protected virtual bool IgnoreDamageActiveRagdollRolling()
        {
            return noActiveRagdollWhileRolling == true && isRolling == true;
        }

        protected override void TriggerDamageReaction(vDamage damage)
        {
            if (!customAction)
            {
                base.TriggerDamageReaction(damage);
            }
            else if (damage.activeRagdoll)
            {
                onActiveRagdoll.Invoke(damage);
            }
        }

        public virtual void ReduceStamina(float value, bool accumulative)
        {
            if (customAction)
            {
                return;
            }

            if (accumulative)
            {
                currentStamina -= value * Time.fixedDeltaTime;
            }
            else
            {
                currentStamina -= value;
            }

            if (currentStamina < 0)
            {
                currentStamina = 0;
                OnStaminaEnd.Invoke();
            }
        }

        /// <summary>
        /// Change the currentStamina of Character
        /// </summary>
        /// <param name="value"></param>
        public virtual void ChangeStamina(int value)
        {
            currentStamina += value;
            currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);
        }

        /// <summary>
        /// Change the MaxStamina of Character
        /// </summary>
        /// <param name="value"></param>
        public virtual void ChangeMaxStamina(int value)
        {
            maxStamina += value;
            if (maxStamina < 0)
            {
                maxStamina = 0;
            }
        }

        public override bool isDead
        {
            get => base.isDead;
            set
            {
                base.isDead = value;
                if (value)
                {
                    if (isGrounded)
                    {
                        if (_rigidbody) _rigidbody.isKinematic = true;
                        if (_capsuleCollider) _capsuleCollider.enabled = false;
                    }
                }
                else if (!ragdolled)
                {
                    if (_rigidbody) _rigidbody.isKinematic = false;
                    if (_capsuleCollider) _capsuleCollider.enabled = true;
                }
            }
        }

        protected virtual void CheckStamina()
        {
            // check how much stamina this action will consume
            if (isSprinting)
            {
                currentStaminaRecoveryDelay = 0.25f;
                ReduceStamina(sprintStamina, true);
            }
        }

        public virtual void StaminaRecovery()
        {
            if (currentStaminaRecoveryDelay > 0)
            {
                currentStaminaRecoveryDelay -= Time.fixedDeltaTime;
            }
            else
            {
                if (currentStamina > maxStamina)
                {
                    currentStamina = maxStamina;
                }

                if (currentStamina < maxStamina)
                {
                    currentStamina += staminaRecovery;
                }
            }
        }

        #endregion

        #region Locomotion

        protected virtual void CalculateRotationMagnitude()
        {
            var eulerDifference = this.transform.eulerAngles - lastCharacterAngle;
            if (eulerDifference.sqrMagnitude < 0.01)
            {
                lastCharacterAngle = transform.eulerAngles;
                rotationMagnitude = 0f;
                return;
            }

            var magnitude = (eulerDifference.NormalizeAngle().y / (isStrafing ? strafeSpeed.rotationSpeed : freeSpeed.rotationSpeed));
            rotationMagnitude = (float)System.Math.Round(magnitude, 2);
            lastCharacterAngle = transform.eulerAngles;
        }

        public virtual void SetControllerSpeedMultiplier(float speed)
        {
            this.speedMultiplier = speed;
        }

        public virtual void ResetControllerSpeedMultiplier()
        {
            this.speedMultiplier = defaultSpeedMultiplier;
        }

        public virtual void SetControllerMoveSpeed(vMovementSpeed speed)
        {
            if (isCrouching)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, speed.crouchSpeed, speed.movementSmooth * Time.fixedDeltaTime);
                return;
            }

            if (speed.walkByDefault || alwaysWalkByDefault)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.runningSpeed : speed.walkSpeed, speed.movementSmooth * Time.fixedDeltaTime);
            }
            else
            {
                moveSpeed = Mathf.Lerp(moveSpeed, isSprinting ? speed.sprintSpeed : speed.runningSpeed, speed.movementSmooth * Time.fixedDeltaTime);
            }
        }

        public virtual void MoveCharacter(Vector3 direction)
        {
            // calculate input smooth
            inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * (useRootMotion ? vTime.deltaTime : vTime.fixedDeltaTime));

            if (isSliding || ragdolled || !isGrounded || isJumping || _rigidbody.isKinematic)
            {
                return;
            }
            var _direction = isStrafing || !moveForwardInFree ? direction : transform.forward;
            _direction.y = 0;
            _direction = _direction.normalized * Mathf.Clamp(direction.magnitude, 0, 1f);

            Vector3 targetPosition = (useRootMotion ? animator.rootPosition : _rigidbody.position) + _direction * (moveSpeed * speedMultiplier) * (useRootMotion ? vTime.deltaTime : vTime.fixedDeltaTime);
            Vector3 targetVelocity = (targetPosition - transform.position) / (useRootMotion ? vTime.deltaTime : vTime.fixedDeltaTime);

            bool useVerticalVelocity = true;

            SnapToGround(ref targetVelocity, ref useVerticalVelocity);

            steepSlopeAhead = CheckForSlope(ref targetVelocity);

            if (!steepSlopeAhead)
            {
                CalculateStepOffset(_direction.normalized, ref targetVelocity, ref useVerticalVelocity);
            }

            CheckStopMove(ref targetVelocity);
            if (useVerticalVelocity)
            {
                targetVelocity.y = _rigidbody.velocity.y;
            }

            _rigidbody.velocity = targetVelocity;
        }

        protected virtual void CheckStopMove(ref Vector3 targetVelocity)
        {
            RaycastHit hit;
            Vector3 origin = transform.position + transform.up * colliderRadiusDefault;
            Vector3 direction = moveDirection.normalized;
            direction = Vector3.ProjectOnPlane(direction, groundHit.normal);
            float distance = colliderRadiusDefault + 1;
            float targetStopWeight = 0;
            float smooth = isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth;
            bool checkStopMoveCondition = isGrounded && !isJumping && !isInAirborne && !applyingStepOffset && !customAction;

            if (steepSlopeAhead)
            {
                targetStopWeight = 1f * _slopeSidewaysSmooth;
            }
            else if (checkStopMoveCondition && CheckStopMove(direction, out hit))
            {
                var angle = Vector3.Angle(direction, -hit.normal);
                if (angle < slopeLimit)
                {
                    float dst = hit.distance - colliderRadiusDefault;
                    targetStopWeight = (1.0f - dst);
                }
                else
                {
                    targetStopWeight = -0.01f;
                }

                if (debugWindow)
                {
                    Debug.DrawLine(origin, hit.point, Color.cyan);
                }
            }
            else
            {
                targetStopWeight = -0.01f;
            }
            stopMoveWeight = Mathf.Lerp(stopMoveWeight, targetStopWeight, smooth * Time.deltaTime);
            stopMoveWeight = Mathf.Clamp(stopMoveWeight, 0f, 1f);

            targetVelocity = Vector3.LerpUnclamped(targetVelocity, Vector3.zero, stopMoveWeight);
        }

        protected virtual bool CheckStopMove(Vector3 direction, out RaycastHit hit)
        {
            Vector3 origin = transform.position + transform.up * colliderRadiusDefault;
            float distance = colliderRadiusDefault + stopMoveRayDistance;
            switch (stopMoveCheckMethod)
            {
                case StopMoveCheckMethod.SphereCast:

                case StopMoveCheckMethod.CapsuleCast:
                    Vector3 p1 = origin + transform.up * (slopeLimitHeight);
                    Vector3 p2 = origin + transform.up * (stopMoveMaxHeight - _capsuleCollider.radius);
                    return Physics.CapsuleCast(p1, p2, _capsuleCollider.radius, direction, out hit, distance, stopMoveLayer);
                default:
                    return Physics.Raycast(origin, direction, out hit, distance, stopMoveLayer);
            }
        }

        protected virtual void SnapToGround(ref Vector3 targetVelocity, ref bool useVerticalVelocity)
        {
            if (!useSnapGround || disableCheckGround || (isRolling))
            {
                return;
            }

            if (groundDistance < groundMinDistance * 0.2f || applyingStepOffset)
            {
                // Debug.Log($"Return = ValidGroundDistance: {(groundDistance < groundMinDistance * 0.2f).ToStringColor()} inStepOffset: { (applyingStepOffset.ToStringColor())}");
                return;
            }

            var snapConditions = isGrounded && groundHit.collider != null && GroundAngle() <= slopeLimit && !disableCheckGround && !isSliding && !isJumping && !customAction && input.magnitude > 0.1f && !isInAirborne;

            //Debug.Log($"SnapCoditions = Grounded: { (isGrounded).ToStringColor() } GroundHIT: { (groundHit.collider != null).ToStringColor()} InSlopLimit: {(GroundAngle() <= slopeLimit).ToStringColor()} NotDisableCheckGround: { (!disableCheckGround).ToStringColor() } NotIsSliding: {(!isSliding).ToStringColor()} NotIsJumping: {(!isJumping).ToStringColor()} NotCustomAction: {(!customAction).ToStringColor()} HasMovementInput: {(input.magnitude > 0.1f).ToStringColor()} NotIsInAirBorne: {(!isInAirborne).ToStringColor()} ");
            if (snapConditions)
            {
                var distanceToGround = Mathf.Max(0.0f, groundDistance);
                var snapVelocity = transform.up * (-distanceToGround * snapPower / Time.fixedDeltaTime);
                targetVelocity = (targetVelocity + snapVelocity).normalized * targetVelocity.magnitude;
                useVerticalVelocity = false;
            }
        }

        protected virtual void CalculateStepOffset(Vector3 moveDir, ref Vector3 targetVelocity, ref bool useVerticalVelocity)
        {
            if (useStepOffset && isGrounded && !disableCheckGround && !isSliding && !isJumping && !customAction && !isInAirborne)
            {
                Vector3 dir = Vector3.Lerp(transform.forward, moveDir.normalized, inputSmooth.magnitude);
                float distance = _capsuleCollider.radius + stepOffsetDistance;
                float height = (stepOffsetMaxHeight + 0.01f + _capsuleCollider.radius * 0.5f);
                Vector3 pA = transform.position + transform.up * (stepOffsetMinHeight + 0.05f);
                Vector3 pB = pA + dir.normalized * distance;
                if (Physics.Linecast(pA, pB, out stepOffsetHit, stepOffsetLayer))
                {
                    if (debugWindow)
                    {
                        Debug.DrawLine(pA, stepOffsetHit.point);
                    }

                    distance = stepOffsetHit.distance + 0.1f;
                }
                Ray ray = new Ray(transform.position + transform.up * height + dir.normalized * distance, Vector3.down);

                if (Physics.SphereCast(ray, _capsuleCollider.radius * 0.5f, out stepOffsetHit, (stepOffsetMaxHeight - stepOffsetMinHeight), stepOffsetLayer) && stepOffsetHit.point.y > transform.position.y)
                {
                    dir = (stepOffsetHit.point) - transform.position;
                    dir.Normalize();
                    targetVelocity = Vector3.Project(targetVelocity, dir);
                    applyingStepOffset = true;
                    useVerticalVelocity = false;
                    return;
                }
            }

            applyingStepOffset = false;
        }

        public virtual void StopCharacterWithLerp()
        {
            isSprinting = false;
            sprintWeight = 0f;
            horizontalSpeed = 0f;
            verticalSpeed = 0f;
            moveDirection = Vector3.zero;
            input = Vector3.Lerp(input, Vector3.zero, 2f * Time.fixedDeltaTime);
            inputSmooth = Vector3.Lerp(inputSmooth, Vector3.zero, 2f * Time.fixedDeltaTime);
            if (!_rigidbody.isKinematic)
                _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, Vector3.zero, 4f * Time.fixedDeltaTime);
            inputMagnitude = Mathf.Lerp(inputMagnitude, 0f, 2f * Time.fixedDeltaTime);
            moveSpeed = Mathf.Lerp(moveSpeed, 0f, 2f * Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputMagnitude, 0f, 0.2f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputVertical, 0f, 0.2f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputHorizontal, 0f, 0.2f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.RotationMagnitude, 0f, 0.2f, Time.fixedDeltaTime);
        }

        public virtual void StopCharacter()
        {
            isSprinting = false;
            sprintWeight = 0f;
            horizontalSpeed = 0f;
            verticalSpeed = 0f;
            moveDirection = Vector3.zero;
            input = Vector3.zero;
            inputSmooth = Vector3.zero;
            if (!_rigidbody.isKinematic)
                _rigidbody.velocity = Vector3.zero;
            inputMagnitude = 0f;
            moveSpeed = 0f;
            animator.SetFloat(vAnimatorParameters.InputMagnitude, 0f, 0.25f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputVertical, 0f, 0.25f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.InputHorizontal, 0f, 0.25f, Time.fixedDeltaTime);
            animator.SetFloat(vAnimatorParameters.RotationMagnitude, 0f, 0.25f, Time.fixedDeltaTime);
        }

        public virtual void RotateToPosition(Vector3 position)
        {
            Vector3 desiredDirection = position - transform.position;
            RotateToDirection(desiredDirection.normalized);
        }

        public virtual void RotateToDirection(Vector3 direction)
        {
            RotateToDirection(direction, isStrafing ? strafeSpeed.rotationSpeed : freeSpeed.rotationSpeed);
        }

        public virtual void RotateToDirection(Vector3 direction, float rotationSpeed)
        {
            if (lockAnimRotation || customAction || (!jumpAndRotate && !isGrounded) || ragdolled || isSliding)
            {
                return;
            }

            direction.y = 0f;
            if (direction.normalized.magnitude == 0)
            {
                direction = transform.forward;
            }

            var euler = transform.rotation.eulerAngles.NormalizeAngle();
            var targetEuler = Quaternion.LookRotation(direction.normalized).eulerAngles.NormalizeAngle();
            euler.y = Mathf.LerpAngle(euler.y, targetEuler.y, rotationSpeed * Time.fixedDeltaTime);
            Quaternion _newRotation = Quaternion.Euler(euler);
            transform.rotation = _newRotation;
        }

        /// <summary>
        /// Check if <see cref="input"/> and <see cref="inputSmooth"/> has some value greater than 0.1f
        /// </summary>
        public virtual bool hasMovementInput
        {
            get => ((inputSmooth.sqrMagnitude + input.sqrMagnitude) > 0.1f || (input - inputSmooth).sqrMagnitude > 0.1f);
        }

        #endregion

        #region Jump Methods

        protected virtual void ControlJumpBehaviour()
        {
            if (!isJumping)
            {
                return;
            }

            jumpCounter -= Time.fixedDeltaTime;
            if (jumpCounter <= 0)
            {
                jumpCounter = 0;
                isJumping = false;
            }
            // apply extra force to the jump height   
            var vel = _rigidbody.velocity;
            vel.y = jumpHeight * jumpMultiplier;
            _rigidbody.velocity = vel;
        }

        public virtual void SetJumpMultiplier(float jumpMultiplier)
        {
            this.jumpMultiplier = jumpMultiplier;
        }

        public virtual void SetJumpMultiplier(float jumpMultiplier, float timeToReset = 1f)
        {
            this.jumpMultiplier = jumpMultiplier;

            if (timeToResetJumpMultiplier <= 0)
            {
                timeToResetJumpMultiplier = timeToReset;
                StartCoroutine(ResetJumpMultiplierRoutine());
            }
            else
            {
                timeToResetJumpMultiplier = timeToReset;
            }
        }

        public virtual void ResetJumpMultiplier()
        {
            StopCoroutine("ResetJumpMultiplierRoutine");
            timeToResetJumpMultiplier = 0;
            jumpMultiplier = 1;
        }

        protected virtual IEnumerator ResetJumpMultiplierRoutine()
        {

            while (timeToResetJumpMultiplier > 0 && jumpMultiplier != 1 && (isJumping || !isGrounded))
            {
                timeToResetJumpMultiplier -= Time.fixedDeltaTime;
                yield return null;
            }
            timeToResetJumpMultiplier = 0;
            jumpMultiplier = 1;
        }

        public virtual void AirControl()
        {
            if ((isGrounded && !isJumping) || isSliding || ragdolled || _rigidbody.isKinematic || customAction)
            {
                return;
            }
            if (transform.position.y > heightReached)
            {
                heightReached = transform.position.y;
            }

            inputSmooth = Vector3.Lerp(inputSmooth, input, airSmooth * Time.fixedDeltaTime);

            if (jumpWithRigidbodyForce && !isGrounded)
            {
                _rigidbody.AddForce(moveDirection * airSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
                return;
            }
            var _moveDirection = moveDirection;
            _moveDirection.y = 0;
            _moveDirection.x = Mathf.Clamp(moveDirection.x, -1f, 1f);
            _moveDirection.z = Mathf.Clamp(moveDirection.z, -1f, 1f);
            moveDirection = _moveDirection;
            Vector3 targetPosition = _rigidbody.position + (moveDirection * airSpeed) * Time.fixedDeltaTime;
            Vector3 targetVelocity = (targetPosition - transform.position) / Time.fixedDeltaTime;

            targetVelocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, targetVelocity, airSmooth * Time.fixedDeltaTime);
        }

        protected virtual bool jumpFwdCondition
        {
            get
            {
                Vector3 p1 = transform.position + _capsuleCollider.center + Vector3.up * -_capsuleCollider.height * 0.5F;
                Vector3 p2 = p1 + Vector3.up * _capsuleCollider.height;
                return Physics.CapsuleCastAll(p1, p2, _capsuleCollider.radius * 0.5f, transform.forward, 0.6f, groundLayer).Length == 0;
            }
        }

        #endregion

        #region Crouch Methods

        public virtual void UseAutoCrouch(bool value)
        {
            autoCrouch = value;
        }

        public virtual void AutoCrouch()
        {
            if (autoCrouch)
            {
                isCrouching = true;
            }

            if (autoCrouch && !inCrouchArea && CanExitCrouch())
            {
                autoCrouch = false;
                isCrouching = false;
            }
        }

        public virtual bool CanExitCrouch()
        {
            if (isCrouching)
            {
                // radius of SphereCast
                float radius = _capsuleCollider.radius * 0.9f;
                // Position of SphereCast origin stating in base of capsule
                Vector3 pos = transform.position + Vector3.up * ((colliderHeight * 0.5f) - colliderRadius);
                // ray for SphereCast
                Ray ray2 = new Ray(pos, Vector3.up);
                // sphere cast around the base of capsule for check ground distance
                if (Physics.SphereCast(ray2, radius, out groundHit, crouchHeadDetect - (colliderRadius * 0.1f), autoCrouchLayer))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return true;
        }

        protected virtual void AutoCrouchExit(Collider other)
        {
            if (other.CompareTag("AutoCrouch"))
            {
                inCrouchArea = false;
            }
        }

        protected virtual void CheckForAutoCrouch(Collider other)
        {
            if (other.gameObject.CompareTag("AutoCrouch"))
            {
                autoCrouch = true;
                inCrouchArea = true;
            }
        }

        #endregion

        #region Roll Methods

        public virtual bool canRollAgain
        {
            get
            {

                return isRolling && animatorStateInfos.GetCurrentNormalizedTime(0) >= timeToRollAgain;
            }
        }

        protected virtual void RollBehavior()
        {
            if (!isRolling)
            {
                return;
            }

            if (rollControl)
            {
                // calculate input smooth
                inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
            }

            // rotation
            RotateToDirection(moveDirection, rollRotationSpeed);

            // movement
            Vector3 deltaPosition = useRollRootMotion ? new Vector3(animator.deltaPosition.x, 0f, animator.deltaPosition.z) : transform.forward * Time.deltaTime;
            Vector3 v = ((deltaPosition * (rollSpeed > 0 ? rollSpeed : 1f)) / Time.deltaTime) * (1f - stopMoveWeight);
            if (rollUseGravity && animator.GetNormalizedTime(baseLayer) >= rollUseGravityTime)
            {
                v.y = _rigidbody.velocity.y;
            }

            _rigidbody.velocity = v;
        }

        #endregion

        #region Ground Check                

        protected virtual void CheckGround()
        {
            CheckGroundDistance();
            SlideOnSteepSlope();
            ControlMaterialPhysics();

            if (isDead || customAction || disableCheckGround || isSliding)
            {
                if (!isDead || isGrounded)
                {
                    isGrounded = true;
                    heightReached = transform.position.y;
                    return;
                }
            }

            if (groundDistance <= groundMinDistance || applyingStepOffset)
            {
                CheckFallDamage();
                isGrounded = true;
                if (!useSnapGround && !applyingStepOffset && !isJumping && groundDistance > 0.05f && extraGravity != 0)
                {
                    _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.fixedDeltaTime), ForceMode.VelocityChange);
                }

                heightReached = transform.position.y;
            }
            else
            {
                if (groundDistance >= groundMaxDistance)
                {
                    if (!isRolling)
                        isGrounded = false;

                    // check vertical velocity
                    verticalVelocity = _rigidbody.velocity.y;
                    // apply extra gravity when falling
                    if (!applyingStepOffset && !isJumping && extraGravity != 0)
                    {
                        _rigidbody.AddForce(transform.up * extraGravity * Time.fixedDeltaTime, ForceMode.VelocityChange);
                    }
                }
                else if (!applyingStepOffset && !isJumping && extraGravity != 0)
                {
                    _rigidbody.AddForce(transform.up * (extraGravity * 2 * Time.fixedDeltaTime), ForceMode.VelocityChange);
                }
            }
        }

        protected virtual void CheckFallDamage()
        {
            if (isGrounded || verticalVelocity > fallMinVerticalVelocity || !_canApplyFallDamage || fallMinHeight == 0 || fallDamage == 0)
            {
                return;
            }

            float fallHeight = (heightReached - transform.position.y);

            fallHeight -= fallMinHeight;
            if (fallHeight > 0)
            {
                int damage = (int)(fallDamage * fallHeight);
                TakeDamage(new vDamage(damage, true));
            }
        }

        protected virtual void ControlMaterialPhysics()
        {
            // change the physics material to very slip when not grounded
            var targetMaterialPhysics = currentMaterialPhysics;

            if (isGrounded && input.magnitude < 0.1f && !isSliding && targetMaterialPhysics != maxFrictionPhysics)
            {
                targetMaterialPhysics = maxFrictionPhysics;
            }
            else if ((isGrounded && input.magnitude > 0.1f) && !isSliding && targetMaterialPhysics != frictionPhysics)
            {
                targetMaterialPhysics = frictionPhysics;
            }
            else if (targetMaterialPhysics != slippyPhysics && (isSliding || !isGrounded))
            {
                targetMaterialPhysics = slippyPhysics;
            }

            if (currentMaterialPhysics != targetMaterialPhysics)
            {
                _capsuleCollider.material = targetMaterialPhysics;
                currentMaterialPhysics = targetMaterialPhysics;
            }
        }

        protected virtual void CheckGroundDistance()
        {
            if (isDead && isGrounded)
            {
                return;
            }

            if (_capsuleCollider != null)
            {
                // radius of the SphereCast
                float radius = _capsuleCollider.radius * 0.9f;
                var dist = groundDetectionDistance;
                // ray for RayCast
                Ray ray2 = new Ray(transform.position + new Vector3(0, colliderHeight / 2, 0), Vector3.down);
                // raycast for check the ground distance
                if (Physics.Raycast(ray2, out groundHit, (colliderHeight / 2) + dist, groundLayer) && !groundHit.collider.isTrigger)
                {
                    dist = transform.position.y - groundHit.point.y;
                }
                // sphere cast around the base of the capsule to check the ground distance
                if (groundCheckMethod == GroundCheckMethod.High && dist >= groundMinDistance)
                {
                    Vector3 pos = transform.position + Vector3.up * (_capsuleCollider.radius);
                    Ray ray = new Ray(pos, -Vector3.up);
                    if (Physics.SphereCast(ray, radius, out groundHit, _capsuleCollider.radius + groundMaxDistance, groundLayer) && !groundHit.collider.isTrigger)
                    {
                        Physics.Linecast(groundHit.point + (Vector3.up * 0.1f), groundHit.point + Vector3.down * 0.15f, out groundHit, groundLayer);
                        float newDist = transform.position.y - groundHit.point.y;
                        if (dist > newDist)
                        {
                            dist = newDist;
                        }
                    }
                }
                groundDistance = (float)System.Math.Round(dist, 2);
            }
        }

        /// <summary>
        /// Return the ground angle
        /// </summary>
        /// <returns></returns>
        public virtual float GroundAngle()
        {
            var groundAngle = Vector3.Angle(groundHit.normal, Vector3.up);
            return groundAngle;
        }

        /// <summary>
        /// Return the angle of ground based on movement direction
        /// </summary>
        /// <returns></returns>
        public virtual float GroundAngleFromDirection()
        {
            var dir = isStrafing && input.magnitude > 0 ? (transform.right * input.x + transform.forward * input.z).normalized : transform.forward;
            var movementAngle = Vector3.Angle(dir, groundHit.normal) - 90;
            return movementAngle;
        }

        /// <summary>
        /// Prototype to align capsule collider with surface normal
        /// </summary>
        protected virtual void AlignWithSurface()
        {
            Ray ray = new Ray(transform.position, -transform.up);
            RaycastHit hit;
            var surfaceRot = transform.rotation;

            if (Physics.Raycast(ray, out hit, 1.5f, groundLayer))
            {
                surfaceRot = Quaternion.FromToRotation(transform.up, hit.normal) * transform.localRotation;
            }
            transform.rotation = Quaternion.Lerp(transform.rotation, surfaceRot, 10f * Time.fixedDeltaTime);
        }

        protected virtual bool CheckForSlope(ref Vector3 targetVelocity)
        {
            if (debugWindow)
            {
                Debug.DrawLine(transform.position + Vector3.up * (_capsuleCollider.height * slopeLimitHeight), transform.position + moveDirection.normalized *
                    (steepSlopeAhead ? _capsuleCollider.radius + slopeMaxDistance : _capsuleCollider.radius + slopeMinDistance), Color.red, 0.01f);
            }

            if (!useSlopeLimit || moveDirection.magnitude == 0f || targetVelocity.magnitude == 0f)
            {
                _slopeSidewaysSmooth = 1f;
                return false;
            }

            // DYNAMIC LINE
            if (Physics.Linecast(transform.position + Vector3.up * (_capsuleCollider.height * slopeLimitHeight), transform.position + moveDirection.normalized *
                (steepSlopeAhead ? _capsuleCollider.radius + slopeMaxDistance : _capsuleCollider.radius + slopeMinDistance), out slopeHitInfo, groundLayer))
            {
                var hitAngle = Vector3.Angle(Vector3.up, slopeHitInfo.normal);

                if (hitAngle > slopeLimit && hitAngle < 85f)
                {
                    var normal = slopeHitInfo.normal;
                    normal.y = 0f;
                    var normalAngle = targetVelocity.normalized.AngleFormOtherDirection(-normal.normalized);
                    var dir = Quaternion.AngleAxis(normalAngle.y > 0f ? 90f : -90, Vector3.up) * normal.normalized * targetVelocity.magnitude;

                    if (Mathf.Abs(normalAngle.y) > stopSlopeMargin)
                    {
                        _slopeSidewaysSmooth = Mathf.Clamp(_slopeSidewaysSmooth - Time.deltaTime * slopeSidewaysSmooth, 0f, 1f);
                    }
                    else
                    {
                        _slopeSidewaysSmooth = 1f;
                    }

                    targetVelocity = Vector3.Lerp(dir, Vector3.zero, _slopeSidewaysSmooth);
                    return true;
                }
            }

            _slopeSidewaysSmooth = 1f;
            return false;
        }

        protected virtual void SlideOnSteepSlope()
        {
            if (useSlide && isGrounded && GroundAngle() > slopeLimit && !disableCheckGround)
            {
                if (_slidingEnterTime <= 0f || isSliding)
                {
                    var normal = groundHit.normal;
                    normal.y = 0f;
                    var dir = Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized;

                    if (!Physics.Raycast(transform.position + Vector3.up * groundMinDistance, dir, groundMaxDistance, groundLayer))
                    {
                        isSliding = true;
                    }
                    //else
                    //{
                    //    isSliding = true;
                    //}
                }
                else
                {
                    _slidingEnterTime -= Time.fixedDeltaTime;
                }
            }
            else
            {
                _rotateSlopeEnterTime = rotateSlopeEnterTime;
                _slidingEnterTime = isGrounded ? slidingEnterTime : 0f;
                isSliding = false;
            }
        }

        protected virtual void SlideMovementBehavior()
        {
            if (!isSliding)
            {
                return;
            }

            var normal = groundHit.normal;
            normal.y = 0f;

            var dir = Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized;

            if (debugWindow)
            {
                Debug.DrawRay(transform.position, dir * slideDownVelocity);
            }

            _rigidbody.velocity = Vector3.Lerp(_rigidbody.velocity, dir * slideDownVelocity, slideDownSmooth * Time.fixedDeltaTime);
            dir.y = 0f;

            if (_rotateSlopeEnterTime <= 0f)
            {
                Vector3 desiredForward = Vector3.RotateTowards(transform.forward, dir, rotateDownSlopeSmooth * Time.fixedDeltaTime, 0f);
                Quaternion _newRotation = Quaternion.LookRotation(desiredForward);
                _rigidbody.MoveRotation(_newRotation);

                var rightMovement = transform.InverseTransformDirection(moveDirection);
                rightMovement.y = 0f;
                rightMovement.z = 0f;
                rightMovement = transform.TransformDirection(rightMovement);
                if (debugWindow)
                {
                    Debug.DrawRay(transform.position, rightMovement * slideSidewaysVelocity, Color.blue);
                }

                _rigidbody.AddForce(rightMovement * slideSidewaysVelocity, ForceMode.VelocityChange);

                if (debugWindow)
                {
                    Debug.DrawRay(transform.position, Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized, Color.blue);
                    Debug.DrawRay(transform.position, Quaternion.AngleAxis(90, groundHit.normal) * Vector3.ProjectOnPlane(normal.normalized, groundHit.normal).normalized, Color.red);
                    Debug.DrawRay(transform.position, transform.TransformDirection(rightMovement.normalized * 2f), Color.green);
                }
            }
            else
            {
                _rotateSlopeEnterTime -= Time.fixedDeltaTime;
            }
        }

        #endregion

        #region Colliders Check

        public virtual void ControlCapsuleHeight()
        {
            if (isCrouching && !isRolling)
            {
                _capsuleCollider.center = colliderCenter / crouchHeightReduction;
                _capsuleCollider.height = colliderHeight / crouchHeightReduction;
                _capsuleCollider.radius = colliderRadius * crouchColliderRadius;
            }
            else if (isRolling || isRolling && isCrouching)
            {
                _capsuleCollider.center = colliderCenter / rollHeightReduction;
                _capsuleCollider.height = colliderHeight / rollHeightReduction;
                _capsuleCollider.radius = colliderRadius * rollColliderRadius;
            }
            else
            {
                // back to the original values
                _capsuleCollider.center = colliderCenter;
                _capsuleCollider.radius = colliderRadius;
                _capsuleCollider.height = colliderHeight;
            }
        }

        /// <summary>
        /// Reset Capsule Height, Radius and Center to default values
        /// </summary>
        public virtual void ResetCapsule()
        {
            colliderCenter = colliderCenterDefault;
            colliderRadius = colliderRadiusDefault;
            colliderHeight = colliderHeightDefault;
        }

        /// <summary>
        /// Disables rigibody gravity, turn the capsule collider trigger and reset all input from the animator.
        /// </summary>
        public virtual void DisableGravityAndCollision()
        {
            animator.SetFloat("InputHorizontal", 0f);
            animator.SetFloat("InputVertical", 0f);
            animator.SetFloat("VerticalVelocity", 0f);
            //Disable gravity and collision
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            _capsuleCollider.isTrigger = true;
        }

        /// <summary>
        /// Turn rigidbody gravity on the uncheck the capsulle collider as Trigger
        /// </summary>      
        public virtual void EnableGravityAndCollision()
        {
            // Enable collision and gravity
            _capsuleCollider.isTrigger = false;
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;
        }

        #endregion

        #region Ragdoll 

        protected virtual void CheckRagdoll()
        {
            if (ragdollVelocity == 0)
            {
                return;
            }

            // check your verticalVelocity and assign a value on the variable RagdollVel at the Player Inspector
            if (verticalVelocity <= ragdollVelocity && groundDistance <= 0.1f && _canApplyFallDamage && !ragdolled)
            {
                onActiveRagdoll.Invoke(null);
            }
        }

        public override void ResetRagdoll()
        {
            onDisableRagdoll.Invoke();
            verticalVelocity = 0f;
            ragdolled = false;
            _rigidbody.WakeUp();
            _rigidbody.useGravity = true;
            _rigidbody.isKinematic = false;
            _capsuleCollider.isTrigger = false;
            _capsuleCollider.enabled = true;
        }

        public override void EnableRagdoll()
        {
            StopCharacter();
            animator.SetFloat("InputHorizontal", 0f);
            animator.SetFloat("InputVertical", 0f);
            animator.SetFloat("InputMagnitude", 0f);
            animator.SetFloat("VerticalVelocity", 0f);
            ragdolled = true;
            _capsuleCollider.isTrigger = true;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            lockAnimMovement = true;
        }

        #endregion

        #region Debug

        public delegate void GetDebugDelegate(ref System.Text.StringBuilder stringBuilder);

        public event GetDebugDelegate OnDebug;
        public virtual string DebugInfo(string additionalText = "")
        {
            string debugInfo = string.Empty;
            if (debugWindow)
            {
                float delta = Time.smoothDeltaTime;
                float fps = 1 / delta;

                debugInfo =
                    " \n" +
                    "FPS " + fps.ToString("#,##0 fps") + "\n" +
                    "Health = " + currentHealth.ToString() + "\n" +
                    "Input Vertical = " + inputSmooth.z.ToString("0.0") + "\n" +
                    "Input Horizontal = " + inputSmooth.x.ToString("0.0") + "\n" +
                    "Input Magnitude = " + inputMagnitude.ToString("0.0") + "\n" +
                    "Rotation Magnitude = " + rotationMagnitude.ToString("0.0") + "\n" +
                    "Vertical Velocity = " + verticalVelocity.ToString("0.00") + "\n" +
                    "Current MoveSpeed = " + moveSpeed.ToString("0.00") + "\n" +
                    "Ground Distance = " + groundDistance.ToString("0.00") + "\n" +
                    "Ground Angle = " + GroundAngleFromDirection().ToString("0.00") + "\n" +
                    "Is Grounded = " + BoolToRichText(isGrounded) + "\n" +
                    "Is Strafing = " + BoolToRichText(isStrafing) + "\n" +
                    "Is Trigger = " + BoolToRichText(_capsuleCollider.isTrigger) + "\n" +
                    "Use Gravity = " + BoolToRichText(_rigidbody.useGravity) + "\n" +
                    "Is Kinematic = " + BoolToRichText(_rigidbody.isKinematic) + "\n" +
                    "Lock Movement = " + BoolToRichText(lockMovement) + "\n" +
                    "Lock AnimMov = " + BoolToRichText(lockAnimMovement) + "\n" +
                    "Lock Rotation = " + BoolToRichText(lockRotation) + "\n" +
                    "Lock AnimRot = " + BoolToRichText(lockAnimRotation) + "\n" +
                    "--- Actions Bools ---" + "\n" +
                    "Is Sliding = " + BoolToRichText(isSliding) + "\n" +
                    "Is Sprinting = " + BoolToRichText(isSprinting) + "\n" +
                    "Is Crouching = " + BoolToRichText(isCrouching) + "\n" +
                    "Is Rolling = " + BoolToRichText(isRolling) + "\n" +
                    "Is Jumping = " + BoolToRichText(isJumping) + "\n" +
                    "Is Airborne = " + BoolToRichText(isInAirborne) + "\n" +
                    "Is Ragdolled = " + BoolToRichText(ragdolled) + "\n" +
                    "CustomAction = " + BoolToRichText(customAction) + "\n" + additionalText;
            }
            if (OnDebug != null)
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                OnDebug.Invoke(ref stringBuilder);
                debugInfo += stringBuilder.ToString();
            }
            return debugInfo;
        }

        protected virtual string BoolToRichText(bool value)
        {
            return value ? "<color=yellow> True </color>" : "<color=red> False </color>";
        }

        protected virtual void OnDrawGizmos()
        {
            if (Application.isPlaying && debugWindow)
            {
                // debug auto crouch
                Vector3 posHead = transform.position + Vector3.up * ((colliderHeight * 0.5f) - colliderRadius);
                Ray ray1 = new Ray(posHead, Vector3.up);
                Gizmos.DrawWireSphere(ray1.GetPoint((crouchHeadDetect - (colliderRadius * 0.1f))), colliderRadius * 0.9f);
            }

        }

        #endregion

        [System.Serializable]
        public class vMovementSpeed
        {
            [vHelpBox("Higher means faster/responsive movement, lower means smooth movement")]
            [Range(1f, 20f)]
            [FormerlySerializedAs("movementSmooth")]
            [SerializeField] public float movementSmooth = 6f;
            [vHelpBox("Lower means faster transitions between animations, higher means slower")]
            [Range(0f, 1f)]
            [FormerlySerializedAs("animationSmooth")]
            [SerializeField] public float animationSmooth = 0.2f;
            [Tooltip("Rotation speed of the character")]
            [FormerlySerializedAs("rotationSpeed")]
            [SerializeField] public float rotationSpeed = 20f;
            [Tooltip("Character will limit the movement to walk instead of running")]
            [FormerlySerializedAs("walkByDefault")]
            [SerializeField] public bool walkByDefault = false;
            [Tooltip("Rotate with the Camera forward when standing idle")]
            [FormerlySerializedAs("rotateWithCamera")]
            [SerializeField] public bool rotateWithCamera = false;
            [Tooltip("Speed to Walk using rigidbody or extra speed if you're using RootMotion")]
            [FormerlySerializedAs("walkSpeed")]
            [SerializeField] public float walkSpeed = 2f;
            [Tooltip("Speed to Run using rigidbody or extra speed if you're using RootMotion")]
            [FormerlySerializedAs("runningSpeed")]
            [SerializeField] public float runningSpeed = 4f;
            [Tooltip("Speed to Sprint using rigidbody or extra speed if you're using RootMotion")]
            [FormerlySerializedAs("sprintSpeed")]
            [SerializeField] public float sprintSpeed = 6f;
            [Tooltip("Speed to Crouch using rigidbody or extra speed if you're using RootMotion")]
            [FormerlySerializedAs("crouchSpeed")]
            [SerializeField] public float crouchSpeed = 2f;

            public float defaultMovementSmooth { get; set; }
            public float defaultAnimationSmooth { get; set; }
            public float defaultRotationSpeed { get; set; }
            public bool defaultWalkByDefault { get; set; }
            public bool defaultRotateWithCamera { get; set; }
            public float defaultWalkSpeed { get; set; }
            public float defaultRunningSpeed { get; set; }
            public float defaultSprintSpeed { get; set; }
            public float defaultCrouchSpeed { get; set; }

            public void Init()
            {
                defaultMovementSmooth = movementSmooth;
                defaultAnimationSmooth = animationSmooth;
                defaultRotationSpeed = rotationSpeed;
                defaultWalkByDefault = walkByDefault;
                defaultRotateWithCamera = rotateWithCamera;
                defaultWalkSpeed = walkSpeed;
                defaultRunningSpeed = runningSpeed;
                defaultSprintSpeed = sprintSpeed;
                defaultCrouchSpeed = crouchSpeed;
            }

            public void ResetToDefault()
            {
                movementSmooth = defaultMovementSmooth;
                animationSmooth = defaultAnimationSmooth;
                rotationSpeed = defaultRotationSpeed;
                walkByDefault = defaultWalkByDefault;
                rotateWithCamera = defaultRotateWithCamera;
                walkSpeed = defaultWalkSpeed;
                runningSpeed = defaultRunningSpeed;
                sprintSpeed = defaultSprintSpeed;
                crouchSpeed = defaultCrouchSpeed;
            }
        }
    }
}