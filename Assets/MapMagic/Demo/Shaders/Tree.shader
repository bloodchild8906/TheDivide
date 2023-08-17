// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Demo/Tree" 
{
	Properties 
	{
		[Enum(Off, 0, Front, 1, Back, 2)] _Culling("Culling", Int) = 1
		_Cutoff("Alpha Ref", Range(0, 1)) = 0.33
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap("Bump Map", 2D) = "normal" {}
		_SSSVanishMap("SSS(R) Vanish(A)", 2D) = "white" {}
		_Specular ("Specular", Color) = (0,0,0,0)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		
		_SSS("SSS", Range(0,2)) = 1.5
		_Saturation("SSS Saturation", Range(0,8)) = 4
		_SSSDistance("SSS Distance", Float) = 100

		_VanishAngle("View Angle: Vanish", Range(0,1)) = 0.2
		_AppearAngle("View Angle: Appear", Range(0,1)) = 0.3

		_ShakingAmplitude("Shaking Amplitude", Float) = 0.2
		_ShakingFrequency("Shaking Frequency", Float) = 0.2

		_WindTex("Wind(XY)", 2D) = "bump" {}
		_WindSize("Wind Size", Range(0, 300)) = 50
		_WindSpeed("Wind Speed", Float) = 0.5
		_WindStrength("Wind Strength", Range(0, 4)) = 0.33

		_TranslucencyColor("Translucency Color", Color) = (0.73,0.85,0.41,1) // (187,219,106,255) 
		//for tree billboards, since default billboard shader uses it
	}

	CGINCLUDE

		//sharing same vars with shadow pass
		half _Cutoff;
		sampler2D _MainTex;
		sampler2D _BumpMap;
		sampler2D _SSSVanishMap;
		half _Glossiness;
		half4 _Specular;
		fixed4 _Color;
		half _Saturation;
		half _SSS;
		float _SSSDistance;
	ENDCG




	SubShader 
	{
		Tags 
		{ 
			"Queue" = "AlphaTest"
			"RenderType"="Overlay" 
		}
		//Cull[_Culling]
		
		CGPROGRAM
		//#pragma surface surf StandardSpecular alphatest:_Cutoff vertex:vert 
		#pragma surface surf LeafSSS vertex:vert //alphatest:_Cutoff
		#include "LightingLeafSSS.cginc"
		#include "LeafFunctions.cginc"
		#pragma target 4.0
		//#pragma debug



		//UNITY_INSTANCING_CBUFFER_START(Props)
		//	UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color)	
		//UNITY_INSTANCING_CBUFFER_END

		struct Input 
		{
			float2 uv_MainTex;
			float viewAngle;
			float viewDist;
		};

		void vert(inout appdata_full v, out Input data)
		{
			float2 vad = GetViewAngleDist(v.vertex, v.normal);
			data.viewAngle = vad.x; //for vanish
			data.viewDist = vad.y; //for sss

			v.vertex = WindShake(v.vertex, v.texcoord2);

			data.uv_MainTex = v.texcoord; //actually what is written here does not matter since it's 
		}

		void surf (Input IN, inout SurfaceOutputStandardSpecular o)
		{
			//UNITY_ACCESS_INSTANCED_PROP(_Color);
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			clip(c.a - _Cutoff);
			o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
			o.Albedo = c.rgb;
			o.Specular = _Specular;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			o.Occlusion = 1;

			//SSS
			fixed4 sssvColor = tex2D(_SSSVanishMap, IN.uv_MainTex);
			o.Emission = sssvColor.r; //storing sss value in emission
			float sssPercent = saturate( (_SSSDistance-IN.viewDist)/(_SSSDistance-_SSSDistance*0.75) );
			o.Emission *= sssPercent;

			//Vanish
			Vanish(sssvColor.a, IN.viewAngle);
		}

		ENDCG
	}

	//rendering tree for a billboard. Doesn't work in some cases
	SubShader
	{	
		Pass
		{
			Name "TreeBillboard"
			Tags{
				"IgnoreProjector" = "True"
				"RenderType" = "TreeBillboard"
				"DisableBatching" = "True"
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#pragma shader_feature _WIND

			//uniform sampler2D _MainTex;
			//uniform fixed _Cutoff;
			//uniform fixed4 _Color;
			half _AmbientPower;

			struct v2f
			{
				float2 uv : TEXCOORD0;
				fixed4 diff : COLOR0;
				float4 vertex : SV_POSITION;
				half ambient : COLOR1;
			};

			v2f vert(appdata_full v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.vertex.z -= 0.01; //moving tree in front a bit to make it render before surface shader
				o.uv = v.texcoord;
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = half4(1,0,0,1); //nl * _LightColor0;
				//o.diff.rgb += ShadeSH9(half4(worldNormal,1));
				o.ambient = v.color.w;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				//clip(col.a - _Cutoff);
				col *= (i.diff*2 + UNITY_LIGHTMODEL_AMBIENT * i.ambient * _AmbientPower);
				col *= _Color;
				col.a = 1;
				return float4(0,1,0,1); 
			}
			ENDCG
		}
	}

	//new billboards. Doesn't work in some cases too
	SubShader 
	{
		Name "TreeBillboard"
        Tags { "Queue" = "Transparent-100" "IgnoreProjector"="True" "RenderType"="TreeBillboard" }

        Pass 
		{
            ColorMask rgb
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #include "UnityCG.cginc"
            #include "TerrainEngine.cginc"

            struct v2f {
                float4 pos : SV_POSITION;
                fixed4 color : COLOR0;
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata_tree_billboard v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                TerrainBillboardTree(v.vertex, v.texcoord1.xy, v.texcoord.y);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.x = v.texcoord.x;
                o.uv.y = v.texcoord.y > 0;
                o.color = v.color;
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }

            //sampler2D _MainTex;
            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 col = tex2D( _MainTex, input.uv);
                col.rgb *= input.color.rgb;
                clip(col.a);
                UNITY_APPLY_FOG(input.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }


	//shadow caster - otherwise it displays like hell
	SubShader
	{
		Pass
		{
			Name "ShadowCaster"
			Tags { "LightMode" = "ShadowCaster" }
		   
			Cull[_Culling] //TODO: frontface culling for shadows
			Fog { Mode Off }
			ZWrite On ZTest Less
			Offset 1, 1
			 
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#pragma fragmentoption ARB_precision_hint_fastest
			#include "LeafFunctions.cginc" 
			#include "UnityCG.cginc"

			struct v2f
			{ 
				 V2F_SHADOW_CASTER;

				 float2 uv : TEXCOORD1;
				 float viewAngle : TEXCOORD2;
			};
		   
			v2f vert(appdata_full v)
			{
				v2f o;
				
				o.viewAngle = GetViewAngleDist(v.vertex, v.normal).x;
				v.vertex = WindShake(v.vertex, v.texcoord2);

				TRANSFER_SHADOW_CASTER(o)
				//TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				//#ifdef SHADOWS_CUBE
				//	o.vec = mul(unity_ObjectToWorld, v.vertex).xyz - _LightPositionRange.xyz; 
				//	o.pos = UnityObjectToClipPos(v.vertex);
				//#else
				//	//o.pos = mul(unity_ObjectToWorld, v.vertex);
				//	o.pos = UnityClipSpaceShadowCasterPos(v.vertex, v.normal); //offset should not be applied when using custom normals!
				//	o.pos = UnityApplyLinearShadowBias(o.pos);
				//#endif
				

				o.uv = v.texcoord;

				return o;
			}
		   
			float4 frag(v2f i) : COLOR
			{
				fixed4 sssvColor = tex2D(_SSSVanishMap, i.uv);
				Vanish(sssvColor.a, i.viewAngle);

				fixed4 col = tex2D(_MainTex, i.uv);
					
				clip(col.a - _Cutoff);

				return col;
			}
 
			ENDCG
		}
	}


	Dependency "BillboardShader" = "Hidden/Nature/Tree Creator Leaves Rendertex"
	//this one works

    Fallback Off

}
