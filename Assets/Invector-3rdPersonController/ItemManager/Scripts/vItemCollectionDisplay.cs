using UnityEngine;

namespace Invector.vItemManager
{
    [vClassHeader("Item Collected Display", helpBoxText = "Use this to display the name of collected items", openClose = false)]
    public class vItemCollectionDisplay : vMonoBehaviour
    {
        private static vItemCollectionDisplay instance;
        /// <summary>
        /// Instance of the <seealso cref="vItemCollectionDisplay"/>
        /// </summary>
        /// 
        public static vItemCollectionDisplay Instance
        {
            get
            {
                if (instance == null) { instance = GameObject.FindObjectOfType<vItemCollectionDisplay>(); }
                return vItemCollectionDisplay.instance;
            }
        }

        public vItemCollectionTextHUD itemCollectedDiplayPrefab;

        /// <summary>
        /// Send text to display in the HUD with fade in <seealso cref="vItemCollectionTextHUD"/>
        /// </summary>
        /// <param name="message">message to show</param>
        /// <param name="timeToStay">time to stay showing</param>
        /// <param name="timeToFadeOut">time to fade out</param>        
        public virtual void FadeText(string message, float timeToStay, float timeToFadeOut)
        {
            var display = Instantiate(itemCollectedDiplayPrefab);
            display.transform.SetParent(transform, false);
            display.transform.SetAsFirstSibling();
            display.Show(message, timeToStay, timeToFadeOut);
        }
    }
}

