using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.Utils
{
    public class vDestroyChildrens : MonoBehaviour
    {

        public virtual void DestroyChildrens()
        {
            DestroyChildrens(transform);
        }
        public virtual void DestroyChildrensOfOther(Transform target)
        {
            DestroyChildrens(target);
        }
        protected virtual void DestroyChildrens(Transform target)
        {
            int childs = target.childCount;
            for (int i = childs - 1; i >= 0; i--)
            {
                Destroy(target.GetChild(i).gameObject);
            }
        }
    }
}
