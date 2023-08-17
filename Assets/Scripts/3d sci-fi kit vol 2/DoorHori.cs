using UnityEngine;
using System.Collections;

public class DoorHori : MonoBehaviour {

	public float translateValue;
	public float easeTime;
	public OTween.EaseType ease;
	public float waitTime;
	
	private Vector3 StartlocalPos;
	private Vector3 endlocalPos;
	
	private void Start(){
		StartlocalPos = transform.localPosition;	
		gameObject.isStatic = false;
	}
		
	public void OpenDoor(){
		OTween.ValueTo( gameObject,ease,0.0f,-translateValue,easeTime,0.0f,"StartOpen","UpdateOpenDoor","EndOpen");
		GetComponent<AudioSource>().Play();
	}
	
	private void UpdateOpenDoor(float f){		
		Vector3 pos = transform.TransformDirection( new Vector3( 1,0,0));
		transform.localPosition = StartlocalPos + pos*f;
		
	}

	private void UpdateCloseDoor(float f){		
		Vector3 pos = transform.TransformDirection( new Vector3( -f,0,0)) ;
		
		transform.localPosition = endlocalPos-pos;
		
	}
	
	private void EndOpen(){
		endlocalPos = transform.localPosition ;
		StartCoroutine( WaitToClose());
	}
	
	private IEnumerator WaitToClose(){
		
		yield return new WaitForSeconds(waitTime);
		OTween.ValueTo( gameObject,ease,0.0f,translateValue,easeTime,0.0f,"StartClose","UpdateCloseDoor","EndClose");
		GetComponent<AudioSource>().Play();
	}
}
