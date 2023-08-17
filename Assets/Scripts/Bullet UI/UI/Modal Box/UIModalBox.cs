using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace DuloGames.UI
{
    [RequireComponent(typeof(UIWindow)), RequireComponent(typeof(UIAlwaysOnTop))]
    public class UIModalBox : MonoBehaviour
    {
        [Header("Texts")]
        [SerializeField] private Text m_Text1;
        [SerializeField] private Text m_Text2;
        [Header("Buttons")]
        [SerializeField] private Button m_ConfirmButton;
        [SerializeField] private Text m_ConfirmButtonText;
        [SerializeField] private Button m_CancelButton;
        [SerializeField] private Text m_CancelButtonText;
        [Header("Inputs")]
        [SerializeField] private string m_ConfirmInput = "Submit";
        [SerializeField] private string m_CancelInput = "Cancel";

        private UIWindow m_Window;
        private bool m_IsActive = false;

        [Header("Events")]
        public UnityEvent onConfirm = new UnityEvent();
        public UnityEvent onCancel = new UnityEvent();

        /// <summary>
        /// Gets a value indicating whether this modal box is active.
        /// </summary>
        public bool isActive
        {
            get { return this.m_IsActive; }
        }

        protected void Awake()
        {
            // Make sure we have the window component
            if (this.m_Window == null)
            {
                this.m_Window = this.gameObject.GetComponent<UIWindow>();
            }

            // Prepare some window parameters
            this.m_Window.ID = UIWindowID.ModalBox;
            this.m_Window.escapeKeyAction = UIWindow.EscapeKeyAction.None;

            // Hook an event to the window
            this.m_Window.onTransitionComplete.AddListener(OnWindowTransitionEnd);

            // Prepare the always on top component
            UIAlwaysOnTop aot = this.gameObject.GetComponent<UIAlwaysOnTop>();
            aot.order = UIAlwaysOnTop.ModalBoxOrder;

            // Hook the button click event
            if (this.m_ConfirmButton != null)
            {
                this.m_ConfirmButton.onClick.AddListener(Confirm);
            }

            if (this.m_CancelButton != null)
            {
                this.m_CancelButton.onClick.AddListener(Close);
            }
        }

        protected void Update()
        {
            if (!string.IsNullOrEmpty(this.m_CancelInput) && Input.GetButtonDown(this.m_CancelInput))
                this.Close();
            
            if (!string.IsNullOrEmpty(this.m_ConfirmInput) && Input.GetButtonDown(this.m_ConfirmInput))
                this.Confirm();
        }

        /// <summary>
        /// Sets the text on the first line.
        /// </summary>
        /// <param name="text"></param>
        public void SetText1(string text)
        {
            if (this.m_Text1 != null)
            {
                this.m_Text1.text = text;
                this.m_Text1.gameObject.SetActive(!string.IsNullOrEmpty(text));
            }
        }

        /// <summary>
        /// Sets the text on the second line.
        /// </summary>
        /// <param name="text"></param>
        public void SetText2(string text)
        {
            if (this.m_Text2 != null)
            {
                this.m_Text2.text = text;
                this.m_Text2.gameObject.SetActive(!string.IsNullOrEmpty(text));
            }
        }

        /// <summary>
        /// Sets the confirm button text.
        /// </summary>
        /// <param name="text">The confirm button text.</param>
        public void SetConfirmButtonText(string text)
        {
            if (this.m_ConfirmButtonText != null)
            {
                this.m_ConfirmButtonText.text = text;
            }
        }

        /// <summary>
        /// Sets the cancel button text.
        /// </summary>
        /// <param name="text">The cancel button text.</param>
        public void SetCancelButtonText(string text)
        {
            if (this.m_CancelButtonText != null)
            {
                this.m_CancelButtonText.text = text;
            }
        }

        /// <summary>
        /// Shows the modal box.
        /// </summary>
        public void Show()
        {
            this.m_IsActive = true;

            // Show the modal
            if (this.m_Window != null)
            {
                this.m_Window.Show();
            }
        }

        /// <summary>
        /// Closes the modal box.
        /// </summary>
        public void Close()
        {
            this.m_IsActive = false;

            // Hide the modal
            if (this.m_Window != null)
            {
                this.m_Window.Hide();
            }

            // Invoke the cancel event
            if (this.onCancel != null)
            {
                this.onCancel.Invoke();
            }
        }

        public void Confirm()
        {
            this.m_IsActive = false;

            // Hide the modal
            if (this.m_Window != null)
            {
                this.m_Window.Hide();
            }

            // Invoke the confirm event
            if (this.onConfirm != null)
            {
                this.onConfirm.Invoke();
            }
        }

        public void OnWindowTransitionEnd(UIWindow window, UIWindow.VisualState state)
        {
            // Destroy the modal box when hidden
            if (state == UIWindow.VisualState.Hidden)
            {
                Destroy(this.gameObject);
            }
        }
    }
}
