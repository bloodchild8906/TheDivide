using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager
{
    /// <summary>
    /// Attribute of the item including the value (int)
    /// </summary>
    [System.Serializable]
    public class vItemAttribute
    {
        public vItemAttributes name = 0;
        public int value = 0;
        public bool isOpen;
        public bool isBool;
        public string displayFormat
        {
            get
            {
                return name.DisplayFormat();
            }
        }
        /// <summary>
        /// Get attribute text
        /// </summary>
        /// <param name="format">custom format, if null, the format will be <seealso cref=" displayFormat"/></param>
        /// <returns>Formated attribute text</returns>
        public string GetDisplayText(string format = null)
        {
            {
                var _text = string.IsNullOrEmpty(format) ? displayFormat : format;
                if (string.IsNullOrEmpty(_text))
                {
                    _text = name.ToString().InsertSpaceBeforeUpperCase().RemoveUnderline();
                    _text += " : " + value.ToString();
                }
                else
                {
                    if (_text.Contains("(NAME)"))
                    {
                        _text = _text.Replace("(NAME)", name.ToString().InsertSpaceBeforeUpperCase().RemoveUnderline());
                    }

                    if (_text.Contains("(VALUE)"))
                    {
                        _text = _text.Replace("(VALUE)", value.ToString());
                    }
                }
                return _text;
            }
        }        
        public vItemAttribute(vItemAttributes name, int value)
        {
            this.name = name;
            this.value = value;
        }        
    }

    public static class vItemAttributeHelper
    {
        public static void CopyTo(this vItemAttribute itemAttribute, vItemAttribute to)
        {
            to.isBool = itemAttribute.isBool;
            to.name = itemAttribute.name;
            to.value = itemAttribute.value;
        }

        public static bool Contains(this List<vItemAttribute> attributes, vItemAttributes name)
        {
            var attribute = attributes.Find(at => at.name == name);
            return attribute != null;
        }

        public static vItemAttribute GetAttributeByType(this List<vItemAttribute> attributes, vItemAttributes name)
        {
            var attribute = attributes.Find(at => at.name == name);
            return attribute;
        }

        public static bool Equals(this vItemAttribute attributeA, vItemAttribute attributeB)
        {
            return attributeA.name == attributeB.name;
        }

        public static List<vItemAttribute> CopyAsNew(this List<vItemAttribute> copy)
        {
            var target = new List<vItemAttribute>();

            if (copy != null)
            {
                for (int i = 0; i < copy.Count; i++)
                {
                    vItemAttribute attribute = new vItemAttribute(copy[i].name, copy[i].value);
                    target.Add(attribute);
                }
            }
            return target;
        }
    }
}