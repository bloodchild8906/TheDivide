using UnityEngine;
using UnityEngine.UI;

namespace DuloGames.UI
{
    [RequireComponent(typeof(ToggleGroup))]
    public class Demo_CreateClass_SetMenu : MonoBehaviour
    {
        [SerializeField] private Transform m_SetsContainer;
        [SerializeField] private Button m_ButtonPrev;
		[SerializeField] private Button m_ButtonNext;
		[SerializeField] private Text m_LabelText;

        [SerializeField] private Transform m_DotsContainer;
        [SerializeField] private Color m_NormalDotColor = Color.white;
        [SerializeField] private Color m_ActiveDotColor = Color.white;

        private int m_ActiveSet = 0;
        private ToggleGroup m_ToggleGroup;

        protected void Awake()
        {
            this.m_ToggleGroup = this.gameObject.GetComponent<ToggleGroup>();

            // Prepare the toggles
            if (this.m_SetsContainer != null && this.m_SetsContainer.childCount > 0)
            {
                Toggle[] toggles = this.m_SetsContainer.gameObject.GetComponentsInChildren<Toggle>(true);

                foreach (Toggle toggle in toggles)
                {
                    // Set the toggle group
                    toggle.group = this.m_ToggleGroup;
                }
            }
        }

        protected void Start()
        {
            // Detect active page
            if (this.m_SetsContainer != null && this.m_SetsContainer.childCount > 0)
            {
                for (int i = 0; i < this.m_SetsContainer.childCount; i++)
                {
                    if (this.m_SetsContainer.GetChild(i).gameObject.activeSelf)
                    {
                        this.m_ActiveSet = i;
                        break;
                    }
                }
            }

            // Prepare the pages visibility
            this.UpdatePagesVisibility();

            // Prepare the dots
            this.CreateDots();
        }

        protected void OnEnable()
        {
            if (this.m_ButtonPrev != null)
                this.m_ButtonPrev.onClick.AddListener(OnPrevClick);

            if (this.m_ButtonNext != null)
                this.m_ButtonNext.onClick.AddListener(OnNextClick);
        }

        protected void OnDisable()
        {
            if (this.m_ButtonPrev != null)
                this.m_ButtonPrev.onClick.RemoveListener(OnPrevClick);

            if (this.m_ButtonNext != null)
                this.m_ButtonNext.onClick.RemoveListener(OnNextClick);
        }

        public void CreateDots()
        {
            if (this.m_DotsContainer == null)
                return;

            // Remove the current dots
            foreach (Transform tr in this.m_DotsContainer)
                Destroy(tr.gameObject);

            // Create the new dots
            if (this.m_SetsContainer != null && this.m_SetsContainer.childCount > 0)
            {
                for (int i = 0; i < this.m_SetsContainer.childCount; i++)
                {
                    GameObject dot = new GameObject("Dot (" + i.ToString() + ")", typeof(RectTransform));
                    dot.transform.SetParent(this.m_DotsContainer);
                    dot.layer = this.m_DotsContainer.gameObject.layer;

                    // Prepare the transform
                    dot.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    dot.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);

                    // Add image component
                    Image img = dot.AddComponent<Image>();
                    img.color = (i == this.m_ActiveSet) ? this.m_ActiveDotColor : this.m_NormalDotColor;
                }
            }
        }

        public void UpdatePagesVisibility()
        {
            if (this.m_SetsContainer == null)
                return;

            if (this.m_SetsContainer.childCount > 0)
            {
                for (int i = 0; i < this.m_SetsContainer.childCount; i++)
                {
                    // Get the canvas group
                    CanvasGroup cg = this.m_SetsContainer.GetChild(i).gameObject.GetComponent<CanvasGroup>();

                    // Make sure we have canvas group
                    if (cg == null)
                    {
                        cg = this.m_SetsContainer.GetChild(i).gameObject.AddComponent<CanvasGroup>();
                    }

                    // Set the alpha
                    cg.alpha = ((i == this.m_ActiveSet) ? 1.0f : 0.0f);
                    cg.interactable = ((i == this.m_ActiveSet) ? true : false);
                    cg.blocksRaycasts = ((i == this.m_ActiveSet) ? true : false);
                }
            }

            // Format and update the label text
            if (this.m_LabelText != null)
                this.m_LabelText.text = "Class Set " + (this.m_ActiveSet + 1).ToString();

            // Update the dots
            if (this.m_DotsContainer != null && this.m_DotsContainer.childCount > 0)
            {
                for (int i = 0; i < this.m_DotsContainer.childCount; i++)
                {
                    Image img = this.m_DotsContainer.GetChild(i).gameObject.GetComponent<Image>();

                    if (img != null)
                        img.color = (i == this.m_ActiveSet) ? this.m_ActiveDotColor : this.m_NormalDotColor;
                }
            }
        }
        
        private void OnPrevClick()
        {
            if (!this.isActiveAndEnabled || this.m_SetsContainer == null)
                return;

            // If we are on the first page, jump to the last one
            if (this.m_ActiveSet == 0)
                this.m_ActiveSet = this.m_SetsContainer.childCount - 1;
            else
                this.m_ActiveSet -= 1;

            // Activate
            this.UpdatePagesVisibility();
        }

        private void OnNextClick()
        {
            if (!this.isActiveAndEnabled || this.m_SetsContainer == null)
                return;

            // If we are on the last page, jump to the first one
            if (this.m_ActiveSet == (this.m_SetsContainer.childCount - 1))
                this.m_ActiveSet = 0;
            else
                this.m_ActiveSet += 1;

            // Activate
            this.UpdatePagesVisibility();
        }
    }
}
