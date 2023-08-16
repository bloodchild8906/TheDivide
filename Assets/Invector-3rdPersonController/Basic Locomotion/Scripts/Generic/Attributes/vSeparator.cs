using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
public class vSeparator : PropertyAttribute
{     
    public string label;
    public string tooltip;
    public string style;
    public int fontSize = 10;

    public vSeparator()
    {       
        fontSize = 15;
    }

    public vSeparator(string label, string tooltip = "")
    {        
        this.label = label;
        this.tooltip = tooltip;
        this.fontSize = 15;
    }

    public vSeparator(string label, int fontSize, string tooltip = "")
    {
        this.label = label;
        this.tooltip = tooltip;
        this.fontSize = fontSize;
    }
}
