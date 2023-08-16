using System.Collections;
using UnityEngine;

namespace Invector
{
    using System.Collections.Generic;
    using vEventSystems;

    [vClassHeader("Explosive", openClose = false)]
    public class vExplosive : vMonoBehaviour
    {
        [System.Serializable]
        public class OnUpdateTime : UnityEngine.Events.UnityEvent<float> { }
        [System.Serializable]
        public class ColliderEvent : UnityEngine.Events.UnityEvent<Collider> { }
        public vDamage damage;
        [vHelpBox("Use this values To define the min and max damage based on range")]
        [Range(0, 1f)]
        public float damageOnMinRangeMultiplier = 1f;
        [Range(0, 1f)]
        public float damageOnMaxRangeMultiplier = 0f;
        [Tooltip("Assign this to set other damage sender")]
        public Transform overrideDamageSender;
        public float explosionForce;
        public float minExplosionRadius;
        public float maxExplosionRadius;
        public float upwardsModifier = 1;
        public ForceMode forceMode;
        public ExplosiveMethod method;
        public LayerMask applyDamageLayer, applyForceLayer;
        public float timeToExplode = 10f;
        public bool destroyAfterExplode = true;
        [Tooltip("convert to progress 0 to 1")]
        public bool normalizeTime;
        public bool showGizmos;
        public UnityEngine.Events.UnityEvent onInitTimer;
        public OnUpdateTime onUpdateTimer;
        public UnityEngine.Events.UnityEvent onExplode;
        public ColliderEvent onHit;
        private bool inTimer;
        protected List<GameObject> collidersReached;

        void OnDrawGizmosSelected()
        {
            if (!showGizmos) return;
            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawSphere(transform.position, minExplosionRadius);
            Gizmos.color = new Color(0, 1, 0, 0.2f);
            Gizmos.DrawSphere(transform.position, maxExplosionRadius);
        }

        public void SetOverrideDamageSender(Transform target) => overrideDamageSender = target;
        public void SetDamage(vDamage damage)
        {
            this.damage = damage;
        }

        public enum ExplosiveMethod
        {
            collisionEnter,
            collisionEnterTimer,
            remote,
            timer,
            remoteTimer
        }

        protected virtual void Start()
        {
            collidersReached = new List<GameObject>();
            if (method == ExplosiveMethod.timer)
            {
                StartCoroutine(StartTimer());
            }
        }

        protected virtual IEnumerator StartTimer()
        {
            if (!inTimer)
            {
                onInitTimer.Invoke();
                inTimer = true;
                var startTime = Time.time;
                var time = 0f;
                while (time < timeToExplode)
                {
                    yield return new WaitForEndOfFrame();
                    time = Time.time - startTime;
                    onUpdateTimer.Invoke(normalizeTime ? (time / timeToExplode) : time);
                }
                if (gameObject)
                {
                    onUpdateTimer.Invoke(normalizeTime ? (1f) : timeToExplode);
                    Explode();
                }
            }
        }

        protected virtual IEnumerator DestroyBomb()
        {
            yield return new WaitForSeconds(0.1f);
            Destroy(gameObject);
        }

        protected virtual void OnCollisionEnter(Collision collision)
        {
            if (method == ExplosiveMethod.collisionEnter)
                Explode();
            else if (method == ExplosiveMethod.collisionEnterTimer)
                StartCoroutine(StartTimer());
        }

        protected virtual void Explode()
        {
            onExplode.Invoke();
            var colliders = Physics.OverlapSphere(transform.position, maxExplosionRadius, applyDamageLayer);

            if (collidersReached == null)
            {
                collidersReached = new List<GameObject>();
            }

            for (int i = 0; i < colliders.Length; ++i)
            {
                if (colliders[i] != null && colliders[i].gameObject != null && !collidersReached.Contains(colliders[i].gameObject))
                {
                    collidersReached.Add(colliders[i].gameObject);
                    var _damage = new vDamage(damage);
                    _damage.sender = overrideDamageSender ? overrideDamageSender : transform;

                    _damage.hitPosition = colliders[i].ClosestPointOnBounds(transform.position);
                    _damage.receiver = colliders[i].transform;
                    var distance = Vector3.Distance(transform.position, _damage.hitPosition);
                    var damageValue = distance <= minExplosionRadius ? damage.damageValue * damageOnMinRangeMultiplier : Mathf.Lerp(damage.damageValue * damageOnMaxRangeMultiplier, damage.damageValue * damageOnMinRangeMultiplier, EvaluateDistance(distance));
                    _damage.activeRagdoll = distance > maxExplosionRadius * 0.5f ? false : _damage.activeRagdoll;

                    _damage.damageValue = (int)damageValue;
                    onHit.Invoke(colliders[i]);
                    colliders[i].gameObject.ApplyDamage(_damage, null);

                }
            }
            StartCoroutine(ApplyExplosionForce());
            if (destroyAfterExplode) StartCoroutine(DestroyBomb());
        }

        protected virtual IEnumerator ApplyExplosionForce()
        {
            yield return new WaitForSeconds(0.1f);

            var colliders = Physics.OverlapSphere(transform.position, maxExplosionRadius, applyForceLayer);
            for (int i = 0; i < colliders.Length; i++)
            {
                var _rigdbody = colliders[i].GetComponent<Rigidbody>();
                var distance = Vector3.Distance(transform.position, colliders[i].ClosestPointOnBounds(transform.position));
                var force = distance <= minExplosionRadius ? explosionForce : GetPercentageForce(distance, explosionForce);
                if (_rigdbody)
                {
                    _rigdbody.AddExplosionForce(force, transform.position, maxExplosionRadius, upwardsModifier, forceMode);
                }
            }
        }

        protected float EvaluateDistance(float distance)
        {
            if (distance > maxExplosionRadius) distance = maxExplosionRadius;

            var distanceLimit = maxExplosionRadius - minExplosionRadius;
            var distanceCalc = Mathf.Clamp(distance - minExplosionRadius, 0, distanceLimit);
            var distanceResult = Mathf.Clamp(distanceLimit - (distanceCalc), 0, distanceLimit);
            var multiple = ((distanceResult / distanceLimit) * 100f) * 0.01f;
            return multiple;
        }
        protected float GetPercentageForce(float distance, float value)
        {

            return value * EvaluateDistance(distance);
        }

        public virtual void SetCollisionEnterMethod()
        {
            method = ExplosiveMethod.collisionEnter;
        }

        public virtual void SetCollisionEnterTimerMethod(int timer)
        {
            method = ExplosiveMethod.collisionEnterTimer;
            this.timeToExplode = timer;
        }

        public virtual void SetRemoveMethod()
        {
            method = ExplosiveMethod.remote;
        }

        public virtual void SetRemoveTimerMethod(int timer)
        {
            method = ExplosiveMethod.remoteTimer;
            this.timeToExplode = timer;
        }

        public virtual void SetTimerMethod(int timer)
        {
            method = ExplosiveMethod.timer;
            this.timeToExplode = timer;
        }

        public virtual void ActiveExplosion()
        {
            if (method == ExplosiveMethod.remote)
                Explode();
            else if (method == ExplosiveMethod.remoteTimer)
            {
                StartCoroutine(StartTimer());
            }
        }

        public void RemoveParent()
        {
            transform.parent = null;
        }

        public void RemoveParentOfOther(Transform other)
        {
            other.parent = null;
        }
    }
}