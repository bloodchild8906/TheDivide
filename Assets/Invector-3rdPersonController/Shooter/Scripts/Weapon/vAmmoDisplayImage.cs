using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
namespace Invector.vItemManager
{
    public class vAmmoDisplayImage : MonoBehaviour
    {
        [System.Serializable]
        public class vDisplayImage
        {
            public Sprite ammoImage;
            public int ammoId;
        }

        public Image displayImage;
        public Sprite defaultAmmoImage;
        public List<vDisplayImage> displayImages = new List<vDisplayImage>();

        private int currentAmmoId;

        /// <summary>
        /// Change Ammo display image by id
        /// </summary>
        /// <param name="id"></param>
        public void ChangeAmmoDisplayImage(int id)
        {
            if (currentAmmoId != id && displayImages != null)
            {
                var display = displayImages.Find(d => d.ammoId.Equals(id));
                if (display != null)
                {
                    displayImage.sprite = display.ammoImage;
                }
                else
                {
                    displayImage.sprite = defaultAmmoImage;
                }
                currentAmmoId = id;
            }
        }
    }
}
