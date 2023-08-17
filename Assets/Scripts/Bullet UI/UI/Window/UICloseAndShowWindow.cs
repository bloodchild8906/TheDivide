using UnityEngine;

namespace DuloGames.UI
{
    public class UICloseAndShowWindow : MonoBehaviour
    {
        [SerializeField] private UIWindow m_CloseWindow;
        [SerializeField] private UIWindow m_ShowWindow;

        public void CloseAndShow()
        {
            if (this.m_CloseWindow != null) this.m_CloseWindow.Hide();
            if (this.m_ShowWindow != null) this.m_ShowWindow.Show();
        }
    }
}
