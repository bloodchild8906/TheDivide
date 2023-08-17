using UnityEngine;
using System.Collections;

public class VertDoorManager : MonoBehaviour {

	public DoorVert door1;
	
	void OnTriggerEnter(){
		if (door1!=null){
			door1.OpenDoor();	
		}

	}
}
