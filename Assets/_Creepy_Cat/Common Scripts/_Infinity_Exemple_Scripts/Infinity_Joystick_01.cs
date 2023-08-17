//
// A very crappy flight model, very crappy... need really be improved.
// If some courageous persons want to work this... If you get some good
// results, be cool and send the code to creepy cat :) 
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Infinity_Joystick_01 : MonoBehaviour
{

	public Rigidbody InfinityModel;

	private Vector2 hMove = Vector2.zero;
	private Vector2 hTilt = Vector2.zero;
	private float hTurn = 0f;

	//private float _engineForce=6;

	public bool IsOnGround = true;
	public float EngineForce = 0;
	public float MoveUpForce = 2.5f;
	public float MoveDownForce = 2.8f;

	public float TurnForce = 6f;
	public float ForwardForce = 30f;
	public float ForwardTiltForce = 20f;
	public float TurnTiltForce = 30f;
	public float EffectiveHeight = 100f;

	public float turnTiltForcePercent = 1.5f;
	public float turnForcePercent = 1.3f;


	// Update is called once per frame
	void Update()
	{

		float tempY = 0;
		float tempX = 0;


		// stable forward
		if (hMove.y > 0)
			tempY = - Time.fixedDeltaTime;

		else
			if (hMove.y < 0)
				tempY = Time.fixedDeltaTime;

		// stable lurn
		if (hMove.x > 0)
			tempX = -Time.fixedDeltaTime;
		else
			if (hMove.x < 0)
				tempX = Time.fixedDeltaTime;


		// speed control
		float speedY =  Input.GetAxis ("Mouse ScrollWheel");

		EngineForce = EngineForce+(speedY*2);
		if (EngineForce < 0) EngineForce = 0;


		if (Input.GetKey(KeyCode.LeftShift))
		{
			EngineForce += MoveUpForce;
		}

		if (Input.GetKey(KeyCode.LeftControl))
		{
			EngineForce -= MoveDownForce;
			if (EngineForce < 0) EngineForce = 0;
		}

		// Forward / barckard


		float joyAxisX =  Input.GetAxis ("Horizontal")*5;
		float joyAxisY =  Input.GetAxis ("Vertical")*5;




		if( joyAxisY >0)
		{
			tempY = (joyAxisY)*Time.fixedDeltaTime;

		}

		if( joyAxisY <0)
		{
			tempY = (joyAxisY)*Time.fixedDeltaTime;
		}

		if( joyAxisX >0)
		{
			tempX = (joyAxisX)*Time.fixedDeltaTime;
		}

		if( joyAxisX <0)
		{
			tempX = (joyAxisX)*Time.fixedDeltaTime;
		}


		if (Input.GetKey(KeyCode.Z))
		{
			//			if (IsOnGround) break;
			tempY = Time.fixedDeltaTime;
		}

		if (Input.GetKey(KeyCode.S))
		{

			//			if (IsOnGround) break;
			tempY = -Time.fixedDeltaTime;
		}




		// Strafe Left / Right

		if (Input.GetKey(KeyCode.Q))
		{

			tempX = -Time.fixedDeltaTime;

			//			if (IsOnGround) break;
			var force = -(turnForcePercent - Mathf.Abs(hMove.y))*InfinityModel.mass;
			InfinityModel.AddRelativeTorque(0f, force, 0);

		
		}

		if (Input.GetKey(KeyCode.D))
		{

			tempX = Time.fixedDeltaTime;

			//			if (IsOnGround) break;
			var force = (turnForcePercent - Mathf.Abs(hMove.y))*InfinityModel.mass;
			InfinityModel.AddRelativeTorque(0f, force, 0);

		
		}








		hMove.x += tempX;
		hMove.x = Mathf.Clamp(hMove.x, -1, 1);

		hMove.y += tempY;
		hMove.y = Mathf.Clamp(hMove.y, -1, 1);
	

	}

	void FixedUpdate()
	{
		LiftProcess();
		MoveProcess();
		TiltProcess();
	}

	private void MoveProcess()
	{
		var turn = TurnForce * Mathf.Lerp(hMove.x, hMove.x * (turnTiltForcePercent - Mathf.Abs(hMove.y)), Mathf.Max(0f, hMove.y));

		hTurn = Mathf.Lerp(hTurn, turn, Time.fixedDeltaTime * TurnForce);
		InfinityModel.AddRelativeTorque(0f, hTurn * InfinityModel.mass, 0f);


		InfinityModel.AddRelativeForce(Vector3.forward * (hMove.y *InfinityModel.mass*ForwardForce));
	}

	private void LiftProcess()
	{
		var upForce = 1 - Mathf.Clamp(InfinityModel.transform.position.y / EffectiveHeight, 0, 1);
		upForce = Mathf.Lerp(0f, EngineForce, upForce) * InfinityModel.mass;
		InfinityModel.AddRelativeForce(Vector3.up * upForce);
	}

	private void TiltProcess()
	{
		hTilt.x = Mathf.Lerp(hTilt.x, hMove.x * TurnTiltForce, Time.deltaTime);
		hTilt.y = Mathf.Lerp(hTilt.y, hMove.y * ForwardTiltForce, Time.deltaTime);
		InfinityModel.transform.localRotation = Quaternion.Euler(hTilt.y, InfinityModel.transform.localEulerAngles.y, -hTilt.x);
	}


	private void OnCollisionEnter()
	{
		IsOnGround = true;
	}

	private void OnCollisionExit()
	{
		IsOnGround = false;
	}

}
