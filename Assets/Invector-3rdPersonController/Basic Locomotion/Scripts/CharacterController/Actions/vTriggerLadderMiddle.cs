using Invector.vCharacterController.vActions;
using UnityEngine;

public class vTriggerLadderMiddle : MonoBehaviour
{
    public Transform refTarget;
    public float stepHeight = 0.5f;
    public float entryAngleFwd = 45f;
    public bool debugMode = true;

    vLadderAction ladderAction;
    internal BoxCollider _collider;

    void Start()
    {
        _collider = GetComponent<BoxCollider>();
    }

    private void OnDrawGizmos()
    {
        if (!debugMode) return;

        if (!_collider)
            _collider = GetComponent<BoxCollider>();
        else
        {
            int steps = (int)(_collider.size.y / stepHeight);

            for (int i = 0; i < steps; i++)
            {
                Gizmos.DrawSphere(refTarget.position + refTarget.up * i * stepHeight, 0.1f);
                Gizmos.DrawLine(refTarget.position + refTarget.up * i * stepHeight, refTarget.position + refTarget.up * i * stepHeight + refTarget.forward * 0.5f);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (Vector3.Angle(other.transform.forward, refTarget.transform.forward) > entryAngleFwd) return;

            ladderAction = other.GetComponent<vLadderAction>();
            if (ladderAction.isUsingLadder) return;
            ladderAction.TriggerMidAirEnterLadder(this);
        }
    }
}
