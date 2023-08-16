using UnityEngine;
using UnityEngine.UI;

namespace Invector.Throw
{
    public class vThrowUI : MonoBehaviour
    {
        public Text maxThrowCount;
        public Text currentThrowCount;
        public Image display;
        internal virtual void UpdateCount(vThrowManagerBase throwManager,bool showMaxAmount = true)
        {
            if (currentThrowCount) currentThrowCount.text = throwManager.CurrentThrowAmount.ToString();
            if (maxThrowCount) maxThrowCount.text = showMaxAmount? throwManager.MaxThrowObjects.ToString():"";
            if (display) display.sprite = throwManager.CurrentThrowableSprite;
        }
    }
}