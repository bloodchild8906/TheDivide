using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.Utils
{
    public class vTargetLookAt : MonoBehaviour
    {
        public Transform target;
        public float smooth;
        public float offsetHeight;
        public bool limitDistance;
        public float minDistanceToLook;
        // Update is called once per frame
        void Update()
        {
            if (!target) return;
            var dir = target.position+Vector3.up*offsetHeight - transform.position;
            Quaternion rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
            if(!limitDistance|| dir.magnitude>minDistanceToLook )
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, smooth * Time.deltaTime);
        }
    }
}