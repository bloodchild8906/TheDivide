using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.vMelee
{
    using vEventSystems;
    [vClassHeader("Melee Object", openClose = false)]
    public partial class vMeleeAttackObject : vMonoBehaviour
    {
        [vReadOnly(false)] public string attackObjectName;
        public vDamage damage;
        public Transform overrideDamageSender;
        public List<vHitBox> hitBoxes;
        public int damageModifier;
        [HideInInspector]
        public bool canApplyDamage;
        /// <summary>
        /// Event called when attack was successful
        /// </summary>
        [vHelpBox("Event called when attack was successful")]
        public OnHitEnter onDamageHit;
        /// <summary>
        /// Event called when the attack causes recoil
        /// </summary>
        [vHelpBox("Event called when the attack causes recoil")]
        public OnHitEnter onRecoilHit;
        /// <summary>
        /// Event called when causes damage 
        /// </summary>
        [vHelpBox("Event called when causes damage ")]
        public OnReceiveDamage onPassDamage;
        [vHelpBox("Events called when  Damage applier (HitBoxes) is enabled or disabled ")]
        public UnityEvent onEnableDamage;
        public UnityEvent onDisableDamage;
        private Dictionary<vHitBox, List<GameObject>> targetColliders;
        [HideInInspector]
        public vMeleeManager meleeManager;

        protected virtual void Start()
        {
            // init list of targetColliders
            targetColliders = new Dictionary<vHitBox, List<GameObject>>();

            if (hitBoxes.Count > 0)
            {
                // initialize hitBox properties
                foreach (vHitBox hitBox in hitBoxes)
                {
                    hitBox.attackObject = this;
                    targetColliders.Add(hitBox, new List<GameObject>());
                }
            }
            else
            {
                this.enabled = false;
            }
        }

        /// <summary>
        /// Set Active all hitBoxes of the MeleeAttackObject
        /// </summary>
        /// <param name="value"> active value</param>  
        public virtual void SetActiveDamage(bool value)
        {
            canApplyDamage = value;
            for (int i = 0; i < hitBoxes.Count; i++)
            {
                var hitCollider = hitBoxes[i];
                hitCollider.trigger.enabled = value;
                if (value == false && targetColliders != null)
                {
                    targetColliders[hitCollider].Clear();
                }
            }
            if (value)
            {
                onEnableDamage.Invoke();
            }
            else
            {
                onDisableDamage.Invoke();
            }
        }

        /// <summary>
        /// Hitboxes Call Back
        /// </summary>
        /// <param name="hitBox">vHitBox object</param>
        /// <param name="other">target Collider</param>
        public virtual void OnHit(vHitBox hitBox, Collider other)
        {
            // check first condition for hit 
            if (canApplyDamage && !targetColliders[hitBox].Contains(other.gameObject) && (meleeManager != null && other.gameObject != meleeManager.gameObject))
            {
                var inDamage = false;
                var inRecoil = false;

                if (meleeManager == null)
                {
                    meleeManager = GetComponentInParent<vMeleeManager>();
                }

                //check if meleeManager exists and apply his hitProperties to this
                HitProperties _hitProperties = meleeManager.hitProperties;

                // damage conditions
                if (((hitBox.triggerType & vHitBoxType.Damage) != 0) && _hitProperties.hitDamageTags == null || _hitProperties.hitDamageTags.Count == 0)
                {
                    inDamage = true;
                }
                else if (((hitBox.triggerType & vHitBoxType.Damage) != 0) && _hitProperties.hitDamageTags.Contains(other.tag))
                {
                    inDamage = true;
                }
                // recoil conditions  
                else if (((hitBox.triggerType & vHitBoxType.Recoil) != 0) &&
                    (_hitProperties.hitRecoilLayer == (_hitProperties.hitRecoilLayer | (1 << other.gameObject.layer))))
                {
                    inRecoil = true;
                }

                if (inDamage || inRecoil)
                {
                    // add target collider in the list to control the frequency of hit
                    targetColliders[hitBox].Add(other.gameObject);
                    vHitInfo hitInfo = new vHitInfo(this, hitBox, other, hitBox.transform.position);

                    if (inDamage == true)
                    {
                        // If there is a meleeManager then call onDamageHit to control damage values
                        // and it will call the ApplyDamage after filter the damage
                        // if meleeManager is null the damage will be directly applied
                        // Finally the OnDamageHit event is called
                        if (meleeManager)
                        {
                            meleeManager.OnDamageHit(ref hitInfo);
                        }
                        else
                        {
                            damage.sender = overrideDamageSender ? overrideDamageSender : transform;
                            // ApplyDamage(hitBox, other, damage);
                        }

                        if (!hitInfo.targetIsBlocking)
                        {
                            onDamageHit.Invoke(hitInfo);
                        }
                    }

                    // recoil just work with OnRecoilHit event and meleeManger
                    if (inRecoil == true)
                    {
                        if (meleeManager)
                        {
                            meleeManager.OnRecoilHit(hitInfo);
                        }

                        onRecoilHit.Invoke(hitInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Apply damage to target collider (TakeDamage, damage))
        /// </summary>
        /// <param name="hitBox">vHitBox object</param>
        /// <param name="other">collider target</param>
        /// <param name="damage"> damage</param>
        public virtual bool ApplyDamage(vHitBox hitBox, Collider other, vDamage damage)
        {
            vDamage _damage = new vDamage(damage);
            _damage.receiver = other.transform;
            _damage.damageValue = Mathf.RoundToInt(((damage.damageValue + damageModifier) * (hitBox.damagePercentage * 0.01f)));
            _damage.hitPosition = hitBox.transform.position;
            other.gameObject.ApplyDamage(_damage, meleeManager.fighter);
            if (_damage.hitReaction && _damage.damageValue > 0)
            {                
                onPassDamage.Invoke(_damage);
            }            

            return _damage.hitReaction;
        }
    }
}

namespace Invector.vMelee
{
    #region Secundary Class
    [System.Serializable]
    public class OnHitEnter : UnityEvent<vHitInfo> { }

    public partial class vHitInfo
    {
        public vMeleeAttackObject attackObject;
        public vHitBox hitBox;
        public Vector3 hitPoint;
        public Collider targetCollider;
        public bool targetIsBlocking;
        public vHitInfo(vMeleeAttackObject attackObject, vHitBox hitBox, Collider targetCollider, Vector3 hitPoint)
        {
            this.attackObject = attackObject;
            this.hitBox = hitBox;
            this.targetCollider = targetCollider;
            this.hitPoint = hitPoint;
        }
    }

    [System.Serializable]
    public class HitProperties
    {
        [Tooltip("Tag to receive Damage")]
        public List<string> hitDamageTags = new List<string>() { "Enemy" };
        [Tooltip("Trigger a HitRecoil animation if the character attacks a obstacle")]
        public bool useRecoil = true;
        public bool drawRecoilGizmos;
        [Range(0, 180f)]
        public float recoilRange = 90f;
        [Tooltip("layer to Recoil Damage")]
        public LayerMask hitRecoilLayer = 1 << 0;
    }
    #endregion
}