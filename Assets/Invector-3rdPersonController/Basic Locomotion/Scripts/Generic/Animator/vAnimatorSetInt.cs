namespace Invector
{
    public class vAnimatorSetInt : vAnimatorSetValue<int>
    {
        vFisherYatesRandom random = new vFisherYatesRandom();
        [vHelpBox("Random Value between Default Value and Max Value")]
        public bool randomEnter;
        [vHideInInspector("randomEnter")]
        public int maxEnterValue;
        public bool randomExit;
        [vHideInInspector("randomExit")]
        public int maxExitValue;

        protected override int GetEnterValue()
        {
            return randomEnter ? random.Range(base.GetEnterValue(), maxEnterValue) : base.GetEnterValue();
        }
        protected override int GetExitValue()
        {
            return randomExit ? random.Range(base.GetExitValue(), maxExitValue) : base.GetExitValue();
        }
    }
}