using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vShooter
{
    public class vBotRifleAnimationControl : MonoBehaviour
    {
        public Animator animator;
        public float pulseSpeed;
        void Start()
        {
            animator = GetComponent<Animator>();
            animator.SetFloat("PulseSpeed", pulseSpeed);
        }
        public void OnChangePowerChanger(float value)
        {
            animator.SetFloat("PowerCharger", value);
        }
    }
}