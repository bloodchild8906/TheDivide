namespace Invector
{
    [vClassHeader("Effect Receiver", "Use with the EffectSender component to trigger a Effect.")]
    public class vEffectReceiver : vMonoBehaviour
    {
        [System.Serializable]
        public class vEffectByName
        {
            public string effectName;
            public vEffectEvent onReceiveEffect;
        }
        [System.Serializable]
        public class vEffectEvent : UnityEngine.Events.UnityEvent<vIEffect> { }

        public vEffectEvent onReceiveEffect;

        public vEffectByName[] effectsByName;

        public virtual void OnReceiveEffect<T>(T effect) where T : vIEffect
        {
            onReceiveEffect.Invoke(effect);
            for (int i = 0; i < effectsByName.Length; i++)
            {
                if (effectsByName[i].effectName.Equals(effect.EffectName)) effectsByName[i].onReceiveEffect.Invoke(effect);
            }
        }
    }
}