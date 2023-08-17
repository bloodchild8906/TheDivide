using UnityEngine;
using UnityEngine.UI;
using DuloGames.UI.Tweens;

namespace DuloGames.UI
{
    [RequireComponent(typeof(CanvasGroup), typeof(ToggleGroup))]
    public class Demo_FindMatch_Menu : MonoBehaviour
    {
        [SerializeField][Range(0f, 1f)] private float m_InactiveAlpha = 0.5f;
        [SerializeField][Range(0f, 1f)] private float m_ActiveAlpha = 1.0f;
        [SerializeField] private float m_TweenDuration = 0.2f;

        private CanvasGroup m_CanvasGroup;
        private ToggleGroup m_ToggleGroup;
        private Demo_FindMatch_Submenu m_ActiveSubmenu;

        // Tween controls
        [System.NonSerialized]
        private readonly TweenRunner<FloatTween> m_TweenRunner;

        // Called by Unity prior to deserialization, 
        // should not be called by users
        protected Demo_FindMatch_Menu()
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

        protected void Start()
        {
            this.m_ToggleGroup.allowSwitchOff = true;
        }

        public void SetActiveSubmenu(Demo_FindMatch_Submenu submenu)
        {
            this.m_ActiveSubmenu = submenu;
            this.Deactivate();
        }

        public void ClearActiveSubmenu()
        {
            if (!this.m_ToggleGroup.AnyTogglesOn())
            {
                this.m_ActiveSubmenu = null;
                this.Activate();
            }
        }

        public void Activate()
        {
            var tween = new FloatTween { duration = this.m_TweenDuration, startFloat = this.m_CanvasGroup.alpha, targetFloat = this.m_ActiveAlpha };
            tween.AddOnChangedCallback(SetAlpha);
            tween.ignoreTimeScale = true;

            this.m_TweenRunner.StartTween(tween);
        }

        public void Deactivate()
        {
            var tween = new FloatTween { duration = this.m_TweenDuration, startFloat = this.m_CanvasGroup.alpha, targetFloat = this.m_InactiveAlpha };
            tween.AddOnChangedCallback(SetAlpha);
            tween.ignoreTimeScale = true;

            this.m_TweenRunner.StartTween(tween);
        }

        protected void SetAlpha(float alpha)
        {
            this.m_CanvasGroup.alpha = alpha;
        }
    }
}
