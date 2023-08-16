using Invector.vShooter;
using UnityEngine;
namespace Invector.IK
{
    public static class vWeaponIKAdjustHelper
    {
        public static IKAdjust Copy(this IKAdjust iKAdjust)
        {
            IKAdjust newCopy = new IKAdjust();
            newCopy.name = iKAdjust.name;
            newCopy.spineOffset = iKAdjust.spineOffset.Copy();
            newCopy.supportHandOffset = iKAdjust.supportHandOffset.Copy();
            newCopy.supportHintOffset = iKAdjust.supportHintOffset.Copy();
            newCopy.weaponHandOffset = iKAdjust.weaponHandOffset.Copy();
            newCopy.weaponHintOffset = iKAdjust.weaponHintOffset.Copy();

            return newCopy;
        }

        public static IKAdjust Copy(this IKAdjust iKAdjust,string name)
        {
            IKAdjust newCopy = new IKAdjust();
            newCopy.name = name;
            newCopy.spineOffset = iKAdjust.spineOffset.Copy();
            newCopy.supportHandOffset = iKAdjust.supportHandOffset.Copy();
            newCopy.supportHintOffset = iKAdjust.supportHintOffset.Copy();
            newCopy.weaponHandOffset = iKAdjust.weaponHandOffset.Copy();
            newCopy.weaponHintOffset = iKAdjust.weaponHintOffset.Copy();

            return newCopy;
        }

        public static IKOffsetSpine Copy(this IKOffsetSpine iKOffsetSpine)
        {
            IKOffsetSpine newCopy = new IKOffsetSpine();
            newCopy.head = iKOffsetSpine.head;
            newCopy.spine = iKOffsetSpine.spine;
            return newCopy;
        }

        public static IKOffsetTransform Copy(this IKOffsetTransform iKOffsetTransform)
        {
            IKOffsetTransform newCopy = new IKOffsetTransform();
            newCopy.position = iKOffsetTransform.position;
            newCopy.eulerAngles = iKOffsetTransform.eulerAngles;
            return newCopy;
        }
    }

    [System.Serializable]
    public class IKAdjust
    {
        public string name;
        [ContextMenuItem("Copy", "ResetBiography")]      
        public IKOffsetTransform weaponHandOffset = new IKOffsetTransform();
        public IKOffsetTransform weaponHintOffset = new IKOffsetTransform();
        public IKOffsetTransform supportHandOffset = new IKOffsetTransform();
        public IKOffsetTransform supportHintOffset = new IKOffsetTransform();
        public IKOffsetSpine spineOffset = new IKOffsetSpine();       
        public IKAdjust()
        {

        }
        public IKAdjust(string name)
        {
            this.name = name;
        }
    }
   

    [System.Serializable]
    public class IKOffsetTransform
    {
        public Vector3 position;
        public Vector3 eulerAngles;
    }

    [System.Serializable]
    public class IKOffsetSpine
    {
        public Vector2 spine;
        public Vector2 head;
    }
}