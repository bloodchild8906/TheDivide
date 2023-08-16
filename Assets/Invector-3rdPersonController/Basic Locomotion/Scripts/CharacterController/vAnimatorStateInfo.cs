using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Invector.vEventSystems
{
    public interface vIAnimatorStateInfoController
    {
        vAnimatorStateInfos animatorStateInfos { get; }
    }
    public static class vIAnimatorStateInfoHelper
    {
        /// <summary>
        /// Register all listener to <see cref="vAnimatorTagBase"/> listener
        /// </summary>
        /// <param name="animatorStateInfos"></param>
        public static void Register(this vIAnimatorStateInfoController animatorStateInfos)
        {
            if (animatorStateInfos.isValid())
            {
                animatorStateInfos.animatorStateInfos.RegisterListener();
            }
        }
        /// <summary>
        /// Remove all listener from <see cref="vAnimatorTagBase"/> 
        /// </summary>
        /// <param name="animatorStateInfos"></param>
        public static void UnRegister(this vIAnimatorStateInfoController animatorStateInfos)
        {
            if (animatorStateInfos.isValid())
            {
                animatorStateInfos.animatorStateInfos.RemoveListener();
            }
        }
        /// <summary>
        /// Check if is valid 
        /// </summary>
        /// <param name="animatorStateInfos"></param>
        /// <returns></returns>
        public static bool isValid(this vIAnimatorStateInfoController animatorStateInfos)
        {
            return animatorStateInfos != null && animatorStateInfos.animatorStateInfos != null && animatorStateInfos.animatorStateInfos.animator != null;
        }

    }
    [System.Serializable]
    public class vAnimatorStateInfos
    {
        public bool debug;
        public Animator animator;
        public vAnimatorStateInfos(Animator animator)
        {
            this.animator = animator;

            Init();
        }

        public void Init()
        {
            if (animator)
            {
                stateInfos = new vStateInfo[animator.layerCount];
                for (int i = 0; i < stateInfos.Length; i++)
                {
                    stateInfos[i] = new vStateInfo(i);
                }
            }
        }

        public void RegisterListener()
        {
            if (animator == null) return;
            for (int i = 0; i < stateInfos.Length; i++)
            {
                var stateInfo = stateInfos[i];
                if(stateInfo!=null)
                {
                    stateInfo.tags.Clear();
                    stateInfo.normalizedTime = 0;
                    stateInfo.layer = i;
                    stateInfo.shortPathHash = 0;
                }
                else stateInfos[i] = new vStateInfo(i);
            }
            var bhv = animator.GetBehaviours<vAnimatorTagBase>();
            for (int i = 0; i < bhv.Length; i++)
            {
                bhv[i].RemoveStateInfoListener(this);
                bhv[i].AddStateInfoListener(this);

            }
           
            if (debug)
            {
                Debug.Log($"Listeners Registered", animator);
            }
        }

        public void RemoveListener()
        {
            if (animator)
            {
                var bhv = animator.GetBehaviours<vAnimatorTagBase>();
                for (int i = 0; i < bhv.Length; i++)
                {
                    bhv[i].RemoveStateInfoListener(this);
                }
                if (debug)
                {
                    Debug.Log($"Listeners Removed", animator);
                }
            }
        }

        public vStateInfo[] stateInfos = new vStateInfo[0];
        [System.Serializable]
        public class vStateInfo
        {
            public int layer;
            public int shortPathHash;
            public float normalizedTime;
            public List<string> tags = new List<string>();
            public vStateInfo(int layer)
            {
                this.layer = layer;
            }
        }
        /// <summary>
        /// Add tag to the layer
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <param name="layer">Animator layer</param>
        public void AddStateInfo(string tag, int layer)
        {
            if (stateInfos.Length > 0 && layer < stateInfos.Length)
            {
                vStateInfo info = stateInfos[layer];
                info.tags.Add(tag);
                info.shortPathHash = 0;
                info.normalizedTime = 0;
            }
            if (debug)
            {
                Debug.Log($"<color=green>Add tag : <b><i>{tag}</i></b></color>,in the animator layer :{layer}", animator);
            }
        }
        /// <summary>
        /// Uptade State info
        /// </summary>       
        /// <param name="layer">state layer</param>
        /// <param name="normalizedTime">state normalizedTime</param>
        /// <param name="fullPathHash">state fullPathHash</param>
        public void UpdateStateInfo(int layer, float normalizedTime, int fullPathHash)
        {
            if (stateInfos.Length > 0 && layer < stateInfos.Length)
            {
                vStateInfo info = stateInfos[layer];
                info.normalizedTime = normalizedTime;
                info.shortPathHash = fullPathHash;
            }
        }
        /// <summary>
        /// Remove Tag of the layer
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <param name="layer">Animator layer</param>
        public void RemoveStateInfo(string tag, int layer)
        {
            if (stateInfos.Length > 0 && layer < stateInfos.Length)
            {
                vStateInfo info = stateInfos[layer];
                if (info.tags.Contains(tag))
                {
                    info.tags.Remove(tag);
                    if (info.tags.Count == 0)
                    {
                        info.shortPathHash = 0;
                        info.normalizedTime = 0;
                    }
                    if (debug)
                    {
                        Debug.Log($"<color=red>Remove tag : <b><i>{tag}</i></b></color>, in the animator layer :{layer}", animator);
                    }
                }
            }
        }
        /// <summary>
        /// Check If StateInfo list contains tag
        /// </summary>
        /// <param name="tag">tag to check</param>
        /// <returns></returns>
        public bool HasTag(string tag)
        {
            for (int i = 0; i < stateInfos.Length; i++)
            {
                if (stateInfos[i].tags.Contains(tag)) return true;
            }
            return false;
        }
        /// <summary>
        /// Check if All tags in in StateInfo List
        /// </summary>
        /// <param name="tags">tags to check</param>
        /// <returns></returns>
        public bool HasAllTags(params string[] tags)
        {
            var has = tags.Length > 0 ? true : false;
            for (int i = 0; i < tags.Length; i++)
            {
                if (!HasTag(tags[i]))
                {
                    has = false;
                    break;
                }
            }
            return has;
        }
        /// <summary>
        /// Check if StateInfo List Contains any tag
        /// </summary>
        /// <param name="tags">tags to check</param>
        /// <returns></returns>
        public bool HasAnyTag(params string[] tags)
        {
            var has = false;
            for (int i = 0; i < tags.Length; i++)
            {
                if (HasTag(tags[i]))
                {
                    has = true;
                    break;
                }
            }
            return has;
        }
        /// <summary>
        /// Get current animator state info using tag
        /// </summary>
        /// <param name="tag">tag</param>
        /// <returns>if tag exit return AnimatorStateInfo? else return null</returns>
        public vStateInfo GetStateInfoUsingTag(string tag)
        {
            return System.Array.Find(stateInfos, info => info.tags.Contains(tag));
        }

        public float GetCurrentNormalizedTime(int layer)
        {
            if (stateInfos.Length > 0 && layer < stateInfos.Length)
            {
                vStateInfo info = stateInfos[layer];
                return info.normalizedTime;
            }
            return 0;
        }

    }
}
