using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace DuloGames.UI
{
    public class UILoadScene : MonoBehaviour
    {
        enum InputKey
        {
            None,
            Submit,
            Cancel,
            Jump
        }
        
        [SerializeField] private string m_Scene;
        [SerializeField] private bool m_UseLoadingOverlay = false;
        [SerializeField] private InputKey m_InputKey = InputKey.None;
        [SerializeField] private Button m_HookToButton;

        protected void OnEnable()
        {
            if (this.m_HookToButton != null)
                this.m_HookToButton.onClick.AddListener(LoadScene);
        }

        protected void OnDisable()
        {
            if (this.m_HookToButton != null)
                this.m_HookToButton.onClick.RemoveListener(LoadScene);
        }

        public void LoadScene()
        {
            if (!string.IsNullOrEmpty(this.m_Scene))
            {
                int id;
                bool isNumeric = int.TryParse(this.m_Scene, out id);
                
                if (this.m_UseLoadingOverlay)
                {
                    UILoadingOverlay loadingOverlay = UILoadingOverlayManager.Instance.Create();

                    if (loadingOverlay != null)
                    {
                        if (isNumeric)
                            loadingOverlay.LoadScene(id);
                        else
                            loadingOverlay.LoadScene(this.m_Scene);
                    }
                }
                else
                {
                    if (isNumeric)
                        SceneManager.LoadScene(id);
                    else
                        SceneManager.LoadScene(this.m_Scene);
                }
            }
        }

        protected void Update()
        {
            if (!this.isActiveAndEnabled || !this.gameObject.activeInHierarchy || this.m_InputKey == InputKey.None)
                return;

            // Check if we are using the escape input for this and if the escape key was used in the window manager
            if (this.m_InputKey == InputKey.Cancel)
            {
                if (UIWindowManager.Instance != null && UIWindowManager.Instance.escapeInputName == "Cancel" && UIWindowManager.Instance.escapedUsed)
                    return;
            }

            string buttonName = string.Empty;

            switch (this.m_InputKey)
            {
                case InputKey.Submit:
                    buttonName = "Submit";
                    break;
                case InputKey.Cancel:
                    buttonName = "Cancel";
                    break;
                case InputKey.Jump:
                    buttonName = "Jump";
                    break;
            }

            if (!string.IsNullOrEmpty(buttonName) && Input.GetButtonDown(buttonName))
            {
                this.LoadScene();
            }
        }
    }
}
