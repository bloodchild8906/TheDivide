using System;

using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Check Property is used to validate field using other filds.
/// </summary>
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = true)]
public class vCheckPropertyAttribute : PropertyAttribute
{
    [Serializable]
    public struct CheckValue
    {
        public string property;
        public object value;

        public bool isValid => value != null;
        public CheckValue(string property, object value)
        {
            this.property = property;
            this.value = value;
        }
    }


    public List<CheckValue> checkValues = new List<CheckValue>();

    public bool hideInInspector;
    public bool invertResult;
    /// <summary>
    /// Check Property is used to validate field using other filds.
    /// </summary>
    /// <param name="propertyNames"> Properties names  separated by "," (comma)  Exemple "PropertyA,PropertyB". Only Enum and Boolean is accepted.</param>
    /// <param name="values">The values to compare, you need to set all values to compare with all properties</param>
    public vCheckPropertyAttribute(string propertyNames, params object[] values)
    {
        
        checkValues.Clear();
        var _props = propertyNames.Split(',');

        for (int i = 0; i < _props.Length; i++)
        {
            try
            {
                checkValues.Add(new CheckValue(_props[i], values[i]));
            }
            catch
            {
                break;
            }
        }

    }
}