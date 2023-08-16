using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vTeleport : MonoBehaviour
{
    public Transform targetPoint;
    public bool includeRoot;

    public enum RotationType
    {
        None,
        TargetForward,
        RelativeForward,
    }

    public RotationType rotationType = RotationType.None;

    public void Teleport(Collider collider)
    {
        Transform teleporter = includeRoot ? collider.transform.root : collider.transform;

        var localPosition = transform.InverseTransformPoint(teleporter.position);
        var localForward = transform.InverseTransformDirection(teleporter.forward);
        localPosition.Set(0, localPosition.y, 0);

        teleporter.position = targetPoint.TransformPoint(localPosition);

        switch (rotationType)
        {
            case RotationType.None:
                break;
            case RotationType.RelativeForward:
                teleporter.forward = targetPoint.TransformDirection(localForward);
                break;
            case RotationType.TargetForward:
                teleporter.rotation = targetPoint.rotation;
                break;
        }
    }
}