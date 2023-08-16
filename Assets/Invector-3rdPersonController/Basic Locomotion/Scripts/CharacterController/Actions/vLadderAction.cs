using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace Invector.vCharacterController.vActions
{
    [vClassHeader("Ladder Action", iconName = "ladderIcon")]
    public class vLadderAction : vActionListener
    {
        #region public variables

        [vEditorToolbar("Settings", overrideChildOrder: true, order = 0)]
        [Tooltip("Tag of the object you want to access")]
        public string actionTag = "LadderTrigger";
        [Tooltip("Speed multiplier for the climb ladder animations")]
        public float climbSpeed = 1.5f;
        [Tooltip("Speed multiplier for the climb ladder animations when the fastClimbInput is pressed")]
        public float fastClimbSpeed = 3f;
        [Tooltip("How much Stamina will be consumed when climbing faster")]
        public float fastClimbStamina = 30f;
        [Tooltip("Input to use the ladder going up or down")]
        public GenericInput verticalInput = new GenericInput("Vertical", "LeftAnalogVertical", "Vertical");
        [Tooltip("Input to enter the ladder")]
        public GenericInput enterInput = new GenericInput("E", "A", "A");
        [Tooltip("Input to exit the ladder")]
        public GenericInput exitInput = new GenericInput("Space", "B", "B");
        [Tooltip("Input to climb faster")]
        public GenericInput fastClimbInput = new GenericInput("LeftShift", "LeftStickClick", "LeftStickClick");
        [Tooltip("Input to climb faster")]
        public GenericInput slideDownInput = new GenericInput("Q", "X", "X");


        [vEditorToolbar("Events")]
        public UnityEvent OnEnterLadder;
        public UnityEvent OnExitLadder;
        public UnityEvent OnEnterTriggerLadder;
        public UnityEvent OnExitTriggerLadder;


        [vEditorToolbar("Debug")]
        public bool debugMode;
        [vReadOnly(false)]
        [SerializeField]
        protected vTriggerLadderAction targetLadderAction;
        [vReadOnly(false)]
        [SerializeField]
        protected vTriggerLadderAction currentLadderAction;
        //[vReadOnly(false)]
        //[SerializeField]
        protected List<vTriggerLadderAction> actionTriggers = new List<vTriggerLadderAction>();
        [vReadOnly(false)]
        [SerializeField]
        protected float _speed;
        protected virtual float speed { get { return _speed; } set { _speed = value; } }
        [vReadOnly(false)]
        [SerializeField]
        protected float currentClimbSpeed;
        [vReadOnly(false)]
        [SerializeField]
        protected bool _isUsingLadder;
        public virtual bool isUsingLadder { get { return _isUsingLadder; } set { _isUsingLadder = value; } }
        [vReadOnly(false)]
        [SerializeField]
        protected bool enterLadderStarted;
        [vReadOnly(false)]
        [SerializeField]
        protected bool inEnterLadderAnimation;
        [vReadOnly(false)]
        [SerializeField]
        protected bool inExitingLadderAnimation;
        [vReadOnly(false)]
        [SerializeField]
        protected bool triggerEnterOnce;
        [vReadOnly(false)]
        [SerializeField]
        protected bool triggerExitOnce;

        #endregion

        protected vThirdPersonInput tpInput;
        protected bool isAligningMidAir;

        protected override void SetUpListener()
        {
            actionEnter = false;
            actionStay = true;
            actionExit = true;
        }

        protected override void Start()
        {
            base.Start();
            tpInput = GetComponent<vThirdPersonInput>();
            if (tpInput)
            {
                tpInput.onUpdate -= UpdateLadderBehavior;
                tpInput.onUpdate += UpdateLadderBehavior;

                tpInput.onAnimatorMove -= UsingLadder;
                tpInput.onAnimatorMove += UsingLadder;
            }
        }

        protected virtual void UpdateLadderBehavior()
        {
            AutoEnterLadder();
            EnterLadderInput();
            ExitLadderInput();
        }

        protected virtual void EnterLadderInput()
        {
            if (targetLadderAction == null || tpInput.cc.customAction || tpInput.cc.isJumping || !tpInput.cc.isGrounded || tpInput.cc.isRolling)
            {
                return;
            }

            if (enterInput.GetButtonDown() && !enterLadderStarted && !isUsingLadder && !targetLadderAction.autoAction)
            {
                TriggerEnterLadder();
            }
        }

        protected virtual void ExitLadderInput()
        {
            if (!isUsingLadder)
            {
                return;
            }

            if (tpInput.cc.baseLayerInfo.IsName("EnterLadderTop") || tpInput.cc.baseLayerInfo.IsName("EnterLadderBottom"))
            {
                return;
            }

            if (targetLadderAction == null)
            {
                if (tpInput.cc.IsAnimatorTag("ClimbLadder"))
                {
                    if (slideDownInput.GetButtonDown() && !inExitingLadderAnimation)
                    {
                        tpInput.cc.animator.CrossFadeInFixedTime("Ladder_SlideDown", 0.2f);
                    }

                    // exit ladder at any moment by pressing the cancelInput
                    if (exitInput.GetButtonDown())
                    {
                        if (debugMode)
                        {
                            Debug.Log("Quick Exit..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
                        }
                        tpInput.cc.animator.speed = 1;
                        tpInput.cc.animator.CrossFadeInFixedTime("QuickExitLadder", 0.1f);
                        Invoke("ResetPlayerSettings", .5f);
                    }
                }
            }
            else
            {
                currentLadderAction = targetLadderAction;
                var animationClip = targetLadderAction.exitAnimation;
                if (animationClip == "ExitLadderBottom")
                {
                    // exit ladder when reach the bottom by pressing the cancelInput or pressing down at
                    if (exitInput.GetButtonDown() && !triggerExitOnce || (speed <= -0.05f && !triggerExitOnce) || (tpInput.cc.IsAnimatorTag("LadderSlideDown") && targetLadderAction != null && !triggerExitOnce))
                    {
                        if (debugMode)
                        {
                            Debug.Log("Exit Bottom..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
                        }
                        triggerExitOnce = true;
                        tpInput.cc.animator.CrossFadeInFixedTime(targetLadderAction.exitAnimation, 0.1f);             // trigger the animation clip        
                    }
                }
                else if (animationClip == "ExitLadderTop" && tpInput.cc.IsAnimatorTag("ClimbLadder"))    // exit the ladder from the top
                {
                    if ((speed >= 0.05f) && !triggerExitOnce && !tpInput.cc.animator.IsInTransition(0))         // trigger the exit animation by pressing up
                    {
                        if (debugMode)
                        {
                            Debug.Log("Exit Top..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
                        }
                        triggerExitOnce = true;
                        tpInput.cc.animator.CrossFadeInFixedTime(targetLadderAction.exitAnimation, 0.1f);             // trigger the animation clip
                    }
                }
            }
        }

        protected virtual void AutoEnterLadder()
        {
            if (targetLadderAction == null || !targetLadderAction.autoAction)
            {
                return;
            }

            if (tpInput.cc.customAction || isUsingLadder || tpInput.cc.animator.IsInTransition(0))
            {
                return;
            }

            // enter the ladder automatically if checked with autoAction
            if (targetLadderAction.autoAction && tpInput.cc.input != Vector3.zero && !tpInput.cc.customAction)
            {
                var inputDir = Camera.main.transform.TransformDirection(new Vector3(tpInput.cc.input.x, 0f, tpInput.cc.input.z));
                inputDir.y = 0f;
                var dist = Vector3.Distance(inputDir.normalized, targetLadderAction.transform.forward);
                if (dist < 0.8f)
                {
                    TriggerEnterLadder();
                }
            }
        }

        public virtual void TriggerMidAirEnterLadder(vTriggerLadderMiddle ladderMiddle)
        {
            if (tpInput.cc.isGrounded || isAligningMidAir) return;

            isAligningMidAir = true;

            Vector3 pos = ladderMiddle.refTarget.position;
            bool pointFounded = false;
            float distance = Vector3.Distance(pos, transform.position);
            int steps = (int)(ladderMiddle._collider.size.y / ladderMiddle.stepHeight);

            for (int i = 0; i < steps; i++)
            {
                pos = ladderMiddle.refTarget.position + ladderMiddle.transform.up * ladderMiddle.stepHeight * i;
                var _distance = Vector3.Distance(pos, transform.position);

                if (_distance <= distance)
                {
                    distance = _distance;
                    pointFounded = true;
                }
                else if (pointFounded)
                {
                    break;
                }
            }

            tpInput.cc.DisableGravityAndCollision();
            tpInput.cc.isCrouching = false;
            tpInput.cc.ControlCapsuleHeight();
            tpInput.UpdateCameraStates();
            tpInput.cc.UpdateAnimator();

            triggerEnterOnce = true;
            enterLadderStarted = true;
            tpInput.cc.animator.SetInteger(vAnimatorParameters.ActionState, 1);     // set actionState 1 to avoid falling transitions

            tpInput.cc.StopCharacter();
            tpInput.SetLockAllInput(true);
            tpInput.cc.ResetInputAnimatorParameters();

            var localPos = ladderMiddle.transform.InverseTransformPoint(transform.position);
            if (localPos.x >= 0f)
                tpInput.cc.animator.CrossFadeInFixedTime("IdleLadder_Left", 0.25f);     // trigger the action animation clip
            else
                tpInput.cc.animator.CrossFadeInFixedTime("IdleLadder_Right", 0.25f);     // trigger the action animation clip

            tpInput.cc.disableAnimations = true;

            StartCoroutine(AlignToMiddleLadder(pos, ladderMiddle.refTarget.rotation));
        }

        IEnumerator AlignToMiddleLadder(Vector3 pos, Quaternion rot)
        {
            float time = 0f;
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;

            while (time <= 1f)
            {
                transform.position = Vector3.Lerp(position, pos, time);
                transform.rotation = Quaternion.Lerp(rotation, rot, time);

                time += Time.fixedDeltaTime * 8f;

                yield return null;
            }

            yield return new WaitForEndOfFrame();

            transform.position = pos;
            transform.rotation = rot;

            isUsingLadder = true;
            isAligningMidAir = false;
        }

        protected virtual void TriggerEnterLadder()
        {
            if (debugMode)
            {
                Debug.Log("Enter Ladder");
            }

            OnExitTriggerLadder.Invoke();

            if (targetLadderAction.targetCharacterParent)
            {
                transform.parent = targetLadderAction.targetCharacterParent;
            }

            tpInput.cc.isCrouching = false;
            tpInput.cc.ControlCapsuleHeight();
            tpInput.UpdateCameraStates();
            tpInput.cc.UpdateAnimator();
            OnEnterLadder.Invoke();
            triggerEnterOnce = true;
            enterLadderStarted = true;
            tpInput.cc.animator.SetInteger(vAnimatorParameters.ActionState, 1);     // set actionState 1 to avoid falling transitions
            tpInput.SetLockAllInput(true);
            tpInput.cc.ResetInputAnimatorParameters();
            targetLadderAction.OnDoAction.Invoke();
            currentLadderAction = targetLadderAction;
            //tpInput.cc.animator.updateMode = AnimatorUpdateMode.Normal;
            if (!string.IsNullOrEmpty(currentLadderAction.playAnimation))
            {
                if (debugMode)
                {
                    Debug.Log("TriggerAnimation " + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
                }

                tpInput.cc.animator.CrossFadeInFixedTime(currentLadderAction.playAnimation, 0.25f);     // trigger the action animation clip                                           
                isUsingLadder = true;
                tpInput.cc.disableAnimations = true;
                tpInput.cc.StopCharacter();

            }
        }

        protected virtual void UsingLadder()
        {
            if (!isUsingLadder)
            {
                return;
            }

            // update the base layer to know what animations are being played
            tpInput.cc.AnimatorLayerControl();
            tpInput.cc.ActionsControl();
            // update camera movement
            tpInput.CameraInput();

            // go up or down 
            speed = verticalInput.GetAxis();
            tpInput.cc.animator.SetFloat(vAnimatorParameters.InputVertical, speed, 0.1f, Time.deltaTime);
            if (speed >= 0.05f || speed <= -0.05f)
            {
                tpInput.cc.animator.speed = Mathf.Lerp(tpInput.cc.animator.speed, currentClimbSpeed, 2f * Time.deltaTime);
            }
            else
            {
                tpInput.cc.animator.speed = Mathf.Lerp(tpInput.cc.animator.speed, 1f, 2f * Time.deltaTime);
            }

            // increase speed by input and consume stamina
            if (fastClimbInput.GetButton() && tpInput.cc.currentStamina > 0)
            {
                currentClimbSpeed = fastClimbSpeed;
                StaminaConsumption();
            }
            else
            {
                currentClimbSpeed = climbSpeed;
            }

            // enter ladder behaviour 
            var _inEnterLadderAnimation = tpInput.cc.baseLayerInfo.IsName("EnterLadderTop") || tpInput.cc.baseLayerInfo.IsName("EnterLadderBottom") && !tpInput.cc.animator.IsInTransition(0);

            if (_inEnterLadderAnimation)
            {
                this.inEnterLadderAnimation = true;

                tpInput.cc.DisableGravityAndCollision();              // disable gravity & turn collision trigger
                // disable ingame hud
                if (currentLadderAction != null)
                {
                    currentLadderAction.OnPlayerExit.Invoke();
                }

                if (currentLadderAction.useTriggerRotation)
                {

                    if (debugMode)
                    {
                        Debug.Log("Rotating to target..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
                    }
                    EvaluateToRotation(currentLadderAction.enterRotationCurve, currentLadderAction.matchTarget.transform.rotation, tpInput.cc.baseLayerInfo.normalizedTime);
                }

                if (currentLadderAction.matchTarget != null)
                {
                    if (transform.parent != currentLadderAction.targetCharacterParent)
                    {
                        transform.parent = currentLadderAction.targetCharacterParent;
                    }

                    if (debugMode)
                    {
                        Debug.Log("Match Target to Enter..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
                    }

                    EvaluateToPosition(currentLadderAction.enterPositionXZCurve, currentLadderAction.enterPositionYCurve, currentLadderAction.matchTarget.position, tpInput.cc.baseLayerInfo.normalizedTime);
                }
            }

            if (!_inEnterLadderAnimation && inEnterLadderAnimation)
            {
                enterLadderStarted = false;
                inEnterLadderAnimation = false;
            }
            TriggerExitLadder();
        }

        protected virtual void TriggerExitLadder()
        {
            // exit ladder behaviour
            inExitingLadderAnimation = tpInput.cc.baseLayerInfo.IsName("ExitLadderTop") || tpInput.cc.baseLayerInfo.IsName("ExitLadderBottom") || tpInput.cc.baseLayerInfo.IsName("QuickExitLadder");

            if (inExitingLadderAnimation)
            {
                tpInput.cc.animator.speed = 1;

                if (currentLadderAction.exitMatchTarget != null && !tpInput.cc.baseLayerInfo.IsName("QuickExitLadder"))
                {
                    if (debugMode)
                    {
                        Debug.Log("Match Target to exit..." + currentLadderAction.name + "_" + currentLadderAction.transform.parent.gameObject.name);
                    }

                    EvaluateToPosition(currentLadderAction.exitPositionXZCurve, currentLadderAction.exitPositionYCurve, currentLadderAction.exitMatchTarget.position, tpInput.cc.baseLayerInfo.normalizedTime);
                }
                var newRot = new Vector3(0, tpInput.animator.rootRotation.eulerAngles.y, 0);
                EvaluateToRotation(currentLadderAction.exitRotationCurve, Quaternion.Euler(newRot), tpInput.cc.baseLayerInfo.normalizedTime);

                if (tpInput.cc.baseLayerInfo.normalizedTime >= 0.8f)
                {
                    // after playing the animation we reset some values
                    ResetPlayerSettings();
                }
            }
        }

        protected virtual void EvaluateToPosition(AnimationCurve XZ, AnimationCurve Y, Vector3 targetPosition, float normalizedTime)
        {
            Vector3 rootPosition = tpInput.cc.animator.rootPosition;

            float evaluatedXZ = XZ.Evaluate(normalizedTime);
            float evaluatedY = Y.Evaluate(normalizedTime);

            if (evaluatedXZ < 1f)
            {
                rootPosition.x = Mathf.Lerp(rootPosition.x, targetPosition.x, evaluatedXZ);
                rootPosition.z = Mathf.Lerp(rootPosition.z, targetPosition.z, evaluatedXZ);
            }

            if (evaluatedY < 1f)
            {
                rootPosition.y = Mathf.Lerp(rootPosition.y, targetPosition.y, evaluatedY);
            }

            transform.position = rootPosition;
        }

        protected virtual void EvaluateToRotation(AnimationCurve curve, Quaternion targetRotation, float normalizedTime)
        {
            Quaternion rootRotation = tpInput.cc.animator.rootRotation;
            float evaluatedCurve = curve.Evaluate(normalizedTime);
            if (evaluatedCurve < 1)
            {
                rootRotation = Quaternion.Lerp(rootRotation, targetRotation, evaluatedCurve);
            }
            transform.rotation = rootRotation;
        }

        protected virtual void StaminaConsumption()
        {
            if (tpInput.cc.currentStamina <= 0)
            {
                return;
            }

            tpInput.cc.ReduceStamina(fastClimbStamina, true);        // call the ReduceStamina method from the player
            tpInput.cc.currentStaminaRecoveryDelay = 0.25f;    // delay to start recovery stamina
        }

        protected virtual void AddLadderTrigger(vTriggerLadderAction _ladderAction)
        {
            if (targetLadderAction != _ladderAction)
            {
                targetLadderAction = _ladderAction;
                if (debugMode)
                {
                    Debug.Log("TriggerStay " + targetLadderAction.name + "_" + targetLadderAction.transform.parent.gameObject.name);
                }
            }

            if (!actionTriggers.Contains(targetLadderAction))
            {
                actionTriggers.Add(targetLadderAction);
                targetLadderAction.OnPlayerEnter.Invoke();
            }
        }

        protected virtual void RemoveLadderTrigger(vTriggerLadderAction _ladderAction)
        {
            if (_ladderAction == targetLadderAction)
            {
                targetLadderAction = null;
            }

            if (actionTriggers.Contains(_ladderAction))
            {
                actionTriggers.Remove(_ladderAction);
                _ladderAction.OnPlayerExit.Invoke();
            }
        }

        protected virtual void CheckForTriggerAction(Collider other)
        {
            // assign the component - it will be null when he exit the trigger area
            var _ladderAction = other.GetComponent<vTriggerLadderAction>();
            if (!_ladderAction)
            {
                AddLadderTrigger(_ladderAction);
                return;
            }
            // check the maxAngle too see if the character can do the action
            var dist = Vector3.Distance(transform.forward, _ladderAction.transform.forward);

            if (isUsingLadder && _ladderAction != null)
            {
                if (targetLadderAction != _ladderAction)
                {
                    targetLadderAction = _ladderAction;
                    if (!actionTriggers.Contains(targetLadderAction))
                    {
                        actionTriggers.Add(targetLadderAction);
                    }
                }
            }
            else if ((_ladderAction.activeFromForward == false || dist <= 0.8f) && !isUsingLadder)
            {
                AddLadderTrigger(_ladderAction);
                OnEnterTriggerLadder.Invoke();
            }
            else
            {
                RemoveLadderTrigger(_ladderAction);
            }
        }

        public virtual void ResetPlayerSettings()
        {
            if (debugMode)
            {
                Debug.Log("Reset Player Settings");
            }

            speed = 0f;
            targetLadderAction = null;
            isUsingLadder = false;
            OnExitLadder.Invoke();
            triggerExitOnce = false;
            triggerEnterOnce = false;
            inEnterLadderAnimation = false;
            enterLadderStarted = false;
            tpInput.cc.animator.SetInteger(vAnimatorParameters.ActionState, 0);
            tpInput.cc.EnableGravityAndCollision();
            tpInput.SetLockAllInput(false);
            tpInput.cc.StopCharacter();
            tpInput.cc.disableAnimations = false;
            //tpInput.cc.animator.updateMode = AnimatorUpdateMode.AnimatePhysics;
            if (transform.parent != null)
            {
                transform.parent = null;
            }
        }

        public override void OnActionStay(Collider other)
        {
            if (other.gameObject.CompareTag(actionTag))
            {
                CheckForTriggerAction(other);
            }
        }

        public override void OnActionExit(Collider other)
        {
            if (other.gameObject.CompareTag(actionTag))
            {
                var _ladderAction = other.GetComponent<vTriggerLadderAction>();
                if (!_ladderAction)
                {
                    return;
                }
                RemoveLadderTrigger(_ladderAction);
                if (debugMode)
                {
                    Debug.Log("TriggerExit " + other.name + "_" + other.transform.parent.gameObject.name);
                }

                OnExitTriggerLadder.Invoke();
            }
        }
    }
}