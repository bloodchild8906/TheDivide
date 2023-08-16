using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector
{
    [vClassHeader("HITDAMAGE PARTICLE", "Default hit Particle to instantiate every time you receive damage and Custom hit Particle to instantiate based on a custom DamageType that comes from the MeleeControl Behaviour (AnimatorController)")]
    public class vHitDamageParticle : vMonoBehaviour
    {
        public List<GameObject> defaultDamageEffects = new List<GameObject>();
        public List<vDamageEffect> customDamageEffects = new List<vDamageEffect>();

        private vFisherYatesRandom _random;

        IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            if (TryGetComponent<vIHealthController>(out vIHealthController healthController))
            {
                healthController.onReceiveDamage.AddListener(OnReceiveDamage);
            }
        }

        public virtual void OnReceiveDamage(vDamage damage)
        {
            // instantiate the hitDamage particle - check if your character has a HitDamageParticle component
            var damageDirection = damage.hitPosition - new Vector3(transform.position.x, damage.hitPosition.y, transform.position.z);
            var hitRotation = damageDirection != Vector3.zero ? Quaternion.LookRotation(damageDirection) : transform.rotation;

            if (damage.damageValue > 0)
            {
                TriggerEffect(new vDamageEffectInfo(damage.hitPosition, hitRotation, damage.damageType, damage.receiver));
            }
        }

        /// <summary>
        /// Raises the hit event.
        /// </summary>
        /// <param name="damageEffectInfo">Hit effect info.</param>
        protected virtual void TriggerEffect(vDamageEffectInfo damageEffectInfo)
        {
            if (_random == null)
            {
                _random = new vFisherYatesRandom();
            }
            var damageEffect = customDamageEffects.Find(effect => effect.damageType.Equals(damageEffectInfo.damageType));

            if (damageEffect != null)
            {
                damageEffect.onTriggerEffect.Invoke();
                if (damageEffect.customDamageEffect != null && damageEffect.customDamageEffect.Count > 0)
                {
                    var randomCustomEffect = damageEffect.customDamageEffect[_random.Next(damageEffect.customDamageEffect.Count)];

                    Instantiate(randomCustomEffect, damageEffectInfo.position,
                        damageEffect.rotateToHitDirection ? damageEffectInfo.rotation : randomCustomEffect.transform.rotation,
                        damageEffect.attachInReceiver && damageEffectInfo.receiver ? damageEffectInfo.receiver : vObjectContainer.root);
                }
            }
            else if (defaultDamageEffects.Count > 0 && damageEffectInfo != null)
            {
                var randomDefaultEffect = defaultDamageEffects[_random.Next(defaultDamageEffects.Count)];
                Instantiate(randomDefaultEffect, damageEffectInfo.position, damageEffectInfo.rotation, vObjectContainer.root);
            }
        }

        protected virtual void Reset()
        {
            defaultDamageEffects = new List<GameObject>();
            var defaultEffect = Resources.Load("defaultDamageEffect");

            if (defaultEffect != null)
            {
                defaultDamageEffects.Add(defaultEffect as GameObject);
            }
        }
    }

    public class vDamageEffectInfo
    {
        public Transform receiver;
        public Vector3 position;
        public Quaternion rotation;
        public string damageType;
        public vDamageEffectInfo(Vector3 position, Quaternion rotation, string damageType = "", Transform receiver = null)
        {
            this.receiver = receiver;
            this.position = position;
            this.rotation = rotation;
            this.damageType = damageType;
        }
    }

    [System.Serializable]
    public class vDamageEffect
    {
        public string damageType = "";
        public List<GameObject> customDamageEffect;
        public bool rotateToHitDirection = true;
        [Tooltip("Attach prefab in Damage Receiver transform")]
        public bool attachInReceiver = false;
        public UnityEvent onTriggerEffect;
    }
}
