using Invector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vSnapToBody : MonoBehaviour
{
    public const string manuallyAssignBone = "ManuallyAssign";
    public vBodySnappingControl bodySnap;
    public Transform boneToSnap;
    public string boneName;

    private void Start()
    {
        bodySnap = transform.root.GetComponentInChildren<vBodySnappingControl>(true);
        if (boneName != manuallyAssignBone)
        {
            if (bodySnap != null && bodySnap.boneSnappingList != null)
            {
                boneToSnap = bodySnap.GetBone(boneName);               
            }
        }

        if (boneToSnap)
        {
            transform.parent = boneToSnap;           
        }
    }    
}
