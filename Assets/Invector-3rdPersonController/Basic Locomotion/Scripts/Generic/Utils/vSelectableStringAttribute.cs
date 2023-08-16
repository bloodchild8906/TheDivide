using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.AttributeUsage(System.AttributeTargets.Field,AllowMultiple =true)]
public class vSelectableStringAttribute : PropertyAttribute
{
    public string tittle;
    public string selectableText;
    public vSelectableStringAttribute(string tittle, string selectableText)
    {
        this.tittle = tittle;
        this.selectableText = selectableText;
    }
}
