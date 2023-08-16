using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Invector
{
    public class vDestroyOnTrigger : MonoBehaviour
    {
        public List<string> targsToDestroy;
        public float destroyDelayTime;

        void OnTriggerEnter(Collider other)
        {
            if (targsToDestroy.Contains(other.gameObject.tag))
            {
                Destroy(other.gameObject, destroyDelayTime);
            }
        }
    }
}