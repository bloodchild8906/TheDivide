using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.Utils
{
    public class vScaleControl : MonoBehaviour
    {

        Vector3 targetScale;
        Vector3 defaultScale;
        private void Awake()
        {
            defaultScale = transform.localScale;
            targetScale = defaultScale;

        }
        public float scaleX
        {
            set
            {
                targetScale.x = defaultScale.x * value;

                transform.localScale = targetScale;
            }
        }
        public float scaleY
        {
            set
            {
                targetScale.y = defaultScale.y * value;

                transform.localScale = targetScale;
            }
        }
        public float scaleZ
        {
            set
            {
                targetScale.z = defaultScale.z * value;

                transform.localScale = targetScale;
            }
        }
    }
}