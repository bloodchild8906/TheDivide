// -------------// ------------------------------------------------------------------------------------------
// Code by creepy cat, if you make some cool modifications, please send me them to :
// black.creepy.cat@gmail.com sometime i give voucher codes... :) 
// This code is given for free and for example, do not ask me for particular demands...
// Do like me, learn by work! https://docs.unity3d.com/ScriptReference/
// ------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace creepycat.scifikitvol3 {

	public class LedFlash : MonoBehaviour {
	    
	    public GameObject[] Flash;
	    public float repeatTime = 0.1f;
	    private float delayInc = 0.0f;

	    void Start(){
	        modifyDelay();
	    }

	  
	    private void modifyDelay(){
	        for (int i = 0; i < Flash.Length; ++i){
	            var emission = Flash[i].GetComponent<ParticleSystem>().main;

	            emission.startDelay = delayInc;
	            delayInc = delayInc + repeatTime;
	      
	        }
	    }
	}

}
