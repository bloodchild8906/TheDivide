using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Infinity_Example_Keys_01 : MonoBehaviour
{
	private Infinity_SwitchEngine_01 compReactor;
	private int AnimSwitch=0; 


    // Start is called before the first frame update
    void Awake()
    {
		compReactor = gameObject.GetComponent <Infinity_SwitchEngine_01>();

    }

    // Update is called once per frame
    void Update()
    {

		// Switch to landing mode or fligh mode
		if (Input.GetKeyDown(KeyCode.G) && compReactor.infinityReactorAnimRunning == 0 ){
			AnimSwitch = 1 - AnimSwitch;

			if (AnimSwitch == 0){
				compReactor.infinityReactorAnimSwitch = 0;
				compReactor.InfinityReactorAnimStart();
			}

			if (AnimSwitch == 1){
				compReactor.infinityReactorAnimSwitch = 1;
				compReactor.InfinityReactorAnimStart();
			}

		}

		// Emergency break
		if (Input.GetKeyDown(KeyCode.B) && compReactor.infinityReactorAnimRunning == 0 ){

			if (compReactor.infinityReactorPreviousAnim ==1){
				compReactor.infinityReactorAnimSwitch = 2;
				compReactor.InfinityReactorAnimStart();
				AnimSwitch = 0;
			}

		}		

		// H.A.L open the door. I'm sorry dave....
		if (Input.GetKeyDown(KeyCode.H) && compReactor.infinityHangarAnimARunning == 0 ){

			compReactor.infinityHangarAnimASwitch = 1 - compReactor.infinityHangarAnimASwitch;
			compReactor.InfinityHangarAnimAStart();

		}


    }
}
