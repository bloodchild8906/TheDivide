// Code by creepy cat, if you make some cool modifications, please send me them to :
// black.creepy.cat@gmail.com sometime i give voucher codes... :)

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider leftWheel;
    public WheelCollider rightWheel;
    public bool motor;
    public bool steering;
}

    public class RoverControler_A : MonoBehaviour
{
    public List<AxleInfo> axleInfos;
    public float maxMotorTorque;
    public float maxSteeringAngle;

    void Awake(){
        Time.timeScale = 2;
    }

    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }



    public void FixedUpdate()
    {
        float motor = maxMotorTorque * Input.GetAxis("Vertical");
        float steering = maxSteeringAngle * Input.GetAxis("Horizontal");
       // Debug.Log(motor);

        if (Input.GetKey(KeyCode.Space))
        {
            Brake();
           
        }


        foreach (AxleInfo axleInfo in axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }


            if (axleInfo.motor)
            {
                axleInfo.leftWheel.brakeTorque = 0;
                axleInfo.rightWheel.brakeTorque = 0;

                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }
            else
            {

              }

         //   axleInfo.leftWheel.brakeTorque = 100;
         //   axleInfo.rightWheel.brakeTorque = 100;


            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }

    private void Brake()
    {

        foreach (AxleInfo axleInfo in axleInfos)
        {
          //  axleInfo.leftWheel.brakeTorque = maxMotorTorque*1500;
         //   axleInfo.rightWheel.brakeTorque = maxMotorTorque*1500;
        }
    }

}