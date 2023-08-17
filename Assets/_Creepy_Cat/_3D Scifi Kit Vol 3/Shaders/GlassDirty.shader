Shader "Creepy Cat/Glass Dirty" {
    Properties {
        _Color("Tint Color (RGB)", Color) = (1, 1, 1, 1)
        _DirtTex("Dirt Texture (RGB)", 2D) = "black" {}
        _MaskTex("Reflection Mask (R)", 2D) = "white" {}
        _MaskPower("Mask Power", Range(0.0, 1.0)) = 0.5
        _Reflectivity("Reflections Power", Range(0.0, 1.0)) = 0.5
    }
 
    SubShader {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "IgnoreProjector" = "True" }
        LOD 100
       
        Pass {
            Name "BASE"
            Tags { "LightMode" = "Always" }
           
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
 
            CGPROGRAM
            #include "UnityCG.cginc"
            //#pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
 
            fixed4 _Color;
            sampler2D _DirtTex, _MaskTex;
            float4 _DirtTex_ST, _MaskTex_ST;
            float _MaskPower, _Reflectivity;
 
            struct a2v {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            struct v2f {
                float4 pos : SV_POSITION;
                float4 coord0 : TEXCOORD0;
                float3 norm : TEXCOORD1;
                float3 eye : TEXCOORD2;
                UNITY_FOG_COORDS(3)
                UNITY_VERTEX_OUTPUT_STEREO
            };
 
            v2f vert (a2v v) {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.coord0.xy = TRANSFORM_TEX(v.texcoord, _DirtTex);
                o.coord0.zw = TRANSFORM_TEX(v.texcoord, _MaskTex);
                o.norm = UnityObjectToWorldNormal(v.normal);
                o.eye = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, v.vertex).xyz;
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }
 
            fixed4 frag(v2f i) : SV_Target {
                fixed3 dirt = tex2D(_DirtTex, i.coord0.xy).rgb;
                fixed mask = tex2D(_MaskTex, i.coord0.zw).r * _MaskPower;
                fixed4 refl = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, reflect(-i.eye, i.norm));
                refl.rgb = DecodeHDR(refl, unity_SpecCube0_HDR) * _Reflectivity;
                fixed3 result = lerp(refl * _Color.rgb, dirt, mask);
                UNITY_APPLY_FOG(i.fogCoord, result);
                return fixed4(result, saturate(mask + _Reflectivity));
            }
            ENDCG
        }
    }
 
    Fallback Off
}