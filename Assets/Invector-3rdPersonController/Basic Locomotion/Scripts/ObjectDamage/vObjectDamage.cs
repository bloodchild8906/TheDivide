using System.Collections.Generic;
using UnityEngine;

namespace Invector
{
    [vClassHeader("OBJECT DAMAGE", iconName = "DamageIcon")]
    public class vObjectDamage : vMonoBehaviour
    {
        [System.Serializable]
        public class OnHitEvent : UnityEngine.Events.UnityEvent<Collider> { }
        [System.Serializable]
        public class OnSendEvent : UnityEngine.Events.UnityEvent<vDamage> { }
        public vDamage damage;
        [Tooltip("Assign this to set other damage sender")]
        public Transform overrideDamageSender;
        [Tooltip("List of layers that can be hit, nothing will apply to all layers")]
        public LayerMask layerToCollide;
        [Tooltip("List of tags that can be hit, nothing will apply to all tags")]
        public vTagMask tags;
        [Tooltip("Check to use the damage Frequency")]
        public bool continuousDamage;
        [Tooltip("Apply damage to each end of the frequency in seconds ")]
        public float damageFrequency = 0.5f;
        protected List<Collider> targets;
        protected List<Collider> disabledTarget;
        protected float currentTime;
        public OnHitEvent onHit;
        public OnSendEvent onSendDamage;
        protected float particleCollisionTime;
        public enum CollisionMethod
        {
            OnTriggerEnter,
            OnColliderEnter,
            OnParticleCollision
        }

        public void SetOverrideDamageSender(Transform target) => overrideDamageSender = target;

        public CollisionMethod collisionMethod = CollisionMethod.OnTriggerEnter;

        public ParticleSystem part;
        public bool limitParticleCollisionEvent = false;
        public int maxParticleCollisionEvent = 1;
        public List<ParticleCollisionEvent> collisionEvents;

        protected virtual void Start()
        {
            targets = new List<Collider>();
            disabledTarget = new List<Collider>();
            if (collisionMethod == CollisionMethod.OnParticleCollision)
            {
                part = GetComponent<ParticleSystem>();
                collisionEvents = new List<ParticleCollisionEvent>();
            }
        }

        protected virtual void Update()
        {
            if (!this.enabled) return;
            if (continuousDamage && targets != null && targets.Count > 0)
            {
                if (currentTime > 0)
                {
                    currentTime -= Time.deltaTime;
                }
                else
                {
                    currentTime = damageFrequency;
                    foreach (Collider collider in targets)
                        if (collider != null)
                        {
                            if (collider.enabled)
                            {

                                ApplyDamage(collider, transform.position); // apply damage to enabled collider

                            }
                            else
                                disabledTarget.Add(collider);// add disabled collider to list of disabled
                        }

                    //remove all disabled colliders of target list
                    if (disabledTarget.Count > 0)
                    {
                        for (int i = disabledTarget.Count; i >= 0; i--)
                        {
                            if (disabledTarget.Count == 0) break;
                            try
                            {
                                if (targets.Contains(disabledTarget[i]))
                                    targets.Remove(disabledTarget[i]);
                            }
                            catch
                            {
                                break;
                            }
                        }
                    }

                    if (disabledTarget.Count > 0) disabledTarget.Clear();
                }
            }
        }

        protected virtual void OnCollisionEnter(Collision hit)
        {
            if (!this.enabled) return;
            if (collisionMethod != CollisionMethod.OnColliderEnter || continuousDamage) return;

            if (CanApplyDamage(hit.gameObject))
            {
                ApplyDamage(hit.collider, hit.contacts[0].point);
            }
        }

        protected virtual void OnTriggerEnter(Collider hit)
        {
            if (!this.enabled) return;
            if (collisionMethod != CollisionMethod.OnTriggerEnter) return;
            if (continuousDamage && CanApplyDamage(hit.gameObject) && !targets.Contains(hit))
            {
                targets.Add(hit);
            }
            else if (CanApplyDamage(hit.gameObject))
            {
                ApplyDamage(hit, transform.position);
            }
        }

        protected virtual bool CanApplyDamage(GameObject hitObject)
        {
            return (tags.Count == 0 || tags.Contains(hitObject.tag)) && layerToCollide == 0 || layerToCollide.ContainsLayer(hitObject.layer);
        }

        protected virtual void OnTriggerExit(Collider hit)
        {
            if (!this.enabled) return;
            if (collisionMethod == CollisionMethod.OnColliderEnter && !continuousDamage) return;

            if (CanApplyDamage(hit.gameObject) && targets.Contains(hit))
            {
                targets.Remove(hit);
            }
        }

        protected virtual void OnParticleCollision(GameObject hit)
        {
            if (!this.enabled) return;
            if (CanApplyDamage(hit))
            {
                if (collisionMethod != CollisionMethod.OnParticleCollision) return;

                if (particleCollisionTime > Time.time) return;
                if (continuousDamage)
                    particleCollisionTime = Time.time + damageFrequency;

                int numCollisionEvents = part.GetCollisionEvents(hit, collisionEvents);
                int i = 0;
                if (continuousDamage)
                {
                    if (hit.TryGetComponent(out Collider collider))
                    {
                        while (i < numCollisionEvents)
                        {
                            ApplyDamage(collider, collisionEvents[0].intersection);
                            Debug.Log(collider);
                            i++;
                        }
                    }
                }
                else
                {
                    if (hit.TryGetComponent(out Collider collider)) ApplyDamage(collider, collisionEvents[0].intersection);

                }

            }
        }

        public virtual void ClearTargets()
        {
            targets.Clear();
        }

        protected virtual void ApplyDamage(Collider target, Vector3 hitPoint)
        {
            damage.hitReaction = true;
            damage.sender = overrideDamageSender ? overrideDamageSender : transform;
            damage.hitPosition = hitPoint;
            damage.receiver = target.transform;
            var d = new vDamage(damage);
            if(d.damageValue > 0)
                target.gameObject.ApplyDamage(d);
            onSendDamage.Invoke(d);
            onHit.Invoke(target);
        }
    }
}