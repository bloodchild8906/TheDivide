using UnityEngine;

namespace Invector.vCharacterController
{
    [System.Serializable]
    public class OnActiveRagdoll : UnityEngine.Events.UnityEvent<vDamage> { }
    public interface vICharacter : vIHealthController
    {
        OnActiveRagdoll onActiveRagdoll { get; }
        Animator animator { get; }
        bool isCrouching { get; }
        bool ragdolled { get; set; }
        void EnableRagdoll();
        void ResetRagdoll();
    }
}