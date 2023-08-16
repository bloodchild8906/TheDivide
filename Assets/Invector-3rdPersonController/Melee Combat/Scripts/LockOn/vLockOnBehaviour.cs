using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Invector.vCharacterController
{
    [vClassHeader("Lock-On")]
    public abstract class vLockOnBehaviour : vMonoBehaviour
    {
        #region properties

        protected Transform watcher;
        [Tooltip("Tags of objects that can be found")]
        public string[] tagsToFind = new string[] { "Enemy" };
        [Tooltip("Check this option to only Lock into target within the camera field of view")]
        public bool withinCameraView = true;
        [vHideInInspector("withinCameraView"), Tooltip("Get only target inside screen margins")]
        public bool onlyInsideRect = true;
        [Tooltip("Layer of Obstacle to prevent the find for targets")]
        public LayerMask layerOfObstacles = 1 << 0;
        [Range(0, 1)]
        [Tooltip("Use this to set a margin of Aim point ")]
        public float screenMarginX = 0.8f;
        [Range(0, 1)]
        [Tooltip("Use this to set a margin of Aim point ")]
        public float screenMarginY = 0.1f;
        [Tooltip("Range of the search for targets")]
        public float rangeToEnterLockOn = 10f;
        [Tooltip("Range to auto exit lock")]
        public float rangeToExitLockOn = 15f;
        [Tooltip("Show the Gizmos and helpers")]
        public bool showDebug;
        public float timeToChangeTarget = 0.25f;

        protected int index = 0;
        protected List<Transform> visibles;
        protected Transform _target;
        protected virtual Transform target { get { return _target; } set { _target = value; } }
        protected vCamera.vThirdPersonCamera tpCamera;
        protected Rect rect;
        protected bool _inLockOn;
        protected bool changingTarget;

        #endregion

        #region public methods   

        protected virtual void Start()
        {
            if (rangeToExitLockOn < rangeToEnterLockOn) rangeToExitLockOn = rangeToEnterLockOn;
        }
        /// <summary>
        /// change the current target to next target of possibles target
        /// if exist more than 1 target in list
        /// </summary>
        public virtual void ChangeTarget(int value)
        {
            StartCoroutine(ChangeTargetRoutine(value));
        }

        /// <summary>
        /// Get current target
        /// </summary>
        /// <returns></returns>
        public virtual Transform currentTarget
        {
            get { return target; }
        }

        /// <summary>
        /// Check if Current target(vIHealthController) is Alive 
        /// </summary>
        /// <returns></returns>
        public virtual bool isCharacterAlive()
        {
            if (currentTarget == null)
            {
                return false;
            }

            var healthController = currentTarget.GetComponent<vIHealthController>();
            if (healthController == null)
            {
                return false;
            }
            //if (ichar.ragdolled) return false;
            if (healthController.currentHealth > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Check if target is a vIHealthController Alive
        /// </summary>
        /// <param name="other">target</param>
        /// <returns></returns>
        public virtual bool isCharacterAlive(Transform other)
        {
            var healthController = other.GetComponent<vIHealthController>();
            if (healthController == null)
            {
                return false;
            }
            //if (ichar.ragdolled) return false;
            if (healthController.currentHealth > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Reset Lock on (removing currentTarget)
        /// </summary>
        public virtual void ResetLockOn()
        {
            target = null;
            _inLockOn = false;
        }

        /// <summary>
        /// Get all target possibles
        /// </summary>
        public virtual List<Transform> allTargets
        {
            get { if (visibles != null && visibles.Count > 0) { return visibles; } return null; }
        }

        #endregion

        #region protected methods

        protected virtual void UpdateLockOn(bool value)
        {
            if (value == true && value != _inLockOn)
            {
                _inLockOn = value;
                visibles = GetPossibleTargets();
                index = 0;
                if (visibles != null && visibles.Count > 0)
                {
                    target = visibles[index];
                }
            }
            else if (value == false && value != _inLockOn)
            {
                _inLockOn = value;
                index = 0;
                target = null;
                if (visibles != null)
                {
                    visibles.Clear();
                }
            }
        }

        protected virtual IEnumerator ChangeTargetRoutine(int value)
        {
            if (!changingTarget)
            {
                changingTarget = true;
                visibles = GetPossibleTargets();
                if ((_inLockOn == true && visibles != null && visibles.Count > 1))
                {
                    if (index + value > visibles.Count - 1)
                    {
                        index = 0;
                    }
                    else if (index + value < 0)
                    {
                        index = visibles.Count - 1;
                    }
                    else
                    {
                        index += value;
                    }

                    target = visibles[index];
                    SetTarget();
                }
                yield return new WaitForSeconds(timeToChangeTarget);
                changingTarget = false;
            }
        }

        protected virtual void SetTarget()
        {

        }

        /// <summary>
        /// Draw GUI of Rect
        /// </summary>
        protected void OnGUI()
        {
            if (showDebug)
            {
                var width = Screen.width - (Screen.width * screenMarginX);
                var height = Screen.height - (Screen.height * screenMarginY);
                var posX = (Screen.width * 0.5f) - (width * 0.5f);
                var posY = (Screen.height * 0.5f) - (height * 0.5f);
                rect = new Rect(posX, posY, width, height);
                GUI.Box(rect, "");
            }
        }

        /// <summary>
        /// Init the properts
        /// </summary>
        protected void Init()
        {
            if (Camera.main == null)
            {
                this.enabled = false;
            }

            tpCamera = Camera.main.transform.root.GetComponent<vCamera.vThirdPersonCamera>();
            var width = Screen.width - (Screen.width * screenMarginX);
            var height = Screen.height - (Screen.height * screenMarginY);
            var posX = (Screen.width * 0.5f) - (width * 0.5f);
            var posY = (Screen.height * 0.5f) - (height * 0.5f);
            rect = new Rect(posX, posY, width, height);
        }

        /// <summary>
        /// Draw the range gizmo
        /// </summary>
        protected void OnDrawGizmos()
        {
            if (showDebug && watcher)
            {
                Gizmos.color = new Color(0, 1, 0, 0.2f);
                Gizmos.DrawSphere(watcher.position, rangeToEnterLockOn + 0.1f);
                if (visibles != null && visibles.Count > 0 && target != null)
                {
                    visibles.ForEach(delegate (Transform _transform)
                    {
                        Gizmos.color = _transform.Equals(currentTarget) ? Color.red : Color.yellow;
                        Gizmos.DrawSphere(_transform.GetComponent<Collider>().bounds.center, .5f);
                    });
                }
            }
        }

        /// <summary>
        /// get all possible targets
        /// </summary>
        /// <returns></returns>
        protected virtual List<Transform> GetPossibleTargets()
        {
            if (tpCamera != null && tpCamera.mainTarget != null)
            {
                watcher = tpCamera.mainTarget;
            }
            else
            {
                watcher = transform;
            }

            var listPrimary = new List<Transform>();
            var targets = Physics.SphereCastAll(watcher.position, rangeToEnterLockOn, watcher.forward, .01f);
            for (int i = 0; i < targets.Length; i++)
            {
                var hitOther = targets[i];
                if (tagsToFind.Contains(hitOther.transform.tag))
                {
                    if (isCharacterAlive(hitOther.transform.GetComponent<Transform>()))
                    {
                        RaycastHit hit;
                        var boundPoints = BoundPoints(hitOther.collider);
                        for (int a = 0; a < boundPoints.Length; a++)
                        {
                            var point = boundPoints[a];
                            if (Physics.Linecast(transform.position, point, out hit, layerOfObstacles))
                            {
                                if (hit.transform == hitOther.transform)
                                {
                                    listPrimary.Add(hitOther.transform);

                                    if (showDebug)
                                    {
                                        Debug.DrawLine(transform.position, point, Color.green, 2);
                                    }

                                    break;
                                }
                                else if (showDebug)
                                {
                                    Debug.DrawLine(transform.position, point, Color.red, 2);
                                }
                            }
                            else
                            {
                                listPrimary.Add(hitOther.transform);

                                if (showDebug)
                                {
                                    Debug.DrawLine(transform.position, point, Color.green, 2);
                                }

                                break;
                            }
                        }
                    }
                }
            }
            SortTargets(ref listPrimary);
            return listPrimary;
        }

        /// <summary>
        /// Sort the targets of possible targets by order of priority
        /// </summary>
        /// <param name="list"></param>
        protected void SortTargets(ref List<Transform> list)
        {
            if (tpCamera == null || tpCamera.targetCamera)
            {
                Init();
            }

            var lpriority_01 = new List<Transform>();
            var lpriority_02 = new List<Transform>();
            var lpriority_03 = new List<Transform>();
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(tpCamera.targetCamera);
            for (int i = 0; i < list.Count; i++)
            {
                var _transform = list[i];
                Vector2 screenPoint = tpCamera.targetCamera.WorldToScreenPoint(_transform.transform.position);
                //Check targets inside camera frustum 
                if (GeometryUtility.TestPlanesAABB(planes, _transform.GetComponent<Collider>().bounds))
                {
                    //target inside screenRect with margins
                    if (rect.Contains(screenPoint))
                    {
                        lpriority_01.Add(_transform);
                    }
                    //targets only inside camera frustum
                    else if (withinCameraView == false || (withinCameraView && !onlyInsideRect))
                    {
                        lpriority_02.Add(_transform);
                    }
                }
                //targets outside camera frustum
                else if (withinCameraView == false)
                {
                    lpriority_03.Add(_transform);
                }
            }

            //targets only inside camera frustum
            //Order by screen center distance
            lpriority_01.Sort(delegate (Transform t1, Transform t2)
            {
                Vector2 screenPoint_01 = tpCamera.targetCamera.WorldToScreenPoint(t1.transform.position);
                Vector2 screenPoint_02 = tpCamera.targetCamera.WorldToScreenPoint(t2.transform.position);
                return Vector2.Distance(screenPoint_01, rect.center)
                    .CompareTo(Vector2.Distance(screenPoint_02, rect.center));
            });
            //targets only inside camera frustum
            //Order by screen center distance
            lpriority_02.Sort(delegate (Transform t1, Transform t2)
            {
                Vector2 screenPoint_01 = tpCamera.targetCamera.WorldToScreenPoint(t1.transform.position);
                Vector2 screenPoint_02 = tpCamera.targetCamera.WorldToScreenPoint(t2.transform.position);
                return Vector2.Distance(screenPoint_01, rect.center)
                    .CompareTo(Vector2.Distance(screenPoint_02, rect.center));
            });
            //targets outside camera frustum
            //Order by character distance
            lpriority_03.Sort(delegate (Transform t1, Transform t2)
            {
                return Vector3.Distance(t1.transform.position, transform.position)
                    .CompareTo(Vector3.Distance(t2.transform.position, transform.position));
            });

            list = lpriority_01.Union(lpriority_02).Union(lpriority_03).ToList();
        }

        /// <summary>
        /// return 8 bound points
        /// </summary>
        /// <param name="collider"></param>
        /// <returns></returns>
        protected Vector3[] BoundPoints(Collider collider)
        {
            var boundPoint1 = collider.bounds.min;
            var boundPoint2 = collider.bounds.max;
            var boundPoint3 = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
            var boundPoint4 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
            var boundPoint5 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
            var boundPoint6 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
            var boundPoint7 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
            var boundPoint8 = new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);
            var lineColor = Color.white;
            if (showDebug)
            {
                Debug.DrawLine(boundPoint6, boundPoint2, lineColor, 1);
                Debug.DrawLine(boundPoint2, boundPoint8, lineColor, 1);
                Debug.DrawLine(boundPoint8, boundPoint4, lineColor, 1);
                Debug.DrawLine(boundPoint4, boundPoint6, lineColor, 1);
                // bottom of rectangular cuboid (3-7-5-1)
                Debug.DrawLine(boundPoint3, boundPoint7, lineColor, 1);
                Debug.DrawLine(boundPoint7, boundPoint5, lineColor, 1);
                Debug.DrawLine(boundPoint5, boundPoint1, lineColor, 1);
                Debug.DrawLine(boundPoint1, boundPoint3, lineColor, 1);
                // legs (6-3, 2-7, 8-5, 4-1)
                Debug.DrawLine(boundPoint6, boundPoint3, lineColor, 1);
                Debug.DrawLine(boundPoint2, boundPoint7, lineColor, 1);
                Debug.DrawLine(boundPoint8, boundPoint5, lineColor, 1);
                Debug.DrawLine(boundPoint4, boundPoint1, lineColor, 1);
            }
            return new Vector3[] { boundPoint1, boundPoint2, boundPoint3, boundPoint4, boundPoint5, boundPoint6, boundPoint7, boundPoint8 };
        }

        #endregion
    }

    /// <summary>
    /// Extencions for Help 
    /// </summary>
    public static class vLockOnHelper
    {
        /// <summary>
        /// Get point of Target relative to Screen
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="targetPoint"></param>
        /// <returns></returns>
        public static Vector2 GetScreenPointOffBoundsCenter(this Transform target, Canvas canvas, Camera cam, float _heightOffset)
        {
            var bounds = target.GetComponent<Collider>().bounds;
            var middle = bounds.center;
            var height = Vector3.Distance(bounds.min, bounds.max);

            var point = middle + new Vector3(0, height * _heightOffset, 0);
            var rectTransform = canvas.transform as RectTransform;
            Vector2 ViewportPosition = cam.WorldToViewportPoint(point);
            Vector2 WorldObject_ScreenPosition = new Vector2(
             ((ViewportPosition.x * rectTransform.sizeDelta.x) - (rectTransform.sizeDelta.x * 0.5f)),
             ((ViewportPosition.y * rectTransform.sizeDelta.y) - (rectTransform.sizeDelta.y * 0.5f)));
            return WorldObject_ScreenPosition;
        }

        public static Vector3 GetPointOffBoundsCenter(this Transform target, float _heightOffset)
        {
            var bounds = target.GetComponent<Collider>().bounds;
            var middle = bounds.center;
            var height = Vector3.Distance(bounds.min, bounds.max);

            var point = middle + new Vector3(0, height * _heightOffset, 0);
            return point;
        }
    }

}