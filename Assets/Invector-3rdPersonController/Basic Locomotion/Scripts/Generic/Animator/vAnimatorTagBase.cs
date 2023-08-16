using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vEventSystems
{
    public abstract class vAnimatorTagBase : StateMachineBehaviour
    {
        public delegate void OnStateTrigger(List<string> tags);
        public List<vAnimatorStateInfos> stateInfos = new List<vAnimatorStateInfos>();
        public event OnStateTrigger onStateEnter;
        public event OnStateTrigger onStateExit;
        public virtual void AddStateInfoListener(vAnimatorStateInfos stateInfo)
        {
            if (!stateInfos.Contains(stateInfo))
            {
                stateInfos.Add(stateInfo);
            }
        }
        public virtual void RemoveStateInfoListener(vAnimatorStateInfos stateInfo)
        {
            if (stateInfos.Contains(stateInfo))
            {
                stateInfos.Remove(stateInfo);
            }
        }
        protected virtual void OnStateEnterEvent(List<string> tags)
        {
            if (onStateEnter != null)
                onStateEnter(tags);
        }
        protected virtual void OnStateExitEvent(List<string> tags)
        {
            if (onStateEnter != null)
                onStateExit(tags);
        }


    }
}