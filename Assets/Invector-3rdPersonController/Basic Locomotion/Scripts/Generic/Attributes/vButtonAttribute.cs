using System;
using System.Runtime.InteropServices;
using UnityEngine;
namespace Invector
{
     [AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
    public class vButtonAttribute : PropertyAttribute
    {
        public readonly string label;
        public readonly string function;
        public readonly int id;
        public readonly Type type;
        public readonly bool enabledJustInPlayMode;

        /// <summary>
        /// Create a button in Inspector
        /// </summary>
        /// <param name="label">button label</param>
        /// <param name="function">function to call on press</param>
        /// <param name="type">parent class type button</param>
        /// <param name="enabledJustInPlayMode">button is enabled just in play mode</param>
        public vButtonAttribute(string label, string function, Type type, bool enabledJustInPlayMode = true)
        {
            this.label = label;
            this.function = function;
            this.type = type;
            this.enabledJustInPlayMode = enabledJustInPlayMode;
        }
    }
}

