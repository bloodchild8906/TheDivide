using UnityEngine;

namespace Invector
{
    [vClassHeader("Check Effect Visibility", "Use it to identify if the object is within the camera frustrum and not hide by obstacles", openClose = false)]
    public class vCheckEffectVisibility : vMonoBehaviour
    {
        public LayerMask layerObstacle;
        [Tooltip("The point to check if effect is visible")]
        public Vector3 checkPoint = new Vector3(0, 1.5f, 0);
        public bool debugMode;
        public vEffectReceiver.vEffectEvent OnVisible;
        public vEffectReceiver.vEffectEvent OnNotVisible;

        private Camera mainCamera;

        Vector3 visibilityPoint => transform.TransformPoint(checkPoint);

        private void OnDrawGizmos()
        {
            if (debugMode)
                Gizmos.DrawSphere(visibilityPoint, 0.1f);
        }

        private void Start()
        {
            mainCamera = Camera.main;
        }

        public void CheckEffectIsVisible(vIEffect effect)
        {
            if (CheckIfEffectIsVisible(effect))
            {
                OnVisible.Invoke(effect);
            }
            else
            {
                OnNotVisible.Invoke(effect);
            }
        }

        bool IsObjectVisible(Camera camera, Vector3 position)
        {
            // Convert the world position of the object to viewport coordinates
            Vector3 viewportPoint = camera.WorldToViewportPoint(position);

            // Check if the viewport coordinates are within the range of (0,0) to (1,1)
            bool isVisible = viewportPoint.x >= 0f && viewportPoint.x <= 1f &&
                             viewportPoint.y >= 0f && viewportPoint.y <= 1f &&
                             viewportPoint.z >= 0f;
            return isVisible;
        }

        bool CheckIfEffectIsVisible(vIEffect effect)
        {
            if (mainCamera != null)
            {
                // Check if the object is within the camera frustum
                if (IsObjectVisible(mainCamera, effect.EffectPosition))
                {
                    // Check if any objects are blocking the view between the camera and the object
                    RaycastHit hitInfo;
                    if (Physics.Linecast(effect.EffectPosition, visibilityPoint, out hitInfo, layerObstacle, QueryTriggerInteraction.Ignore))
                    {
                        if (hitInfo.collider.gameObject != gameObject && hitInfo.distance >= 1f)
                        {
                            if (debugMode) Debug.DrawLine(effect.EffectPosition, hitInfo.point, Color.red, effect.EffectDuration);
                            if (debugMode) Debug.Log("Object is not in view because of a obstacle: " + hitInfo.collider.name, hitInfo.collider.gameObject);
                            return false;
                        }
                    }
                    else if (debugMode)
                    {
                        Debug.DrawLine(effect.EffectPosition, visibilityPoint, Color.green, effect.EffectDuration);
                    }
                    if (debugMode) Debug.Log("Object is in view!");
                    return true;
                }
                else
                {
                    if (debugMode) Debug.Log("Object is in view!");
                    return true;
                }
            }

            return false;
        }
    }
}