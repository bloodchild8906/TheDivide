using UnityEngine;

namespace Invector.IK
{
    using Invector;
    /// <summary>
    /// Class that  helps to Align arm to a position using a <seealso cref="aimReference"/> 
    /// </summary>
    public class vArmAimAlign
    {
        #region Bones
        protected Transform root;
        protected Transform upperArm;
        protected Transform foreArm;
        protected Transform hand;

        public bool smoothIKAlignmentPoint = true;
        #endregion

        #region Bone Helpers
        public Transform upperArmHelper { get; protected set; }
        public Transform foreArmHelper { get; protected set; }
        public Transform handHelper { get; protected set; }
        public Transform aimReferenceHelper { get; protected set; }
        #endregion

        #region public Variables
        /// <summary>
        /// Smooth the alignment
        /// </summary>
        public float smooth { get; set; }
        /// <summary>
        /// Max angle (x) of the alignment based on Aim Reference
        /// </summary>
        public float maxVerticalAligmentAngle { get; set; }
        /// <summary>
        /// Max angle (y) of the alignment based on Aim Reference
        /// </summary>
        public float maxHorizontalAligmentAngle { get; set; }
        /// <summary>
        /// Reference transform to align arm. if this transform be null, the arm don't will be aligned;
        /// </summary>
        public Transform aimReference { get; set; }
        #endregion

        #region Protected Variables
        protected Quaternion _upperArmRotation;
        protected Quaternion _foreArmRotation;
        protected Quaternion _handRotation;
        protected Quaternion _aimReferenceRotation;
        protected Quaternion _upperArmRotationSmooth;
        protected Quaternion _handRotationSmooth;
        protected Quaternion smoothRotationDirection;

        protected Vector3 _upperArmPosition;
        protected Vector3 _foreArmPosition;
        protected Vector3 _handPosition;
        protected Vector3 _aimReferencePosition;
        #endregion

        public vArmAimAlign(Transform upperArm, Transform foreArm, Transform hand)
        {
            this.upperArm = upperArm;
            this.foreArm = foreArm;
            this.hand = hand;

            if (!upperArm || !foreArm || !hand) return;

            root = upperArm.parent;
            if (!upperArmHelper)
            {
                upperArmHelper = new GameObject("UpperArmHelper").transform;
                upperArmHelper.gameObject.tag = "Ignore Ragdoll";
            }
            if (!foreArmHelper)
            {
                foreArmHelper = new GameObject("ForeArmHelper").transform;
                foreArmHelper.gameObject.tag = "Ignore Ragdoll";
            }
            if (!handHelper)
            {
                handHelper = new GameObject("HandHelper").transform;
                handHelper.gameObject.tag = "Ignore Ragdoll";
            }
            if (!aimReferenceHelper)
            {
                aimReferenceHelper = new GameObject("AimReferenceHelper").transform;
                aimReferenceHelper.gameObject.tag = "Ignore Ragdoll";
            }

            upperArmHelper.parent = root;
            foreArmHelper.parent = upperArmHelper;
            handHelper.parent = foreArmHelper;
            aimReferenceHelper.parent = handHelper;
        }

        /// <summary>
        /// Check if the Arm is valid (all necessary transforms is assigned)
        /// </summary>
        public bool IsValid => this.upperArm && this.foreArm && this.hand && this.aimReference;

        /// <summary>
        /// Update Arm to use default animation alignment
        /// </summary>
        /// <param name="aimReference"></param>
        public void UpdateDefaultAlignment()
        {
            if (!IsValid) return;
            upperArmHelper.SetPositionAndRotation(upperArm.position, upperArm.rotation);
            upperArmHelper.localScale = upperArm.localScale;
            foreArmHelper.SetPositionAndRotation(foreArm.position, foreArm.rotation);
            foreArmHelper.localScale = foreArm.localScale;
            handHelper.SetPositionAndRotation(hand.position, hand.rotation);
            handHelper.localScale = hand.localScale;
            aimReferenceHelper.SetPositionAndRotation(aimReference.position, aimReference.rotation);
            aimReferenceHelper.localScale = aimReference.localScale;

            _upperArmRotation = upperArmHelper.localRotation;
            _foreArmRotation = foreArmHelper.localRotation;
            _handRotation = handHelper.localRotation;
            _aimReferenceRotation = aimReferenceHelper.localRotation;

            _upperArmPosition = upperArmHelper.localPosition;
            _foreArmPosition = foreArmHelper.localPosition;
            _handPosition = handHelper.localPosition;
            _aimReferencePosition = aimReferenceHelper.localPosition;
        }

        /// <summary>
        /// Restore last alignment applied from <seealso cref="AlignToArmToPosition"/>
        /// </summary>
        /// <param name="aimReference"></param>
        public void RestoreToLastAlignment()
        {
            if (!IsValid) return;
            upperArmHelper.localRotation = _upperArmRotation;
            foreArmHelper.localRotation = _foreArmRotation;
            handHelper.localRotation = _handRotation;
            aimReferenceHelper.localRotation = _aimReferenceRotation;

            upperArmHelper.localPosition = _upperArmPosition;
            foreArmHelper.localPosition = _foreArmPosition;
            handHelper.localPosition = _handPosition;
            aimReferenceHelper.localPosition = _aimReferencePosition;
        }

        /// <summary>
        /// Align Arm to a position
        /// </summary>
        /// <param name="targetPosition">position to align Arm</param>
        /// <param name="weight">alignment weight (0~1)</param>
        /// <param name="alignUpperArm"></param>
        /// <param name="alignHand"></param>
        public void AlignToArmToPosition(Vector3 targetPosition, float weight, bool alignUpperArm = true, bool alignHand = true)
        {
            if (!IsValid) return;

            if (smoothIKAlignmentPoint)
            {
                targetPosition = SmootAndClampPosition(targetPosition, maxHorizontalAligmentAngle, maxVerticalAligmentAngle);
            }

            if (alignUpperArm) AlignUpperArm(targetPosition, alignHand ? weight * 0.5f : weight);
            if (alignHand) AlignHand(targetPosition, weight);
        }

        /// <summary>
        /// Align upper arm to a position
        /// </summary>
        /// <param name="targetPosition">position to align upper arm</param>
        /// <param name="weight">alignment weight (0~1)</param>
        protected virtual void AlignUpperArm(Vector3 targetPosition, float weight)
        {
            AlignArmBone(upperArm, upperArmHelper, ref _upperArmRotationSmooth, targetPosition, weight);
        }

        /// <summary>
        /// Align  hand to a position
        /// </summary>
        /// <param name="targetPosition">position to align hand</param>
        /// <param name="weight">alignment weight (0~1)</param>
        protected virtual void AlignHand(Vector3 targetPosition, float weight)
        {
            AlignArmBone(hand, handHelper, ref _handRotationSmooth, targetPosition, weight);
        }

        /// <summary>
        /// Align bone to a target position
        /// </summary>
        /// <param name="bone">target bone</param>
        /// <param name="boneHelper">target bone helper</param>
        /// <param name="smoothRotation">rotation of this bone (used to smooth)</param>
        /// <param name="targetPosition">position to align bone </param>
        /// <param name="weight">alignment weight (0~1)</param>
        protected virtual void AlignArmBone(Transform bone, Transform boneHelper, ref Quaternion smoothRotation, Vector3 targetPosition, float weight)
        {
            if (!smoothIKAlignmentPoint && System.Math.Round(weight, 1) == 0)
            {
                smoothRotation = Quaternion.identity;
                return;
            }

            Vector3 v = targetPosition - aimReferenceHelper.position;

            Quaternion targetRotation = Quaternion.identity;

            if (!smoothIKAlignmentPoint)
            {
                targetRotation = ClampRotation(Quaternion.LookRotation(v, aimReferenceHelper.up), maxHorizontalAligmentAngle * (weight), maxVerticalAligmentAngle * (weight));
            }
            else
            {
                targetRotation = Quaternion.LookRotation(v, aimReferenceHelper.up);
            }
            var currentOrientation = boneHelper.InverseTransformDirection(aimReferenceHelper.forward);
            var targetOrientation = boneHelper.InverseTransformDirection(targetRotation * Vector3.forward);

            var alignmentRotation = Quaternion.FromToRotation(currentOrientation, targetOrientation);

            if ((!float.IsNaN(alignmentRotation.x) && !float.IsNaN(alignmentRotation.y) && !float.IsNaN(alignmentRotation.z)))
            {
                Quaternion additiveRotation;
                if (!smoothIKAlignmentPoint)
                {
                    smoothRotation = Quaternion.Slerp(smoothRotation, Quaternion.Lerp(Quaternion.identity, alignmentRotation, weight), smooth * Time.fixedDeltaTime);
                    additiveRotation = smoothRotation;
                }
                else
                    additiveRotation = Quaternion.Lerp(Quaternion.identity, alignmentRotation, weight);

                bone.localRotation *= additiveRotation;
                boneHelper.localRotation *= additiveRotation;

            }
        }

        /// <summary>
        /// Clamp the target IK  alignment rotation base to <see cref="aimReferenceHelper"/> transform
        /// </summary>
        /// <param name="rotation">rotation to clamp</param>    
        /// <param name="maxHorizontalAngle">max horizontal angle (y)</param>
        /// <param name="maxVerticalAngle">max vertical angle (x)</param>
        /// <returns></returns>
        protected virtual Quaternion ClampRotation(Quaternion rotation, float maxHorizontalAngle, float maxVerticalAngle)
        {
            var direction = rotation * Vector3.forward;
            var localDirection = aimReferenceHelper.InverseTransformDirection(direction);
            var localRotation = Quaternion.LookRotation(localDirection);
            var rotationEuler = localRotation.eulerAngles.NormalizeAngle();
            rotationEuler.y = Mathf.Clamp(rotationEuler.y, -maxHorizontalAngle, maxHorizontalAngle);
            rotationEuler.x = Mathf.Clamp(rotationEuler.x, -maxVerticalAngle, maxVerticalAngle);
            rotationEuler.z = 0;
            direction = aimReferenceHelper.TransformDirection(Quaternion.Euler(rotationEuler.NormalizeAngle()) * Vector3.forward);
            return Quaternion.LookRotation(direction, aimReferenceHelper.up);
        }

        /// <summary>
        /// Apply smooth and  Clamp to the target Ik alignment Position base to <see cref="aimReferenceHelper"/> transform 
        /// </summary>
        /// <param name="position">position to clamp smoothly</param>
        /// <param name="maxHorizontalAngle"></param>
        /// <param name="maxVerticalAngle"></param>
        /// <returns></returns>
        protected virtual Vector3 SmootAndClampPosition(Vector3 position, float maxHorizontalAngle, float maxVerticalAngle)
        {
            var direction = position - aimReferenceHelper.position;
            smoothRotationDirection = Quaternion.Slerp(ClampRotation(smoothRotationDirection, maxHorizontalAngle, maxVerticalAngle), Quaternion.LookRotation(position - aimReferenceHelper.position, aimReferenceHelper.up), smooth * Time.fixedDeltaTime);
            position = aimReferenceHelper.position + smoothRotationDirection * Vector3.forward * direction.magnitude;
            return position;
        }

        public void DrawBones(Color color)
        {
            if (!IsValid) return;
            Debug.DrawLine(upperArm.position, foreArm.position, color);
            Debug.DrawLine(foreArm.position, hand.position, color);


        }

        public void DrawHelpers(Color color)
        {
            if (!IsValid) return;
            Debug.DrawLine(upperArmHelper.position, foreArmHelper.position, color);
            Debug.DrawLine(foreArmHelper.position, handHelper.position, color);
        }
    }
}