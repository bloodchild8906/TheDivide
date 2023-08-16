using UnityEngine;
namespace Invector.Throw
{
    [CreateAssetMenu(menuName = "Invector/Throw/New ThrowSettings")]
    public class vThrowSettings : ScriptableObject
    {
        [Tooltip("Align End point UP to surface normal direction")]
        public bool alignToSurfaceNormal = true;
        [Tooltip("End point UP always face to the view direction when don't has a surface")]
        public bool alignToViewWhenNotHit = true;
        public bool disableEndPointWhenNotHit = true;
        [Tooltip("End point alignment smooth")]
        public float alignmentSmooth = 20f;
        [Min(0.001f)]
        public float metersPerSeconds = 0.1f;
        [vMinMax(0.0001f, 10)]
        public Vector2 minMaxTime = new Vector2(0.1f, 1);
        public float maxDistance = 100;
        public float maxVelocity = 10;
        public float throwMaxForce = 15f;
        public float throwDelayTime = .25f;
        [Min(0.001f)]
        public float lineStepPerTime = .1f;
        public float maxLineLength = 100;
        public float exitThrowModeDelay = 0.5f;

        public string cameraStateStanding = "ThrowStanding";
        public string cameraStateCrouching = "ThrowCrouching";

        public string throwAnimation = "ThrowObject";
        public string holdingAnimation = "HoldingObject";
        public string cancelAnimation = "CancelThrow";
    }
}