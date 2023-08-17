// ----------------------------------------------------------------------------------------------------------
// Door script copyright 2017 by Creepy Cat do not distribute/sale full or partial code without my permission
// ----------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorDoubleSlide : MonoBehaviour {
    
    //Doors
    public Transform doorL = null;
    public Transform doorR = null;

    private Vector3 initialDoorL;
    private Vector3 initialDoorR;
    private Vector3 doorDirection;

    public enum Direction { X, Y, Z };
    public Direction directionType = Direction.Y;

    //Control Variables
    public float speed = 2.0f;
    public float openDistance = 2.0f;

    //Internal... stuff
    private float point = 0.0f;
    private bool opening = false;

    //Record initial positions
	void Start () {
        if (doorL){
            initialDoorL = doorL.localPosition;
        }

        if (doorR){
            initialDoorR = doorR.localPosition;
        }
	}

    //Something approaching? open doors
    void OnTriggerEnter(Collider other)
    {
        opening = true;

        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();
    }

    //Something left? close doors
    void OnTriggerExit(Collider other)
    {
        opening = false;

        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();
    }
	

    //Open or close doors
	void Update () {
        // Direction selection
        if (directionType == Direction.X)
        {
            doorDirection = Vector3.right;
        }
        else if (directionType == Direction.Y)
        {
            doorDirection = Vector3.up;
        }
        else if (directionType == Direction.Z)
        {
            doorDirection = Vector3.back;
        }

        // If opening
        if (opening)
        {
            point = Mathf.Lerp(point, 1.0f, Time.deltaTime * speed);
        }
        else
        {
            point = Mathf.Lerp(point, 0.0f, Time.deltaTime * speed);
        }

        // Move doors
        if (doorL)
        {
            doorL.localPosition = initialDoorL + (doorDirection * point * openDistance);
        }

        if (doorR)
        {
            doorR.localPosition = initialDoorR + (-doorDirection * point * openDistance);
        }
	}
}
