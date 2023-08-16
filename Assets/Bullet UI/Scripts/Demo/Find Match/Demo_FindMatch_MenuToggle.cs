using UnityEngine;
using UnityEngine.UI;

namespace DuloGames.UI
{
    [RequireComponent(typeof(Toggle))]
    public class Demo_FindMatch_MenuToggle : MonoBehaviour
    {
        [SerializeField] private Demo_FindMatch_Submenu m_TargetSubmenu;

        private Toggle m_Toggle;
        private Demo_FindMatch_Menu m_Menu;

        protected void Awake()
        {
            this.m_Toggle = this.gameObject.GetComponent<Toggle>();
            this.m_Menu = UIUtility.FindInParents<Demo_FindMatch_Menu>(this.gameObject);
        }

        protected void OnEnable()
        {
            this.m_Toggle.onValueChanged.AddListener(OnToggleValueChanged);
        }

        protected void OnDisable()
        {
            this.m_Toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
        }

        protected void OnToggleValueChanged(bool value)
        {
            if (this.m_TargetSubmenu != null)
            {
                if (value)
                {
                    this.m_TargetSubmenu.Activate();

                    if (this.m_Menu != null)
                        this.m_Menu.SetActiveSubmenu(this.m_TargetSubmenu);
                }
                else
                {
                    this.m_TargetSubmenu.Deactivate();

                    if (this.m_Menu != null)
                        this.m_Menu.ClearActiveSubmenu();
                }
            }
        }
    }
}
