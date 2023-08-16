
using UnityEngine;

namespace Invector.vCharacterController
{
    using vEventSystems;
    public class vThirdPersonAnimator : vThirdPersonMotor
    {
        #region Variables       
        public float randomIdleCount;
        public const float walkSpeed = 0.5f;
        public const float runningSpeed = 1f;
        public const float sprintSpeed = 1.5f;
        public Vector3 lastCharacterPosition { get; protected set; }

        #endregion

#if UNITY_EDITOR
        [ContextMenu("Enable AnimatorStateInfos Debug")]
        void EnableAnimatorStateInfosDebug()
        {
            if (animatorStateInfos != null)
            {
                animatorStateInfos.debug = true;
            }
        }

        [ContextMenu("Disable AnimatorStateInfos Debug")]
        void DisableAnimatorStateInfosDebug()
        {
            if (animatorStateInfos != null)
            {
                animatorStateInfos.debug = false;
            }
        }
#endif
        protected override void Start()
        {
            base.Start();
            RegisterAnimatorStateInfos();
        }

        protected virtual void RegisterAnimatorStateInfos()
        {
            animatorStateInfos = new vAnimatorStateInfos(GetComponent<Animator>());
            animatorStateInfos.RegisterListener();
        }

        protected virtual void OnEnable()
        {
            if (animatorStateInfos.animator != null)
            {
                animatorStateInfos.RegisterListener();
            }
        }

        protected virtual void OnDisable()
        {

            animatorStateInfos.RemoveListener();
        }

        public virtual void UpdateAnimator()
        {
            if (animator == null || !animator.enabled)
            {
                return;
            }

            AnimatorLayerControl();
            ActionsControl();

            TriggerRandomIdle();

            UpdateAnimatorParameters();
            DeadAnimation();
        }

        public virtual void AnimatorLayerControl()
        {
            baseLayerInfo = animator.GetCurrentAnimatorStateInfo(baseLayer);
            underBodyInfo = animator.GetCurrentAnimatorStateInfo(underBodyLayer);
            rightArmInfo = animator.GetCurrentAnimatorStateInfo(rightArmLayer);
            leftArmInfo = animator.GetCurrentAnimatorStateInfo(leftArmLayer);
            upperBodyInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayer);
            fullBodyInfo = animator.GetCurrentAnimatorStateInfo(fullbodyLayer);
        }

        public virtual void ActionsControl()
        {
            // to have better control of your actions, you can assign bools to know if an animation is playing or not
            // this way you can use this bool to create custom behavior for the controller

            // identify if the rolling animations is playing
            isRolling = IsAnimatorTag("IsRolling");
            // identify if a turn on spot animation is playing
            isTurningOnSpot = IsAnimatorTag("TurnOnSpot");
            // locks player movement while a animation with tag 'LockMovement' is playing
            lockAnimMovement = IsAnimatorTag("LockMovement");
            // locks player rotation while a animation with tag 'LockRotation' is playing
            lockAnimRotation = IsAnimatorTag("LockRotation");
            // ! -- you can add the Tag "CustomAction" into a AnimationState and the character will not perform any Melee action -- !            
            customAction = IsAnimatorTag("CustomAction");
            // identify if the controller is airborne
            isInAirborne = IsAnimatorTag("Airborne");
        }

        public virtual void UpdateAnimatorParameters()
        {
            if (disableAnimations)
            {
                return;
            }

            animator.SetBool(vAnimatorParameters.IsStrafing, isStrafing);
            animator.SetBool(vAnimatorParameters.IsSprinting, isSprinting);
            animator.SetBool(vAnimatorParameters.IsSliding, isSliding && !isRolling);
            animator.SetBool(vAnimatorParameters.IsCrouching, isCrouching);
            animator.SetBool(vAnimatorParameters.IsGrounded, isGrounded);
            animator.SetBool(vAnimatorParameters.IsDead, isDead);
            animator.SetFloat(vAnimatorParameters.GroundDistance, groundDistance);
            animator.SetFloat(vAnimatorParameters.GroundAngle, GroundAngleFromDirection());

            if (!isGrounded)
            {
                animator.SetFloat(vAnimatorParameters.VerticalVelocity, verticalVelocity);
            }

            //if (!lockAnimMovement)
            {
                if (isStrafing)
                {
                    animator.SetFloat(vAnimatorParameters.InputHorizontal, horizontalSpeed, strafeSpeed.animationSmooth, Time.fixedDeltaTime);
                    animator.SetFloat(vAnimatorParameters.InputVertical, verticalSpeed, strafeSpeed.animationSmooth, Time.fixedDeltaTime);
                }
                else
                {
                    animator.SetFloat(vAnimatorParameters.InputVertical, verticalSpeed, freeSpeed.animationSmooth, Time.fixedDeltaTime);
                    animator.SetFloat(vAnimatorParameters.InputHorizontal, 0, freeSpeed.animationSmooth, Time.fixedDeltaTime);
                }

                animator.SetFloat(vAnimatorParameters.InputMagnitude, Mathf.LerpUnclamped(inputMagnitude, 0f, stopMoveWeight), isStrafing ? strafeSpeed.animationSmooth : freeSpeed.animationSmooth, Time.fixedDeltaTime);

                if (useLeanMovementAnim && inputMagnitude >= 0.1f)
                {
                    animator.SetFloat(vAnimatorParameters.RotationMagnitude, rotationMagnitude, leanSmooth, Time.fixedDeltaTime);
                }
                else if (useTurnOnSpotAnim && inputMagnitude < 0.1f)
                {
                    animator.SetFloat(vAnimatorParameters.RotationMagnitude, (float)System.Math.Round(rotationMagnitude, 2), turnOnSpotSmooth, Time.fixedDeltaTime);
                }
            }
        }

        public virtual void SetAnimatorMoveSpeed(vMovementSpeed speed)
        {
            Vector3 relativeInput = transform.InverseTransformDirection(moveDirection);
            verticalSpeed = relativeInput.z;
            horizontalSpeed = relativeInput.x;

            var newInput = new Vector2(verticalSpeed, horizontalSpeed);

            if (speed.walkByDefault || alwaysWalkByDefault)
            {
                inputMagnitude = Mathf.Clamp(newInput.magnitude, 0, isSprinting ? runningSpeed : walkSpeed);
            }
            else
            {
                var mag = newInput.magnitude;
                sprintWeight = Mathf.Lerp(sprintWeight, isSprinting ? 1f : 0f, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.fixedDeltaTime);
                inputMagnitude = Mathf.Clamp(Mathf.Lerp(mag, mag + (newInput.magnitude > 0.1f ? 0.5f : 0), sprintWeight), 0, isSprinting ? sprintSpeed : runningSpeed);
            }
        }

        public virtual void SetInputDirection(Vector3 direction)
        {
            float movementDirection = direction.magnitude >= .1 ? transform.forward.AngleFormOtherDirection(direction).y : 0;
            if (vAnimatorParameters.InputDirection != -1) animator.SetFloat(vAnimatorParameters.InputDirection, movementDirection);
        }

        public virtual void ResetInputAnimatorParameters()
        {
            animator.SetBool(vAnimatorParameters.IsSprinting, false);
            animator.SetBool(vAnimatorParameters.IsSliding, false);
            animator.SetBool(vAnimatorParameters.IsCrouching, false);
            animator.SetBool(vAnimatorParameters.IsGrounded, true);
            animator.SetFloat(vAnimatorParameters.GroundDistance, 0f);
            animator.SetFloat("InputHorizontal", 0);
            animator.SetFloat("InputVertical", 0);
            animator.SetFloat("InputMagnitude", 0);
        }

        protected virtual void TriggerRandomIdle()
        {
            if (input != Vector3.zero || customAction)
            {
                return;
            }

            if (randomIdleTime > 0)
            {
                if (input.sqrMagnitude == 0 && !isCrouching && _capsuleCollider.enabled && isGrounded)
                {
                    randomIdleCount += Time.fixedDeltaTime;
                    if (randomIdleCount > 6)
                    {
                        randomIdleCount = 0;
                        animator.SetTrigger(vAnimatorParameters.IdleRandomTrigger);
                        animator.SetInteger(vAnimatorParameters.IdleRandom, Random.Range(1, 4));
                    }
                }
                else
                {
                    randomIdleCount = 0;
                    animator.SetInteger(vAnimatorParameters.IdleRandom, 0);
                }
            }
        }

        protected virtual void DeadAnimation()
        {
            if (!isDead)
            {
                return;
            }

            // death by animation
            if (deathBy == DeathBy.Animation)
            {
                int deadLayer = 0;

                var info = animatorStateInfos.GetStateInfoUsingTag("Dead");
                if (info != null)
                {
                    if (!animator.IsInTransition(deadLayer) && info.normalizedTime >= 0.99f && groundDistance <= 0.15f)
                    {
                        RemoveComponents();
                    }

                }
            }
            // death by animation & ragdoll after a time
            else if (deathBy == DeathBy.AnimationWithRagdoll)
            {
                int deadLayer = 0;
                var info = animatorStateInfos.GetStateInfoUsingTag("Dead");
                if (info != null)
                {
                    if (!animator.IsInTransition(deadLayer) && info.normalizedTime >= 0.8f)
                    {
                        onActiveRagdoll.Invoke(null);
                    }
                }
            }
            // death by ragdoll
            else if (deathBy == DeathBy.Ragdoll)
            {
                onActiveRagdoll.Invoke(null);
            }
        }

        #region Generic Animations Methods

        public virtual void SetActionState(int value)
        {
            animator.SetInteger(vAnimatorParameters.ActionState, value);
        }

        public virtual bool IsAnimatorTag(string tag)
        {
            if (animator == null)
            {
                return false;
            }

            if (animatorStateInfos != null)
            {
                if (animatorStateInfos.HasTag(tag))
                {
                    return true;
                }
            }
            if (baseLayerInfo.IsTag(tag))
            {
                return true;
            }

            if (underBodyInfo.IsTag(tag))
            {
                return true;
            }

            if (rightArmInfo.IsTag(tag))
            {
                return true;
            }

            if (leftArmInfo.IsTag(tag))
            {
                return true;
            }

            if (upperBodyInfo.IsTag(tag))
            {
                return true;
            }

            if (fullBodyInfo.IsTag(tag))
            {
                return true;
            }

            return false;
        }

        public virtual void MatchTarget(Vector3 matchPosition, Quaternion matchRotation, AvatarTarget target, MatchTargetWeightMask weightMask, float normalisedStartTime, float normalisedEndTime)
        {
            if (animator.isMatchingTarget || animator.IsInTransition(0))
            {
                return;
            }

            float normalizeTime = Mathf.Repeat(animator.GetCurrentAnimatorStateInfo(0).normalizedTime, 1f);

            if (normalizeTime > normalisedEndTime)
            {
                return;
            }

            animator.MatchTarget(matchPosition, matchRotation, target, weightMask, normalisedStartTime, normalisedEndTime);
        }

        #endregion

    }

    public static partial class vAnimatorParameters
    {
        public static int InputHorizontal = Animator.StringToHash("InputHorizontal");
        public static int InputVertical = Animator.StringToHash("InputVertical");
        public static int InputMagnitude = Animator.StringToHash("InputMagnitude");
        public static int RotationMagnitude = Animator.StringToHash("RotationMagnitude");
        public static int TurnOnSpotDirection = Animator.StringToHash("TurnOnSpotDirection");
        public static int ActionState = Animator.StringToHash("ActionState");
        public static int ResetState = Animator.StringToHash("ResetState");
        public static int IsDead = Animator.StringToHash("isDead");
        public static int IsGrounded = Animator.StringToHash("IsGrounded");
        public static int IsCrouching = Animator.StringToHash("IsCrouching");
        public static int IsStrafing = Animator.StringToHash("IsStrafing");
        public static int IsSprinting = Animator.StringToHash("IsSprinting");
        public static int IsSliding = Animator.StringToHash("IsSliding");
        public static int GroundDistance = Animator.StringToHash("GroundDistance");
        public static int GroundAngle = Animator.StringToHash("GroundAngle");
        public static int VerticalVelocity = Animator.StringToHash("VerticalVelocity");
        public static int IdleRandom = Animator.StringToHash("IdleRandom");
        public static int IdleRandomTrigger = Animator.StringToHash("IdleRandomTrigger");
        public static int InputDirection = Animator.StringToHash("InputDirection");
    }
}