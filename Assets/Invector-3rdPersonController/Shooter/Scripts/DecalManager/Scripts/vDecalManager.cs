using UnityEngine;
using System.Collections.Generic;

namespace Invector.vShooter
{
    [vClassHeader("Decal Manager", openClose = false)]
    public class vDecalManager : vMonoBehaviour
    {
        public LayerMask layermask;
      
        public List<DecalObject> decalObjects;

        public virtual void CreateDecal(RaycastHit hitInfo)
        {
            CreateDecal(hitInfo.collider.gameObject, hitInfo.point, hitInfo.normal);
        }

        public virtual void CreateDecal(GameObject target, Vector3 position, Vector3 normal)
        {
            if (layermask == (layermask | (1 << target.layer)))
            {
                DecalObject decalObj = GetDecal(target.tag);
                if (decalObj != null)
                {
                    RaycastHit hit;
                    if (Physics.SphereCast(new Ray(position + (normal * 0.1f), -normal), 0.0001f, out hit, 1f, layermask))
                    {

                        if (hit.collider.gameObject == target)
                        {
                            var rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
                            var point = hit.point;
                            decalObj.CreateEffect(point, rotation,gameObject, target);
                           
                        }
                    }
                }
            }
        }

        protected virtual DecalObject GetDecal(string tag)
        {
            return decalObjects.Find(d=>d.tag.Equals(tag));
        }

        [System.Serializable]
        public class DecalObject
        {
            public string tag;          
             
            [SerializeField] protected vImpactEffectBase impactEffect;
            [SerializeField] protected List<vImpactEffectBase> additionalEffects;
            public void CreateEffect(Vector3 position,Quaternion rotation,GameObject impactSender, GameObject impactReceiver)
            {
                impactEffect.DoImpactEffect(position, rotation,impactSender, impactReceiver);
                for(int i=0;i<additionalEffects.Count;i++)
                {
                    additionalEffects[i].DoImpactEffect(position, rotation,impactSender, impactReceiver);
                }
             
            }
        }
    }
}