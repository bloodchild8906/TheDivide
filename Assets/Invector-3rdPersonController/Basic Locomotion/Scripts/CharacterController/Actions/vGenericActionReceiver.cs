using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
namespace Invector.vCharacterController.vActions
{
    /// <summary>
    /// Generic Action receiver for <seealso cref="vGenericAction"/> events. 
    /// Use this component inside character with <see cref="vGenericAction"/> component.
    /// This is usefull for trigger events based in the <seealso cref="vTriggerGenericAction.actionName"/>.
    /// </summary>
    [vClassHeader("Action Receiver")]
    public class vGenericActionReceiver : vMonoBehaviour
    {
        public List<string> supportedActionNames = new List<string>() { "Action" };
        public UnityEngine.Events.UnityEvent onEnterTriggerAction;
        public UnityEngine.Events.UnityEvent onExitTriggerAction;
        public UnityEngine.Events.UnityEvent onStartAction;
        public UnityEngine.Events.UnityEvent onCancelAction;
        public UnityEngine.Events.UnityEvent onEndAction; 
        
        private void Start()
        {
            vGenericAction genericAction = gameObject.GetComponentInParent<vGenericAction>();
            if(genericAction)
            {
                genericAction.OnEnterTriggerAction.AddListener(OnEnterTriggerAction);
                genericAction.OnExitTriggerAction.AddListener(OnExitTriggerAction);
                genericAction.OnStartAction.AddListener(OnStartAction);
                genericAction.OnCancelAction.AddListener(OnCancelAction);
                genericAction.OnEndAction.AddListener(OnEndAction);
            }
        }
        private void OnDestroy()
        {
            vGenericAction genericAction = GetComponentInParent<vGenericAction>();
            if (genericAction)
            {
                genericAction.OnEnterTriggerAction.RemoveListener(OnEnterTriggerAction);
                genericAction.OnExitTriggerAction.RemoveListener(OnExitTriggerAction);
                genericAction.OnStartAction.RemoveListener(OnStartAction);
                genericAction.OnCancelAction.RemoveListener(OnCancelAction);
                genericAction.OnEndAction.RemoveListener(OnEndAction);
            }
        }

        protected virtual bool IsValidAction(vTriggerGenericAction actionInfo)
        {
            bool isValid = this.enabled && this.gameObject.activeInHierarchy && actionInfo != null && supportedActionNames.Contains(actionInfo.actionName);           
            return isValid;
        }

        /// <summary>
        /// Event called when Enter in trigger
        /// </summary>
        /// <param name="actionInfo"></param>
        public virtual void OnEnterTriggerAction(vTriggerGenericAction actionInfo)
        {
            if (IsValidAction(actionInfo))
            {
                onEnterTriggerAction.Invoke();
            }
        }
        /// <summary>
        /// Event Called when exit Trigger
        /// </summary>
        /// <param name="actionInfo"></param>
        public virtual void OnExitTriggerAction(vTriggerGenericAction actionInfo)
        {
            if (IsValidAction(actionInfo))
            {
                onExitTriggerAction.Invoke();
            }
        }
        /// <summary>
        /// Event called when action is started
        /// </summary>
        /// <param name="actionInfo"></param>
        public virtual void OnStartAction(vTriggerGenericAction actionInfo)
        {          
            if (IsValidAction(actionInfo))
            {               
                onStartAction.Invoke();
            }
        }
        /// <summary>
        /// Event called when action is canceled
        /// </summary>
        /// <param name="actionInfo"></param>
        public virtual void OnCancelAction(vTriggerGenericAction actionInfo)
        {
            if (IsValidAction(actionInfo))
            {
                onCancelAction.Invoke();
            }
        }
        /// <summary>
        /// Event called when action is finished or canceled
        /// </summary>
        /// <param name="actionInfo"></param>
        public virtual void OnEndAction(vTriggerGenericAction actionInfo)
        {           
            if (IsValidAction(actionInfo))
            {          
                onEndAction.Invoke();
            }
        }
    }
}