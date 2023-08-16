using UnityEngine;
using UnityEngine.UI;
namespace Invector
{
    public class vTutorialTextTrigger : MonoBehaviour
    {
        [TextAreaAttribute(5, 3000), Multiline]
        public string text;
        public Text _textUI;
        public GameObject painel;

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                EnableTutorialPanel();
            }
        }

        public virtual void EnableTutorialPanel()
        {
            painel.SetActive(true);
            _textUI.gameObject.SetActive(true);
            _textUI.text = text;
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                DisableTutorialPanel();
            }
        }

        public virtual void DisableTutorialPanel()
        {
            painel.SetActive(false);
            _textUI.gameObject.SetActive(false);
            _textUI.text = " ";
        }
    }
}