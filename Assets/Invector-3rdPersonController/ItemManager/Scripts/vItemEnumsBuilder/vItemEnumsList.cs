using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Invector.vItemManager
{
    public class vItemEnumsList : ScriptableObject
    {
        [SerializeField,HideInInspector]
        public List<string> itemTypeEnumValues = new List<string>();
        [SerializeField, HideInInspector]
        public List<string> itemAttributesEnumValues = new List<string>();

        [SerializeField, HideInInspector]
        public List<string> itemTypeEnumFormats = new List<string>();
        [SerializeField, HideInInspector]
        public List<string> itemAttributesEnumFormats = new List<string>();


    }
}
