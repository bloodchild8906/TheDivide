namespace Invector.Throw
{
    public abstract partial class vThrowVisualControlBase : vMonoBehaviour
    {
        protected vThrowManagerBase tm;
        protected virtual void Awake()
        {
            tm = GetComponentInParent<vThrowManagerBase>();
            if (tm)
            {
                OnInit(tm);
                tm.onChangeVisualSettings.AddListener(OnChangeVisual);
            }
        }
        protected abstract void OnInit(vThrowManagerBase manager);
        public abstract void OnChangeVisual(vThrowVisualSettings settings);
    }
}
