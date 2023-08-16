using UnityEngine;
using System.Collections.Generic;

namespace Invector
{
    [System.Serializable]
    public class vThirdPersonCameraState
    {
        public string Name;
        public float forward;
        public float right;
        public float defaultDistance;
        public float maxDistance;
        public float minDistance;
        public float height;
        public float smooth = 10f;
        public float smoothDamp = 0f;
        public float xMouseSensitivity;
        public float yMouseSensitivity;
        public float yMinLimit;
        public float yMaxLimit;
        public float xMinLimit;
        public float xMaxLimit;

        public Vector3 rotationOffSet;
        public float cullingHeight;
        public float cullingMinDist;
        public float fov;
        public bool useZoom;
        public Vector2 fixedAngle;
        public List<LookPoint> lookPoints;
        public TPCameraMode cameraMode;

        public vThirdPersonCameraState(string name)
        {
            Name = name;
            forward = -1f;
            right = 0f;
            defaultDistance = 1.5f;
            maxDistance = 3f;
            minDistance = 0.5f;
            height = 0f;
            smooth = 10f;
            smoothDamp = 0f;
            xMouseSensitivity = 3f;
            yMouseSensitivity = 3f;
            yMinLimit = -40f;
            yMaxLimit = 80f;
            xMinLimit = -360f;
            xMaxLimit = 360f;
            cullingHeight = 0.2f;
            cullingMinDist = 0.1f;
            fov = 60f;
            useZoom = false;
            forward = 60;
            fixedAngle = Vector2.zero;
            cameraMode = TPCameraMode.FreeDirectional;
        }
    }

    [System.Serializable]
    public class LookPoint
    {
        public string pointName;
        public Vector3 positionPoint;
        public Vector3 eulerAngle;
        public bool freeRotation;
    }

    public enum TPCameraMode
    {
        FreeDirectional,
        FixedAngle,
        FixedPoint
    }
}