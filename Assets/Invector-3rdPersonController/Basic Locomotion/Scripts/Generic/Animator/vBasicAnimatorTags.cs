using UnityEngine;
namespace Invector
{
    public static partial class vAnimatorTags
    {
        [Tooltip("Use to lock the controller movement to use the root movement instead")]
        public const string LockMovement = "LockMovement";
        [Tooltip("Use to lock the controller rotation to use the root rotation instead")]
        public const string LockRotation = "LockRotation";
        [Tooltip("Use for Generic Actions like push lever, it will lock the players input, movement and rotation and use the animation root motion")]
        public const string CustomAction = "CustomAction";
        [Tooltip("Use to identify if this is a Airborne animation")]
        public const string Airborne = "Airborne";
        [Tooltip("Use to Ignore the Headtrack")]
        public const string IgnoreHeadtrack = "IgnoreHeadtrack";
        [Tooltip("Use to identify a Death animation")]
        public const string Dead = "Dead";

    }
}
