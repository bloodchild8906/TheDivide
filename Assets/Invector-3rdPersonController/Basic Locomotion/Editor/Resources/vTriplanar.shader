// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Invector/Triplanar"
{
	Properties
	{
		[Toggle]_TriplanarAlbedo("TriplanarAlbedo", Float) = 0
		[Toggle]_TriplanarNormal("TriplanarNormal", Float) = 0
		[Toggle]_TriplanarMetallicSmoothneess("TriplanarMetallicSmoothneess", Float) = 0
		_AlbedoColor("AlbedoColor", Color) = (1,1,1,1)
		_Albedo("Albedo", 2D) = "white" {}
		[Normal]_Normal("Normal", 2D) = "bump" {}
		_NormalScale("NormalScale", Float) = 0
		_MetallicSmoothness("MetallicSmoothness", 2D) = "white" {}
		_Metallic("Metallic", Range( 0 , 1)) = 0
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		[Toggle]_TriplanarDetailAlbedo("TriplanarDetailAlbedo", Float) = 0
		[Toggle]_TriplanarDetailNormal("TriplanarDetailNormal", Float) = 0
		[Toggle]_TriplanarDetailMetallicSmoothneess("TriplanarDetailMetallicSmoothneess", Float) = 0
		_DetailColor("DetailColor", Color) = (1,1,1,1)
		_AlbedoDetail("AlbedoDetail", 2D) = "white" {}
		[Normal]_NormalDetail("NormalDetail", 2D) = "bump" {}
		_NormalDetailScale("NormalDetailScale", Float) = 0
		_MetallicSmoothnessDetail("MetallicSmoothnessDetail", 2D) = "white" {}
		_MetallicDetail("MetallicDetail", Range( 0 , 1)) = 0
		_SmoothnessDetail("SmoothnessDetail", Range( 0 , 1)) = 0
		[Toggle]_TriplanarGlobal("TriplanarGlobal", Float) = 1
		[Toggle]_ShowVertexMaskR("ShowVertexMask(R)", Float) = 0
		[Toggle]_InvertVertexColorR("InvertVertexColor(R)", Float) = 0
		_VertexMaskContrast("VertexMaskContrast", Float) = 1
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGINCLUDE
		#include "UnityStandardUtils.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 4.0
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
			float3 worldNormal;
			INTERNAL_DATA
			float4 vertexColor : COLOR;
		};

		uniform float _TriplanarNormal;
		uniform sampler2D _Normal;
		uniform float4 _Normal_ST;
		uniform float _NormalScale;
		uniform float _TriplanarGlobal;
		uniform float _TriplanarDetailNormal;
		uniform sampler2D _NormalDetail;
		uniform float4 _NormalDetail_ST;
		uniform float _NormalDetailScale;
		uniform float _InvertVertexColorR;
		uniform float _VertexMaskContrast;
		uniform float _ShowVertexMaskR;
		uniform float _TriplanarAlbedo;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float4 _AlbedoColor;
		uniform float _TriplanarDetailAlbedo;
		uniform sampler2D _AlbedoDetail;
		uniform float4 _AlbedoDetail_ST;
		uniform float4 _DetailColor;
		uniform float _TriplanarMetallicSmoothneess;
		uniform sampler2D _MetallicSmoothness;
		uniform float4 _MetallicSmoothness_ST;
		uniform float _Metallic;
		uniform float _TriplanarDetailMetallicSmoothneess;
		uniform sampler2D _MetallicSmoothnessDetail;
		uniform float4 _MetallicSmoothnessDetail_ST;
		uniform float _MetallicDetail;
		uniform float _Smoothness;
		uniform float _SmoothnessDetail;


		inline float3 TriplanarSampling31( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			xNorm.xyz  = half3( UnpackScaleNormal( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz  = half3( UnpackScaleNormal( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz  = half3( UnpackScaleNormal( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + zNorm.xyz * projNormal.z );
		}


		inline float3 TriplanarSampling32( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			xNorm.xyz  = half3( UnpackScaleNormal( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz  = half3( UnpackScaleNormal( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz  = half3( UnpackScaleNormal( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + zNorm.xyz * projNormal.z );
		}


		inline float3 TriplanarSampling81( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			xNorm.xyz  = half3( UnpackScaleNormal( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz  = half3( UnpackScaleNormal( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz  = half3( UnpackScaleNormal( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + zNorm.xyz * projNormal.z );
		}


		inline float3 TriplanarSampling82( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			xNorm.xyz  = half3( UnpackScaleNormal( xNorm, normalScale.y ).xy * float2(  nsign.x, 1.0 ) + worldNormal.zy, worldNormal.x ).zyx;
			yNorm.xyz  = half3( UnpackScaleNormal( yNorm, normalScale.x ).xy * float2(  nsign.y, 1.0 ) + worldNormal.xz, worldNormal.y ).xzy;
			zNorm.xyz  = half3( UnpackScaleNormal( zNorm, normalScale.y ).xy * float2( -nsign.z, 1.0 ) + worldNormal.xy, worldNormal.z ).xyz;
			return normalize( xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + zNorm.xyz * projNormal.z );
		}


		float4 CalculateContrast( float contrastValue, float4 colorTarget )
		{
			float t = 0.5 * ( 1.0 - contrastValue );
			return mul( float4x4( contrastValue,0,0,t, 0,contrastValue,0,t, 0,0,contrastValue,t, 0,0,0,1 ), colorTarget );
		}

		inline float4 TriplanarSampling1( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling23( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling69( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling68( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling45( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling46( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling90( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		inline float4 TriplanarSampling92( sampler2D topTexMap, float3 worldPos, float3 worldNormal, float falloff, float2 tiling, float3 normalScale, float3 index )
		{
			float3 projNormal = ( pow( abs( worldNormal ), falloff ) );
			projNormal /= ( projNormal.x + projNormal.y + projNormal.z ) + 0.00001;
			float3 nsign = sign( worldNormal );
			half4 xNorm; half4 yNorm; half4 zNorm;
			xNorm = tex2D( topTexMap, tiling * worldPos.zy * float2(  nsign.x, 1.0 ) );
			yNorm = tex2D( topTexMap, tiling * worldPos.xz * float2(  nsign.y, 1.0 ) );
			zNorm = tex2D( topTexMap, tiling * worldPos.xy * float2( -nsign.z, 1.0 ) );
			return xNorm * projNormal.x + yNorm * projNormal.y + zNorm * projNormal.z;
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Normal = i.uv_texcoord * _Normal_ST.xy + _Normal_ST.zw;
			float UseGlobal21 = (( _TriplanarGlobal )?( 1.0 ):( 0.0 ));
			float3 ase_worldPos = i.worldPos;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_worldToTangent = float3x3( ase_worldTangent, ase_worldBitangent, ase_worldNormal );
			float4 ase_vertexTangent = mul( unity_WorldToObject, float4( ase_worldTangent, 0 ) );
			float3 ase_vertexBitangent = mul( unity_WorldToObject, float4( ase_worldBitangent, 0 ) );
			float3 ase_vertexNormal = mul( unity_WorldToObject, float4( ase_worldNormal, 0 ) );
			float3x3 objectToTangent = float3x3(ase_vertexTangent.xyz, ase_vertexBitangent, ase_vertexNormal);
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float3 triplanar31 = TriplanarSampling31( _Normal, ase_vertex3Pos, ase_vertexNormal, 1.0, _Normal_ST.xy, _NormalScale, 0 );
			float3 tanTriplanarNormal31 = mul( objectToTangent, triplanar31 );
			float3 triplanar32 = TriplanarSampling32( _Normal, ase_worldPos, ase_worldNormal, 1.0, _Normal_ST.xy, _NormalScale, 0 );
			float3 tanTriplanarNormal32 = mul( ase_worldToTangent, triplanar32 );
			float3 NormalValue37 = (( _TriplanarNormal )?( ( UseGlobal21 == 1.0 ? tanTriplanarNormal31 : tanTriplanarNormal32 ) ):( UnpackScaleNormal( tex2D( _Normal, uv_Normal ), _NormalScale ) ));
			float2 uv_NormalDetail = i.uv_texcoord * _NormalDetail_ST.xy + _NormalDetail_ST.zw;
			float3 triplanar81 = TriplanarSampling81( _NormalDetail, ase_vertex3Pos, ase_vertexNormal, 1.0, _NormalDetail_ST.xy, _NormalDetailScale, 0 );
			float3 tanTriplanarNormal81 = mul( objectToTangent, triplanar81 );
			float3 triplanar82 = TriplanarSampling82( _NormalDetail, ase_worldPos, ase_worldNormal, 1.0, _NormalDetail_ST.xy, _NormalDetailScale, 0 );
			float3 tanTriplanarNormal82 = mul( ase_worldToTangent, triplanar82 );
			float3 NormalDetailValue86 = (( _TriplanarDetailNormal )?( ( UseGlobal21 == 1.0 ? tanTriplanarNormal81 : tanTriplanarNormal82 ) ):( UnpackScaleNormal( tex2D( _NormalDetail, uv_NormalDetail ), _NormalDetailScale ) ));
			float clampResult130 = clamp( (( _InvertVertexColorR )?( ( 1.0 - CalculateContrast(_VertexMaskContrast,i.vertexColor).r ) ):( CalculateContrast(_VertexMaskContrast,i.vertexColor).r )) , 0.0 , 1.0 );
			float VertexMask110 = clampResult130;
			float3 lerpResult114 = lerp( NormalValue37 , NormalDetailValue86 , VertexMask110);
			o.Normal = lerpResult114;
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 triplanar1 = TriplanarSampling1( _Albedo, ase_vertex3Pos, ase_vertexNormal, 1.0, _Albedo_ST.xy, 1.0, 0 );
			float4 triplanar23 = TriplanarSampling23( _Albedo, ase_worldPos, ase_worldNormal, 1.0, _Albedo_ST.xy, 1.0, 0 );
			float4 AlbedoValue25 = ( (( _TriplanarAlbedo )?( ( UseGlobal21 == 1.0 ? triplanar1 : triplanar23 ) ):( tex2D( _Albedo, uv_Albedo ) )) * _AlbedoColor );
			float2 uv_AlbedoDetail = i.uv_texcoord * _AlbedoDetail_ST.xy + _AlbedoDetail_ST.zw;
			float4 triplanar69 = TriplanarSampling69( _AlbedoDetail, ase_vertex3Pos, ase_vertexNormal, 1.0, _AlbedoDetail_ST.xy, 1.0, 0 );
			float4 triplanar68 = TriplanarSampling68( _AlbedoDetail, ase_worldPos, ase_worldNormal, 1.0, _AlbedoDetail_ST.xy, 1.0, 0 );
			float4 AlbedoDetailValue74 = ( (( _TriplanarDetailAlbedo )?( ( UseGlobal21 == 1.0 ? triplanar69 : triplanar68 ) ):( tex2D( _AlbedoDetail, uv_AlbedoDetail ) )) * _DetailColor );
			float4 lerpResult111 = lerp( AlbedoValue25 , AlbedoDetailValue74 , VertexMask110);
			float4 temp_cast_4 = (clampResult130).xxxx;
			o.Albedo = (( _ShowVertexMaskR )?( temp_cast_4 ):( lerpResult111 )).xyz;
			float2 uv_MetallicSmoothness = i.uv_texcoord * _MetallicSmoothness_ST.xy + _MetallicSmoothness_ST.zw;
			float4 triplanar45 = TriplanarSampling45( _MetallicSmoothness, ase_vertex3Pos, ase_vertexNormal, 1.0, _MetallicSmoothness_ST.xy, 1.0, 0 );
			float4 triplanar46 = TriplanarSampling46( _MetallicSmoothness, ase_worldPos, ase_worldNormal, 1.0, _MetallicSmoothness_ST.xy, 1.0, 0 );
			float4 break58 = (( _TriplanarMetallicSmoothneess )?( ( UseGlobal21 == 1.0 ? triplanar45 : triplanar46 ) ):( tex2D( _MetallicSmoothness, uv_MetallicSmoothness ) ));
			float Metallic51 = ( break58.x * _Metallic );
			float2 uv_MetallicSmoothnessDetail = i.uv_texcoord * _MetallicSmoothnessDetail_ST.xy + _MetallicSmoothnessDetail_ST.zw;
			float4 triplanar90 = TriplanarSampling90( _MetallicSmoothnessDetail, ase_vertex3Pos, ase_vertexNormal, 1.0, _MetallicSmoothnessDetail_ST.xy, 1.0, 0 );
			float4 triplanar92 = TriplanarSampling92( _MetallicSmoothnessDetail, ase_worldPos, ase_worldNormal, 1.0, _MetallicSmoothnessDetail_ST.xy, 1.0, 0 );
			float4 break97 = (( _TriplanarDetailMetallicSmoothneess )?( ( UseGlobal21 == 1.0 ? triplanar90 : triplanar92 ) ):( tex2D( _MetallicSmoothnessDetail, uv_MetallicSmoothnessDetail ) ));
			float MetallicDetail101 = ( break97.x * _MetallicDetail );
			float lerpResult117 = lerp( Metallic51 , MetallicDetail101 , VertexMask110);
			o.Metallic = lerpResult117;
			float Smoothness61 = ( break58.w * _Smoothness );
			float SmoothnessDetail102 = ( break97.w * _SmoothnessDetail );
			float lerpResult119 = lerp( Smoothness61 , SmoothnessDetail102 , VertexMask110);
			o.Smoothness = lerpResult119;
			o.Alpha = 1;
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float2 customPack1 : TEXCOORD1;
				float4 tSpace0 : TEXCOORD2;
				float4 tSpace1 : TEXCOORD3;
				float4 tSpace2 : TEXCOORD4;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.vertexColor = IN.color;
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
}
/*ASEBEGIN
Version=18707
7;1;1906;937;7249.011;3714.194;4.343441;True;False
Node;AmplifyShaderEditor.CommentaryNode;105;-4370.507,-1545.753;Inherit;False;2074.498;3120.262;Comment;3;75;103;104;Detail;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;64;-2053.003,-1790.01;Inherit;False;2185.85;3253.25;Comment;3;38;27;52;Main;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;103;-4315.909,698.7947;Inherit;False;1915.93;682.9655;Comment;16;87;88;89;90;91;92;93;94;95;96;97;98;99;100;101;102;DetailMetallicSmoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;52;-1957.893,644.9448;Inherit;False;1896.387;682.9651;Comment;16;51;59;60;53;43;58;50;46;45;49;41;48;47;44;42;61;MetallicSmoothness;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;27;-1946.744,-1542.512;Inherit;False;1723.788;682.9651;Comment;12;2;11;19;1;22;23;12;24;10;25;131;132;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;75;-4320.507,-1210.484;Inherit;False;1736.423;682.9651;Comment;11;65;66;67;68;69;70;71;72;73;74;133;DetailAlbedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;2;-1896.744,-1423.619;Inherit;True;Property;_Albedo;Albedo;4;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.ToggleSwitchNode;13;-3637.315,-1835.433;Float;False;Property;_TriplanarGlobal;TriplanarGlobal;20;0;Create;True;0;0;False;0;False;1;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;65;-4270.507,-1091.591;Inherit;True;Property;_AlbedoDetail;AlbedoDetail;14;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.TexturePropertyNode;87;-4265.909,817.6876;Inherit;True;Property;_MetallicSmoothnessDetail;MetallicSmoothnessDetail;17;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;126;113.0739,-857.726;Inherit;False;Property;_VertexMaskContrast;VertexMaskContrast;23;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;8;111.0191,-1054.506;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;41;-1907.893,763.8376;Inherit;True;Property;_MetallicSmoothness;MetallicSmoothness;7;0;Create;True;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WireNode;44;-1686.285,1092.136;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.WireNode;24;-1675.136,-1095.321;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TextureTransformNode;67;-4004.891,-910.9911;Inherit;False;-1;False;1;0;SAMPLER2D;;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.TextureTransformNode;88;-4000.292,998.2877;Inherit;False;-1;False;1;0;SAMPLER2D;;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.CommentaryNode;38;-1939.679,-433.379;Inherit;False;1736.424;682.9651;Comment;11;28;29;30;31;32;33;34;35;36;37;40;NormalMap;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;66;-4048.899,-763.2926;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;21;-3429.048,-1835.962;Float;False;UseGlobal;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;104;-4300.613,-225.9704;Inherit;False;1736.424;682.9655;Comment;11;76;77;78;79;80;81;82;83;84;85;86;DetailNormal;1,1,1,1;0;0
Node;AmplifyShaderEditor.TextureTransformNode;12;-1631.128,-1243.019;Inherit;False;-1;False;1;0;SAMPLER2D;;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.WireNode;89;-4044.301,1145.986;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TextureTransformNode;42;-1642.277,944.4377;Inherit;False;-1;False;1;0;SAMPLER2D;;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.SimpleContrastOpNode;128;298.1642,-976.4363;Inherit;False;2;1;COLOR;0,0,0,0;False;0;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TriplanarNode;69;-3783.754,-959.9141;Inherit;True;Spherical;Object;False;Top Texture 7;_TopTexture7;white;0;None;Mid Texture 7;_MidTexture7;white;-1;None;Bot Texture 7;_BotTexture7;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;1;-1409.991,-1291.942;Inherit;True;Spherical;Object;False;Top Texture 0;_TopTexture0;white;0;None;Mid Texture 0;_MidTexture0;white;-1;None;Bot Texture 0;_BotTexture0;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;70;-3409.129,-1002.627;Inherit;False;21;UseGlobal;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;23;-1416.867,-1084.547;Inherit;True;Spherical;World;False;Top Texture 1;_TopTexture1;white;0;None;Mid Texture 1;_MidTexture1;white;-1;None;Bot Texture 1;_BotTexture1;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;90;-3779.155,949.3647;Inherit;True;Spherical;Object;False;Top Texture 10;_TopTexture10;white;0;None;Mid Texture 10;_MidTexture10;white;-1;None;Bot Texture 10;_BotTexture10;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;68;-3790.63,-752.5185;Inherit;True;Spherical;World;False;Top Texture 6;_TopTexture6;white;0;None;Mid Texture 6;_MidTexture6;white;-1;None;Bot Texture 6;_BotTexture6;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TriplanarNode;45;-1421.14,895.5147;Inherit;True;Spherical;Object;False;Top Texture 2;_TopTexture2;white;0;None;Mid Texture 2;_MidTexture2;white;-1;None;Bot Texture 2;_BotTexture2;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BreakToComponentsNode;129;502.1642,-989.4363;Inherit;False;COLOR;1;0;COLOR;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;91;-3404.529,906.6517;Inherit;False;21;UseGlobal;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;46;-1428.016,1102.91;Inherit;True;Spherical;World;False;Top Texture 3;_TopTexture3;white;0;None;Mid Texture 5;_MidTexture5;white;-1;None;Bot Texture 5;_BotTexture5;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;28;-1889.679,-314.4859;Inherit;True;Property;_Normal;Normal;5;1;[Normal];Create;True;0;0;False;0;False;None;None;False;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.GetLocalVarNode;47;-1046.514,852.8017;Inherit;False;21;UseGlobal;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;22;-1035.366,-1334.655;Inherit;False;21;UseGlobal;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;92;-3786.031,1156.76;Inherit;True;Spherical;World;False;Top Texture 11;_TopTexture11;white;0;None;Mid Texture 11;_MidTexture11;white;-1;None;Bot Texture 11;_BotTexture11;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;76;-4250.613,-107.0775;Inherit;True;Property;_NormalDetail;NormalDetail;15;1;[Normal];Create;True;0;0;False;0;False;None;None;False;bump;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.Compare;19;-862.2614,-1236.142;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;11;-1401.155,-1492.512;Inherit;True;Property;_TextureSample0;Texture Sample 0;3;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Compare;71;-3236.024,-904.114;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Compare;48;-873.4094,951.3146;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureTransformNode;77;-3984.997,73.5229;Inherit;False;-1;False;1;0;SAMPLER2D;;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.TextureTransformNode;30;-1624.063,-133.8856;Inherit;False;-1;False;1;0;SAMPLER2D;;False;2;FLOAT2;0;FLOAT2;1
Node;AmplifyShaderEditor.SamplerNode;72;-3774.918,-1160.484;Inherit;True;Property;_TextureSample1;Texture Sample 1;3;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;94;-3770.319,748.7947;Inherit;True;Property;_TextureSample5;Texture Sample 5;3;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;29;-1668.071,13.81256;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.WireNode;79;-4029.005,221.221;Inherit;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.SamplerNode;49;-1412.304,694.9447;Inherit;True;Property;_TextureSample3;Texture Sample 3;3;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;40;-1884.061,-96.68526;Inherit;False;Property;_NormalScale;NormalScale;6;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;124;343.8102,-1180.972;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Compare;93;-3231.425,1005.165;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;78;-4244.995,110.7232;Inherit;False;Property;_NormalDetailScale;NormalDetailScale;16;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;131;-676.9653,-1364.098;Inherit;False;Property;_AlbedoColor;AlbedoColor;3;0;Create;True;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;10;-719.1732,-1488.711;Float;False;Property;_TriplanarAlbedo;TriplanarAlbedo;0;0;Create;True;0;0;False;0;False;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ToggleSwitchNode;73;-3090.277,-1156.683;Float;False;Property;_TriplanarDetailAlbedo;TriplanarDetailAlbedo;10;0;Create;True;0;0;False;0;False;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TriplanarNode;31;-1402.926,-182.8086;Inherit;True;Spherical;Object;True;Top Texture 4;_TopTexture4;white;0;None;Mid Texture 3;_MidTexture3;white;-1;None;Bot Texture 3;_BotTexture3;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;80;-3389.234,-18.1132;Inherit;False;21;UseGlobal;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;33;-1028.3,-225.5217;Inherit;False;21;UseGlobal;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;133;-3077.096,-1000.119;Inherit;False;Property;_DetailColor;DetailColor;13;0;Create;True;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;95;-3299.687,756.8478;Float;False;Property;_TriplanarDetailMetallicSmoothneess;TriplanarDetailMetallicSmoothneess;12;0;Create;True;0;0;False;0;False;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TriplanarNode;81;-3763.86,24.59992;Inherit;True;Spherical;Object;True;Top Texture 8;_TopTexture8;white;0;None;Mid Texture 8;_MidTexture8;white;-1;None;Bot Texture 8;_BotTexture8;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;122;768.17,-1181.77;Inherit;False;Property;_InvertVertexColorR;InvertVertexColor(R);22;0;Create;True;0;0;False;0;False;0;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TriplanarNode;82;-3770.736,231.9949;Inherit;True;Spherical;World;True;Top Texture 9;_TopTexture9;white;0;None;Mid Texture 9;_MidTexture9;white;-1;None;Bot Texture 9;_BotTexture9;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ToggleSwitchNode;50;-921.6719,702.9979;Float;False;Property;_TriplanarMetallicSmoothneess;TriplanarMetallicSmoothneess;2;0;Create;True;0;0;False;0;False;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TriplanarNode;32;-1409.802,24.58645;Inherit;True;Spherical;World;True;Top Texture 5;_TopTexture5;white;0;None;Mid Texture 4;_MidTexture4;white;-1;None;Bot Texture 4;_BotTexture4;white;-1;None;Triplanar Sampler;Tangent;10;0;SAMPLER2D;;False;5;FLOAT;1;False;1;SAMPLER2D;;False;6;FLOAT;0;False;2;SAMPLER2D;;False;7;FLOAT;0;False;9;FLOAT3;0,0,0;False;8;FLOAT;1;False;3;FLOAT2;1,1;False;4;FLOAT;1;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;132;-417.9653,-1377.098;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Compare;35;-855.1957,-127.0086;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;84;-3755.024,-175.9705;Inherit;True;Property;_TextureSample4;Texture Sample 4;3;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;34;-1394.09,-383.379;Inherit;True;Property;_TextureSample2;Texture Sample 2;3;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;43;-729.066,913.1169;Inherit;False;Property;_Metallic;Metallic;8;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;97;-2963.819,798.0764;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.RangedFloatNode;98;-3071.619,1068.734;Inherit;False;Property;_SmoothnessDetail;SmoothnessDetail;19;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;134;-2836.166,-1013.28;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;53;-713.6039,1014.884;Inherit;False;Property;_Smoothness;Smoothness;9;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;96;-3087.081,966.9669;Inherit;False;Property;_MetallicDetail;MetallicDetail;18;0;Create;True;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;58;-612.0302,728.73;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.Compare;83;-3216.13,80.3999;Inherit;False;0;4;0;FLOAT;0;False;1;FLOAT;1;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;130;1011.164,-1037.436;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;85;-3073.042,-172.1694;Float;False;Property;_TriplanarDetailNormal;TriplanarDetailNormal;11;0;Create;True;0;0;False;0;False;0;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;110;1124.165,-1177.538;Float;False;VertexMask;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;25;-434.3207,-1475.861;Float;False;AlbedoValue;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;-419.0094,816.8051;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;36;-712.1075,-379.5779;Float;False;Property;_TriplanarNormal;TriplanarNormal;1;0;Create;True;0;0;False;0;False;0;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;74;-2808.084,-1143.833;Float;False;AlbedoDetailValue;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;100;-2772.12,1004.303;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-414.1049,950.4531;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-2777.025,870.6551;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;26;195.9109,-766.1079;Inherit;False;25;AlbedoValue;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;101;-2633.019,863.6385;Float;False;MetallicDetail;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;51;-281.0204,809.7886;Float;False;Metallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;37;-427.2551,-366.7279;Float;False;NormalValue;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;61;-265.9636,947.1776;Float;False;Smoothness;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;86;-2788.189,-159.3195;Float;False;NormalDetailValue;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;112;157.0223,-579.7432;Inherit;False;110;VertexMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;102;-2616.269,1001.028;Float;False;SmoothnessDetail;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;106;165.5688,-679.6685;Inherit;False;74;AlbedoDetailValue;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;118;573.4484,236.7849;Inherit;False;110;VertexMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;531.0038,145.0117;Inherit;False;102;SmoothnessDetail;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;113;551.3004,-308.0634;Inherit;False;110;VertexMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;111;574.894,-737.1118;Inherit;False;3;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.GetLocalVarNode;115;511.8091,-401.313;Inherit;False;86;NormalDetailValue;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;39;542.075,-486.8524;Inherit;False;37;NormalValue;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;62;567.5157,-223.602;Inherit;False;51;Metallic;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;121;554.6287,-138.4864;Inherit;False;101;MetallicDetail;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;63;555.7031,49.30299;Inherit;False;61;Smoothness;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;116;560.1594,-49.66634;Inherit;False;110;VertexMask;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;119;816.3548,79.81663;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;114;800.1128,-435.5008;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ToggleSwitchNode;7;813.7422,-798.3528;Float;False;Property;_ShowVertexMaskR;ShowVertexMask(R);21;0;Create;True;0;0;False;0;False;0;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.LerpOp;117;806.0189,-191.8691;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;1122.627,-526.1324;Float;False;True;-1;4;;0;0;Standard;Invector/Triplanar;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;True;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;44;0;41;0
WireConnection;24;0;2;0
WireConnection;67;0;65;0
WireConnection;88;0;87;0
WireConnection;66;0;65;0
WireConnection;21;0;13;0
WireConnection;12;0;2;0
WireConnection;89;0;87;0
WireConnection;42;0;41;0
WireConnection;128;1;8;0
WireConnection;128;0;126;0
WireConnection;69;0;65;0
WireConnection;69;3;67;0
WireConnection;1;0;2;0
WireConnection;1;3;12;0
WireConnection;23;0;24;0
WireConnection;23;3;12;0
WireConnection;90;0;87;0
WireConnection;90;3;88;0
WireConnection;68;0;66;0
WireConnection;68;3;67;0
WireConnection;45;0;41;0
WireConnection;45;3;42;0
WireConnection;129;0;128;0
WireConnection;46;0;44;0
WireConnection;46;3;42;0
WireConnection;92;0;89;0
WireConnection;92;3;88;0
WireConnection;19;0;22;0
WireConnection;19;2;1;0
WireConnection;19;3;23;0
WireConnection;11;0;2;0
WireConnection;71;0;70;0
WireConnection;71;2;69;0
WireConnection;71;3;68;0
WireConnection;48;0;47;0
WireConnection;48;2;45;0
WireConnection;48;3;46;0
WireConnection;77;0;76;0
WireConnection;30;0;28;0
WireConnection;72;0;65;0
WireConnection;94;0;87;0
WireConnection;29;0;28;0
WireConnection;79;0;76;0
WireConnection;49;0;41;0
WireConnection;124;0;129;0
WireConnection;93;0;91;0
WireConnection;93;2;90;0
WireConnection;93;3;92;0
WireConnection;10;0;11;0
WireConnection;10;1;19;0
WireConnection;73;0;72;0
WireConnection;73;1;71;0
WireConnection;31;0;28;0
WireConnection;31;8;40;0
WireConnection;31;3;30;0
WireConnection;95;0;94;0
WireConnection;95;1;93;0
WireConnection;81;0;76;0
WireConnection;81;8;78;0
WireConnection;81;3;77;0
WireConnection;122;0;129;0
WireConnection;122;1;124;0
WireConnection;82;0;79;0
WireConnection;82;8;78;0
WireConnection;82;3;77;0
WireConnection;50;0;49;0
WireConnection;50;1;48;0
WireConnection;32;0;29;0
WireConnection;32;8;40;0
WireConnection;32;3;30;0
WireConnection;132;0;10;0
WireConnection;132;1;131;0
WireConnection;35;0;33;0
WireConnection;35;2;31;0
WireConnection;35;3;32;0
WireConnection;84;0;76;0
WireConnection;84;5;78;0
WireConnection;34;0;28;0
WireConnection;34;5;40;0
WireConnection;97;0;95;0
WireConnection;134;0;73;0
WireConnection;134;1;133;0
WireConnection;58;0;50;0
WireConnection;83;0;80;0
WireConnection;83;2;81;0
WireConnection;83;3;82;0
WireConnection;130;0;122;0
WireConnection;85;0;84;0
WireConnection;85;1;83;0
WireConnection;110;0;130;0
WireConnection;25;0;132;0
WireConnection;59;0;58;0
WireConnection;59;1;43;0
WireConnection;36;0;34;0
WireConnection;36;1;35;0
WireConnection;74;0;134;0
WireConnection;100;0;97;3
WireConnection;100;1;98;0
WireConnection;60;0;58;3
WireConnection;60;1;53;0
WireConnection;99;0;97;0
WireConnection;99;1;96;0
WireConnection;101;0;99;0
WireConnection;51;0;59;0
WireConnection;37;0;36;0
WireConnection;61;0;60;0
WireConnection;86;0;85;0
WireConnection;102;0;100;0
WireConnection;111;0;26;0
WireConnection;111;1;106;0
WireConnection;111;2;112;0
WireConnection;119;0;63;0
WireConnection;119;1;120;0
WireConnection;119;2;118;0
WireConnection;114;0;39;0
WireConnection;114;1;115;0
WireConnection;114;2;113;0
WireConnection;7;0;111;0
WireConnection;7;1;130;0
WireConnection;117;0;62;0
WireConnection;117;1;121;0
WireConnection;117;2;116;0
WireConnection;0;0;7;0
WireConnection;0;1;114;0
WireConnection;0;3;117;0
WireConnection;0;4;119;0
ASEEND*/
//CHKSM=BE6DD8827123393EB17A9BE54CFC0DE03E6BC7DD