using UnityEngine;

namespace Invector.IK
{
    [System.Serializable]
    public class vIKSolver
    {
        public Transform rootTransform;
        public Transform rootBone;
        public Transform middleBone;
        public Transform endBone;
        [Header("Optional")]
        public Transform endBoneRef;
        public Transform middleBoneRef;
        public Transform endBoneOffset;
        public Transform middleBoneOffset;

        string middleTag, endTag;
        float _weight;
        Vector3? hintPosition;        
        /// <summary>
        /// Manual creation of the bone targets
        /// </summary>
        /// <param name="rootBone"></param>
        /// <param name="middleBone"></param>
        /// <param name="endBone"></param>
        public vIKSolver(Transform rootTransform, Transform rootBone, Transform middleBone, Transform endBone)
        {
            this.rootTransform = rootTransform;
            this.rootBone = rootBone;
            this.middleBone = middleBone;
            this.endBone = endBone;
        }

        /// <summary>
        /// Auto creation of the bone targets
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="ikGoal"></param>
        public vIKSolver(Animator animator, AvatarIKGoal ikGoal)
        {
            if (animator == null) { return; }
            this.rootTransform = animator.transform;
            if (animator.isHuman)
            {
                switch (ikGoal)
                {
                    case AvatarIKGoal.LeftHand:
                        rootBone = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                        middleBone = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                        endBone = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                        endTag = "LeftHand";
                        middleTag = "LeftHint";
                        break;
                    case AvatarIKGoal.RightHand:
                        rootBone = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                        middleBone = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
                        endBone = animator.GetBoneTransform(HumanBodyBones.RightHand);
                        endTag = "RightHand";
                        middleTag = "RightHint";
                        break;
                    case AvatarIKGoal.LeftFoot:
                        rootBone = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                        middleBone = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                        endBone = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
                        endTag = "LeftFoot";
                        middleTag = "LeftHint";
                        break;
                    case AvatarIKGoal.RightFoot:
                        rootBone = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                        middleBone = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                        endBone = animator.GetBoneTransform(HumanBodyBones.RightFoot);
                        endTag = "RightFoot";
                        middleTag = "RightHint";
                        break;
                }
            }
            CreateBones();
        }       
        public bool isValidBones
        {
            get
            {
                return rootBone &&
                       middleBone &&
                       endBone && endBoneRef &&
                       middleBoneRef &&
                       endBoneOffset &&
                       middleBoneOffset;

            }
        }

        void CreateBones()
        {
            if (rootTransform && rootBone && middleBone && endBone)
            {
                if (!endBoneRef)
                {
                    endBoneRef = new GameObject(endTag + "Ref").transform;
                    endBoneRef.hideFlags = HideFlags.HideInHierarchy;
                    endBoneRef.SetParent(rootTransform);
                }
                if (!middleBoneRef)
                {
                    middleBoneRef = new GameObject(middleTag + "Ref").transform;
                    middleBoneRef.hideFlags = HideFlags.HideInHierarchy;
                    middleBoneRef.SetParent(rootTransform);
                }

                if (!endBoneOffset)
                {
                    endBoneOffset = new GameObject(endTag + "Offset").transform;
                    endBoneOffset.SetParent(endBoneRef);
                    endBoneOffset.localPosition = Vector3.zero;
                    endBoneOffset.localEulerAngles = Vector3.zero;
                }
                if (!middleBoneOffset)
                {
                    middleBoneOffset = new GameObject(middleTag + "Offset").transform;
                    middleBoneOffset.SetParent(middleBoneRef);
                    middleBoneOffset.localPosition = Vector3.zero;
                    middleBoneOffset.localEulerAngles = Vector3.zero;
                }
            }
        }

        /// <summary>
        /// Get IK Weight
        /// </summary>
        public virtual float ikWeight { get { return _weight; } }

        /// <summary>
        /// Set IK Weight
        /// </summary>
        /// <param name="weight"></param>
        public virtual void SetIKWeight(float weight)
        {
            _weight = weight;
        }

        public void UpdateIK()
        {
            if (endBoneRef)
            {
                endBoneRef.position = endBone.position;
                endBoneRef.rotation = endBone.rotation;

            }
            if (middleBoneRef)
            {
                middleBoneRef.position = middleBone.position;
                middleBoneRef.rotation = middleBone.rotation;
            }

        }

        public virtual void AnimationToIK()
        {
            if (!isValidBones)
            {
                CreateBones();
                return;
            }
            UpdateIK();
            SetIKHintPosition(middleBoneOffset.position);
            SetIKPosition(endBoneOffset.position);
            SetIKRotation(endBoneOffset.rotation);
        }
        /// <summary>
        /// Set IK Position
        /// </summary>
        /// <param name="ikPosition"></param>
        public virtual void SetIKPosition(Vector3 ikPosition)
        {
            if (ikWeight <= 0.0f) return;
            // Calculate middleBone Direction
            Vector3 middleBoneDirection = Vector3.zero;

            if (hintPosition != null)
            {
                // if middleBoneGoal is null, the direction will be calculated with forearm's point
                middleBoneDirection = (Vector3)hintPosition - rootBone.position;
            }
            else
            {
                middleBoneDirection = Vector3.Cross(endBone.position - rootBone.position, Vector3.Cross(endBone.position - rootBone.position, endBone.position - middleBone.position));
            }

            // Get lengths of Arm
            float rootBoneLength = (middleBone.position - rootBone.position).magnitude;
            float middleBoneLength = (endBone.position - middleBone.position).magnitude;

            // Calculate the desired middleBone  position	
            Vector3 middleBonePos = GetHintPosition(rootBone.position, ikPosition, rootBoneLength, middleBoneLength, middleBoneDirection);

            // Rotate the bone transformations to align correctly
            Quaternion upperarmRotation = Quaternion.FromToRotation(middleBone.position - rootBone.position, middleBonePos - rootBone.position) * rootBone.rotation;
            if (!(System.Single.IsNaN(upperarmRotation.x) || System.Single.IsNaN(upperarmRotation.y) || System.Single.IsNaN(upperarmRotation.z)))
            {
                //Rotate with transition
                rootBone.rotation = Quaternion.Slerp(rootBone.rotation, upperarmRotation, ikWeight);
                Quaternion middleBoneRotation = Quaternion.FromToRotation(endBone.position - middleBone.position, ikPosition - middleBonePos) * middleBone.rotation;
                middleBone.rotation = Quaternion.Slerp(middleBone.rotation, middleBoneRotation, ikWeight);

            }
            hintPosition = null;
        }

        /// <summary>
        /// Set IK Rotation
        /// </summary>
        /// <param name="rotation"></param>
        public virtual void SetIKRotation(Quaternion rotation)
        {
            if (!(rootBone && middleBone && endBone) || ikWeight <= 0.0f) return;
            var _rotation = rotation;

            endBone.rotation = Quaternion.Slerp(endBone.rotation, _rotation, ikWeight);
        }

        /// <summary>
        /// Set IK Hint Position
        /// ps: Call before SetIKPosition
        /// </summary>
        /// <param name="hintPosition"></param>
        public virtual void SetIKHintPosition(Vector3 hintPosition)
        {
            this.hintPosition = hintPosition;
        }

        /// <summary>
        /// Get IK Hint Position
        /// </summary>
        /// <param name="rootPos"></param>
        /// <param name="endPos"></param>
        /// <param name="rootBoneLength"></param>
        /// <param name="middleBoneLength"></param>
        /// <param name="middleBoneDirection"></param>
        /// <returns></returns>
        protected virtual Vector3 GetHintPosition(Vector3 rootPos, Vector3 endPos, float rootBoneLength, float middleBoneLength, Vector3 middleBoneDirection)
        {
            Vector3 rootToEndDir = endPos - rootPos;
            float rootToEndMag = rootToEndDir.magnitude;

            float maxDist = (rootBoneLength + middleBoneLength) * 0.999f;
            if (rootToEndMag > maxDist)
            {
                endPos = rootPos + (rootToEndDir.normalized * maxDist);
                rootToEndDir = endPos - rootPos;
                rootToEndMag = maxDist;
            }

            float minDist = Mathf.Abs(rootBoneLength - middleBoneLength) * 1.001f;
            if (rootToEndMag < minDist)
            {
                endPos = rootPos + (rootToEndDir.normalized * minDist);
                rootToEndDir = endPos - rootPos;
                rootToEndMag = minDist;
            }

            float aa = ((rootToEndMag * rootToEndMag + rootBoneLength * rootBoneLength - middleBoneLength * middleBoneLength) * 0.5f) / rootToEndMag;
            float bb = Mathf.Sqrt(rootBoneLength * rootBoneLength - aa * aa);
            Vector3 crossElbow = Vector3.Cross(rootToEndDir, Vector3.Cross(middleBoneDirection, rootToEndDir));
            return rootPos + (aa * rootToEndDir.normalized) + (bb * crossElbow.normalized);
        }
    }
}
