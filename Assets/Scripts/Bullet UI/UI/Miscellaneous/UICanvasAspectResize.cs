using UnityEngine;

namespace DuloGames.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(RectTransform))]
    public class UICanvasAspectResize : MonoBehaviour
    {
        [SerializeField] private Camera m_Camera;
        
        private RectTransform m_RectTransform;

        protected void Awake()
        {
            this.m_RectTransform = this.transform as RectTransform;
        }

        void Update()
        {
            if (this.m_Camera == null)
                return;

            this.m_RectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, this.m_Camera.aspect * this.m_RectTransform.rect.size.y);
        }
    }
}
