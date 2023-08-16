using UnityEngine;
namespace Invector.Throw
{
    [RequireComponent(typeof(LineRenderer))]
    [vClassHeader("Line Controller", openClose = false)]
    public class vThrowLineController : vThrowVisualControlBase
    {
        protected LineRenderer line;
        public string materialTextureChannel = "_MainTex";
        public string materialColorChannel = "_Color";

        public float lineWidthMultiplier = 1f;

        protected override void OnInit(vThrowManagerBase tm)
        {
            line = GetComponent<LineRenderer>();
        }

        public override void OnChangeVisual(vThrowVisualSettings settings)
        {
            if (settings != null && line)
            {
                line.enabled = settings.useLine;
                SetLineColor(settings.lineRendererColor);
                SetLineWidth(settings.lineRendererWidth);
                SetLineTexture(settings.lineTexture);
                SetLineAlignment(settings.lineAlignment);
                SetLineTextureMode(settings.lineTextureMode);
                SetLineTextureScale(settings.lineTile);
                SetLineTextureOffset(settings.lineOffset);
            }
        }

        public virtual void SetLineColor(Color color)
        {

            line.material.SetColor(materialColorChannel, color);
        }
        public virtual void SetLineAlignment(LineAlignment alignment)
        {
            line.alignment = alignment;
        }
        public virtual void SetLineWidth(float size)
        {
            line.widthMultiplier = size * lineWidthMultiplier;
        }
        public virtual void SetLineTexture(Texture2D texture)
        {
            line.material.SetTexture(materialTextureChannel, texture);
        }
        public virtual void SetLineTextureMode(LineTextureMode lineTextureMode)
        {
            line.textureMode = lineTextureMode;
        }
        public virtual void SetLineTextureScale(Vector2 scale)
        {
            line.material.SetTextureScale(materialTextureChannel, scale);
        }
        public virtual void SetLineTextureOffset(Vector2 offset)
        {
            line.material.SetTextureOffset(materialTextureChannel, offset);
        }
    }
}