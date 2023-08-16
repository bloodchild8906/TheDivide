using System.Collections.Generic;
using UnityEngine;

namespace Invector.vShooter
{
    using Invector.vEventSystems;
    [vClassHeader("Projectile Control", "The damage value is changed from minDamage, maxDamage, DropOffStart, DropOffEnd of the ShooterWeapon", openClose = false)]
    public class vProjectileControl : vMonoBehaviour
    {
        public vBulletLifeSettings bulletLifeSettings;
        public int bulletLife = 100;
        public bool debugTrajetory;
        public bool debugHittedObject;
        public vDamage damage;
        public float forceMultiplier = 1;
        public bool destroyOnCast = true;
        [Tooltip("Control Trail renderer")]
        public TrailRenderer trail;
        public ProjectilePassDamage onPassDamage;
        public ProjectileCastColliderEvent onCastCollider;
        public ProjectileCastColliderEvent onDestroyProjectile;
        public vProjectileInstantiateData instantiateData;

        internal bool damageByDistance;
        internal float velocity = 580;
        internal int minDamage;
        internal int maxDamage;
        internal float minDamageDistance = 8f;
        internal float maxDamageDistance = 50f;
        internal Vector3 startPosition;
        internal LayerMask hitLayer = -1;
        internal List<string> ignoreTags = new List<string>();
        internal Transform shooterTransform;

        protected Vector3 previousPosition;
        protected Rigidbody _rigidBody;
        protected Color debugColor = Color.green;
        protected int debugLife;
        protected float castDist;
        protected List<Vector3> trajectoryPositions = new List<Vector3>();

        protected virtual void Start()
        {
            transform.SetParent(vObjectContainer.root, true);
            debugLife = bulletLife;
            _rigidBody = GetComponent<Rigidbody>();
            startPosition = transform.position;
            previousPosition = transform.position - transform.forward * 0.1f;

            if (trail)
            {
                AddTrailPosition();
            }
        }

        protected virtual void Update()
        {
            RaycastHit hitInfo;
            if (_rigidBody.velocity.magnitude > 1)
            {
                transform.rotation = Quaternion.LookRotation(_rigidBody.velocity.normalized, transform.up);
            }

            if (Physics.Linecast(previousPosition, transform.position + transform.forward * 0.5f, out hitInfo, hitLayer))
            {
                if (!hitInfo.collider)
                {
                    return;
                }

                var dist = Vector3.Distance(startPosition, transform.position) + castDist;
                if (!(ignoreTags.Contains(hitInfo.collider.gameObject.tag) || (shooterTransform != null && hitInfo.collider.transform.IsChildOf(shooterTransform))))
                {
                    if (debugHittedObject)
                    {
                        Debug.Log(hitInfo.collider.gameObject.name, hitInfo.collider);
                    }

                    onCastCollider.Invoke(hitInfo);
                    damage.damageValue = maxDamage;
                    if (damageByDistance)
                    {
                        var result = 0f;
                        var damageDifence = maxDamage - minDamage;

                        //Calc damage per distance
                        if (dist - minDamageDistance >= 0)
                        {
                            int percentComplete = (int)System.Math.Round((double)(100 * (dist - minDamageDistance)) / (maxDamageDistance - minDamageDistance));
                            result = Mathf.Clamp(percentComplete * 0.01f, 0, 1f);
                            damage.damageValue = maxDamage - (int)(damageDifence * result);
                        }
                        else
                        {
                            damage.damageValue = maxDamage;
                        }
                    }
                    damage.hitPosition = hitInfo.point;
                    damage.receiver = hitInfo.collider.transform;
                    damage.force = transform.forward * damage.damageValue * forceMultiplier;
                    if (damage.damageValue > 0)
                    {
                        onPassDamage.Invoke(damage);

                        hitInfo.collider.gameObject.ApplyDamage(damage, damage.sender ? damage.sender.GetComponent<vIMeleeFighter>() : null);
                    }

                    var rigb = hitInfo.collider.gameObject.GetComponent<Rigidbody>();
                    if (rigb)
                    {                       
                        rigb.AddForce(transform.forward * damage.damageValue * forceMultiplier, ForceMode.Impulse);
                    }

                    startPosition = transform.position;
                    castDist = dist;


                    if (destroyOnCast)
                    {
                        if (bulletLifeSettings)
                        {
                            var bulletLifeInfo = bulletLifeSettings.GetReduceLife(hitInfo.collider.gameObject.tag, hitInfo.collider.gameObject.layer);
                            bulletLife -= bulletLifeInfo.lostLife;
                            if (debugTrajetory)
                            {
                                DrawHitPoint(hitInfo.point);
                            }

                            var crossed = false;

                            if (bulletLife > 0 && !bulletLifeInfo.ricochet)
                            {
                                var position = transform.position = hitInfo.point + transform.forward * 0.001f;

                                if (trail)
                                {
                                    trail.AddPosition(transform.position);
                                }

                                if (debugTrajetory)
                                {
                                    Debug.DrawLine(transform.position, previousPosition, debugColor, 10f);
                                }

                                for (float i = 0; i <= bulletLifeInfo.maxThicknessToCross; i += 0.01f)
                                {

                                    var pointToCheck = position + transform.forward * (i);
                                    if (Physics.Linecast(pointToCheck, position))
                                    {
                                        hitInfo.point = pointToCheck;
                                        hitInfo.normal = transform.forward;
                                        onCastCollider.Invoke(hitInfo);
                                        crossed = true;
                                        break;
                                    }
                                }
                                if (crossed)
                                {
                                    if (trail)
                                    {
                                        AddTrailPosition();
                                    }
                                }
                            }

                            if (!crossed && !bulletLifeInfo.ricochet)
                            {
                                bulletLife = 0;
                                transform.position = hitInfo.point;
                                if (debugTrajetory)
                                {
                                    Debug.DrawLine(transform.position, previousPosition, debugColor, 10f);
                                }

                                onDestroyProjectile.Invoke(hitInfo);
                                if (trail && trail.gameObject != this.gameObject)
                                {
                                    if (trail)
                                    {
                                        AddTrailPosition();
                                    }
                                    trail.transform.SetParent(vObjectContainer.root);
                                }
                                Destroy(gameObject);
                                return;
                            }

                            maxDamage -= (maxDamage) - ((maxDamage * bulletLifeInfo.lostDamage) / 100);
                            minDamage -= (minDamage) - ((minDamage * bulletLifeInfo.lostDamage) / 100);
                            if (maxDamage < 0)
                            {
                                maxDamage = 0;
                            }

                            if (minDamage < 0)
                            {
                                minDamage = 0;
                            }

                            var x = Random.Range(bulletLifeInfo.minChangeTrajectory, bulletLifeInfo.maxChangeTrajectory) * (Random.Range(-1, 1) >= 0 ? 1 : -1);
                            var y = Random.Range(bulletLifeInfo.minChangeTrajectory, bulletLifeInfo.maxChangeTrajectory) * (Random.Range(-1, 1) >= 0 ? 1 : -1);

                            if (y > 60 || y < -60)
                            {
                                x = Mathf.Clamp(x, -15, 15);
                            }

                            if (x != 0 || y != 0)
                            {
                                var dir = Quaternion.Euler(x, y, 0) * _rigidBody.velocity;
                                if (dir != Vector3.zero)
                                {
                                    _rigidBody.velocity = dir * (bulletLifeInfo.ricochet ? -1 : 1);

                                    transform.forward = dir * (bulletLifeInfo.ricochet ? -1 : 1);
                                }
                            }
                            if (debugTrajetory)
                            {
                                var lostedLifePercent = (bulletLife / (float)debugLife) * 100f;
                                debugColor = lostedLifePercent > 76 ? Color.green : lostedLifePercent > 51 ? Color.yellow : lostedLifePercent > 26 ? new Color(1, .5f, 0) : Color.red;
                                debugColor.a = 0.5f;
                            }
                        }
                        else
                        {
                            bulletLife = 0;
                        }

                        if (bulletLife <= 0 || bulletLifeSettings == null)
                        {
                            transform.position = hitInfo.point;
                            if (debugTrajetory)
                            {
                                Debug.DrawLine(transform.position, previousPosition, debugColor, 10f);
                            }

                            onDestroyProjectile.Invoke(hitInfo);
                            if (trail && trail.gameObject != this.gameObject)
                            {
                                if (trail)
                                {
                                    AddTrailPosition();
                                }

                                trail.transform.SetParent(vObjectContainer.root);
                            }
                            Destroy(gameObject);
                            return;
                        }
                    }
                }
                else
                {
                    transform.position = hitInfo.point + transform.forward * 0.001f;
                    if (trail && trail.gameObject != this.gameObject)
                    {

                        if (trail)
                        {
                            AddTrailPosition();
                        }
                    }
                    if (debugTrajetory)
                    {
                        Debug.DrawLine(transform.position, previousPosition, debugColor, 10f);
                    }
                }
            }
            else
            {
                if (debugTrajetory)
                {
                    Debug.DrawLine(transform.position, previousPosition, debugColor, 10f);
                }
            }

            previousPosition = transform.position;


        }

        private void AddTrailPosition()
        {
            if (trajectoryPositions.Count > 0)
            {
                var lastPosition = trajectoryPositions[trajectoryPositions.Count - 1];
                var distance = Vector3.Distance(lastPosition, transform.position);
                var dir = transform.position - lastPosition;
                var count = (int)(distance / 0.5f);
                for (int i = 0; i < count; i++)
                {
                    trajectoryPositions.Add(lastPosition + dir.normalized * 0.5f);
                    if (debugTrajetory)
                    {
                        Debug.DrawRay(lastPosition, Vector3.up * .1f, Color.red, 10);
                    }

                    lastPosition = lastPosition + dir.normalized * 0.5f;
                }

            }
            else
            {
                trajectoryPositions.Add(transform.position);
            }

            trail.Clear();
            var position = trajectoryPositions.ToArray();

            trail.AddPositions(position);

        }

        void DrawHitPoint(Vector3 point)
        {
            Debug.DrawRay(point, -transform.forward * 0.1f, Color.red, 10f);
            Debug.DrawRay(point, transform.right * 0.1f, Color.red, 10f);
            Debug.DrawRay(point, -transform.right * 0.1f, Color.red, 10f);
            Debug.DrawRay(point, transform.up * 0.1f, Color.red, 10f);
            Debug.DrawRay(point, -transform.up * 0.1f, Color.red, 10f);
        }

        public void RemoveParentOfOther(Transform other)
        {
            other.SetParent(vObjectContainer.root, true);
        }

        [System.Serializable]
        public class ProjectileCastColliderEvent : UnityEngine.Events.UnityEvent<RaycastHit> { }
        [System.Serializable]
        public class ProjectilePassDamage : UnityEngine.Events.UnityEvent<vDamage> { }

    }
}