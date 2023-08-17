// **********************************************************************************
// Creepy Cat note : A simple class to reduce light intensity after hit
// **********************************************************************************
using UnityEngine;
using System.Collections;

public class LaserLight : MonoBehaviour {
	private bool laserHit = false;
    private Light Component;

	public float lightInt;

	void Start () 	{
		laserHit = true;

        // I change the light comp intensity
        Component = gameObject.GetComponent<Light>();
        Component.intensity = 4;
        
        lightInt = Component.intensity * Component.intensity * (( Component.intensity < 0.0f ) ? -1.0f : 1.0f);
	}
	
	void Update () 	{
		if (laserHit){
            // I decrease the light inensity
            Component.intensity -= (1.0f / Time.deltaTime) * lightInt * .001f;

            // If intensity 0 or <0 i delete the gameobject
            if (Component.intensity <= 0.0f){
				Destroy (gameObject,2f);
			}
		}
	}
}