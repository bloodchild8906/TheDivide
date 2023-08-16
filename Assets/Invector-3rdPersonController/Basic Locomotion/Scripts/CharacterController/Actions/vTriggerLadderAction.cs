using UnityEngine;
using System.Collections;
using UnityEngine.Events;
namespace Invector.vCharacterController.vActions
{
    [vClassHeader("Trigger Ladder Action", false)]
    public class vTriggerLadderAction : vMonoBehaviour
    {
        [vEditorToolbar("Settings")]
        [Header("Trigger Action Options")]
        [Tooltip("Automatically execute the action without the need to press a Button")]
        public bool autoAction;

        [Header("Enter")]
        [Tooltip("Trigger an Animation - Use the exactly same name of the AnimationState you want to trigger")]
        public string playAnimation;
      
        [Header("Exit")]
        [Tooltip("Trigger an Animation - Use the exactly same name of the AnimationState you want to trigger")]
        public string exitAnimation;

        [Tooltip("Use this to limit the trigger to active if forward of character is close to this forward")]
        public bool activeFromForward;      
        [Tooltip("Rotate Character for this rotation when active")]
        public bool useTriggerRotation;


        [Tooltip("Target Character parent, used to movable ladders to set character child of target, keep empty if ladder is static")]
        public Transform targetCharacterParent;
        [vEditorToolbar("MatchTarget")]
        [Tooltip("Use a transform to help the character climb any height, take a look at the Example Scene ClimbUp, StepUp, JumpOver objects.")]
        public Transform matchTarget;
        [Tooltip("Use a empty gameObject as a reference for the character to exit")]
        public Transform exitMatchTarget;
        public AnimationCurve enterPositionXZCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve enterPositionYCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve exitPositionXZCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve exitPositionYCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve enterRotationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public AnimationCurve exitRotationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));
        public UnityEvent OnDoAction;
        public UnityEvent OnPlayerEnter;
        public UnityEvent OnPlayerStay;
        public UnityEvent OnPlayerExit;
    }
}