using UnityEngine;
using DuloGames.UI.Tweens;

namespace DuloGames.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Demo_FindMatch_Info : MonoBehaviour
    {
        [SerializeField][Range(0f, 1f)] private float m_InactiveAlpha = 0.0f;
        [SerializeField][Range(0f, 1f)] private float m_ActiveAlpha = 1.0f;
        [SerializeField] private float m_TweenDuration = 0.2f;

        private CanvasGroup m_CanvasGroup;

        // Tween controls
        [System.NonSerialized]
        private readonly TweenRunner<FloatTween> m_TweenRunner;

        // Called by Unity prior to deserialization, 
        // should not be called by users
        protected Demo_FindMatch_Info()
        {
            if (this.m_TweenRunner == null)
                this.m_TweenRunner = new TweenRunner<FloatTween>();

            this.m_TweenRunner.Init(this);
        }

        protected void Awake()
        {
            this.m_CanvasGroup = this.gameObject.GetComponent<CanvasGroup>();
        }

        public void Start()
        {
            this.m_CanvasGroup.alpha = 0f;
            this.m_CanvasGroup.interactable = false;
            this.m_CanvasGroup.blocksRaycasts = false;
        }

        public void Activate()
        {
            this.m_CanvasGroup.interactable = true;
            this.m_CanvasGroup.blocksRaycasts = true;

            var tween = new FloatTween { duration = this.m_TweenDuration, startFloat = this.m_CanvasGroup.alpha, targetFloat = this.m_ActiveAlpha };
            tween.AddOnChangedCallback(SetAlpha);
            tween.ignoreTimeScale = true;

            this.m_TweenRunner.StartTween(tween);
        }

        public void Deactivate()
        {
            this.m_CanvasGroup.interactable = false;
            this.m_CanvasGroup.blocksRaycasts = false;

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
