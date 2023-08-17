Shader "Creepy Cat/Planet_Base" 
{
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_AOalbedo("AO in albedy", Range(0.0, 2)) = 0.0
		[NoScaleOffset]_Glossiness("Glossiness", 2D) = "black" {}
		_SmoothnessShift("Smoothness shift", Range(-1, 1)) = 0.0
		_AOsmoothness("AO in smoothness", Range(-1, 1)) = 0.0
		[NoScaleOffset]_BumpMap("Normal Map", 2D) = "bump" {}
		_BumpScale("Bump Scale", Range(0.0, 1.0)) = 1.0
		[NoScaleOffset]_AOMap("AO Map", 2D) = "white" {}
		_AOintensity("AO intensity", Range(0.2, 5)) = 1.0
		[NoScaleOffset] _EmissionMap("Emission Map", 2D) = "white" {}
		_EmissionScale("Emission Scale", Float) = 0.0
	}
	CGINCLUDE
	//@TODO: should this be pulled into a shader_feature, to be able to turn it off?
	#define _GLOSSYENV 1
	#define UNITY_SETUP_BRDF_INPUT SpecularSetup
	//#define UNITY_SETUP_BRDF_INPUT MetallicSetup
	ENDCG

	SubShader
		{
			Tags{ "RenderType" = "Opaque" }
			LOD 300

			CGPROGRAM
			#pragma target 3.0
			#include "UnityPBSLighting.cginc"
			//#pragma surface surf Standard
			#pragma surface surf Standard fullforwardshadows
			//#pragma surface surf Lambert
			//#define UNITY_PASS_FORWARDBASE
			sampler2D _MainTex;
			sampler2D _BumpMap;
			sampler2D _Glossiness;
			sampler2D _AOMap;
			sampler2D _EmissionMap;

			uniform float _AOalbedo;
			uniform float _AOsmoothness;
			uniform float _SmoothnessShift;
			uniform float _EmissionScale;
			uniform float _AOintensity;
			uniform half _BumpScale;

			struct Input 
			{
				float2 uv_MainTex;
			};
	
			void surf(Input IN, inout SurfaceOutputStandard o) 
			{
				float3 albedotex = tex2D(_MainTex, IN.uv_MainTex);
				float4 glossinesstex = tex2D(_Glossiness, IN.uv_MainTex);
				float4 normaltex = tex2D(_BumpMap, IN.uv_MainTex);
				float4 ao = tex2D(_AOMap, IN.uv_MainTex);
				float3 emission = tex2D(_EmissionMap, IN.uv_MainTex);

				o.Occlusion = pow(ao.g, _AOintensity);
				o.Albedo = lerp(albedotex, float3(0.0f, 0.0f, 0.0f), saturate((1.0f-o.Occlusion) * _AOalbedo));
				o.Emission = emission.rgb * _EmissionScale;
				o.Normal = normalize(UnpackScaleNormal(normaltex, _BumpScale));
				o.Metallic = 0.0f; 
				o.Smoothness = saturate(glossinesstex.g + ((1.0f-o.Occlusion) * _AOsmoothness) + _SmoothnessShift);
			}
			ENDCG
	}
	FallBack "Diffuse"
}
