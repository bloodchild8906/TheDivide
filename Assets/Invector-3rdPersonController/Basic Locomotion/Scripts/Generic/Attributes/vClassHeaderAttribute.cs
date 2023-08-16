using System;

namespace Invector
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
    public sealed class vClassHeaderAttribute : Attribute
    {
        public string header;
        public bool openClose;
        public string iconName;
        public bool useHelpBox;
        public string helpBoxText;

        public vClassHeaderAttribute(string header, bool openClose = true, string iconName = "icon_v2", bool useHelpBox = false, string helpBoxText = "")
        {
            this.header = header.ToUpper();
            this.openClose = openClose;
            this.iconName = iconName;
            this.useHelpBox = useHelpBox;
            this.helpBoxText = helpBoxText;
        }

        public vClassHeaderAttribute(string header, string helpBoxText)
        {
            this.header = header.ToUpper();
            this.openClose = true;
            this.iconName = "icon_v2";
            this.useHelpBox = true;
            this.helpBoxText = helpBoxText;
        }
    }
}