using UnityEngine;
namespace Invector.Throw
{   
    [RequireComponent(typeof(Projector))]
    [vClassHeader("Range Projector Controller", openClose = false)]
    public class vThrowRangeProjectorController : vThrowVisualControlBase
    {
        protected Projector projector;
        public string materialTextureChannel = "_MainTex";
        public string materialColorChannel = "_Color";
        public float projectorSizeMultiplier = 1f;

        protected override void OnInit(vThrowManagerBase tm)
        {
            projector = GetComponent<Projector>();

            if (tm)
            {
                tm.onSetActiveIndicator.AddListener((bool active) => projector.enabled = active);
            }
        }
        public override void OnChangeVisual(vThrowVisualSettings settings)
        {
            if (settings != null && projector)
            {
                projector.enabled = settings.useIndicator;
                SetProjectorColor(settings.indicatorColor);
                SetProjectorSize(settings.indicatorRange);
                SetProjectorTexture(settings.indicatorTexture);
            }
        }
        public void SetProjectorColor(Color color)
        {
            projector.material.SetColor(materialColorChannel, color);
        }
        public void SetProjectorSize(float size)
        {
            if(tm.ObjectToThrow!=null)
                projector.orthographicSize = size * projectorSizeMultiplier * tm.ObjectToThrow.indicatorRangeMultiplier;
        }
        public void SetProjectorTexture(Texture2D texture)
        {
            projector.material.SetTexture(materialTextureChannel, texture);
        }
    }
}