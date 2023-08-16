using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vAnimatorIncreaseDecreaseValue : StateMachineBehaviour
{
    public string targetFloat;

    public bool decrease;
    private float time;
    public float speed=1;
    
    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!decrease) time += Time.deltaTime*speed;
        else time -= Time.deltaTime * speed;

        animator.SetFloat(targetFloat, time);
    }
   
    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
