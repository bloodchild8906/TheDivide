using UnityEngine;
namespace Invector
{
    [System.Serializable]
    public class vDamage
    {
        [Tooltip("Apply damage to the Character Health")]
        public float damageValue = 15;
        [Tooltip("How much stamina the target will lost when blocking this attack")]
        public float staminaBlockCost = 5;
        [Tooltip("How much time the stamina of the target will wait to recovery")]
        public float staminaRecoveryDelay = 1;
        [Tooltip("Apply damage even if the Character is blocking")]
        public bool ignoreDefense;
        [Tooltip("Activated Ragdoll when hit the Character")]
        public bool activeRagdoll;
        [vHideInInspector("activeRagdoll"),Tooltip("Time to keep Ragdoll active")]
        public float senselessTime;
        [HideInInspector]
        public Transform sender;
        [HideInInspector]
        public Transform receiver;
        [HideInInspector]
        public Vector3 hitPosition;
        public bool hitReaction = true;
        [HideInInspector]
        public int recoil_id = 0;
        [HideInInspector]
        public int reaction_id = 0;
        public string damageType;
        [HideInInspector] public Vector3 force;

        public vDamage()
        {
            this.damageValue = 15;
            this.staminaBlockCost = 5;
            this.staminaRecoveryDelay = 1;
            this.hitReaction = true;
        }

        public vDamage(int value)
        {
            this.damageValue = value;
            this.hitReaction = true;
        }

        public vDamage(int value, bool ignoreReaction)
        {
            this.damageValue = value;
            this.hitReaction = !ignoreReaction;
            if (ignoreReaction)
            {
                this.recoil_id = -1;
                this.reaction_id = -1;
            }                
        }

        public vDamage(vDamage damage)
        {
            this.damageValue = damage.damageValue;
            this.staminaBlockCost = damage.staminaBlockCost;
            this.staminaRecoveryDelay = damage.staminaRecoveryDelay;
            this.ignoreDefense = damage.ignoreDefense;
            this.activeRagdoll = damage.activeRagdoll;
            this.sender = damage.sender;
            this.receiver = damage.receiver;
            this.recoil_id = damage.recoil_id;
            this.reaction_id = damage.reaction_id;
            this.damageType = damage.damageType;
            this.hitPosition = damage.hitPosition;
            this.senselessTime = damage.senselessTime;
            this.force = damage.force;
        }

        /// <summary>
        /// Calc damage Resuction percentage
        /// </summary>
        /// <param name="damageReduction"></param>
        public void ReduceDamage(float damageReduction)
        {
            int result = (int)(this.damageValue - ((this.damageValue * damageReduction) / 100));
            this.damageValue = result;
        }
    }
}