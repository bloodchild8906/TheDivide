// My first unity script :) I use this crap since 7 years to animate anything...
// black.creepy.cat@gmail.com 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]

namespace creepycat.scifikitvol3 {
		
	public class Turn_Move : MonoBehaviour {
		public float TurnX;
	    public float TurnY;
	    public float TurnZ;

	    public float MoveX;
	    public float MoveY;
	    public float MoveZ;

		public bool World;
		
		// Update is called once per frame
		void Update () {
			if (World == true) {
				transform.Rotate(TurnX * Time.deltaTime,TurnY * Time.deltaTime,TurnZ * Time.deltaTime, Space.World);
				transform.Translate(MoveX * Time.deltaTime, MoveY * Time.deltaTime, MoveZ * Time.deltaTime, Space.World);
			}else{
				transform.Rotate(TurnX * Time.deltaTime,TurnY * Time.deltaTime,TurnZ * Time.deltaTime, Space.Self);
				transform.Translate(MoveX * Time.deltaTime, MoveY * Time.deltaTime, MoveZ * Time.deltaTime, Space.Self);
			}
		}

	}

}

