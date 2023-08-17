using UnityEngine;
using UnityEngine.UI;

namespace DuloGames.UI
{
    [RequireComponent(typeof(VerticalLayoutGroup), typeof(ContentSizeFitter))]
    public class Demo_KillDisplay : MonoBehaviour
    {
        public enum TextColor
        {
            Friendly,
            Hostile
        }

        public enum TextEffect
        {
            None,
            Shadow,
            Outline
        }

        [SerializeField] private int m_MaxRows = 5;

        [Header("Row Properties")]
        [SerializeField] private Sprite m_RowBackgroundSprite;
        [SerializeField] private Color m_RowBackgroundColor = Color.white;
        [SerializeField] private Image.Type m_RowBackgroundType = Image.Type.Sliced;
        [SerializeField] private RectOffset m_RowPadding;
        [SerializeField] private float m_RowVerticalSpacing = 0f;
        [SerializeField] private float m_RowHorizontalSpacing = 0f;
        [SerializeField] private float m_RowPreferredHeight = 0f;

        [Header("Text Properties")]
        [SerializeField] private Font m_TextFont;
        [SerializeField] private int m_TextFontSize = 16;
        [SerializeField] private Color m_TextFriendlyColor = Color.white;
        [SerializeField] private TextEffect m_TextFiendlyEffect = TextEffect.None;
        [SerializeField] private Color m_TextFriendlyEffectColor = new Color(0f, 0f, 0f, 128f);
        [SerializeField] private Vector2 m_TextFriendlyEffectDistance = new Vector2(1f, -1f);
        [SerializeField] private Color m_TextHostileColor = Color.white;
        [SerializeField] private TextEffect m_TextHostileEffect = TextEffect.None;
        [SerializeField] private Color m_TextHostileEffectColor = new Color(0f, 0f, 0f, 128f);
        [SerializeField] private Vector2 m_TextHostileEffectDistance = new Vector2(1f, -1f);

        [Header("Remove Properties")]
        [SerializeField] private bool m_AutoRemove = true;
        [SerializeField] private float m_AutoRemoveDelay = 5f;

        protected void Awake()
        {
            // Prepare the vertical group
            VerticalLayoutGroup vlg = this.gameObject.GetComponent<VerticalLayoutGroup>();
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;
            vlg.spacing = this.m_RowVerticalSpacing;

            // Prepare the content size fitter
            ContentSizeFitter csf = this.gameObject.GetComponent<ContentSizeFitter>();
            csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        protected void Start()
        {
            // Clean up
            this.DestroyAll();
        }

        public void AddFriendlyKillHostile(string friendlyName, string hostileName, Sprite weaponSprite)
        {
            GameObject rowGo = this.CreateRow();
            this.CreateText(friendlyName, TextColor.Friendly, rowGo.transform);
            this.CreateSprite(weaponSprite, rowGo.transform);
            this.CreateText(hostileName, TextColor.Hostile, rowGo.transform);

            this.CheckMaxRows();
        }

        public void AddHostileKillFriendly(string hostileName, string friendlyName, Sprite weaponSprite)
        {
            GameObject rowGo = this.CreateRow();
            this.CreateText(hostileName, TextColor.Hostile, rowGo.transform);
            this.CreateSprite(weaponSprite, rowGo.transform);
            this.CreateText(friendlyName, TextColor.Friendly, rowGo.transform);

            this.CheckMaxRows();
        }

        public void AddHostileKillHostile(string hostileName1, string hostileName2, Sprite weaponSprite)
        {
            GameObject rowGo = this.CreateRow();
            this.CreateText(hostileName1, TextColor.Hostile, rowGo.transform);
            this.CreateSprite(weaponSprite, rowGo.transform);
            this.CreateText(hostileName2, TextColor.Hostile, rowGo.transform);

            this.CheckMaxRows();
        }

        public void AddFriendlyKillFriendly(string friendlyName1, string friendlyName2, Sprite weaponSprite)
        {
            GameObject rowGo = this.CreateRow();
            this.CreateText(friendlyName1, TextColor.Friendly, rowGo.transform);
            this.CreateSprite(weaponSprite, rowGo.transform);
            this.CreateText(friendlyName2, TextColor.Friendly, rowGo.transform);

            this.CheckMaxRows();
        }

        /// <summary>
        /// Checks if the rows are at the max and manages them.
        /// </summary>
        public void CheckMaxRows()
        {
            if (this.transform.childCount < this.m_MaxRows)
                return;

            // Remove excess rows
            for (int i = 0; i < (this.transform.childCount - this.m_MaxRows); i++)
                this.Destroy(this.transform.GetChild(i).gameObject);
        }

        private GameObject CreateRow()
        {
            // Create the game object
            GameObject go = new GameObject("Row", typeof(RectTransform));
            go.layer = this.gameObject.layer;

            // Prepare the rect
            RectTransform rt = go.transform as RectTransform;
            rt.SetParent(this.transform, false);
            rt.localScale = new Vector3(1f, 1f, 1f);
            rt.localPosition = Vector3.zero;
            rt.pivot = new Vector2(0f, 1f);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);

            // Prepare the layout
            HorizontalLayoutGroup hlg = go.AddComponent<HorizontalLayoutGroup>();
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            hlg.padding = this.m_RowPadding;
            hlg.spacing = this.m_RowHorizontalSpacing;

            // Prepare the image
            Image img = go.AddComponent<Image>();
            img.sprite = this.m_RowBackgroundSprite;
            img.color = this.m_RowBackgroundColor;
            img.type = this.m_RowBackgroundType;

            // Prepare the auto remove
            if (this.m_AutoRemove)
            {
                Demo_KillDisplay_Remover remover = go.AddComponent<Demo_KillDisplay_Remover>();
                remover.Initialize(this.m_AutoRemoveDelay);
            }

            return go;
        }

        private void CreateText(string playerName, TextColor textColor, Transform parent)
        {
            // Create the game object
            GameObject go = new GameObject("Text", typeof(RectTransform));
            go.layer = this.gameObject.layer;

            // Prepare the rect
            RectTransform rt = go.transform as RectTransform;
            rt.SetParent(parent, false);
            rt.localScale = new Vector3(1f, 1f, 1f);
            rt.localPosition = Vector3.zero;
            rt.pivot = new Vector2(0f, 1f);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);

            // Prepare the text
            Text text = go.AddComponent<Text>();
            text.text = playerName;
            text.font = this.m_TextFont;
            text.fontSize = this.m_TextFontSize;
            
            switch (textColor)
            {
                case TextColor.Friendly:
                    text.color = this.m_TextFriendlyColor;
                    break;
                case TextColor.Hostile:
                    text.color = this.m_TextHostileColor;
                    break;
            }

            TextEffect effect = TextEffect.None;
            Color effectColor = Color.white;
            Vector2 effectDistance = Vector2.zero;

            switch (textColor)
            {
                case TextColor.Friendly:
                    effect = this.m_TextFiendlyEffect;
                    effectColor = this.m_TextFriendlyEffectColor;
                    effectDistance = this.m_TextFriendlyEffectDistance;
                    break;
                case TextColor.Hostile:
                    effect = this.m_TextHostileEffect;
                    effectColor = this.m_TextHostileEffectColor;
                    effectDistance = this.m_TextHostileEffectDistance;
                    break;
            }

            switch (effect)
            {
                case TextEffect.Shadow:
                    Shadow shadow = go.AddComponent<Shadow>();
                    shadow.effectColor = effectColor;
                    shadow.effectDistance = effectDistance;
                    break;
                case TextEffect.Outline:
                    Outline outline = go.AddComponent<Outline>();
                    outline.effectColor = effectColor;
                    outline.effectDistance = effectDistance;
                    break;
            }
        }

        private void CreateSprite(Sprite sprite, Transform parent)
        {
            // Create the game object
            GameObject go = new GameObject("Sprite Cell", typeof(RectTransform));
            go.layer = this.gameObject.layer;

            // Prepare the rect
            RectTransform rt = go.transform as RectTransform;
            rt.SetParent(parent, false);
            rt.localScale = new Vector3(1f, 1f, 1f);
            rt.localPosition = Vector3.zero;
            rt.pivot = new Vector2(0f, 1f);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.anchoredPosition = Vector2.zero;

            // Prepare layout element
            LayoutElement le = go.AddComponent<LayoutElement>();
            le.preferredWidth = sprite.rect.width;
            le.preferredHeight = this.m_RowPreferredHeight;

            // Create game object for the sprite
            GameObject spriteGo = new GameObject("Sprite", typeof(RectTransform));
            spriteGo.layer = this.gameObject.layer;

            // Prepare the rect
            RectTransform spriteRt = spriteGo.transform as RectTransform;
            spriteRt.SetParent(rt, false);
            spriteRt.localScale = new Vector3(1f, 1f, 1f);
            spriteRt.localPosition = Vector3.zero;
            spriteRt.pivot = new Vector2(0.5f, 0.5f);
            spriteRt.anchorMin = new Vector2(0.5f, 0.5f);
            spriteRt.anchorMax = new Vector2(0.5f, 0.5f);
            spriteRt.anchoredPosition = Vector2.zero;

            // Prepare the image
            Image img = spriteGo.AddComponent<Image>();
            img.sprite = sprite;
            img.SetNativeSize();
            spriteRt.anchoredPosition = Vector2.zero;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Creates test rows (Editor Only).
        /// </summary>
        [ContextMenu("Create Test Rows (Editor Only)")]
        public void CreateTestRows()
        {
            // Clean up
            this.DestroyAll();

            Sprite weaponSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Bullet UI/Textures/HUD/Kill Display/Example_KillDisplay_Weapon.png");

            this.AddFriendlyKillHostile("UnkNown24", "[FR]NoOne", weaponSprite);
            this.AddHostileKillFriendly("[FR]SomeOneZor", "TheDeadOne", weaponSprite);
            this.AddFriendlyKillHostile("UnkNown24", "[FR]SomeOneZor", weaponSprite);
            this.AddHostileKillHostile("[FR]TeamKiller", "[FR]TeamMate", weaponSprite);
            this.AddFriendlyKillFriendly("UnkNown24", "TheVictim", weaponSprite);
        }

        [ContextMenu("Add Test Row (Editor Only)")]
        public void AddTestRow()
        {
            Sprite weaponSprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Bullet UI/Textures/HUD/Kill Display/Example_KillDisplay_Weapon.png");

            this.AddFriendlyKillHostile("Player " + Random.value, "Player " + Random.value, weaponSprite);
        }
#endif

        /// <summary>
        /// Destroy all the rows.
        /// </summary>
        public void DestroyAll()
        {
            if (this.transform.childCount == 0)
                return;

            foreach (Transform trans in this.transform)
                this.Destroy(trans.gameObject);
        }

        /// <summary>
        /// Destroy a game object (Editor and Runtime).
        /// </summary>
        /// <param name="go">The game object.</param>
        private void Destroy(GameObject go)
        {
            if (go == null)
                return;
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    DestroyImmediate(go);
                };
            }
            else MonoBehaviour.Destroy(go);
#else
            MonoBehaviour.Destroy(go);
#endif
        }
    }
}
