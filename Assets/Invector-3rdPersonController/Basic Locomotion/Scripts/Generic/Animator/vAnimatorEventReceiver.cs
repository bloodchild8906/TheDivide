using Invector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vEventSystems
{
    [vClassHeader("Animator Event Receiver")]
    public class vAnimatorEventReceiver : vMonoBehaviour
    {
        [Tooltip("Check this option if the Animator component is on the parent of this GameObject")]
        public bool getAnimatorInParent;
        [vHelpBox("Use <b>vAnimatorEvent</b> on a AnimatorState to trigger a Event below", vHelpBoxAttribute.MessageType.Info)]
        public List<vAnimatorEvent> animatorEvents;

        [System.Serializable]
        public class vAnimatorEvent
        {
            [System.Serializable]
            public class StateEvent : UnityEngine.Events.UnityEvent<string> { }
            public string eventName;
            public bool debug;
            public StateEvent onTriggerEvent;

            public virtual void OnTriggerEvent(string eventName)
            {
                if (debug)
                {
                    Debug.Log("<color=green><b>Event " + eventName + " was called</b></color>");
                }

                onTriggerEvent.Invoke(eventName);
            }
        }

        private bool hasValidBehaviours;
        private bool hasAnimator;

        private void Start()
        {
            RegisterEvents();
        }

        private void OnDisable()
        {            
            RemoveEvents();            
        }

        public void OnEnable()
        {
            if (hasAnimator && hasValidBehaviours)
            {
                RemoveEvents();
                RegisterEvents();
            }
        }

        private void OnDestroy()
        {
            RemoveEvents();
        }

        public virtual void RegisterEvents()
        {
            if (animatorEvents.Count > 0)
            {
                var animator = getAnimatorInParent ? GetComponentInParent<Animator>() : GetComponent<Animator>();
                if(getAnimatorInParent && animator.gameObject == gameObject && transform.parent)
                {
                    animator = transform.parent.GetComponentInParent<Animator>();
                }
                if (animator)
                {
                    hasAnimator = true;
                    var behaviours = animator.GetBehaviours<Invector.vEventSystems.vAnimatorEvent>();
                    for (int a = 0; a < animatorEvents.Count; a++)
                    {
                        var hasEvent = false;
                        for (int i = 0; i < behaviours.Length; i++)
                        {
                            if (behaviours[i].HasEvent(animatorEvents[a].eventName))
                            {
                                behaviours[i].RegisterEvents(animatorEvents[a].eventName, animatorEvents[a].OnTriggerEvent);
                                if (animatorEvents[a].debug)
                                {
                                    Debug.Log("<color=green>" + gameObject.name + " Register event : " + animatorEvents[a].eventName + "</color> in the " + animator.gameObject.name, gameObject);
                                }

                                hasValidBehaviours = true;
                                hasEvent = true;
                            }
                        }
                        if (!hasEvent && animatorEvents[a].debug)
                        {
                            Debug.LogWarning(animator.gameObject.name + " Animator doesn't have Event with name: " + animatorEvents[a].eventName, gameObject);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Can't Find Animator to register Events in " + gameObject.name + (getAnimatorInParent ? " Parent" : ""), gameObject);
                }
            }
        }

        public virtual void RemoveEvents()
        {
            if (!hasAnimator || !hasValidBehaviours)
            {
                return;
            }

            if (animatorEvents.Count > 0)
            {
                var animator = getAnimatorInParent ? GetComponentInParent<Animator>() : GetComponent<Animator>();
                if (animator)
                {
                    var behaviours = animator.GetBehaviours<Invector.vEventSystems.vAnimatorEvent>();
                    for (int a = 0; a < animatorEvents.Count; a++)
                    {
                        for (int i = 0; i < behaviours.Length; i++)
                        {
                            if (behaviours[i].HasEvent(animatorEvents[a].eventName))
                            {
                                behaviours[i].RemoveEvents(animatorEvents[a].eventName, animatorEvents[a].OnTriggerEvent);
                                if (animatorEvents[a].debug)
                                {
                                    Debug.Log("<color=red>" + gameObject.name + " Remove event : " + animatorEvents[a].eventName + "</color> Of the " + animator.gameObject.name, gameObject);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}