using System.Collections;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
    public class vSimpleMeleeAI_Companion : vSimpleMeleeAI_Controller
    {
        [vEditorToolbar("Companion")]
        [SerializeField] protected string _companionTag = "Player";
        public virtual string companionTag { get { return _companionTag; } set { _companionTag = value; } }
        public virtual float companionMaxDistance { get { return _companionMaxDistance; } set { _companionMaxDistance = value; } }
        [SerializeField] protected float _companionMaxDistance = 10f;
        [Range(0f, 1.5f)]
        [SerializeField] protected float _followSpeed = 1f;
        public virtual float followSpeed { get { return _followSpeed; } set { _followSpeed = value; } }
        [SerializeField] protected float _followStopDistance = 2f;
        public virtual float followStopDistance { get { return _followStopDistance; } set { _followStopDistance = value; } }
        [Range(0f, 1.5f)]
        [SerializeField] protected float _moveToStopDistance = 0.5f;
        public virtual float moveToStopDistance { get { return _moveToStopDistance; } set { _moveToStopDistance = value; } }
        [SerializeField] protected Transform _moveToTarget;
        public virtual Transform moveToTarget { get { return _moveToTarget; } set { _moveToTarget = value; } }
        [SerializeField] protected CompanionState _companionState = CompanionState.Follow;
        public virtual CompanionState companionState { get { return _companionState; } set { _companionState = value; } }
        [SerializeField] protected Transform _companion;
        public virtual Transform companion { get { return _companion; } set { _companion = value; } }

        public bool debug = true;
        public UnityEngine.UI.Text debugUIText;

        public enum CompanionState
        {
            None, // this state works with AiController normal routine
            Follow,
            MoveTo,
            Stay
        }

        protected void LateUpdate()
        {
            CompanionInputs();
        }

        protected virtual void CompanionInputs()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                companionState = CompanionState.Stay;
                agressiveAtFirstSight = false;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                companionState = CompanionState.Follow;
                agressiveAtFirstSight = false;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                agressiveAtFirstSight = !agressiveAtFirstSight;
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) && moveToTarget != null)
            {
                SetMoveTo(moveToTarget);
                companionState = CompanionState.MoveTo;
                agressiveAtFirstSight = false;
            }
        }

        /// <summary>
        /// Gets the companion distance.
        /// </summary>
        /// <value>The companion distance.</value>
        protected virtual float companionDistance
        {
            get { return companion != null ? Vector3.Distance(transform.position, companion.transform.position) : 0f; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="vSimpleMeleeAI_Companion"/> is near of companion. Relative to <see cref="companionMaxDistance"/>
        /// </summary>
        /// <value><c>true</c> if near of companion; otherwise, <c>false</c>.</value>
        protected virtual bool nearOfCompanion
        {
            get
            {
                var value = ((companion != null && companion.gameObject.activeSelf && companionDistance < companionMaxDistance) || (companion == null || !companion.gameObject.activeSelf));
                return value;
            }
        }

        /// <summary>
        /// Sets the target Move to.
        /// </summary>
        /// <param name="_target">Target.</param>
        public virtual void SetMoveTo(Transform _target)
        {
            companionState = CompanionState.MoveTo;
            moveToTarget = _target;
        }

        #region Override Ai Controller rotine
        protected override void Start()
        {
            try
            {
                var comp = GameObject.FindGameObjectWithTag(companionTag);
                if (comp != null)
                {
                    companion = comp.transform;
                }
                else
                {
                    companionState = CompanionState.None;
                    Debug.LogWarning("Cant find the " + companionTag);
                }
            }
            catch (UnityException e)
            {
                companionState = CompanionState.None;
                Debug.LogWarning("AICompanion Cant find the " + companionTag);
                Debug.LogWarning("AICompanion " + e.Message);
            }
            Init();
            agent.enabled = true;
            StartCoroutine(CompanionStateRoutine());
            StartCoroutine(FindTarget());
            StartCoroutine(DestinationBehaviour());
        }

        /// <summary>
        /// override <see cref="vSimpleMeleeAI_Companion.StateRoutine()"/>
        /// ps: this rotine work with internal while loop
        /// </summary>
        /// <returns></returns>
        protected IEnumerator CompanionStateRoutine()
        {
            while (this.enabled)
            {
                yield return new WaitForEndOfFrame();
                System.Text.StringBuilder debugString = new System.Text.StringBuilder();
                debugString.AppendLine("----DEBUG----");
                debugString.AppendLine("Agressive : " + agressiveAtFirstSight);

                CheckIsOnNavMesh();
                CheckAutoCrouch();
                SetTarget();

                //Companion Behavior (override Aicontroller Behavior)
                switch (companionState)
                {
                    #region Companion rotine
                    case CompanionState.Follow:
                        if (canSeeTarget && nearOfCompanion)
                        {
                            yield return StartCoroutine(base.Chase());
                        }
                        else
                        {
                            yield return StartCoroutine(FollowCompanion());
                        }

                        debugString.AppendLine(canSeeTarget && nearOfCompanion ? "Chase/Follow" : "Follow");

                        break;
                    case CompanionState.MoveTo:
                        if (canSeeTarget)
                        {
                            yield return StartCoroutine(base.Chase());
                        }
                        else
                        {
                            yield return StartCoroutine(MoveTo());
                        }

                        debugString.AppendLine(canSeeTarget ? "Chase/MoveTo" : "MoveTo");
                        break;
                    case CompanionState.Stay:
                        if (canSeeTarget)
                        {
                            yield return StartCoroutine(base.Chase());
                        }
                        else
                        {
                            yield return StartCoroutine(Stay());
                        }

                        debugString.AppendLine(canSeeTarget ? "Chase/Stay" : "Stay");
                        break;
                    #endregion
                    case CompanionState.None:
                        //Aicontroller Behavior
                        #region Ai controller Normal Rotine
                        debugString.AppendLine("None : using normal AI routine");
                        switch (currentState)
                        {
                            case AIStates.Idle:
                                debugString.AppendLine("idle");
                                yield return StartCoroutine(base.Idle());
                                break;
                            case AIStates.Chase:
                                yield return StartCoroutine(base.Chase());
                                break;
                            case AIStates.Wander:
                                yield return StartCoroutine(base.Wander());
                                break;
                        }
                        break;
                        #endregion
                }
                if (debugUIText != null && debug)
                {
                    debugUIText.text = debugString.ToString();
                }
            }
        }

        /// <summary>
        /// override <see cref="vSimpleMeleeAI_Companion.Idle()"/>
        /// </summary>
        /// <returns></returns>
        protected IEnumerator Stay()
        {
            if (companion != null)
            {
                agent.speed = Mathf.Lerp(agent.speed, 0, 2f * Time.deltaTime);
            }
            else
            {
                yield return StartCoroutine(Idle());
            }
        }

        protected override void SetAggressive(bool value)
        {
            if (companionState != CompanionState.Follow)
            {
                base.SetAggressive(value);
            }
        }

        #endregion

        #region Companion rotine

        /// <summary>
        /// Follows the companion.
        /// </summary>
        /// <returns>The companion.</returns>
        protected virtual IEnumerator FollowCompanion()
        {
            while (!agent.enabled || currentHealth <= 0)
            {
                yield return null;
            }

            // check if companion exist in Scene to work follow rotine
            if (companion != null && companion.gameObject.activeSelf)
            {
                agent.speed = Mathf.Lerp(agent.speed, followSpeed, 10f * Time.deltaTime);
                agent.stoppingDistance = followStopDistance;
                UpdateDestination(companion.position);
            }
            else // go to start position case companion dont exist
            {
                agent.speed = Mathf.Lerp(agent.speed, moveToSpeed, 10f * Time.deltaTime);
                agent.stoppingDistance = moveToStopDistance;
                UpdateDestination(startPosition);
            }
        }

        /// <summary>
        /// Moves to target applied from <see cref="SetMoveTo"/>
        /// </summary>
        /// <returns>The to.</returns>
        protected virtual IEnumerator MoveTo()
        {
            while (!agent.enabled || currentHealth <= 0)
            {
                yield return null;
            }

            agent.speed = Mathf.Lerp(agent.speed, moveToSpeed, 2f * Time.deltaTime);
            agent.stoppingDistance = moveToStopDistance;
            // update destination to moveTo target position
            UpdateDestination(moveToTarget.position);
            // check if can see some target (included from SetUpTarget method)
            if (canSeeTarget && nearOfCompanion)
            {
                currentState = AIStates.Chase;
            }
        }

        protected override float maxSpeed
        {
            get
            {
                if (companionState != CompanionState.None)
                {
                    return companionState == CompanionState.Follow ? followSpeed : companionState == CompanionState.MoveTo ? moveToSpeed : 0;
                }
                return base.maxSpeed;
            }
        }

        #endregion
    }
}