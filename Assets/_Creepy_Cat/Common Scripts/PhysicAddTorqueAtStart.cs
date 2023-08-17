// A simple code to add a force via mouse button
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicAddTorqueAtStart : MonoBehaviour {

        //  public float hoverForce;
    public float torque;
   // public enum WhatButton { Left, Right, None };
   // public WhatButton theButton;
 

    private Rigidbody rb;


    void Start()
    {
       rb = GetComponent<Rigidbody>();
       Time.timeScale = 1.5f;

        // rb.AddForce(-transform.forward * hoverForce, ForceMode.Acceleration);
        rb.AddTorque(-transform.forward * torque);
        rb.AddTorque(-transform.right * torque);
        rb.AddTorque(-transform.up * torque);
    }

   



}
