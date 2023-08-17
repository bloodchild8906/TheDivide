using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DuloGames.UI
{
    public class Demo_ExitButton : MonoBehaviour
    {
        [SerializeField] private Button m_Button;
        [SerializeField] private bool m_autoHook = true;

        protected void Awake()
        {
            if (this.m_Button == null)
                this.m_Button = this.gameObject.GetComponent<Button>();
        }

        protected void OnEnable()
        {
            if (this.m_Button != null && this.m_autoHook)
            {
                this.m_Button.onClick.AddListener(ExitGame);
            }
        }

        protected void OnDisable()
        {
            if (this.m_Button != null && this.m_autoHook)
            {
                this.m_Button.onClick.RemoveListener(ExitGame);
            }
        }

        public void ExitGame()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
