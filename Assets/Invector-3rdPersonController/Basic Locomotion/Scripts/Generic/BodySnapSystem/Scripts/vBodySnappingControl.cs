using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Invector
{
    [vClassHeader("Body Snapping Control", openClose = false)]
    public class vBodySnappingControl : vMonoBehaviour
    {
        [vButton("Create New BodyStruct", "NewBodyStruct", typeof(vBodySnappingControl), false)]
        [vButton("Load Bones", "LoadBones", typeof(vBodySnappingControl), false)]
        //[vHelpBox("To create a new BodyStruct for a new Character to go: \n<b>*Menu Create/Invector/New Body Struct</b>, you can set for both Humanoid or Generic Rig")]
        public vBodyStruct bodyStruct;
        public bool showLabels;
        [HideInInspector] public List<vBoneTransformSnapping> boneSnappingList = new List<vBoneTransformSnapping>();
        private bool bonesIsLoaded;

        protected virtual void Reset()
        {
            LoadBones();
        }

#if UNITY_EDITOR
        public void NewBodyStruct()
        {
            vBodyStruct newBodyStruct = ScriptableObject.CreateInstance<vBodyStruct>();
            AssetDatabase.CreateAsset(newBodyStruct, "Assets/BodyStruct@" + gameObject.transform.parent.name + ".asset");
            AssetDatabase.SaveAssets();
            SerializedObject serializedObj = new SerializedObject(this);

            this.bodyStruct = newBodyStruct;
            serializedObj.ApplyModifiedProperties();
        }
#endif

        public virtual void LoadBones()
        {
            var animator = GetComponentInParent<Animator>();
            var bones = bodyStruct ? bodyStruct.bones : vBodyStruct.GetHumanBones();
            if (bodyStruct)
            {
                var needToRemove = boneSnappingList.FindAll(_b => !bones.Exists(_b2 => _b2.name.Equals(_b.name)));
                for (int i = 0; i < needToRemove.Count; i++)
                {
                    boneSnappingList.Remove(needToRemove[i]);
                }
            }
            if (bones.Count > 0)
            {
                for (int i = 0; i < bones.Count; i++)
                {
                    Transform bone = null;

                    if (bones[i].isHuman && animator && animator.isHuman)
                    {
                        bone = animator.GetBoneTransform(bones[i].humanBone);
                    }
                    else
                    {
                        bone = GetBoneByName(bones[i].genericBone);
                    }

                    vBoneTransformSnapping b = boneSnappingList.Find(_b => _b.name.Equals(bones[i].name));
                    if (b == null)
                    {
                        b = new vBoneTransformSnapping();
                        b.name = bones[i].name;
                        b.bone = bone;
                        boneSnappingList.Add(b);
                    }
                    else
                    {
                        b.bone = bone;
                    }
                }
            }
            boneSnappingList = boneSnappingList
            .OrderBy(x => x.bone != null && x.name.ToUpper().Contains("LEFT"))
            .ThenBy(x => x.bone != null && x.name.ToUpper().Contains("RIGHT"))
            .ToList();
            if (!Application.isPlaying)
            {
                bonesIsLoaded = true;
            }
        }

        protected virtual void Awake()
        {
            LoadBones();
            SnapAll();
        }

        public virtual void SnapAll()
        {
            foreach (var bt in boneSnappingList)
            {
                bt.Snap();
            }
        }

        public virtual Transform GetBone(string name)
        {
            if (!bonesIsLoaded)
            {
                LoadBones();
            }

            var bone = boneSnappingList.Find(b => b.name.Equals(name));       
            return bone != null ? bone.bone : null;
        }

        protected virtual Transform GetBoneByName(string name)
        {
            var animator = GetComponentInParent<Animator>();
            if (!animator) return null;
            var root = animator.GetBoneTransform(HumanBodyBones.Hips);
            if (root == null)
            {
                root = animator.transform;
            }

            List<Transform> childrens = root.gameObject.GetComponentsInChildren<Transform>(true).vToList();
            Transform t = null;
            if (childrens.Count > 0)
            {
                if (!string.IsNullOrEmpty(name.Trim()))
                {
                    string[] nameSplited = name.Trim().Split(';');
                    t = childrens.Find(child => ContainsName(nameSplited, child.gameObject.name.Trim()));
                }

            }
            return t;
        }

        protected virtual bool ContainsName(string[] nameSplited, string targetName)
        {
            bool contains = false;
            for (int i = 0; i < nameSplited.Length; i++)
            {
                if (targetName.Contains(nameSplited[i]))
                {
                    contains = true;
                    break;
                }
            }
            return contains;
        }

        [System.Serializable]
        public class vBoneTransformSnapping
        {
            public string name;
            public Transform bone;
            public Transform target;
            public Orientation orientation;
            public UnityEngine.Events.UnityEvent onSnap;
#if UNITY_EDITOR
            [SerializeField]
            private bool showProperties;
#endif
            public void Snap()
            {
                if (bone && target)
                {
                    if (Application.isPlaying && target.parent != bone)
                    {
                        target.parent = bone;
                        onSnap.Invoke();
                    }
                    target.rotation = targetRotation;
                    target.position = bone.position;
                }

            }
            public enum Orientation
            {
                Forward, Back, Right, Left, Up, Down
            }
            public Quaternion targetRotation
            {
                get
                {
                    Quaternion rot = Quaternion.LookRotation(Vector3.forward);
                    Vector3 lookAt = Vector3.forward;
                    if (bone && target && bone.parent)
                    {
                        switch (orientation)
                        {
                            case Orientation.Back:
                                lookAt = Vector3.back;
                                break;
                            case Orientation.Right:
                                lookAt = Vector3.right;
                                break;
                            case Orientation.Left:
                                lookAt = Vector3.left;
                                break;
                            case Orientation.Up:
                                lookAt = Vector3.up;
                                break;
                            case Orientation.Down:
                                lookAt = Vector3.down;
                                break;
                        }
                        rot = Quaternion.LookRotation(bone.TransformDirection(lookAt), bone.up);
                    }
                    return rot;
                }
            }
        }
    }
}