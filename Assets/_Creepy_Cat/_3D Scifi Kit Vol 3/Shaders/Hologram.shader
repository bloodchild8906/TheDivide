Shader "Creepy Cat/Hologram" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _HoloTex ("Hologram Pattern", 2D) = "white" {}
        _HoloSpace ("Hologram Spacing", Range(1,10)) = 1.0
        _HoloSpeed ("Hologram Speed", Range(1,10)) = 1.0
        _FresnelColor ("Fresnel Color", Color) = (1,0,0,1)
        _FresnelIntensity ("Fresnel Intensity", Range(0,1)) = 0.5
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        LOD 200
        Zwrite Off
        
        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:blend

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex, _HoloTex;

        struct Input {
            float2 uv_MainTex;
            float4 screenPos;
            float3 viewDir;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color, _FresnelColor;
        half _HoloSpace, _HoloSpeed, _FresnelIntensity;

        void surf (Input IN, inout SurfaceOutputStandard o) {

            float3 N = o.Normal;
            float anguloVision = dot(IN.viewDir, N);
            o.Emission = _FresnelColor.rgb * -(1- 1/anguloVision) * _FresnelIntensity;

            float2 uv2 = (IN.screenPos/IN.screenPos.w).xy;
            uv2.y *= _HoloSpace;
            uv2.y += _Time.x * _HoloSpeed * _HoloSpace;

            //float a = 1.0;
            half a = tex2D (_HoloTex, uv2).rgba;

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb * _Color;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 0.25 + a;
            o.Emission += (a*_Color);
        }
        ENDCG
    }
    FallBack "Diffuse"
}