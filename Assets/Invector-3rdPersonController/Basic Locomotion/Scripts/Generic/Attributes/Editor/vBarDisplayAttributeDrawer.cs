using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(vBarDisplayAttribute), true)]
public class vBarDisplayAttributeDrawer : PropertyDrawer
{
    Gradient g = new Gradient();
    GradientColorKey[] gck = new GradientColorKey[] { new GradientColorKey(Color.red, 0), new GradientColorKey(Color.yellow, 0.75f), new GradientColorKey(Color.green, 1) };
    GradientAlphaKey[] gak = new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) };
    Rect rectA;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var color = GUI.color;
        var atrbt = attribute as vBarDisplayAttribute;
        position.height = base.GetPropertyHeight(property, label);
        if (atrbt != null)
        {
            if (atrbt.showJuntInPlayMode && !Application.isPlaying)
            {
                EditorGUI.PropertyField(position, property, label);
                return;
            }

            var maxValue = property.serializedObject.FindProperty(property.propertyPath.Replace(property.name, atrbt.maxValueProperty));
            if (maxValue != null)
            {
                GUI.BeginGroup(position, "", EditorStyles.toolbar);

                float valueA = 0;
                float valueB = 0;
                switch (property.propertyType)
                {
                    case SerializedPropertyType.Float:
                        valueA = property.floatValue;
                        break;
                    case SerializedPropertyType.Integer:
                        valueA = property.intValue;
                        break;
                }
                switch (maxValue.propertyType)
                {
                    case SerializedPropertyType.Float:
                        valueB = maxValue.floatValue;
                        break;
                    case SerializedPropertyType.Integer:
                        valueB = maxValue.intValue;
                        break;
                }
                float currentValue = valueA / valueB;

                g.SetKeys(gck, gak);
                GUI.color = g.Evaluate(currentValue);
                rectA = position;
                rectA.y = 0;
                rectA.x = 0;
                rectA.width = position.width * currentValue;

                GUI.color = g.Evaluate(currentValue);
                GUI.Box(rectA, "");
                rectA.width = position.width;
                rectA.x = (position.width / 3);
                GUI.Label(rectA, property.displayName + ":" + valueA.ToString("0") + "/" + valueB.ToString("0"));
                GUI.EndGroup();
                position.y += base.GetPropertyHeight(property, label) + 5;
            }
        }
        GUI.color = color;

        EditorGUI.PropertyField(position, property, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var atrbt = attribute as vBarDisplayAttribute;
        if (atrbt.showJuntInPlayMode && !Application.isPlaying)
            return base.GetPropertyHeight(property, label);
        else return (base.GetPropertyHeight(property, label) * 2f) + 5;
    }
}
