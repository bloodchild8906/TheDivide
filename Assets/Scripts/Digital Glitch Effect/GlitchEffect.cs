/**
This work is licensed under a Creative Commons Attribution 3.0 Unported License.
http://creativecommons.org/licenses/by/3.0/deed.en_GB

You are free:

to copy, distribute, display, and perform the work
to make derivative works
to make commercial use of the work
*/

using UnityEngine;

#if UNITY_EDITOR
[AddComponentMenu ("Image Effects/GlitchEffect")]
#endif
[RequireComponent(typeof(Camera))]
public class GlitchEffect : MonoBehaviour
{
    public Shader shader;
    public Texture2D displacementMap;

    [Header("Glitch Intensity")][Range(0f, 1f)] public float intensity;
    [Range(0f, 1f)] public float flipIntensity;
    [Range(0f, 1f)] public float colorIntensity;
	
    public float minInterval = 10.0f;
    public float maxInterval = 60.0f;

    private Material m_Material;
    private float glitchup;
    private float glitchdown;
    private float flicker;
    private float displace;
    private float glitchupTime = 0.0f;
    private float glitchdownTime = 0.0f;
    private float flickerTime = 0.5f;
    private float displaceTime = 0.5f;
    
    public float randomInterval
    {
        get { return this.minInterval + (Random.value * (this.maxInterval - this.minInterval)); }
    }

    protected Material material
    {
        get
        {
            if (m_Material == null)
            {
                m_Material = new Material(shader);
                m_Material.hideFlags = HideFlags.HideAndDontSave;
            }
            return m_Material;
        }
    }

    protected void Start()
    {
        // Disable if we don't support image effects
        if (!SystemInfo.supportsImageEffects)
        {
            enabled = false;
            return;
        }

        // Disable the image effect if the shader can't
        // run on the users graphics card
        if (!shader || !shader.isSupported)
            enabled = false;
    }

    protected void OnDisable()
    {
        if (m_Material)
        {
            DestroyImmediate(m_Material);
        }
    }

    // Called by camera to apply image effect
    protected void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
		material.SetFloat("_Intensity", intensity);
        material.SetFloat("_ColorIntensity", colorIntensity);
		material.SetTexture("_DispTex", displacementMap);
        
        flicker += Time.deltaTime * colorIntensity;
        if (flicker > flickerTime){
			material.SetFloat("filterRadius", Random.Range(-3f, 3f) * colorIntensity);
            material.SetVector("direction", Quaternion.AngleAxis(Random.Range(0, 360) * colorIntensity, Vector3.forward) * Vector4.one);
            flicker = 0;
			flickerTime = Random.value;
		}

        if (colorIntensity == 0)
            material.SetFloat("filterRadius", 0);
        
        glitchup += Time.deltaTime * flipIntensity;
        if (glitchup > glitchupTime){
			if(Random.value < 0.1f * flipIntensity)
				material.SetFloat("flip_up", Random.Range(0, 1f) * flipIntensity);
			else
				material.SetFloat("flip_up", 0);
			
			glitchup = 0;
			glitchupTime = Random.value / 10f;
        }

        if (flipIntensity == 0)
            material.SetFloat("flip_up", 0);


        glitchdown += Time.deltaTime * flipIntensity;
        if (glitchdown > glitchdownTime){
			if (Random.value < 0.1f * flipIntensity)
				material.SetFloat("flip_down", 1 - Random.Range(0, 1f) * flipIntensity);
			else
				material.SetFloat("flip_down", 1);
            
			glitchdown = 0;
			glitchdownTime = Random.value / 10f;
		}

        if (flipIntensity == 0)
            material.SetFloat("flip_down", 1);

        displace += Time.deltaTime;
        if (displace > displaceTime)
        {
			material.SetFloat("displace", Random.value * intensity);
			material.SetFloat("scale", 1 - Random.value * intensity);

            if (Random.value > (0.5f * intensity))
            {
                displace = 0;
                displaceTime = this.randomInterval;
            }
        }
        else
			material.SetFloat("displace", 0);
		
		Graphics.Blit (source, destination, material);
	}
}
