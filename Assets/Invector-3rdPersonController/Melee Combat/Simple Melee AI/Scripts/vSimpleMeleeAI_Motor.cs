using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
    using UnityEngine.AI;
    using vMelee;

    public class vSimpleMeleeAI_Motor : vCharacter
    {
        #region Variables

        #region Layers
        [vEditorToolbar("Layers")]

        [Tooltip("Layers that the character can walk on")]
        public LayerMask groundLayer = 1 << 0;
        [Tooltip("Distance to became not grounded")]
        [SerializeField]
        protected float groundCheckDistance = 0.5f;
        [Tooltip("What objects can make the character auto crouch")]
        public LayerMask autoCrouchLayer = 1 << 0;
        [Tooltip("[SPHERECAST] ADJUST IN PLAY MODE - White Spherecast put just above the head, this will make the character Auto-Crouch if something hit the sphere.")]
        public float headDetect = 0.95f;
        #endregion

        #region AI variables
        [vEditorToolbar("Locomotion")]
        [Tooltip("Use to limit your locomotion animation, if you want to patrol walking set this value to 0.5f")]
        [Range(0f, 1.5f)]
        public float patrolSpeed = 0.5f;
        [Tooltip("Use to limit your locomotion animation, if you want to wander walking set this value to 0.5f")]
        [Range(0f, 1.5f)]
        public float wanderSpeed = 0.5f;
        [Tooltip("Use to limit your locomotion animation, if you want to chase the target walking set this value to 0.5f")]
        [Range(0f, 1.5f)]
        public float chaseSpeed = 1f;
        [Tooltip("Use to limit your locomotion animation, if you want to strafe the target walking set this value to 0.5f")]
        [Range(0f, 1.5f)]
        public float strafeSpeed = 1f;

        [Header("--- Strafe ---")]
        [Tooltip("Strafe around the target")]
        public bool strafeSideways = true;
        [Tooltip("Strafe a few steps backwards")]
        public bool strafeBackward = true;
        [Tooltip("Distance to switch to the strafe locomotion, leave with 0 if you don't want your character to strafe")]
        public float strafeDistance = 3f;
        [Tooltip("Min time to change the strafe direction")]
        public float minStrafeSwape = 2f;
        [Tooltip("Max time to change the strafe direction")]
        public float maxStrafeSwape = 5f;
        [Tooltip("Velocity to rotate the character while strafing")]
        public float strafeRotationSpeed = 5f;

        [vEditorToolbar("Detection")]
        protected AIStates _currentState = AIStates.PatrolWaypoints;
        public virtual AIStates currentState { get { return _currentState; } set { _currentState = value; } }
        [Header("Who is your Target?")]
        public vSimpleMeleeAI_SphereSensor sphereSensor;
        public vTagMask tagsToDetect = new vTagMask { "Player" };
        public LayerMask layersToDetect = 1 << 0;
        public LayerMask obstaclesLayer;
        public bool sortTargetFromDistance = false;
        [Range(0f, 360f)]
        public float fieldOfView = 95f;
        [Tooltip("Max Distance to detect the Target with FOV")]
        public float maxDetectDistance = 5f;
        [Tooltip("Min Distance to noticed the Target without FOV")]
        public float minDetectDistance = 2f;
        [Tooltip("Distance to lost the Target")]
        public float distanceToLostTarget = 5f;
        public float lostTargetDistance { get { return maxDetectDistance + distanceToLostTarget; } }
        [Tooltip("Distance to stop when chasing the Player")]
        public float chaseStopDistance = 1f;
        public bool drawAgentPath = false;
        public bool displayGizmos;

        [vEditorToolbar("Combat")]

        [Tooltip("Check if you want the Enemy to be passive even if you attack him")]
        public bool passiveToDamage = false;
        [Tooltip("Check if you want the Enemy to chase the Target at first sight")]
        public bool agressiveAtFirstSight = true;
        [Tooltip("Velocity to rotate the character while attacking")]
        public float attackRotationSpeed = 0.5f;
        [Tooltip("Delay to trigger the first attack when close to the target")]
        public float firstAttackDelay = 0f;
        [Tooltip("Min frequency to attack")]
        public float minTimeToAttack = 4f;
        [Tooltip("Max frequency to attack")]
        public float maxTimeToAttack = 6f;
        [Tooltip("How many attacks the AI will make on a combo")]
        public int maxAttackCount = 3;
        [Tooltip("Randomly attacks based on the maxAttackCount")]
        public bool randomAttackCount = true;
        [Range(0f, 1f)]
        public float chanceToRoll = .1f;
        [Range(0f, 1f)]
        public float chanceToBlockInStrafe = .1f;
        [Range(0f, 1f)]
        public float chanceToBlockAttack = 0f;
        [Tooltip("How much time the character will stand up the shield")]
        public float raiseShield = 4f;
        [Tooltip("How much time the character will lower the shield")]
        public float lowerShield = 2f;

        [vEditorToolbar("Waypoint")]

        [Tooltip("Max Distance to change waypoint")]
        [Range(0.5f, 100f)]
        public float distanceToChangeWaypoint = 1f;
        [Tooltip("Min Distance to stop when Patrolling through waypoints")]
        [Range(0.5f, 100f)]
        public float patrollingStopDistance = 0.5f;
        public AIPatrolWithOutAreaStyle patrolWithoutAreaStyle = AIPatrolWithOutAreaStyle.GoToStartPoint;
        public vWaypointArea pathArea;
        public bool randomWaypoints;

        public vFisherYatesRandom randomWaypoint = new vFisherYatesRandom();
        public vFisherYatesRandom randomPatrolPoint = new vFisherYatesRandom();
        [HideInInspector]
        public CapsuleCollider _capsuleCollider;
        // there is a prefab of health hud example that you can drag and drop into the head bone of your character
        [HideInInspector]
        public v_SpriteHealth healthSlider;
        // attach a meleeManager component to create new hitboxs and set up different weapons
        [HideInInspector]
        public vMeleeManager meleeManager;
        // check your MeleeWeapon Inspector, each weapon can set up different distances to attack        
        public OnSetAgressiveEvent onSetAgressive = new OnSetAgressiveEvent();
        public class OnSetAgressiveEvent : UnityEngine.Events.UnityEvent<bool> { }
        [HideInInspector]
        public bool lockMovement;

        public enum AIPatrolWithOutAreaStyle
        {
            GoToStartPoint,
            Wander,
            Idle
        }

        public enum AIStates
        {
            Idle,
            PatrolSubPoints,
            PatrolWaypoints,
            Wander,
            Chase
        }

        #endregion

        #region Protected Variables
        public CharacterTarget currentTarget;// { protected set; get; }
        [System.Serializable]
        public struct CharacterTarget
        {
            public Transform transform;
            public vIHealthController character;
            public Collider colliderTarget;
        }
        protected Vector3 _targetPos;
        protected virtual Vector3 targetPos { get { return _targetPos; } set { _targetPos = value; } }
        [SerializeField, vReadOnly]
        protected bool _canSeeTarget;
        protected virtual bool canSeeTarget { get { return _canSeeTarget; } set { _canSeeTarget = value; } }
        protected Vector3 _destination;
        protected virtual Vector3 destination { get { return _destination; } set { _destination = value; } }
        protected Vector3 _fwd;
        protected virtual Vector3 fwd { get { return _fwd; } set { _fwd = value; } }

        protected virtual bool _isGrounded { get; set; }
        protected virtual bool isGrounded { get { return _isGrounded; } set { _isGrounded = value; } }
        protected bool _isStrafing;
        protected virtual bool isStrafing { get { return _isStrafing; } set { _isStrafing = value; } }
        protected virtual bool inResetAttack { get; set; }
        protected virtual bool firstAttack { get; set; }
        protected virtual int attackCount { get; set; }
        protected virtual int _currentWaypoint { get; set; }
        protected virtual int currentWaypoint { get { return _currentWaypoint; } set { _currentWaypoint = value; } }
        protected virtual int _currentPatrolPoint { get; set; }
        protected virtual int currentPatrolPoint { get { return _currentPatrolPoint; } set { _currentPatrolPoint = value; } }
        protected float _direction;
        protected virtual float direction { get { return _direction; } set { _direction = value; } }
        protected float _timer;
        protected virtual float timer { get { return _timer; } set { _timer = value; } }
        protected float _wait;
        protected virtual float wait { get { return _wait; } set { _wait = value; } }

        protected float _fovAngle;
        protected virtual float fovAngle { get { return _fovAngle; } set { _fovAngle = value; } }
        protected virtual float sideMovement { get; set; }
        protected virtual float fwdMovement { get; set; }
        protected virtual float strafeSwapeFrequency { get; set; }
        protected virtual float groundDistance { get; set; }
        protected Vector3 _startPosition;
        protected virtual Vector3 startPosition { get { return _startPosition; } set { _startPosition = value; } }
        protected RaycastHit groundHit;
        protected NavMeshAgent agent;
        protected NavMeshPath agentPath;
        protected Quaternion freeRotation;
        protected Quaternion desiredRotation;
        protected Vector3 oldPosition;
        protected Vector3 combatMovement;
        protected Vector3 rollDirection;
        protected Rigidbody _rigidbody;
        protected PhysicMaterial frictionPhysics;
        protected Transform head;
        protected Collider colliderTarget;
        protected vWaypoint targetWaypoint;
        protected vPoint targetPatrolPoint;
        protected List<vPoint> visitedPatrolPoint = new List<vPoint>();
        protected List<vWaypoint> visitedWaypoint = new List<vWaypoint>();
        #endregion

        #region Actions
        protected bool _isCrouched;
        protected bool _canAttack;
        protected bool _tryingBlock;
        protected bool _isRolling;
        protected virtual bool isCrouched { get { return _isCrouched; } set { _isCrouched = value; } }
        protected virtual bool canAttack { get { return _canAttack; } set { _canAttack = value; } }
        protected virtual bool tryingBlock { get { return _tryingBlock; } set { _tryingBlock = value; } }
        protected virtual bool isRolling { get { return _isRolling; } set { _isRolling = value; } }
        protected bool _isBlocking;
        public virtual bool isBlocking { get { return _isBlocking; } protected set { _isBlocking = value; } }
        protected bool _isAttacking;
        public bool isAttacking { get { return _isAttacking; } protected set { _isAttacking = value; } }

        public bool isArmed { get { return meleeManager != null && (meleeManager.rightWeapon != null || (meleeManager.leftWeapon != null && meleeManager.leftWeapon.meleeType != vMeleeType.OnlyDefense)); } }

        protected bool _actions;
        public virtual bool actions { get { return _actions; } set { _actions = value; } }

        #endregion

        #endregion


        public override void Init()
        {
            base.Init();

            fwd = transform.forward;
            destination = transform.position;
            agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

            agentPath = new UnityEngine.AI.NavMeshPath();
            sphereSensor = GetComponentInChildren<vSimpleMeleeAI_SphereSensor>();
            if (sphereSensor)
            {
                sphereSensor.root = transform;
            }
            meleeManager = GetComponent<vMeleeManager>();
            canAttack = true;
            attackCount = 0;
            sideMovement = GetRandonSide();
            destination = transform.position;

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = true;
            _rigidbody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.enabled = false;
            _capsuleCollider = GetComponent<CapsuleCollider>();

            // avoid collision detection with inside colliders 
            Collider[] AllColliders = this.GetComponentsInChildren<Collider>();
            Collider thisCollider = GetComponent<Collider>();
            for (int i = 0; i < AllColliders.Length; i++)
            {
                Physics.IgnoreCollision(thisCollider, AllColliders[i]);
            }

            healthSlider = GetComponentInChildren<v_SpriteHealth>();
            head = animator.GetBoneTransform(HumanBodyBones.Head);
            oldPosition = transform.position;
            currentHealth = maxHealth;
            startPosition = transform.position;
        }

        #region AI Locomotion
        public float distanceToAttack
        {
            get { if (meleeManager) { return meleeManager.GetAttackDistance(); } return 1f; }
        }

        public bool OnCombatArea
        {
            get
            {
                if (currentTarget.transform == null)
                {
                    return false;
                }

                var inFloor = Vector3.Distance(new Vector3(0, transform.position.y, 0), new Vector3(0, currentTarget.transform.position.y, 0)) < distanceToAttack;
                return (inFloor && agressiveAtFirstSight && TargetDistance <= strafeDistance && !agent.isOnOffMeshLink);
            }
        }

        public bool OnStrafeArea
        {
            get
            {
                if (!canSeeTarget)
                {
                    isStrafing = false;
                    return false;
                }

                if (currentTarget.transform == null || !agressiveAtFirstSight)
                {
                    return false;
                }

                var inFloor = Vector3.Distance(new Vector3(0, transform.position.y, 0), new Vector3(0, currentTarget.transform.position.y, 0)) < 1.5f;
                // exit strafe 
                if (isStrafing)
                {
                    isStrafing = TargetDistance < (strafeDistance + 2f);
                }
                // enter strafe                 
                else
                {
                    isStrafing = OnCombatArea;
                }

                return inFloor ? isStrafing : false;
            }
        }

        public bool AgentDone()
        {
            if (!agent.enabled && agent.updatePosition)
            {
                return true;
            }

            return !agent.pathPending && AgentStopping() && agent.updatePosition;
        }

        public bool AgentStopping()
        {
            if (!agent.enabled || !agent.isOnNavMesh)
            {
                return true;
            }

            return agent.remainingDistance <= agent.stoppingDistance;
        }

        public int GetRandonSide()
        {
            var side = UnityEngine.Random.Range(-1, 1);
            if (side < 0)
            {
                side = -1;
            }
            else
            {
                side = 1;
            }

            return side;
        }

        protected void CheckGroundDistance()
        {
            if (_capsuleCollider != null)
            {
                var dist = 10f;
                Ray ray1 = new Ray(transform.position + new Vector3(0, _capsuleCollider.height / 2, 0), Vector3.down);

                if (Physics.Raycast(ray1, out groundHit, _capsuleCollider.height * 0.75f, groundLayer))
                {
                    dist = transform.position.y - groundHit.point.y;
                }

                groundDistance = dist;

                if (!actions && !isRolling && groundDistance < 0.3f)
                {
                    if (currentHealth > 0)
                    {
                        UnityEngine.AI.NavMeshHit navHit;
                        if (UnityEngine.AI.NavMesh.SamplePosition(transform.position, out navHit, 5f, UnityEngine.AI.NavMesh.AllAreas))
                        {
                            _rigidbody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                            _rigidbody.useGravity = false;
                            agent.updatePosition = false;
                            agent.enabled = true;
                        }
                    }

                    if (agent.enabled && agent.isOnNavMesh && Vector3.Distance(agent.nextPosition, transform.position) <= 0.1f)
                    {
                        agent.updatePosition = true;
                    }
                    else if (agent.enabled && agent.isOnNavMesh && !agent.updatePosition)
                    {
                        agent.nextPosition = transform.position;
                    }
                }

                if (!agent.isOnNavMesh && groundDistance > 0.3f && !ragdolled)
                {
                    _rigidbody.useGravity = true;
                    _rigidbody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
                    agent.enabled = false;
                    agent.updatePosition = false;
                }
            }
        }

        public void CheckAutoCrouch()
        {
            // radius of SphereCast
            float radius = _capsuleCollider.radius * 0.9f;
            // Position of SphereCast origin stating in base of capsule
            Vector3 pos = transform.position + Vector3.up * ((_capsuleCollider.height * 0.5f) - _capsuleCollider.radius);
            // ray for SphereCast
            Ray ray2 = new Ray(pos, Vector3.up);
            RaycastHit groundHit;
            // sphere cast around the base of capsule for check ground distance
            //if (Physics.SphereCast(ray2, radius, out groundHit, _capsuleCollider.bounds.max.y - (_capsuleCollider.radius * 0.1f), groundLayer))
            if (Physics.SphereCast(ray2, radius, out groundHit, headDetect - (_capsuleCollider.radius * 0.1f), autoCrouchLayer))
            {
                isCrouched = true;
            }
            else
            {
                isCrouched = false;
            }
        }

        #endregion

        #region Check Target       

        /// <summary>
        /// Calculate Fov Angle
        /// </summary>
        /// <returns></returns>
        public bool onFovAngle()
        {
            if (currentTarget.transform == null)
            {
                return false;
            }

            var freeRotation = Quaternion.LookRotation(currentTarget.transform.position - transform.position, Vector3.up);
            var newAngle = freeRotation.eulerAngles - transform.eulerAngles;
            fovAngle = newAngle.NormalizeAngle().y;

            if (fovAngle < fieldOfView && fovAngle > -fieldOfView)
            {
                return true;
            }

            return false;
        }

        int canSeeTargetIteration;
        /// <summary>
        /// Target Detection
        /// </summary>
        /// <param name="_target"></param>
        /// <returns></returns>
        public void CheckTarget()
        {
            if (currentTarget.transform == null || !agressiveAtFirstSight)
            {
                canSeeTarget = false;
                canSeeTargetIteration = 0;
                return;
            }

            if (TargetDistance > maxDetectDistance)
            {
                canSeeTarget = false;
                canSeeTargetIteration = 0;
                return;
            }

            if (currentTarget.colliderTarget == null || currentTarget.colliderTarget.transform != currentTarget.transform)
            {
                currentTarget.colliderTarget = currentTarget.transform.GetComponent<Collider>();
            }

            if (currentTarget.colliderTarget == null)
            {
                canSeeTarget = false;
                canSeeTargetIteration = 0;
                return;
            }
            var top = new Vector3(currentTarget.colliderTarget.bounds.center.x, currentTarget.colliderTarget.bounds.max.y, currentTarget.colliderTarget.bounds.center.z);
            var bottom = new Vector3(currentTarget.colliderTarget.bounds.center.x, currentTarget.colliderTarget.bounds.min.y, currentTarget.colliderTarget.bounds.center.z);
            var offset = Vector3.Distance(top, bottom) * 0.15f;
            top.y -= offset;
            bottom.y += offset;

            if (!onFovAngle() && TargetDistance > minDetectDistance)
            {
                canSeeTarget = false;
                canSeeTargetIteration = 0;
                return;
            }

            RaycastHit hit;
            if (canSeeTargetIteration == 0 && !Physics.Linecast(head.position, top, out hit, obstaclesLayer))
            {

                canSeeTarget = true;
                canSeeTargetIteration = 0;
                return;
            }

            else if (canSeeTargetIteration == 1 && !Physics.Linecast(head.position, bottom, out hit, obstaclesLayer))
            {
                canSeeTarget = true;
                canSeeTargetIteration = 0;
                return;
            }
            else if (canSeeTargetIteration == 2 && !Physics.Linecast(head.position, currentTarget.colliderTarget.bounds.center, out hit, obstaclesLayer))
            {
                canSeeTarget = true;
                canSeeTargetIteration = 0;
                return;
            }
            else
            {
                canSeeTargetIteration++;
            }

            if (canSeeTargetIteration > 1)
            {
                canSeeTargetIteration = 0;
            }

            canSeeTarget = false;
        }

        public float TargetDistance
        {
            get
            {
                if (currentTarget.transform != null)
                {
                    return currentTarget.colliderTarget ? Vector3.Distance(_capsuleCollider.bounds.center, currentTarget.colliderTarget.bounds.center) :
                        Vector3.Distance(transform.position, currentTarget.transform.position);
                }

                return maxDetectDistance + 1f;
            }
        }

        public Transform headTarget
        {
            get
            {
                if (currentTarget.transform != null && currentTarget.transform.GetComponent<Animator>() != null)
                {
                    return currentTarget.transform.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion

        #region AI Health       

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

            if (agent != null)
            {
                Destroy(agent);
            }

            var comps = GetComponents<MonoBehaviour>();
            for (int i = 0; i < comps.Length; i++)
            {
                Destroy(comps[i]);
            }
        }

        /// <summary>
        /// TAKE DAMAGE - you can override the take damage method from the vCharacter and add your own calls 
        /// </summary>
        /// <param name="damage"> damage to apply </param>
        public override void TakeDamage(vDamage damage)
        {
            // ignore damage if the character is rolling, dead or the animator is disable
            if (isRolling || currentHealth <= 0 || !animator.enabled)
            {
                return;
            }

            if (!damage.ignoreDefense && !actions && CheckChanceToRoll())
            {
                return;
            }

            base.TakeDamage(damage);
        }

        protected override void TriggerDamageReaction(vDamage damage)
        {
            if (!isRolling)
            {
                base.TriggerDamageReaction(damage);
            }
        }
        #endregion

        #region AI Melee Combat Methods

        protected bool CheckChanceToRoll()
        {
            if (isAttacking || actions)
            {
                return false;
            }

            float randomRoll = UnityEngine.Random.value;
            if (randomRoll < chanceToRoll && randomRoll > 0 && currentTarget.transform != null)
            {
                animator.SetTrigger("ResetState");
                sideMovement = GetRandonSide();
                Ray ray = new Ray(currentTarget.transform.position, currentTarget.transform.right * sideMovement);
                rollDirection = ray.direction;
                animator.CrossFadeInFixedTime("Roll", 0.1f);

                return true;
            }
            return false;
        }

        protected IEnumerator CheckChanceToBlock(float chance, float timeToEnter)
        {
            tryingBlock = true;
            float randomBlock = UnityEngine.Random.value;
            if (randomBlock < chance && randomBlock > 0 && !isBlocking)
            {
                if (timeToEnter > 0)
                {
                    yield return new WaitForSeconds(timeToEnter);
                }

                isBlocking = currentTarget.transform == null || (actions) || isAttacking ? false : true;
                StartCoroutine(ResetBlock());
                tryingBlock = false;
            }
            else
            {
                tryingBlock = false;
            }
        }

        protected IEnumerator ResetBlock()
        {
            yield return new WaitForSeconds(currentTarget.transform == null ? 0 : raiseShield);
            isBlocking = false;
        }

        protected virtual void SetAggressive(bool value)
        {
            agressiveAtFirstSight = value;//
            onSetAgressive.Invoke(value);
        }

        int GetDamageResult(int damage, float defenseRate)
        {
            int result = (int)(damage - ((damage * defenseRate) / 100));
            return result;
        }

        #endregion

        #region Ragdoll

        public override void ResetRagdoll()
        {
            oldPosition = transform.position;
            ragdolled = false;
            _capsuleCollider.isTrigger = false;
            _rigidbody.isKinematic = false;
            agent.updatePosition = false;
            agent.enabled = true;
        }

        public override void EnableRagdoll()
        {
            agent.enabled = false;
            agent.updatePosition = false;
            ragdolled = true;
            _rigidbody.isKinematic = true;
            _capsuleCollider.isTrigger = true;
        }

        #endregion
    }
}