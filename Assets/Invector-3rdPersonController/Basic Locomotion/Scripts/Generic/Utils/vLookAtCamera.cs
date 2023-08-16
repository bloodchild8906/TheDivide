using UnityEngine;
using System.Collections;
namespace Invector
{
    public class vLookAtCamera : MonoBehaviour
    {
        [Tooltip("Align position to stay always on top of parent")]
        public bool alignUp = false;
        [Tooltip("Height of alignment on top of parent \n!!(Check alignUp to work)!!")]
        public float height = 1;
        [Tooltip("Detach of the parent on start \n!!(if alignUp not is checked, the object not follow the parent)!!")]
        public bool detachOnStart;
        [Tooltip("use smoth to look at camera")]
        public bool useSmothRotation = true;
        protected Transform parent;
        public bool justY;
        internal Camera cameraMain;
        void Start()
        {
            if (detachOnStart)
            {
                parent = transform.parent;
                transform.SetParent(null);
            }
            cameraMain = Camera.main;
        }

        private void OnEnable()
        {
            cameraMain = Camera.main;
            Align(false);
        }
        void FixedUpdate()
        {
            Align(useSmothRotation);
        }
        void Align(bool useSmooth)
        {
            if (alignUp && parent)
                transform.position = parent.position + Vector3.up * height;
            if (!cameraMain) return;
            var lookPos = cameraMain.transform.position - transform.position;
            lookPos.y = 0;
            var rotation = Quaternion.LookRotation(lookPos);
            if (useSmooth)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 4f);
                transform.eulerAngles = new Vector3(justY ? 0 : transform.eulerAngles.x, transform.eulerAngles.y, 0);
            }
            else
            {
                transform.eulerAngles = new Vector3(justY ? 0 : rotation.eulerAngles.x, rotation.eulerAngles.y, 0);
            }
        }
    }
}