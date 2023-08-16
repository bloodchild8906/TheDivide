using System.Collections.Generic;
using UnityEngine;

namespace Invector.vEventSystems
{
    public class vAnimatorTagAdvanced : vAnimatorTagBase
    {
        public enum vAnimatorEventTriggerType
        {
            AllByNormalizedTime, EnterStateExitByNormalized, EnterByNormalizedExitState, EnterStateExitState
        }
        [System.Serializable]

        public class vAdvancedTags
        {
            public string tagName;
            public vAnimatorEventTriggerType tagType = vAnimatorEventTriggerType.AllByNormalizedTime;

            public Vector2 normalizedTime = new Vector2(0.1f, 0.8f);
            private int loopCount;
            public vAdvancedTags(string tag)
            {
                this.tagName = tag;
                this.tagType = vAnimatorEventTriggerType.AllByNormalizedTime;
            }

            bool isEnter;
            bool isExit;
            public void UpdateEventTrigger(float normalizedTime, List<vAnimatorStateInfos> stateInfos, int layer, float speed = 1,bool looping= false, bool inExit = false, bool debug = false)
            {
                var normalizedTimeClamped = normalizedTime % 1;
                if (!isEnter && !inExit && tagType != vAnimatorEventTriggerType.EnterStateExitByNormalized &&
                                     tagType != vAnimatorEventTriggerType.EnterStateExitState && normalizedTimeClamped >= this.normalizedTime.x )
                {
                    if (debug) Debug.Log("ADD TAG " + tagName + " in  " + normalizedTime);

                    AddTag(stateInfos, layer);
                }
              
                if (!isExit && isEnter && tagType != vAnimatorEventTriggerType.EnterByNormalizedExitState &&
                                               tagType != vAnimatorEventTriggerType.EnterStateExitState && (normalizedTimeClamped >= this.normalizedTime.y || inExit))
                {
                    RemoveTag(stateInfos, layer);
                    if (debug) Debug.Log("REMOVE TAG " + tagName + " in  " + normalizedTime);
                }
              
                if (looping && normalizedTime > loopCount + 1)
                {
                    isEnter = false;
                    isExit = false;
                    loopCount++;
                }
            }

            public void AddTag(List<vAnimatorStateInfos> stateInfos, int layer)
            {
                for (int i = 0; i < stateInfos.Count; i++)
                    stateInfos[i].AddStateInfo(tagName, layer);

                isEnter = true;
            }

            public void RemoveTag(List<vAnimatorStateInfos> stateInfos, int layer)
            {
                for (int i = 0; i < stateInfos.Count; i++)
                {
                    stateInfos[i].RemoveStateInfo(tagName, layer); isExit = true;
                }
            }

            public void Init(List<vAnimatorStateInfos> stateInfos, int layer)
            {
                if(isEnter && !isExit)
                {
                    RemoveTag(stateInfos, layer);
                }
                isEnter = false;
                isExit = false;
                loopCount = 0;
            }
        }
        public bool debug;
        public List<vAdvancedTags> tags = new List<vAdvancedTags>() { new vAdvancedTags("CustomAction") };

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
           
            if (stateInfos != null)
            {
                for (int i = 0; i < tags.Count; i++)
                {
                    tags[i].Init(stateInfos,layerIndex);
                    if (debug) Debug.Log("Init " + tags[i].tagName + " OnStateEnter  ");
                    if (tags[i].tagType == vAnimatorEventTriggerType.EnterStateExitState || tags[i].tagType == vAnimatorEventTriggerType.EnterStateExitByNormalized)
                    {
                        if (debug) Debug.Log("ADD TAG " + tags[i].tagName + " OnStateEnter  ");
                        tags[i].AddTag(stateInfos, layerIndex);
                    }
                    else
                        tags[i].UpdateEventTrigger(stateInfo.normalizedTime, stateInfos, layerIndex, animator.speed,stateInfo.loop, false, debug);
                }
               
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfos != null)
            {
                for (int i = 0; i < tags.Count; i++)
                {
                    if (tags[i].tagType != vAnimatorEventTriggerType.EnterStateExitState)
                        tags[i].UpdateEventTrigger(stateInfo.normalizedTime, stateInfos, layerIndex, animator.speed, stateInfo.loop, false, debug);
                }
               
                for (int a = 0; a < stateInfos.Count; a++)
                {
                    stateInfos[a].UpdateStateInfo(layerIndex, stateInfo.normalizedTime, stateInfo.shortNameHash);
                }
            }
            base.OnStateUpdate(animator, stateInfo, layerIndex);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfos != null)
            {
                for (int i = 0; i < tags.Count; i++)
                {
                    if (tags[i].tagType == vAnimatorEventTriggerType.EnterStateExitState || tags[i].tagType == vAnimatorEventTriggerType.EnterByNormalizedExitState)
                    {
                        if (debug) Debug.Log("REMOVE TAG " + tags[i].tagName + " OnStateExit  ");
                        tags[i].RemoveTag(stateInfos, layerIndex);
                    }
                    else
                    {                        
                        tags[i].UpdateEventTrigger(stateInfo.normalizedTime, stateInfos, layerIndex, animator.speed, stateInfo.loop, true, debug);
                    }
                }
            }
            base.OnStateExit(animator, stateInfo, layerIndex);
        }
    }
}