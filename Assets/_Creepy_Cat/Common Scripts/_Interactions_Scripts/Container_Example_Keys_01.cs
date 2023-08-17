using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Uween;

public class Container_Example_Keys_01 : MonoBehaviour
{

	public Container_Open_Close containerScript;

	[ColorUsageAttribute(false,true)]
	public Color neutralColor = Color.white;

	[ColorUsageAttribute(false,true)]
	public Color actionColor = Color.red;

	private Material rend;
	private Transform button;

	private float pushButtonDist = -0.01f ;

	void Start(){
		rend = GetComponent<Renderer>().material;
		button = GetComponent<Transform>();
	}

	void Update(){
		if (containerScript.containerAnimARunning == 0){
			rend.SetColor("_EmissionColor",neutralColor);
		}
	}

	void OnMouseOver(){

		if (Input.GetMouseButtonDown(0) && containerScript.containerAnimARunning == 0){
			containerScript.containerAnimSwitch = 1 - containerScript.containerAnimSwitch;
			containerScript.ContainerAnimStart ();

			TweenY.Add(gameObject, 0.2f , pushButtonDist).From(button.localPosition.y).Then(ButtonAnimEnd);
			rend.SetColor("_EmissionColor", actionColor);
		}
	}

	void ButtonAnimEnd(){
		TweenY.Add(gameObject, 0.2f ,-pushButtonDist).From(button.localPosition.y);
	}


}



