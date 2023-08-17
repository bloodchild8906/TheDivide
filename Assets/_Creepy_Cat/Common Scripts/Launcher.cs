using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ------------------------------------------------------------------------------------------
// Code by creepy cat, if you make some cool modifications, please send me them to :
// black.creepy.cat@gmail.com sometime i give voucher codes... :) 
// This code is given for free and for example, do not ask me for particular demands...
// Do like me, learn by work! https://docs.unity3d.com/ScriptReference/
// ------------------------------------------------------------------------------------------

public class Launcher : MonoBehaviour
{
    [SerializeField]
    public float FireRate;

    [SerializeField]
    public bool fire;

    [SerializeField]
    public float nextlaunch;

    [SerializeField]
    private GuidedMissile missile;

    [SerializeField]
    private Transform target;

    [SerializeField]
    private Transform launchSpot;

    private AudioSource audioData;

    // Start is called before the first frame update
    private void FireMissil()
    {
        Debug.Log("Fire");

        GuidedMissile newMissile = Instantiate(missile, launchSpot.position, launchSpot.rotation);
        newMissile.target = target;

        audioData = GetComponent<AudioSource>();
        audioData.Play(0);

    }

    // Update is called once per frame
    private void Update()
    {
        fire = Input.GetKey(KeyCode.Space);

        if (fire && Time.time >= nextlaunch)
        {
            nextlaunch = Time.time + FireRate;
            FireMissil();
        }
    }

}

