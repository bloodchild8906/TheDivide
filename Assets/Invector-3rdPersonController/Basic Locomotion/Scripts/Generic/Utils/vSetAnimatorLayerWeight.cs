using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.Utils
{
    public class vSetAnimatorLayerWeight : MonoBehaviour
    {
        public Animator animator;
        public int layer;
        public float weight;
        public float speed;
        [Invector.vReadOnly]
        protected float currentWeight;
        private void Update()
        {
            currentWeight = Mathf.Lerp(currentWeight, weight, speed * Time.deltaTime);
            animator.SetLayerWeight(layer, currentWeight);
        }
        public void SetWeight(float value)
        {
            weight = value;
        }
    }
}