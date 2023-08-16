using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Invector
{
    [InitializeOnLoad]
    public static class vAnimatorTagEditorHelper
    {
        private static GUIStyle _customTagStyle;

        private static GUIStyle _defaultTagStyle;       

        static vAnimatorTagEditorHelper()
        {
            TAGS = new Dictionary<string, TooltipAttribute>();
            FieldInfo[] fields = typeof(vAnimatorTags).GetFields(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < fields.Length; i++)
            {
                if(fields[i].FieldType.Equals(typeof(string)))
                {
                    string fieldvalue = (string)fields[i].GetValue(null);
                    if(!TAGS.ContainsKey(fieldvalue))
                    {
                        TAGS.Add(fieldvalue, fields[i].GetCustomAttribute<TooltipAttribute>());
                    }
                }
            }
        }

        public static string GetTooltip(string tag)
        {
            if (TAGS!=null && TAGS.ContainsKey(tag))
            {
                TooltipAttribute tooltipAttribute = TAGS[tag];
                string tooltip = tooltipAttribute!=null ? tooltipAttribute.tooltip : "";
                return tooltip;
            }
            return "You can use custom tags with the method 'IsAnimatorTag(customTag)' to create special conditions in your code while this animation is being played";
        }

        public static Dictionary<string, TooltipAttribute> TAGS
        {
            get;
            private set;
        }

        public static GUIStyle CustomTagStyle
        {
            get
            {
                if (_customTagStyle == null)
                {
                    _customTagStyle = new GUIStyle(EditorStyles.textField);
                    _customTagStyle.fontStyle = FontStyle.Italic;
                    _customTagStyle.fontSize = EditorStyles.textField.fontSize - 1;
                }
                return  _customTagStyle;
            }
        }

        public static GUIStyle DefaultTagStyle
        {
            get
            {
                if(_defaultTagStyle==null)
                {
                    _defaultTagStyle = new GUIStyle(EditorStyles.textField);
                    _defaultTagStyle.fontStyle = FontStyle.Bold;                  
                }
                return _defaultTagStyle;
            }
        }

        public static bool IsDefaultTag(string tag)
        {
            return TAGS != null && TAGS.ContainsKey(tag);
        }
    }
}
