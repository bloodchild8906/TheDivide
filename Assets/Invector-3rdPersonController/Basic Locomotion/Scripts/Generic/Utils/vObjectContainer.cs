using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vObjectContainer : MonoBehaviour {

    static vObjectContainer instance;
	public static Transform root
    {
        get
        {
            if(!instance)
            {
                instance = new GameObject("Object Container", typeof(vObjectContainer)).GetComponent<vObjectContainer>();
            }
            return instance.transform;
        }
    }
}
