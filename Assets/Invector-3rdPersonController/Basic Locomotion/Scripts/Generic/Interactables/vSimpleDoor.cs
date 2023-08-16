using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Invector
{
    [RequireComponent(typeof(BoxCollider))]
    [vClassHeader("Simple Door", openClose = false)]
    public class vSimpleDoor : vMonoBehaviour
    {
        [vReadOnly]
        public DoorState state;
        public Transform pivot;
        [SerializeField] protected bool _startOpened;
        public virtual bool startOpened { get { return _startOpened; } set { _startOpened = value; } }
        [SerializeField, FormerlySerializedAs("autoOpen")] protected bool _autoOpen = true;
        public virtual bool autoOpen { get { return _autoOpen; } set { _autoOpen = value; } }
        [SerializeField, FormerlySerializedAs("autoClose")] protected bool _autoClose = true;
        public virtual bool autoClose { get { return _autoClose; } set { _autoClose = value; } }
        [vHideInInspector("autoClose"), Tooltip("Close the door only if door is completely opened\n**The TimeToClose will be used yet")]
        public bool closeOnlyWhenOpened;
        [Tooltip("Target angle of Opened door")]
        public float angleOfOpen = 90f;
        [vHideInInspector("autoOpen"), Tooltip("Min angle between character forward and door that  can auto open")]
        public float minAngleToOpen = 45f;
        [Tooltip("Door can open to left side and to right side, if false, door will open just in to right side")]
        public bool openBothSide = true;
        public float closeSpeed = 2f;
        public float openSpeed = 2f;
        [vHideInInspector("autoClose"), Tooltip("Time to auto close door after Opened")]
        public float timeToClose = 1f;
        [Tooltip("Used when autoOpen or autoClose is checked")]
        public vTagMask tagsToOpen = new List<string>() { "Player" };

        private Vector3 currentAngle;
        private float angle;
        private bool _invertOpenSide;
        private Collider colliderInTrigger;
        public UnityEvent onStartOpen, onStartOpenRight, onStartOpenLeft, onStartClose;
        public UnityEvent onOpen, onOpenRight, onOpenLeft, onClose;

        public enum DoorState
        {
            Closed, Opened, Closing, Opening
        }

        float targetDoorAngle;
        bool stopDoor;

        protected virtual void Start()
        {
            if (!pivot) enabled = false;
            if (startOpened)
            {
                state = DoorState.Closed;
                Open();
            }
            else onClose.Invoke();
        }

        protected virtual void OnDrawGizmos()
        {
            if (pivot)
            {
                Gizmos.DrawSphere(transform.position, 0.1f);
                Gizmos.DrawLine(transform.position, pivot.position);
                Gizmos.DrawSphere(pivot.position, 0.1f);
            }
        }
        /// <summary>
        /// Set Door to auto open
        /// </summary>
        /// <param name="value"> auto open </param>
        public virtual void SetAutoOpen(bool value)
        {
            autoOpen = value;
        }

        /// <summary>
        /// Set Door to auto close
        /// </summary>
        /// <param name="value">auto close</param>
        public virtual void SetAutoClose(bool value)
        {
            autoClose = value;
        }

        /// <summary>
        /// Open Door
        /// </summary>
        /// <param name="invert">invert direction to open</param>
        public virtual void Open(bool invert)
        {
            _invertOpenSide = invert;
            Open();
        }

        /// <summary>
        /// Open Door
        /// </summary>
        public virtual void Open()
        {
            if (state != DoorState.Opening && state != DoorState.Opening)
            {
                targetDoorAngle = invertOpenSide ? -angleOfOpen : angleOfOpen;
                StartCoroutine(HandleDoor());
            }
        }

        /// <summary>
        /// Close Door
        /// </summary>
        public virtual void Close()
        {
            if (state != DoorState.Closing && state != DoorState.Closed)
            {
                targetDoorAngle = 0;
                StartCoroutine(HandleDoor());
            }
        }
        /// <summary>
        /// Open or close door depending othe your current <see cref="state"/>
        /// </summary>
        public virtual void ToggleOpenClose()
        {
            if (state == DoorState.Closed && state != DoorState.Opening)
            {
                Open();
            }
            else
            {
                Close();
            }
        }

        /// <summary>
        /// Open or close door Routine
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator HandleDoor()
        {
            bool open = Mathf.Abs(targetDoorAngle).Equals(angleOfOpen);
            state = open ? DoorState.Opening : DoorState.Closing;
            switch (state)///Call start event based in state;
            {
                case DoorState.Opening:
                    onStartOpen.Invoke();
                    if (invertOpenSide)
                        onStartOpenLeft.Invoke();
                    else
                        onStartOpenRight.Invoke();
                    break;
                case DoorState.Closing: onStartClose.Invoke(); break;
            }

            stopDoor = true;  //break last routine to exit (While) function
            yield return new WaitForEndOfFrame();
            stopDoor = false;  //start new routine
            while (!stopDoor)
            {
                ///Lerp  current angle to target door angle
                currentAngle.y = Mathf.MoveTowardsAngle(currentAngle.y, targetDoorAngle, (open ? openSpeed : closeSpeed));
                if (Mathf.Abs(currentAngle.y - targetDoorAngle) < 0.01f)///Check if target Door angle is reached
                {
                    currentAngle.y = targetDoorAngle;
                    pivot.localEulerAngles = currentAngle;
                    break;
                }
                ///Apply the angle to pivot door
                pivot.localEulerAngles = currentAngle;
                yield return null;
            }

            if (!stopDoor)
            {
                state = open ? DoorState.Opened : DoorState.Closed;
                ///Close door if auto close and dont has a collider in trigger
                if (open && autoClose && !colliderInTrigger) CloseWithDelay();

                switch (state)//Call finish event based in state
                {
                    case DoorState.Opened:
                        onOpen.Invoke();
                        if (invertOpenSide)
                            onOpenLeft.Invoke();
                        else
                            onOpenRight.Invoke();
                        break;
                    case DoorState.Closed: onClose.Invoke(); break;
                }
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (tagsToOpen.Contains(other.tag))
            {
                if (autoOpen && (state == DoorState.Closing || state == DoorState.Closed))
                {
                    Vector3 relativePos = transform.InverseTransformPoint(other.transform.position);
                    if (relativePos.z > 0) _invertOpenSide = false;
                    else _invertOpenSide = true;
                    angle = Mathf.Abs(Vector3.Angle(_invertOpenSide ? transform.forward : -transform.forward, other.transform.forward));
                    if (angle < minAngleToOpen)
                    {
                        if (!colliderInTrigger) colliderInTrigger = other;
                        Open();
                    }
                }
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (autoClose && tagsToOpen.Contains(other.tag) &&
                (colliderInTrigger != null && colliderInTrigger.gameObject.Equals(other.gameObject) || colliderInTrigger == null))
            {
                colliderInTrigger = null;
                if (!closeOnlyWhenOpened || state == DoorState.Opened)
                {
                    CloseWithDelay();
                }
            }
        }

        protected virtual bool invertOpenSide
        {
            get
            {
                return _invertOpenSide && openBothSide;
            }
        }
        /// <summary>
        /// Close Door using <see cref="timeToClose"/> delay
        /// </summary>
        protected virtual void CloseWithDelay()
        {
            CancelInvoke("Close");
            Invoke("Close", timeToClose);
        }
    }
}