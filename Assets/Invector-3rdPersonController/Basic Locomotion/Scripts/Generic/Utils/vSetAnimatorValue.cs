using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.Utils
{
    [vClassHeader("Set Animator Value",useHelpBox =true,helpBoxText ="Use this component to set animator value using events")]
    public class vSetAnimatorValue : vMonoBehaviour
    {
        [vSelectableString("Set Float exemple", "ParamenterName,Float,1.0")]
        [vSelectableString("Set Integer exemple", "ParamenterName,Int,1")]
        [vSelectableString("Set Bool exemple", "ParamenterName,Bool,true")]
        [vSelectableString("Set Trigger exemple", "ParamenterName,Trigger,Set")]
        [vSelectableString("Reset Trigger exemple", "ParamenterName,Trigger,Reset")]
        [vHelpBox("SetValueByExpression examples",vHelpBoxAttribute.MessageType.Info)]
        [Tooltip("Target Animator to set value")]
        public Animator targetAnimator;
        [Tooltip("Target paramenter for normal methods\nDon't will be used when using SetValueByExpression method")]
        public string targetParamenter;
        [SerializeField,vReadOnly]
        protected int paramenterHash;

        enum ValueType
        {
            Float,Int,Bool,Trigger
        }

        public void SetTargetAnimator(Animator animator)
        {
            targetAnimator = animator;
        }
        public void SetTrigger()
        {
            targetAnimator.SetTrigger(paramenterHash);
        }
        public void ResetTrigger()
        {
            targetAnimator.ResetTrigger(paramenterHash);
        }
        public void SetBoolean(bool value)
        {
            targetAnimator.SetBool(paramenterHash, value);
        }
        public void SetInteger(int value)
        {
            targetAnimator.SetInteger(paramenterHash, value);
        }
        public void SetFloat(float value)
        {
            targetAnimator.SetFloat(paramenterHash, value);
        }
        public void SetTargetParamenter(string targetParamenter)
        {
            this.targetParamenter = targetParamenter;

        }
        public void SetValueByExpression(string expression)
        {
            string[] splited = expression.Split(',');
            if (splited.Length < 3)
            {
                Debug.LogWarning($"Expression :<color=green>{expression}</color> does't match any valid expression");
                return;
            }
            string parameterName = splited[0];
            
            if (System.Enum.TryParse<ValueType>(splited[1],out ValueType valueType))
            {              
                var value = splited[2];
                bool setFail = false;
                switch(valueType)
                {
                    case ValueType.Bool:
                        if(System.Boolean.TryParse(value, out bool boolValue))
                        {
                            targetAnimator.SetBool(parameterName, boolValue);
                        }else
                        {
                            setFail = true;
                        }
                        break;
                    case ValueType.Int:
                        if(System.Int32.TryParse(value, out int intValue))
                        {
                            targetAnimator.SetInteger(targetParamenter, intValue);
                        }
                        else
                        {
                            setFail = true;
                        }
                        break;
                    case ValueType.Float:
                        if(System.Double.TryParse(value,out double doubleValue))
                        {
                            targetAnimator.SetFloat(parameterName, (float)doubleValue);
                        }
                        else
                        {
                            setFail = true;
                        }
                        break;
                    case ValueType.Trigger:
                        if(value=="Set")
                        {
                            targetAnimator.SetTrigger(parameterName);
                        }
                        else if(value=="Reset")
                        {
                            targetAnimator.ResetTrigger(parameterName);
                        }
                        else
                        {
                            setFail = true;
                        }
                        break;
                }
                if(setFail)
                {
                    Debug.LogWarning($"Expression :<color=green>{value}</color> does't match any valid value");
                }
            }          
            else
            {
                Debug.LogWarning($"Expression :<color=green>{splited[0]}</color> does't match any animator value");
            }
        }
    }
}