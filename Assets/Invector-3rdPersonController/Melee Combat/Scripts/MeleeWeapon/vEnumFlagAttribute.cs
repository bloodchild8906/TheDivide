using UnityEngine;
namespace Invector
{
    [System.AttributeUsage(System.AttributeTargets.Field,AllowMultiple = true,Inherited = true)]
    public class vEnumFlagAttribute : PropertyAttribute
    {
        public string enumName;
      
        public vEnumFlagAttribute() { }

        public vEnumFlagAttribute(string name)
        {
            enumName = name;
           
        }
    }
}