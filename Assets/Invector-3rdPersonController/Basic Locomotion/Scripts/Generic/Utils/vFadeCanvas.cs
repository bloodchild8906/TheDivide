using System.Collections;
using UnityEngine;

namespace Invector.Utils
{
    [vClassHeader("Fade Canvas")]
    public class vFadeCanvas : vMonoBehaviour
    {
        public CanvasGroup group;

        public float fadeSpeed = 2f;
        public UnityEngine.Events.UnityEvent onStartFadeIn, onFinishFadeIn, onStartFadeOut, onFinishFadeOut;
        public UnityEngine.UI.Slider.SliderEvent OnChangeValue;
        public bool autoControlCanvasGroup;
        public bool fadeInStart;
        public bool fadeOutStart;
        public bool startWithAlphaZero = true;
        public bool startWithAlphaFull;

        protected virtual float currentValue { get; set; }
        protected virtual bool inFade { get; set; }

        private void Awake()
        {
            if (!group) group = GetComponent<CanvasGroup>();
        }

        protected virtual void Start()
        {
            InitilizeFadeEffect();
        }

        private void OnEnable()
        {
            InitilizeFadeEffect();
        }

        protected virtual void InitilizeFadeEffect()
        {
            if (fadeInStart) FadeIn();
            if (fadeOutStart) FadeOut();
            if (startWithAlphaZero) AlphaZero();
            if (startWithAlphaFull) AlphaFull();
        }

        public virtual void AlphaZero()
        {
            if (group) group.alpha = 0f;
        }

        public virtual void AlphaFull()
        {
            if (group) group.alpha = 1f;
        }

        public virtual void FadeIn()
        {
            StartCoroutine(Fade(1f));
        }

        public virtual void FadeOut()
        {
            StartCoroutine(Fade(0f));
        }

        public IEnumerator Fade(float targetValue)
        {
            if (targetValue == 1)
            {
                onStartFadeIn.Invoke();
                if (autoControlCanvasGroup && group)
                {
                    group.interactable = false;
                    group.blocksRaycasts = true;
                }
            }
            else
            {
                if (autoControlCanvasGroup && group)
                {
                    group.interactable = false;
                    group.blocksRaycasts = true;
                }
                onStartFadeOut.Invoke();
            }

            inFade = false;
            yield return new WaitForEndOfFrame();
            inFade = true;
            if (group) currentValue = group.alpha;

            while ((targetValue == 1 ? currentValue < 1 : currentValue > 0) && inFade)
            {
                yield return null;
                currentValue = (targetValue == 1) ? currentValue + Time.unscaledDeltaTime * fadeSpeed : currentValue - Time.unscaledDeltaTime * fadeSpeed;
                if (group) group.alpha = currentValue;
                OnChangeValue.Invoke(currentValue);
            }
            if (targetValue == 1)
            {
                onFinishFadeIn.Invoke();
                if (autoControlCanvasGroup && group)
                {
                    group.interactable = true;
                    group.blocksRaycasts = true;
                }
            }
            else
            {
                if (autoControlCanvasGroup && group)
                {
                    group.interactable = false;
                    group.blocksRaycasts = false;
                }

                onFinishFadeOut.Invoke();
            }
        }
    }
}
