using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.Utils
{
    [RequireComponent(typeof(UnityEngine.UI.Slider))]
    [RequireComponent(typeof(CanvasGroup))]
    [vClassHeader("ShowHideSlideControl",false)]
    public class ShowHideSlideControl : vMonoBehaviour
    {
        public UnityEngine.UI.Slider slider;
        public UnityEngine.CanvasGroup canvasGroup;
        public float fadeIn=1, fadeOut=1;

        public UnityEngine.Events.UnityEvent onStartFadeIn,onStartFadeOut, onFinishFadeIn,onFinishFadeOut;
        Coroutine currentRoutine;
        // Start is called before the first frame update


        float slideValue => (float) System.Math.Round(slider.value, 1);
        void Start()
        {
            if(slider == null)slider = GetComponent<UnityEngine.UI.Slider>();
            if (canvasGroup == null) canvasGroup = GetComponent<UnityEngine.CanvasGroup>();
            if (slider !=null)
            {
                slider.onValueChanged.AddListener(OnChangeSlideValue);

                CheckValue();
            }           
        }

        private void CheckValue()
        {
            if (slideValue >= slider.maxValue)
            {
                HideBar();
            }
            else
            {
                ShowBar();
            }
        }

        public void ShowBar()
        {
            if (inFadeIn || canvasGroup.alpha >= 1) return;
            inFadeIn = false;
            inFadeOut = false;
            if (currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(ControllBarAlphaRoutine());
        }
        public void HideBar()
        {
            if (inFadeOut || canvasGroup.alpha <=0) return;
            inFadeIn = false;
            inFadeOut = false;
            if (currentRoutine != null) StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(ControllBarAlphaRoutine(false));
        }


        bool inFadeIn, inFadeOut;



        IEnumerator ControllBarAlphaRoutine(bool show = true)
        {
            float value = canvasGroup.alpha;
            
            if(show)
            {
                inFadeIn = true;
                onStartFadeIn.Invoke();
                while(canvasGroup.alpha < 1f)
                {
                    canvasGroup.alpha += Time.deltaTime * fadeIn;
                    yield return null;
                }
                canvasGroup.alpha = 1;
                inFadeIn = false;
                onFinishFadeIn.Invoke();
            }
            else
            {
                inFadeOut = true;
                onStartFadeOut.Invoke();
                while (canvasGroup.alpha > 0f)
                {
                    canvasGroup.alpha -= Time.deltaTime * fadeOut;
                    yield return null;
                }
                canvasGroup.alpha = 0;
                inFadeOut = false;
                onFinishFadeOut.Invoke();
            }
            currentRoutine = null;
        }
        private void OnChangeSlideValue(float arg0)
        {           
            CheckValue();
        }       
    }
}