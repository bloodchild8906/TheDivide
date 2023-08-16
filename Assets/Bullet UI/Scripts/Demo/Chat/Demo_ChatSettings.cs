using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

namespace DuloGames.UI
{
    public class Demo_ChatSettings : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Demo_Chat m_Chat;
        [SerializeField] private UIWindow m_Window;
        [SerializeField] private float m_AutoHideTimer = 3f;

        protected void OnEnable()
        {
            if (this.m_Window != null)
            {
                this.m_Window.onTransitionComplete.AddListener(OnTransitionComplete);
            }
        }

        protected void OnDisable()
        {
            if (this.m_Window != null)
            {
                this.m_Window.onTransitionComplete.RemoveListener(OnTransitionComplete);
            }
        }

        public void OnTransitionComplete(UIWindow window, UIWindow.VisualState state)
        {
            if (state == UIWindow.VisualState.Shown)
            {
                StartCoroutine("AutoHide");
            }
            else
            {
                StopCoroutine("AutoHide");
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            StopCoroutine("AutoHide");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartCoroutine("AutoHide");
        }

        IEnumerator AutoHide()
        {
            yield return new WaitForSeconds(this.m_AutoHideTimer);
            this.Hide();
        }

        public void Hide()
        {
            if (this.m_Window != null)
            {
                this.m_Window.Hide();
            }
        }
    }
}
