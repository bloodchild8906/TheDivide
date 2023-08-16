using UnityEngine;

namespace Invector.Utils
{
    [vClassHeader("v Instantiate", openClose = false)]
    public class vInstantiate : vMonoBehaviour
    {
        public GameObject prefab;
        public bool instantiateOnStart;
        public bool setThisAsParent;

        protected virtual void Start()
        {
            if (instantiateOnStart)
                InstantiateObject();
        }

        public virtual void InstantiateObject()
        {
            if (prefab)
            {
                var obj = Instantiate(prefab, transform.position, transform.rotation);
                obj.SetActive(true);
                if (setThisAsParent) obj.transform.parent = transform;
            }
        }
    }
}
