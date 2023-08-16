using UnityEngine;
namespace Invector.Throw
{
    public class vThrowAnimatorEvent : StateMachineBehaviour
    {
        public enum ThrowEventType
        {
            EquipThrowable, EnableAiming, CancelAiming, StartLaunch, FinishLaunch
        }

        public ThrowEventType eventType;
        [Range(0, 1f)]
        public float time;
        bool isTrigger;
        vThrowManagerBase manager;
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            manager = animator.GetComponentInChildren<vThrowManagerBase>();
            isTrigger = false;
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.normalizedTime >= time && !isTrigger)
            {
                OnTrigger();
            }
        }

        protected virtual void OnTrigger()
        {
            isTrigger = true;
            manager.TriggerAnimatorEvent(eventType);
        }
    }
}
