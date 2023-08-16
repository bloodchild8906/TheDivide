using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Invector
{
    [RequireComponent(typeof(BoxCollider))]
    [vClassHeader("SimpleTrigger", openClose = false, useHelpBox = true, helpBoxText = "Tags and Layer To Detect : Use this to filter tags and layer that can interact with trigger, Select Nothing  to ignore filter")]
    public class vSimpleTrigger : vMonoBehaviour
    {
        public static bool drawGizmos = true;
        [System.Serializable]
        public class vTriggerEvent : UnityEvent<Collider> { }
        [vButton("ToggleGizmos", "ToggleGizmos", typeof(vSimpleTrigger), false)]
        public bool useFilter = true;
        public bool ignoreIsTriggerColliders;
        public bool debugMode;
        public vTagMask tagsToDetect = new List<string>() { "Player" };
        public LayerMask layersToDetect = 0;
        public vTriggerEvent onTriggerEnter;
        public vTriggerEvent onTriggerExit;
        public vTriggerEvent onTriggerStay;

        protected bool inCollision;
        protected bool triggerStay;
        protected Collider other;
        protected BoxCollider _selfCollider;
        public void ToggleGizmos()
        {
            drawGizmos = !drawGizmos;
        }
        public virtual BoxCollider selfCollider
        {
            get
            {
                if (!_selfCollider && transform.GetComponent<BoxCollider>() == null)
                {
                    _selfCollider = gameObject.AddComponent<BoxCollider>();
                }
                else if (!_selfCollider)
                {
                    _selfCollider = transform.GetComponent<BoxCollider>();
                }

                return _selfCollider;
            }
            protected set
            {
                _selfCollider = value;
            }
        }

        protected virtual void OnDrawGizmos()
        {
            if (!drawGizmos)
            {
                return;
            }

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, (transform.lossyScale));
            Vector3 position = selfCollider.center;
            Vector3 size = Vector3.one;
            size.x *= selfCollider.size.x;
            size.y *= selfCollider.size.y;
            size.z *= selfCollider.size.z;
            Gizmos.color = Color.green * 0.8f;
            Gizmos.DrawWireCube(position, size);

            Color red = new Color(1, 0, 0, 0.2f);
            Color green = new Color(0, 1, 0, 0.2f);
            Gizmos.color = inCollision && Application.isPlaying ? red : green;
            Gizmos.DrawCube(position, size);
        }

        protected virtual void Start()
        {
            inCollision = false;
            selfCollider.isTrigger = true;
        }

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (this.enabled && this.other == null && CanTrigger(other) && IsInTagMask(other.gameObject.tag) && IsInLayerMask(other.gameObject.layer))
            {
                inCollision = true;
                this.other = other;
                onTriggerEnter.Invoke(other);
                if (debugMode)
                {
                    Debug.Log(other.gameObject.name + "TriggerEnter");
                }

                if (this.enabled && gameObject.activeInHierarchy)
                {
                    StartCoroutine(TriggerStayRoutine());
                }
            }
        }

        private bool CanTrigger(Collider other)
        {
            return ignoreIsTriggerColliders ? !other.isTrigger : true;
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (this.enabled && this.other != null && CanTrigger(other) && this.other.gameObject == other.gameObject)
            {
                inCollision = false;
                onTriggerExit.Invoke(other);
                if (debugMode)
                {
                    Debug.Log(other.gameObject.name + "TriggerExit");
                }

                this.other = null;
            }
        }

        protected virtual bool IsInTagMask(string tag)
        {
            if (tagsToDetect.Count == 0)
            {
                return true;
            }
            else
            {
                return tagsToDetect.Contains(tag);
            }
        }

        protected virtual bool IsInLayerMask(int layer)
        {
            return (layersToDetect.value == 0 || (layersToDetect.value & (1 << layer)) > 0);
        }

        protected IEnumerator TriggerStayRoutine()
        {
            while (other != null)
            {
                if (other == null || !other.gameObject.activeInHierarchy)
                {
                    OnTriggerExit(other);
                    break;
                }
                else
                {
                    onTriggerStay.Invoke(other);
                    if (debugMode)
                    {
                        Debug.Log(other.gameObject.name + "TriggerStay");
                    }
                }

                yield return null;
            }
        }
    }
}