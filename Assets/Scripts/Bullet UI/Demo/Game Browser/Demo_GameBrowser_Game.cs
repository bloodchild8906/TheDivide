using UnityEngine;
using UnityEngine.UI;

namespace DuloGames.UI
{
    [RequireComponent(typeof(Toggle))]
    [ExecuteInEditMode]
    public class Demo_GameBrowser_Game : MonoBehaviour
    {
        [Header("Background")]
        [SerializeField] private Image m_Background;
        [SerializeField] private Color m_BackgroundNormalColor = Color.white;
        [SerializeField] private Color m_BackgroundActiveColor = Color.white;
        [Header("Lock")]
        [SerializeField] private GameObject m_LockIcon;
        [SerializeField] private bool m_LockEnabled = false;
        [Header("Texts")]
        [SerializeField] private Text m_NameText;
        [SerializeField] private Color m_NameNormalColor = Color.white;
        [SerializeField] private Color m_NameActiveColor = Color.white;
        [SerializeField] private Text m_MapText;
        [SerializeField] private Color m_MapNormalColor = Color.white;
        [SerializeField] private Color m_MapActiveColor = Color.white;
        [SerializeField] private Text m_ModeText;
        [SerializeField] private Color m_ModeNormalColor = Color.white;
        [SerializeField] private Color m_ModeActiveColor = Color.white;
        [SerializeField] private Text m_PlayersText;
        [SerializeField] private Color m_PlayersNormalColor = Color.white;
        [SerializeField] private Color m_PlayersActiveColor = Color.white;
        [SerializeField] private Text m_PingText;
        [SerializeField] private Color m_PingNormalColor = Color.white;
        [SerializeField] private Color m_PingActiveColor = Color.white;
        [Header("Transition")]
        [SerializeField] private float m_TransitionDuration = 0.1f;
        
        [HideInInspector][SerializeField] private Toggle m_Toggle;
        
        public bool lockEnabled
        {
            get { return this.m_LockEnabled; }
            set {
                this.m_LockEnabled = value;

                if (this.m_LockIcon != null)
                    this.m_LockIcon.gameObject.SetActive(this.m_LockEnabled);
            }
        }

        protected void Awake()
        {
            if (this.m_Toggle == null) this.m_Toggle = this.gameObject.GetComponent<Toggle>();

            // Make sure we have toggle group
            if (this.m_Toggle.group == null)
            {
                // Try to find the group in the parents
                ToggleGroup grp = UIUtility.FindInParents<ToggleGroup>(this.gameObject);

                if (grp != null)
                {
                    this.m_Toggle.group = grp;
                }
                else
                {
                    // Add new group on the parent
                    this.m_Toggle.group = this.transform.parent.gameObject.AddComponent<ToggleGroup>();
                }
            }
        }

        protected void OnEnable()
        {
            // Hook an event listener
            this.m_Toggle.onValueChanged.AddListener(OnToggleStateChanged);

            // Apply initial state
            this.InternalEvaluateAndTransitionState(true);
        }

        protected void OnDisable()
        {
            // Unhook the event listener
            this.m_Toggle.onValueChanged.RemoveListener(OnToggleStateChanged);
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            this.m_TransitionDuration = Mathf.Max(this.m_TransitionDuration, 0f);

            if (this.enabled || this.gameObject.activeInHierarchy)
            {
                if (this.m_LockIcon != null)
                    this.m_LockIcon.gameObject.SetActive(this.m_LockEnabled);

                this.InternalEvaluateAndTransitionState(true);
            }
        }
#endif

        /// <summary>
		/// Raises the toggle state changed event.
		/// </summary>
		/// <param name="state">If set to <c>true</c> state.</param>
		protected void OnToggleStateChanged(bool state)
        {
            if (!this.enabled || !this.gameObject.activeInHierarchy)
                return;

            this.InternalEvaluateAndTransitionState(!Application.isPlaying);
        }

        /// <summary>
        /// Internaly evaluates and transitions to the current state.
        /// </summary>
        /// <param name="instant">If set to <c>true</c> instant.</param>
        private void InternalEvaluateAndTransitionState(bool instant)
        {
            if (!this.isActiveAndEnabled)
                return;

            // Toggle the content
            //this.EvaluateAndToggleContent();

            // Transition the active graphic children
            if (this.m_Toggle.graphic != null && this.m_Toggle.graphic.transform.childCount > 0)
            {
                float targetAlpha = (!this.m_Toggle.isOn) ? 0f : 1f;

                // Loop through the children
                foreach (Transform child in this.m_Toggle.graphic.transform)
                {
                    // Try getting a graphic component
                    Graphic g = child.GetComponent<Graphic>();

                    if (g != null)
                    {
                        if (!g.canvasRenderer.GetAlpha().Equals(targetAlpha))
                        {
                            if (instant) g.canvasRenderer.SetAlpha(targetAlpha);
                            else g.CrossFadeAlpha(targetAlpha, 0.1f, true);
                        }
                    }
                }
            }

            // Do a state transition
            this.DoStateTransition(instant);
        }

        /// <summary>
		/// Does the state transitioning.
		/// </summary>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		protected void DoStateTransition(bool instant)
        {
            if (!this.enabled || !this.gameObject.activeInHierarchy)
                return;
            
            // Check if active in the scene
            if (this.gameObject.activeInHierarchy)
            {
                float duration = (instant ? 0f : this.m_TransitionDuration);

                if (this.m_Background != null)
                    this.StartColorTween(this.m_Background, (this.m_Toggle.isOn ? this.m_BackgroundActiveColor : this.m_BackgroundNormalColor), duration);

                if (this.m_NameText != null)
                    this.StartColorTween(this.m_NameText, (this.m_Toggle.isOn ? this.m_NameActiveColor : this.m_NameNormalColor), duration);

                if (this.m_MapText != null)
                    this.StartColorTween(this.m_MapText, (this.m_Toggle.isOn ? this.m_MapActiveColor : this.m_MapNormalColor), duration);

                if (this.m_ModeText != null)
                    this.StartColorTween(this.m_ModeText, (this.m_Toggle.isOn ? this.m_ModeActiveColor : this.m_ModeNormalColor), duration);

                if (this.m_PlayersText != null)
                    this.StartColorTween(this.m_PlayersText, (this.m_Toggle.isOn ? this.m_PlayersActiveColor : this.m_PlayersNormalColor), duration);

                if (this.m_PingText != null)
                    this.StartColorTween(this.m_PingText, (this.m_Toggle.isOn ? this.m_PingActiveColor : this.m_PingNormalColor), duration);
            }
        }

        /// <summary>
		/// Starts a color tween.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="targetColor">Target color.</param>
		/// <param name="duration">Duration.</param>
		private void StartColorTween(Graphic target, Color targetColor, float duration)
        {
            if (target == null)
                return;

            if (!Application.isPlaying || duration == 0f)
            {
                target.canvasRenderer.SetColor(targetColor);
            }
            else
            {
                target.CrossFadeColor(targetColor, duration, true, true);
            }
        }
    }
}
