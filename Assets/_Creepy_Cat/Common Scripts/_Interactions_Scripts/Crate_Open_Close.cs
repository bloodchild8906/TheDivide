// *************************************************************************************************
// Interactivity controls for the scifi kit 3, by creepy cat copyright (2019)
// Do not sale directly, do not distribute for free, but use it for freeware/shareware games/apps...  
// This script allow you to open/close the crates :)
// Create your own by a copy of this class! Do not use it directly because i'll change something in.
// *************************************************************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uween;

public class Crate_Open_Close : MonoBehaviour
{
	[Header("Crate Setup")]
	public GameObject crateTop;
	public AudioClip crateSound;

	public float crateMoveAngle = 90.0f;
	public float crateMoveTime = 6.0f;

	[Header("Public vars you can use")]
	public int crateAnimARunning = 0;
	public int crateAnimSwitch = 0;

	private AudioSource audioSource;

    // Start is called before the first frame update
    void Start()
    {
		audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	// *********************************
	// Public : Functions for animations
	// *********************************
	public void CrateAnimStart(){
		crateAnimARunning = 1;

		if (crateAnimSwitch == 0){
			TweenRZ.Add (crateTop, crateMoveTime,0).From(crateMoveAngle).EaseInOutCubic().Then(CrateAnimEnd);
			audioSource.PlayOneShot(crateSound, 1.0F);
		}

		if (crateAnimSwitch == 1){
			TweenRZ.Add (crateTop, crateMoveTime,crateMoveAngle).Relative().EaseInOutCubic().Then(CrateAnimEnd); 
			audioSource.PlayOneShot(crateSound, 1.0F);
		}
	}
		
	void CrateAnimEnd(){
		crateAnimARunning = 0;
	}

	void OnMouseOver(){
		if (Input.GetMouseButtonDown(0) && crateAnimARunning == 0){
			crateAnimSwitch = 1 - crateAnimSwitch;
			CrateAnimStart ();
		}
	}


}
