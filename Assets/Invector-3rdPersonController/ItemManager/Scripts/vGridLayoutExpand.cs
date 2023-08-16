using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace Invector
{
    public class vGridLayoutExpand : MonoBehaviour
    {
        GridLayoutGroup grid;
        public int count;
        RectTransform rect;
        float multiple;
        float oldMultiple;

        void Start()
        {
            grid = GetComponent<GridLayoutGroup>();
            rect = GetComponent<RectTransform>();
        }

        void OnDrawGizmos()
        {
            if (Application.isPlaying) return;
            grid = GetComponent<GridLayoutGroup>();
            rect = GetComponent<RectTransform>();
            UpdateBottomSize();
        }

        void UpdateBottomSize()
        {
            double value = ((double)rect.childCount / (double)count);
            multiple = (float)System.Math.Round(value, System.MidpointRounding.AwayFromZero) + 1;
            if (multiple != oldMultiple)
            {
                var scale = rect.offsetMin;
                var desiredScaleY = (grid.cellSize.y + grid.spacing.y) * -multiple;
                scale.y = desiredScaleY;
                rect.offsetMin = scale;
                oldMultiple = multiple;
            }
        }

        void Update()
        {
            UpdateBottomSize();
        }
    }
}