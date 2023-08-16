using UnityEngine;
namespace Invector.Throw
{
    [CreateAssetMenu(menuName = "Invector/Throw/New ThrowVisualSettings")]
    public class vThrowVisualSettings : ScriptableObject
    {
        [vSeparator("Visual Line")]
        public bool useLine;
        [ColorUsage(true, true)]
        public Color lineRendererColor = Color.white;
        public float lineRendererWidth = 1f;
        internal LineAlignment lineAlignment = LineAlignment.TransformZ;
        public Texture2D lineTexture;
        public LineTextureMode lineTextureMode = LineTextureMode.Stretch;
        public Vector2 lineTile = Vector2.one;
        public Vector2 lineOffset = Vector2.zero;
        [vSeparator("Visual Indicator")]
        public bool useIndicator;
        [ColorUsage(true, true)]
        public Color indicatorColor = Color.white;
        public float indicatorRange = 1f;
        public Texture2D indicatorTexture;
    }
}