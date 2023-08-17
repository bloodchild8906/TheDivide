// **********************************************************************************
// If you make some cool modifications, please send me them to :
// black.creepy.cat@gmail.com sometime i give voucher codes... :)
// Simple code to make the turrets look at gameobject point, just given for example...
//
// Special thanks to this guy : https://github.com/beinteractive/Uween for this pure 
// piece of code...
//
// Thanks to : Dirk Jacobasch for the timer to kill bullets :)
// Blog : http://blog.ggames4u.com/
//
//
// **********************************************************************************
using UnityEngine;
using System.Collections;
using Uween;

public class LauncherLookAt : MonoBehaviour
{

	public Transform targetGameobject;
	public Transform launcherGameobject;


	public float turretDegreesPerSecond = 45.0f;
	public float launcherDegreesPerSecond = 45.0f;

	public float maxlauncherAngle = 45.0f;


	private Quaternion qTurret;
	private Quaternion qlauncherGameobject;
	private Quaternion launcherGameobjectStartRot;
	private Transform trans;

	private float canonStartPos;
	private AudioSource shootAudio;

	void Start()
	{
		trans = transform;
		launcherGameobjectStartRot = launcherGameobject.transform.localRotation;

		canonStartPos = launcherGameobject.transform.localPosition.z;


		shootAudio = GetComponent<AudioSource>();
	}

	void EndAnim()
	{

		TweenZ.Add(launcherGameobject.gameObject, 1.5f, canonStartPos).EaseOutElastic();
	}


	void Update()
	{


			if (targetGameobject){
				float distanceToPlane = Vector3.Dot(trans.up, targetGameobject.position - trans.position);
				Vector3 planePoint = targetGameobject.position - trans.up * distanceToPlane;

				qTurret = Quaternion.LookRotation(planePoint - trans.position, transform.up);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, qTurret, turretDegreesPerSecond * Time.deltaTime);

				Vector3 v3 = new Vector3(0.0f, distanceToPlane, (planePoint - transform.position).magnitude);
				qlauncherGameobject = Quaternion.LookRotation(v3);

                // Thanks to Max Braun for the fix..
                if (Quaternion.Angle(launcherGameobjectStartRot, qlauncherGameobject) <= maxlauncherAngle){
                    launcherGameobject.localRotation = Quaternion.RotateTowards(launcherGameobject.localRotation, qlauncherGameobject, launcherDegreesPerSecond * Time.deltaTime);
                }else{
                    Debug.Log("");
                }
    }

		//}


	}

}
