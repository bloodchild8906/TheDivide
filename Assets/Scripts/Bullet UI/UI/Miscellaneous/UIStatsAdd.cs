using UnityEngine;
using UnityEngine.UI;

namespace DuloGames.UI
{
	public class UIStatsAdd : MonoBehaviour {
		
		[SerializeField] private Text m_ValueText;
		
		public void OnButtonPress()
		{
			if (this.m_ValueText == null)
				return;
			
			this.m_ValueText.text = (int.Parse(this.m_ValueText.text) + 1).ToString();
		}
	}
}
