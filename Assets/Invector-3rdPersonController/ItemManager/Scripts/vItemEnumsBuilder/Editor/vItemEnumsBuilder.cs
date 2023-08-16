using UnityEditor;
using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;

namespace Invector.vItemManager
{
    public static class Helper
    {
        public static bool NameEquals(this vItemEnumsBuilder.ItemEnumObject a, vItemEnumsBuilder.ItemEnumObject b)
        {
            return a.name.Equals(b.name);
        }
        public static bool FormatEquals(this vItemEnumsBuilder.ItemEnumObject a, vItemEnumsBuilder.ItemEnumObject b)
        {
            return a.format.Equals(b.format);
        }
    }
    public class vItemEnumsBuilder
    {
      
        public class ItemEnumObject
        {
           public string name;
           public string format;
           public ItemEnumObject (string name,string format)
           {
                this.name = name;
                this.format = format;
           }
        }
        public static void RefreshItemEnums()
        {
            string name = "vItemEnums";
            string copyPath = "Assets/Invector-3rdPersonController/ItemManager/Scripts/vItemEnumsBuilder/" + name + ".cs";

            vItemEnumsList[] datas = Resources.LoadAll<vItemEnumsList>("");
            List<string> defaultItemTypeNames = new List<string>();
            List<string> defaultItemAttributesNames = new List<string>();
            List<string> _itemTypeNames = new List<string>();
            List<string> _itemAttributeNames = new List<string>();
            List<string> _itemTypeFormats = new List<string>();
            List<string> _itemAttributeFormats = new List<string>();
            #region Get all vItemType values of current Enum
            try
            {
                _itemTypeNames = Enum.GetNames(typeof(vItemType)).vToList();

            }
            catch
            {

            }
            #endregion

            #region Get all vItemAttributes values of current Enum
            try
            {
                _itemAttributeNames = Enum.GetNames(typeof(vItemAttributes)).vToList();
            }
            catch
            {

            }
            #endregion

            if (datas != null)
            {
                #region Get all enum of ItemEnumList
                for (int i = 0; i < datas.Length; i++)
                {
                    if (datas[i].itemTypeEnumValues != null)
                    {

                        for (int a = 0; a < datas[i].itemTypeEnumValues.Count; a++)
                        {
                            if (!string.IsNullOrEmpty(datas[i].itemTypeEnumValues[a]) && !defaultItemTypeNames.Contains(datas[i].itemTypeEnumValues[a]))
                            {

                                defaultItemTypeNames.Add(datas[i].itemTypeEnumValues[a]);
                            }
                        }
                    }
                    if (datas[i].itemAttributesEnumValues != null)
                    {
                        for (int a = 0; a < datas[i].itemAttributesEnumValues.Count; a++)
                        {
                            if (!string.IsNullOrEmpty(datas[i].itemAttributesEnumValues[a]) && !defaultItemAttributesNames.Contains(datas[i].itemAttributesEnumValues[a]))
                            {
                                defaultItemAttributesNames.Add(datas[i].itemAttributesEnumValues[a]);
                            }
                        }
                    }
                }

                foreach (string value in defaultItemTypeNames)
                {
                    if (!_itemTypeNames.Contains(value))
                    {
                        bool replace = false;
                        for (int i = 0; i < _itemTypeNames.Count; i++)
                        {
                            if (!defaultItemTypeNames.Contains(_itemTypeNames[i]))
                            {
                                replace = true;
                                _itemTypeNames[i] = value;
                                break;
                            }
                        }
                        if (!replace)
                            _itemTypeNames.Add(value);
                    }
                }
                #endregion

                #region Remove enum that not exist
                var typesToRemove = _itemTypeNames.FindAll(x => !defaultItemTypeNames.Contains(x));
                foreach (string value in typesToRemove)
                    _itemTypeNames.Remove(value);

                foreach (string value in defaultItemAttributesNames)
                {
                    if (!_itemAttributeNames.Contains(value))
                    {
                        bool replace = false;
                        for (int i = 0; i < _itemAttributeNames.Count; i++)
                        {
                            if (!defaultItemAttributesNames.Contains(_itemAttributeNames[i]))
                            {
                                replace = true;
                                _itemAttributeNames[i] = value;
                                break;
                            }
                        }
                        if (!replace)
                            _itemAttributeNames.Add(value);
                    }
                }
                var attributesToRemove = _itemAttributeNames.FindAll(x => !defaultItemAttributesNames.Contains(x));
                foreach (string value in attributesToRemove)
                    _itemAttributeNames.Remove(value);
                #endregion


                #region Get Enum Text Formats
                for(int i =0;i< _itemTypeNames.Count;i++)
                {
                    vItemEnumsList data = datas.vToList().Find(d => d.itemTypeEnumValues.Contains(_itemTypeNames[i]));
                    if(data!=null)
                    {
                        var index = data.itemTypeEnumValues.IndexOf(_itemTypeNames[i]);
                        if(index<data.itemTypeEnumFormats.Count)
                        {
                            _itemTypeFormats.Add(data.itemTypeEnumFormats[index]);
                        }
                        else
                            _itemTypeFormats.Add("");
                    }
                    else _itemTypeFormats.Add("");
                }

                for (int i = 0; i < _itemAttributeNames.Count; i++)
                {
                    vItemEnumsList data = datas.vToList().Find(d => d.itemAttributesEnumValues.Contains(_itemAttributeNames[i]));
                    if (data != null)
                    {
                        var index = data.itemAttributesEnumValues.IndexOf(_itemAttributeNames[i]);
                        if (index < data.itemAttributesEnumFormats.Count)
                        {
                            _itemAttributeFormats.Add(data.itemAttributesEnumFormats[index]);
                        }
                        else
                            _itemAttributeFormats.Add("");
                    }
                    else _itemAttributeFormats.Add("");
                }
                #endregion
            }
            CreateEnumClass(copyPath, _itemTypeNames, _itemAttributeNames,_itemTypeFormats,_itemAttributeFormats);
        }

        static void CreateEnumClass(string copyPath, List<string> itemTypes = null, List<string> itemAttributes = null,
                                                     List<string> itemTypesFormat = null, List<string> itemAttributesFormat = null)
        {
            if (File.Exists(copyPath)) File.Delete(copyPath);
            using (StreamWriter outfile = new StreamWriter(copyPath))
            {
                outfile.WriteLine("using System.ComponentModel;");
                outfile.WriteLine("namespace Invector.vItemManager {");
                outfile.WriteLine("     public enum vItemType {");
                if (itemTypes != null)
                {
                    for (int i = 0; i < itemTypes.Count; i++)
                    {
                        outfile.WriteLine("       "+string.Format("[Description(\"{0}\")] ",itemTypesFormat[i]) + itemTypes[i] + "=" + i + (i == itemTypes.Count - 1 ? "" : ","));
                    }
                }
                outfile.WriteLine("     }");
                outfile.WriteLine("     public enum vItemAttributes {");
                if (itemAttributes != null)
                {
                    for (int i = 0; i < itemAttributes.Count; i++)
                    {
                        outfile.WriteLine("       " + string.Format("[Description(\"{0}\")] ", itemAttributesFormat[i]) + itemAttributes[i] + "=" + i + (i == itemAttributes.Count - 1 ? "" : ","));
                    }
                }
                outfile.WriteLine("     }");
                outfile.WriteLine("}");
            }
            AssetDatabase.Refresh();

        }
    }
}