using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Invector
{
    [vClassHeader("Rotate To Target")]
    public class vRotateToTarget : vMonoBehaviour
    {
        public Transform targetTransform;
        [FormerlySerializedAs("angleRoot")]
        public Transform angleRootH;
        public Transform rotatorH;
        public Transform angleRootV;
        [FormerlySerializedAs("rotator")]
        public Transform rotatorV;
       
        public bool rotateH;
        public bool rotateV;
        public float rotationSpeedInAngle;
        public float rotationSpeedOutAngle;
        [Range(0.0f, 180.0f)]
        public float maxAngleVertical = 60.0f;
        [Range(0.0f, 180.0f)]
        public float maxAngleHorizontal = 60.0f;
        [Range(0.0f, 180.0f)]
        public float angleToReachTarget = 45f;

        public UnityEngine.Events.UnityEvent onEnterAngle;
        public UnityEngine.Events.UnityEvent onStayAngle;
        public UnityEngine.Events.UnityEvent onExitAngle;
        public bool targetIsInAngleRange { protected set; get; }
        protected float angleH;
        protected float angleV;
        protected virtual void Start()
        {
            if (angleRootH == null) angleRootH = transform;
            if (angleRootV == null) angleRootV = transform;          
          
        }



        protected virtual void Update()
        {
            if (!angleRootH || !rotatorV ) return;
            Transform target = targetTransform;
            if (rotatorV)
                angleV = rotatorV.localEulerAngles.x;
            if(rotatorH)
                angleH = rotatorH.localEulerAngles.y;
            if (target)
            {
                Vector3 targetDirV = target.position - angleRootV.position;
                Vector3 targetDirH = target.position - angleRootH.position;
                float _targetAngleV = angleRootV.forward.AngleFormOtherDirection(targetDirV.normalized).x;
                float _targetAngleH = angleRootH.forward.AngleFormOtherDirection(targetDirH.normalized).y;

                bool _isInAngle = Mathf.Abs(_targetAngleV) <= maxAngleVertical && Mathf.Abs(_targetAngleH) <= maxAngleHorizontal;

                if(_isInAngle!=targetIsInAngleRange)
                {
                    if (_isInAngle) onEnterAngle.Invoke();
                    else onExitAngle.Invoke();
                    targetIsInAngleRange = _isInAngle;
                }
                if (targetIsInAngleRange)
                {                   
                    onStayAngle.Invoke();
                   
                    angleV = Mathf.LerpAngle(angleV, _targetAngleV, rotationSpeedInAngle * Time.deltaTime);

                    angleH = Mathf.LerpAngle(angleH, _targetAngleH, rotationSpeedInAngle * Time.deltaTime);

                }
                else
                {
                    angleV = Mathf.LerpAngle(angleV, 0, rotationSpeedOutAngle * Time.deltaTime);

                    angleH = Mathf.LerpAngle(angleH, 0, rotationSpeedOutAngle * Time.deltaTime);
                }
            }
            else
            {
                if(targetIsInAngleRange)
                {
                    onExitAngle.Invoke();
                    targetIsInAngleRange = false;
                }
                if(rotatorV.localEulerAngles.magnitude>0)
                {
                    angleV = Mathf.LerpAngle(angleV, 0, rotationSpeedOutAngle * Time.deltaTime);                   
                    angleH = Mathf.LerpAngle(angleH, 0, rotationSpeedOutAngle * Time.deltaTime);
                    
                }
            }
            if (rotateV && rotateV)
            {
                Vector3 eulerV = rotatorV.localEulerAngles;
                eulerV.x = angleV;
                rotatorV.localEulerAngles = eulerV;
            }
          
            if (rotateH && rotatorH)
            {
                Vector3 eulerH = rotatorH.localEulerAngles;
                eulerH.y = angleH;
                rotatorH.localEulerAngles = eulerH;
            }

        }

        public void SetTarget(Transform target)
        {
            targetTransform = target;
        }

        public void ClearTarget()
        {
            targetTransform = null;
        }
    }
}