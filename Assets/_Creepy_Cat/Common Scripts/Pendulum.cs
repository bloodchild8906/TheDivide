// Script to put a pendulum movement, it's cool...
// (C)black.creepy.cat@gmail.com 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pendulum : MonoBehaviour
{

    public float MaxAngleDeflection = 10.0f;
    public float SpeedOfPendulum = 0.5f;

    public enum Direction { OnlyX,  OnlyY,  OnlyZ,XAndY,XAndZ, XYAndZ };
    public Direction myDirection;

    void Start()
    {
        
        myDirection = Direction.OnlyX;
    }

    void Update()
    {

        float angleX = MaxAngleDeflection * Mathf.Sin(Time.time * SpeedOfPendulum);
        float angleZ = MaxAngleDeflection * Mathf.Cos(Time.time * SpeedOfPendulum);

        switch (myDirection)
        {
            case Direction.OnlyX:
                transform.localRotation = Quaternion.Euler(angleX, 0, 0);
                break;
            case Direction.OnlyY:
                transform.localRotation = Quaternion.Euler(0, angleX, 0);
                break;
            case Direction.OnlyZ:
                transform.localRotation = Quaternion.Euler(0, 0, angleX);
                break;

            case Direction.XAndY:
                transform.localRotation = Quaternion.Euler(angleX, angleZ, 0);
                break;
            case Direction.XAndZ:
                transform.localRotation = Quaternion.Euler(angleX, 0, angleZ);
                break;
            case Direction.XYAndZ:
                transform.localRotation = Quaternion.Euler(angleX, -angleX, angleZ);
                break;
        }



    }

}