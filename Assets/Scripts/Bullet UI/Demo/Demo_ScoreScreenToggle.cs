using UnityEngine;
using System.Collections.Generic;

namespace DuloGames.UI
{
    public class Demo_ScoreScreenToggle : MonoBehaviour
    {
        private UIWindow m_Window;

        protected void Awake()
        {
            this.m_Window = this.gameObject.GetComponent<UIWindow>();
        }

        void Update()
        {
            if (this.m_Window == null)
                return;

            // Check if any other window is open
            List<UIWindow> windows = UIWindow.GetWindows();
            
            foreach (UIWindow window in windows)
            {
                if (window.IsOpen && window != this.m_Window)
                    return;
            }

            // Handle inputs
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                this.m_Window.Show();
            }
            else if (Input.GetKeyUp(KeyCode.Tab))
            {
                this.m_Window.Hide();
            }
        }
    }
}
