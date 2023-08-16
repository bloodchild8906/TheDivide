using System.Collections.Generic;
using UnityEngine;
namespace Invector.vShooter
{
    [CreateAssetMenu(menuName = "Invector/Shooter/New BulletLifeSettings")]
    public class vBulletLifeSettings : ScriptableObject
    {
        public List<vBulletLostLife> bulletLostLifeList;
        private bool seedGenerated;
        public vBulletLifeInfo GetReduceLife(string tag, int layer)
        {
            var bulletLostLife = bulletLostLifeList.Find(blf => isValid(blf, tag, layer));
            vBulletLifeInfo bInfo = new vBulletLifeInfo();
            if (bulletLostLife != null)
            {
                bInfo.lostLife = bulletLostLife.reduceLife;
                bInfo.lostDamage = bulletLostLife.damageReducePercentage;
                bInfo.minChangeTrajectory = bulletLostLife.minChangeTrajectory;
                bInfo.maxChangeTrajectory = bulletLostLife.maxChangeTrajectory;
                bInfo.maxThicknessToCross = bulletLostLife.maxThicknessToCross;
                bInfo.ricochet = bulletLostLife.ricochet;
            }
            return bInfo;
        }
        bool isValid(vBulletLostLife blf, string tag, int layer)
        {
            return (blf.layers == (blf.layers | (1 << layer))) && blf.tags.Contains(tag);
        }

        [System.Serializable]
        public class vBulletLostLife
        {
            public LayerMask layers = 1 << 0;
            public List<string> tags = new List<string>() { "Untagged" };
            public int reduceLife = 100;
            public bool ricochet = false;
            [vHideInInspector("ricochet", true)]
            public float maxThicknessToCross = 0.2f;
            [Range(0, 100)]
            public int damageReducePercentage = 50;
            [Range(0, 90)]
            public float minChangeTrajectory = 2f;
            [Range(0, 90)]
            public float maxChangeTrajectory = 2f;

            public vBulletLostLife()
            {
                layers = 1 << 0;
                tags = new List<string>() { "Untagged" };
                reduceLife = 100;
            }
        }

        public struct vBulletLifeInfo
        {
            public int lostLife;
            public int lostDamage;
            public float minChangeTrajectory;
            public float maxChangeTrajectory;
            public float maxThicknessToCross;
            public bool ricochet;
        }
    }

}
