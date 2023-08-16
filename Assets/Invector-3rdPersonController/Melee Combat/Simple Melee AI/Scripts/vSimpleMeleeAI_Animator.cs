using System;
using System.Collections;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
    public class vSimpleMeleeAI_Animator : vSimpleMeleeAI_Motor
    {
        #region AI Variables    

        private bool triggerDieBehaviour;
        private bool resetState;
        private float strafeInput;

        // get Layers from the Animator Controller        
        public AnimatorStateInfo baseLayerInfo, rightArmInfo, leftArmInfo, fullBodyInfo, upperBodyInfo, underBodyInfo;

        int baseLayer { get { return animator.GetLayerIndex("Base Layer"); } }
        int underBodyLayer { get { return animator.GetLayerIndex("UnderBody"); } }
        int rightArmLayer { get { return animator.GetLayerIndex("RightArm"); } }
        int leftArmLayer { get { return animator.GetLayerIndex("LeftArm"); } }
        int upperBodyLayer { get { return animator.GetLayerIndex("UpperBody"); } }
        int fullbodyLayer { get { return animator.GetLayerIndex("FullBody"); } }

        #endregion

        public void UpdateAnimator(float _speed, float _direction)
        {
            if (animator == null || !animator.enabled)
            {
                return;
            }

            LayerControl();
            LocomotionAnimation(_speed, _direction);

            RollAnimation();
            CrouchAnimation();

            ResetAndLockAgent();
            MoveSetIDControl();
            MeleeATK_Animation();
            DEF_Animation();

            DeadAnimation();
        }

        void LayerControl()
        {
            baseLayerInfo = animator.GetCurrentAnimatorStateInfo(baseLayer);
            underBodyInfo = animator.GetCurrentAnimatorStateInfo(underBodyLayer);
            rightArmInfo = animator.GetCurrentAnimatorStateInfo(rightArmLayer);
            leftArmInfo = animator.GetCurrentAnimatorStateInfo(leftArmLayer);
            upperBodyInfo = animator.GetCurrentAnimatorStateInfo(upperBodyLayer);
            fullBodyInfo = animator.GetCurrentAnimatorStateInfo(fullbodyLayer);
        }

        void OnAnimatorMove()
        {
            actions = baseLayerInfo.IsTag("CustomAction") || lockMovement;

            if (Time.timeScale == 0 || agent == null)
            {
                return;
            }

            if (agent.enabled && !agent.isOnOffMeshLink && agent.updatePosition)
            {
                Vector3 velocity = animator.deltaPosition / Time.deltaTime;
                if (!velocity.IsVectorNaN())
                {
                    agent.velocity = velocity;
                }
                else
                {
                    agent.velocity = Vector3.zero;
                }
            }

            if (!_rigidbody.useGravity && !actions && !agent.isOnOffMeshLink)
            {
                _rigidbody.velocity = animator.deltaPosition;
            }

            if (!agent.updatePosition && !actions)
            {
                var point = agent.enabled ? agent.nextPosition : destination;
                if (Vector3.Distance(transform.position, point) > 0.5f)
                {
                    desiredRotation = Quaternion.LookRotation(point - transform.position);
                    var rot = Quaternion.Euler(transform.eulerAngles.x, desiredRotation.eulerAngles.y, transform.eulerAngles.z);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, agent.angularSpeed * Time.deltaTime);
                }
                transform.position = animator.rootPosition;
                return;
            }
            // Strafe Movement
            if (OnStrafeArea && !actions && currentTarget.transform != null && canSeeTarget && currentHealth > 0f)
            {
                Vector3 targetDir = currentTarget.transform.position - transform.position;
                float step = (meleeManager != null && isAttacking) ? attackRotationSpeed * Time.deltaTime : (strafeRotationSpeed * Time.deltaTime);
                Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
                var rot = Quaternion.LookRotation(newDir);
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, rot.eulerAngles.y, transform.eulerAngles.z);
            }
            // Rotate the Character to the OffMeshLink End
            else if (agent.isOnOffMeshLink && !actions)
            {
                var pos = agent.nextOffMeshLinkData.endPos;
                targetPos = pos;
                UnityEngine.AI.OffMeshLinkData data = agent.currentOffMeshLinkData;
                desiredRotation = Quaternion.LookRotation(new Vector3(data.endPos.x, transform.position.y, data.endPos.z) - transform.position);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, (agent.angularSpeed * 2f) * Time.deltaTime);
            }
            // Free Movement
            else if (agent.desiredVelocity.magnitude > 0.1f && !actions && agent.enabled && currentHealth > 0f)
            {
                if (meleeManager != null && isAttacking)
                {
                    desiredRotation = Quaternion.LookRotation(agent.desiredVelocity);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, agent.angularSpeed * attackRotationSpeed * Time.deltaTime);
                }
                else
                {
                    desiredRotation = Quaternion.LookRotation(agent.desiredVelocity);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, agent.angularSpeed * Time.deltaTime);
                }
            }
            // Use the Animator rotation while doing an Action
            else if (actions || currentHealth <= 0f || isAttacking)
            {
                if (isRolling)
                {
                    desiredRotation = Quaternion.LookRotation(rollDirection, Vector3.up);
                    transform.rotation = desiredRotation;
                }
                else
                {
                    transform.rotation = animator.rootRotation;
                }

                // Use the Animator position while doing an Action
                if (!agent.enabled)
                {
                    destination = transform.position;
                    transform.position = animator.rootPosition;
                }
            }
        }

        #region AI Locomotion Animations

        /// <summary>
        /// Control the Locomotion behaviour of the AI
        /// </summary>
        /// <param name="_speed"></param>
        /// <param name="_direction"></param>
        void LocomotionAnimation(float _speed, float _direction)
        {
            isGrounded = agent.enabled ? agent.isOnNavMesh : isRolling ? true : groundDistance <= groundCheckDistance;
            animator.SetBool("IsGrounded", isGrounded);
            _speed = Mathf.Clamp(_speed, -maxSpeed, maxSpeed);
            if (OnStrafeArea)
            {
                _direction = Mathf.Clamp(_direction, -strafeSpeed, strafeSpeed);
            }

            var animSpeed = Mathf.Abs(_speed) > 0.1f ? _speed : 0;
            var animDirection = Mathf.Abs(_direction) > 0.1f ? _direction : 0;
            var newInput = new Vector2(animSpeed, animDirection);
            strafeInput = Mathf.Clamp(newInput.magnitude, 0, 1.5f);

            animator.SetFloat("InputMagnitude", strafeInput, .2f, Time.deltaTime);
            animator.SetFloat("InputVertical", actions ? 0 : (_speed != 0) ? _speed : 0, 0.2f, Time.fixedDeltaTime);
            animator.SetFloat("InputHorizontal", _direction, 0.2f, Time.fixedDeltaTime);
            animator.SetBool("IsStrafing", OnStrafeArea);
            animator.SetBool("isDead", isDead);
        }

        protected virtual float maxSpeed
        {
            get
            {
                return (OnStrafeArea ? strafeSpeed : chaseSpeed);
            }
        }

        /// <summary>
        /// Trigger a Death by Animation, Animation with Ragdoll or just turn the Ragdoll On
        /// </summary>
        void DeadAnimation()
        {
            if (!isDead)
            {
                return;
            }

            if (!triggerDieBehaviour)
            {
                triggerDieBehaviour = true;
                DeathBehaviour();
            }

            // death by animation
            if (deathBy == DeathBy.Animation)
            {
                if (fullBodyInfo.IsName("Dead"))
                {
                    if (fullBodyInfo.normalizedTime >= 0.99f && groundDistance <= 0.15f)
                    {
                        RemoveComponents();
                    }
                }
            }
            // death by animation & ragdoll after a time
            else if (deathBy == DeathBy.AnimationWithRagdoll)
            {
                if (fullBodyInfo.IsName("Dead"))
                {
                    // activate the ragdoll after the animation finish played
                    if (fullBodyInfo.normalizedTime >= 0.8f)
                    {
                        onActiveRagdoll.Invoke(null);
                        RemoveComponents();
                    }
                }
            }
            // death by ragdoll
            else if (deathBy == DeathBy.Ragdoll)
            {
                onActiveRagdoll.Invoke(null);
                RemoveComponents();
            }
        }

        private void DeathBehaviour()
        {
            // change the culling mode to render the animation until finish
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            // trigger die animation            
            if (deathBy == DeathBy.Animation || deathBy == DeathBy.AnimationWithRagdoll)
            {
                animator.SetBool("isDead", isDead);
            }
        }

        void CrouchAnimation()
        {
            animator.SetBool("IsCrouching", isCrouched);

            if (animator != null && animator.enabled)
            {
                CheckAutoCrouch();
            }
        }

        protected void RollAnimation()
        {
            if (animator == null || animator.enabled == false)
            {
                return;
            }

            isRolling = baseLayerInfo.IsName("Roll");
            if (isRolling)
            {
                _rigidbody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
                _rigidbody.useGravity = true;
                agent.enabled = false;
                agent.updatePosition = false;
            }
        }

        void ResetAIRotation()
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }

        #endregion

        #region AI Melee Combat Animations

        /// <summary>
        /// MOVE SET ID - check the Animator to see what MoveSet the character will move, also check your weapon to see if the moveset matches
        /// ps* Move Set is the way your character will move, ATK_ID is the way your character will attack. You can have different locomotion animations and attacks.
        /// </summary>
        void MoveSetIDControl()
        {
            if (meleeManager == null)
            {
                return;
            }

            animator.SetFloat("MoveSet_ID", meleeManager.GetMoveSetID());
        }

        /// <summary>
        /// Control Attack Behaviour
        /// </summary>
        void MeleeATK_Animation()
        {
            if (meleeManager == null)
            {
                return;
            }

            if (actions)
            {
                attackCount = 0;
            }

            animator.SetInteger("AttackID", meleeManager.GetAttackID());
        }

        /// <summary>
        /// ATTACK MELEE ANIMATION - it's activate by the AttackInput() method at the TPController by a trigger
        /// </summary>
        void DEF_Animation()
        {
            if (meleeManager == null)
            {
                return;
            }

            if (isBlocking)
            {
                animator.SetInteger("DefenseID", meleeManager.GetDefenseID());
            }
            animator.SetBool("IsBlocking", isBlocking);
        }

        /// <summary>
        /// Trigger the Attack animation
        /// </summary>
        public void MeleeAttack()
        {
            if (animator != null && animator.enabled && !actions)
            {
                animator.SetTrigger("WeakAttack");
            }
        }

        /// <summary>
        /// Check if is in LockMovement to reset attack and disable agent
        /// </summary>
        void ResetAndLockAgent()
        {
            lockMovement = fullBodyInfo.IsTag("LockMovement") || upperBodyInfo.IsTag("ResetState");

            if (lockMovement)
            {
                if (attackCount > 0)
                {
                    canAttack = false;
                    attackCount = 0;
                }

                if (baseLayerInfo.normalizedTime > 0.1f)
                {
                    animator.ResetTrigger("ResetState");
                    _rigidbody.constraints = RigidbodyConstraints.None | RigidbodyConstraints.FreezeRotation;
                    _rigidbody.useGravity = true;
                    agent.enabled = false;
                    agent.updatePosition = false;
                }
            }
        }

        /// <summary>
        /// Trigger Recoil Animation - It's Called at the MeleeWeapon
        /// </summary>
        public void TriggerRecoil(int recoil_id)
        {
            if (animator != null && animator.enabled && !isRolling)
            {
                animator.SetInteger("RecoilID", recoil_id);
                animator.SetTrigger("TriggerRecoil");
                animator.SetTrigger("ResetState");
            }
        }

        #endregion               
    }
}