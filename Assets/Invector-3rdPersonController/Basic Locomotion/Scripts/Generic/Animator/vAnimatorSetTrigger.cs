using UnityEngine;

public class vAnimatorSetTrigger : StateMachineBehaviour
{
    public bool setOnEnter, setOnExit;
    public string trigger;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (setOnEnter)
            animator.SetTrigger(trigger);
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (setOnExit)
            animator.SetTrigger(trigger);
    }
}
