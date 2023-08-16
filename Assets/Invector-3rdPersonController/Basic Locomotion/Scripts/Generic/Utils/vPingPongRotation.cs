using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vPingPongRotation : MonoBehaviour
{
    [vHelpBox("This Component needs to be child of a root transform")]
    [vMinMax(-180, 180)]
    public Vector2 angleX;
    [vMinMax(-180, 180)]
    public Vector2 angleY;
    [vMinMax(-180, 180)]
    public Vector2 angleZ;

    public Vector3 speed = Vector3.one;

    Vector3 pingPongTime;
    Vector3 euler;
    float evaluateToDirection;
    Vector3 defaultLocalForward;
    public Transform targetTransform;
    Vector3 evaluate;

    void Start()
    {
        if (targetTransform == null)
        {
            targetTransform = transform;
        }

        defaultLocalForward = targetTransform.parent.InverseTransformDirection(targetTransform.forward);
    }

    private void OnEnable()
    {    
        evaluateToDirection = 0;    
    }

    public void Reset()
    {
        evaluateToDirection = 0;
    }

    void Update()
    {
        Vector3 forward = targetTransform.parent.TransformDirection(defaultLocalForward);
        if (angleX.magnitude > 0)
        {
            pingPongTime.x = Time.time * speed.x;
        }

        if (angleY.magnitude > 0)
        {
            pingPongTime.y = Time.time * speed.y;
        }

        if (angleZ.magnitude > 0)
        {
            pingPongTime.z = Time.time * speed.z;
        }

        if (evaluateToDirection < 1)
        {
            evaluateToDirection += Time.deltaTime * speed.magnitude;
        }
        else
        {
            evaluateToDirection = 1;
        }

        if (angleX.magnitude > 0)
        {
            evaluate.x = Mathf.PingPong(pingPongTime.x, 1f);
        }

        if (angleY.magnitude > 0)
        {
            evaluate.y = Mathf.PingPong(pingPongTime.y, 1f);
        }

        if (angleZ.magnitude > 0)
        {
            pingPongTime.z = Time.time * speed.z;
        }

        evaluate.z = Mathf.PingPong(pingPongTime.z, 1f);

        if (angleX.magnitude > 0)
        {
            euler.x = Mathf.Lerp(angleX.x, angleX.y, evaluate.x);
        }

        if (angleY.magnitude > 0)
        {
            euler.y = Mathf.Lerp(angleY.x, angleY.y, evaluate.y);
        }

        if (angleZ.magnitude > 0)
        {
            pingPongTime.z = Time.time * speed.z;
        }

        euler.z = Mathf.Lerp(angleZ.x, angleZ.y, evaluate.z);

        targetTransform.forward = Vector3.Lerp(forward, Quaternion.Euler(euler) * forward, evaluateToDirection);
    }
}
