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

public class Infinity_SwitchEngine_01 : MonoBehaviour {

	[Header("Reactors setup")]
	public GameObject reactorSupportLeft;
	public GameObject reactorSupportRight;

	public AudioClip shipReactorSoundOn;
	public AudioClip shipReactorSoundOff;
	public AudioClip shipEngineSound;

	public float reactorSupportMoveAngle = 35.0f;
	public float reactorSupportMoveTime = 6.0f;

	public GameObject reactorLeftForward;
	public GameObject reactorLeftBackward;

	public float reactorLeftMoveTime = 6.0f;

	public GameObject reactorRightForward;
	public GameObject reactorRightBackward;

	public float reactorRightMoveTime = 6.0f;

	[Header("Left flaps setup")]
	public GameObject reactorLeftForwardFlapA;
	public GameObject reactorLeftForwardFlapB;
	public GameObject reactorLeftForwardFlapC;
	public GameObject reactorLeftForwardFlapD;

	public GameObject reactorLeftBackwardFlapA;
	public GameObject reactorLeftBackwardFlapB;
	public GameObject reactorLeftBackwardFlapC;
	public GameObject reactorLeftBackwardFlapD;

	public float reactorLeftFlapMoveTime = 6.0f;

	[Header("Right flaps setup")]
	public GameObject reactorRightForwardFlapA;
	public GameObject reactorRightForwardFlapB;
	public GameObject reactorRightForwardFlapC;
	public GameObject reactorRightForwardFlapD;

	public GameObject reactorRightBackwardFlapA;
	public GameObject reactorRightBackwardFlapB;
	public GameObject reactorRightBackwardFlapC;
	public GameObject reactorRightBackwardFlapD;

	public float reactorRightFlapMoveTime = 6.0f;

	// -------------------------
	// Var for reactor particles
	// -------------------------
	[Header("Particles setup")]
	public ParticleSystem reactorAfterburnerA;
	public ParticleSystem reactorAfterburnerB;
	public ParticleSystem reactorAfterburnerC;
	public ParticleSystem reactorAfterburnerD;

	public ParticleSystem reactorSmokeA;
	public ParticleSystem reactorSmokeB;
	public ParticleSystem reactorSmokeC;
	public ParticleSystem reactorSmokeD;

	public float reactorAfterburnerStrengh = 60;
	public float reactorSmokeStrengh = 60;

	// -------------------
	// Var for hangar door
	// -------------------
	[Header("HangarA door setup")]
	public GameObject shipHangarDoorA;
	public AudioClip shipHangarSound;

	public float shipHangarMoveATime = 10;



	// --------------------
	// Var for anims switch
	// --------------------
	[Header("Public vars you can use")]
	public int infinityReactorAnimSwitch = 0;
	public int infinityReactorAnimRunning =0;
	public int infinityReactorPreviousAnim = 0;
	public int infinityHangarAnimASwitch = 0;
	public int infinityHangarAnimARunning =0;

	// Private vars
	private AudioSource audioSource;

	private float ParticleAfterburnerDeath;
	private float ParticleSmokeDeath;
	private float EngineSoundPitch=1.0f;

	// Enum for ease
//	public enum MyEnum{
//		One, Two, Three
//	}
//
//	public MyEnum fooBar;

	void Start(){
		audioSource = GetComponent<AudioSource>();

		audioSource.clip = shipEngineSound;
		audioSource.Play();
		//audioSource.pitch = 1;


		var mainA = reactorAfterburnerA.main;
		mainA.startSpeed =0.0f;

		var mainB = reactorAfterburnerB.main;
		mainB.startSpeed =0.0f;

		var mainC = reactorAfterburnerC.main;
		mainC.startSpeed =0.0f;

		var mainD = reactorAfterburnerD.main;
		mainD.startSpeed =0.0f;

	}

	void Update(){

		//  ---------------------------------------------------------
		// Function to manage particles via the differents animations
		//  ---------------------------------------------------------
		if (infinityReactorAnimSwitch == 0){
			ParticleAfterburnerDeath = Mathf.Lerp(ParticleAfterburnerDeath,0.0f,Time.deltaTime * 0.05f); //0.12
			ParticleSmokeDeath = Mathf.Lerp(ParticleSmokeDeath,reactorSmokeStrengh,Time.deltaTime * 0.20f);

			InfinityReactorSoundPitch (1.0f,0.15f);
		}	
			
		if (infinityReactorAnimSwitch == 1){
			ParticleAfterburnerDeath = Mathf.Lerp(ParticleAfterburnerDeath,reactorAfterburnerStrengh,Time.deltaTime * 1.65f);//0.35
			ParticleSmokeDeath = Mathf.Lerp(ParticleSmokeDeath,0.0f,Time.deltaTime * 0.17f);

			InfinityReactorSoundPitch (3.0f,0.15f);

		}	


		if (infinityReactorAnimSwitch == 2){
			ParticleAfterburnerDeath = Mathf.Lerp(ParticleAfterburnerDeath,reactorAfterburnerStrengh,Time.deltaTime * 1.65f);//0.35
			ParticleSmokeDeath = Mathf.Lerp(ParticleSmokeDeath,0.0f,Time.deltaTime * 0.17f);

			InfinityReactorSoundPitch (1.0f,0.15f);
		}	



		//  --------------------------------
		// Function to poke particles params
		//  --------------------------------
		var mainA = reactorAfterburnerA.main;
		mainA.startSpeed = ParticleAfterburnerDeath;

		var mainB = reactorAfterburnerB.main;
		mainB.startSpeed = ParticleAfterburnerDeath;

		var mainC = reactorAfterburnerC.main;
		mainC.startSpeed = ParticleAfterburnerDeath;

		var mainD = reactorAfterburnerD.main;
		mainD.startSpeed = ParticleAfterburnerDeath;



		var mainE = reactorSmokeA.main;
		mainE.startSpeed = ParticleSmokeDeath;

		var mainF = reactorSmokeB.main;
		mainF.startSpeed = ParticleSmokeDeath;

		var mainG = reactorSmokeC.main;
		mainG.startSpeed = ParticleSmokeDeath;

		var mainH = reactorSmokeD.main;
		mainH.startSpeed = ParticleSmokeDeath;
	}



	// **********************
	// Functions for reactors
	// **********************
	public void InfinityReactorAnimStart(){
		infinityReactorAnimRunning = 1;	

		if (infinityReactorAnimSwitch == 0){
			AnimLandingModeStart ();
		}

		if (infinityReactorAnimSwitch == 1){
			AnimCruiseModeStart ();
		}	

		if (infinityReactorAnimSwitch == 2){
			AnimBreakModeStart ();
		}	

	}
		
	void  InfinityReactorAnimEnd(){
		infinityReactorAnimRunning = 0;
		infinityReactorPreviousAnim = infinityReactorAnimSwitch;
	}

	void InfinityReactorSoundPitch(float pitch, float speed){
		EngineSoundPitch = Mathf.Lerp (EngineSoundPitch, pitch, Time.deltaTime * speed);
		audioSource.pitch = EngineSoundPitch;
	}


	// ***********************************
	// Public : Functions for hangar doors
	// ***********************************
	public void InfinityHangarAnimAStart(){
		infinityHangarAnimARunning = 1;

		//audioSource.clip = shipHangarSound;
		audioSource.PlayOneShot(shipHangarSound, 1.0F);

		if (infinityHangarAnimASwitch == 0){
			TweenRX.Add (shipHangarDoorA, shipHangarMoveATime,0).From(90).EaseInOutCubic().Then(InfinityHangarAnimAEnd);
		}else{
			TweenRX.Add (shipHangarDoorA, shipHangarMoveATime,90).Relative().EaseInOutCubic().Then(InfinityHangarAnimAEnd); 
		}
	}
		
	void InfinityHangarAnimAEnd(){
		infinityHangarAnimARunning = 0;
	}

	// ***********************************************************************************************************
	// Functions for the differents animations, you can, with a bit of intellectual curiosity, create your owns...
	// ***********************************************************************************************************

	// --------------------------
	// Launching landing mode anim
	// --------------------------
	void AnimLandingModeStart(){
		// Launch audio
		audioSource.PlayOneShot(shipReactorSoundOff, 1.0F);

		// Rotate to landing mode
		TweenRX.Add (reactorSupportRight, reactorSupportMoveTime,0).From(-reactorSupportMoveAngle).EaseInOutBack(); 
		TweenRX.Add (reactorSupportLeft, reactorSupportMoveTime, 0).From(-reactorSupportMoveAngle).EaseInOutBack();

		TweenRX.Add (reactorLeftForward, reactorLeftMoveTime,0).From(-reactorSupportMoveAngle).EaseInOutBack().Then(InfinityReactorAnimEnd); 
		TweenRX.Add (reactorLeftBackward, reactorLeftMoveTime, 0).From(-reactorSupportMoveAngle).EaseInOutBack();

		TweenRX.Add (reactorRightForward, reactorRightMoveTime,0).From(-reactorSupportMoveAngle).EaseInOutBack(); 
		TweenRX.Add (reactorRightBackward, reactorRightMoveTime, 0).From(-reactorSupportMoveAngle).EaseInOutBack();




		TweenRX.Add (reactorRightForwardFlapA, reactorRightFlapMoveTime,0).From(-90).Delay(7).EaseOutBounce(); 
		TweenRX.Add (reactorRightForwardFlapB, reactorRightFlapMoveTime, 0).From(-90).Delay(5).EaseOutBounce();
		TweenRX.Add (reactorRightForwardFlapC, reactorRightFlapMoveTime, 0).From(-90).Delay(7).EaseOutBounce();
		TweenRX.Add (reactorRightForwardFlapD, reactorRightFlapMoveTime, 0).From(-90).Delay(6).EaseOutBounce();

		TweenRX.Add (reactorRightBackwardFlapA, reactorRightFlapMoveTime,0).From(-90).Delay(6).EaseOutBounce(); 
		TweenRX.Add (reactorRightBackwardFlapB, reactorRightFlapMoveTime, 0).From(-90).Delay(7).EaseOutBounce();
		TweenRX.Add (reactorRightBackwardFlapC, reactorRightFlapMoveTime, 0).From(-90).Delay(5).EaseOutBounce();
		TweenRX.Add (reactorRightBackwardFlapD, reactorRightFlapMoveTime, 0).From(-90).Delay(7).EaseOutBounce();



		TweenRX.Add (reactorLeftForwardFlapA, reactorLeftFlapMoveTime,0).From(-90).Delay(7).EaseOutBounce(); 
		TweenRX.Add (reactorLeftForwardFlapB, reactorLeftFlapMoveTime, 0).From(-90).Delay(6).EaseOutBounce();
		TweenRX.Add (reactorLeftForwardFlapC, reactorLeftFlapMoveTime, 0).From(-90).Delay(5).EaseOutBounce();
		TweenRX.Add (reactorLeftForwardFlapD, reactorLeftFlapMoveTime, 0).From(-90).Delay(7).EaseOutBounce();

		TweenRX.Add (reactorLeftBackwardFlapA, reactorLeftFlapMoveTime,0).From(-90).Delay(5).EaseOutBounce(); 
		TweenRX.Add (reactorLeftBackwardFlapB, reactorLeftFlapMoveTime, 0).From(-90).Delay(7).EaseOutBounce();
		TweenRX.Add (reactorLeftBackwardFlapC, reactorLeftFlapMoveTime, 0).From(-90).Delay(6).EaseOutBounce();
		TweenRX.Add (reactorLeftBackwardFlapD, reactorLeftFlapMoveTime, 0).From(-90).Delay(5).EaseOutBounce();

	}


	// --------------------------
	// Launching cruise mode anim
	// --------------------------
	void AnimCruiseModeStart(){
		// Launch audio
		audioSource.PlayOneShot(shipReactorSoundOn, 1.0F);

		// Rotate to fligh mode
		TweenRX.Add (reactorSupportRight, reactorSupportMoveTime,-reactorSupportMoveAngle).Relative().Delay(3).EaseInOutBack(); 
		TweenRX.Add (reactorSupportLeft, reactorSupportMoveTime, -reactorSupportMoveAngle).Relative().Delay(3).EaseInOutBack(); 

		TweenRX.Add (reactorLeftForward, reactorLeftMoveTime,-reactorSupportMoveAngle).Relative().Delay(8).EaseInOutBack().Then(InfinityReactorAnimEnd); 
		TweenRX.Add (reactorLeftBackward, reactorLeftMoveTime, -reactorSupportMoveAngle).Relative().Delay(9).EaseInOutBack();

		TweenRX.Add (reactorRightForward, reactorRightMoveTime,-reactorSupportMoveAngle).Relative().Delay(8).EaseInOutBack(); 
		TweenRX.Add (reactorRightBackward, reactorRightMoveTime, -reactorSupportMoveAngle).Relative().Delay(9).EaseInOutBack();




		TweenRX.Add (reactorRightForwardFlapA, reactorRightFlapMoveTime,-90).Relative().Delay(13).EaseOutBounce(); 
		TweenRX.Add (reactorRightForwardFlapB, reactorRightFlapMoveTime, -90).Relative().Delay(12).EaseOutBounce();
		TweenRX.Add (reactorRightForwardFlapC, reactorRightFlapMoveTime, -90).Relative().Delay(11).EaseOutBounce();
		TweenRX.Add (reactorRightForwardFlapD, reactorRightFlapMoveTime, -90).Relative().Delay(13).EaseOutBounce();

		TweenRX.Add (reactorRightBackwardFlapA, reactorRightFlapMoveTime,-90).Relative().Delay(12).EaseOutBounce(); 
		TweenRX.Add (reactorRightBackwardFlapB, reactorRightFlapMoveTime, -90).Relative().Delay(11).EaseOutBounce();
		TweenRX.Add (reactorRightBackwardFlapC, reactorRightFlapMoveTime, -90).Relative().Delay(13).EaseOutBounce();
		TweenRX.Add (reactorRightBackwardFlapD, reactorRightFlapMoveTime, -90).Relative().Delay(12).EaseOutBounce();



		TweenRX.Add (reactorLeftForwardFlapA, reactorLeftFlapMoveTime,-90).Relative().Delay(12).EaseOutBounce(); 
		TweenRX.Add (reactorLeftForwardFlapB, reactorLeftFlapMoveTime, -90).Relative().Delay(13).EaseOutBounce();
		TweenRX.Add (reactorLeftForwardFlapC, reactorLeftFlapMoveTime, -90).Relative().Delay(11).EaseOutBounce();
		TweenRX.Add (reactorLeftForwardFlapD, reactorLeftFlapMoveTime, -90).Relative().Delay(12).EaseOutBounce();

		TweenRX.Add (reactorLeftBackwardFlapA, reactorLeftFlapMoveTime,-90).Relative().Delay(11).EaseOutBounce(); 
		TweenRX.Add (reactorLeftBackwardFlapB, reactorLeftFlapMoveTime, -90).Relative().Delay(13).EaseOutBounce();
		TweenRX.Add (reactorLeftBackwardFlapC, reactorLeftFlapMoveTime, -90).Relative().Delay(11).EaseOutBounce();
		TweenRX.Add (reactorLeftBackwardFlapD, reactorLeftFlapMoveTime, -90).Relative().Delay(12).EaseOutBounce();
	}


	// -----------------------------------
	// Launching emergency break mode anim
	// -----------------------------------
	void AnimBreakModeStart(){
		// Launch audio
		audioSource.PlayOneShot(shipReactorSoundOff, 1.0F);

		// Rotate to break mode
		TweenRX.Add (reactorSupportRight, reactorSupportMoveTime/2,80).Relative().EaseInOutBack(); 
		TweenRX.Add (reactorSupportLeft, reactorSupportMoveTime/2, 80).Relative().EaseInOutBack(); 

		TweenRX.Add (reactorLeftForward, reactorLeftMoveTime/2,reactorSupportMoveAngle).Relative().EaseInOutBack().Then(InfinityReactorAnimEnd); 
		TweenRX.Add (reactorLeftBackward, reactorLeftMoveTime/2, reactorSupportMoveAngle).Relative().EaseInOutBack().Then(AnimBreakModeEnd);

		TweenRX.Add (reactorRightForward, reactorRightMoveTime/2,reactorSupportMoveAngle).Relative().EaseInOutBack(); 
		TweenRX.Add (reactorRightBackward, reactorRightMoveTime/2, reactorSupportMoveAngle).Relative().EaseInOutBack();




		TweenRX.Add (reactorRightForwardFlapA, reactorRightFlapMoveTime,90).Relative().Delay(3).EaseOutBounce(); 
		TweenRX.Add (reactorRightForwardFlapB, reactorRightFlapMoveTime, 90).Relative().Delay(3).EaseOutBounce();
		TweenRX.Add (reactorRightForwardFlapC, reactorRightFlapMoveTime, 90).Relative().Delay(3).EaseOutBounce();
		TweenRX.Add (reactorRightForwardFlapD, reactorRightFlapMoveTime, 90).Relative().Delay(3).EaseOutBounce();

		TweenRX.Add (reactorRightBackwardFlapA, reactorRightFlapMoveTime,90).Relative().Delay(4).EaseOutBounce(); 
		TweenRX.Add (reactorRightBackwardFlapB, reactorRightFlapMoveTime, 90).Relative().Delay(4).EaseOutBounce();
		TweenRX.Add (reactorRightBackwardFlapC, reactorRightFlapMoveTime, 90).Relative().Delay(4).EaseOutBounce();
		TweenRX.Add (reactorRightBackwardFlapD, reactorRightFlapMoveTime, 90).Relative().Delay(4).EaseOutBounce();


		TweenRX.Add (reactorLeftForwardFlapA, reactorLeftFlapMoveTime,90).Relative().Delay(3).EaseOutBounce(); 
		TweenRX.Add (reactorLeftForwardFlapB, reactorLeftFlapMoveTime, 90).Relative().Delay(3).EaseOutBounce();
		TweenRX.Add (reactorLeftForwardFlapC, reactorLeftFlapMoveTime, 90).Relative().Delay(3).EaseOutBounce();
		TweenRX.Add (reactorLeftForwardFlapD, reactorLeftFlapMoveTime, 90).Relative().Delay(3).EaseOutBounce();

		TweenRX.Add (reactorLeftBackwardFlapA, reactorLeftFlapMoveTime,90).Relative().Delay(4).EaseOutBounce(); 
		TweenRX.Add (reactorLeftBackwardFlapB, reactorLeftFlapMoveTime, 90).Relative().Delay(4).EaseOutBounce();
		TweenRX.Add (reactorLeftBackwardFlapC, reactorLeftFlapMoveTime, 90).Relative().Delay(4).EaseOutBounce();
		TweenRX.Add (reactorLeftBackwardFlapD, reactorLeftFlapMoveTime, 90).Relative().Delay(4).EaseOutBounce();
	}

	void AnimBreakModeEnd(){

		// Rotate to break mode
		TweenRX.Add (reactorSupportRight, reactorSupportMoveTime/2,-reactorSupportMoveAngle).Relative().EaseInOutBack(); 
		TweenRX.Add (reactorSupportLeft, reactorSupportMoveTime/2, -reactorSupportMoveAngle).Relative().EaseInOutBack(); 

		InfinityReactorAnimEnd ();
		infinityReactorAnimSwitch = 0;
	}

}
