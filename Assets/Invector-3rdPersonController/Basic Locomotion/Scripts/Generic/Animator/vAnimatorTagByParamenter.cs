using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vEventSystems
{
    public class vAnimatorTagByParamenter : vAnimatorTag
    {
        public enum ParamenterType
        {
            Bool, Float, Int
        }

        public enum NumberCompare
        {
            Equals, Greater, Less
        }

        public string paramenterName;
        public ParamenterType paramenterType;
        [vCheckProperty("paramenterType", ParamenterType.Bool, hideInInspector = true)]
        public bool boolValue;
        [vCheckProperty("paramenterType", ParamenterType.Float, hideInInspector = true)]
        public float floatValue;
        [vCheckProperty("paramenterType", ParamenterType.Int, hideInInspector = true)]
        public int intValue;     
        [vCheckProperty("paramenterType", ParamenterType.Bool, hideInInspector = true,invertResult =true)]
        public NumberCompare compare;
        [vReadOnly] public bool tagAdded;
        vAnimatorParameter paramenter;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (paramenter == null) paramenter = new vAnimatorParameter(animator, paramenterName);
            ///don't do anything
        }
        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);           
            CheckForParamenter(animator,layerIndex);
        }
       
        private void CheckForParamenter(Animator animator,int layerIndex)
        {
            if (paramenter.isValid)
            {
                bool isValid = false;
                switch (paramenterType)
                {
                    case ParamenterType.Float:

                        isValid = CompareNumber(floatValue, animator.GetFloat(paramenter), compare);

                        break;
                    case ParamenterType.Int:
                        isValid = CompareNumber(intValue, animator.GetInteger(paramenter), compare);
                        break;
                    case ParamenterType.Bool:
                        isValid =   boolValue == animator.GetBool(paramenter);
                        break;

                }
              
                if (isValid != tagAdded)
                {
                    tagAdded = isValid;
                    if (isValid)
                    {
                       
                        AddTags(layerIndex);
                    }
                    else
                    {
                      
                        RemoveTags(layerIndex);
                    }
                }
            }
        }

        void AddTags(int layerIndex)
        {
            if (stateInfos != null)
            {
               
                for (int i = 0; i < tags.Length; i++)
                {
                    for (int a = 0; a < stateInfos.Count; a++)
                    {
                        stateInfos[a].AddStateInfo(tags[i], layerIndex);
                    }
                }
            }
        }

        void RemoveTags(int layerIndex)
        {
            if (stateInfos != null)
            {
               
                for (int i = 0; i < tags.Length; i++)
                {
                    for (int a = 0; a < stateInfos.Count; a++)
                    {
                        stateInfos[a].RemoveStateInfo(tags[i], layerIndex);
                    }
                }
            }
        }

        bool CompareNumber(float a,float b,NumberCompare compare)
        {
            switch (compare)
            {
                case NumberCompare.Equals:
                    Debug.Log($"{b} == {a}");
                    return b == a;
                  
                case NumberCompare.Greater:
                    Debug.Log($"{b} > {a}");
                    return b > a;
                case NumberCompare.Less:
                    Debug.Log($"{b} < {a}");
                    return b < a;
            }
            return false;
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if(tagAdded)
            {
                tagAdded = false;
                base.OnStateExit(animator, stateInfo, layerIndex);
            }
           
        }

    }
}