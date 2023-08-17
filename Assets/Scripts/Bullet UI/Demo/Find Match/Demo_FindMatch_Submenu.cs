using UnityEngine;
using UnityEngine.UI;
using DuloGames.UI.Tweens;

namespace DuloGames.UI
{
    [RequireComponent(typeof(CanvasGroup), typeof(ToggleGroup))]
    public class Demo_FindMatch_Submenu : MonoBehaviour
    {
        [SerializeField] private float m_TweenDuration = 0.2f;

        private CanvasGroup m_CanvasGroup;
        private ToggleGroup m_ToggleGroup;

        // Tween controls
        [System.NonSerialized]
        private readonly TweenRunner<FloatTween> m_TweenRunner;

        // Called by Unity prior to deserialization, 
        // should not be called by users
        protected Demo_FindMatch_Submenu()
        {
            if (this.m_TweenRunner == null)
                this.m_TweenRunner = new TweenRunner<FloatTween>();

            this.m_TweenRunner.Init(this);
        }

        protected void Awake()
        {
            this.m_CanvasGroup = this.gameObject.GetComponent<CanvasGroup>();
            this.m_ToggleGroup = this.gameObject.GetComponent<ToggleGroup>();
        }

        public void Start()
        {
            this.m_CanvasGroup.alpha = 0f;
            this.m_CanvasGroup.interactable = false;
            this.m_CanvasGroup.blocksRaycasts = false;

            this.m_ToggleGroup.allowSwitchOff = true;
        }

        public void Activate()
        {
            this.m_CanvasGroup.interactable = true;
            this.m_CanvasGroup.blocksRaycasts = true;

            var tween = new FloatTween { duration = this.m_TweenDuration, startFloat = this.m_CanvasGroup.alpha, targetFloat = 1.0f };
            tween.AddOnChangedCallback(Animate);
            tween.ignoreTimeScale = true;

            this.m_TweenRunner.StartTween(tween);
        }

        public void Deactivate()
        {
            this.m_CanvasGroup.interactable = false;
            this.m_CanvasGroup.blocksRaycasts = false;

            this.m_ToggleGroup.SetAllTogglesOff();

            var tween = new FloatTween { duration = this.m_TweenDuration, startFloat = this.m_CanvasGroup.alpha, targetFloat = 0.0f };
            tween.AddOnChangedCallback(Animate);
            tween.ignoreTimeScale = true;

            this.m_TweenRunner.StartTween(tween);
        }

        protected void Animate(float percent)
        {
            this.m_CanvasGroup.alpha = percent;

            RectTransform rt = this.transform as RectTransform;
            rt.anchoredPosition = new Vector2(-160f + (160f * percent), rt.anchoredPosition.y);
        }
    }
}
