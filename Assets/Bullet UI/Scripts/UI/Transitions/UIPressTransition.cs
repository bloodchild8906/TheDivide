using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DuloGames.UI.Tweens;

namespace DuloGames.UI
{
    [ExecuteInEditMode, AddComponentMenu("UI/Press Transition")]
    public class UIPressTransition : MonoBehaviour, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler 
    {

        public enum VisualState
        {
            Normal,
            Pressed
        }

        public enum Transition
        {
            None,
            ColorTint,
            SpriteSwap,
            Animation,
            TextColor
        }

        [SerializeField] private Transition m_Transition = Transition.None;

        [SerializeField] private Color m_NormalColor = ColorBlock.defaultColorBlock.normalColor;
        [SerializeField] private Color m_PressedColor = ColorBlock.defaultColorBlock.pressedColor;
        [SerializeField] private float m_Duration = 0.1f;

        [SerializeField, Range(1f, 6f)] private float m_ColorMultiplier = 1f;

        [SerializeField] private Sprite m_PressedSprite;

        [SerializeField] private string m_NormalTrigger = "Normal";
        [SerializeField] private string m_PressedTrigger = "Pressed";

        [SerializeField, Tooltip("Graphic that will have the selected transtion applied.")]
        private Graphic m_TargetGraphic;

        [SerializeField, Tooltip("GameObject that will have the selected transtion applied.")]
        private GameObject m_TargetGameObject;
        
        /// <summary>
        /// Gets or sets the transition type.
        /// </summary>
        /// <value>The transition.</value>
        public Transition transition
        {
            get
            {
                return this.m_Transition;
            }
            set
            {
                this.m_Transition = value;
            }
        }

        /// <summary>
        /// Gets or sets the target graphic.
        /// </summary>
        /// <value>The target graphic.</value>
        public Graphic targetGraphic
        {
            get
            {
                return this.m_TargetGraphic;
            }
            set
            {
                this.m_TargetGraphic = value;
            }
        }

        /// <summary>
        /// Gets or sets the target game object.
        /// </summary>
        /// <value>The target game object.</value>
        public GameObject targetGameObject
        {
            get
            {
                return this.m_TargetGameObject;
            }
            set
            {
                this.m_TargetGameObject = value;
            }
        }

        /// <summary>
        /// Gets the animator.
        /// </summary>
        /// <value>The animator.</value>
        public Animator animator
        {
            get
            {
                if (this.m_TargetGameObject != null)
                    return this.m_TargetGameObject.GetComponent<Animator>();

                // Default
                return null;
            }
        }

        // Tween controls
        [System.NonSerialized]
        private readonly TweenRunner<ColorTween> m_ColorTweenRunner;

        // Called by Unity prior to deserialization, 
        // should not be called by users
        protected UIPressTransition()
        {
            if (this.m_ColorTweenRunner == null)
                this.m_ColorTweenRunner = new TweenRunner<ColorTween>();

            this.m_ColorTweenRunner.Init(this);
        }

        protected void OnEnable()
        {
            this.InternalEvaluateAndTransitionToNormalState(true);
        }

        protected void OnDisable()
        {
            this.InstantClearState();
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            this.m_Duration = Mathf.Max(this.m_Duration, 0f);

            if (this.isActiveAndEnabled)
            {
                this.DoSpriteSwap(null);
                this.InternalEvaluateAndTransitionToNormalState(true);
            }
        }
#endif

        /// <summary>
        /// Instantly clears the visual state.
        /// </summary>
        protected void InstantClearState()
        {
            switch (this.m_Transition)
            {
                case Transition.ColorTint:
                    this.StartColorTween(Color.white, true);
                    break;
                case Transition.SpriteSwap:
                    this.DoSpriteSwap(null);
                    break;
                case Transition.Animation:
                    this.TriggerAnimation(this.m_NormalTrigger);
                    break;
                case Transition.TextColor:
                    this.SetTextColor(Color.white);
                    break;
            }
        }

        /// <summary>
        /// Internally evaluates and transitions to normal state.
        /// </summary>
        /// <param name="instant">If set to <c>true</c> instant.</param>
        private void InternalEvaluateAndTransitionToNormalState(bool instant)
        {
            this.DoStateTransition(VisualState.Normal, instant);
        }
        
        /// <summary>
		/// Raises the pointer down event.
		/// </summary>
		/// <param name="eventData">Event data.</param>
		public virtual void OnPointerDown(PointerEventData eventData)
        {
            this.DoStateTransition(VisualState.Pressed, false);
        }

        /// <summary>
        /// Raises the pointer up event.
        /// </summary>
        /// <param name="eventData">Event data.</param>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            this.DoStateTransition(VisualState.Normal, false);
        }

        /// <summary>
        /// Does the state transition.
        /// </summary>
        /// <param name="state">State.</param>
        /// <param name="instant">If set to <c>true</c> instant.</param>
        protected virtual void DoStateTransition(VisualState state, bool instant)
        {
            // Check if the script is enabled
            if (!this.enabled || !this.gameObject.activeInHierarchy)
                return;

            Color color = this.m_NormalColor;
            Sprite newSprite = null;
            string triggername = this.m_NormalTrigger;

            // Prepare the transition values
            switch (state)
            {
                case VisualState.Normal:
                    color = this.m_NormalColor;
                    newSprite = null;
                    triggername = this.m_NormalTrigger;
                    break;
                case VisualState.Pressed:
                    color = this.m_PressedColor;
                    newSprite = this.m_PressedSprite;
                    triggername = this.m_PressedTrigger;
                    break;
            }

            // Do the transition
            switch (this.m_Transition)
            {
                case Transition.ColorTint:
                    this.StartColorTween(color * this.m_ColorMultiplier, instant);
                    break;
                case Transition.SpriteSwap:
                    this.DoSpriteSwap(newSprite);
                    break;
                case Transition.Animation:
                    this.TriggerAnimation(triggername);
                    break;
                case Transition.TextColor:
                    this.StartTextColorTween(color, false);
                    break;
            }
        }

        /// <summary>
        /// Starts the color tween.
        /// </summary>
        /// <param name="targetColor">Target color.</param>
        /// <param name="instant">If set to <c>true</c> instant.</param>
        private void StartColorTween(Color targetColor, bool instant)
        {
            if (this.m_TargetGraphic == null)
                return;

            if (instant || this.m_Duration == 0f || !Application.isPlaying)
            {
                this.m_TargetGraphic.canvasRenderer.SetColor(targetColor);
            }
            else
            {
                this.m_TargetGraphic.CrossFadeColor(targetColor, this.m_Duration, true, true);
            }
        }

        private void DoSpriteSwap(Sprite newSprite)
        {
            Image image = this.m_TargetGraphic as Image;

            if (image == null)
                return;

            image.overrideSprite = newSprite;
        }

        private void TriggerAnimation(string triggername)
        {
            if (this.targetGameObject == null)
                return;

            if (this.animator == null || !this.animator.enabled || !this.animator.isActiveAndEnabled || this.animator.runtimeAnimatorController == null || !this.animator.hasBoundPlayables || string.IsNullOrEmpty(triggername))
                return;

            this.animator.ResetTrigger(this.m_PressedTrigger);
            this.animator.SetTrigger(triggername);
        }

        private void StartTextColorTween(Color targetColor, bool instant)
        {
            if (this.m_TargetGraphic == null)
                return;

            if ((this.m_TargetGraphic is Text) == false)
                return;

            if (instant || this.m_Duration == 0f || !Application.isPlaying)
            {
                (this.m_TargetGraphic as Text).color = targetColor;
            }
            else
            {
                var colorTween = new ColorTween { duration = this.m_Duration, startColor = (this.m_TargetGraphic as Text).color, targetColor = targetColor };
                colorTween.AddOnChangedCallback(SetTextColor);
                colorTween.ignoreTimeScale = true;

                this.m_ColorTweenRunner.StartTween(colorTween);
            }
        }

        /// <summary>
		/// Sets the text color.
		/// </summary>
		/// <param name="targetColor">Target color.</param>
		private void SetTextColor(Color targetColor)
        {
            if (this.m_TargetGraphic == null)
                return;

            if (this.m_TargetGraphic is Text)
            {
                (this.m_TargetGraphic as Text).color = targetColor;
            }
        }
    }
}
