using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class vInventoryDisplayFormat
{
    static readonly List<string> ItemTypeFormats = new List<string>();
    static readonly List<string> ItemAttributeFormats = new List<string>();

    /// <summary>
    /// Get Item type string format using Description in <seealso cref="Invector.vItemManager.vItemType"/> value
    /// </summary>
    /// <param name="value">target Item type</param>
    /// <returns></returns>
    public static string DisplayFormat(this Invector.vItemManager.vItemType value)
    {
        if (ItemTypeFormats.Count == 0)
        {

            var values = System.Enum.GetValues(typeof(Invector.vItemManager.vItemType)).OfType<Invector.vItemManager.vItemType>().ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                Invector.vItemManager.vItemType v = values[i];
                ItemTypeFormats.Add(GetDisplayFormat(v));
            }
        }
        return ItemTypeFormats[(int)value];
    }

    /// <summary>
    /// Get Item Attribute string format using Description in <seealso cref="Invector.vItemManager.vItemAttributes"/> value  
    /// </summary>
    /// <param name="value">target Item Attribute</param>
    /// <returns></returns>
    public static string DisplayFormat(this Invector.vItemManager.vItemAttributes value)
    {
        if (ItemAttributeFormats.Count == 0)
        {
            var values = System.Enum.GetValues(typeof(Invector.vItemManager.vItemAttributes)).OfType<Invector.vItemManager.vItemAttributes>().ToArray();
            for (int i = 0; i < values.Length; i++)
            {
                Invector.vItemManager.vItemAttributes v = values[i];
                ItemAttributeFormats.Add(GetDisplayFormat(v));
            }
        }
        return ItemAttributeFormats[(int)value];
    }

    static string GetDisplayFormat<T>(this T value) where T : System.Enum
    {
        return
        value
        .GetType()
        .GetMember(value.ToString())
        .FirstOrDefault()
        ?.GetCustomAttribute<DescriptionAttribute>()
        ?.Description
        ?? value.ToString();
    }

}
