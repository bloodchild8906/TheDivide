using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Walkyrie_Example_Keys_01 : MonoBehaviour
{
	private Walkyrie_SwitchEngine_01 compReactor;
	private int AnimASwitch=0; 
	private int AnimBSwitch=0; 
	private int AnimCSwitch=0; 

    // Start is called before the first frame update
    void Awake()
    {
		compReactor = gameObject.GetComponent <Walkyrie_SwitchEngine_01>();

    }

    // Update is called once per frame
    void Update()
    {

		// Switch wings to opened/closed
		if (Input.GetKeyDown(KeyCode.F) && compReactor.walkyrieAnimARunning == 0 ){
			AnimASwitch = 1 - AnimASwitch;

			if (AnimASwitch == 0){
				compReactor.walkyrieAnimSwitch = 0;
				compReactor.WalkyrieWingAnimStart();
			}

			if (AnimASwitch == 1){
				compReactor.walkyrieAnimSwitch = 1;
				compReactor.WalkyrieWingAnimStart();
			}

		}


		// Switch cockpit open / close
		if (Input.GetKeyDown(KeyCode.C) && compReactor.walkyrieAnimBRunning == 0 ){
			AnimBSwitch = 1 - AnimBSwitch;

			if (AnimBSwitch == 0){
				compReactor.walkyrieAnimSwitch = 2;
				compReactor.WalkyrieCockpitAnimStart();
			}

			if (AnimBSwitch == 1){
				compReactor.walkyrieAnimSwitch = 3;
				compReactor.WalkyrieCockpitAnimStart();
			}

		}

		// Switch to landing mode or fligh mode
		if (Input.GetKeyDown(KeyCode.G) && compReactor.walkyrieAnimCRunning == 0 ){
			AnimCSwitch = 1 - AnimCSwitch;

			if (AnimCSwitch == 0){
				compReactor.walkyrieAnimSwitch = 4;
				compReactor.WalkyrieGearAnimStart();
			}

			if (AnimCSwitch == 1){
				compReactor.walkyrieAnimSwitch = 5;
				compReactor.WalkyrieGearAnimStart();
			}

		}

		// Afterburner on/off
		if (Input.GetKeyDown(KeyCode.R) ){
			compReactor.walkyrieAfterburnerOn = 1 - compReactor.walkyrieAfterburnerOn;

			if (compReactor.walkyrieAfterburnerOn==1){
				compReactor.walkyrieEngineOn = 1;
			}else{
				compReactor.walkyrieEngineOn = 0;
			}

			//Debug.Log (compReactor.walkyrieEngineOn);
		}
    }
}
