using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Invector
{
    public class vTriggerSoundByState : StateMachineBehaviour
    {
        public GameObject audioSource;
        public List<AudioClip> sounds;
        public float triggerTime;
        private vFisherYatesRandom _random;
        private bool isTrigger;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            isTrigger = false;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (stateInfo.normalizedTime % 1 >= triggerTime && !isTrigger)
            {
                TriggerSound(animator, stateInfo, layerIndex);
            }
            else if (stateInfo.normalizedTime % 1 < triggerTime && isTrigger) isTrigger = false;
        }

        void TriggerSound(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (_random == null)
                _random = new vFisherYatesRandom();
            isTrigger = true;
            GameObject audioObject = null;
            if (audioSource != null)
                audioObject = Instantiate(audioSource.gameObject, animator.transform.position, Quaternion.identity) as GameObject;
            else
            {
                audioObject = new GameObject("audioObject");
                audioObject.transform.position = animator.transform.position;
            }
            if (audioObject != null)
            {
                var source = audioObject.gameObject.GetComponent<AudioSource>();
                var clip = sounds[_random.Next(sounds.Count)];
                source.PlayOneShot(clip);
            }
        }      
    }
}