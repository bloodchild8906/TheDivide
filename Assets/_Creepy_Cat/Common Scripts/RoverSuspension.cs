using UnityEngine;
using System.Collections;

[ExecuteInEditMode()]
public class RoverSuspension : MonoBehaviour {
	[Range(0, 20)]
	public float naturalFrequency = 10;

	[Range(0, 3)]
	public float dampingRatio = 0.8f;

	[Range(-1, 1)]
	public float forceShift = 0.03f;

	public bool setSuspensionDistance = true;

	void Update () {
		// work out the stiffness and damper parameters based on the better spring model
		foreach (WheelCollider wc in GetComponentsInChildren<WheelCollider>()) {
			JointSpring spring = wc.suspensionSpring;

			spring.spring = Mathf.Pow(Mathf.Sqrt(wc.sprungMass) * naturalFrequency, 2);
			spring.damper = 2 * dampingRatio * Mathf.Sqrt(spring.spring * wc.sprungMass);

			wc.suspensionSpring = spring;

			Vector3 wheelRelativeBody = transform.InverseTransformPoint(wc.transform.position);
			float distance = GetComponent<Rigidbody>().centerOfMass.y - wheelRelativeBody.y + wc.radius;

			wc.forceAppPointDistance = distance - forceShift;

			// the following line makes sure the spring force at maximum droop is exactly zero
			if (spring.targetPosition > 0 && setSuspensionDistance)
				wc.suspensionDistance = wc.sprungMass * Physics.gravity.magnitude / (spring.targetPosition * spring.spring);
		}
	}

// uncomment OnGUI to observe how parameters change

/*
	public void OnGUI()
	{
		foreach (WheelCollider wc in GetComponentsInChildren<WheelCollider>()) {
			GUILayout.Label (string.Format("{0} sprung: {1}, k: {2}, d: {3}", wc.name, wc.sprungMass, wc.suspensionSpring.spring, wc.suspensionSpring.damper));
		}

		var rb = GetComponent<Rigidbody> ();

		GUILayout.Label ("Inertia: " + rb.inertiaTensor);
		GUILayout.Label ("Mass: " + rb.mass);
		GUILayout.Label ("Center: " + rb.centerOfMass);
	}
*/

}
