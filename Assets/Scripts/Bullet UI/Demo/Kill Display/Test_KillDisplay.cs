using System.Collections;
using UnityEngine;

namespace DuloGames.UI
{
    public class Test_KillDisplay : MonoBehaviour
    {
        [SerializeField] private Demo_KillDisplay m_KillDisplay;
        [SerializeField] private Sprite m_Sprite;

        void OnEnable()
        {
            if (this.m_KillDisplay != null)
                this.StartCoroutine(WaitAndAdd());
        }

        IEnumerator WaitAndAdd()
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 3f));
            this.AddRandom();
            this.StartCoroutine(WaitAndAdd());
        }

        private void AddRandom()
        {
            if (this.m_KillDisplay == null)
                return;

            int randomInt = Random.Range(0, 4);
            
            switch (randomInt)
            {
                case 0:
                    this.m_KillDisplay.AddFriendlyKillHostile("UnkNown24", "[FR]NoOne", this.m_Sprite);
                    break;
                case 1:
                    this.m_KillDisplay.AddHostileKillFriendly("[FR]SomeOneZor", "TheDeadOne", this.m_Sprite);
                    break;
                case 2:
                    this.m_KillDisplay.AddFriendlyKillHostile("UnkNown24", "[FR]SomeOneZor", this.m_Sprite);
                    break;
                case 3:
                    this.m_KillDisplay.AddHostileKillHostile("[FR]TeamKiller", "[FR]TeamMate", this.m_Sprite);
                    break;
                case 4:
                    this.m_KillDisplay.AddFriendlyKillFriendly("UnkNown24", "TheVictim", this.m_Sprite);
                    break;
            }
        }
    }
}
