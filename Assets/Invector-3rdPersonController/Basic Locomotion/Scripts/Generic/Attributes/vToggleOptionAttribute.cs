using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field,AllowMultiple = true,Inherited = true)]
public class vToggleOptionAttribute : PropertyAttribute
{
   public string label,falseValue, trueValue;
   public vToggleOptionAttribute(string label="",string falseValue = "No",string trueValue ="Yes")
   {
        this.label = label;
        this.falseValue = falseValue;
        this.trueValue = trueValue;
   }
}
