using UnityEngine;

namespace DuloGames.UI
{
    public class Test_UISlotBase_Assign : MonoBehaviour
    {
        [SerializeField] private UISlotBase slot;
        [SerializeField] private Texture texture;
        [SerializeField] private Sprite sprite;

        void Start()
        {
            if (this.slot != null)
            {
                if (this.texture != null)
                {
                    this.slot.Assign(this.texture);
                }
                else if (this.sprite != null)
                {
                    this.slot.Assign(this.sprite);
                }
            }
        }
    }
}
