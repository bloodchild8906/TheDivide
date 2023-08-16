using UnityEngine;
using UnityEngine.UI;
using System;

namespace DuloGames.UI
{
    [Serializable]
    public enum UITooltipTextEffectType
    {
        Shadow,
        Outline
    }

    [Serializable]
    public enum OverrideTextAlignment
    {
        No,
        Left,
        Center,
        Right
    }

    [Serializable]
    public class UITooltipTextEffect
    {
        public UITooltipTextEffectType Effect;
        public Color EffectColor;
        public Vector2 EffectDistance;
		public bool UseGraphicAlpha;

        public UITooltipTextEffect()
        {
            this.Effect = UITooltipTextEffectType.Shadow;
            this.EffectColor = new Color(0f, 0f, 0f, 128f);
            this.EffectDistance = new Vector2(1f, -1f);
            this.UseGraphicAlpha = true;
        }
    }

    [Serializable]
    public class UITooltipLineStyle
    {
        public string Name;
        public Font TextFont;
        public FontStyle TextFontStyle;
        public int TextFontSize;
        public float TextFontLineSpacing;
        public OverrideTextAlignment OverrideTextAlignment;
        public Color TextFontColor;
        public UITooltipTextEffect[] TextEffects;

        public UITooltipLineStyle()
        {
            this.Name = "";
            this.TextFont = FontData.defaultFontData.font;
            this.TextFontStyle = FontData.defaultFontData.fontStyle;
            this.TextFontSize = FontData.defaultFontData.fontSize;
            this.TextFontLineSpacing = FontData.defaultFontData.lineSpacing;
            this.OverrideTextAlignment = OverrideTextAlignment.No;
            this.TextFontColor = Color.white;
            this.TextEffects = new UITooltipTextEffect[0];
        }
    }
}
