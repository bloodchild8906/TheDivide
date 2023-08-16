using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace Invector
{
    public class vDisplayWeaponStandalone : MonoBehaviour
    {
        [Header("Weapon Display source")]
        public Image weaponIcon;
        public Text weaponText;
        [Header("Weapon unarmed sources")]
        public Sprite defaultIcon;
        public string defaultText;

        protected virtual void Start()
        {
            RemoveWeaponIcon();
            RemoveWeaponText();
        }

        public virtual void SetWeaponIcon(Sprite icon)
        {
            if (!weaponIcon) return;
            weaponIcon.sprite = icon;
            if (!weaponIcon.gameObject.activeSelf)
                weaponIcon.gameObject.SetActive(true);
        }

        public virtual void SetWeaponText(string text)
        {
            if (!weaponText) return;
            weaponText.text = text;
            if (!weaponText.gameObject.activeSelf)
                weaponText.gameObject.SetActive(true);
        }

        public virtual void RemoveWeaponIcon()
        {
            if (!weaponIcon) return;
            weaponIcon.sprite = defaultIcon;
            if (weaponIcon.gameObject.activeSelf && weaponIcon.sprite == null)
                weaponIcon.gameObject.SetActive(false);
        }

        public virtual void RemoveWeaponText()
        {
            if (!weaponText) return;
            weaponText.text = defaultText;
        }

    }
}