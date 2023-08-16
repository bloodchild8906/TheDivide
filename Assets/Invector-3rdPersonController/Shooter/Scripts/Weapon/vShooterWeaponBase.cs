using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Invector.vShooter
{
    [vClassHeader("Shooter Weapon", openClose = false)]
    public class vShooterWeaponBase : vMonoBehaviour
    {
        #region Variables

        [vEditorToolbar("Weapon Settings")]
        [Tooltip("The category of the weapon\n Used to the IK offset system. \nExample: HandGun, Pistol, Machine-Gun")]
        public string weaponCategory = "MyCategory";

        [SerializeField, Tooltip("Frequency of shots"), FormerlySerializedAs("shootFrequency")]
        protected float _shootFrequency;
        public virtual float shootFrequency { get { return _shootFrequency; } set { _shootFrequency = value; } }

        [vEditorToolbar("Ammo")]

        [Tooltip("Unlimited ammo")]
        public bool isInfinityAmmo;

        [Tooltip("Starting ammo")]
        [SerializeField, vHideInInspector("isInfinityAmmo", true), FormerlySerializedAs("ammo")]
        protected int _ammo;
        public virtual int ammo { get { return _ammo; } set { _ammo = value; } }

        [vEditorToolbar("Layer & Tag")]
        public List<string> ignoreTags = new List<string>();
        public LayerMask hitLayer = 1 << 0;

        [vEditorToolbar("Projectile")]
        [Tooltip("Prefab of the projectile")]
        public GameObject projectile;
        [Tooltip("Assign the muzzle of your weapon")]
        public Transform muzzle;
        [Tooltip("How many projectiles will spawn per shot")]
        [Range(1, 20)]
        public int projectilesPerShot = 1;
        [Range(0, 90)]
        [Tooltip("how much dispersion the weapon have")]
        public float dispersion = 0;
        [vToggleOption("DispersionShape", "Circle", "Quad")]
        public bool quadDispersion = false;
        [Range(0, 1000)]
        [Tooltip("Velocity of your projectile")]
        public float velocity = 380;

        [vHelpBox("If you're using the ItemManager attribute 'Damage' on your item, the damage will be always maxDamage, ignoring the distance or minDamage", vHelpBoxAttribute.MessageType.Info)]

        [Tooltip("Check this to calculate damage automatically based on distance using min and max damage, higher distance less damage, less distance more damage")]
        public bool damageByDistance = true;
        [Tooltip("Min distance to apply damage, used to evaluate the damage between minDamage and maxDamage")]
        [SerializeField, vHideInInspector("damageByDistance"), FormerlySerializedAs("minDamageDistance")]
        protected float _minDamageDistance = 8f;
        public virtual float minDamageDistance { get { return _minDamageDistance; } set { _minDamageDistance = value; } }
        [Tooltip("Max distance to apply damage, used to evaluate the damage between minDamage and maxDamage")]
        [SerializeField, vHideInInspector("damageByDistance"), FormerlySerializedAs("maxDamageDistance")]
        protected float _maxDamageDistance = 50f;
        public virtual float maxDamageDistance { get { return _maxDamageDistance; } set { _maxDamageDistance = value; } }
        [vHideInInspector("damageByDistance")]
        [SerializeField, Tooltip("Minimum damage caused by the shot, regardless the distance"), FormerlySerializedAs("minDamage")]
        protected int _minDamage;
        public virtual int minDamage { get { return _minDamage; } set { _minDamage = value; } }
        [SerializeField, Tooltip("Maximum damage caused by the close shot"), FormerlySerializedAs("maxDamage")]
        protected int _maxDamage;
        public virtual int maxDamage { get { return _maxDamage; } set { _maxDamage = value; } }

        [vEditorToolbar("Audio & VFX")]
        [Header("Audio")]
        public AudioSource source;
        public AudioClip fireClip;
        public AudioClip emptyClip;

        [Header("Effects")]
        public bool testShootEffect;
        public Light lightOnShot;
        [SerializeField]
        public ParticleSystem[] emittShurykenParticle;


        [HideInInspector]
        public OnDestroyEvent onDestroy;
        [System.Serializable]
        public class OnDestroyEvent : UnityEvent<GameObject> { }
        [System.Serializable]
        public class OnInstantiateProjectile : UnityEvent<vProjectileControl> { }

        [vEditorToolbar("Events")]
        public UnityEvent onShot, onEmptyClip;

        public OnInstantiateProjectile onInstantiateProjectile;

        protected virtual float _nextShootTime { get; set; }
        protected virtual float _nextEmptyClipTime { get; set; }
        protected virtual Transform sender { get; set; }
        #endregion

        #region Public Methods
        /// <summary>
        /// Apply additional velocity to the Shot projectile 
        /// </summary>
        public virtual float velocityMultiplierMod { get; set; }

        /// <summary>
        /// Apply additional damage to the projectile
        /// </summary>
        public virtual float damageMultiplierMod { get; set; }

        /// <summary>
        /// Weapon Name
        /// </summary>
        public virtual string weaponName
        {
            get
            {
                var value = gameObject.name.Replace("(Clone)", string.Empty);
                return value;
            }
        }

        /// <summary>
        /// Shoot to direction of the muzzle forward
        /// </summary>
        public virtual void Shoot()
        {
            Shoot(muzzle.position + muzzle.forward * 100f);
        }

        /// <summary>
        /// Shoot to direction of the muzzle forward
        /// </summary>
        /// <param name="sender">Sender to reference of the damage</param>
        /// <param name="successfulShot">Action to check if shoot is successful</param>
        public virtual void Shoot(Transform _sender = null, UnityAction<bool> successfulShot = null)
        {
            Shoot(muzzle.position + muzzle.forward * 100f, _sender, successfulShot);
        }

        /// <summary>
        /// Shoot to direction of the aim Position
        /// </summary>
        /// <param name="aimPosition">Aim position to override direction of the projectile</param>
        /// <param name="sender">ender to reference of the damage</param>
        /// <param name="successfulShot">Action to check if shoot is successful</param>
        public virtual void Shoot(Vector3 aimPosition, Transform _sender = null, UnityAction<bool> successfulShot = null)
        {
            Shoot(muzzle.position, aimPosition, _sender, successfulShot);
        }


        public virtual void Shoot(Vector3 startPoint, Vector3 endPoint, Transform _sender = null, UnityAction<bool> successfulShot = null)
        {
            if (HasAmmo())
            {
                if (!CanDoShot)
                {
                    return;
                }

                UseAmmo();
                this.sender = _sender != null ? _sender : transform;
                HandleShot(startPoint, endPoint);
                if (successfulShot != null)
                {
                    successfulShot.Invoke(true);
                }

                _nextShootTime = Time.time + shootFrequency;
                _nextEmptyClipTime = _nextShootTime;
            }
            else
            {
                if (!CanDoEmptyClip)
                {
                    return;
                }

                EmptyClipEffect();
                if (successfulShot != null)
                {
                    successfulShot.Invoke(false);
                }

                _nextEmptyClipTime = Time.time + shootFrequency;
            }
        }
        /// <summary>
        /// Check if can shoot by <seealso cref="shootFrequency"/>
        /// </summary>
        public virtual bool CanDoShot
        {
            get
            {
                bool _canShot = _nextShootTime < Time.time;
                return _canShot;
            }
        }
        /// <summary>
        /// Check if can do empty clip effect, <seealso cref="shootFrequency"/>
        /// </summary>
        public virtual bool CanDoEmptyClip
        {
            get
            {
                bool _canShot = _nextEmptyClipTime < Time.time;
                return _canShot;
            }
        }

        /// <summary>
        /// Use weapon Ammo
        /// </summary>
        /// <param name="count">count to use</param>
        public virtual void UseAmmo(int count = 1)
        {
            if (ammo <= 0)
            {
                return;
            }

            ammo -= count;
            if (ammo <= 0)
            {
                ammo = 0;
            }
        }

        /// <summary>
        /// Check if Weapon Has Ammo
        /// </summary>
        /// <returns></returns>
        public virtual bool HasAmmo()
        {

            return isInfinityAmmo || ammo > 0;
        }
        #endregion

        #region Protected Methods

        protected virtual void OnDestroy()
        {
            onDestroy.Invoke(gameObject);
        }

        private void OnApplicationQuit()
        {
            onDestroy.RemoveAllListeners();
        }
        protected virtual void HandleShot(Vector3 startPoint, Vector3 endPoint)
        {
            ShootBullet(startPoint, endPoint);
            ShotEffect();
        }
        public virtual Vector3 Dispersion(Vector3 aim, float dispersion)
        {
            return quadDispersion ? QuadDispersion(aim, dispersion) : CircleDispersion(aim, dispersion);
        }

        public virtual Vector3 CircleDispersion(Vector3 aim, float dispersion)
        {
            var rotatedAim = Quaternion.Euler(Random.insideUnitSphere * dispersion);
            aim = (rotatedAim) * aim;
            return aim;
        }

        public virtual Vector3 QuadDispersion(Vector3 aim, float dispersion)
        {
            var rotatedAim = Quaternion.Euler
                (
                Random.Range(-dispersion, dispersion),
                Random.Range(-dispersion, dispersion),
                Random.Range(-dispersion, dispersion)
                );

            aim = (rotatedAim) * aim;

            return aim.normalized;
        }
        //IEnumerator DebugDispersion(Vector3 startPoint, Vector3 endPoint)
        //{
        //    var dir = endPoint - startPoint;
        //    float time = 10;
        //    while (time>0)
        //    {
        //        var dispersionDir = Dispersion(dir.normalized, dispersion);
        //        (startPoint + dispersionDir * dir.magnitude).DebugPoint(Color.red, 10, 0.02f);
        //        yield return null;
        //        time -= Time.deltaTime;
        //    }
        //}

        protected virtual void ShootBullet(Vector3 startPoint, Vector3 endPoint)
        {
            var dir = endPoint - startPoint;
            //StartCoroutine(DebugDispersion(startPoint, endPoint));
            var rotation = Quaternion.LookRotation(dir);
            GameObject bulletObject = null;
            var velocityChanged = 0f;
            if (dispersion > 0 && projectile)
            {
                for (int i = 0; i < projectilesPerShot; i++)
                {
                    var dispersionDir = Dispersion(dir.normalized, dispersion);
                    var spreadRotation = Quaternion.LookRotation(dispersionDir);
                    bulletObject = Instantiate(projectile, startPoint, spreadRotation);

                    var pCtrl = bulletObject.GetComponent<vProjectileControl>();
                    if (pCtrl.debugTrajetory && i == 0)
                    {
                        startPoint.DebugPoint(Color.red, 10, 0.1f);
                        Debug.DrawLine(startPoint, endPoint, Color.red, 10);
                        endPoint.DebugPoint(Color.red, 10, 0.1f);
                    }
                    pCtrl.shooterTransform = sender;
                    pCtrl.ignoreTags = ignoreTags;
                    pCtrl.hitLayer = hitLayer;
                    pCtrl.damage.sender = sender;
                    pCtrl.startPosition = bulletObject.transform.position;
                    pCtrl.damageByDistance = damageByDistance;
                    pCtrl.maxDamage = (int)((maxDamage / projectilesPerShot) * damageMultiplier);
                    pCtrl.minDamage = (int)((minDamage / projectilesPerShot) * damageMultiplier);
                    pCtrl.minDamageDistance = minDamageDistance;
                    pCtrl.maxDamageDistance = maxDamageDistance;
                    onInstantiateProjectile.Invoke(pCtrl);
                    velocityChanged = velocity * velocityMultiplier;
                    ApplyForceToBullet(bulletObject, dispersionDir, velocityChanged);

                    pCtrl = CreateProjectileData(endPoint, velocityChanged, dispersionDir, pCtrl);
                }
            }
            else if (projectilesPerShot > 0 && projectile)
            {
                bulletObject = Instantiate(projectile, startPoint, rotation);
                var pCtrl = bulletObject.GetComponent<vProjectileControl>();
                if (pCtrl.debugTrajetory)
                {
                    startPoint.DebugPoint(Color.red, 10, 0.1f);
                    Debug.DrawLine(startPoint, endPoint, Color.red, 10);
                    endPoint.DebugPoint(Color.red, 10, 0.1f);
                }
                pCtrl.shooterTransform = sender;
                pCtrl.ignoreTags = ignoreTags;
                pCtrl.hitLayer = hitLayer;
                pCtrl.damage.sender = sender;
                pCtrl.startPosition = bulletObject.transform.position;
                pCtrl.damageByDistance = damageByDistance;
                pCtrl.maxDamage = (int)((maxDamage / projectilesPerShot) * damageMultiplier);
                pCtrl.minDamage = (int)((minDamage / projectilesPerShot) * damageMultiplier);
                pCtrl.minDamageDistance = minDamageDistance;
                pCtrl.maxDamageDistance = maxDamageDistance;
                onInstantiateProjectile.Invoke(pCtrl);
                velocityChanged = velocity * velocityMultiplier;

                ApplyForceToBullet(bulletObject, dir, velocityChanged);
            }
        }

        protected virtual vProjectileControl CreateProjectileData(Vector3 aimPosition, float velocityChanged, Vector3 dispersionDir, vProjectileControl pCtrl)
        {
            pCtrl.instantiateData = new vProjectileInstantiateData
            {
                aimPos = aimPosition,
                dir = dispersionDir,
                vel = velocityChanged
            };
            return pCtrl;
        }

        protected virtual void ApplyForceToBullet(GameObject bulletObject, Vector3 direction, float velocityChanged)
        {
            try
            {
                var _rigidbody = bulletObject.GetComponent<Rigidbody>();
                _rigidbody.mass = _rigidbody.mass / projectilesPerShot;//Change mass per projectiles count.

                _rigidbody.AddForce((direction.normalized * velocityChanged), ForceMode.VelocityChange);
            }
            catch
            {

            }
        }

        protected virtual float damageMultiplier
        {
            get
            {
                return 1 + damageMultiplierMod;
            }
        }

        protected virtual float velocityMultiplier
        {
            get
            {
                return 1 + velocityMultiplierMod;
            }
        }

        #region Effects
        protected virtual void ShotEffect()
        {
            onShot.Invoke();

            StopCoroutine(LightOnShoot());
            if (source && fireClip)
            {

                source.PlayOneShot(fireClip);
            }

            StartCoroutine(LightOnShoot(0.037f));
            StartEmitters();
        }

        protected virtual void StopSound()
        {
            if (source)
            {
                source.Stop();
            }
        }

        protected virtual IEnumerator LightOnShoot(float time = 0)
        {
            if (lightOnShot)
            {
                lightOnShot.enabled = true;

                yield return new WaitForSeconds(time);
                lightOnShot.enabled = false;
            }
        }

        protected virtual void StartEmitters()
        {
            if (emittShurykenParticle != null)
            {
                foreach (ParticleSystem pe in emittShurykenParticle)
                {
                    pe.Emit(1);
                }
            }
        }

        protected virtual void StopEmitters()
        {
            if (emittShurykenParticle != null)
            {
                foreach (ParticleSystem pe in emittShurykenParticle)
                {
                    pe.Stop();
                }
            }
        }

        protected virtual void EmptyClipEffect()
        {
            if (source && emptyClip)
            {
                source.PlayOneShot(emptyClip);
            }

            onEmptyClip.Invoke();
        }
        #endregion

        #endregion
    }
}