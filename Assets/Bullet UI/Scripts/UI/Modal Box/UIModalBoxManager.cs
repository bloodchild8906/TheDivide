using UnityEngine;

namespace DuloGames.UI
{
    public class UIModalBoxManager : ScriptableObject
    {
        #region singleton
        private static UIModalBoxManager m_Instance;
        public static UIModalBoxManager Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = Resources.Load("ModalBoxManager") as UIModalBoxManager;

                return m_Instance;
            }
        }
        #endregion

        [SerializeField] private GameObject m_ModalBoxPrefab;

        /// <summary>
        /// Gets the modal box prefab.
        /// </summary>
        public GameObject prefab
        {
            get
            {
                return this.m_ModalBoxPrefab;
            }
        }

        /// <summary>
        /// Creates a modal box
        /// </summary>
        /// <param name="rel">Relative game object used to find the canvas.</param>
        public UIModalBox Create(GameObject rel)
        {
            if (this.m_ModalBoxPrefab == null || rel == null)
                return null;

            Canvas canvas = UIUtility.FindInParents<Canvas>(rel);

            if (canvas != null)
            {
                GameObject obj = Instantiate(this.m_ModalBoxPrefab, canvas.transform, false);

                return obj.GetComponent<UIModalBox>();
            }

            return null;
        }
    }
}
