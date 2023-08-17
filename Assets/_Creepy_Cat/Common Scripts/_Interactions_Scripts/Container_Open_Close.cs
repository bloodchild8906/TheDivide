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

public class Container_Open_Close : MonoBehaviour
{
	[Header("Container Setup")]
	public GameObject containerDoor;
	public AudioClip containerSound;

	// -------------------------------------------------------------
	// Unfortunaly i got a bug with angles sup to -90 / 90
	// A strange gimbal lock problem appear, tryed TweenRXYZ, and
	// different axis based methods, i get always a problem...
	// -------------------------------------------------------------
	public float containerMoveAngle = 90.0f;
	public float containerMoveTime = 6.0f;

	[Header("Public vars you can use")]
	public int containerAnimARunning = 0;
	public int containerAnimSwitch = 0;

	private AudioSource audioSource;

    void Start(){
		audioSource = GetComponent<AudioSource>();
    }

	// ---------------------------------
	// Public : Functions for animations
	// ---------------------------------
	public void ContainerAnimStart(){
		containerAnimARunning = 1;

		if (containerAnimSwitch == 0){
			TweenRX.Add(containerDoor, containerMoveTime,0).From(containerMoveAngle).EaseInOutCubic().Then(ContainerAnimEnd);
			audioSource.PlayOneShot(containerSound, 1.0F);
		}

		if (containerAnimSwitch == 1){
			TweenRX.Add(containerDoor, containerMoveTime, containerMoveAngle).Relative().EaseInOutCubic().Then(ContainerAnimEnd); 
			audioSource.PlayOneShot(containerSound, 1.0F);
		}
	}
		
	void ContainerAnimEnd(){
		containerAnimARunning = 0;
	}
}
