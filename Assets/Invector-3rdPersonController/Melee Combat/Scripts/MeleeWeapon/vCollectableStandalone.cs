using UnityEngine;
using System.Collections;
using UnityEngine.Events;

namespace Invector.vCharacterController.vActions
{
    using vMelee;
    [vClassHeader("Collectable Standalone", "Use this component when your character doesn't have a ItemManager", openClose = false)]
    public class vCollectableStandalone : vTriggerGenericAction
    {
        [vEditorToolbar("Collectable")]
        public string targetEquipPoint;
        public bool twoHandWeapon;
        public GameObject weapon;
        public Sprite weaponIcon;
        public string weaponText;
        [vEditorToolbar("Events")]
        public UnityEvent OnEquip;
        public UnityEvent OnDrop;

        private vCollectMeleeControl manager;

        public override IEnumerator OnPressActionDelay(GameObject cc)
        {
            yield return StartCoroutine(base.OnPressActionDelay(cc));

            manager = cc.GetComponent<vCollectMeleeControl>();

            if (manager != null)
            {
                manager.HandleCollectableInput(this);
            }
        }
    }
}