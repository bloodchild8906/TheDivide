using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Invector
{
    public class vBlindGhostVFX : MonoBehaviour
    {
        [SerializeField]
        private RawImage imageToDisplay;

        public UnityEngine.Events.UnityEvent onStartGhostFX, onFinishGhostFX;

        public void TriggerGoBlind(vIEffect effect)
        {
            if (effect != null) TriggerGoBlind(effect.EffectDuration);
        }

        public void TriggerGoBlind(float duration)
        {
            StopAllCoroutines();
            StartCoroutine(GhostEffectRoutine(duration));
        }

        private IEnumerator GhostEffectRoutine(float duration)
        {
            yield return new WaitForEndOfFrame();
            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            if (imageToDisplay == null) Debug.LogWarning("Please assign the Image component");

            // Assign the texture to the Image component
            imageToDisplay.texture = texture;
            StartGhostFX();
            yield return new WaitForSeconds(duration);
            FinishGhostFX();
        }

        void StartGhostFX()
        {
            onStartGhostFX.Invoke();
        }
        void FinishGhostFX()
        {
            onFinishGhostFX.Invoke();
        }
    }
}