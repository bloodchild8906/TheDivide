using Invector;
using UnityEngine;

[vClassHeader(" Weapon Constrain ", "Weapon Constrain Helper: call true OnEquip and false OnDrop by CollectableStandalone events to avoid handler movement on 2018.x", iconName = "meleeIcon")]
public class vWeaponConstrain : vMonoBehaviour
{
    Rigidbody m_Rigidbody;

    protected virtual void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    // Call OnEquip (true) / OnDrop (false)
    public virtual void Inv_Weapon_FreezeAll(bool status)
    {
        if (status)
        {
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
        }
        else
        {
            m_Rigidbody.constraints = RigidbodyConstraints.None;
        }
    }
}