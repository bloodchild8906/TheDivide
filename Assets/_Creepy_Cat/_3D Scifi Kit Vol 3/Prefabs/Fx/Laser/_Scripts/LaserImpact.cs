// **********************************************************************************
// Creepy Cat note : A simple class to make laser shoot and explode
// **********************************************************************************
using UnityEngine;
using System.Collections;

public class LaserImpact : MonoBehaviour {
    
    public GameObject laserImpact;
    public GameObject laserEmitter;
    public GameObject[] laserTrails;

    [HideInInspector]
    public Vector3 collisionNormal; 
    public bool oneTime = false;

	void Start () {
        laserEmitter = Instantiate(laserEmitter, transform.position, transform.rotation) as GameObject;
        laserEmitter.transform.parent = transform;
	}

	void OnCollisionEnter (Collision hit) {

        // If this bool is not used, this damn unity make multiple instances (clone)(clone) etc... Use this to avoid the problem (i waste one day on this shit..).
        if (oneTime == false)
        {
            // YOU NEED TO DEFINE A DESTRUCTIBLE TAG AND ASSIGN IT TO ALL GAMEOBJECTS YOU WANT DESTROY + A RIGID BODY
            if (hit.gameObject.tag == "Destructible"){
                
                // Poke into the the hitted object and i change the component parameters
                LaserDying yesDie = hit.gameObject.GetComponent<LaserDying>();
                yesDie.killMe = true;
                yesDie.objectToKill = hit.gameObject;

            }

            // Instanciate the impact of the shoot and place it on the transform of the ray
            laserImpact = Instantiate(laserImpact, transform.position, Quaternion.FromToRotation(Vector3.up, collisionNormal)) as GameObject;

            // Removing child laser trail
            foreach (GameObject trail in laserTrails){
                GameObject curTrail = transform.Find(laserEmitter.name + "/" + trail.name).gameObject;

                // If different than null, else debug log error
                if (curTrail == null){
                    curTrail.transform.parent = null;
                    Destroy(curTrail);
                }
            }

            // Cleaning memory
            Destroy(laserEmitter, 7f);
            Destroy(laserImpact, 7f);
            Destroy(gameObject);
            oneTime = true;
        }

	}

}