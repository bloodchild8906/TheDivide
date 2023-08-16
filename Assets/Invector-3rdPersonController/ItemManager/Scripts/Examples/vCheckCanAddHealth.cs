using System.Collections.Generic;

namespace Invector.vItemManager
{
    using vCharacterController;

    [vClassHeader("Check if can Add Health", "Simple Example to verify if the health item can be used based on the character's health is full or not.", openClose = false)]
    public class vCheckCanAddHealth : vMonoBehaviour
    {
        public vItemManager itemManager;
        public vThirdPersonController tpController;

        public bool getInParent = true;

        protected bool canUse;
        protected bool firstRun;

        protected virtual void Start()
        {
            // first we need to access our itemManager from the Controller
            if (itemManager == null)
            {
                //check 'getInParent' if this script is attached to a children of the itemManager
                if (getInParent)
                    itemManager = GetComponentInParent<vItemManager>();
                else
                    itemManager = GetComponent<vItemManager>();
            }

            // now we access the Controller itself to know the currentHealth later
            if (tpController == null)
            {
                if (getInParent)
                    tpController = GetComponentInParent<vThirdPersonController>();
                else
                    tpController = GetComponent<vThirdPersonController>();
            }

            // if a itemManager is founded, we use this event to call our CanUseItem method 
            if (itemManager)
            {
                itemManager.canUseItemDelegate -= new vItemManager.CanUseItemDelegate(CanUseItem);
                itemManager.canUseItemDelegate += new vItemManager.CanUseItemDelegate(CanUseItem);
            }
        }

        protected virtual void OnDestroy()
        {
            var itemManager = GetComponent<vItemManager>();
            if (itemManager)
                // remove the event when this gameObject is destroyed
                itemManager.canUseItemDelegate -= new vItemManager.CanUseItemDelegate(CanUseItem);
        }

        protected virtual void CanUseItem(vItem item, ref List<bool> validateResult)
        {
            // search for the attribute 'Health' 
            if (item.GetItemAttribute(vItemAttributes.Health) != null)
            {
                // the variable valid will identify if the currentHealth is lower than the maxHealth, allowing to use the item
                var valid = tpController.currentHealth < tpController.maxHealth;
                if (valid != canUse || !firstRun)
                {
                    canUse = valid;
                    firstRun = true;
                    // trigger a custom text if there is a HUDController in the scene
                    vHUDController.instance.ShowText(valid ? "Increase health" : "Can't use " + item.name + " because your health is full", 4f);
                }

                if (!valid)
                    validateResult.Add(valid);
            }
        }
    }
}