// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "Creepy Cat/Glass Clean"  {
    Properties {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        
        // Colour property is used only to set influence of alpha, i.e. transparency
        _Colour ("Transparency (A only)", Color) = (0.5, 0.5, 0.5, 1)
        // Reflection map
        _Cube ("Reflection Cubemap", Cube) = "_Skybox" { TexGen CubeReflect }
        // Reflection Tint - leave as white to display reflection texture exactly as in cubemap
        _ReflectColour ("Reflection Colour", Color) = (1,1,1,0.5)
        // Reflection brightness
        _ReflectBrightness ("Reflection Brightness", Float) = 1.0
        // Specular
        _SpecularMap ("Specular / Reflection Map", 2D) = "white" {}
        // Rim
        _RimColour ("Rim Colour", Color) = (0.26,0.19,0.16,0.0)
    }
    SubShader {
        // This will be a transparent material
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
        }
        CGPROGRAM
        // Use surface shader with function called "surf"
        // Use the inbuilt BlinnPhong lighting model
        // Use alpha blending
        #pragma surface surf BlinnPhong alpha

        // Access the Shaderlab properties
        sampler2D _MainTex;
        sampler2D _SpecularMap;
        samplerCUBE _Cube;
        fixed4 _ReflectColour;
        fixed _ReflectBrightness;
        fixed4 _Colour;
        float4 _RimColour;

        struct Input {
            float2 uv_MainTex;
            float3 worldRefl; // Used for reflection map
            float3 viewDir; // Used for rim lighting
        };

        // Surface shader
        void surf (Input IN, inout SurfaceOutput o) {
        
            // Diffuse texture
            half4 c = tex2D (_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            
            // How transparent is the surface?
            o.Alpha = _Colour.a * c.a;
            
            // Specular map
            half specular = tex2D(_SpecularMap, IN.uv_MainTex).r;
            o.Specular = specular;
            
            // Reflection map
            fixed4 reflcol = texCUBE (_Cube, IN.worldRefl);
            reflcol *= c.a;
            o.Emission = reflcol.rgb * _ReflectColour.rgb * _ReflectBrightness;
            o.Alpha = o.Alpha * _ReflectColour.a;
            
            //Rim
            // Rim lighting is emissive lighting based on angle between surface normal and view direction.
            // You get more reflection at glancing angles
            half intensity = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
            o.Emission += intensity * _RimColour.rgb;
        }
        ENDCG
    } 
    FallBack "Diffuse"
}
