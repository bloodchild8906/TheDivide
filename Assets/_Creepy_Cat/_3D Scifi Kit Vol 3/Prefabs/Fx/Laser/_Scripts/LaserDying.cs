// *************************************************************************************************
// Creepy Cat note : A simple script to auto kill a laser shoot (used by the class : LaserImpact.cs)
// *************************************************************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDying : MonoBehaviour {

    public GameObject explodeEmitter;
    public GameObject objectToKill;
    public float dieAfterSeconds = 3;

    // Do not activate into the inspector! shared var
    public bool killMe = false;

    // If kill me activated by LaserImpact
    void Update(){
        if (killMe == true){
            KillGameObject();
        }      
    }

	// Explode instanciate and source object kill
	void KillGameObject () {
        explodeEmitter = Instantiate(explodeEmitter, transform.position, transform.rotation) as GameObject;
        Destroy(objectToKill);
	}
}
