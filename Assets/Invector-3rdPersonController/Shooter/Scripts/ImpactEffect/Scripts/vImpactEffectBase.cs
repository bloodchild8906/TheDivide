using UnityEngine;
namespace Invector.vShooter
{
    /// <summary>
    /// Base Class used to create Impact Effect
    /// </summary>
    public abstract class vImpactEffectBase : ScriptableObject
    {
        /// <summary>
        /// Do Impact effect
        /// </summary>
        /// <param name="position">position of impact effect</param>
        /// <param name="rotation">rotation of impact effect</param>
        /// <param name="sender">Impact effect sender</param>
        /// <param name="receiver">Impact effect receiver</param>
        public abstract void DoImpactEffect(Vector3 position, Quaternion rotation, GameObject sender, GameObject receiver);
    }
}