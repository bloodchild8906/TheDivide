// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Creepy Cat/Planet_Clouds_Atmosphere" 
{
	Properties{
		_MainTex("Clouds Base (RGB)", 2D) = "white" {}
		_AlphaScale("Clouds alpha Scale", Range(0.0, 2.0)) = 1.0
		[NoScaleOffset]_BumpMap("Clouds normal Map", 2D) = "bump" {}
		_BumpScale("Clouds bump Scale", Range(0.0, 1.0)) = 1.0
        _DisplacementClouds ("Clouds displacement", Range(0, .05)) = 0.005

        _Highatmospherecolor ("High atmosphere color", Color) = (0.5,0.5,0.5,0)
        _Lowatmospherecolor ("Low atmosphere color", Color) = (0.5,0.5,0.5,1)
        _Inneratmosphere ("Inner atmosphere", Color) = (0.5,0.5,0.5,1)
        _Outeratmospherelimit ("Outer atmosphere limit", Range(0, 1)) = 0.548887
        _Outeratmopsheredensity ("Outer atmopshere density", Range(0, 1)) = 1
        _Inneratmopsheredensity ("Inner atmopshere density", Range(0, 1)) = 0.08092485
        _Innerouterlimit ("Inner outer limit", Range(0, 1)) = 0.6647399
        _Inneroutersmoothness ("Inner outer smoothness", Range(0, 1)) = 0.2572428
        _DisplacementAtmosphere ("Displacement atmosphere", Range(0, .1)) = 0.1

	}
	CGINCLUDE
	#define _GLOSSYENV 1
	#define UNITY_SETUP_BRDF_INPUT SpecularSetup
	ENDCG

	SubShader
		{
			Tags
			{ 
				"RenderType" = "Transparent"
				"Queue" = "Transparent"
			}
			LOD 300
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
			
				CGPROGRAM
				#pragma target 3.0
				#include "UnityPBSLighting.cginc"
				//#pragma surface surf Standard
				#pragma surface surf Standard fullforwardshadows alpha vertex:vert
				//#pragma surface surf Lambert
				//#define UNITY_PASS_FORWARDBASE
				sampler2D _MainTex;
				sampler2D _BumpMap;
				uniform float _BumpScale;
				uniform float _AlphaScale;
	            uniform float _DisplacementClouds;

				struct Input 
				{
					float2 uv_MainTex;
				};

				void vert (inout appdata_full v) 
				{
					v.vertex.xyz += v.normal * _DisplacementClouds;
				}

				void surf(Input IN, inout SurfaceOutputStandard o) 
				{
					float4 albedotex = tex2D(_MainTex, IN.uv_MainTex);
					float4 normaltex = tex2D(_BumpMap, IN.uv_MainTex);

					o.Albedo = albedotex.rgb;
					o.Alpha = saturate(albedotex.a * _AlphaScale);
					o.Normal = normalize(UnpackScaleNormal(normaltex, _BumpScale));
					o.Metallic = 0.0f;
					o.Smoothness = 0.0f;
				}
				ENDCG

				        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            //Offset 10, -1
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase
            //#pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 

            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform float4 _Highatmospherecolor;
            uniform float4 _Lowatmospherecolor;
            uniform float _Outeratmopsheredensity;
            uniform float _Inneratmopsheredensity;
            uniform float _Innerouterlimit;
            uniform float _Inneroutersmoothness;
            uniform float _Outeratmospherelimit;
            uniform float4 _Inneratmosphere;
            uniform float _DisplacementAtmosphere;

            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
            };

            VertexOutput vert (VertexInput v) 
			{
				float4 vpos = v.vertex + float4(normalize(v.normal)*_DisplacementAtmosphere, 0.0f);

                VertexOutput o = (VertexOutput)0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, vpos);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(vpos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR 
			{
                i.normalDir = normalize(i.normalDir);
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
                float attenuation = 1;
                float3 attenColor = _LightColor0.xyz;
                float NdotL = max(0, dot( normalDirection, lightDirection ));
/////// Diffuse:
                NdotL = dot( normalDirection, lightDirection );
                float3 w = float3(.25f,.25f,.25f)*0.5; // Light wrapping
                float3 NdotLWrap = NdotL * ( 1.0 - w );
                float3 forwardLight = max(float3(0.0,0.0,0.0), NdotLWrap + w );
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = forwardLight * attenColor;
                float ramp1 = smoothstep( 0.0 , _Outeratmospherelimit , pow(1.0-max(0,dot(i.normalDir, viewDirection)),7.0) );
                float ramp2 = smoothstep( max((_Innerouterlimit-_Inneroutersmoothness),0.0) , min((_Innerouterlimit+_Inneroutersmoothness),1.0) , 1.0-max(0,dot(i.normalDir, viewDirection)));
                float3 diffuseColor = lerp(_Inneratmosphere.rgb,lerp(_Lowatmospherecolor.rgb,_Highatmospherecolor.rgb,ramp1),ramp2);
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                return fixed4(finalColor,(lerp(_Inneratmopsheredensity,((1.0 - ramp1)*_Outeratmopsheredensity),ramp2)*saturate((dot(lightDirection,i.normalDir)*2.0))));
            }
            ENDCG
        }
        Pass {
            Name "FORWARD_DELTA"
            Tags {
                "LightMode"="ForwardAdd"
            }
            Blend One One
            ZWrite Off
            Offset -1, -1
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDADD
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #pragma multi_compile_fwdadd
            #pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
            #pragma target 3.0
            uniform float4 _LightColor0;
            uniform float4 _Highatmospherecolor;
            uniform float4 _Lowatmospherecolor;
            uniform float _Outeratmopsheredensity;
            uniform float _Inneratmopsheredensity;
            float Smoothstep1( float Minvalue , float Maxvalue , float Value ){
            return smoothstep(Minvalue, Maxvalue, Value);
            
            }
            
            float Smoothstep2( float Minvalue , float Maxvalue , float Value ){
            return smoothstep(Minvalue, Maxvalue, Value);
            
            }
            
            uniform float _Innerouterlimit;
            uniform float _Inneroutersmoothness;
            uniform float _Outeratmospherelimit;
            uniform float4 _Inneratmosphere;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                LIGHTING_COORDS(2,3)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                float3 lightColor = _LightColor0.rgb;
                o.pos = UnityObjectToClipPos(v.vertex);
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                i.normalDir = normalize(i.normalDir);
/////// Vectors:
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
                float3 lightDirection = normalize(lerp(_WorldSpaceLightPos0.xyz, _WorldSpaceLightPos0.xyz - i.posWorld.xyz,_WorldSpaceLightPos0.w));
                float3 lightColor = _LightColor0.rgb;
                float3 halfDirection = normalize(viewDirection+lightDirection);
////// Lighting:
                float attenuation = LIGHT_ATTENUATION(i);
                float3 attenColor = attenuation * _LightColor0.xyz;
                float NdotL = max(0, dot( normalDirection, lightDirection ));
/////// Diffuse:
                NdotL = dot( normalDirection, lightDirection );
                float3 w = float3(.25f,.25f,.25f)*0.5; // Light wrapping
                float3 NdotLWrap = NdotL * ( 1.0 - w );
                float3 forwardLight = max(float3(0.0,0.0,0.0), NdotLWrap + w );
                NdotL = max(0.0,dot( normalDirection, lightDirection ));
                float3 directDiffuse = forwardLight * attenColor;
                float ramp1 = smoothstep( 0.0 , _Outeratmospherelimit , pow(1.0-max(0,dot(i.normalDir, viewDirection)),7.0) );
                float ramp2 = Smoothstep2( max((_Innerouterlimit-_Inneroutersmoothness),0.0) , min((_Innerouterlimit+_Inneroutersmoothness),1.0) , 1.0-max(0,dot(i.normalDir, viewDirection)) );
                float3 diffuseColor = lerp(_Inneratmosphere.rgb,lerp(_Lowatmospherecolor.rgb,_Highatmospherecolor.rgb,ramp1),ramp2);
                float3 diffuse = directDiffuse * diffuseColor;
/// Final Color:
                float3 finalColor = diffuse;
                return fixed4(finalColor * (lerp(_Inneratmopsheredensity,((1.0 - ramp1)*_Outeratmopsheredensity),ramp2)*saturate((dot(lightDirection,i.normalDir)*2.0))),0);
            }
            ENDCG
        }
	}
	FallBack "Diffuse"
}
