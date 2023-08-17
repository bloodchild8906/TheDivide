using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DuloGames.UI
{
    public class Demo_OptionRow : Selectable, ISubmitHandler
    {
        [Header("Option Properties")]
        [SerializeField] private Selectable m_Target;
        [SerializeField] private GameObject m_Description;

        protected override void Awake()
        {
            base.Awake();

            if (this.m_Description != null)
                this.m_Description.SetActive(false);
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            if (this.m_Target == null)
                return;

            if (this.m_Target is ISubmitHandler)
                (this.m_Target as ISubmitHandler).OnSubmit(eventData);
        }

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            if (this.m_Description != null)
                this.m_Description.SetActive(true);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);

            if (this.m_Description != null)
                this.m_Description.SetActive(false);
        }
#else
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            if (this.m_Description != null)
                this.m_Description.SetActive(true);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            if (this.m_Description != null)
                this.m_Description.SetActive(false);
        }
#endif
    }
}
