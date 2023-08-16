using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Invector/SnapBody/New Body Struct")]
public class vBodyStruct : ScriptableObject
{
    public List<Bone> bones = new List<Bone>();
    [System.Serializable]
    public class Bone
    {
        public string name;
        public HumanBodyBones humanBone;
        public string genericBone;
        public bool isHuman = true;
    }
    protected virtual void Reset()
    {
        bones.Clear();
        bones = GetHumanBones();

    }
    #region Static
    public static List<Bone> GetHumanBones()
    {
        List<Bone> bones = new List<Bone>();
        string[] humanBoneName = System.Enum.GetNames(typeof(HumanBodyBones));
        for (int i = 0; i < humanBoneName.Length; i++)
        {
            if (IsIgnoredBone(humanBoneName[i])) continue;
            HumanBodyBones humanBone = HumanBodyBones.Chest;
            if (humanBoneName[i].ToEnum(ref humanBone))
            {
                Bone b = new Bone();
                b.isHuman = true;
                b.name = humanBoneName[i];
                b.genericBone = humanBoneName[i];
                b.humanBone = humanBone;
                bones.Add(b);
            }
        }
        return bones.OrderBy(x => x.name.ToUpper().Contains("LEFT")).ThenBy(x => x.name.ToUpper().Contains("RIGHT")).ToList();
    }
    static string[] ignoreBones { get { return new string[] { "Thumb", "Distal", "Little", "Middle", "Index", "Ring", "Eye", "Toes", "Jaw", "LastBone" }; } }

    static bool IsIgnoredBone(string bone)
    {
        bool ignored = false;
        for (int i = 0; i < ignoreBones.Length; i++)
        {
            if (bone.Contains(ignoreBones[i]))
            {
                ignored = true;
                break;
            }
        }
        return ignored;
    }
    #endregion
}

public static class vBodyStructHelper
{
    public static bool ToEnum<T>(this string value, ref T enumTarget)
    {
        var enumValue = System.Enum.Parse(typeof(T), value);
        if (enumValue != null) enumTarget = (T)enumValue;
        return enumValue != null;
    }
}