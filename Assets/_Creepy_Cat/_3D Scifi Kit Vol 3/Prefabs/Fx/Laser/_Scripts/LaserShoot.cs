// **********************************************************************************
// Creepy Cat note : A simple class to make laser shoot when left mouse clicked
// **********************************************************************************
using UnityEngine;
using System.Collections;

public class LaserShoot : MonoBehaviour 
{
    RaycastHit hit;
    public GameObject[] laserShoot;
    public Transform spawnPosition;

    [HideInInspector]
    public int currentLaser = 0;
	public float speed = 1000;

	void Update () 	{

        if (Input.GetKeyDown(KeyCode.Mouse0)){
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity)){
                    GameObject projectile = Instantiate(laserShoot[currentLaser], spawnPosition.position, Quaternion.identity) as GameObject;
                    // projectile.name = "LaserShoot-Instance";
    
                    projectile.transform.LookAt(hit.point);
                    projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * speed);
                    projectile.GetComponent<LaserImpact>().collisionNormal = hit.normal;

                }  

        }

        Debug.DrawRay(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction*200, Color.green);
	}

}
