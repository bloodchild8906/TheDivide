#ifndef LIGHTINGLEAFSSS_INCLUDED
#define LIGHTINGLEAFSSS_INCLUDED

#include "UnityPBSLighting.cginc"
#include "LeafFunctions.cginc"

//need a custom light model because o.Emission does not recieve shadows!
inline half4 LightingLeafSSS(inout SurfaceOutputStandardSpecular s, half3 viewDir, UnityGI gi)
{
	half mask = s.Emission;  //1;// abs(s.Albedo.g - s.Albedo.r) * 4;
	s.Emission = 0;

	half4 frontLight = LightingStandardSpecular(s, viewDir, gi);

	half ndotl = dot(s.Normal, -gi.light.dir);
	//ndotl = ndotl*0.5 + 0.5;
	ndotl = ndotl*0.75 + 0.25;
	ndotl = max(0, ndotl);
	ndotl = ndotl*ndotl;

	half4 backLight = ndotl * half4(Saturation(s.Albedo, _Saturation), 0) * half4(gi.light.color, 0);

	return frontLight + backLight*_SSS*mask;
}

inline half4 LightingLeafSSS_Deferred_disabled(inout SurfaceOutputStandardSpecular s, half3 viewDir, UnityGI gi, out half4 outGBuffer0, out half4 outGBuffer1, out half4 outGBuffer2)
{
	half mask = s.Emission;  //1;// abs(s.Albedo.g - s.Albedo.r) * 4;
	s.Emission = 0;

	half4 frontLight = LightingStandardSpecular_Deferred(s, viewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);

	half ndotl = dot(s.Normal, -gi.light.dir);
	//ndotl = ndotl*0.5 + 0.5;
	ndotl = max(0, ndotl);
	ndotl = ndotl*ndotl;

	half4 backLight = ndotl * half4(Saturation(s.Albedo, _Saturation), 0) * half4(gi.light.color, 0) * _SSS;// * mask;

	return frontLight + backLight;
}

half4 LightingLeafSSS_PrePass(SurfaceOutputStandardSpecular s, half3 viewDir, UnityGI gi)
{
	return LightingStandardSpecular(s, viewDir, gi);
}

inline void LightingLeafSSS_GI(
	SurfaceOutputStandardSpecular s,
	UnityGIInput data,
	inout UnityGI gi)
{
	LightingStandardSpecular_GI(s, data, gi);

}

#endif