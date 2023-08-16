#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace Invector
{
    public static class vScriptableObjectUtility
    {
        /// <summary>
        /// Create new asset from <see cref="ScriptableObject"/> type with unique Name at
        /// selected folder in project window. Asset creation can be cancelled by pressing
        /// escape key when asset is initially being named.
        /// </summary>
        /// <typeparam Name="T">Type of scriptable object.</typeparam>
        /// 
        public static void CreateAsset<T>() where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            ProjectWindowUtil.CreateAsset(asset, "New " + typeof(T).Name + ".asset");
        }
    }
}
#endif
