Shader "Transparent/VertexLit-2sided" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_SpecColor ("Spec Color", Color) = (1,1,1,0)
	_Emission ("Emissive Color", Color) = (0,0,0,0)
	_Shininess ("Shininess", Range (0.1, 1)) = 0.7
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
	Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
	LOD 100
	Pass {
		Tags { "LightMode" = "Vertex" }
		Cull Off
		Alphatest Greater 0
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha 
		ColorMask RGB
		Material {
			Diffuse [_Color]
			Ambient [_Color]
			Shininess [_Shininess]
			Specular [_SpecColor]
			Emission [_Emission]	
		}
		Lighting On
		SeparateSpecular On
		SetTexture [_MainTex] {
			Combine texture * primary DOUBLE, texture * primary
		} 
	}
}
}