using UnityEngine;

namespace Invector.vCamera
{
    public static class vThirdPersonCameraExtensions
    {
        /// <summary>
        /// Lerp between CameraStates
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="time"></param>
        public static void Slerp(this vThirdPersonCameraState to, vThirdPersonCameraState from, float time)
        {
            to.Name = from.Name;
            to.forward = Mathf.Lerp(to.forward, from.forward, time);
            to.right = Mathf.Lerp(to.right, from.right, time);
            to.defaultDistance = Mathf.Lerp(to.defaultDistance, from.defaultDistance, time);
            to.maxDistance = Mathf.Lerp(to.maxDistance, from.maxDistance, time);
            to.minDistance = Mathf.Lerp(to.minDistance, from.minDistance, time);
            to.height = Mathf.Lerp(to.height, from.height, time);
            to.fixedAngle = Vector2.Lerp(to.fixedAngle, from.fixedAngle, time);
            to.smooth = Mathf.Lerp(to.smooth, from.smooth, time);
            to.xMouseSensitivity = Mathf.Lerp(to.xMouseSensitivity, from.xMouseSensitivity, time);
            to.yMouseSensitivity = Mathf.Lerp(to.yMouseSensitivity, from.yMouseSensitivity, time);
            to.yMinLimit = Mathf.Lerp(to.yMinLimit, from.yMinLimit, time);
            to.yMaxLimit = Mathf.Lerp(to.yMaxLimit, from.yMaxLimit, time);
            to.xMinLimit = Mathf.Lerp(to.xMinLimit, from.xMinLimit, time);
            to.xMaxLimit = Mathf.Lerp(to.xMaxLimit, from.xMaxLimit, time);
            to.rotationOffSet = Vector3.Lerp(to.rotationOffSet, from.rotationOffSet, time);
            to.cullingHeight = Mathf.Lerp(to.cullingHeight, from.cullingHeight, time);
            to.cullingMinDist = Mathf.Lerp(to.cullingMinDist, from.cullingMinDist, time);
            to.cameraMode = from.cameraMode;
            to.useZoom = from.useZoom;
            to.lookPoints = from.lookPoints;
            to.fov = Mathf.Lerp(to.fov, from.fov, time);

            if (to.fov <= 0) to.fov = 1f;
        }

        /// <summary>
        /// Copy of CameraStates
        /// </summary>
        /// <param name="to"></param>
        /// <param name="from"></param>
        public static void CopyState(this vThirdPersonCameraState to, vThirdPersonCameraState from)
        {
            to.Name = from.Name;
            to.forward = from.forward;
            to.right = from.right;
            to.defaultDistance = from.defaultDistance;
            to.maxDistance = from.maxDistance;
            to.minDistance = from.minDistance;
            to.height = from.height;
            to.fixedAngle = from.fixedAngle;
            to.lookPoints = from.lookPoints;
            to.smooth = from.smooth;
            to.xMouseSensitivity = from.xMouseSensitivity;
            to.yMouseSensitivity = from.yMouseSensitivity;
            to.yMinLimit = from.yMinLimit;
            to.yMaxLimit = from.yMaxLimit;
            to.xMinLimit = from.xMinLimit;
            to.xMaxLimit = from.xMaxLimit;
            to.rotationOffSet = from.rotationOffSet;
            to.cullingHeight = from.cullingHeight;
            to.cullingMinDist = from.cullingMinDist;
            to.cameraMode = from.cameraMode;
            to.useZoom = from.useZoom;
            to.fov = from.fov;

            if (to.fov <= 0) to.fov = 1f;
        }

        public static ClipPlanePoints NearClipPlanePoints(this Camera camera, Vector3 pos, float clipPlaneMargin)
        {
            var clipPlanePoints = new ClipPlanePoints();

            var transform = camera.transform;
            var halfFOV = (camera.fieldOfView / 2) * Mathf.Deg2Rad;
            var aspect = camera.aspect;
            var distance = camera.nearClipPlane;
            var height = distance * Mathf.Tan(halfFOV);
            var width = height * aspect;
            height *= 1 + clipPlaneMargin;
            width *= 1 + clipPlaneMargin;
            clipPlanePoints.LowerRight = pos + transform.right * width;
            clipPlanePoints.LowerRight -= transform.up * height;
            clipPlanePoints.LowerRight += transform.forward * distance;

            clipPlanePoints.LowerLeft = pos - transform.right * width;
            clipPlanePoints.LowerLeft -= transform.up * height;
            clipPlanePoints.LowerLeft += transform.forward * distance;

            clipPlanePoints.UpperRight = pos + transform.right * width;
            clipPlanePoints.UpperRight += transform.up * height;
            clipPlanePoints.UpperRight += transform.forward * distance;

            clipPlanePoints.UpperLeft = pos - transform.right * width;
            clipPlanePoints.UpperLeft += transform.up * height;
            clipPlanePoints.UpperLeft += transform.forward * distance;

            return clipPlanePoints;
        }
    }

    public struct ClipPlanePoints
    {
        public Vector3 UpperLeft;
        public Vector3 UpperRight;
        public Vector3 LowerLeft;
        public Vector3 LowerRight;
    }
}