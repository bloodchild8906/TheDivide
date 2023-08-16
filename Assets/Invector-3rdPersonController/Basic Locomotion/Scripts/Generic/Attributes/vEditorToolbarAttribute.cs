using UnityEngine;
using System.Collections;
namespace Invector
{
    public class vEditorToolbarAttribute : PropertyAttribute
    {
        public readonly string title;
        public readonly string icon;
        public readonly bool useIcon;
        public readonly bool overrideChildOrder;
        public readonly bool overrideIcon;
        public vEditorToolbarAttribute(string title,bool useIcon = false,string iconName="",bool overrideIcon = false, bool overrideChildOrder = false)
        {
            this.title = title;
            this.icon = iconName;
            this.useIcon = useIcon;
            this.overrideChildOrder = overrideChildOrder;
            this.overrideIcon = overrideIcon;
        }
    }
}
