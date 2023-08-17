using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DuloGames.UI.Tweens;

namespace DuloGames.UI
{
	[ExecuteInEditMode, AddComponentMenu("UI/Highlight Transition")]
	public class UIHighlightTransition : MonoBehaviour, IEventSystemHandler, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
		public enum VisualState
		{
			Normal,
			Highlighted,
			Selected,
            Pressed,
            Active
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
		[SerializeField] private Color m_HighlightedColor = ColorBlock.defaultColorBlock.highlightedColor;
		[SerializeField] private Color m_SelectedColor = ColorBlock.defaultColorBlock.highlightedColor;
        [SerializeField] private Color m_PressedColor = ColorBlock.defaultColorBlock.pressedColor;
        [SerializeField] private Color m_ActiveColor = ColorBlock.defaultColorBlock.highlightedColor;
		[SerializeField] private float m_Duration = 0.1f;
		
		[SerializeField, Range(1f, 6f)] 
		private float m_ColorMultiplier = 1f;
		
		[SerializeField] private Sprite m_HighlightedSprite;
		[SerializeField] private Sprite m_SelectedSprite;
		[SerializeField] private Sprite m_PressedSprite;
        [SerializeField] private Sprite m_ActiveSprite;

        [SerializeField] private string m_NormalTrigger = "Normal";
		[SerializeField] private string m_HighlightedTrigger = "Highlighted";
		[SerializeField] private string m_SelectedTrigger = "Selected";
		[SerializeField] private string m_PressedTrigger = "Pressed";
        [SerializeField] private string m_ActiveBool = "Active";

        [SerializeField, Tooltip("Graphic that will have the selected transtion applied.")]
		private Graphic m_TargetGraphic;
		
		[SerializeField, Tooltip("GameObject that will have the selected transtion applied.")]
		private GameObject m_TargetGameObject;
		
        [SerializeField] private bool m_UseToggle = false;
        [SerializeField] private Toggle m_TargetToggle;

		private bool m_Highlighted = false;
		private bool m_Selected = false;
        private bool m_Pressed = false;
        private bool m_Active = false;

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
        protected UIHighlightTransition()
        {
            if (this.m_ColorTweenRunner == null)
                this.m_ColorTweenRunner = new TweenRunner<ColorTween>();

            this.m_ColorTweenRunner.Init(this);
        }

        protected void Awake()
        {
            if (this.m_UseToggle)
            {
                if (this.m_TargetToggle == null)
                    this.m_TargetToggle = this.gameObject.GetComponent<Toggle>();

                if (this.m_TargetToggle != null)
                    this.m_Active = this.m_TargetToggle.isOn;
            }
        }
        
        protected void OnEnable()
		{
            if (this.m_TargetToggle != null)
                this.m_TargetToggle.onValueChanged.AddListener(OnToggleValueChange);

            this.InternalEvaluateAndTransitionToNormalState(true);
		}
		
		protected void OnDisable()
		{
            if (this.m_TargetToggle != null)
                this.m_TargetToggle.onValueChanged.RemoveListener(OnToggleValueChange);

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
		
        protected void OnToggleValueChange(bool value)
        {
            if (!this.m_UseToggle || this.m_TargetToggle == null)
                return;
            
            this.m_Active = this.m_TargetToggle.isOn;
            
            if (this.m_Transition == Transition.Animation)
            {
                if (this.targetGameObject == null || this.animator == null || !this.animator.isActiveAndEnabled || this.animator.runtimeAnimatorController == null || !animator.hasBoundPlayables || string.IsNullOrEmpty(this.m_ActiveBool))
                    return;

                this.animator.SetBool(this.m_ActiveBool, this.m_Active);
            }
            
            this.DoStateTransition(this.m_Active ? VisualState.Active : 
                (this.m_Selected ? VisualState.Selected : 
                    (this.m_Highlighted ? VisualState.Highlighted : VisualState.Normal)), false);
        }

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
                    this.SetTextColor(this.m_NormalColor);
                    break;
            }
		}
		
		/// <summary>
		/// Internally evaluates and transitions to normal state.
		/// </summary>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		private void InternalEvaluateAndTransitionToNormalState(bool instant)
		{
			this.DoStateTransition(this.m_Active ? VisualState.Active : VisualState.Normal, instant);
		}
		
		public void OnSelect(BaseEventData eventData)
		{
			this.m_Selected = true;

            if (this.m_Active)
                return;

            this.DoStateTransition(VisualState.Selected, false);
		}
		
		public void OnDeselect(BaseEventData eventData)
		{
            this.m_Selected = false;

            if (this.m_Active)
                return;

            this.DoStateTransition((this.m_Highlighted ? VisualState.Highlighted : VisualState.Normal), false);
		}
		
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.m_Highlighted = true;
            
            if (!this.m_Selected && !this.m_Pressed && !this.m_Active)
				this.DoStateTransition(VisualState.Highlighted, false);
		}
		
		public void OnPointerExit(PointerEventData eventData)
		{
			this.m_Highlighted = false;
			
			if (!this.m_Selected && !this.m_Pressed && !this.m_Active)
				this.DoStateTransition(VisualState.Normal, false);
		}
        
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!this.m_Highlighted)
                return;

            this.m_Pressed = true;
            this.DoStateTransition(VisualState.Pressed, false);
        }
        
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            this.m_Pressed = false;

            VisualState newState = VisualState.Normal;

            if (this.m_Active)
            {
                newState = VisualState.Active;
            }
            else if (this.m_Selected)
            {
                newState = VisualState.Selected;
            }
            else if (this.m_Highlighted)
            {
                newState = VisualState.Highlighted;
            }

            this.DoStateTransition(newState, false);
        }

        /// <summary>
        /// Does the state transition.
        /// </summary>
        /// <param name="state">State.</param>
        /// <param name="instant">If set to <c>true</c> instant.</param>
        protected virtual void DoStateTransition(VisualState state, bool instant)
		{
			// Check if active in the scene
			if (!this.gameObject.activeInHierarchy)
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
				case VisualState.Highlighted:
					color = this.m_HighlightedColor;
					newSprite = this.m_HighlightedSprite;
					triggername = this.m_HighlightedTrigger;
					break;
				case VisualState.Selected:
					color = this.m_SelectedColor;
					newSprite = this.m_SelectedSprite;
					triggername = this.m_SelectedTrigger;
					break;
                case VisualState.Pressed:
                    color = this.m_PressedColor;
                    newSprite = this.m_PressedSprite;
                    triggername = this.m_PressedTrigger;
                    break;
                case VisualState.Active:
                    color = this.m_ActiveColor;
                    newSprite = this.m_ActiveSprite;
                    triggername = this.m_HighlightedTrigger;
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
			if (this.targetGameObject == null || this.animator == null || !this.animator.isActiveAndEnabled || this.animator.runtimeAnimatorController == null || !this.animator.hasBoundPlayables || string.IsNullOrEmpty(triggername))
				return;
            
            this.animator.ResetTrigger(this.m_HighlightedTrigger);
			this.animator.ResetTrigger(this.m_SelectedTrigger);
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
