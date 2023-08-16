using UnityEngine;

namespace Invector.Throw
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    [vClassHeader("Throwable Object", openClose = false)]
    public class vThrowableObject : vMonoBehaviour
    {
        [System.Serializable]
        public class ThrowSenderEvent : UnityEngine.Events.UnityEvent<Transform> { }

        protected Rigidbody _rigidbody;
        public vThrowSettings throwSettings;
        public vThrowVisualSettings throwVisualSettings;
        public float indicatorRangeMultiplier = 1f;
        public ThrowSenderEvent onThrow;
        public virtual Rigidbody selfRigidbody
        {
            get
            {
                if (_rigidbody == null)
                {
                    if (TryGetComponent(out Rigidbody r)) _rigidbody = r;
                    else _rigidbody = gameObject.AddComponent<Rigidbody>();
                }
                return _rigidbody;
            }

        }

        public static implicit operator Rigidbody(vThrowableObject throwable) => throwable.selfRigidbody;
        public virtual bool isKinematic
        {
            get
            {
                return selfRigidbody.isKinematic;
            }
            set
            {
                selfRigidbody.isKinematic = value;
            }
        }
    }
}