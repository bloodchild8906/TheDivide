using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vItemManager
{
    /// <summary>
    /// This attribute is used to draw aditional custom editor like a ToolBar for <see cref="vItem"/> in  partial classes of the <see cref="vItemDrawer"/> using <see cref="vItemDrawer.OnDrawItem"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class vItemDrawerToolBarAttribute : Attribute
    {
        public string title;
        public vItemDrawerToolBarAttribute(string title)
        {
            this.title = title;
        }
    }
}