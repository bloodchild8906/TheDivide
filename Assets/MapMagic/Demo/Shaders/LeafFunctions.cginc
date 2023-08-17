#ifndef LEAFFUNCTIONS_INCLUDED
#define LEAFFUNCTIONS_INCLUDED

// Vertex function that gets current view angle (and view dist btw)

	inline float2 GetViewAngleDist(float4 pos, float3 norm) //x is view angle, y is view dist
	{
		float3 worldViewDir = _WorldSpaceCameraPos.xyz - mul(unity_ObjectToWorld, pos);
		float viewDist = length(worldViewDir);
		worldViewDir /= viewDist; //normalize

		float3 worldNormal = normalize(mul(unity_ObjectToWorld, float4(norm, 0)));

		float viewDir = dot(worldNormal, worldViewDir);

		return float2(viewDir, viewDist);
	}


// Color Saturation Slider

	inline half3 Saturation(half3 color, half saturation)
	{
		float P = sqrt(color.r*color.r*0.3 + color.g*color.g*0.5 + color.b*color.b*0.2);

		color.r = P + (color.r - P) * saturation;
		color.g = P + (color.g - P) * saturation;
		color.b = P + (color.b - P) * saturation;

		return color;
	}


// Leaf Vanish

	half _VanishAngle;
	half _AppearAngle;

	inline void Vanish(float vanishMapVal, float viewAngle)
	{
		viewAngle = abs(viewAngle); //for shadows. Actually, it does not matter if it's frontface or backface - it should be vanished if it's near perpendicular anyways.
		float vanishPercent = saturate((viewAngle - _VanishAngle) / (_AppearAngle - _VanishAngle));
		clip(vanishMapVal - (1 - vanishPercent));
	}


//Leaf Shake

	half _ShakingAmplitude;
	half _ShakingFrequency;

	sampler2D _WindTex;
	float _WindSize;
	float _WindSpeed;
	float _WindStrength;

	int _IsNonPlaymode;


	inline float4 WindShake(float4 pos, float2 uv)
	{
		float3 worldPos = mul(unity_ObjectToWorld, pos).xyz;
		float3 objectPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1)).xyz;

		//wind
		float2 windUV = (float2(_WindSpeed*_Time.y*0.33, _WindSpeed*_Time.y) + worldPos.xz) / _WindSize;
		//TODO: add inertia 
		//wind side speed is three times lower than the front one 
		//object position was divided by 2 for better effect to reduce "lag"

		half4 windColor = tex2Dlod(_WindTex, float4(windUV, 0, 0));

		float3 windDir = 0;
		windDir.xz = windColor.wy * 2 - 1; //UnpackNormal(windColor);
		windDir.y = sqrt(1 - saturate(dot(windDir.xz, windDir.xz))) - 1;
		windDir.y /= 4;

		worldPos += windDir * _WindStrength * uv.y * (1- _IsNonPlaymode);

		//shaking
		float3 random = float3(
			frac(dot(pos, float3(12.9898, 78.233, 45.5432)) ),
			frac(dot(pos, float3(12.233, 78.9898, 54.3245)) ),
			frac(dot(pos, float3(27.233, 31.791, 32.45))) );

		float curFrequency = _ShakingFrequency * uv.x;
		float3 shakePhase = cos((_Time.y + random) * curFrequency);

		float curAmplitude = _ShakingAmplitude * uv.y * (1 + length(windDir.xz)*_WindStrength);
			//_ShakingAmplitude * (uv.y + uv.x / 2) * (1 + length(windDir.xz)*_WindStrength);  //taking into account uv2.y, partly uv2.x and a wind speed
		worldPos += (shakePhase - 0.5) * curAmplitude  * (1 - _IsNonPlaymode);


		return mul(unity_WorldToObject, float4(worldPos, pos.w));
		//			return pos;
	}




#endif