using System.Collections.Generic;
using UnityEngine;
namespace Invector.vShooter
{
    [CreateAssetMenu(menuName = "Invector/Shooter/Impact Effects/New ImpactEffect", fileName = "ImpactEffect@")]
    public class vImpactEffect : vImpactEffectBase
    {
        public List<GameObject> decals;
        public List<GameObject> hitEffects;
        protected virtual GameObject GetRandomObject(List<GameObject> referenceList)
        {
            if (referenceList.Count > 1)
            {
                var index = Random.Range(0, referenceList.Count);
                return referenceList[index];
            }
            else if (referenceList.Count == 1)
                return referenceList[0];
            else
                return null;
        }

        protected virtual GameObject CreateDecal(Vector3 position, Quaternion rotation)
        {
            return CreateInstance(GetRandomObject(decals), position, rotation);
        }
        protected virtual GameObject CreateHitEffect(Vector3 position, Quaternion rotation)
        {
            return CreateInstance(GetRandomObject(hitEffects), position, rotation);
        }

        protected GameObject CreateInstance(GameObject target, Vector3 position, Quaternion rotation)
        {
            if (target == null) return null;
            else return Instantiate(target, position, rotation);
        }

        public override void DoImpactEffect(Vector3 position, Quaternion rotation, GameObject sender, GameObject receiver)
        {
            var decal = CreateInstance(GetRandomObject(decals), position, rotation);
            decal.transform.Rotate(Vector3.forward, Random.Range(0, 360), Space.Self);
            var hitEffect = CreateInstance(GetRandomObject(hitEffects), position, rotation);
            if (decal && receiver)
            {
                decal.transform.SetParent(receiver.transform, true);
            }
            if (hitEffect)
            {
                hitEffect.transform.SetParent(vObjectContainer.root, true);
            }
        }
    }
}