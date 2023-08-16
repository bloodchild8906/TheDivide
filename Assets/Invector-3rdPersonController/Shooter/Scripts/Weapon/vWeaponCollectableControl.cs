using Invector.vCharacterController.vActions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vWeaponCollectableControl : MonoBehaviour
{
    [vHelpBox("This component is used for NO INVENTORY weapons", vHelpBoxAttribute.MessageType.Info)]

    public Rigidbody _rigidbody;
    public Collider _physicsCollider;
    public Collider _selfCollider;

    public virtual void Start()
    {
        var _collectable = GetComponent<vCollectableStandalone>();
        if (_collectable)
        {
            _collectable.OnEquip.AddListener(OnEquip);
            _collectable.OnDrop.AddListener(OnDrop);
        }
        if (_rigidbody == null)
        {
            _rigidbody = GetComponentInParent<Rigidbody>();
        }

        if (_physicsCollider == null)
        {
            _physicsCollider = GetComponentInParent<Collider>();
        }

        if (_selfCollider == null)
        {
            _selfCollider = GetComponentInChildren<Collider>();
        }
    }

    public virtual void OnEquip()
    {
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
        }
        if (_physicsCollider != null)
        {
            _physicsCollider.enabled = false;
        }
        if (_selfCollider != null)
        {
            _selfCollider.enabled = false;
        }
    }

    public virtual void OnDrop()
    {
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = false;
        }
        if (_physicsCollider != null)
        {
            _physicsCollider.enabled = true;
        }
        if (_selfCollider != null)
        {
            _selfCollider.enabled = true;
        }
    }
}
