using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector
{
    public class vSpawnOnParticleCollision : MonoBehaviour
    {
        [vHelpBox("This component is used to spawn objects when particle collide with something\n")]
        public ParticleSystem part;
        public GameObject prefab;
        public List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

        Transform damageSender;
        [vHelpBox("When the prefab is a ObjectDamage you can use this properties to override the damage frequency and the damage value")]
        public bool overrideDamage;
        public float damageFrequeny;
        public vDamage damage;       
        public void SetOverrideDamageSender(Transform sender)
        {
            damageSender = sender;
        }

        protected virtual void OnParticleCollision(GameObject hit)
        {
            int numCollisionEvents = part.GetCollisionEvents(hit, collisionEvents);
            var intersection = collisionEvents[0].intersection;
            var normal = collisionEvents[0].normal;
            var go = Instantiate(prefab, intersection, Quaternion.LookRotation(normal), vObjectContainer.root);

            if (damageSender && go.TryGetComponent(out vObjectDamage d))
            {
                d.SetOverrideDamageSender(damageSender);
                if (overrideDamage)
                {
                    d.damage.damageValue = 0;
                    d.onSendDamage.AddListener(OnSendDamage);
                }
            }


        }

        protected Dictionary<GameObject, DamageHandle> targetStorage = new Dictionary<GameObject, DamageHandle>();

        protected class DamageHandle
        {
            public Transform target;
            public float damageFrequeny;
            float lastDamageTime;
            public bool canApplyDamage => lastDamageTime < Time.time;
            public void ApplyDamage(vDamage damage)
            {
                if (canApplyDamage)
                {
                    lastDamageTime = Time.time + damageFrequeny;
                    damage.receiver.gameObject.ApplyDamage(damage);
                }
            }
        }

        protected virtual void OnSendDamage(vDamage damage)
        {
            if (!targetStorage.ContainsKey(damage.receiver.gameObject))
            {
                targetStorage.Add(damage.receiver.gameObject, new DamageHandle());
            }

            if (targetStorage.TryGetValue(damage.receiver.gameObject, out DamageHandle handle))
            {
                handle.damageFrequeny = damageFrequeny;
                if (handle.canApplyDamage)
                {
                    damage.damageValue = this.damage.damageValue;
                    damage.damageType = this.damage.damageType;
                    damage.reaction_id = this.damage.reaction_id;
                    damage.recoil_id = this.damage.recoil_id;
                    handle.ApplyDamage(damage);
                }

            }

        }
    }
}