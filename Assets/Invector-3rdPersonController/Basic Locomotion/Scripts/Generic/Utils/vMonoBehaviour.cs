using UnityEngine;

namespace Invector
{
    public  class vMonoBehaviour : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private bool openCloseEvents ;
        [SerializeField, HideInInspector]
        private bool openCloseWindow;
        [SerializeField, HideInInspector]       
        private int selectedToolbar;
    }  
}
