using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(RectMask2D))]
    public class UIHorizontalCompass : MonoBehaviour
    {
        private string[] m_HeadingLabels = new string[4] { "N", "E", "S", "W" };

        [SerializeField] private Transform m_Target;
        [SerializeField] private Font m_TextFont;
        [SerializeField] private Color m_TextColor = Color.white;
        [SerializeField] private int m_TextSize = 16;
        [SerializeField] private Sprite m_SeparatorLarge;
        [SerializeField] private Sprite m_SeparatorSmall;
        [SerializeField] private int m_NumSeparatorsLarge = 3;
        [SerializeField] private int m_NumSeparatorsSmall = 2;
        [SerializeField] private float m_CellWidth = 100f;

        [SerializeField][HideInInspector] private RectTransform m_ContentRect;

        public RectTransform rectTransform
        {
            get { return this.transform as RectTransform; }
        }

        protected void Start()
        {
            // Construct the compass if not constructed
            if (this.m_ContentRect == null)
                this.ConstructCompass();
        }

        public float headingRectWidth
        {
            get
            {
                float headingRectWidth = this.m_CellWidth;
                headingRectWidth += this.m_CellWidth * this.m_NumSeparatorsLarge;
                headingRectWidth += this.m_CellWidth * (this.m_NumSeparatorsSmall * (this.m_NumSeparatorsLarge + 1));

                return headingRectWidth;
            }
        }

        void Update()
        {
            if (this.m_Target == null || this.m_ContentRect == null)
                return;

            // 0 to 360 heading
            float compassHeading = this.m_Target.transform.rotation.eulerAngles.y;

            float numberOfPixelsNorthToNorth = this.headingRectWidth * 4f;
            float rationAngleToPixel = numberOfPixelsNorthToNorth / 360f;
            
            this.m_ContentRect.anchoredPosition = new Vector2(((compassHeading * rationAngleToPixel) + this.headingRectWidth) * -1f, 0f);
        }

        [ContextMenu("Construct Compass")]
        public void ConstructCompass()
        {
            // Destroy the content
            this.DestroyContent();

            // Create the content rect
            GameObject contentGo = new GameObject("Content", typeof(RectTransform));
            contentGo.layer = this.gameObject.layer;

            // Prepare the rect
            this.m_ContentRect = contentGo.transform as RectTransform;
            this.m_ContentRect.SetParent(this.transform, false);
            this.m_ContentRect.pivot = new Vector2(0f, 0.5f);
            this.m_ContentRect.localScale = new Vector3(1f, 1f, 1f);
            this.m_ContentRect.localPosition = Vector3.zero;
            this.m_ContentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.rectTransform.rect.height);
            this.m_ContentRect.anchoredPosition = new Vector2(this.headingRectWidth * -1f, 0f);

            // Prepare the horizontal layout group
            HorizontalLayoutGroup hlg = contentGo.AddComponent<HorizontalLayoutGroup>();
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.childControlWidth = true;
            hlg.childControlHeight = false;

            // Prepare the content size fitter
            ContentSizeFitter csf = contentGo.AddComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;

            // Create the last heading before the first one
            this.CreateHeading(this.m_HeadingLabels[3]);

            // Create the headings
            for (int i = 0; i < 4; i++)
                this.CreateHeading(this.m_HeadingLabels[i]);

            // Create the first heading after the last
            this.CreateHeading(this.m_HeadingLabels[0]);
        }

        private void CreateHeading(string label)
        {
            // Create the game object
            GameObject go = new GameObject("Heading " + label, typeof(RectTransform));
            go.layer = this.gameObject.layer;

            // Prepare the rect
            RectTransform rt = go.transform as RectTransform;
            rt.SetParent(this.m_ContentRect, false);
            rt.localScale = new Vector3(1f, 1f, 1f);
            rt.localPosition = Vector3.zero;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.rectTransform.rect.height);

            // Prepare the grid
            GridLayoutGroup grid = go.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(this.m_CellWidth, this.rectTransform.rect.height);
            grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            grid.constraintCount = 1;
            
            // Create the text cell
            GameObject textCellGo = this.CreateCell("Label Cell", rt);

            // Create the label text
            this.CreateLabelText(label, textCellGo.transform);
            
            // Create the separators
            for (int i = 0; i < this.m_NumSeparatorsLarge; i++)
            {
                if (i == 0)
                {
                    // Create the small separators
                    for (int si = 0; si < this.m_NumSeparatorsSmall; si++)
                    {
                        // Create the cell
                        GameObject smallSepCellGo = this.CreateCell("Small Separator Cell", rt);

                        // Create the separator image
                        this.CreateSeparator(this.m_SeparatorSmall, smallSepCellGo.transform);
                    }
                }

                // Create th cell
                GameObject sepCellGo = this.CreateCell("Large Separator Cell", rt);

                // Create the separator image
                this.CreateSeparator(this.m_SeparatorLarge, sepCellGo.transform);

                // Create the small separators
                for (int si2 = 0; si2 < this.m_NumSeparatorsSmall; si2++)
                {
                    // Create the cell
                    GameObject smallSepCellGo2 = this.CreateCell("Small Separator Cell", rt);

                    // Create the separator image
                    this.CreateSeparator(this.m_SeparatorSmall, smallSepCellGo2.transform);
                }
            }
        }

        private GameObject CreateCell(string name, Transform parent)
        {
            // Create the game object
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.layer = this.gameObject.layer;

            // Prepare the rect
            RectTransform rt = go.transform as RectTransform;
            rt.SetParent(parent, false);
            rt.localScale = new Vector3(1f, 1f, 1f);
            rt.localPosition = Vector3.zero;

            return go;
        }

        private void CreateLabelText(string label, Transform parent)
        {
            // Prepare the text game object
            GameObject textGo = new GameObject("Text", typeof(RectTransform));
            textGo.layer = this.gameObject.layer;

            // Prepare the text rect
            RectTransform textRt = textGo.transform as RectTransform;
            textRt.SetParent(parent, false);
            textRt.pivot = new Vector2(0.5f, 0.5f);
            textRt.anchorMin = new Vector2(0f, 0.5f);
            textRt.anchorMax = new Vector2(0f, 0.5f);
            textRt.localScale = new Vector3(1f, 1f, 1f);
            textRt.localPosition = Vector3.zero;
            textRt.anchoredPosition = Vector2.zero;

            // Prepare the text
            Text text = textGo.AddComponent<Text>();
            text.text = label;
            text.font = this.m_TextFont;
            text.fontSize = this.m_TextSize;
            text.color = this.m_TextColor;

            // Prepare the content size fitter
            ContentSizeFitter textCsf = textGo.AddComponent<ContentSizeFitter>();
            textCsf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            textCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void CreateSeparator(Sprite sprite, Transform parent)
        {
            // Prepare the text game object
            GameObject sepGo = new GameObject("Image", typeof(RectTransform));
            sepGo.layer = this.gameObject.layer;

            // Prepare the text rect
            RectTransform textRt = sepGo.transform as RectTransform;
            textRt.SetParent(parent, false);
            textRt.pivot = new Vector2(0.5f, 0.5f);
            textRt.anchorMin = new Vector2(0f, 0.5f);
            textRt.anchorMax = new Vector2(0f, 0.5f);
            textRt.localScale = new Vector3(1f, 1f, 1f);
            textRt.localPosition = Vector3.zero;
            textRt.anchoredPosition = Vector2.zero;

            // Prepare the image
            Image img = sepGo.AddComponent<Image>();
            img.sprite = sprite;
            img.SetNativeSize();
        }

        private void DestroyContent()
        {
            if (this.m_ContentRect == null)
                return;

            GameObject go = this.m_ContentRect.gameObject;

            // Destroy bullets
            if (Application.isEditor)
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(go);
                };
#endif
            }
            else Destroy(go);

            // Null the variable
            this.m_ContentRect = null;
        }
    }
}
