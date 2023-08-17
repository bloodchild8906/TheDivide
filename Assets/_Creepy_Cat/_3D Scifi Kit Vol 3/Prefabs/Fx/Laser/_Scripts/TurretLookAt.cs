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

public class TurretLookAt : MonoBehaviour
{
	public GameObject[] laserShoot;
	public Transform shootPosition;

	[HideInInspector]
	public int currentLaser = 0;
	public float shootSpeed = 1000;

	[Tooltip("Time until a projectile has been destroyed")]
	[SerializeField] private float delay = 5f;

	public Transform targetGameobject;
	public Transform gunGameobject;

	public float turretDegreesPerSecond = 45.0f;
	public float gunDegreesPerSecond = 45.0f;

	public float maxgunAngle = 45.0f;
	public float gunRecoil = 1.2f;

	private Quaternion qTurret;
	private Quaternion qgunGameobject;
	private Quaternion gunGameobjectStartRot;
	private Transform trans;

	private float canonStartPos;
	private AudioSource shootAudio;

	void Start(){
		trans = transform;
		gunGameobjectStartRot = gunGameobject.transform.localRotation;

		canonStartPos = gunGameobject.transform.localPosition.z;
		shootAudio = GetComponent<AudioSource>();
	}

	void EndAnim(){
		TweenZ.Add(gunGameobject.gameObject, 1.5f, canonStartPos).EaseOutElastic();
	}


	void Update(){
		//Debug.DrawRay(gunGameobject.position, gunGameobject.fozrward * 20.0f);

		if (Input.GetKeyDown(KeyCode.Mouse0)){

			// GameObject projectile = Instantiate(laserShoot[currentLaser], gunGameobject.position, Quaternion.identity) as GameObject;
			// GameObject projectile = Instantiate(laserShoot[currentLaser], gunGameobject.position, gunGameobject.rotation) as GameObject;
			GameObject projectile = Instantiate(laserShoot[currentLaser], shootPosition.position, gunGameobject.rotation) as GameObject;
			StartCoroutine(DestroyProjectile(Mathf.Abs(delay), projectile));



			// **********************************************************************************
			// Crappy code i know, if any better coder than me make some cool fix... send me an email!
			// I hate quaternions... if anybody can clamp the angles, i take too :)
			// **********************************************************************************
			projectile.GetComponent<Rigidbody>().AddForce(projectile.transform.forward * shootSpeed);
			// projectile.GetComponent<LaserImpact>().collisionNormal = hit.normal; // need to do...

			TweenZ.Add(gunGameobject.gameObject, 1.0f, canonStartPos - gunRecoil).EaseOutElastic().Then(EndAnim);
			shootAudio.Play();


		}else{

			if (targetGameobject){
				float distanceToPlane = Vector3.Dot(trans.up, targetGameobject.position - trans.position);
				Vector3 planePoint = targetGameobject.position - trans.up * distanceToPlane;

				qTurret = Quaternion.LookRotation(planePoint - trans.position, transform.up);
				transform.rotation = Quaternion.RotateTowards(transform.rotation, qTurret, turretDegreesPerSecond * Time.deltaTime);

				Vector3 v3 = new Vector3(0.0f, distanceToPlane, (planePoint - transform.position).magnitude);
				qgunGameobject = Quaternion.LookRotation(v3);

                // Thanks to Max Braun for the fix..
                if (Quaternion.Angle(gunGameobjectStartRot, qgunGameobject) <= maxgunAngle){
                    gunGameobject.localRotation = Quaternion.RotateTowards(gunGameobject.localRotation, qgunGameobject, gunDegreesPerSecond * Time.deltaTime);
                }else{
                    Debug.Log("");
                }
            }

		}


	}

	/// <summary>
	/// Destroy projectile after the given delay.
	/// </summary>
	/// <param name="delay"></param>
	/// <param name="projectile"></param>
	/// <returns></returns>
	private IEnumerator DestroyProjectile(float delay, GameObject projectile) {
		yield return new WaitForSeconds(delay);

		Destroy(projectile);
	}
}
