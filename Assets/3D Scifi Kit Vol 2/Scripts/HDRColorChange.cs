using UnityEngine;
using System.Collections;

public class HDRColorChange : MonoBehaviour {
	[ColorUsageAttribute(true,true,0f,8f,0.125f,8f)]
	public Color colorStart = Color.white;

	private Renderer rend;

	// Use this for initialization
	void Start () {
		rend = GetComponent<Renderer>();
		Material mat = rend.material;

		mat.EnableKeyword ("_EMISSION");
		mat.SetColor("_EmissionColor", colorStart);
		
		DynamicGI.SetEmissive(rend, colorStart);
		DynamicGI.UpdateEnvironment();
	}


}

/*
public class ColorBlink : MonoBehaviour {
	[ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
	public Color colorStart = Color.white;
	
	[ColorUsageAttribute(true,true,0f,8f,0.125f,3f)]
	public Color colorEnd = Color.green;
	
	public float duration = 1.0F;
	
	private Renderer rend;
	
	// Use this for initialization
	void Start () {
		rend = GetComponent<Renderer>();
	}
	
	// Update is called once per frame
	void LateUpdate () {
		
		
		Material mat = rend.material;
		
		//float emission = Mathf.PingPong (Time.time, 1.0f);
		//Color finalColor = colorStart* Mathf.LinearToGammaSpace (emission);
		float intensity = 2.0f;
		
		
		float lerp = Mathf.PingPong(Time.time, duration) / duration;
		mat.color = Color.Lerp(colorStart, colorEnd, lerp);
		
		
		mat.EnableKeyword ("_EMISSION");
		mat.SetColor("_EmissionColor", mat.color * intensity);
		
		DynamicGI.SetEmissive(rend, mat.color * intensity);
		DynamicGI.UpdateEnvironment();
		
	}
}
*/

