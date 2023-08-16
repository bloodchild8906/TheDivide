// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Invector/EmissionWithMask"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,0)
		_Difuse("Difuse", 2D) = "white" {}
		_Metallic("Metallic", Float) = 0
		_Smoothness("Smoothness", Float) = 0
		_Normal("Normal", 2D) = "bump" {}
		_NormalPower("NormalPower", Float) = 0
		_EmissionColor("EmissionColor", Color) = (0,0,0,0)
		_EmissionMask("Emission Mask", 2D) = "white" {}
		_Emission("Emission", 2D) = "white" {}
		_Emissionpower("Emission power", Float) = 1
		_EmissionTransition("EmissionTransition", Range( 0 , 1)) = 100
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		ZTest LEqual
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows 
		struct Input
		{
			fixed2 uv_texcoord;
		};

		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform fixed _NormalPower;
		uniform fixed4 _Color;
		uniform sampler2D _Difuse;
		uniform fixed _EmissionTransition;
		uniform fixed _Emissionpower;
		uniform sampler2D _Emission;
		uniform float4 _Emission_ST;
		uniform sampler2D _EmissionMask;
		uniform fixed4 _EmissionColor;
		uniform fixed _Metallic;
		uniform fixed _Smoothness;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			fixed3 tex2DNode234 = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			fixed3 tex2DNode239 = UnpackNormal( tex2D( _Normal, uv_Normal ) );
			o.Normal = (( tex2DNode234 == tex2DNode239 ) ? tex2DNode239 :  (( _NormalPower == 0.0 ) ? tex2DNode234 :  ( tex2DNode234 * _NormalPower ) ) );
			float2 uv_TexCoord184 = i.uv_texcoord * float2( 1,1 ) + float2( 0,0 );
			o.Albedo = ( _Color * tex2D( _Difuse, uv_TexCoord184 ) ).rgb;
			fixed4 _Color0 = fixed4(0,0,0,0);
			float2 uv_Emission = i.uv_texcoord * _Emission_ST.xy + _Emission_ST.zw;
			float4 clampResult231 = clamp( ( ( 10.0 - ( _EmissionTransition * 10.0 ) ) * tex2D( _EmissionMask, uv_TexCoord184 ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 lerpResult204 = lerp( ( _Emissionpower * tex2D( _Emission, uv_Emission ) ) , _Color0 , clampResult231.r);
			o.Emission = ( (( _EmissionTransition == 0.0 ) ? _Color0 :  lerpResult204 ) * _EmissionColor ).rgb;
			o.Metallic = _Metallic;
			o.Smoothness = _Smoothness;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=14001
127;427;1611;714;2016.102;442.1141;1.751905;True;False
Node;AmplifyShaderEditor.RangedFloatNode;225;-1115.701,49.90767;Float;False;Constant;_Float1;Float 1;6;0;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;202;-1116.353,167.4139;Float;False;Property;_EmissionTransition;EmissionTransition;10;0;100;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;184;-1545.983,-129.3771;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;208;-753.9263,-21.28509;Float;False;2;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;186;-1065.837,-310.1118;Float;True;Property;_EmissionMask;Emission Mask;7;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;226;-570.2495,49.78324;Float;False;2;0;FLOAT;0.0;False;1;FLOAT;0.0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;189;-705.2523,324.1106;Float;True;Property;_Emission;Emission;8;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;227;-543.9016,-244.162;Float;False;2;2;0;FLOAT;0.0;False;1;COLOR;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;200;-667.4572,248.7424;Float;False;Property;_Emissionpower;Emission power;9;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;234;-271.2045,609.6976;Float;True;Property;_Normal;Normal;4;0;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;206;-216.3532,420.7819;Float;False;Constant;_Color0;Color 0;5;0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;235;147.5006,661.5864;Float;False;Property;_NormalPower;NormalPower;5;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;207;-185.3293,256.5106;Float;False;2;2;0;FLOAT;0.0;False;1;COLOR;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;231;-251.5598,41.64429;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;236;303.4202,764.9469;Float;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;224;79.60226,427.4489;Float;False;Constant;_Float0;Float 0;6;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;204;46.02625,47.13078;Float;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;191;-126.8973,-375.5471;Float;False;Property;_Color;Color;0;0;1,1,1,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;239;-272.9558,943.6423;Float;True;Global;TextureSample0;Texture Sample 0;4;0;None;True;0;False;bump;Auto;True;Instance;234;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;242;410.2879,300.6936;Float;False;Property;_EmissionColor;EmissionColor;6;0;0,0,0,0;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCCompareEqual;237;275.3888,463.6211;Float;False;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;FLOAT3;0,0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;183;-168.125,-131.0807;Float;True;Property;_Difuse;Difuse;1;0;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0.0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1.0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TFHCCompareEqual;228;256.7383,171.6437;Float;False;4;0;FLOAT;0.0;False;1;FLOAT;0.0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;241;636.2836,171.0526;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;233;396.2709,-147.1261;Float;False;Property;_Metallic;Metallic;2;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;192;267.7587,-353.9806;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCCompareEqual;240;452.3333,605.5255;Float;False;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;232;208.8171,-22.74053;Float;False;Property;_Smoothness;Smoothness;3;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1031.008,-84.49763;Fixed;False;True;2;Fixed;ASEMaterialInspector;0;0;Standard;Invector/EmissionWithMask;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;3;False;0;0;Opaque;0;True;True;0;False;Opaque;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;False;0;255;255;0;0;0;0;0;0;0;0;False;0;4;10;25;False;0.5;True;0;Zero;Zero;0;Zero;Zero;Add;Add;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;0;0;False;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0.0;False;9;FLOAT;0.0;False;10;FLOAT;0.0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0.0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;208;0;202;0
WireConnection;208;1;225;0
WireConnection;186;1;184;0
WireConnection;226;0;225;0
WireConnection;226;1;208;0
WireConnection;227;0;226;0
WireConnection;227;1;186;0
WireConnection;207;0;200;0
WireConnection;207;1;189;0
WireConnection;231;0;227;0
WireConnection;236;0;234;0
WireConnection;236;1;235;0
WireConnection;204;0;207;0
WireConnection;204;1;206;0
WireConnection;204;2;231;0
WireConnection;237;0;235;0
WireConnection;237;1;224;0
WireConnection;237;2;234;0
WireConnection;237;3;236;0
WireConnection;183;1;184;0
WireConnection;228;0;202;0
WireConnection;228;1;224;0
WireConnection;228;2;206;0
WireConnection;228;3;204;0
WireConnection;241;0;228;0
WireConnection;241;1;242;0
WireConnection;192;0;191;0
WireConnection;192;1;183;0
WireConnection;240;0;234;0
WireConnection;240;1;239;0
WireConnection;240;2;239;0
WireConnection;240;3;237;0
WireConnection;0;0;192;0
WireConnection;0;1;240;0
WireConnection;0;2;241;0
WireConnection;0;3;233;0
WireConnection;0;4;232;0
ASEEND*/
//CHKSM=777C21294EFEC542FA3F36A428BA69E123B6EB9C