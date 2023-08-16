using UnityEngine;

namespace Invector
{
    public partial interface vIEffect
    {
        string EffectName { get; }
        float EffectDuration { get; }
        Vector3 EffectPosition { get; }
        Transform Sender { get; }
    }
}