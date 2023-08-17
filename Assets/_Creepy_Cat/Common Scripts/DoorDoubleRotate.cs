// ----------------------------------------------------------------------------------------------------------
// Door script copyright 2017 by Creepy Cat do not distribute/sale full or partial code without my permission
// ----------------------------------------------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorDoubleRotate : MonoBehaviour {
    
    //Doors
    public Transform doorL = null;
    public Transform doorR = null;

    public float doorOpenAngle = 90.0f ;
    public float doorCloseAngle = 0.0f;

    public float openSmooth = 2.0f;

    public enum Direction { X, Y, Z };
    public Direction directionType = Direction.Y;

    //Internal... stuff
    private bool opening = false;


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

        // If opening
        if (opening)
        {
            // Direction selection
            if (directionType == Direction.X)
            {
                doorL.localRotation = Quaternion.Slerp(doorL.localRotation, Quaternion.Euler( doorOpenAngle ,0 ,0), Time.deltaTime * openSmooth);
                doorR.localRotation = Quaternion.Slerp(doorR.localRotation, Quaternion.Euler( -doorOpenAngle, 0, 0), Time.deltaTime * openSmooth);
            }
            else if (directionType == Direction.Y)
            {
                doorL.localRotation = Quaternion.Slerp(doorL.localRotation, Quaternion.Euler(0,doorOpenAngle, 0), Time.deltaTime * openSmooth);
                doorR.localRotation = Quaternion.Slerp(doorR.localRotation, Quaternion.Euler(0,-doorOpenAngle, 0), Time.deltaTime * openSmooth);
            }
            else if (directionType == Direction.Z)
            {
                doorL.localRotation = Quaternion.Slerp(doorL.localRotation, Quaternion.Euler(0, 0, doorOpenAngle), Time.deltaTime * openSmooth);
                doorR.localRotation = Quaternion.Slerp(doorR.localRotation, Quaternion.Euler(0, 0, -doorOpenAngle), Time.deltaTime * openSmooth);
            }


        }
        else
        {
            // Direction selection
            if (directionType == Direction.X)
            {
                doorL.localRotation = Quaternion.Slerp(doorL.localRotation, Quaternion.Euler(doorCloseAngle, 0, 0), Time.deltaTime * openSmooth);
                doorR.localRotation = Quaternion.Slerp(doorR.localRotation, Quaternion.Euler(-doorCloseAngle, 0, 0), Time.deltaTime * openSmooth);
            }
            else if (directionType == Direction.Y)
            {
                doorL.localRotation = Quaternion.Slerp(doorL.localRotation, Quaternion.Euler(0, doorCloseAngle, 0), Time.deltaTime * openSmooth);
                doorR.localRotation = Quaternion.Slerp(doorR.localRotation, Quaternion.Euler(0, -doorCloseAngle, 0), Time.deltaTime * openSmooth);
            }
            else if (directionType == Direction.Z)
            {
                doorL.localRotation = Quaternion.Slerp(doorL.localRotation, Quaternion.Euler(0, 0, doorCloseAngle), Time.deltaTime * openSmooth);
                doorR.localRotation = Quaternion.Slerp(doorR.localRotation, Quaternion.Euler(0, 0, -doorCloseAngle), Time.deltaTime * openSmooth);
            }
        }

	}
}
