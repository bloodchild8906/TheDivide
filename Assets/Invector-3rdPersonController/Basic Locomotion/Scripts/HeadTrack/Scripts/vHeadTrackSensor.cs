using UnityEngine;
using System.Collections;
namespace Invector.vCharacterController
{
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(Rigidbody))]
    public class vHeadTrackSensor : MonoBehaviour
    {
        [HideInInspector]
        public vHeadTrack headTrack;
        public SphereCollider sphere;

        void OnDrawGizmos()
        {
            if (Application.isPlaying && sphere && headTrack)
            {
                sphere.radius = headTrack.distanceToDetect;
            }
        }

        void Start()
        {
            var _rigidB = GetComponent<Rigidbody>();
            sphere = GetComponent<SphereCollider>();
            sphere.isTrigger = true;
            _rigidB.useGravity = false;
            _rigidB.isKinematic = true;
            _rigidB.constraints = RigidbodyConstraints.FreezeAll;
            if (headTrack) sphere.radius = headTrack.distanceToDetect;
        }

        void OnTriggerEnter(Collider other)
        {
            if (headTrack != null) headTrack.OnDetect(other);
        }

        void OnTriggerExit(Collider other)
        {
            if (headTrack != null) headTrack.OnLost(other);
        }

    }
}