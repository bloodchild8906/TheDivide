using UnityEngine;

namespace Invector.vMelee
{
    public class vRandomAttackBehaviour : StateMachineBehaviour
    {
        public int attackCount;

        //OnStateMachineEnter is called when entering a statemachine via its Entry Node
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            animator.SetInteger("RandomAttack", Random.Range(0, attackCount));
        }
    }
}