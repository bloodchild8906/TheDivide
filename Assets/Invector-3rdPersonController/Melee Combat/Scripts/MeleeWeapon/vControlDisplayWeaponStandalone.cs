using System.Collections;
using UnityEngine;
namespace Invector
{
    public class vControlDisplayWeaponStandalone : MonoBehaviour
    {
        [SerializeField]
        protected vDisplayWeaponStandalone leftDisplay, rightDisplay;

        #region Control Left Display

        public virtual void SetLeftWeaponIcon(Sprite icon)
        {
            if (!leftDisplay)
            {
                return;
            }

            leftDisplay.SetWeaponIcon(icon);
        }

        public virtual void SetLeftWeaponText(string text)
        {
            if (!leftDisplay)
            {
                return;
            }

            leftDisplay.SetWeaponText(text);
        }

        public virtual void RemoveLeftWeaponIcon()
        {
            if (!leftDisplay)
            {
                return;
            }

            leftDisplay.RemoveWeaponIcon();
        }

        public virtual void RemoveLeftWeaponText()
        {
            if (!leftDisplay)
            {
                return;
            }

            leftDisplay.RemoveWeaponText();
        }

        #endregion

        #region Control Right Display

        public virtual void SetRightWeaponIcon(Sprite icon)
        {
            if (!rightDisplay)
            {
                return;
            }

            rightDisplay.SetWeaponIcon(icon);
        }

        public virtual void SetRightWeaponText(string text)
        {
            if (!rightDisplay)
            {
                return;
            }

            rightDisplay.SetWeaponText(text);
        }

        public virtual void RemoveRightWeaponIcon()
        {
            if (!rightDisplay)
            {
                return;
            }

            rightDisplay.RemoveWeaponIcon();
        }

        public virtual void RemoveRightWeaponText()
        {
            if (!rightDisplay)
            {
                return;
            }

            rightDisplay.RemoveWeaponText();

        }

        #endregion
    }
}