using System.Collections;
using UnityEngine;

namespace Invector.vEventSystems
{
    public interface vIMeleeFighter : vIAttackReceiver, vIAttackListener
    {
        void BreakAttack(int breakAtkID);

        void OnRecoil(int recoilID);

        bool isAttacking { get; }

        bool isArmed { get; }

        bool isBlocking { get; }

        Transform transform { get; }

        GameObject gameObject { get; }

        vCharacterController.vICharacter character { get; }
    }

    public static class vIMeeleFighterHelper
    {
        /// <summary>
        /// check if gameObject has a <see cref="vIMeleeFighter"/> Component
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns>return true if gameObject contains a <see cref="vIMeleeFighter"/></returns>
        public static bool IsMeleeFighter(this GameObject receiver)
        {
            return receiver.GetComponent<vIMeleeFighter>() != null;
        }

        /// <summary>
        /// Apply damage using OnReeiveAttack method if receiver dosent has a vIAttackReceiver, the Simple ApplyDamage is called
        /// </summary>
        /// <param name="receiver">target damage receiver</param>
        /// <param name="damage">damage</param>
        /// <param name="attacker">damage sender</param>
        public static void ApplyDamage(this GameObject receiver, vDamage damage, vIMeleeFighter attacker)
        {
            var attackReceiver = receiver.GetComponent<vIAttackReceiver>();
            if (attackReceiver != null)
            {
                attackReceiver.OnReceiveAttack(damage, attacker);
            }
            else
            {
                receiver.ApplyDamage(damage);
            }
        }

        /// <summary>
        /// Get <see cref="vIMeleeFighter"/> of gameObject
        /// </summary>
        /// <param name="receiver"></param>
        /// <returns>the <see cref="vIMeleeFighter"/> component</returns>
        public static vIMeleeFighter GetMeleeFighter(this GameObject receiver)
        {
            return receiver.GetComponent<vIMeleeFighter>();
        }
    }
}
