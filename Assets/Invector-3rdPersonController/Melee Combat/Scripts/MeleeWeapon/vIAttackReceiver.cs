using UnityEngine;
namespace Invector.vEventSystems
{
    public interface vIAttackReceiver
    {        
        void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker);
    }
}

