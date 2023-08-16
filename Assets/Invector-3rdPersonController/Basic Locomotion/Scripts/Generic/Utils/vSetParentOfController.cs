using Invector.vCharacterController;
using UnityEngine;

namespace Invector.Utils
{
    public class vSetParentOfController : MonoBehaviour
    {
        [vHelpBox("Set this GameObject as parent of the Controller")]

        private vThirdPersonController cc;

        public UnityEngine.Events.UnityEvent onStart;

        private void Start()
        {
            cc = GetComponentInParent<vThirdPersonController>();
            transform.parent = cc.transform;

            onStart.Invoke();
        }
    }
}