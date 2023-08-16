using UnityEngine;
using UnityEngine.UI;

namespace DuloGames.UI
{
    public class Demo_CreateClass_WeaponSelect_Toggle : Toggle
    {
        protected override void OnDisable()
        {
            base.OnDisable();

            if (this.group != null)
                this.group.RegisterToggle(this);
        }
    }
}
