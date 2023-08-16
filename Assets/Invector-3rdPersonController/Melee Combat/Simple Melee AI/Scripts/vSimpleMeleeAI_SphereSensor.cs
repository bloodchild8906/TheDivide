using System.Collections.Generic;
using UnityEngine;
namespace Invector.vCharacterController.AI
{
    public class vSimpleMeleeAI_SphereSensor : MonoBehaviour
    {
        public Transform root;

        public List<Transform> targetsInArea;
        protected bool getFromDistance;
        protected float lastDetectionDistance;

        protected virtual void Start()
        {
            targetsInArea = new List<Transform>();
        }

        public virtual void AddTarget(Transform _transform)
        {
            if (!targetsInArea.Contains(_transform))
            {
                targetsInArea.Add(_transform);
            }
        }

        public virtual void SetColliderRadius(float radius)
        {
            var collider = GetComponent<SphereCollider>();
            if (collider)
            {
                collider.radius = radius;
            }
        }

        public virtual Transform GetTargetTransform()
        {
            if (targetsInArea.Count > 0)
            {
                SortTargets();
                if (targetsInArea.Count > 0)
                {
                    return targetsInArea[0].transform;
                }
            }
            return null;
        }

        public virtual vIHealthController GetTargetvCharacter()
        {
            if (targetsInArea.Count > 0)
            {
                SortCharacters();
                if (targetsInArea.Count > 0)
                {
                    var vChar = targetsInArea[0].GetComponent<vIHealthController>();
                    if (vChar != null && vChar.currentHealth > 0)
                    {
                        return vChar;
                    }
                }
            }

            return null;
        }

        protected virtual void SortCharacters()
        {
            for (int i = targetsInArea.Count - 1; i >= 0; i--)
            {
                var t = targetsInArea[i];
                var dist = Vector3.Distance(transform.position, targetsInArea[i].transform.position);
                if (t == null || dist > lastDetectionDistance || t.GetComponent<vIHealthController>() == null)
                {
                    targetsInArea.RemoveAt(i);
                }
            }


            if (getFromDistance)
            {
                targetsInArea.Sort(delegate (Transform c1, Transform c2)
                {
                    return Vector3.Distance(this.transform.position, c1 != null ? c1.transform.position : Vector3.one * Mathf.Infinity).CompareTo
                        ((Vector3.Distance(this.transform.position, c2 != null ? c2.transform.position : Vector3.one * Mathf.Infinity)));
                });
            }
        }

        protected virtual void SortTargets()
        {
            for (int i = targetsInArea.Count - 1; i >= 0; i--)
            {
                var t = targetsInArea[i];
                var dist = Vector3.Distance(transform.position, targetsInArea[i].transform.position);
                if (t == null || dist > lastDetectionDistance)
                {
                    targetsInArea.RemoveAt(i);
                }
            }
            if (getFromDistance)
            {
                targetsInArea.Sort(delegate (Transform c1, Transform c2)
                {
                    return Vector3.Distance(this.transform.position, c1 != null ? c1.transform.position : Vector3.one * Mathf.Infinity).CompareTo
                        ((Vector3.Distance(this.transform.position, c2 != null ? c2.transform.position : Vector3.one * Mathf.Infinity)));
                });
            }
        }

        public virtual void CheckTargetsAround(float FOV, float minDistance, float maxDistance, vTagMask detectTags, LayerMask detectMask, bool getTargetFromDistance = false)
        {
            this.getFromDistance = getTargetFromDistance;
            lastDetectionDistance = maxDistance;
            var targetsAround = Physics.OverlapSphere(transform.position, maxDistance, detectMask);
            targetsAround = System.Array.FindAll(targetsAround, t =>
                                                 (root && root != t.transform)
                                                 && (detectTags != null && detectTags.Count > 0 && detectTags.Contains(t.gameObject.tag))
                                                 && InFovAngle(t.transform, minDistance, FOV));
            targetsInArea = System.Array.ConvertAll(targetsAround, c => c.transform).vToList();
        }

        protected virtual bool InFovAngle(Transform target, float minDistance, float FOV)
        {
            var dist = Vector3.Distance(transform.position, target.position);
            if (dist < minDistance)
            {
                return true;
            }

            var rot = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
            var detectionAngle = transform.eulerAngles;
            var newAngle = rot.eulerAngles - detectionAngle;
            var fovAngleY = newAngle.NormalizeAngle().y;
            var fovAngleX = newAngle.NormalizeAngle().x;
            if (fovAngleY <= (FOV * 0.5f) && fovAngleY >= -(FOV * 0.5f) && fovAngleX <= (FOV * 0.5f) && fovAngleX >= -(FOV * 0.5f))
            {
                return true;
            }

            return false;
        }
        //void OnTriggerEnter(Collider other)
        //{
        //    if (tagsToDetect.Contains(other.gameObject.tag) && !targetsInArea.Contains(other.transform))
        //        targetsInArea.Add(other);
        //}

        //void OnTriggerExit(Collider other)
        //{
        //    if (tagsToDetect.Contains(other.gameObject.tag) && targetsInArea.Contains(other.transform))
        //        targetsInArea.Remove(other);
        //}
    }
}

