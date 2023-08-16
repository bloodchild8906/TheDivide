namespace Invector
{
    public class vAnimatorSetFloat : vAnimatorSetValue<float>
    {        
        [vHelpBox("Random Value between Default Value and Max Value")]
        public bool randomEnter;
        [vHideInInspector("randomEnter")]
        public float maxEnterValue;       
        public bool randomExit;
        [vHideInInspector("randomExit")]
        public float maxExitValue;
        [vHelpBox("Use this in <b>Random mode</b> to generat a rounded value")]
        public bool roundValue;
        [UnityEngine.Tooltip("Digits after the comma")]
        [vHideInInspector("roundValue")]
        public int roundDigits =1;
        [UnityEngine.Tooltip("Invert number randomly")]
        public bool randomInvert;
        protected override float GetEnterValue()
        {
            var val = 0f;
            if(randomEnter)
            {
                val = UnityEngine.Random.Range(base.GetEnterValue(), maxEnterValue);
                if (roundValue) val =(float) System.Math.Round(val, roundDigits);
                
            }
            else val = base.GetEnterValue();
            if (randomInvert)
            {
                if (UnityEngine.Random.Range(0, 100) > 50) val *= -1;
            }
            return val;
        }
        protected override float GetExitValue()
        {
            var val = 0f;
            if (randomEnter)
            {
                val = UnityEngine.Random.Range(base.GetExitValue(), maxEnterValue);
                if (roundValue) val = (float)System.Math.Round(val, roundDigits);
            }
            else val = base.GetExitValue();
            if (randomInvert)
            {
                if (UnityEngine.Random.Range(0, 100) > 50) val *= -1;
            }
            return val;
        }
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateMove is called right after Animator.OnAnimatorMove(). Code that processes and affects root motion should be implemented here
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK(). Code that sets up animation IK (inverse kinematics) should be implemented here.
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
        //
        //}
    }
}