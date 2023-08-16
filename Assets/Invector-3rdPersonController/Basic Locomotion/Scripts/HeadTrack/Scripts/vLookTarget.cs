using UnityEngine;

using Invector.vCharacterController;

namespace Invector
{
    public class vLookTarget : MonoBehaviour
    {
        public bool ignoreHeadTrackAngle;
        [Header("Set this to assign a different point to look")]
        public Transform lookPointTarget;
        [Header("Area to check if is visible")]
        public Vector3 centerArea = Vector3.zero;
        public Vector3 sizeArea = Vector3.one;
        public bool useLimitToDetect = true;
        public float minDistanceToDetect = 2f;
        public VisibleCheckType visibleCheckType;
        [Tooltip("use this to turn the object undetectable")]
        public bool HideObject;
        [System.Serializable]
        public class OnLookEvent : UnityEngine.Events.UnityEvent<vHeadTrack> { }
        public OnLookEvent onEnterLook, onExitLook;
       

        public enum VisibleCheckType
        {
            None, SingleCast, BoxCast
        }

        void OnDrawGizmosSelected()
        {
            DrawBox();
        }

        void Start()
        {
            var layer = LayerMask.NameToLayer("HeadTrack");
            gameObject.layer = layer;
        }

        /// <summary>
        /// Point to look
        /// </summary>
        public Vector3 lookPoint
        {
            get
            {
                if (lookPointTarget)
                {
                    return lookPointTarget.position;
                }
                else
                    return transform.TransformPoint(centerArea);
            }
        }

        void DrawBox()
        {
            Gizmos.color = new Color(1, 0, 0, 1f);
            Gizmos.DrawSphere(lookPoint, 0.05f);
            if (visibleCheckType == VisibleCheckType.BoxCast)
            {
                var sizeX = transform.lossyScale.x * sizeArea.x;
                var sizeY = transform.lossyScale.y * sizeArea.y;
                var sizeZ = transform.lossyScale.z * sizeArea.z;
                var centerX = transform.lossyScale.x * centerArea.x;
                var centerY = transform.lossyScale.y * centerArea.y;
                var centerZ = transform.lossyScale.z * centerArea.z;
                Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position + new Vector3(centerX, centerY, centerZ), transform.rotation, new Vector3(sizeX, sizeY, sizeZ) * 2f);
                Gizmos.matrix = rotationMatrix;
                Gizmos.color = new Color(0, 1, 0, 0.2f);
                Gizmos.DrawCube(Vector3.zero, Vector3.one);
                Gizmos.color = new Color(0, 1, 0, 1f);
                Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

            }
            else if (visibleCheckType == VisibleCheckType.SingleCast)
            {
                var point = transform.TransformPoint(centerArea);
                Gizmos.color = new Color(0, 1, 0, 1f);
                Gizmos.DrawSphere(point, 0.05f);
            }
        }

        internal void EnterLook(vHeadTrack vHeadTrack)
        {
            onEnterLook.Invoke(vHeadTrack);
        }

        internal void ExitLook(vHeadTrack vHeadTrack)
        {
            onExitLook.Invoke(vHeadTrack);
        }
    }

    #region Helper
    public static class vLookTargetHelper
    {
        struct LookPoints
        {
            public Vector3 frontTopLeft;
            public Vector3 frontTopRight;
            public Vector3 frontBottomLeft;
            public Vector3 frontBottomRight;
            public Vector3 backTopLeft;
            public Vector3 backTopRight;
            public Vector3 backBottomLeft;
            public Vector3 backBottomRight;
        }

        static LookPoints GetLookPoints(vLookTarget lookTarget)
        {
            LookPoints points = new LookPoints();
            var centerArea = lookTarget.centerArea;
            var sizeArea = lookTarget.sizeArea;
            var lookTransform = lookTarget.transform;
            points.frontTopLeft = new Vector3(centerArea.x - sizeArea.x, centerArea.y + sizeArea.y, centerArea.z - sizeArea.z);
            points.frontTopRight = new Vector3(centerArea.x + sizeArea.x, centerArea.y + sizeArea.y, centerArea.z - sizeArea.z);
            points.frontBottomLeft = new Vector3(centerArea.x - sizeArea.x, centerArea.y - sizeArea.y, centerArea.z - sizeArea.z);
            points.frontBottomRight = new Vector3(centerArea.x + sizeArea.x, centerArea.y - sizeArea.y, centerArea.z - sizeArea.z);
            points.backTopLeft = new Vector3(centerArea.x - sizeArea.x, centerArea.y + sizeArea.y, centerArea.z + sizeArea.z);
            points.backTopRight = new Vector3(centerArea.x + sizeArea.x, centerArea.y + sizeArea.y, centerArea.z + sizeArea.z);
            points.backBottomLeft = new Vector3(centerArea.x - sizeArea.x, centerArea.y - sizeArea.y, centerArea.z + sizeArea.z);
            points.backBottomRight = new Vector3(centerArea.x + sizeArea.x, centerArea.y - sizeArea.y, centerArea.z + sizeArea.z);

            points.frontTopLeft = lookTransform.TransformPoint(points.frontTopLeft);
            points.frontTopRight = lookTransform.TransformPoint(points.frontTopRight);
            points.frontBottomLeft = lookTransform.TransformPoint(points.frontBottomLeft);
            points.frontBottomRight = lookTransform.TransformPoint(points.frontBottomRight);
            points.backTopLeft = lookTransform.TransformPoint(points.backTopLeft);
            points.backTopRight = lookTransform.TransformPoint(points.backTopRight);
            points.backBottomLeft = lookTransform.TransformPoint(points.backBottomLeft);
            points.backBottomRight = lookTransform.TransformPoint(points.backBottomRight);
            return points;
        }

        /// <summary>
        /// Check if anny corner points of LookTarget area is visible from observer
        /// </summary>
        /// <param name="lookTarget">principal transform of lookTarget</param>
        /// <param name="from">observer point</param>
        /// <param name="layerMask">Layer to check</param>
        /// <param name="debug">Draw lines </param>
        /// <returns></returns>
        public static bool IsVisible(this vLookTarget lookTarget, Vector3 from, LayerMask layerMask, bool debug = false)
        {

            if (lookTarget.HideObject) return false;

            if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.None)
            {
                if (lookTarget.useLimitToDetect && Vector3.Distance(from, lookTarget.transform.position) > lookTarget.minDistanceToDetect) return false;

                return true;
            }
            else if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.SingleCast)
            {
                if (lookTarget.useLimitToDetect && Vector3.Distance(from, lookTarget.centerArea) > lookTarget.minDistanceToDetect) return false;
                if (CastPoint(from, lookTarget.transform.TransformPoint(lookTarget.centerArea), lookTarget.transform, layerMask, debug)) return true; else return false;
            }
            else if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.BoxCast)
            {
                if (lookTarget.useLimitToDetect && Vector3.Distance(from, lookTarget.transform.position) > lookTarget.minDistanceToDetect) return false;
                LookPoints points = GetLookPoints(lookTarget);

                if (CastPoint(from, points.frontTopLeft, lookTarget.transform, layerMask, debug)) return true;

                if (CastPoint(from, points.frontTopRight, lookTarget.transform, layerMask, debug)) return true;

                if (CastPoint(from, points.frontBottomLeft, lookTarget.transform, layerMask, debug)) return true;

                if (CastPoint(from, points.frontBottomRight, lookTarget.transform, layerMask, debug)) return true;

                if (CastPoint(from, points.backTopLeft, lookTarget.transform, layerMask, debug)) return true;

                if (CastPoint(from, points.backTopRight, lookTarget.transform, layerMask, debug)) return true;

                if (CastPoint(from, points.backBottomLeft, lookTarget.transform, layerMask, debug)) return true;

                if (CastPoint(from, points.backBottomRight, lookTarget.transform, layerMask, debug)) return true;
            }
            return false;
        }

        /// <summary>
        /// Check if anny corner points of LookTarget area is visible from observer
        /// </summary>
        /// <param name="lookTarget">principal transform of lookTarget</param>
        /// <param name="from">observer point</param>
        /// <param name="debug">Draw lines </param>
        /// <returns></returns>
        public static bool IsVisible(this vLookTarget lookTarget, Vector3 from, bool debug = false)
        {
            if (lookTarget.HideObject) return false;
            LookPoints points = GetLookPoints(lookTarget);
            if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.None)
            {
                return true;
            }
            else if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.SingleCast)
            {
                if (CastPoint(from, lookTarget.transform.TransformPoint(lookTarget.centerArea), lookTarget.transform, debug)) return true; else return false;
            }
            else if (lookTarget.visibleCheckType == vLookTarget.VisibleCheckType.BoxCast)
            {
                if (CastPoint(from, points.frontTopLeft, lookTarget.transform, debug)) return true;

                if (CastPoint(from, points.frontTopRight, lookTarget.transform, debug)) return true;

                if (CastPoint(from, points.frontBottomLeft, lookTarget.transform, debug)) return true;

                if (CastPoint(from, points.frontBottomRight, lookTarget.transform, debug)) return true;

                if (CastPoint(from, points.backTopLeft, lookTarget.transform, debug)) return true;

                if (CastPoint(from, points.backTopRight, lookTarget.transform, debug)) return true;

                if (CastPoint(from, points.backBottomLeft, lookTarget.transform, debug)) return true;

                if (CastPoint(from, points.backBottomRight, lookTarget.transform, debug)) return true;
            }

            return false;
        }

        static bool CastPoint(Vector3 from, Vector3 point, Transform lookTarget, LayerMask layerMask, bool debug = false)
        {
            RaycastHit hit;

            if (Physics.Linecast(from, point, out hit, layerMask))
            {
                if (hit.transform != lookTarget.transform)
                {
                    if (debug) Debug.DrawLine(from, hit.point, Color.red);
                    return false;
                }

                else
                {
                    if (debug) Debug.DrawLine(from, hit.point, Color.green);
                    return true;
                }
            }
            if (debug) Debug.DrawLine(from, point, Color.green);
            return true;
        }

        static bool CastPoint(Vector3 from, Vector3 point, Transform lookTarget, bool debug = false)
        {
            RaycastHit hit;

            if (Physics.Linecast(from, point, out hit))
            {
                if (hit.transform != lookTarget.transform)
                {
                    if (debug) Debug.DrawLine(from, hit.point, Color.red);
                    return false;
                }

                else
                {
                    if (debug) Debug.DrawLine(from, hit.point, Color.green);
                    return true;
                }
            }
            if (debug) Debug.DrawLine(from, point, Color.green);
            return true;
        }
    }
    #endregion
}