using UnityEngine;


namespace Invector.vShooter
{
    public class vArrow : MonoBehaviour
    {
        public vProjectileControl projectileControl;
        public Transform detachObject;
        [Tooltip("Use to raycast other side of the penetration")]
        public bool raycastBackSide = true;
        public bool alignToNormal = true;
        [HideInInspector]
        public float penetration;
        public bool debugPenetration;

        private void Start()
        {
            if (!projectileControl) projectileControl = GetComponent<vProjectileControl>();
        }

        public void OnDestroyProjectile(RaycastHit hit)
        {
            //Check if hit object contains a ArrowParent
            Transform arrowParent = hit.transform.Find("ArrowParent");
            if (!arrowParent)///Create a new Arrow Parent
            {
                arrowParent = new GameObject("ArrowParent").transform;
                arrowParent.position = hit.transform.position;
                arrowParent.parent = hit.transform;
            }

            detachObject.parent = arrowParent.transform;
            if (alignToNormal)
                detachObject.rotation = Quaternion.LookRotation(-hit.normal);
            detachObject.position = hit.point + transform.forward * penetration;
            if (debugPenetration) Debug.DrawLine(hit.point, hit.point + transform.forward * penetration, Color.red, 10f);

            if (projectileControl && raycastBackSide && penetration > 0f)
            {
                var pA = hit.point + transform.forward * penetration;
                var pb = hit.point + transform.forward * 0.001f;

                if (Physics.Linecast(pA, pb, out hit, projectileControl.hitLayer))
                {
                    projectileControl.onCastCollider.Invoke(hit);
                }
            }
        }
    }
}