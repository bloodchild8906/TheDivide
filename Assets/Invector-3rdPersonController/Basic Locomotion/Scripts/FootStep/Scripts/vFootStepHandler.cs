using UnityEngine;
using System.Collections;
namespace Invector
{
    public class vFootStepHandler : MonoBehaviour
    {
        [Tooltip("Use this to select a specific material or texture if your mesh has multiple materials, the footstep will play only the selected index.")]
        [SerializeField]
        private int materialIndex = 0;
        public int material_ID
        {
            get
            {
                return materialIndex;
            }
        }

        public StepHandleType stepHandleType;
        public enum StepHandleType
        {
            materialName,
            textureName
        }
    }
}