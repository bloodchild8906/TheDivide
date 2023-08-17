using UnityEngine;
using System.Collections;

public class ObjectFacingCamera : MonoBehaviour
{
	private Camera m_Camera;

	void Start()
	{
		//This gets the Main Camera from the Scene
		m_Camera = Camera.main;

	}

	//Orient the camera after all movement is completed this frame to avoid jittering
	void LateUpdate()
	{
		transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.forward,
			m_Camera.transform.rotation * Vector3.up);
	}
}