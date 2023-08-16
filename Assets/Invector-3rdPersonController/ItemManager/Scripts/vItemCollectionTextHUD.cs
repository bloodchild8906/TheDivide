using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Invector.vItemManager
{
    [vClassHeader("Item Collection HUD", helpBoxText = "Contains all behaviour to show messages sended")]
    public class vItemCollectionTextHUD : MonoBehaviour
    {
        public Text Message;
        /// <summary>
        /// Show a Message on the HUD
        /// </summary>
        /// <param name="message">message to show</param>
        /// <param name="timeToStay">time to stay showing</param>
        /// <param name="timeToFadeOut">time to fade out</param>
        public void Show(string message, float timeToStay = 1, float timeToFadeOut = 1)
        {
            Message.text = message;
            StartCoroutine(Timer(timeToStay, timeToFadeOut));
        }

        IEnumerator Timer(float timeToStay = 1, float timeToFadeOut = 1)
        {
            Message.CrossFadeAlpha(1, 0.5f, false);

            yield return new WaitForSeconds(timeToStay);
            Message.CrossFadeAlpha(0, timeToFadeOut, false);

            yield return new WaitForSeconds(timeToFadeOut + 0.1f);
            Destroy(gameObject);
        }

        private void Awake()
        {
            Clear();
        }

        /// <summary>
        /// Clear message text display
        /// </summary>
        /// 
        public void Clear()
        {
            Message.text = "";
            Message.CrossFadeAlpha(0, 0, false);
        }
    }
}