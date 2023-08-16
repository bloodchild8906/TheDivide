using UnityEngine;

namespace Invector
{
    [vClassHeader("Effect Sender", "Use this component with the vEffectReceiver added on your Player Root to trigger Effects", openClose = false)]
    public class vEffectSender : vMonoBehaviour
    {
        [vHelpBox("Make sure you check where the vEffectReceiver is on your player, root object, parented or inside as children. By default is located inside the Invector Components > vEffects")]

        public vEffectStruct[] effects;
        [System.Serializable]
        public class vEffectStruct : vIEffect
        {
            public enum GetReceiverMethod
            {
                InTarget, InParent, InChildren
            }
            public GetReceiverMethod getReceiverMethod = GetReceiverMethod.InChildren;
            [SerializeField] protected string effectName;
            [SerializeField] protected float effectDuration;
            public string EffectName => effectName;

            public float EffectDuration => effectDuration;

            public Vector3 EffectPosition { get; set; }

            public Transform Sender { get; set; }
        }

        public Transform overrideEffectSender;
        public virtual void SetOverrideEffectSender(Transform t)
        {
            overrideEffectSender = t;
        }
        public void Send(GameObject target)
        {
            Send(target.transform);
        }
        public void Send(Transform target)
        {
            for (int i = 0; i < effects.Length; i++)
            {
                var effect = effects[i];
                effect.Sender = overrideEffectSender ? overrideEffectSender : transform;
                effect.EffectPosition = transform.position;
                vEffectReceiver receiver = null;
                switch (effect.getReceiverMethod)
                {
                    case vEffectStruct.GetReceiverMethod.InTarget:
                        if (target.TryGetComponent(out receiver)) receiver.OnReceiveEffect(effect);
                        break;
                    case vEffectStruct.GetReceiverMethod.InParent:
                        receiver = target.GetComponentInParent<vEffectReceiver>();
                        if (receiver) receiver.OnReceiveEffect(effect);
                        break;
                    case vEffectStruct.GetReceiverMethod.InChildren:
                        receiver = target.GetComponentInChildren<vEffectReceiver>();
                        if (receiver) receiver.OnReceiveEffect(effect);
                        break;
                }
            }
        }
        public void Send(Collider target)
        {
            Send(target.transform);
        }

    }
}