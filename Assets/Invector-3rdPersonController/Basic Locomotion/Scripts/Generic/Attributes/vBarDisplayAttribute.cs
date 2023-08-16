using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Field | System.AttributeTargets.Property, AllowMultiple = true, Inherited = true)]
public class vBarDisplayAttribute : PropertyAttribute
{
    public readonly string maxValueProperty;
    public readonly bool showJuntInPlayMode;
    public vBarDisplayAttribute(string maxValueProperty, bool showJuntInPlayMode = false)
    {
        this.maxValueProperty = maxValueProperty;
        this.showJuntInPlayMode = showJuntInPlayMode;
    }
}
