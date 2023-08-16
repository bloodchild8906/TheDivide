using System;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vMelee
{
    using Invector.vItemManager;
    using vEventSystems;
    /// <summary>
    /// Event called when you equip a weapon (Weapon, isLeftWeapon)
    /// </summary>
    [System.Serializable]
    public class OnEquipWeaponEvent : UnityEngine.Events.UnityEvent<vMeleeWeapon, bool> { }
    public class vMeleeManager : vMonoBehaviour, IWeaponEquipmentListener
    {
        #region SeralizedProperties in CustomEditor

        public List<vBodyMember> Members = new List<vBodyMember>();
        public vDamage defaultDamage = new vDamage(10);
        public HitProperties hitProperties;
        public vMeleeWeapon leftWeapon, rightWeapon;
        public vOnHitEvent onDamageHit, onRecoilHit;
        public OnEquipWeaponEvent onEquipWeapon;
        public OnEquipWeaponEvent onUnequipWeapon;
        #endregion

        [Tooltip("NPC ONLY- Ideal distance for the attack")]
        public float defaultAttackDistance = 1f;
        [Tooltip("Default cost for stamina when attack")]
        public float defaultStaminaCost = 20f;
        [Tooltip("Default recovery delay for stamina when attack")]
        public float defaultStaminaRecoveryDelay = 1f;
        [Range(0, 100)]
        public int defaultDefenseRate = 100;
        [Range(0, 180)]
        public float defaultDefenseRange = 90;

        public virtual vIMeleeFighter fighter { get; set; }
        protected virtual int damageMultiplier { get; set; }
        protected virtual int currentRecoilID { get; set; }
        protected virtual int currentReactionID { get; set; }
        protected virtual bool ignoreDefense { get; set; }
        protected virtual bool activeRagdoll { get; set; }
        protected virtual float senselessTime { get; set; }
        protected virtual bool inRecoil { get; set; }
        protected virtual string attackName { get; set; }

        protected virtual void Start()
        {
            Init();
        }

        /// <summary>
        /// Init properties
        /// </summary>
        public virtual void Init()
        {
            fighter = gameObject.GetMeleeFighter();
            ///Initialize all bodyMembers and weapons
            foreach (vBodyMember member in Members)
            {
                ///damage of member will be always the defaultDamage
                //member.attackObject.damage = defaultDamage;       
                if (member.attackObject == null)
                {
                    var attackObjects = GetComponentsInChildren<vMeleeAttackObject>();
                    if (attackObjects.Length > 0)
                        member.attackObject = System.Array.Find(attackObjects, a => a.attackObjectName.Equals(member.bodyPart));

                    if (member.attackObject == null)
                    {
                        Debug.LogWarning("Can't find the attack Object " + member.bodyPart);
                        continue;
                    }
                }
                member.attackObject.damage.damageValue = defaultDamage.damageValue;
                if (member.bodyPart == HumanBodyBones.LeftLowerArm.ToString())
                {
                    var weapon = member.attackObject.GetComponentInChildren<vMeleeWeapon>(true);
                    leftWeapon = weapon;
                }
                if (member.bodyPart == HumanBodyBones.RightLowerArm.ToString())
                {
                    var weapon = member.attackObject.GetComponentInChildren<vMeleeWeapon>(true);
                    rightWeapon = weapon;
                }
                member.attackObject.meleeManager = this;
                member.SetActiveDamage(false);
            }

            if (leftWeapon != null)
            {
                leftWeapon.SetActiveDamage(false);
                leftWeapon.meleeManager = this;
            }
            if (rightWeapon != null)
            {
                rightWeapon.meleeManager = this;
                rightWeapon.SetActiveDamage(false);
            }
        }

        /// <summary>
        /// Set active Multiple Parts to attack
        /// </summary>
        /// <param name="bodyParts"></param>
        /// <param name="type"></param>
        /// <param name="active"></param>
        /// <param name="damageMultiplier"></param>
        public virtual void SetActiveAttack(List<string> bodyParts, vAttackType type, bool active = true, int damageMultiplier = 0, int recoilID = 0, int reactionID = 0, bool ignoreDefense = false, bool activeRagdoll = false, float senselessTime = 0, string attackName = "")
        {
            for (int i = 0; i < bodyParts.Count; i++)
            {
                var bodyPart = bodyParts[i];
                SetActiveAttack(bodyPart, type, active, damageMultiplier, recoilID, reactionID, ignoreDefense, activeRagdoll, senselessTime, attackName);
            }
        }

        /// <summary>
        /// Set active Single Part to attack
        /// </summary>
        /// <param name="bodyPart"></param>
        /// <param name="type"></param>
        /// <param name="active"></param>
        /// <param name="damageMultiplier"></param>
        public virtual void SetActiveAttack(string bodyPart, vAttackType type, bool active = true, int damageMultiplier = 0, int recoilID = 0, int reactionID = 0, bool ignoreDefense = false, bool activeRagdoll = false, float senselessTime = 0, string attackName = "")
        {
            this.damageMultiplier = damageMultiplier;
            currentRecoilID = recoilID;
            currentReactionID = reactionID;
            this.ignoreDefense = ignoreDefense;
            this.activeRagdoll = activeRagdoll;
            this.attackName = attackName;
            this.senselessTime = senselessTime;
            if (type == vAttackType.Unarmed)
            {
                /// find attackObject by bodyPart
                var attackObject = Members.Find(member => member.bodyPart == bodyPart);
                if (attackObject != null)
                {
                    attackObject.SetActiveDamage(active);
                }
            }
            else
            {   ///if AttackType == MeleeWeapon
                ///this work just for Right and Left Lower Arm         
                if (bodyPart == "RightLowerArm" && rightWeapon != null)
                {
                    rightWeapon.meleeManager = this;
                    rightWeapon.SetActiveDamage(active);
                }
                else if (bodyPart == "LeftLowerArm" && leftWeapon != null)
                {
                    leftWeapon.meleeManager = this;
                    leftWeapon.SetActiveDamage(active);
                }
            }
        }

        /// <summary>
        /// Listener of Damage Event
        /// </summary>
        /// <param name="hitInfo"></param>
        public virtual void OnDamageHit(ref vHitInfo hitInfo)
        {
            vDamage damage = new vDamage(hitInfo.attackObject.damage);
            damage.sender = transform;
            damage.reaction_id = currentReactionID;
            damage.recoil_id = currentRecoilID;
            if (this.activeRagdoll) damage.activeRagdoll = this.activeRagdoll;
            if (this.attackName != string.Empty) damage.damageType = this.attackName;
            if (this.ignoreDefense) damage.ignoreDefense = this.ignoreDefense;
            if (this.senselessTime != 0) damage.senselessTime = this.senselessTime;
            /// Calc damage with multiplier 
            /// and Call ApplyDamage of attackObject 

            damage.damageValue *= damageMultiplier > 1 ? damageMultiplier : 1;
            hitInfo.targetIsBlocking = !hitInfo.attackObject.ApplyDamage(hitInfo.hitBox, hitInfo.targetCollider, damage);

            onDamageHit.Invoke(hitInfo);
        }

        /// <summary>
        /// Listener of Recoil Event
        /// </summary>
        /// <param name="hitInfo"></param>
        public virtual void OnRecoilHit(vHitInfo hitInfo)
        {
            if (hitProperties.useRecoil && InRecoilRange(hitInfo) && !inRecoil)
            {
                inRecoil = true;
                var id = currentRecoilID;
                if (fighter != null) fighter.OnRecoil(id);
                onRecoilHit.Invoke(hitInfo);
                Invoke("ResetRecoil", 1f);
            }
        }

        /// <summary>
        /// Call Weapon Defense Events.
        /// </summary>
        public virtual void OnDefense()
        {
            if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
            {
                leftWeapon.OnDefense();
            }
            if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
            {
                rightWeapon.OnDefense();
            }
        }

        /// <summary>
        /// Get Current Attack ID
        /// </summary>
        /// <returns></returns>
        public virtual int GetAttackID()
        {
            if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyDefense && rightWeapon.gameObject.activeInHierarchy) return rightWeapon.attackID;
            return 0;
        }

        /// <summary>
        /// Get StaminaCost
        /// </summary>
        /// <returns></returns>
        public virtual float GetAttackStaminaCost()
        {
            if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyDefense && rightWeapon.gameObject.activeInHierarchy) return rightWeapon.staminaCost;
            return defaultStaminaCost;
        }

        /// <summary>
        /// Get StaminaCost
        /// </summary>
        /// <returns></returns>
        public virtual float GetAttackStaminaRecoveryDelay()
        {
            if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyDefense && rightWeapon.gameObject.activeInHierarchy) return rightWeapon.staminaRecoveryDelay;
            return defaultStaminaRecoveryDelay;
        }

        /// <summary>
        /// Get ideal distance for the attack
        /// </summary>
        /// <returns></returns>
        public virtual float GetAttackDistance()
        {
            if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyDefense && rightWeapon.gameObject.activeInHierarchy) return rightWeapon.distanceToAttack;
            return defaultAttackDistance;
        }

        /// <summary>
        /// Get Current Defense ID
        /// </summary>
        /// <returns></returns>
        public virtual int GetDefenseID()
        {
            if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
            {
                GetComponent<Animator>().SetBool("FlipAnimation", false);
                return leftWeapon.defenseID;
            }
            if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
            {
                GetComponent<Animator>().SetBool("FlipAnimation", true);
                return rightWeapon.defenseID;
            }
            return 0;
        }

        /// <summary>
        /// Get Defense Rate of Melee Defense 
        /// </summary>
        /// <returns></returns>
        public virtual int GetDefenseRate()
        {
            if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
            {
                return leftWeapon.defenseRate;
            }
            if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
            {
                return rightWeapon.defenseRate;
            }
            return defaultDefenseRate;
        }

        /// <summary>
        /// Get Current MoveSet ID
        /// </summary>
        /// <returns></returns>
        public virtual int GetMoveSetID()
        {
            if (rightWeapon != null && rightWeapon.gameObject.activeInHierarchy) return rightWeapon.movesetID;
            // if (leftWeapon != null && leftWeapon.gameObject.activeInHierarchy) return leftWeapon.movesetID;
            return 0;
        }

        /// <summary>
        /// Check if defence can break Attack
        /// </summary>
        /// <returns></returns>
        public virtual bool CanBreakAttack()
        {
            if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
            {
                return leftWeapon.breakAttack;
            }
            if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
            {
                return rightWeapon.breakAttack;
            }
            return false;
        }

        /// <summary>
        /// Check if attack can be blocked
        /// </summary>
        /// <param name="attackPoint"></param>
        /// <returns></returns>
        public virtual bool CanBlockAttack(Vector3 attackPoint)
        {
            if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
            {
                return Math.Abs(transform.HitAngle(attackPoint)) <= leftWeapon.defenseRange;
            }
            if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
            {
                return Math.Abs(transform.HitAngle(attackPoint)) <= rightWeapon.defenseRange;
            }
            return Math.Abs(transform.HitAngle(attackPoint)) <= defaultDefenseRange;
        }

        /// <summary>
        /// Get defense recoilID for break attack
        /// </summary>
        /// <returns></returns>
        public virtual int GetDefenseRecoilID()
        {
            if (leftWeapon != null && leftWeapon.meleeType != vMeleeType.OnlyAttack && leftWeapon.gameObject.activeInHierarchy)
            {
                return leftWeapon.recoilID;
            }
            if (rightWeapon != null && rightWeapon.meleeType != vMeleeType.OnlyAttack && rightWeapon.gameObject.activeInHierarchy)
            {
                return rightWeapon.recoilID;
            }
            return 0;
        }

        /// <summary>
        /// Check if angle of hit point is in range of recoil
        /// </summary>
        /// <param name="hitInfo"></param>
        /// <returns></returns>
        protected virtual bool InRecoilRange(vHitInfo hitInfo)
        {
            var center = new Vector3(transform.position.x, hitInfo.hitPoint.y, transform.position.z);
            var euler = (Quaternion.LookRotation(hitInfo.hitPoint - center).eulerAngles - transform.eulerAngles).NormalizeAngle();

            return euler.y <= hitProperties.recoilRange;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weaponObject"></param>
        public virtual void SetLeftWeapon(GameObject weaponObject)
        {
            if (weaponObject)
            {
                leftWeapon = weaponObject.GetComponent<vMeleeWeapon>();
                SetLeftWeapon(leftWeapon);
            }
            else
            {
                if (leftWeapon) onUnequipWeapon.Invoke(leftWeapon, true);
                leftWeapon = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weaponObject"></param>
        public virtual void SetRightWeapon(GameObject weaponObject)
        {
            if (weaponObject)
            {
                rightWeapon = weaponObject.GetComponent<vMeleeWeapon>();
                SetRightWeapon(rightWeapon);
            }
            else
            {
                if (rightWeapon) onUnequipWeapon.Invoke(rightWeapon, false);
                rightWeapon = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weaponObject"></param>
        public virtual void SetLeftWeapon(vMeleeWeapon weapon)
        {
            if (weapon)
            {
                leftWeapon = weapon;
                leftWeapon.IsEquipped = true;
                leftWeapon.SetActiveDamage(false);
                leftWeapon.meleeManager = this;
                onEquipWeapon.Invoke(weapon, true);
            }
            else
            {
                if (leftWeapon) onUnequipWeapon.Invoke(leftWeapon, true);
                leftWeapon = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weaponObject"></param>
        public virtual void SetRightWeapon(vMeleeWeapon weapon)
        {
            if (weapon)
            {               
                rightWeapon = weapon;
                rightWeapon.IsEquipped = true;
                rightWeapon.meleeManager = this;
                rightWeapon.SetActiveDamage(false);
                onEquipWeapon.Invoke(weapon, false);
            }
            else
            {
                if (rightWeapon) onUnequipWeapon.Invoke(rightWeapon, false);
                rightWeapon = null;
            }
        }

        public virtual vMeleeWeapon CurrentActiveAttackWeapon
        {
            get
            {
                var weapon = CurrentAttackWeapon;
                if (weapon && weapon.IsEquipped) return weapon;
                return null;
            }
        }

        public virtual vMeleeWeapon CurrentActiveDefenseWeapon
        {
            get
            {
                var weapon = CurrentDefenseWeapon;
                if (weapon && weapon.IsEquipped) return weapon;
                return null;
            }
        }

        public virtual vMeleeWeapon CurrentAttackWeapon
        {
            get
            {
                if (rightWeapon && (rightWeapon.meleeType == vMeleeType.OnlyAttack || rightWeapon.meleeType == vMeleeType.AttackAndDefense)) return rightWeapon;
                if (leftWeapon && (leftWeapon.meleeType == vMeleeType.OnlyAttack || leftWeapon.meleeType == vMeleeType.AttackAndDefense)) return leftWeapon;
                return null;
            }
        }

        public virtual vMeleeWeapon CurrentDefenseWeapon
        {
            get
            {
                if (rightWeapon && (rightWeapon.meleeType == vMeleeType.OnlyDefense || rightWeapon.meleeType == vMeleeType.AttackAndDefense)) return rightWeapon;
                if (leftWeapon && (leftWeapon.meleeType == vMeleeType.OnlyDefense || leftWeapon.meleeType == vMeleeType.AttackAndDefense)) return leftWeapon;
                return null;
            }
        }

        protected virtual void ResetRecoil()
        {
            inRecoil = false;
        }
    }

    #region Secundary Classes
    [Serializable]
    public class vOnHitEvent : UnityEngine.Events.UnityEvent<vHitInfo> { }

    [Serializable]
    public class vBodyMember
    {
        public Transform transform;
        public string bodyPart;

        public vMeleeAttackObject attackObject;
        public bool isHuman;
        public void SetActiveDamage(bool active)
        {
            attackObject.SetActiveDamage(active);
        }
    }

    public enum vHumanBones
    {
        RightHand, RightLowerArm, RightUpperArm, RightShoulder,
        LeftHand, LeftLowerArm, LeftUpperArm, LeftShoulder,
        RightFoot, RightLowerLeg, RightUpperLeg,
        LeftFoot, LeftLowerLeg, LeftUpperLeg,
        Chest,
        Head
    }

    public enum vAttackType
    {
        Unarmed, MeleeWeapon
    }
    #endregion
}
