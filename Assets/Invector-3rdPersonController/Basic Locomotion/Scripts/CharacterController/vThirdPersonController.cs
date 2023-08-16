using System.Collections;
using UnityEngine;

namespace Invector.vCharacterController
{
    [vClassHeader("THIRD PERSON CONTROLLER", iconName = "controllerIcon")]
    public class vThirdPersonController : vThirdPersonAnimator
    {
        /// <summary>
        /// Move the controller to a specific Position, you must Lock the Input first 
        /// </summary>
        /// <param name="targetPosition"></param>
        public virtual void MoveToPosition(Transform targetPosition)
        {
            MoveToPosition(targetPosition.position);
        }

        /// <summary>
        /// Move the controller to a specific Position, you must Lock the Input first 
        /// </summary>
        /// <param name="targetPosition"></param>
        public virtual void MoveToPosition(Vector3 targetPosition)
        {
            Vector3 dir = targetPosition - transform.position;
            dir.y = 0;
            /*dir = dir.normalized * Mathf.Min(1f, dir.magnitude);*/           /*That is to make smootly stop*/

            if (dir.magnitude < 0.1f)
            {
                input = Vector3.zero;
                moveDirection = Vector3.zero;
            }
            else
            {
                input = transform.InverseTransformDirection(dir.normalized);
                moveDirection = dir.normalized;
            }
        }

        /// <summary>
        /// Handle RootMotion movement and specific Actions
        /// </summary>       
        public virtual void ControlAnimatorRootMotion()
        {
            if (!this.enabled)
            {
                return;
            }

            if (isRolling)
            {
                RollBehavior();
                return;
            }

            if (customAction || lockAnimMovement)
            {
                StopCharacterWithLerp();              
                transform.position = animator.rootPosition;
                transform.rotation = animator.rootRotation;
            }

            if (useRootMotion)
            {
                MoveCharacter(moveDirection);
            }
        }

        /// <summary>
        /// Set the Controller movement speed (rigidbody, animator and root motion)
        /// </summary>
        public virtual void ControlLocomotionType()
        {
            if (lockAnimMovement || lockMovement || customAction)
            {
                return;
            }

            if (!lockSetMoveSpeed)
            {
                if (locomotionType.Equals(LocomotionType.FreeWithStrafe) && !isStrafing || locomotionType.Equals(LocomotionType.OnlyFree))
                {
                    SetControllerMoveSpeed(freeSpeed);
                    SetAnimatorMoveSpeed(freeSpeed);
                }
                else if (locomotionType.Equals(LocomotionType.OnlyStrafe) || locomotionType.Equals(LocomotionType.FreeWithStrafe) && isStrafing)
                {
                    isStrafing = true;
                    SetControllerMoveSpeed(strafeSpeed);
                    SetAnimatorMoveSpeed(strafeSpeed);
                }
            }

            if (!useRootMotion)
            {
                MoveCharacter(moveDirection);
            }
        }

        /// <summary>
        /// Manage the Control Rotation Type of the Player
        /// </summary>
        public virtual void ControlRotationType()
        {
            if (lockAnimRotation || lockRotation || customAction || isRolling)
            {
                return;
            }

            bool validInput = input != Vector3.zero || (isStrafing ? strafeSpeed.rotateWithCamera : freeSpeed.rotateWithCamera);

            if (validInput)
            {
                if (lockAnimMovement)
                {
                    // calculate input smooth
                    inputSmooth = Vector3.Lerp(inputSmooth, input, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
                }
                Vector3 dir = (isStrafing && isGrounded && (!isSprinting || sprintOnlyFree == false) || (freeSpeed.rotateWithCamera && input == Vector3.zero)) && rotateTarget ? rotateTarget.forward : moveDirection;

                //RotationTest(dir);

                RotateToDirection(dir);
            }
        }

        /// <summary>
        /// Use it to keep the direction the Player is moving (most used with CCV camera)
        /// </summary>
        public virtual void ControlKeepDirection()
        {
            // update oldInput to compare with current Input if keepDirection is true
            if (!keepDirection)
            {
                oldInput = input;
            }
            else if ((input.magnitude < 0.01f || Vector3.Distance(oldInput, input) > 0.9f) && keepDirection)
            {
                keepDirection = false;
            }
        }

        /// <summary>
        /// Determine the direction the player will face based on input and the referenceTransform
        /// </summary>
        /// <param name="referenceTransform"></param>
        public virtual void UpdateMoveDirection(Transform referenceTransform = null)
        {
            if (isRolling && !rollControl /*|| input.magnitude <= 0.01*/)
            {
                moveDirection = Vector3.Lerp(moveDirection, Vector3.zero, (isStrafing ? strafeSpeed.movementSmooth : freeSpeed.movementSmooth) * Time.deltaTime);
                return;
            }

            if (referenceTransform && !rotateByWorld)
            {
                //get the right-facing direction of the referenceTransform
                var right = referenceTransform.right;
                right.y = 0;
                //get the forward direction relative to referenceTransform Right
                var forward = Quaternion.AngleAxis(-90, Vector3.up) * right;
                // determine the direction the player will face based on input and the referenceTransform's right and forward directions
                moveDirection = (inputSmooth.x * right) + (inputSmooth.z * forward);
                var moveDirectionRaw= (input.x * right) + (input.z * forward);
                SetInputDirection(moveDirectionRaw);
            }
            else
            {
                moveDirection = new Vector3(inputSmooth.x, 0, inputSmooth.z);
                var moveDirectionRaw = new Vector3(input.x, 0, input.z);
                SetInputDirection(moveDirectionRaw);
            }
        }

        /// <summary>
        /// Set the isSprinting bool and manage the Sprint Behavior 
        /// </summary>
        /// <param name="value"></param>
        public virtual void Sprint(bool value)
        {
            var sprintConditions = (!isCrouching || (!inCrouchArea && CanExitCrouch())) && (currentStamina > 0 && hasMovementInput &&
                !(isStrafing && (horizontalSpeed >= 0.5 || horizontalSpeed <= -0.5 || verticalSpeed <= 0.1f) && !sprintOnlyFree));

            if (value && sprintConditions)
            {
                if (currentStamina > (finishStaminaOnSprint ? sprintStamina : 0) && hasMovementInput)
                {
                    finishStaminaOnSprint = false;
                    if (isGrounded && useContinuousSprint)
                    {
                        isCrouching = false;
                        isSprinting = !isSprinting;
                        if (isSprinting)
                        {
                            OnStartSprinting.Invoke();
                            alwaysWalkByDefault = false;
                        }
                        else
                        {
                            OnFinishSprinting.Invoke();
                        }
                    }
                    else if (!isSprinting)
                    {
                        OnStartSprinting.Invoke();

                        alwaysWalkByDefault = false;
                        isSprinting = true;
                    }
                }
                else if (!useContinuousSprint && isSprinting)
                {
                    if (currentStamina <= 0)
                    {
                        finishStaminaOnSprint = true;
                        OnFinishSprintingByStamina.Invoke();
                    }
                    isSprinting = false;
                    OnFinishSprinting.Invoke();
                }
            }
            else if (isSprinting && (!useContinuousSprint || !sprintConditions))
            {
                if (currentStamina <= 0)
                {
                    finishStaminaOnSprint = true;
                    OnFinishSprintingByStamina.Invoke();
                }

                isSprinting = false;
                OnFinishSprinting.Invoke();
            }
        }

        /// <summary>
        /// Manage the isCrouching bool
        /// </summary>
        public virtual void Crouch()
        {
            if (isGrounded && !customAction)
            {
                AutoCrouch();
                if (isCrouching && CanExitCrouch())
                {
                    isCrouching = false;
                }
                else
                {
                    isCrouching = true;
                    isSprinting = false;
                }
            }
        }

        /// <summary>
        /// Set the isStrafing bool
        /// </summary>
        public virtual void Strafe()
        {
            isStrafing = !isStrafing;
        }

        /// <summary>
        /// Triggers the Jump Animation and set the necessary variables to make the Jump behavior in the <seealso cref="vThirdPersonMotor"/>
        /// </summary>
        /// <param name="consumeStamina">Option to consume or not the stamina</param>
        public virtual void Jump(bool consumeStamina = false)
        {
            // trigger jump behaviour
            jumpCounter = jumpTimer;
            OnJump.Invoke();

            // trigger jump animations
            if (input.sqrMagnitude < 0.1f)
            {
                StartCoroutine(DelayToJump());
                animator.CrossFadeInFixedTime("Jump", 0.1f);
            }
            else
            {
                isJumping = true;
                animator.CrossFadeInFixedTime("JumpMove", .2f);
            }

            // reduce stamina
            if (consumeStamina)
            {
                ReduceStamina(jumpStamina, false);
                currentStaminaRecoveryDelay = 1f;
            }
        }

        protected IEnumerator DelayToJump()
        {
            inJumpStarted = true;
            yield return new WaitForSeconds(jumpStandingDelay);
            isJumping = true;
            inJumpStarted = false;
        }

        /// <summary>
        /// Triggers the Roll Animation and set the stamina cost for this action
        /// </summary>
        public virtual void Roll()
        {
            OnRoll.Invoke();
            isRolling = true;
            animator.CrossFadeInFixedTime("Roll", rollTransition, baseLayer);
            ReduceStamina(rollStamina, false);
            currentStaminaRecoveryDelay = 2f;
        }


        #region Check Action Triggers 

        /// <summary>
        /// Call this in OnTriggerEnter or OnTriggerStay to check if enter in triggerActions     
        /// </summary>
        /// <param name="other">collider trigger</param>                         
        protected override void OnTriggerStay(Collider other)
        {
            try
            {
                CheckForAutoCrouch(other);
            }
            catch (UnityException e)
            {
                Debug.LogWarning(e.Message);
            }
            base.OnTriggerStay(other);
        }

        /// <summary>
        /// Call this in OnTriggerExit to check if exit of triggerActions 
        /// </summary>
        /// <param name="other"></param>
        protected override void OnTriggerExit(Collider other)
        {
            AutoCrouchExit(other);
            base.OnTriggerExit(other);
        }

        #endregion
    }
}