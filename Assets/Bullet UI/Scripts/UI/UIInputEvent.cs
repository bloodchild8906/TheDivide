using UnityEngine;
using UnityEngine.Events;

namespace DuloGames.UI
{
    public class UIInputEvent : MonoBehaviour
    {
        [SerializeField] private string m_InputName;

        [SerializeField] private UnityEvent m_OnButton;
        [SerializeField] private UnityEvent m_OnButtonDown;
        [SerializeField] private UnityEvent m_OnButtonUp;

        protected void Update()
        {
            if (!this.isActiveAndEnabled || !this.gameObject.activeInHierarchy || string.IsNullOrEmpty(this.m_InputName))
                return;

            if (Input.GetButton(this.m_InputName))
                this.m_OnButton.Invoke();

            if (Input.GetButtonDown(this.m_InputName))
                this.m_OnButtonDown.Invoke();

            if (Input.GetButtonUp(this.m_InputName))
                this.m_OnButtonUp.Invoke();
        }
    }
}
