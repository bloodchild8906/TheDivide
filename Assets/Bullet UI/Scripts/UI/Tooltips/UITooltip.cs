using UnityEngine;
using UnityEngine.UI;
using DuloGames.UI.Tweens;
using System;

namespace DuloGames.UI
{
	[DisallowMultipleComponent, AddComponentMenu("UI/Tooltip", 58)]
    [RequireComponent(typeof(RectTransform)), RequireComponent(typeof(CanvasGroup)), RequireComponent(typeof(VerticalLayoutGroup)), RequireComponent(typeof(ContentSizeFitter))]
	public class UITooltip : MonoBehaviour {
		
		public enum Transition
		{
			None,
			Fade
		}
		
		public enum VisualState
		{
			Shown,
			Hidden
		}
		
		public enum Corner : int
		{
			BottomLeft = 0,
			TopLeft = 1,
			TopRight = 2,
			BottomRight = 3,
		}
		
        #region singleton
        private static UITooltip mInstance;
        public static UITooltip Instance { get { return mInstance; } }
        #endregion

        /// <summary>
        /// The default horizontal fit mode.
        /// </summary>
        public const ContentSizeFitter.FitMode DefaultHorizontalFitMode = ContentSizeFitter.FitMode.Unconstrained;
		
		[SerializeField, Tooltip("Used when no width is specified for the current tooltip display.")]
		private float m_DefaultWidth = 257f;
		
		[SerializeField, Tooltip("Should the tooltip follow the mouse movement or anchor to the position where it was called.")]
		private bool m_followMouse = false;
		
		[SerializeField, Tooltip("Tooltip offset from the pointer when not anchored to a rect.")]
		private Vector2 m_Offset = Vector2.zero;
		
		[SerializeField, Tooltip("Tooltip offset when anchored to a rect.")]
		private Vector2 m_AnchoredOffset = Vector2.zero;
		
		[SerializeField] private Graphic m_AnchorGraphic;
		[SerializeField] private Vector2 m_AnchorGraphicOffset = Vector2.zero;
		
		[SerializeField] private Transition m_Transition = Transition.None;
		[SerializeField] private TweenEasing m_TransitionEasing = TweenEasing.Linear;
		[SerializeField] private float m_TransitionDuration = 0.1f;
		
		private RectTransform m_Rect;
		private CanvasGroup m_CanvasGroup;
		private ContentSizeFitter m_SizeFitter;
		private Canvas m_Canvas;
		private VisualState m_VisualState = VisualState.Shown;
		private RectTransform m_AnchorToTarget;
		private UITooltipLines m_LinesTemplate;
		
		/// <summary>
		/// Gets or sets the default width of the tooltip.
		/// </summary>
		/// <value>The default width.</value>
		public float defaultWidth
		{
			get { return this.m_DefaultWidth; }
			set { this.m_DefaultWidth = value; }
		}
		
		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="UnityEngine.UI.UITooltip"/> should follow the mouse movement.
		/// </summary>
		/// <value><c>true</c> if follow mouse; otherwise, <c>false</c>.</value>
		public bool followMouse
		{
			get { return this.m_followMouse; }
			set { this.m_followMouse = value; }
		}
		
		/// <summary>
		/// Gets or sets the tooltip offset (from the pointer).
		/// </summary>
		/// <value>The offset.</value>
		public Vector2 offset
		{
			get { return this.m_Offset; }
			set { this.m_Offset = value; }
		}
		
		/// <summary>
		/// Gets or sets the tooltip anchored offset (from the anchored rect).
		/// </summary>
		/// <value>The anchored offset.</value>
		public Vector2 anchoredOffset
		{
			get { return this.m_AnchoredOffset; }
			set { this.m_AnchoredOffset = value; }
		}
		
		/// <summary>
		/// Gets the alpha of the tooltip.
		/// </summary>
		/// <value>The alpha.</value>
		public float alpha
		{
			get { return (this.m_CanvasGroup != null) ? this.m_CanvasGroup.alpha : 1f; }
		}
		
		/// <summary>
		/// Gets the the visual state of the tooltip.
		/// </summary>
		/// <value>The state of the visual.</value>
		public VisualState visualState
		{
			get { return this.m_VisualState; }
		}
		
		/// <summary>
		/// Gets the camera responsible for the tooltip.
		/// </summary>
		/// <value>The camera.</value>
		public Camera uiCamera
		{
			get
			{
				if (this.m_Canvas == null)
					return null;
				
				if (this.m_Canvas.renderMode == RenderMode.ScreenSpaceOverlay || (this.m_Canvas.renderMode == RenderMode.ScreenSpaceCamera && this.m_Canvas.worldCamera == null))
				{
					return null;
				}
				
				return (!(this.m_Canvas.worldCamera != null)) ? Camera.main : this.m_Canvas.worldCamera;
			}
		}
		
		/// <summary>
		/// Gets or sets the transition.
		/// </summary>
		/// <value>The transition.</value>
		public Transition transition
		{
			get { return this.m_Transition; }
			set { this.m_Transition = value; }
		}
		
		/// <summary>
		/// Gets or sets the transition easing.
		/// </summary>
		/// <value>The transition easing.</value>
		public TweenEasing transitionEasing
		{
			get { return this.m_TransitionEasing; }
			set { this.m_TransitionEasing = value; }
		}
		
		/// <summary>
		/// Gets or sets the duration of the transition.
		/// </summary>
		/// <value>The duration of the transition.</value>
		public float transitionDuration
		{
			get { return this.m_TransitionDuration; }
			set { this.m_TransitionDuration = value; }
		}
		
		[NonSerialized]
		private readonly TweenRunner<FloatTween> m_FloatTweenRunner;
		
		// Called by Unity prior to deserialization, 
		// should not be called by users
		protected UITooltip()
		{
			if (this.m_FloatTweenRunner == null)
				this.m_FloatTweenRunner = new TweenRunner<FloatTween>();
			
			this.m_FloatTweenRunner.Init(this);
		}
		
		protected virtual void Awake()
		{
			// Save instance reference
			mInstance = this;
			
			// Get the rect transform
			this.m_Rect = this.gameObject.GetComponent<RectTransform>();
			
			// Get the canvas group
			this.m_CanvasGroup = this.gameObject.GetComponent<CanvasGroup>();

            // Make sure the tooltip does not block raycasts
            this.m_CanvasGroup.blocksRaycasts = false;
            this.m_CanvasGroup.interactable = false;

            // Get the content size fitter
            this.m_SizeFitter = this.gameObject.GetComponent<ContentSizeFitter>();

            // Prepare the content size fitter
            this.m_SizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Prepare the vertical layout group
            VerticalLayoutGroup vlg = this.gameObject.GetComponent<VerticalLayoutGroup>();
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;

            // Make sure we have the always on top component
            UIAlwaysOnTop aot = this.gameObject.GetComponent<UIAlwaysOnTop>();
            if (aot == null)
            {
                aot = this.gameObject.AddComponent<UIAlwaysOnTop>();
                aot.order = UIAlwaysOnTop.TooltipOrder;
            }

            // Hide
            this.SetAlpha(0f);
            this.m_VisualState = VisualState.Hidden;
            this.InternalOnHide();
        }

        protected virtual void Start()
        {
            // Make sure anchor is center
            this.m_Rect.anchorMin = new Vector2(0.5f, 0.5f);
            this.m_Rect.anchorMax = new Vector2(0.5f, 0.5f);
        }

        protected virtual void OnDestroy()
		{
			mInstance = null;
		}
		
		protected virtual void OnCanvasGroupChanged()
		{
			// Get the canvas responsible for the tooltip
			this.m_Canvas = UIUtility.FindInParents<Canvas>(this.gameObject);
		}
		
		public virtual bool IsActive()
		{
			return this.enabled && this.gameObject.activeInHierarchy;
		}
		
		protected virtual void Update()
		{
			// Update the tooltip position
			if (this.m_followMouse && this.enabled && this.IsActive() && this.alpha > 0f)
			{
				this.UpdatePositionAndPivot();
			}
		}
		
		/// <summary>
		/// Updates the tooltip position.
		/// </summary>
		public virtual void UpdatePositionAndPivot()
		{
            if (this.m_Canvas == null)
                return;

			// Update the tooltip position to the mosue position
			// If the tooltip is not anchored to a target
			// Anchored position should be updated after updating the pivot
			if (this.m_AnchorToTarget == null)
			{
				// Convert the offset based on the pivot
				Vector2 pivotBasedOffset = new Vector2(((this.m_Rect.pivot.x == 1f) ? (this.m_Offset.x * -1f) : this.m_Offset.x), 
				                                       ((this.m_Rect.pivot.y == 1f) ? (this.m_Offset.y * -1f) : this.m_Offset.y));

                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.m_Canvas.transform as RectTransform, Input.mousePosition, this.uiCamera, out localPoint))
                {
                    this.m_Rect.anchoredPosition = pivotBasedOffset + localPoint;
                }
            }
			
			// Update the tooltip pivot
			this.UpdatePivot();
			
			// Check if we are anchored to a rect
			if (this.m_AnchorToTarget != null)
			{
				// Set the anchor position to the opposite of the tooltip's pivot
				Vector3[] targetWorldCorners = new Vector3[4];
				this.m_AnchorToTarget.GetWorldCorners(targetWorldCorners);
				
				// Convert the tooltip pivot to corner
				Corner pivotCorner = UITooltip.VectorPivotToCorner(this.m_Rect.pivot);
				
				// Get the opposite corner of the pivot corner
				Corner oppositeCorner = UITooltip.GetOppositeCorner(pivotCorner);
				
				// Convert the offset based on the pivot
				Vector2 pivotBasedOffset = new Vector2(((this.m_Rect.pivot.x == 1f) ? (this.m_AnchoredOffset.x * -1f) : this.m_AnchoredOffset.x), 
				                                       ((this.m_Rect.pivot.y == 1f) ? (this.m_AnchoredOffset.y * -1f) : this.m_AnchoredOffset.y));

                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(this.uiCamera, targetWorldCorners[(int)oppositeCorner]);
                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(this.m_Canvas.transform as RectTransform, screenPoint, this.uiCamera, out localPoint))
                {
                    this.m_Rect.anchoredPosition = pivotBasedOffset + localPoint;
                }
            }
		}
		
		/// <summary>
		/// Updates the pivot.
		/// </summary>
		public void UpdatePivot()
		{
			// Get the mouse position
			Vector3 targetPosition = Input.mousePosition;

			// Determine which corner of the screen is closest to the mouse position
			Vector2 corner = new Vector2(
				((targetPosition.x > (Screen.width / 2f)) ? 1f : 0f),
				((targetPosition.y > (Screen.height / 2f)) ? 1f : 0f)
			);
			
			// Set the pivot
			this.SetPivot(UITooltip.VectorPivotToCorner(corner));
		}
		
		/// <summary>
		/// Sets the pivot corner.
		/// </summary>
		/// <param name="point">Point.</param>
		protected void SetPivot(Corner point)
		{
			// Update the pivot
			switch (point)
			{
				case Corner.BottomLeft:
					this.m_Rect.pivot = new Vector2(0f, 0f);
						break;
				case Corner.BottomRight:
					this.m_Rect.pivot = new Vector2(1f, 0f);
						break;
				case Corner.TopLeft:
					this.m_Rect.pivot = new Vector2(0f, 1f);
						break;
				case Corner.TopRight:
					this.m_Rect.pivot = new Vector2(1f, 1f);
						break;
			}	

			// Update the anchor graphic position to the new pivot point
			this.UpdateAnchorGraphicPosition();
		}
		
		protected void UpdateAnchorGraphicPosition()
		{
			if (this.m_AnchorGraphic == null)
				return;
			
			// Get the rect transform
			RectTransform rt = (this.m_AnchorGraphic.transform as RectTransform);
			
			// Pivot should always be bottom left
			rt.pivot = Vector2.zero;
			
			// Update it's anchor to the tooltip's pivot
			rt.anchorMax = this.m_Rect.pivot;
			rt.anchorMin = this.m_Rect.pivot;
			
			// Update it's local position to the defined offset
			rt.localPosition = new Vector3(((this.m_Rect.pivot.x == 1f) ? (this.m_AnchorGraphicOffset.x * -1f) : this.m_AnchorGraphicOffset.x), 
			                               ((this.m_Rect.pivot.y == 1f) ? (this.m_AnchorGraphicOffset.y * -1f) : this.m_AnchorGraphicOffset.y), 
			                               rt.localPosition.z);
			
			// Flip the anchor graphic based on the pivot
			rt.localScale = new Vector3(((this.m_Rect.pivot.x == 0f) ? 1f : -1f), ((this.m_Rect.pivot.y == 0f) ? 1f : -1f), rt.localScale.z);
		}
		
		/// <summary>
		/// Shows the tooltip.
		/// </summary>
		protected virtual void Internal_Show()
		{
			// Create the attribute rows
			this.EvaluateAndCreateTooltipLines();
			
			// Update the tooltip position
			this.UpdatePositionAndPivot();
			
			// Bring forward
			UIUtility.BringToFront(this.gameObject);
			
			// Transition
			this.EvaluateAndTransitionToState(true, false);
		}
		
		/// <summary>
		/// Hides the tooltip.
		/// </summary>
		protected virtual void Internal_Hide()
		{
			this.EvaluateAndTransitionToState(false, false);
		}
		
		/// <summary>
		/// Sets the anchor rect target.
		/// </summary>
		/// <param name="targetRect">Target rect.</param>
		protected virtual void Internal_AnchorToRect(RectTransform targetRect)
		{
			this.m_AnchorToTarget = targetRect;
		}
		
		/// <summary>
		/// Sets the width of the toolip.
		/// </summary>
		/// <param name="width">Width.</param>
		protected void Internal_SetWidth(float width)
		{
			this.m_Rect.sizeDelta = new Vector2(width, this.m_Rect.sizeDelta.y);
		}
		
		/// <summary>
		/// Sets the horizontal fit mode of the tooltip.
		/// </summary>
		/// <param name="mode">Mode.</param>
		protected void Internal_SetHorizontalFitMode(ContentSizeFitter.FitMode mode)
		{
			this.m_SizeFitter.horizontalFit = mode;
		}
		
		/// <summary>
		/// Evaluates and transitions to the given state.
		/// </summary>
		/// <param name="state">If set to <c>true</c> transition to shown <c>false</c> otherwise.</param>
		/// <param name="instant">If set to <c>true</c> instant.</param>
		private void EvaluateAndTransitionToState(bool state, bool instant)
		{
			// Do the transition
			switch (this.m_Transition)
			{
				case Transition.Fade:
					this.StartAlphaTween((state ? 1f : 0f), (instant ? 0f : this.m_TransitionDuration));
					break;
				case Transition.None:
				default:
					this.SetAlpha(state ? 1f : 0f);
					this.m_VisualState = (state ? VisualState.Shown : VisualState.Hidden);
					break;
			}
			
			// If we are transitioning to hidden state and the transition is none
			// Call the internal on hide to do a cleanup
			if (this.m_Transition == Transition.None && !state)
				this.InternalOnHide();
		}
		
		/// <summary>
		/// Sets the alpha of the tooltip.
		/// </summary>
		/// <param name="alpha">Alpha.</param>
		public void SetAlpha(float alpha)
		{
			this.m_CanvasGroup.alpha = alpha;
		}

		/// <summary>
		/// Starts a alpha tween on the tooltip.
		/// </summary>
		/// <param name="targetAlpha">Target alpha.</param>
		public void StartAlphaTween(float targetAlpha, float duration)
		{
			var floatTween = new FloatTween { duration = duration, startFloat = this.m_CanvasGroup.alpha, targetFloat = targetAlpha };
			floatTween.AddOnChangedCallback(SetAlpha);
			floatTween.AddOnFinishCallback(OnTweenFinished);
			floatTween.ignoreTimeScale = true;
			floatTween.easing = this.m_TransitionEasing;
			this.m_FloatTweenRunner.StartTween(floatTween);
		}
		
		/// <summary>
		/// Raises the tween finished event.
		/// </summary>
		protected virtual void OnTweenFinished()
		{
			// Check if the tooltip is not visible meaning it was Fade Out
			if (this.alpha == 0f)
			{
				// Flag as hidden
				this.m_VisualState = VisualState.Hidden;
				
				// Call the internal on hide
				this.InternalOnHide();
			}
			else
			{
				// Flag as shown
				this.m_VisualState = VisualState.Shown;
			}
		}
		
		/// <summary>
		/// Called internally when the tooltip finishes the hide transition.
		/// </summary>
		private void InternalOnHide()
		{
			// Do a cleanup
			this.CleanupLines();

			// Clear the anchor to target
			this.m_AnchorToTarget = null;
			
			// Set the default fit mode
			this.m_SizeFitter.horizontalFit = UITooltip.DefaultHorizontalFitMode;
			
			// Set the default width
			this.m_Rect.sizeDelta = new Vector2(this.m_DefaultWidth, this.m_Rect.sizeDelta.y);
		}
		
		/// <summary>
		/// Evaluates and creates the tooltip lines.
		/// </summary>
		private void EvaluateAndCreateTooltipLines()
		{
			if (this.m_LinesTemplate == null || this.m_LinesTemplate.lineList.Count == 0)
				return;
			
			// We need to apply top padding on the first attribute line for desgin puprpose
			bool firstAttr = true;
			
			// Loop through our attributes
			foreach (UITooltipLines.Line line in this.m_LinesTemplate.lineList)
			{
				// Check if this is the first attribute line
				if (line.style == UITooltipLines.LineStyle.Attribute && firstAttr)
				{
					firstAttr = false;
					line.padding.top += 3;
				}
				
				// Create new row object
				GameObject lineObject = this.CreateLine(line.padding);
				
				// Create each of the columns of this row
				for (int i = 0; i < 2; i++)
				{
					string column = (i == 0) ? line.left : line.right;
					
					// Check if the column is empty so we can skip it
					if (string.IsNullOrEmpty(column))
						continue;
					
					// Create new column
					this.CreateLineColumn(lineObject.transform, column, (i == 0), line.style, line.customStyle);
				}
			}
		}
		
		/// <summary>
		/// Creates new line object.
		/// </summary>
		/// <returns>The attribute row.</returns>
		private GameObject CreateLine(RectOffset padding)
		{
			GameObject obj = new GameObject("Line", typeof(RectTransform));
			(obj.transform as RectTransform).pivot = new Vector2(0f, 1f);
			obj.transform.SetParent(this.transform);
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.transform.localPosition = Vector3.zero;
            obj.layer = this.gameObject.layer;
			HorizontalLayoutGroup hlg = obj.AddComponent<HorizontalLayoutGroup>();
			hlg.padding = padding;
            hlg.childControlHeight = true;
            hlg.childControlWidth = true;

            return obj;
		}

        /// <summary>
        /// Creates new line column object.
        /// </summary>
        /// <param name="parent">Parent.</param>
        /// <param name="content">Content.</param>
        /// <param name="isLeft">If set to <c>true</c> is left.</param>
        /// <param name="style">The style.</param>
        /// <param name="customStyle">The custom style name.</param>
        private void CreateLineColumn(Transform parent, string content, bool isLeft, UITooltipLines.LineStyle style, string customStyle)
		{
			// Create the game object
			GameObject obj = new GameObject("Column", typeof(RectTransform), typeof(CanvasRenderer));
			obj.layer = this.gameObject.layer;
			obj.transform.SetParent(parent);
            obj.transform.localScale = new Vector3(1f, 1f, 1f);
            obj.transform.localPosition = Vector3.zero;

            // Set the pivot to top left
            (obj.transform as RectTransform).pivot = new Vector2(0f, 1f);
			
			// Set a fixed size for attribute columns
			if (style == UITooltipLines.LineStyle.Attribute)
			{
				VerticalLayoutGroup vlg = this.gameObject.GetComponent<VerticalLayoutGroup>();
				HorizontalLayoutGroup phlg = parent.gameObject.GetComponent<HorizontalLayoutGroup>();
				LayoutElement le = obj.AddComponent<LayoutElement>();
				le.preferredWidth = (this.m_Rect.sizeDelta.x - vlg.padding.horizontal - phlg.padding.horizontal) / 2f;
			}
			
			// Prepare the text component
			Text text = obj.AddComponent<Text>();
			text.text = content;
			text.supportRichText = true;
            
            // Get the line style
            UITooltipLineStyle lineStyle = UITooltipManager.Instance.defaultLineStyle;
            
            switch (style)
			{
			    case UITooltipLines.LineStyle.Title:
                    lineStyle = UITooltipManager.Instance.titleLineStyle;
				    break;
			    case UITooltipLines.LineStyle.Attribute:
                    lineStyle = UITooltipManager.Instance.attributeLineStyle;
                    break;
			    case UITooltipLines.LineStyle.Description:
                    lineStyle = UITooltipManager.Instance.descriptionLineStyle;
                    break;
                case UITooltipLines.LineStyle.Custom:
                    lineStyle = UITooltipManager.Instance.GetCustomStyle(customStyle);
                    break;
			}

            // Apply the line style
            text.font = lineStyle.TextFont;
            text.fontStyle = lineStyle.TextFontStyle;
            text.fontSize = lineStyle.TextFontSize;
            text.lineSpacing = lineStyle.TextFontLineSpacing;
            text.color = lineStyle.TextFontColor;

            if (lineStyle.OverrideTextAlignment == OverrideTextAlignment.No)
                text.alignment = (isLeft) ? TextAnchor.LowerLeft : TextAnchor.LowerRight;
            else
            {
                switch (lineStyle.OverrideTextAlignment)
                {
                    case OverrideTextAlignment.Left:
                        text.alignment = TextAnchor.LowerLeft;
                        break;
                    case OverrideTextAlignment.Center:
                        text.alignment = TextAnchor.LowerCenter;
                        break;
                    case OverrideTextAlignment.Right:
                        text.alignment = TextAnchor.LowerRight;
                        break;
                }
            }

            // Add text effect components
            if (lineStyle.TextEffects.Length > 0)
            {
                foreach (UITooltipTextEffect tte in lineStyle.TextEffects)
                {
                    if (tte.Effect == UITooltipTextEffectType.Shadow)
                    {
                        Shadow s = obj.AddComponent<Shadow>();
                        s.effectColor = tte.EffectColor;
                        s.effectDistance = tte.EffectDistance;
                        s.useGraphicAlpha = tte.UseGraphicAlpha;
                    }
                    else if (tte.Effect == UITooltipTextEffectType.Outline)
                    {
                        Outline o = obj.AddComponent<Outline>();
                        o.effectColor = tte.EffectColor;
                        o.effectDistance = tte.EffectDistance;
                        o.useGraphicAlpha = tte.UseGraphicAlpha;
                    }
                }
            }
		}
		
		/// <summary>
		/// Does a line cleanup.
		/// </summary>
		protected virtual void CleanupLines()
		{
			// Clear out the line objects
			foreach (Transform t in this.transform)
			{
				// If the component is not part of the layout dont destroy it
				if (t.gameObject.GetComponent<LayoutElement>() != null)
				{
					if (t.gameObject.GetComponent<LayoutElement>().ignoreLayout)
						continue;
				}
				
				Destroy(t.gameObject);
			}

			// Clear out the attributes template
			this.m_LinesTemplate = null;
		}

        #region OOP Line Methods
        /// <summary>
        /// Sets the lines template.
        /// </summary>
        /// <param name="lines">Lines template.</param>
        protected void Internal_SetLines(UITooltipLines lines)
		{
			this.m_LinesTemplate = lines;
		}
		
		/// <summary>
		/// Adds a new single column line.
		/// </summary>
		/// <param name="a">Content.</param>
		protected void Internal_AddLine(string a, RectOffset padding)
		{
			// Make sure we have a template initialized
			if (this.m_LinesTemplate == null)
				this.m_LinesTemplate = new UITooltipLines();
			
			// Add the line
			this.m_LinesTemplate.AddLine(a, padding);
		}

        /// <summary>
        /// Adds a new single column line.
        /// </summary>
        /// <param name="a">Content.</param>
        /// <param name="style">The line style.</param>
        protected void Internal_AddLine(string a, UITooltipLines.LineStyle style)
        {
            // Make sure we have a template initialized
            if (this.m_LinesTemplate == null)
                this.m_LinesTemplate = new UITooltipLines();

            // Add the line
            this.m_LinesTemplate.AddLine(a, new RectOffset(), style);
        }

        /// <summary>
        /// Adds a new single column line.
        /// </summary>
        /// <param name="a">Content.</param>
        /// <param name="customStyle">The custom line style name.</param>
        protected void Internal_AddLine(string a, string customStyle)
        {
            // Make sure we have a template initialized
            if (this.m_LinesTemplate == null)
                this.m_LinesTemplate = new UITooltipLines();

            // Add the line
            this.m_LinesTemplate.AddLine(a, new RectOffset(), customStyle);
        }

        /// <summary>
        /// Adds a new single column line.
        /// </summary>
        /// <param name="a">Content.</param>
        /// <param name="style">The line style.</param>
        protected void Internal_AddLine(string a, RectOffset padding, UITooltipLines.LineStyle style)
		{
			// Make sure we have a template initialized
			if (this.m_LinesTemplate == null)
				this.m_LinesTemplate = new UITooltipLines();
			
			// Add the line
			this.m_LinesTemplate.AddLine(a, padding, style);
		}

        /// <summary>
        /// Adds a new single column line.
        /// </summary>
        /// <param name="a">Content.</param>
        /// <param name="padding">The line padding.</param>
        /// <param name="customStyle">The custom line style name.</param>
        protected void Internal_AddLine(string a, RectOffset padding, string customStyle)
        {
            // Make sure we have a template initialized
            if (this.m_LinesTemplate == null)
                this.m_LinesTemplate = new UITooltipLines();

            // Add the line
            this.m_LinesTemplate.AddLine(a, padding, customStyle);
        }
        
        /// <summary>
        /// Adds a column (Either to the last line if it's not complete or to a new one).
        /// </summary>
        /// <param name="a">Content.</param>
        protected void Internal_AddLineColumn(string a)
		{
			// Make sure we have a template initialized
			if (this.m_LinesTemplate == null)
				this.m_LinesTemplate = new UITooltipLines();
			
			// Add the column
			this.m_LinesTemplate.AddColumn(a);
		}

        /// <summary>
        /// Adds a column (Either to the last line if it's not complete or to a new one).
        /// </summary>
        /// <param name="a">Content.</param>
        /// <param name="style">The line style.</param>
        protected void Internal_AddLineColumn(string a, UITooltipLines.LineStyle style)
        {
            // Make sure we have a template initialized
            if (this.m_LinesTemplate == null)
                this.m_LinesTemplate = new UITooltipLines();

            // Add the column
            this.m_LinesTemplate.AddColumn(a, style);
        }

        /// <summary>
        /// Adds a column (Either to the last line if it's not complete or to a new one).
        /// </summary>
        /// <param name="a">Content.</param>
        /// <param name="style">The custom line style name.</param>
        protected void Internal_AddLineColumn(string a, string customStyle)
        {
            // Make sure we have a template initialized
            if (this.m_LinesTemplate == null)
                this.m_LinesTemplate = new UITooltipLines();

            // Add the column
            this.m_LinesTemplate.AddColumn(a, customStyle);
        }

        /// <summary>
        /// Adds title line.
        /// </summary>
        /// <param name="title">Tooltip title.</param>
        protected virtual void Internal_AddTitle(string title)
		{
			// Make sure we have a template initialized
			if (this.m_LinesTemplate == null)
				this.m_LinesTemplate = new UITooltipLines();
			
			// Add the title line
			this.m_LinesTemplate.AddLine(title, new RectOffset(0, 0, 0, 0), UITooltipLines.LineStyle.Title);
		}
		
		/// <summary>
		/// Adds description line.
		/// </summary>
		/// <param name="description">Tooltip description.</param>
		protected virtual void Internal_AddDescription(string description)
		{
			// Make sure we have a template initialized
			if (this.m_LinesTemplate == null)
				this.m_LinesTemplate = new UITooltipLines();
			
			// Add the description line
			this.m_LinesTemplate.AddLine(description, new RectOffset(0, 0, 0, 0), UITooltipLines.LineStyle.Description);
		}

        /// <summary>
        /// Adds spacer line.
        /// </summary>
        protected virtual void Internal_AddSpacer()
        {
            // Make sure we have a template initialized
            if (this.m_LinesTemplate == null)
                this.m_LinesTemplate = new UITooltipLines();

            this.m_LinesTemplate.AddLine("", new RectOffset(0, 0, UITooltipManager.Instance.spacerHeight, 0));
        }
        #endregion

        #region Static Line Methods
        /// <summary>
		/// Sets the lines template.
		/// </summary>
		/// <param name="lines">Lines template.</param>
		public static void SetLines(UITooltipLines lines)
        {
            if (mInstance != null)
                mInstance.Internal_SetLines(lines);
        }

        /// <summary>
        /// Adds a new single column line.
        /// </summary>
        /// <param name="content">Content.</param>
        public static void AddLine(string content)
        {
            if (mInstance != null)
                mInstance.Internal_AddLine(content, new RectOffset());
        }

        /// <summary>
        /// Adds a new single column line.
        /// </summary>
        /// <param name="content">Content.</param>
        /// <param name="style">The line style.</param>
        public static void AddLine(string content, UITooltipLines.LineStyle style)
        {
            if (mInstance != null)
                mInstance.Internal_AddLine(content, new RectOffset(), style);
        }

        /// <summary>
        /// Adds a new single column line.
        /// </summary>
        /// <param name="content">Content.</param>
        /// <param name="customStyle">The custom line style name.</param>
        public static void AddLine(string content, string customStyle)
        {
            if (mInstance != null)
                mInstance.Internal_AddLine(content, new RectOffset(), customStyle);
        }

        /// <summary>
        /// Adds a new single column line.
        /// </summary>
        /// <param name="content">Content.</param>
        /// <param name="padding">The line padding.</param>
        public static void AddLine(string content, RectOffset padding)
        {
            if (mInstance != null)
                mInstance.Internal_AddLine(content, padding);
        }

        /// <summary>
        /// Adds a new single column line.
        /// </summary>
        /// <param name="content">Content.</param>
        /// <param name="padding">The line padding.</param>
        /// <param name="style">The line style.</param>
        public static void AddLine(string content, RectOffset padding, UITooltipLines.LineStyle style)
        {
            if (mInstance != null)
                mInstance.Internal_AddLine(content, padding, style);
        }

        /// <summary>
        /// Adds a new single column line.
        /// </summary>
        /// <param name="content">Content.</param>
        /// <param name="padding">The line padding.</param>
        /// <param name="customStyle">The custom line style name.</param>
        public static void AddLine(string content, RectOffset padding, string customStyle)
        {
            if (mInstance != null)
                mInstance.Internal_AddLine(content, padding, customStyle);
        }
        
        /// <summary>
        /// Adds a column (Either to the last line if it's not complete or to a new one).
        /// </summary>
        /// <param name="content">Content.</param>
        public static void AddLineColumn(string content)
        {
            if (mInstance != null)
                mInstance.Internal_AddLineColumn(content);
        }

        /// <summary>
        /// Adds a column (Either to the last line if it's not complete or to a new one).
        /// </summary>
        /// <param name="content">Content.</param>
        /// <param name="style">The line style.</param>
        public static void AddLineColumn(string content, UITooltipLines.LineStyle style)
        {
            if (mInstance != null)
                mInstance.Internal_AddLineColumn(content, style);
        }

        /// <summary>
        /// Adds a column (Either to the last line if it's not complete or to a new one).
        /// </summary>
        /// <param name="content">Content.</param>
        /// <param name="customStyle">The custom line style name.</param>
        public static void AddLineColumn(string content, string customStyle)
        {
            if (mInstance != null)
                mInstance.Internal_AddLineColumn(content, customStyle);
        }

        /// <summary>
        /// Adds a spacer line.
        /// </summary>
        public static void AddSpacer()
        {
            if (mInstance != null)
                mInstance.Internal_AddSpacer();
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Instantiate the tooltip game object if necessary.
        /// </summary>
        /// <param name="rel">Relative game object used to find the canvas.</param>
        public static void InstantiateIfNecessary(GameObject rel)
        {
            if (mInstance != null || UITooltipManager.Instance.prefab == null)
                return;

            Canvas canvas = UIUtility.FindInParents<Canvas>(rel);

            if (canvas != null)
            {
                Instantiate(UITooltipManager.Instance.prefab, canvas.transform, false);
            }
        }

        /// <summary>
        /// Adds title line.
        /// </summary>
        /// <param name="title">Tooltip title.</param>
        public static void AddTitle(string title)
		{
			if (mInstance != null)
				mInstance.Internal_AddTitle(title);
		}
		
		/// <summary>
		/// Adds description line.
		/// </summary>
		/// <param name="description">Tooltip description.</param>
		public static void AddDescription(string description)
		{
			if (mInstance != null)
				mInstance.Internal_AddDescription(description);
		}
		
		/// <summary>
		/// Show the tooltip.
		/// </summary>
		public static void Show()
		{
			if (mInstance != null && mInstance.IsActive())
				mInstance.Internal_Show();
		}
		
		/// <summary>
		/// Hide the tooltip.
		/// </summary>
		public static void Hide()
		{
			if (mInstance != null)
				mInstance.Internal_Hide();
		}
		
		/// <summary>
		/// Anchors the tooltip to a rect.
		/// </summary>
		/// <param name="targetRect">Target rect.</param>
		public static void AnchorToRect(RectTransform targetRect)
		{
			if (mInstance != null)
				mInstance.Internal_AnchorToRect(targetRect);
		}
		
		/// <summary>
		/// Sets the horizontal fit mode of the tooltip.
		/// </summary>
		/// <param name="mode">Mode.</param>
		public static void SetHorizontalFitMode(ContentSizeFitter.FitMode mode)
		{
			if (mInstance != null)
				mInstance.Internal_SetHorizontalFitMode(mode);
		}

        /// <summary>
        /// Sets the width of the toolip.
        /// </summary>
        /// <param name="width">Width.</param>
        public static void SetWidth(float width)
		{
			if (mInstance != null)
				mInstance.Internal_SetWidth(width);
		}
		
		/// <summary>
		/// Convert vector pivot to corner.
		/// </summary>
		/// <returns>The corner.</returns>
		/// <param name="pivot">Pivot.</param>
		public static Corner VectorPivotToCorner(Vector2 pivot)
		{
			// Pivot to that corner
			if (pivot.x == 0f && pivot.y == 0f)
			{
				return Corner.BottomLeft;
			}
			else if (pivot.x == 0f && pivot.y == 1f)
			{
				return Corner.TopLeft;
			}
			else if (pivot.x == 1f && pivot.y == 0f)
			{
				return Corner.BottomRight;
			}
			
			// 1f, 1f
			return Corner.TopRight;
		}
		
		/// <summary>
		/// Gets the opposite corner.
		/// </summary>
		/// <returns>The opposite corner.</returns>
		/// <param name="corner">Corner.</param>
		public static Corner GetOppositeCorner(Corner corner)
		{
			switch (corner)
			{
				case Corner.BottomLeft:
					return Corner.TopRight;
				case Corner.BottomRight:
					return Corner.TopLeft;
				case Corner.TopLeft:
					return Corner.BottomRight;
				case Corner.TopRight:
					return Corner.BottomLeft;
			}
			
			// Default
			return Corner.BottomLeft;
		}
		#endregion
	}
}
