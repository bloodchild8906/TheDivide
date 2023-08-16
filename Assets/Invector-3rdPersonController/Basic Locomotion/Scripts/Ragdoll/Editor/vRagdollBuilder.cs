using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Invector.vCharacterController
{
    class vRagdollBuilder : ScriptableWizard
    {
        public enum vRagdollAvatarType
        {
            Human, Generic
        }
        public vRagdollAvatarType avatarType = vRagdollAvatarType.Human;
        [Header("--- Animator of cursorObject Character ---")]
        public Animator animator;
        [Header("---Generic Character template---")]
        public vRagdollGenericTemplate genericTemplate = null;
        [Header("--- Bones ---")]
        public Transform root;

        public Transform leftHips;
        public Transform leftKnee;
        public Transform leftFoot;

        public Transform rightHips;
        public Transform rightKnee;
        public Transform rightFoot;

        public Transform leftArm;
        public Transform leftElbow;

        public Transform rightArm;
        public Transform rightElbow;

        public Transform middleSpine;
        public Transform head;

        [Header("--- Properties ---")]
        public bool enableProjection = true;
        public bool proportionalMass = true;
        [Header("Total Mass will be ignored and set to 1 if Proportional Mass is true")]
        public float totalMass = 20;
        public float strength = 0.0F;

        Vector3 right = Vector3.right;
        Vector3 up = Vector3.up;
        Vector3 forward = Vector3.forward;

        Vector3 worldRight = Vector3.right;
        Vector3 worldUp = Vector3.up;
        Vector3 worldForward = Vector3.forward;

        public bool flipForward = false;
        class BoneInfo
        {
            public string name;

            public Transform anchor;
            public CharacterJoint joint;
            public BoneInfo parent;

            public float minLimit;
            public float maxLimit;
            public float swingLimit;

            public Vector3 axis;
            public Vector3 normalAxis;

            public float radiusScale;
            public Type colliderType;

            public ArrayList children = new ArrayList();
            public float density;
            public float summedMass;// The mass of this and all children bodies
        }

        ArrayList bones;
        BoneInfo rootBone;

        string CheckConsistency()
        {
            PrepareBones();
            Hashtable map = new Hashtable();
            foreach (BoneInfo bone in bones)
            {
                if (bone.anchor)
                {
                    if (map[bone.anchor] != null)
                    {
                        BoneInfo oldBone = (BoneInfo)map[bone.anchor];
                        return String.Format("{0} and {1} may not be assigned to the same bone.", bone.name, oldBone.name);
                    }
                    map[bone.anchor] = bone;
                }
            }

            foreach (BoneInfo bone in bones)
            {
                if (bone.anchor == null)
                {
                    return String.Format("{0} has not been assigned yet.\n", bone.name);
                }
            }

            return "";
        }

        [MenuItem("Invector/Basic Locomotion/Components/Ragdoll")]
        static void CreateWizard()
        {
            ScriptableWizard.DisplayWizard("Create Ragdoll", typeof(vRagdollBuilder));

            //ScriptableWizard.DisplayWizard("Create Ragdoll", typeof (RagdollBuilder),"Create","Load Bones");
        }

        void DecomposeVector(out Vector3 normalCompo, out Vector3 tangentCompo, Vector3 outwardDir, Vector3 outwardNormal)
        {
            outwardNormal = outwardNormal.normalized;
            normalCompo = outwardNormal * Vector3.Dot(outwardDir, outwardNormal);
            tangentCompo = outwardDir - normalCompo;
        }

        void CalculateAxes()
        {
            if (head != null && root != null)
            {
                up = CalculateDirectionAxis(root.InverseTransformPoint(head.position));
            }

            if (rightElbow != null && root != null)
            {
                Vector3 removed, temp;
                DecomposeVector(out temp, out removed, root.InverseTransformPoint(rightElbow.position), up);
                right = CalculateDirectionAxis(removed);
            }

            forward = Vector3.Cross(right, up);
            if (flipForward)
            {
                forward = -forward;
            }
        }

        void Update()
        {
            errorString = CheckConsistency();
            CalculateAxes();

            if (errorString.Length != 0)
            {
                helpString = "Drag all bones from the hierarchy into their slots.\nMake sure your character is in T-Stand.\n";
            }
            else
            {
                helpString = "Make sure your character is in T-Stand.\nMake sure the blue axis faces in the same direction the chracter is looking.\nUse flipForward to flip the direction";
            }

            isValid = errorString.Length == 0;
        }

        void PrepareBones()
        {
            if (Selection.activeGameObject != null && Selection.activeGameObject.transform.GetComponent<Animator>() != null)
            {
                animator = Selection.activeGameObject.transform.GetComponent<Animator>();
                animator.Rebind();
            }
            if (animator != null && avatarType == vRagdollAvatarType.Human)
            {
                GetHumanBones();
            }
            else
            {
                GetGenericBones();
            }

            if (root)
            {
                worldRight = root.TransformDirection(right);
                worldUp = root.TransformDirection(up);
                worldForward = root.TransformDirection(forward);
            }

            bones = new ArrayList();

            rootBone = new BoneInfo();
            rootBone.name = "Root";
            rootBone.anchor = root;
            rootBone.parent = null;
            rootBone.density = 2.5F;
            bones.Add(rootBone);

            AddMirroredJoint("Hips", leftHips, rightHips, "Root", worldRight, worldForward, -20, 70, 30, typeof(CapsuleCollider), 0.3F, 1.5F);
            AddMirroredJoint("Knee", leftKnee, rightKnee, "Hips", worldRight, worldForward, -80, 0, 0, typeof(CapsuleCollider), 0.25F, 1.5F);
            //		AddMirroredJoint ("Hips", leftHips, rightHips, "Root", worldRight, worldForward, -0, -70, 30, typeof(CapsuleCollider), 0.3F, 1.5F);
            //		AddMirroredJoint ("Knee", leftKnee, rightKnee, "Hips", worldRight, worldForward, -0, -50, 0, typeof(CapsuleCollider), .25F, 1.5F);

            AddJoint("Middle Spine", middleSpine, "Root", worldRight, worldForward, -20, 20, 10, null, 1, 2.5F);

            AddMirroredJoint("Arm", leftArm, rightArm, "Middle Spine", worldUp, worldForward, -70, 10, 50, typeof(CapsuleCollider), 0.25F, 1.0F);
            AddMirroredJoint("Elbow", leftElbow, rightElbow, "Arm", worldForward, worldUp, -90, 0, 0, typeof(CapsuleCollider), 0.20F, 1.0F);

            AddJoint("Head", head, "Middle Spine", worldRight, worldForward, -40, 25, 25, null, 1, 1.0F);
        }

        void GetHumanBones()
        {
            try
            {
                root = animator.GetBoneTransform(HumanBodyBones.Hips);

                leftHips = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
                leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);

                rightHips = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
                rightKnee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);

                leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
                leftElbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);

                rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
                rightElbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);

                middleSpine = animator.GetBoneTransform(HumanBodyBones.Chest);

                head = animator.GetBoneTransform(HumanBodyBones.Head);

                EditorUtility.SetDirty(this);
            }
            catch
            {
            }
        }

        void GetGenericBones()
        {
            if (!genericTemplate && Selection.activeObject)
            {
                return;
            }

            try
            {
                var rootTransform = Selection.activeTransform;
                root = genericTemplate.GetRoot(rootTransform);

                leftHips = genericTemplate.GetLeftHips(rootTransform);
                leftKnee = genericTemplate.GetLeftKnee(rootTransform);
                leftFoot = genericTemplate.GetLeftFoot(rootTransform);

                rightHips = genericTemplate.GetRightHips(rootTransform);
                rightKnee = genericTemplate.GetRightKnee(rootTransform);
                rightFoot = genericTemplate.GetRightFoot(rootTransform);

                leftArm = genericTemplate.GetLeftArm(rootTransform);
                leftElbow = genericTemplate.GetLeftElbow(rootTransform);

                rightArm = genericTemplate.GetRightArm(rootTransform);
                rightElbow = genericTemplate.GetRightElbow(rootTransform);

                middleSpine = genericTemplate.GetMiddleSpine(rootTransform);

                head = genericTemplate.GetHead(rootTransform);

                EditorUtility.SetDirty(this);
            }
            catch
            {
            }
        }

        void OnWizardCreate()
        {
            //if(Selection.activeGameObject!=null)
            var hasRagdoll = Selection.activeGameObject.GetComponent<vRagdoll>();
            if (!hasRagdoll)
            {
                Selection.activeGameObject.AddComponent<vRagdoll>();
            }

            Selection.activeGameObject.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            Cleanup();
            BuildCapsules();
            AddBreastColliders();
            AddHeadCollider();

            BuildBodies();
            BuildJoints();
            CalculateMass();
        }

        BoneInfo FindBone(string name)
        {
            foreach (BoneInfo bone in bones)
            {
                if (bone.name == name)
                {
                    return bone;
                }
            }
            return null;
        }

        void AddMirroredJoint(string name, Transform leftAnchor, Transform rightAnchor, string parent, Vector3 worldTwistAxis, Vector3 worldSwingAxis, float minLimit, float maxLimit, float swingLimit, Type colliderType, float radiusScale, float density)
        {
            AddJoint("Left " + name, leftAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit, swingLimit, colliderType, radiusScale, density);
            AddJoint("Right " + name, rightAnchor, parent, worldTwistAxis, worldSwingAxis, minLimit, maxLimit, swingLimit, colliderType, radiusScale, density);
        }

        void AddJoint(string name, Transform anchor, string parent, Vector3 worldTwistAxis, Vector3 worldSwingAxis, float minLimit, float maxLimit, float swingLimit, Type colliderType, float radiusScale, float density)
        {
            BoneInfo bone = new BoneInfo();
            bone.name = name;
            bone.anchor = anchor;
            bone.axis = worldTwistAxis;
            bone.normalAxis = worldSwingAxis;
            bone.minLimit = minLimit;
            bone.maxLimit = maxLimit;
            bone.swingLimit = swingLimit;
            bone.density = density;
            bone.colliderType = colliderType;
            bone.radiusScale = radiusScale;

            if (FindBone(parent) != null)
            {
                bone.parent = FindBone(parent);
            }
            else if (name.StartsWith("Left"))
            {
                bone.parent = FindBone("Left " + parent);
            }
            else if (name.StartsWith("Right"))
            {
                bone.parent = FindBone("Right " + parent);
            }

            bone.parent.children.Add(bone);
            bones.Add(bone);
        }

        void BuildCapsules()
        {
            foreach (BoneInfo bone in bones)
            {
                if (bone.colliderType != typeof(CapsuleCollider))
                {
                    continue;
                }

                int direction;
                float distance;
                if (bone.children.Count == 1)
                {
                    BoneInfo childBone = (BoneInfo)bone.children[0];
                    Vector3 endPoint = childBone.anchor.position;
                    CalculateDirection(bone.anchor.InverseTransformPoint(endPoint), out direction, out distance);
                }
                else
                {
                    Vector3 endPoint = (bone.anchor.position - bone.parent.anchor.position) + bone.anchor.position;
                    CalculateDirection(bone.anchor.InverseTransformPoint(endPoint), out direction, out distance);

                    if (bone.anchor.GetComponentsInChildren(typeof(Transform)).Length > 1)
                    {
                        Bounds bounds = new Bounds();
                        foreach (Transform child in bone.anchor.GetComponentsInChildren(typeof(Transform)))
                        {
                            bounds.Encapsulate(bone.anchor.InverseTransformPoint(child.position));
                        }

                        if (distance > 0)
                        {
                            distance = bounds.max[direction];
                        }
                        else
                        {
                            distance = bounds.min[direction];
                        }
                    }
                }

                CapsuleCollider collider = bone.anchor.gameObject.AddComponent<CapsuleCollider>();
                collider.direction = direction;

                Vector3 center = Vector3.zero;
                center[direction] = distance * 0.5F;
                collider.center = center;
                collider.height = Mathf.Abs(distance);
                collider.radius = Mathf.Abs(distance * bone.radiusScale);
            }
        }

        void Cleanup()
        {
            foreach (BoneInfo bone in bones)
            {
                if (!bone.anchor)
                {
                    continue;
                }

                Component[] joints = bone.anchor.GetComponentsInChildren(typeof(Joint));
                foreach (Joint joint in joints)
                {
                    DestroyImmediate(joint);
                }

                Component[] bodies = bone.anchor.GetComponentsInChildren(typeof(Rigidbody));
                foreach (Rigidbody body in bodies)
                {
                    DestroyImmediate(body);
                }

                Component[] colliders = bone.anchor.GetComponentsInChildren(typeof(Collider));
                foreach (Collider collider in colliders)
                {
                    if (collider.transform != leftFoot.transform && collider.transform != rightFoot)
                    {
                        DestroyImmediate(collider);
                    }
                }

            }
        }

        void BuildBodies()
        {
            foreach (BoneInfo bone in bones)
            {
                bone.anchor.gameObject.AddComponent<Rigidbody>();
                var hasDamageReceiver = bone.anchor.gameObject.GetComponent<vDamageReceiver>();
                if (!hasDamageReceiver)
                {
                    bone.anchor.gameObject.AddComponent<vDamageReceiver>();
                }

                bone.anchor.GetComponent<Rigidbody>().mass = bone.density;
                bone.anchor.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                // set layer "BodyPart" to each bone 
                bone.anchor.gameObject.layer = LayerMask.NameToLayer("BodyPart");
            }
        }

        void BuildJoints()
        {
            foreach (BoneInfo bone in bones)
            {
                if (bone.parent == null)
                {
                    continue;
                }

                CharacterJoint joint = bone.anchor.gameObject.AddComponent<CharacterJoint>();
                bone.joint = joint;

                // Setup connection and axis
                joint.axis = CalculateDirectionAxis(bone.anchor.InverseTransformDirection(bone.axis));
                joint.swingAxis = CalculateDirectionAxis(bone.anchor.InverseTransformDirection(bone.normalAxis));
                joint.anchor = Vector3.zero;
                joint.connectedBody = bone.parent.anchor.GetComponent<Rigidbody>();

                // Setup limits			
                SoftJointLimit limit = new SoftJointLimit();

                limit.limit = bone.minLimit;
                joint.lowTwistLimit = limit;

                limit.limit = bone.maxLimit;
                joint.highTwistLimit = limit;

                limit.limit = bone.swingLimit;
                joint.swing1Limit = limit;

                limit.limit = 0;
                joint.swing2Limit = limit;
                joint.enableProjection = enableProjection;
            }
        }

        void CalculateMassRecurse(BoneInfo bone)
        {
            float mass = bone.anchor.GetComponent<Rigidbody>().mass;
            foreach (BoneInfo child in bone.children)
            {
                CalculateMassRecurse(child);
                mass += child.summedMass;
            }
            bone.summedMass = mass;
        }

        void CalculateMass()
        {
            // Calculate allChildMass by summing all bodies
            CalculateMassRecurse(rootBone);

            // Rescale the mass so that the whole character weights totalMass
            float massScale = totalMass / rootBone.summedMass;
            foreach (BoneInfo bone in bones)
            {
                if (proportionalMass)
                {
                    bone.anchor.GetComponent<Rigidbody>().mass = 10;
                }
                else
                {
                    bone.anchor.GetComponent<Rigidbody>().mass *= massScale;
                }
            }

            // Recalculate allChildMass by summing all bodies
            CalculateMassRecurse(rootBone);
        }

        static void CalculateDirection(Vector3 point, out int direction, out float distance)
        {
            // Calculate longest axis
            direction = 0;
            if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
            {
                direction = 1;
            }

            if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
            {
                direction = 2;
            }

            distance = point[direction];
        }

        static Vector3 CalculateDirectionAxis(Vector3 point)
        {
            int direction = 0;
            float distance;
            CalculateDirection(point, out direction, out distance);
            Vector3 axis = Vector3.zero;
            if (distance > 0)
            {
                axis[direction] = 1.0F;
            }
            else
            {
                axis[direction] = -1.0F;
            }

            return axis;
        }

        static int SmallestComponent(Vector3 point)
        {
            int direction = 0;
            if (Mathf.Abs(point[1]) < Mathf.Abs(point[0]))
            {
                direction = 1;
            }

            if (Mathf.Abs(point[2]) < Mathf.Abs(point[direction]))
            {
                direction = 2;
            }

            return direction;
        }

        static int LargestComponent(Vector3 point)
        {
            int direction = 0;
            if (Mathf.Abs(point[1]) > Mathf.Abs(point[0]))
            {
                direction = 1;
            }

            if (Mathf.Abs(point[2]) > Mathf.Abs(point[direction]))
            {
                direction = 2;
            }

            return direction;
        }

        static int SecondLargestComponent(Vector3 point)
        {
            int smallest = SmallestComponent(point);
            int largest = LargestComponent(point);
            if (smallest < largest)
            {
                int temp = largest;
                largest = smallest;
                smallest = temp;
            }

            if (smallest == 0 && largest == 1)
            {
                return 2;
            }
            else if (smallest == 0 && largest == 2)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        Bounds Clip(Bounds bounds, Transform relativeTo, Transform clipTransform, bool below)
        {
            int axis = LargestComponent(bounds.size);

            if (Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.max)) > Vector3.Dot(worldUp, relativeTo.TransformPoint(bounds.min)) == below)
            {
                Vector3 min = bounds.min;
                min[axis] = relativeTo.InverseTransformPoint(clipTransform.position)[axis];
                bounds.min = min;
            }
            else
            {
                Vector3 max = bounds.max;
                max[axis] = relativeTo.InverseTransformPoint(clipTransform.position)[axis];
                bounds.max = max;
            }
            return bounds;
        }

        Bounds GetBreastBounds(Transform relativeTo)
        {
            // Root bounds
            Bounds bounds = new Bounds();
            bounds.Encapsulate(relativeTo.InverseTransformPoint(leftHips.position));
            bounds.Encapsulate(relativeTo.InverseTransformPoint(rightHips.position));
            bounds.Encapsulate(relativeTo.InverseTransformPoint(leftArm.position));
            bounds.Encapsulate(relativeTo.InverseTransformPoint(rightArm.position));
            Vector3 size = bounds.size;
            size[SmallestComponent(bounds.size)] = size[LargestComponent(bounds.size)] / 2.0F;
            bounds.size = size;
            return bounds;
        }

        void AddBreastColliders()
        {
            // Middle spine and root
            if (middleSpine != null && root != null)
            {
                Bounds bounds;
                BoxCollider box;

                // Middle spine bounds
                bounds = Clip(GetBreastBounds(root), root, middleSpine, false);
                box = root.gameObject.AddComponent<BoxCollider>();
                box.center = bounds.center;
                box.size = bounds.size;

                bounds = Clip(GetBreastBounds(middleSpine), middleSpine, middleSpine, true);
                box = middleSpine.gameObject.AddComponent<BoxCollider>();
                box.center = bounds.center;
                box.size = bounds.size;
            }
            // Only root
            else
            {
                Bounds bounds = new Bounds();
                bounds.Encapsulate(root.InverseTransformPoint(leftHips.position));
                bounds.Encapsulate(root.InverseTransformPoint(rightHips.position));
                bounds.Encapsulate(root.InverseTransformPoint(leftArm.position));
                bounds.Encapsulate(root.InverseTransformPoint(rightArm.position));

                Vector3 size = bounds.size;
                size[SmallestComponent(bounds.size)] = size[LargestComponent(bounds.size)] / 2.0F;

                BoxCollider box = root.gameObject.AddComponent<BoxCollider>();
                box.center = bounds.center;
                box.size = size;
            }
        }

        void AddHeadCollider()
        {
            if (head.GetComponent<Collider>())
            {
                Destroy(head.GetComponent<Collider>());
            }

            float radius = Vector3.Distance(root.InverseTransformPoint(rightArm.transform.position), root.InverseTransformPoint(leftArm.transform.position));
            radius /= 4;

            SphereCollider sphere = head.gameObject.AddComponent<SphereCollider>();
            sphere.radius = radius;
            Vector3 center = Vector3.zero;

            int direction;
            float distance;
            CalculateDirection(head.InverseTransformPoint(root.position), out direction, out distance);
            if (distance > 0)
            {
                center[direction] = -radius;
            }
            else
            {
                center[direction] = radius;
            }

            sphere.center = center;
        }

    }
}
