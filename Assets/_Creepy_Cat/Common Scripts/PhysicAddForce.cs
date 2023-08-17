// A simple code to add a force via mouse button
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicAddForce : MonoBehaviour {

    public float hoverForce;
    public float torque;
    public enum WhatButton { Left, Right, None };
    public WhatButton theButton;
    // public float stability = 0.3f;

    private Rigidbody rb;


    void Start()
    {
       rb = GetComponent<Rigidbody>();
       Time.timeScale = 2;
    }

             
    void FixedUpdate()
    {
        //Vector3 predictedUp = Quaternion.AngleAxis(rb.angularVelocity.magnitude * Mathf.Rad2Deg * stability / hoverForce,rb.angularVelocity) * transform.up;
        //Vector3 torqueVector = Vector3.Cross(predictedUp, Vector3.forward);
        //rb.AddTorque(torqueVector * hoverForce * hoverForce);


        switch(theButton)
        {
        case WhatButton.Left:
            if (Input.GetMouseButton(0)){
                rb.AddForce(-transform.forward * hoverForce, ForceMode.Acceleration);
                rb.AddTorque(-transform.forward * torque);
            }
            break;
            case WhatButton.Right:
            if (Input.GetMouseButton(1)){
                rb.AddForce(-transform.forward * hoverForce, ForceMode.Acceleration);
                rb.AddTorque(-transform.forward * torque);
            }
            break;
            case WhatButton.None:
                rb.AddForce(-transform.forward * hoverForce, ForceMode.Acceleration);
                rb.AddTorque(-transform.forward * torque);
            break;
        }

    }


}
