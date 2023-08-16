using UnityEngine;
using System;

namespace Invector.vMelee
{
    [vClassHeader("HitBox", openClose = false)]
    public class vHitBox : vMonoBehaviour
    {
        [HideInInspector]
        public vMeleeAttackObject attackObject;
        // [HideInInspector]
        public Collider trigger
        {
            get
            {
                _trigger = gameObject.GetComponent<Collider>();

                if (!_trigger) _trigger = gameObject.AddComponent<BoxCollider>();
                return _trigger;
            }
        }
        public int damagePercentage = 100;
        [vEnumFlag]
        public vHitBoxType triggerType = vHitBoxType.Damage | vHitBoxType.Recoil;
        protected bool canHit;
        protected Collider _trigger;
        void OnDrawGizmos()
        {
           
            Color color = (triggerType & vHitBoxType.Damage) != 0 && (triggerType & vHitBoxType.Recoil) == 0 ? Color.green :
                           (triggerType & vHitBoxType.Damage) != 0 && (triggerType & vHitBoxType.Recoil) != 0 ? Color.yellow :
                           (triggerType & vHitBoxType.Recoil) != 0 && (triggerType & vHitBoxType.Damage) == 0 ? Color.red : Color.black;
            color.a = 0.6f;
            Gizmos.color = color;
            if (!Application.isPlaying && trigger && !trigger.enabled) trigger.enabled = true;
            if (trigger && trigger.enabled)
            {
                if (trigger as BoxCollider)
                {
                    BoxCollider box = trigger as BoxCollider;

                    //var sizeX = transform.lossyScale.x * box.size.x;
                    //var sizeY = transform.lossyScale.y * box.size.y;
                    //var sizeZ = transform.lossyScale.z * box.size.z;
                    //Matrix4x4 rotationMatrix = Matrix4x4.TRS(box.bounds.center, transform.rotation, new Vector3(sizeX, sizeY, sizeZ));
                    //Gizmos.matrix = rotationMatrix;
                    //Gizmos.DrawCube(Vector3.zero, Vector3.one);

                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                    Gizmos.DrawCube(box.center, Vector3.Scale(Vector3.one, box.size));
                }
            }
        }

        void Start()
        {          
            if (trigger)
            {
                trigger.isTrigger = true;
                trigger.enabled = false;
            }
            var h_layer = LayerMask.NameToLayer("Ignore Raycast");
            transform.gameObject.layer = h_layer;
            canHit = ((triggerType & vHitBoxType.Damage) != 0 || (triggerType & vHitBoxType.Recoil) != 0);
        }

        void OnTriggerEnter(Collider other)
        {
            if (TriggerCondictions(other))
            {
                if (attackObject != null)
                {
                    attackObject.OnHit(this, other);
                }
            }
        }

        bool TriggerCondictions(Collider other)
        {
            return (canHit && (attackObject != null && (attackObject.meleeManager == null || other.gameObject != attackObject.meleeManager.gameObject)));
        }
    }

    [Flags]
    public enum vHitBoxType
    {
        Damage = 1, Recoil = 2
    }
}