using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace Invector.vCharacterController.vActions
{
    [vClassHeader("Generic Animation", "Use this script to trigger a simple animation.")]
    public class vGenericAnimation : vMonoBehaviour
    {
        #region Variables

        [Tooltip("Input to trigger the custom animation")]
        public GenericInput actionInput = new GenericInput("L", "A", "A");
        [Tooltip("Name of the animation clip")]
        public string animationClip;
        [Tooltip("Where in the end of the animation will trigger the event OnEndAnimation")]
        public float animationEnd = 0.8f;

        public UnityEvent OnPlayAnimation;
        public UnityEvent OnEndAnimation;

        protected bool isPlaying;
        protected bool triggerOnce;
        protected vThirdPersonInput tpInput;
        
        #endregion

        protected virtual void Start()
        {
            tpInput = GetComponent<vThirdPersonInput>();
        }

        protected virtual void LateUpdate()
        {
            TriggerAnimation();
            AnimationBehaviour();            
        }

        protected virtual void TriggerAnimation()
        {
            // condition to trigger the animation
            bool playConditions = !isPlaying && !tpInput.cc.customAction && !string.IsNullOrEmpty(animationClip);

            if (actionInput.GetButtonDown() && playConditions)                           
                PlayAnimation();            
        }

        public virtual void PlayAnimation()
        {
            // we use a bool to trigger the event just once at the end of the animation
            triggerOnce = true;
            // trigger the OnPlay Event
            OnPlayAnimation.Invoke();
            // trigger the animationClip
            tpInput.cc.animator.CrossFadeInFixedTime(animationClip, 0.1f);
        }

        protected virtual void AnimationBehaviour()
        {
            // know if the animation is playing or not
            isPlaying = tpInput.cc.baseLayerInfo.IsName(animationClip);

            if (isPlaying)
            {
                // detected the end of the animation clip to trigger the OnEndAnimation Event
                if (tpInput.cc.animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= animationEnd)
                {
                    if(triggerOnce)
                    {
                        triggerOnce = false;        // reset the bool so we can call the event again
                        OnEndAnimation.Invoke();    // call the OnEnd Event at the end of the animation
                    }                    
                }
            }
        }
    }
}