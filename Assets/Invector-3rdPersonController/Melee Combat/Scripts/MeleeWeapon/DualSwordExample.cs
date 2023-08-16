using Invector;
using Invector.vMelee;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DualSwordExample : vMonoBehaviour
{
    public vMeleeWeapon secundaryWeaponPrefab;
    public string otherSideHandlerName;
    [vReadOnly, SerializeField] protected vMeleeWeapon secundaryWeapon;
    [vReadOnly, SerializeField] protected Transform otherSideTransform;
    [vReadOnly, SerializeField] protected vMeleeManager manager;

    private void Start()
    {
        OnEnable();
    }

    private void OnDestroy()
    {
        OnDisable();
        if (secundaryWeapon) Destroy(secundaryWeapon.gameObject);

    }

    private void OnEnable()
    {
        if (!otherSideTransform)
        {
            Animator animator = GetComponentInParent<Animator>();
            if (animator)
            {
                var _otherSideHand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
                var childrens = _otherSideHand.GetComponentsInChildren<Transform>();
                for (int i = 0; i < childrens.Length; i++)
                    if (childrens[i].gameObject.name.Equals(otherSideHandlerName))
                    {
                        otherSideTransform = childrens[i]; break;
                    }
            }

        }
        if (otherSideTransform)
        {
            ActiveSecundaryWeapon();
        }
    }

    private void ActiveSecundaryWeapon()
    {
        if (secundaryWeapon)
        {
            secundaryWeapon.gameObject.SetActive(true);
        }
        else
        {
            secundaryWeapon = Instantiate(secundaryWeaponPrefab);
            secundaryWeapon.transform.parent = otherSideTransform;
            secundaryWeapon.transform.localPosition = Vector3.zero;
            secundaryWeapon.transform.localEulerAngles = Vector3.zero;

        }
        if (!manager) manager = GetComponentInParent<vMeleeManager>();
        if (manager)
        {
            manager.SetLeftWeapon(secundaryWeapon);
        }
    }

    private void OnDisable()
    {
        if (secundaryWeapon)
        {
            secundaryWeapon.gameObject.SetActive(false);
            manager.leftWeapon = null;
        }
    }
}
