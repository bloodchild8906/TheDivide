// *************************************************************************************************
// Infinity Spaceship control state engine, by creepy cat copyright (2019)
// Do not sale directly, do not distribute for free, but use it for freeware/shareware games/apps...  
//
// Special thanks to this guy : https://github.com/beinteractive/Uween for this pure 
// piece of code... UWEEN : finally a library of tweening simple to understand...
//
// This code is give for example, to show you how to interact with the kit... 
// It's not a fligh model, just a example of tweening animation by code...
//
// Creepy note : it's a low level kernel, it need to be structured... But it's a good start
// to devs your owns, because the code is simple to understand... no complicated quaternion shits...
// You just need to read the Uween doc and make some simple tests. 
//
// Create your own by a copy of this class! Do not use it directly because i'll change something in.
// *************************************************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uween;


public class Walkyrie_SwitchEngine_01 : MonoBehaviour
{

// -------------------------------
// Inspector help to organise vars
// -------------------------------
//	[System.Serializable]
//	public class Movement
//	{
//		public float moveV;
//		public float moveH;
//
//		public bool isRunning;
//		public bool isLooking;
//	}
//	public Movement movement;




	[Header("Wings setup")]
	public GameObject wingLeft;
	public GameObject wingRight;
	public AudioClip wingSound;

	public float wingMoveAngle = 90.0f;
	public float wingMoveTime = 6.0f;

	[Header("Cockpit setup")]
	public GameObject cockpitGlass;
	public float cockpitMoveAngle = 90.0f;
	public float cockpitMoveTime = 6.0f;
	public AudioClip cockpitSound;

	[Header("Gears setup")]
	public GameObject gearA;
	public GameObject gearB;
	public GameObject gearC;
	public float gearMoveAngle = 90.0f;
	public float gearMoveTime = 6.0f;
	public AudioClip gearSound;

	[Header("Particles setup")]
	public ParticleSystem reactorAfterburnerA;
	public ParticleSystem reactorAfterburnerB;
	public AudioClip reactorAfterburnerASound;
	public float reactorAfterburnerStrengh = 60;


	// --------------------
	// Var for anims switch
	// --------------------
	[Header("Public vars you can use")]
	public int walkyrieAnimSwitch = 0;
	public int walkyrieAnimARunning = 0;
	public int walkyrieAnimBRunning = 0;
	public int walkyrieAnimCRunning = 0;
	public int walkyrieAfterburnerOn = 0;
	public int walkyrieEngineOn = 0;

	private float ParticleAfterburnerDeath;
	private AudioSource audioSource;

	private float EngineSoundPitch=1.0f;

    // Start is called before the first frame update
    void Start(){
		
		audioSource = GetComponent<AudioSource>();
		audioSource.clip = reactorAfterburnerASound;
		audioSource.Play();

		var mainA = reactorAfterburnerA.main;
		mainA.startLifetime =0.0f;

		var mainB = reactorAfterburnerB.main;
		mainB.startLifetime =0.0f;
    }

    // Update is called once per frame
    void Update(){

		if (walkyrieAfterburnerOn == 0){
			ParticleAfterburnerDeath = Mathf.Lerp(ParticleAfterburnerDeath,0,Time.deltaTime * 0.25f);//0.35
			WalkyrieReactorSoundPitch (1.0f,0.25f);
		}else{
			ParticleAfterburnerDeath = Mathf.Lerp(ParticleAfterburnerDeath,reactorAfterburnerStrengh,Time.deltaTime * 0.25f);//0.35
			WalkyrieReactorSoundPitch (3.0f,0.25f);
		}

		//  --------------------------------
		// Function to poke particles params
		//  --------------------------------
		var mainA = reactorAfterburnerA.main;
		mainA.startLifetime = ParticleAfterburnerDeath;

		var mainB = reactorAfterburnerB.main;
		mainB.startLifetime = ParticleAfterburnerDeath;
    }

	// ***********************************
	// Public : Functions for hangar doors
	// ***********************************
	public void WalkyrieWingAnimStart(){
		walkyrieAnimARunning = 1;

		if (walkyrieAnimSwitch == 0){
			TweenRY.Add (wingLeft, wingMoveTime,0).From(wingMoveAngle).EaseInOutCubic().Then(WalkyrieWingAnimEnd);
			TweenRY.Add (wingRight, wingMoveTime,0).From(-wingMoveAngle).EaseInOutCubic().Then(WalkyrieWingAnimEnd);
			audioSource.PlayOneShot(wingSound, 1.0F);
		}

		if (walkyrieAnimSwitch == 1){
			TweenRY.Add (wingLeft, wingMoveTime,wingMoveAngle).Relative().EaseInOutCubic().Then(WalkyrieWingAnimEnd); 
			TweenRY.Add (wingRight, wingMoveTime,-wingMoveAngle).Relative().EaseInOutCubic().Then(WalkyrieWingAnimEnd); 
			audioSource.PlayOneShot(wingSound, 1.0F);
		}
	}


	void WalkyrieWingAnimEnd(){
		walkyrieAnimARunning = 0;
	}

	void WalkyrieReactorSoundPitch(float pitch, float speed){
		EngineSoundPitch = Mathf.Lerp (EngineSoundPitch, pitch, Time.deltaTime * speed);
		audioSource.pitch = EngineSoundPitch;
	}


	// ************************************
	// Public : Functions for cockpit glass
	// ************************************
	public void WalkyrieCockpitAnimStart(){
		walkyrieAnimBRunning = 1;

		if (walkyrieAnimSwitch == 2){
			TweenRX.Add (cockpitGlass, cockpitMoveTime,0).From(cockpitMoveAngle).EaseInOutCubic().Then(WalkyrieCockpitAnimEnd);
			audioSource.PlayOneShot(cockpitSound, 1.0F);
		}

		if (walkyrieAnimSwitch == 3){
			TweenRX.Add (cockpitGlass, cockpitMoveTime,cockpitMoveAngle).Relative().EaseInOutCubic().Then(WalkyrieCockpitAnimEnd); 
			audioSource.PlayOneShot(cockpitSound, 1.0F);
		}
	}


	void WalkyrieCockpitAnimEnd(){
		walkyrieAnimBRunning = 0;
	}

	// *********************************
	// Public : Functions for gear anims
	// *********************************
	public void WalkyrieGearAnimStart(){
		walkyrieAnimCRunning = 1;

		if (walkyrieAnimSwitch == 4){
			TweenRX.Add (gearA, gearMoveTime,0).From(gearMoveAngle).EaseInOutCubic().Then(WalkyrieGearAnimEnd);
			TweenRX.Add (gearB, gearMoveTime,0).From(gearMoveAngle).EaseInOutCubic().Then(WalkyrieGearAnimEnd);
			TweenRX.Add (gearC, gearMoveTime,0).From(gearMoveAngle).EaseInOutCubic().Then(WalkyrieGearAnimEnd);
			audioSource.PlayOneShot(gearSound, 1.0F);
		}

		if (walkyrieAnimSwitch == 5){
			TweenRX.Add (gearA, gearMoveTime,gearMoveAngle).Relative().EaseInOutCubic().Then(WalkyrieGearAnimEnd); 
			TweenRX.Add (gearB, gearMoveTime,gearMoveAngle).Relative().EaseInOutCubic().Then(WalkyrieGearAnimEnd); 
			TweenRX.Add (gearC, gearMoveTime,gearMoveAngle).Relative().EaseInOutCubic().Then(WalkyrieGearAnimEnd); 
			audioSource.PlayOneShot(gearSound, 1.0F);
		}
	}


	void WalkyrieGearAnimEnd(){
		walkyrieAnimCRunning = 0;
	}



}
