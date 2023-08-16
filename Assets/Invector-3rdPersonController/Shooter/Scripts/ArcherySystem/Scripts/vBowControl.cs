using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Invector.vShooter
{
    [vClassHeader("v Bow Control")]
    [RequireComponent(typeof(vShooterWeapon))]
    public class vBowControl : vMonoBehaviour
    {
        private vShooterWeapon weapon;
        private Animator animator;
        public float delayToSpringAfterShot;
        public float minPenetration, maxPenetration;
        public UnityEngine.Events.UnityEvent OnFinishShot, OnEnableArrow, OnDisableArrow;

        void Start()
        {
            weapon = GetComponent<vShooterWeapon>();
            animator = GetComponent<Animator>();
        }

        public void EnableArrow()
        {
            OnEnableArrow.Invoke();
          
            if (animator) animator.SetFloat("PowerCharger", 0);
            if (animator) animator.ResetTrigger("Shot");
           
        }

        public void DisableArrow()
        {
            OnDisableArrow.Invoke();

            if (animator) animator.ResetTrigger("Shot");
            if (animator) animator.SetFloat("PowerCharger", 0);
        }

        public void OnChangePowerCharger(float charger)
        {            
            if (animator) animator.SetFloat("PowerCharger", charger);
        }

        public void Shot()
        {
            if (!weapon) return;
            if (animator) animator.SetFloat("PowerCharger", 0);
            if (animator) animator.SetTrigger("Shot");
            StartCoroutine(ShootEffect());
        }

        public void OnInstantiateProjectile(vProjectileControl pCtrl)
        {
            if (!weapon) return;
            var arrow = pCtrl.GetComponent<vArrow>();
            if (arrow)
            {
                arrow.penetration = Mathf.Lerp(minPenetration, maxPenetration, weapon.powerCharge);
            }
        }

        IEnumerator ShootEffect()
        {
            yield return new WaitForSeconds(delayToSpringAfterShot);
            if (weapon.isAiming)
            {
                if (weapon.ammoCount > 0) OnFinishShot.Invoke();
            }
        }

    }
}