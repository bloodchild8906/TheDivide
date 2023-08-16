using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Invector
{
    public class vImageColorChange : MonoBehaviour
    {
        public Image image;
        public Color[] colors;

        public void ChangeColor(int colorIndex)
        {
            if (colors.Length > 0 && colorIndex < colors.Length)
            {
                image.color = colors[colorIndex];
            }
        }
    }
}