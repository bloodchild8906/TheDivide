using UnityEngine;
namespace Invector.Utils
{
    public class vSetParent : MonoBehaviour
    {
        public void RemoveParent()
        {
            transform.parent = null;
        }

        public void RemoveParent(Transform target)
        {
            target.parent = null;
        }

        public void SetParent(Transform parent)
        {
            transform.parent = parent;
        }
        
    }
}