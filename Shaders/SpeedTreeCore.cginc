#ifndef SPEEDTREE_CORE_INCLUDED
#define SPEEDTREE_CORE_INCLUDED

#include "SpeedTreeCommon.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

struct KiteLight
{
	half3 color;
	half3 dir;
};

KiteLight MainLight(half atten)
{
	KiteLight l;
	l.color = _LightColor0.rgb * atten;
	l.dir = _WorldSpaceLightPos0.xyz;
	return l;
}

half3 AmbientVert(SpeedTreeVB v)
{
	float4 posWorld = mul(unity_ObjectToWorld, v.vertex);
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
	half3 ambient = 0;
	#if UNITY_SHOULD_SAMPLE_SH
		#if UNITY_SAMPLE_FULL_SH_PER_PIXEL
			ambient = 0;
		#elif (SHADER_TARGET < 30)
			ambient = ShadeSH9(half4(normalWorld, 1.0));
		#else
			// Optimization: L2 per-vertex, L0..L1 per-pixel
			ambient = ShadeSH3Order(half4(normalWorld, 1.0));
		#endif
		// Add approximated illumination from non-important point lights
		#ifdef VERTEXLIGHT_ON
			ambient += Shade4PointLights (
				unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
				unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
				unity_4LightAtten0, posWorld, normalWorld);
		#endif
		return ambient;
	#endif
	return 0;
}

half3 AmbientFrag(half3 ambientV, half3 normalWorld)
{
	#if UNITY_SHOULD_SAMPLE_SH
		#if UNITY_SAMPLE_FULL_SH_PER_PIXEL
			half3 sh = ShadeSH9(half4(normalWorld, 1.0));
		#elif (SHADER_TARGET >= 30) && !UNITY_STANDARD_SIMPLE
			half3 sh = ambientV + ShadeSH12Order(half4(normalWorld, 1.0));
		#else
			half3 sh = ambientV;
		#endif
		return sh;
	#endif
	return 0;
}

fixed4 LightingSpeedTree(SpeedTreeFragOut s, KiteLight light, half3 ambientV)
{
	fixed diff = max(0, dot(s.Normal, light.dir));

	fixed4 c;
	c.rgb = s.Albedo * light.color * diff;
	c.a = s.Alpha;

	c.rgb += s.Albedo * AmbientFrag(ambientV, s.Normal);

	return c;
}

void AddTangentAndNormal(SpeedTreeVB v, out half3 tangentToWorld[3])
{
	float3 normalWorld = UnityObjectToWorldNormal(v.normal);
	#ifdef EFFECT_BUMP
		float4 tangentWorld = float4(UnityObjectToWorldDir(v.tangent.xyz), v.tangent.w);
		float3x3 _tangentToWorld = CreateTangentToWorldPerVertex(normalWorld, tangentWorld.xyz, tangentWorld.w);
		tangentToWorld[0] = _tangentToWorld[0];
		tangentToWorld[1] = _tangentToWorld[1];
		tangentToWorld[2] = _tangentToWorld[2];
	#else
		tangentToWorld[0] = 0;
		tangentToWorld[1] = 0;
		tangentToWorld[2] = normalWorld;
	#endif
}

struct v2f 
{
	float4 pos				: SV_POSITION;
	Input data				: TEXCOORD0;
	half3 tangentToWorld[3]	: TEXCOORD4;
	half3 ambientV			: TEXCOORD7;
	SHADOW_COORDS(8)
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

v2f vert(SpeedTreeVB v)
{
	v2f o;

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	SpeedTreeVert(v, o.data);

	o.pos = UnityObjectToClipPos(v.vertex);
	AddTangentAndNormal(v, o.tangentToWorld);
	o.ambientV = AmbientVert(v);

	TRANSFER_SHADOW(o);

	return o;
}

struct fragOutput
{
	float4 color : SV_Target;
#if defined(ANTI_ALIASING_SUPPORT) && defined(SPEEDTREE_ALPHATEST)
	uint coverage : SV_Coverage;
#endif
};

fragOutput frag(v2f i)
{
	UNITY_SETUP_INSTANCE_ID(i);
	fragOutput fo;

	#if defined(ANTI_ALIASING_SUPPORT) && defined(SPEEDTREE_ALPHATEST)
		#if !defined(MSAA_2X) && !defined(MSAA_4X)
			float alpha = tex2D(_MainTex, i.data.mainTexUV).a * _Color.a;
			if (alpha > _Cutoff)
				fo.coverage = 1;
			else
				fo.coverage = 0;
		#else
			#ifdef MSAA_2X
				static const uint nSampleCount = 2;
				static const float2 vMSAAOffsets[2] = {float2(0.25, 0.25), float2(-0.25, -0.25)};
			#endif
			#ifdef MSAA_4X
				static const uint nSampleCount = 4;
				static const float2 vMSAAOffsets[4] = {float2(-0.125, -0.375), float2(0.375, -0.125), float2(-0.375, 0.125), float2(0.125, 0.375)};
			#endif
			const float2 vDDX = ddx(i.data.mainTexUV);
			const float2 vDDY = ddy(i.data.mainTexUV);
			fo.coverage = 0;
			[unroll] for (uint s = 0; s < nSampleCount; ++s)
			{
				float2 shift = vMSAAOffsets[s].x * vDDX + vMSAAOffsets[s].y * vDDY;
				float alpha = tex2D(_MainTex, i.data.mainTexUV + shift).a * _Color.a;
				fo.coverage |= ((alpha - _Cutoff) >= 0) ? (1u << s) : 0;
			}
		#endif
	#endif

	SpeedTreeFragOut o;
	SpeedTreeFrag(i.data, i.tangentToWorld, o);

	fo.color = LightingSpeedTree(o, MainLight(SHADOW_ATTENUATION(i)), i.ambientV);
	fo.color.rgb += o.Emission;

	return fo;
}

// Deferred

struct v2fDeferred
{
	float4 pos				: SV_POSITION;
	Input data : TEXCOORD0;
	half3 tangentToWorld[3]	: TEXCOORD4;
	half3 ambientV			: TEXCOORD7;
	SHADOW_COORDS(8)
	UNITY_VERTEX_INPUT_INSTANCE_ID
	UNITY_VERTEX_OUTPUT_STEREO
};

v2fDeferred vertDeferred(SpeedTreeVB v)
{
	v2fDeferred o;

	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

	SpeedTreeVert(v, o.data);

	o.pos = UnityObjectToClipPos(v.vertex);
	AddTangentAndNormal(v, o.tangentToWorld);
	o.ambientV = AmbientVert(v);

	TRANSFER_SHADOW(o);

	return o;
}

struct fragOutputDeferred
{
	half4 outDiffuse : SV_Target0;			// RT0: diffuse color (rgb), occlusion (a)
	half4 outSpecSmoothness : SV_Target1;	// RT1: spec color (rgb), smoothness (a)
	half4 outNormal : SV_Target2;			// RT2: normal (rgb), --unused, very low precision-- (a) 
#if defined(DISABLE_SCREEN_SPACE_AMBIENT) || defined(DEBUG_MODE_ON)
	half4 outEmission : SV_Target3;			// RT3: emission (rgb), --unused-- (a)
#endif
};

fragOutputDeferred fragDeferred(v2fDeferred i)
{
	UNITY_SETUP_INSTANCE_ID(i);
	fragOutputDeferred o;

	SpeedTreeFragOut s;
	SpeedTreeFrag(i.data, i.tangentToWorld, s);

#ifndef DISABLE_SCREEN_SPACE_AMBIENT
	fixed3 c = 0;
#else
	fixed3 c = s.Albedo * AmbientFrag(i.ambientV, s.Normal);
#endif

	c += s.Emission;

#ifndef UNITY_HDR_ON
	c.rgb = exp2(-c.rgb);
#endif

	o.outDiffuse = half4(s.Albedo, 1);
	//o.outSpecSmoothness = half4(s.Specular, s.Smoothness);
	o.outSpecSmoothness = half4(0, 0, 0, 0);
	o.outNormal = half4(s.Normal*0.5 + 0.5, 1);
#if defined(DISABLE_SCREEN_SPACE_AMBIENT) || defined(DEBUG_MODE_ON)
	o.outEmission = half4(c.rgb, 1);
#endif
	return o;
}

#endif // SPEEDTREE_CORE_INCLUDED
