using UnityEngine;
using System.Collections;

namespace Invector
{    
    [vClassHeader("Destroy GameObject", openClose = false)]
    public class vDestroyGameObject : vMonoBehaviour
    {
        public float delay;
        public UnityEngine.Events.UnityEvent onDestroy;
        IEnumerator Start()
        {
            yield return new WaitForSeconds(delay);
            onDestroy.Invoke();
            Destroy(gameObject);
        }
    }
}