using UnityEngine;
using System.Collections;

namespace DuloGames.UI
{
    public class Demo_KillDisplay_Remover : MonoBehaviour
    {
        private float m_Delay = 5f;

        /// <summary>
        /// Initialize the remover.
        /// </summary>
        /// <param name="delay">The auto remove delay.</param>
        public void Initialize(float delay)
        {
            this.m_Delay = delay;

            if (!Application.isPlaying)
                return;

            if (this.m_Delay > 0f)
            {
                this.StartCoroutine(WaitAndAnimate());
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        IEnumerator WaitAndAnimate()
        {
            yield return new WaitForSeconds(this.m_Delay);
            Destroy(this.gameObject);
        }
    }
}
