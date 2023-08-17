// **********************************************************************************
// Creepy Cat note : A simple code to auto kill a explode emitter, yeah that short :)
// **********************************************************************************
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectDieAfter : MonoBehaviour
{
    public float dieAfterSeconds = 3;

    void Update(){
        Destroy(gameObject, dieAfterSeconds);
    }
}
