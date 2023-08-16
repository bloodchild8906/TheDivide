using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vEventSystems
{
    public interface vIAttackListener
    {
        void OnEnableAttack();

        void OnDisableAttack();

        void ResetAttackTriggers();
    }
}
