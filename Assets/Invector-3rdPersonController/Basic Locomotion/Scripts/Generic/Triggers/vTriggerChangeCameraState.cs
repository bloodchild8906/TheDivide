using UnityEngine;
namespace Invector.vCharacterController
{
    [vClassHeader("Trigger Change Camera State", openClose = false)]
    public class vTriggerChangeCameraState : vMonoBehaviour
    {
        [Tooltip("Check if you want to lerp the state transitions, you can change the lerp value on the TPCamera - Smooth Follow variable")]
        public bool smoothTransition = true;
        public bool keepDirection = true;
        [vHelpBox("Keep it empty to Reset back to Default")]
        [Tooltip("Check your CameraState List and set the State here, use the same String value.\n*Leave this field empty to return the original state")]
        public string cameraState;
        public bool resetCameraStateOnExitTrigger;
        [Tooltip("Set a new target for the camera.\n*Leave this field empty to return the original target (Player)")]
        public string customCameraPoint;

        public Color gizmoColor = Color.green;
        private Component comp = null;

        public vThirdPersonInput tpInput;

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (tpInput == null || tpInput.gameObject != other.gameObject)
                {
                    tpInput = other.GetComponent<vThirdPersonInput>();
                }

                if (tpInput != null)
                {
                    if (cameraState != string.Empty)
                    {
                        tpInput.ChangeCameraState(cameraState, smoothTransition);
                    }
                    else if (cameraState == string.Empty)
                    {
                        tpInput.ResetCameraState();
                    }

                    if (!string.IsNullOrEmpty(customCameraPoint))
                    {                        
                        tpInput.customlookAtPoint = customCameraPoint;
                    }

                    tpInput.cc.keepDirection = keepDirection;/// set Input to keep Direction
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (resetCameraStateOnExitTrigger && other.gameObject.CompareTag("Player"))
            {
                if (tpInput != null)
                {
                    tpInput.ResetCameraState();
                }
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;

            comp = gameObject.GetComponent<BoxCollider>();

            if (comp != null)
            {
                gameObject.GetComponent<BoxCollider>().isTrigger = true;
                gameObject.GetComponent<BoxCollider>().center = Vector3.zero;
                gameObject.GetComponent<BoxCollider>().size = Vector3.one;
            }

            Gizmos.matrix = transform.localToWorldMatrix;
            if (comp == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }

            Gizmos.DrawCube(Vector3.zero, Vector3.one);
        }

        Vector3 getLargerScale(Vector3 value)
        {
            if (value.x > value.y || value.x > value.z)
            {
                return new Vector3(value.x, value.x, value.x);
            }

            if (value.y > value.x || value.y > value.z)
            {
                return new Vector3(value.y, value.y, value.y);
            }

            if (value.z > value.y || value.z > value.x)
            {
                return new Vector3(value.z, value.z, value.z);
            }

            return transform.localScale;
        }
    }
}