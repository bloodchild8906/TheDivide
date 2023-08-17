using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ------------------------------------------------------------------------------------------
// Code by creepy cat, if you make some cool modifications, please send me them to :
// black.creepy.cat@gmail.com sometime i give voucher codes... :) 
// This code is given for free and for example, do not ask me for particular demands...
// Do like me, learn by work! https://docs.unity3d.com/ScriptReference/
// ------------------------------------------------------------------------------------------

public class GuidedMissile : MonoBehaviour
{
    [SerializeField]
    private float acceleration = 20f;

    [SerializeField]
    private float accelerationTime = 7f;

    [SerializeField]
    private float missileSpeed = 20f;

    [SerializeField]
    private float turnRate = 50f;

    [SerializeField]
    private float trackingDelay = 3f;

    public Transform target;

    [SerializeField]
    private bool missileActive = false;

    [SerializeField]
    private bool explodeAtStart = true;

    [SerializeField]
    private bool targetTracking = false;

    private bool isAccelerating = false;

    private float accelerateActiveTime = 0f;
    private Quaternion guideRotation;

    private Rigidbody rb;
    private AudioSource audioData;

    [SerializeField]
    private GameObject explosion;


    IEnumerator TargetTrackingDelay()
    {
        yield return new WaitForSeconds(Random.Range(trackingDelay, trackingDelay + 3f));
        targetTracking = true;
      //  Debug.Log("Tracking activate");
    }

    private void Start()
    {
       rb = GetComponent<Rigidbody>();
       ActivateMissile();

if (explodeAtStart)
       Instantiate(explosion, transform.position, transform.rotation);

    }

    private void ActivateMissile()
    {
        missileActive = true;
        accelerateActiveTime = Time.time;
        StartCoroutine(TargetTrackingDelay());

        audioData = GetComponent<AudioSource>();
        audioData.Play(0);

       // missileSpeed = Random.Range(missileSpeed, missileSpeed + (missileSpeed*2) );

    }

    private void OnCollisionEnter(Collision other)
    {
        if (missileActive == true)
        {

          if (targetTracking)
            {
                Instantiate(explosion, transform.position, transform.rotation);


                /*

                // Problem with physic explode radius, if someone can help...


                float radius = 15.0F;
                float power = 500.0F;

                Vector3 explosionPos = transform.position;
                Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
                foreach (Collider hit in colliders)

                {
                    Rigidbody rb = hit.GetComponent<Rigidbody>();

                    if (rb != null)
                        rb.AddExplosionForce(power, explosionPos, radius, 0.0F);
                }

                */

                Destroy(gameObject);
            }
        }
    }

    private void Update()
    {
        Run();
        GuideMissile();
    }
    
    private void Run()
    {
        if (Since(accelerateActiveTime) > accelerationTime)
            isAccelerating = false;
        else
            isAccelerating = true;

        if (!missileActive) return;

        if (isAccelerating)
            missileSpeed += acceleration * Time.deltaTime;

       // Debug.Log("Running");
        rb.velocity = transform.forward * missileSpeed;

        if (targetTracking)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, guideRotation, turnRate * Time.deltaTime);
    }
    
    private float Since(float since)
    {
        return Time.time - since;
    }


    private void GuideMissile()
    {
        if (target == null) return;

        if(targetTracking)
        {
            Vector3 relativePosition = target.position - transform.position;
            guideRotation = Quaternion.LookRotation(relativePosition, transform.up);
        }

       // Debug.Log("Tracking");
    }
}
