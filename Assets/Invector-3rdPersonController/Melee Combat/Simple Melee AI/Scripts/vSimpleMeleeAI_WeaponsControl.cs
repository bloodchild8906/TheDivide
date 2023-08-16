using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Invector.vCharacterController.AI
{
    using vItemManager;
    using vMelee;
    public class vSimpleMeleeAI_WeaponsControl : MonoBehaviour
    {
        #region Variables

        [Header("---- Right Weapon Settings----")]
        public bool useRightWeapon = true;
        public int rightWeaponID;
        public bool randomRightWeapon = true;

        [Header("Right Equip Point")]
        public Transform defaultEquipPointR;
        public List<Transform> customEquipPointR;

        [Header("---- Left Weapon Settings----")]
        public bool useLeftWeapon = true;
        public int leftWeaponID;
        public bool randomLeftWeapon = true;

        [Header("Left Equip Point")]
        public Transform defaultEquipPointL;
        public List<Transform> customEquipPointL;

        public vItemCollection itemCollection;
        protected vSimpleMeleeAI_Controller ai;
        protected vMeleeManager manager;
        protected vItem leftWeaponItem, rightWeaponItem;
        protected GameObject leftWeapon, rightWeapon;
        protected List<vItem> weaponItems = new List<vItem>();
        protected Transform leftArm;
        protected Transform rightArm;
        protected bool inEquip;
        protected bool inUnequip;
        protected float equipTimer, unequipTimer;
        protected float timeToStart;
        protected int equipCalls;
        protected bool changeLeft, changeRight;
#if !UNITY_5_4_OR_NEWER
    protected System.Random random;
#endif
        #endregion

        IEnumerator Start()
        {
            itemCollection = GetComponentInChildren<vItemCollection>(true);
            ai = GetComponent<vSimpleMeleeAI_Controller>();
            manager = GetComponent<vMeleeManager>();
            yield return new WaitForEndOfFrame();

            if (itemCollection && ai && manager)
            {
                ai.onSetAgressive.AddListener(OnSetAgressive);
                leftArm = ai.animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
                rightArm = ai.animator.GetBoneTransform(HumanBodyBones.RightLowerArm);

                for (int i = 0; i < itemCollection.items.Count; i++)
                {
                    if (itemCollection.items[i].amount > 0)
                    {
                        var item = itemCollection.itemListData.items.Find(_item => _item.id == itemCollection.items[i].id && _item.type == vItemType.MeleeWeapon);
                        if (item != null)
                        {
                            AddItem(itemCollection.items[i].id, itemCollection.items[i].amount);
                        }
                    }
                }

                if (useRightWeapon)
                {
                    if (randomRightWeapon)
                    {
                        GetRandomWeapon(ref rightWeaponItem, vMeleeType.OnlyAttack);
                    }
                    else
                    {
                        GetItemWeapon(rightWeaponID, ref rightWeaponItem, vMeleeType.OnlyAttack);
                    }
                }

                if (useLeftWeapon)
                {
                    if (randomLeftWeapon)
                    {
                        GetRandomWeapon(ref leftWeaponItem, vMeleeType.OnlyDefense);
                    }
                    else
                    {
                        GetItemWeapon(leftWeaponID, ref leftWeaponItem, vMeleeType.OnlyDefense);
                    }
                }

                if (rightArm && rightWeaponItem)
                {
                    Transform equipPoint = null;
                    if (customEquipPointR.Count > 0)
                    {
                        equipPoint = customEquipPointR.Find(t => t.name == rightWeaponItem.customHandler);
                    }

                    if (equipPoint == null)
                    {
                        equipPoint = defaultEquipPointR;
                    }

                    if (equipPoint)
                    {
                        rightWeapon = Instantiate(rightWeaponItem.originalObject);
                        rightWeapon.transform.parent = equipPoint;
                        rightWeapon.transform.localPosition = Vector3.zero;
                        rightWeapon.transform.localEulerAngles = Vector3.zero;
                        manager.SetRightWeapon(rightWeapon);
                        rightWeapon.SetActive(false);
                        if (ai.agressiveAtFirstSight)
                        {
                            StartCoroutine(EquipItemRoutine(false, rightWeaponItem, rightWeapon));
                        }
                    }
                }

                if (leftArm && leftWeaponItem)
                {
                    Transform equipPoint = null;
                    if (customEquipPointL.Count > 0)
                    {
                        equipPoint = customEquipPointL.Find(t => t.name == leftWeaponItem.customHandler);
                    }

                    if (equipPoint == null)
                    {
                        equipPoint = defaultEquipPointL;
                    }

                    if (equipPoint)
                    {
                        leftWeapon = Instantiate(leftWeaponItem.originalObject);
                        leftWeapon.transform.parent = equipPoint;
                        leftWeapon.transform.localPosition = Vector3.zero;
                        leftWeapon.transform.localEulerAngles = Vector3.zero;
                        var scale = leftWeapon.transform.localScale;
                        scale.x *= -1;
                        leftWeapon.transform.localScale = scale;
                        manager.SetLeftWeapon(leftWeapon);
                        leftWeapon.SetActive(false);
                        if (ai.agressiveAtFirstSight)
                        {
                            StartCoroutine(EquipItemRoutine(true, leftWeaponItem, leftWeapon));
                        }
                    }
                }
            }
        }

        public void OnSetAgressive(bool value)
        {
            timeToStart = 2f;
            if (value)
            {
                if (rightWeapon && !rightWeapon.activeSelf && !changeRight)
                {
                    changeRight = true;
                    StartCoroutine(EquipItemRoutine(false, rightWeaponItem, rightWeapon));
                }
                if (leftWeapon && !leftWeapon.activeSelf && !changeLeft)
                {
                    changeLeft = true;
                    StartCoroutine(EquipItemRoutine(true, leftWeaponItem, leftWeapon));
                }
            }
            else
            {
                if (rightWeapon && rightWeapon.activeSelf)
                {
                    StartCoroutine(UnequipItemRoutine(false, rightWeaponItem, rightWeapon));
                }

                if (leftWeapon && leftWeapon.activeSelf)
                {
                    StartCoroutine(UnequipItemRoutine(true, leftWeaponItem, leftWeapon));
                }
            }
        }

        void GetItemWeapon(int id, ref vItem weaponItem, vMeleeType type)
        {
            if (weaponItems.Count > 0)
            {
                weaponItem = weaponItems.Find(_item => _item.id == id &&
                            _item.originalObject != null && _item.originalObject.GetComponent<vMeleeWeapon>() != null &&
                            (_item.originalObject.GetComponent<vMeleeWeapon>().meleeType == vMeleeType.AttackAndDefense || _item.originalObject.GetComponent<vMeleeWeapon>().meleeType == type));

                weaponItems.Remove(weaponItem);
            }
        }

        void GetRandomWeapon(ref vItem weaponItem, vMeleeType type)
        {
            if (weaponItems.Count > 0)
            {
                var _weaponItems = weaponItems.FindAll(_item =>
                             _item.originalObject != null && _item.originalObject.GetComponent<vMeleeWeapon>() != null &&
                             (_item.originalObject.GetComponent<vMeleeWeapon>().meleeType == vMeleeType.AttackAndDefense || _item.originalObject.GetComponent<vMeleeWeapon>().meleeType == type));
                var itemIndex = 0;
#if UNITY_5_4_OR_NEWER
                Random.InitState(Random.Range(0, System.DateTime.Now.Millisecond));
                itemIndex = Random.Range(0, _weaponItems.Count - 1);
#else
            random = new System.Random(Random.Range(0, System.DateTime.Now.Millisecond));
            itemIndex = random.Next(0, _weaponItems.Count - 1);
#endif
                weaponItem = _weaponItems[itemIndex];
                weaponItems.Remove(weaponItem);
            }
        }

        IEnumerator EquipItemRoutine(bool flipEquip, vItem item, GameObject weapon)
        {
            equipCalls++;
            while (inEquip || timeToStart > 0 || ai.ragdolled)
            {
                timeToStart -= Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            if (!inUnequip)
            {
                inEquip = true;
                if (weapon != null && item != null)
                {
                    equipTimer = item.enableDelayTime;
                    var type = item.type;
                    if (type != vItemType.Consumable && (!ai.isDead))
                    {
                        if (!string.IsNullOrEmpty(item.EnableAnim))
                        {
                            ai.animator.SetBool("FlipEquip", flipEquip);
                            ai.animator.CrossFade(item.EnableAnim, 0.25f);
                        }
                    }
                    if (weapon != null)
                    {
                        while (equipTimer > 0)
                        {
                            equipTimer -= Time.deltaTime;
                            yield return new WaitForEndOfFrame();
                        }
                        if (!ai.isDead)
                        {
                            weapon.SetActive(true);
                        }
                    }
                }

                inEquip = false;
                equipCalls--;

                if (equipCalls == 0)
                {
                    ai.lockMovement = false;
                }

                if (flipEquip)
                {
                    changeLeft = false;
                }
                else
                {
                    changeRight = false;
                }
            }

        }

        IEnumerator UnequipItemRoutine(bool flipEquip, vItem item, GameObject weapon)
        {
            ai.lockMovement = true;

            while (inUnequip || ai.actions || ai.isAttacking || ai.ragdolled)
            {
                yield return new WaitForEndOfFrame();
            }

            if (!inEquip)
            {
                yield return new WaitForSeconds(1);
                inUnequip = true;
                if (weapon != null && item != null)
                {
                    unequipTimer = item.enableDelayTime;
                    var type = item.type;
                    if (type != vItemType.Consumable)
                    {
                        if (!string.IsNullOrEmpty(item.DisableAnim))
                        {
                            ai.animator.SetBool("FlipEquip", flipEquip);
                            ai.animator.CrossFade(item.DisableAnim, 0.25f);
                        }

                    }
                    if (weapon != null)
                    {
                        while (unequipTimer > 0)
                        {
                            unequipTimer -= Time.deltaTime;
                            yield return new WaitForEndOfFrame();
                        }
                        weapon.SetActive(false);
                    }
                }
                inUnequip = false;
            }
            ai.lockMovement = true;
        }

        /// <summary>
        /// Add new Instance of Item to itemList
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(int itemID, int amount)
        {
            if (itemCollection.itemListData != null && itemCollection.itemListData.items.Count > 0)
            {
                var item = itemCollection.itemListData.items.Find(t => t.id.Equals(itemID));
                if (item)
                {
                    var sameItems = weaponItems.FindAll(i => i.stackable && i.id == item.id && i.amount < i.maxStack);
                    if (sameItems.Count == 0)
                    {
                        var _item = Instantiate(item);
                        _item.name = _item.name.Replace("(Clone)", string.Empty);
                        _item.amount = 0;
                        for (int i = 0; i < item.maxStack && _item.amount < _item.maxStack && amount > 0; i++)
                        {
                            _item.amount++;
                            amount--;
                        }

                        weaponItems.Add(_item);
                        if (amount > 0)
                        {
                            AddItem(item.id, amount);
                        }
                    }
                    else
                    {
                        var indexOffItem = weaponItems.IndexOf(sameItems[0]);

                        for (int i = 0; i < weaponItems[indexOffItem].maxStack && weaponItems[indexOffItem].amount < weaponItems[indexOffItem].maxStack && amount > 0; i++)
                        {
                            weaponItems[indexOffItem].amount++;
                            amount--;
                        }
                        if (amount > 0)
                        {
                            AddItem(item.id, amount);
                        }
                    }
                }
            }
        }

    }
}