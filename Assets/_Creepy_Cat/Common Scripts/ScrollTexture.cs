using UnityEngine;
using System.Collections;

namespace creepycat.scifikitvol3 {
	
	public class ScrollTexture : MonoBehaviour {
		public float scrollSpeed = 0.5F;
		public Renderer rend;

		void Start() {
			rend = GetComponent<Renderer>();
		}

		void Update() {
			float offset = Time.time * scrollSpeed;
			rend.material.SetTextureOffset("_MainTex", new Vector2(0, -offset));
		}
	}

}