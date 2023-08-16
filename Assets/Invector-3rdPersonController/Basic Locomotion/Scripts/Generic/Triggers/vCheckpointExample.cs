using Invector;
using Invector.vCharacterController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector.Utils
{
    /// <summary>
    /// Simple Checkpoint Example, works by updating the vGameController SpawnPoint to this transform position/rotation.
    /// </summary>    
    [RequireComponent(typeof(BoxCollider))]
    public class vCheckpointExample : MonoBehaviour
    {
        vGameController gm;

        public UnityEvent onTriggerEnter;

        void Start()
        {
            gm = GetComponentInParent<vGameController>();
            this.GetComponent<BoxCollider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                vHUDController.instance.ShowText("Checkpoint reached!");
                gm.spawnPoint = this.gameObject.transform;
                onTriggerEnter.Invoke();
                this.gameObject.SetActive(false);
            }
        }
    }
}


