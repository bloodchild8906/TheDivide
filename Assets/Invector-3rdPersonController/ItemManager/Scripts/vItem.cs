using System.Collections.Generic;
using UnityEngine;

namespace Invector.vItemManager
{
    [System.Serializable]
    public partial class vItem : ScriptableObject
    {
        #region SerializedProperties in customEditor

        [HideInInspector]
        public int id;
        [HideInInspector]
        public string description = "Item Description";
        [HideInInspector]
        public vItemType type;
        [HideInInspector]
        public Sprite icon;
        [HideInInspector]
        public bool stackable = true;
        [HideInInspector]
        public int maxStack;
        [HideInInspector]
        public int amount;
        [HideInInspector]
        public GameObject originalObject;
        [HideInInspector]
        public GameObject dropObject;
        [HideInInspector]
        public List<vItemAttribute> attributes = new List<vItemAttribute>();
        [HideInInspector]
        public bool isInEquipArea;
        [HideInInspector]
        public bool isEquiped;
        #endregion

        #region Properties in defaultInspector

        public bool destroyAfterUse = true;
        public bool canBeEquipped = true;
        public bool canBeUsed = true;
        public bool canBeDroped = true;
        public bool canBeDestroyed = true;

        [Header("Animation Settings")]
        [vHelpBox("Triggers a animation when Equipping a Weapon or enabling item.\nYou can also trigger an animation if the ItemType is a Consumable")]
        public string EnableAnim = "LowBack";
        [vHelpBox("Triggers a animation when Unequipping a Weapon or disable item")]
        public string DisableAnim = "LowBack";
        [vHelpBox("Delay to enable the Weapon/Item object when Equipping\n If ItemType is a Consumable use this to delay the item usage.")]
        public float enableDelayTime = 0.5f;
        [vHelpBox("Delay to hide the Weapon/Item object when Unequipping")]
        public float disableDelayTime = 0.5f;
        [vHelpBox("If the item is equippable use this to set a custom handler to instantiate the SpawnObject")]
        public string customHandler;
        [vHelpBox("If the item is equippable and need to use two hand\n<color=yellow><b>This option makes it impossible to equip two items</b></color>")]
        public bool twoHandWeapon;

        [HideInInspector]
        public OnHandleItemEvent onDestroy;
        #endregion

        public void OnDestroy()
        {
            onDestroy.Invoke(this);
        }

        /// <summary>
        /// Convert Sprite icon to texture
        /// </summary>
        public Texture2D iconTexture
        {
            get
            {
                if (!icon) return null;
                try
                {
                    if (icon.rect.width != icon.texture.width || icon.rect.height != icon.texture.height)
                    {
                        Texture2D newText = new Texture2D((int)icon.textureRect.width, (int)icon.textureRect.height);
                        newText.name = icon.name;
                        Color[] newColors = icon.texture.GetPixels((int)icon.textureRect.x, (int)icon.textureRect.y, (int)icon.textureRect.width, (int)icon.textureRect.height);
                        newText.SetPixels(newColors);
                        newText.Apply();
                        return newText;
                    }
                    else
                        return icon.texture;
                }
                catch
                {
                    Debug.LogWarning("Icon texture of the " + name + " is not Readable", icon.texture);
                    return icon.texture;
                }
            }
        }

        /// <summary>
        /// Get the Item Attribute via <seealso cref="vItemAttribute"/>
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public vItemAttribute GetItemAttribute(vItemAttributes attribute)
        {
            if (attributes != null) return attributes.Find(_attribute => _attribute.name == attribute);
            return null;
        }

        /// <summary>
        /// Get the Item Attribute via string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public vItemAttribute GetItemAttribute(string name)
        {
            if (attributes != null)
                return attributes.Find(attribute => attribute.name.ToString().Equals(name));
            return null;
        }

        /// <summary>
        /// Get Selected Item Attributes via <seealso cref="vItemAttribute"/> by ignoring the ones you don't want
        /// </summary>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public string GetItemAttributesText(List<vItemAttributes> ignore = null)
        {
            System.Text.StringBuilder text = new System.Text.StringBuilder();
            for (int i = 0; i < attributes.Count; i++)
            {
                if (ignore == null || !ignore.Contains(attributes[i].name))
                    text.AppendLine(GetItemAttributeText(i));
            }
            return text.ToString();
        }

        /// <summary>
        /// Get Item Attribute Text 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        protected string GetItemAttributeText(int i)
        {
            if (attributes.Count > 0 && i < attributes.Count)
            {
                if (attributes.Count > 0 && i < attributes.Count)
                {
                    return attributes[i].GetDisplayText();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Get Item Attribut Text with a custom Format to display
        /// </summary>
        /// <param name="i"></param>
        /// <param name="customFormat"></param>
        /// <returns></returns>
        protected string GetItemAttributeText(int i, string customFormat)
        {
            if (attributes.Count > 0 && i < attributes.Count)
            {
                return attributes[i].GetDisplayText(customFormat);
            }
            return string.Empty;
        }

        /// <summary>
        /// Get Default Item type text  
        /// </summary>
        /// <returns></returns>
        public string ItemTypeText()
        {
            return ItemTypeText(type.DisplayFormat());
        }

        /// <summary>
        /// Get Custom Item type text
        /// </summary>
        /// <param name="format"> Custom format for text </param>
        /// <returns></returns>
        public string ItemTypeText(string format)
        {
            var _text = format;
            var value = type.ToString().InsertSpaceBeforeUpperCase().RemoveUnderline();
            if (string.IsNullOrEmpty(_text))
                return value;
            else if (_text.Contains("(NAME)")) _text.Replace("(NAME)", value);
            return _text;
        }

        /// <summary>
        /// Get Item Full Description text including item Name, Type, Description and Attributes
        /// </summary>
        /// <param name="format">Custom format</param>
        /// <param name="ignoreAttributes">Attributes to ignore</param>
        /// <returns></returns>
        public string GetFullItemDescription(string format = null, List<vItemAttributes> ignoreAttributes = null)
        {
            string text = "";
            if (string.IsNullOrEmpty(format))
            {
                text += (name);
                text += "\n" + (ItemTypeText());
                text += "\n" + (description);
                text += "\n" + (GetItemAttributesText());
            }
            else
            {
                text = format;
                if (text.Contains("(NAME)")) text = text.Replace("(NAME)", name);
                if (text.Contains("(TYPE)")) text = text.Replace("(TYPE)", ItemTypeText());
                if (text.Contains("(DESC)")) text = text.Replace("(DESC)", description);
                if (text.Contains("(ATTR)")) text = text.Replace("(ATTR)", GetItemAttributesText(ignoreAttributes));
            }
            return text;
        }
    }
}