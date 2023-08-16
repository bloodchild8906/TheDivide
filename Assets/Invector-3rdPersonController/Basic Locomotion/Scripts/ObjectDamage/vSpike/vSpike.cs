using UnityEngine;
namespace Invector
{
    public class vSpike : MonoBehaviour
    {
        HingeJoint joint;
        [HideInInspector]
        public vSpikeControl control;

        void Start()
        {
            joint = GetComponent<HingeJoint>();
        }
        bool inConnect;
        Transform impaled;

        void OnCollisionEnter(Collision collision)
        {
            if (collision.rigidbody != null && collision.collider.GetComponent<vCharacterController.vDamageReceiver>() != null && !inConnect)
            {
                bool condition = control == null ? true : !control.attachColliders.Contains(collision.collider.transform);
                if (control) control.attachColliders.Add(collision.collider.transform);
                if (condition)
                {
                    inConnect = true;
                    if (joint && collision.rigidbody)
                        joint.connectedBody = collision.rigidbody;

                    impaled = collision.transform;
                    foreach (Rigidbody body in collision.transform.root.GetComponentsInChildren<Rigidbody>())
                    {
                        if (!body.isKinematic)
                        {
                            body.velocity = Vector3.zero;
                        }
                    }
                    var damageReceiver = collision.collider.GetComponent<vCharacterController.vDamageReceiver>();
                    if (damageReceiver && damageReceiver.ragdoll && damageReceiver.ragdoll.iChar != null) damageReceiver.ragdoll.iChar.ChangeHealth((int)-damageReceiver.ragdoll.iChar.currentHealth);
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (other.transform != null && impaled != null && other.transform == impaled)
            {
                if (joint)
                    joint.connectedBody = null;
                impaled = null;
                if (control != null && control.attachColliders.Contains(impaled))
                    control.attachColliders.Remove(impaled);
                inConnect = false;
            }
        }
    }
}