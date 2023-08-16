using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Invector.vItemManager
{
    public class vAmmoDisplay : MonoBehaviour
    {
        public int displayID = 1;
        [System.Serializable]
        public class OnChangeAmmoEvent : UnityEvent<int> { }

        [SerializeField]
        [vHelpBox("Ammo loaded in the Clip")]
        protected Text display;
        [SerializeField]
        [vHelpBox("Ammo left in the Inventory")]
        protected Text secundaryDisplay;

        public UnityEvent onShow, onHide;

        [vHelpBox("Event based in the current AmmoID")]
        public OnChangeAmmoEvent onChangeAmmo;
        private int currentAmmoId;

        void Start()
        {
            if (display == null)
            {
                Destroy(gameObject);
            }

            display.text = "";
            if (secundaryDisplay)
            {
                secundaryDisplay.text = "";
            }

            currentAmmoId = -1;
        }

        public void Show()
        {
            if (display)
            {
                display.gameObject.SetActive(true);
            }

            if (secundaryDisplay)
            {
                secundaryDisplay.gameObject.SetActive(true);
            }

            onShow.Invoke();
        }

        public void Hide()
        {
            if (display)
            {
                display.gameObject.SetActive(false);
            }

            if (secundaryDisplay)
            {
                secundaryDisplay.gameObject.SetActive(true);
            }

            onHide.Invoke();
        }

        public void UpdateDisplay(string text1, string text2 = "", int id = 0)
        {
            if (display && !text1.Equals("") && !display.gameObject.activeSelf)
            {
                display.gameObject.SetActive(true);
            }
            if (secundaryDisplay && !text2.Equals("") && !secundaryDisplay.gameObject.activeSelf)
            {
                secundaryDisplay.gameObject.SetActive(true);
            }
            if (currentAmmoId != id)
            {
                onChangeAmmo.Invoke(id);
                currentAmmoId = id;
            }

            if (display)
            {
                display.text = text1;
            }

            if (secundaryDisplay)
            {
                secundaryDisplay.text = text2;
            }
        }
    }
}