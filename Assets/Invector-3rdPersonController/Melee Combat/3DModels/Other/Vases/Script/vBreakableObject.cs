using System.Collections;
using UnityEngine;

namespace Invector
{
    public class vBreakableObject : MonoBehaviour, vIDamageReceiver
    {
        public Transform brokenObject;
        [Header("Break Object Settings")]
        [Tooltip("Break objet  OnTrigger with Player rolling")]
        public bool breakOnPlayerRoll = true;
        [Tooltip("Break objet  OnCollision with other object")]
        public bool breakOnCollision = true;
        [Tooltip("Rigidbody velocity to break OnCollision whit other object")]
        public float maxVelocityToBreak = 5f;

        public UnityEngine.Events.UnityEvent OnBroken;

        [SerializeField] protected OnReceiveDamage _onStartReceiveDamage = new OnReceiveDamage();
        [SerializeField] protected OnReceiveDamage _onReceiveDamage = new OnReceiveDamage();

        public OnReceiveDamage onStartReceiveDamage { get { return _onStartReceiveDamage; } protected set { _onStartReceiveDamage = value; } }
        public OnReceiveDamage onReceiveDamage { get { return _onReceiveDamage; } protected set { _onReceiveDamage = value; } }

        protected bool isBroken;
        protected Collider _collider;
        protected Rigidbody _rigidBody;

        protected virtual void Start()
        {
            _collider = GetComponent<Collider>();
            _rigidBody = GetComponent<Rigidbody>();
        }

        public void TakeDamage(vDamage damage)
        {
            onStartReceiveDamage.Invoke(damage);
            if (!isBroken)
            {
                isBroken = true;
                StartCoroutine(BreakObject());
            }
            if (damage.damageValue > 0)
            {
                onReceiveDamage.Invoke(damage);
            }
        }

        protected virtual IEnumerator BreakObject()
        {
            if (_rigidBody) Destroy(_rigidBody);
            if (_collider) Destroy(_collider);
            yield return new WaitForEndOfFrame();
            brokenObject.transform.parent = null;
            brokenObject.gameObject.SetActive(true);
            OnBroken.Invoke();
            Destroy(gameObject);
        }

#if INVECTOR_BASIC
        protected virtual void OnTriggerStay(Collider other)
        {
            if (breakOnPlayerRoll && other.gameObject.CompareTag("Player"))
            {
                var thirdPerson = other.gameObject.GetComponent<Invector.vCharacterController.vThirdPersonController>();
                if (thirdPerson && thirdPerson.isRolling && !isBroken)
                {
                    isBroken = true;
                    StartCoroutine(BreakObject());
                }
            }
        }
#endif
        protected virtual void OnCollisionEnter(Collision other)
        {
            if (breakOnCollision && _rigidBody && _rigidBody.velocity.magnitude > 5f && !isBroken)
            {
                isBroken = true;
                StartCoroutine(BreakObject());
            }
        }
    }
}