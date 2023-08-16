using UnityEngine;
namespace Invector.Utils
{
    public class vUpdateUIPosition : MonoBehaviour
    {
        public Transform referenceLocalParent;

        public bool updateLocalX, updateLocalY, updateLocalZ;
        
        public bool limitOnBox;
        [vHideInInspector("limitOnBox")]
        public BoxCollider box;
        public void UpdatePosition(GameObject target)
        {
            SetLocalPosition(target.transform.position);
        }

        public void UpdatePosition(Collider target)
        {
            SetLocalPosition(target.transform.position);
        }

        public void UpdatePosition(Transform target)
        {
            SetLocalPosition(target.position);
        }
        void SetLocalPosition(Vector3 position)
        {
            if (limitOnBox && box) position = box.ClosestPointOnBounds(position);
            var localPosition = referenceLocalParent.InverseTransformPoint(position);
            var selfLocalPosition = transform.localPosition;
            if (updateLocalX) selfLocalPosition.x = localPosition.x;
            if (updateLocalY) selfLocalPosition.y = localPosition.y;
            if (updateLocalZ) selfLocalPosition.z = localPosition.z;
            transform.localPosition = selfLocalPosition;
        }
    }
}