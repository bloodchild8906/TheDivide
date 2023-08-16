using System.Collections.Generic;
using UnityEngine;

namespace Invector.vMelee
{
    using vEventSystems;
    public class vMeleeAttackControl : StateMachineBehaviour
    {
        [Tooltip("normalizedTime of Active Damage")]
        public float startDamage = 0.05f;
        [Tooltip("normalizedTime of Disable Damage")]
        public float endDamage = 0.9f;        
        public int damageMultiplier;
        public int recoilID;
        public int reactionID;

        public vAttackType meleeAttackType = vAttackType.Unarmed;
        [Tooltip("You can use a name as reference to trigger a custom HitDamageParticle")]
        public string damageType;
        [HideInInspector]
        [Header("This work with vMeleeManager to active vMeleeAttackObject from bodyPart and id")]
        public List<string> bodyParts = new List<string> { "RightLowerArm" };
        public bool ignoreDefense;
        public bool activeRagdoll;
        [vHideInInspector("activeRagdoll"), Tooltip("Time to keep Ragdoll active")]
        public float senselessTime;
        [Tooltip("Check true in the last attack of your combo to reset the triggers")]
        public bool resetAttackTrigger;
        private bool isActive;
        public bool debug;
        private vIAttackListener mFighter;
        private bool isAttacking;

        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            mFighter = animator.GetComponent<vIAttackListener>();
            isAttacking = true;
            if (mFighter != null)
                mFighter.OnEnableAttack();

            if (debug)
                Debug.Log("Enter " + damageType);
        }

        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.normalizedTime % 1 >= startDamage && stateInfo.normalizedTime % 1 <= endDamage && !isActive)
            {
                if (debug) Debug.Log(animator.name + " attack " + damageType + " enable damage in " + System.Math.Round(stateInfo.normalizedTime % 1, 2));
                isActive = true;
                ActiveDamage(animator, true);
            }
            else if (stateInfo.normalizedTime % 1 > endDamage && isActive)
            {
                if (debug) Debug.Log(animator.name + " attack " + damageType + " disable damage in " + System.Math.Round(stateInfo.normalizedTime % 1, 2));
                isActive = false;
                ActiveDamage(animator, false);
            }

            if (stateInfo.normalizedTime % 1 > endDamage && isAttacking)
            {
                isAttacking = false;
                if (mFighter != null)
                    mFighter.OnDisableAttack();
            }

            //if (stateInfo.normalizedTime % 1 > allowRotationAt && isAttacking)
            //{              
            //    isAttacking = false;
            //    if (mFighter != null)
            //        mFighter.OnDisableAttack();
            //}
        }

        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (debug)
                Debug.Log("Exit " + damageType);

            if (isActive)
            {
                isActive = false;
                ActiveDamage(animator, false);
            }

            if (isAttacking)
            {
                isAttacking = false;
                if (mFighter != null)
                    mFighter.OnDisableAttack();
            }
            if (mFighter != null && resetAttackTrigger)
                mFighter.ResetAttackTriggers();

            if (debug) Debug.Log(animator.name + " attack " + damageType + " stateExit");
        }

        void ActiveDamage(Animator animator, bool value)
        {
            var meleeManager = animator.GetComponent<vMeleeManager>();
            if (meleeManager)
                meleeManager.SetActiveAttack(bodyParts, meleeAttackType, value, damageMultiplier, recoilID, reactionID, ignoreDefense, activeRagdoll,senselessTime, damageType);
        }
    }
}