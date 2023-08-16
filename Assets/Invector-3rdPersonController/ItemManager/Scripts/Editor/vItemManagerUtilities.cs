using UnityEngine;
using System.Collections.Generic;

namespace Invector.vItemManager
{   
    public static class vItemManagerUtilities
    {  
        public static void CreateDefaultEquipPoints(vItemManager itemManager)
        {

            var animator = itemManager.GetComponent<Animator>();
            if (itemManager.equipPoints == null)
                itemManager.equipPoints = new List<EquipPoint>();

            #region LeftEquipPoint
            var equipPointL = itemManager.equipPoints.Find(p => p.equipPointName == "LeftArm");
            if (equipPointL == null)
            {
                EquipPoint pointL = new EquipPoint();
                pointL.equipPointName = "LeftArm";              
                itemManager.equipPoints.Add(pointL);
            }          
            #endregion

            #region RightEquipPoint
            var equipPointR = itemManager.equipPoints.Find(p => p.equipPointName == "RightArm");
            if (equipPointR == null)
            {
                EquipPoint pointR = new EquipPoint();
                pointR.equipPointName = "RightArm";              
                itemManager.equipPoints.Add(pointR);
            }
           
            #endregion
        }     
    }
}
