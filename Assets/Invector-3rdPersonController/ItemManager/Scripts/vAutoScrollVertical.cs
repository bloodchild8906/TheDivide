using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace Invector
{
    public class vAutoScrollVertical : MonoBehaviour
    {
        private ScrollRect sr;
        RectTransform contentRect;

        public void Awake()
        {
            sr = this.gameObject.GetComponent<ScrollRect>();
            if (sr)
            {
                contentRect = sr.content;
            }
        }

        void Update()
        {
            OnUpdateSelected();
        }

        public void OnUpdateSelected()
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected == null || selected.transform.parent != contentRect.transform) return;

            // helper vars
            float contentHeight = sr.content.rect.height;
            float viewportHeight = sr.viewport.rect.height;

            // what bounds must be visible?
            float centerLine = selected.transform.localPosition.y; // selected item's center
            float upperBound = centerLine + (selected.GetComponent<RectTransform>().rect.height / 2f); // selected item's upper bound
            float lowerBound = centerLine - (selected.GetComponent<RectTransform>().rect.height / 2f); // selected item's lower bound

            // what are the bounds of the currently visible area?
            float lowerVisible = (contentHeight - viewportHeight) * sr.normalizedPosition.y - (contentHeight * 0.5f);
            float upperVisible = lowerVisible + viewportHeight;

            // is our item visible right now?
            float desiredLowerBound;
            if (upperBound > upperVisible)
            {
                // need to scroll up to upperBound
                desiredLowerBound = upperBound - viewportHeight + selected.GetComponent<RectTransform>().rect.height;
            }
            else if (lowerBound < lowerVisible)
            {
                // need to scroll down to lowerBound
                desiredLowerBound = lowerBound - selected.GetComponent<RectTransform>().rect.height;
            }
            else
            {
                // item already visible - all good
                return;
            }

            // normalize and set the desired viewport
            float normalizedDesired = (desiredLowerBound + contentHeight) / (contentHeight - viewportHeight);
            var normalizedPosition = new Vector2(0f, Mathf.Clamp01(normalizedDesired));
            sr.normalizedPosition = Vector2.Lerp(sr.normalizedPosition, normalizedPosition, 10 * Time.fixedDeltaTime);
        }

    }
}
