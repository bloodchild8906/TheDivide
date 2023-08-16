using UnityEngine;

namespace Invector.vCharacterController
{
    using vEventSystems;
    [vClassHeader("DAMAGE RECEIVER", "You can add damage multiplier for example causing twice damage on Headshots", openClose = false)]
    public partial class vDamageReceiver : vMonoBehaviour, vIAttackReceiver
    {
        public void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
        {       
            if ((ragdoll && !ragdoll.iChar.isDead))
            {
                var _damage = ApplyDamageModifiers(damage);
                ragdoll.gameObject.ApplyDamage(_damage, attacker);
                onReceiveDamage.Invoke(_damage);
            }
            else if(targetReceiver)
            {
                var _damage = ApplyDamageModifiers(damage);
                targetReceiver.gameObject.ApplyDamage(_damage, attacker);
                onReceiveDamage.Invoke(_damage);
            }
            else
            {
                TakeDamage(damage);
            }
        }
      
    }
}