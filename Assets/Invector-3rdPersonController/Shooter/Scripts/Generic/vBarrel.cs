using UnityEngine;
using System.Collections.Generic;

namespace Invector
{
    public class vBarrel : vHealthController
    {
        public Transform referenceTransformUP;
        public float maxAngleUp = 90;
        protected bool isBarrelRoll;
        public UnityEngine.Events.UnityEvent onBarrelRoll;
        [vEditorToolbar("Health")]
        public List<string> acceptableAttacks = new List<string>() { "explosion", "projectile" };

        void OnCollisionEnter()
        {
            if (!referenceTransformUP) return;
            var angle = Vector3.Angle(referenceTransformUP.up, Vector3.up);

            if (angle > maxAngleUp && !isBarrelRoll)
            {
                isBarrelRoll = true;
                onBarrelRoll.Invoke();
            }
        }

        public override void TakeDamage(vDamage damage)
        {
            if (acceptableAttacks.Contains(damage.damageType))
            {
                base.TakeDamage(damage);
            }
        }
    }
}