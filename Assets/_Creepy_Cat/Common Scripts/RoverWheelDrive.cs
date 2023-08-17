// Code by creepy cat, if you make some cool modifications, please send me them to :
// black.creepy.cat@gmail.com sometime i give voucher codes... :) Second Physic atempt

using UnityEngine;
using System.Collections;

public class RoverWheelDrive : MonoBehaviour {

	private WheelCollider[] wheels;

	public float steerMax = 30f;
	public float motorMax = 300f;
    public GameObject wheelShape;

    // Private vars
    private float steer = 0f;
    private float motor = 0f;

    void Awake(){
        Time.timeScale = 2; // Increase physic speed
    }


	// Find all the WheelColliders in the hierarchy
	public void Start()
	{
		wheels = GetComponentsInChildren<WheelCollider>();

		for (int i = 0; i < wheels.Length; ++i) 
		{
			var wheel = wheels [i];

			// create wheel shapes only when needed
			if (wheelShape != null)
			{
				var ws = GameObject.Instantiate (wheelShape);
				ws.transform.parent = wheel.transform;
			}
		}
	}

	// simple approach to updating wheels
	public void Update()
	{
		steer = steerMax * Input.GetAxis("Horizontal");
        motor  = motorMax * Input.GetAxis("Vertical");

		foreach (WheelCollider wheel in wheels)
		{
			// a simple car where front wheels steer while rear ones drive
			if (wheel.transform.localPosition.z > 0)
                wheel.steerAngle = steer;

			if (wheel.transform.localPosition.z < 0)
                wheel.motorTorque =  motor;


			// Positionning wheels

			if (wheelShape) 
			{
				Quaternion q;
				Vector3 p;
				wheel.GetWorldPose (out p, out q);

				// assume that the only child of the wheelcollider is the wheel shape
				Transform shapeTransform = wheel.transform.GetChild (0);
				shapeTransform.position = p;
				shapeTransform.rotation = q;
			}

            // Detect ground hit or not
            WheelHit hit;

            if (wheel.GetGroundHit(out hit) == false){
                //Debug.Log("Car quit floor");
            }

		}
	}
}
