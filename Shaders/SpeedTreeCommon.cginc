#ifndef SPEEDTREE_COMMON_INCLUDED
#define SPEEDTREE_COMMON_INCLUDED

#include "UnityCG.cginc"

#define SPEEDTREE_Y_UP

#ifdef GEOM_TYPE_BRANCH_DETAIL
	#define GEOM_TYPE_BRANCH
#endif

#include "SpeedTreeVertex.cginc"

// Define Input structure

struct Input
{
	fixed4 color;
	half3 interpolator1;
	#ifdef GEOM_TYPE_BRANCH_DETAIL
		half3 interpolator2;
	#endif
	UNITY_DITHER_CROSSFADE_COORDS
};
	
// Define uniforms

#define mainTexUV interpolator1.xy
uniform sampler2D _MainTex;

#ifdef GEOM_TYPE_BRANCH_DETAIL
	#define Detail interpolator2
	uniform sampler2D _DetailTex;
#endif

#if defined(GEOM_TYPE_FROND) || defined(GEOM_TYPE_LEAF) || defined(GEOM_TYPE_FACING_LEAF)
	#define SPEEDTREE_ALPHATEST
	uniform fixed _Cutoff;
#endif

#ifdef EFFECT_HUE_VARIATION
	#define HueVariationAmount interpolator1.z
	uniform half4 _HueVariation;
#endif

#ifdef EFFECT_BUMP
	uniform sampler2D _BumpMap;
#endif

uniform fixed4 _Color;
uniform half _Shininess;

// Vertex processing

void SpeedTreeVert(inout SpeedTreeVB IN, out Input OUT)
{
	UNITY_INITIALIZE_OUTPUT(Input, OUT);

	OUT.mainTexUV = IN.texcoord.xy;
	OUT.color = _Color;
	OUT.color.rgb *= IN.color.r; // ambient occlusion factor

	#ifdef EFFECT_HUE_VARIATION
		float hueVariationAmount = frac(unity_ObjectToWorld[0].w + unity_ObjectToWorld[1].w + unity_ObjectToWorld[2].w);
		hueVariationAmount += frac(IN.vertex.x + IN.normal.y + IN.normal.x) * 0.5 - 0.3;
		OUT.HueVariationAmount = saturate(hueVariationAmount * _HueVariation.a);
	#endif

	#ifdef GEOM_TYPE_BRANCH_DETAIL
		// The two types are always in different sub-range of the mesh so no interpolation (between detail and blend) problem.
		OUT.Detail.xy = IN.texcoord2.xy;
		if (IN.color.a == 0) // Blend
			OUT.Detail.z = IN.texcoord2.z;
		else // Detail texture
			OUT.Detail.z = 2.5f; // stay out of Blend's .z range
	#endif

	OffsetSpeedTreeVertex(IN, unity_LODFade.x);

	UNITY_TRANSFER_DITHER_CROSSFADE(OUT, IN.vertex)
}

// Fragment processing

struct SpeedTreeFragOut
{
	fixed3 Albedo;
	fixed Alpha;
	half Specular;
	fixed Gloss;
	fixed3 Normal;
	half3 Emission;
};

// Snow layer
#ifdef ENABLE_SNOW
	fixed4 _SnowColor;
	float _SnowMinAngleCos;
	float _SnowMaxAngleCos;
	float _SnowOpacity;
	sampler2D _SnowAlbedoTex;
	sampler2D _SnowNormalTex;
	//sampler2D _SnowSpecularTex;
	sampler2D _SnowMaskTex;
	float _SnowTextureTiling;
	float _SnowMaskTiling;
#endif

#ifdef DEBUG_MODE_ON
	float4 _MainTex_TexelSize;
	#include "KiteUtils.cginc"
#endif

void SpeedTreeFrag(Input IN, half3 tangentToWorld[3], out SpeedTreeFragOut o)
{
	half4 diffuseColor = tex2D(_MainTex, IN.mainTexUV);

	o.Alpha = diffuseColor.a * _Color.a;
	#if defined(SPEEDTREE_ALPHATEST) && !defined(ANTI_ALIASING_SUPPORT)
		clip(o.Alpha - _Cutoff);
	#endif

	UNITY_APPLY_DITHER_CROSSFADE(IN)

	#ifdef GEOM_TYPE_BRANCH_DETAIL
		half4 detailColor = tex2D(_DetailTex, IN.Detail.xy);
		diffuseColor.rgb = lerp(diffuseColor.rgb, detailColor.rgb, IN.Detail.z < 2.0f ? saturate(IN.Detail.z) : detailColor.a);
	#endif

	#ifdef EFFECT_HUE_VARIATION
		half3 shiftedColor = lerp(diffuseColor.rgb, _HueVariation.rgb, IN.HueVariationAmount);
		half maxBase = max(diffuseColor.r, max(diffuseColor.g, diffuseColor.b));
		half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
		maxBase /= newMaxBase;
		maxBase = maxBase * 0.5f + 0.5f;
		// preserve vibrance
		shiftedColor.rgb *= maxBase;
		diffuseColor.rgb = saturate(shiftedColor);
	#endif

	o.Albedo = diffuseColor.rgb * IN.color.rgb;
	o.Gloss = diffuseColor.a;
	o.Specular = _Shininess;
	o.Emission = 0;

	#ifdef EFFECT_BUMP
		o.Normal = UnpackNormal(tex2D(_BumpMap, IN.mainTexUV));
	#endif

	// Calculate world-space normal
	#if defined(EFFECT_BUMP) || defined(ENABLE_SNOW)
		half3 tangent = tangentToWorld[0].xyz;
		half3 binormal = tangentToWorld[1].xyz;
		half3 normal = tangentToWorld[2].xyz;
	#endif
	#if defined(EFFECT_BUMP)
		o.Normal = normalize(tangent * o.Normal.x + binormal * o.Normal.y + normal * o.Normal.z);
	#else
		o.Normal = normalize(tangentToWorld[2].xyz);
	#endif

	// Snow layer
	#ifdef ENABLE_SNOW
		float4 snowMaskSample = tex2D(_SnowMaskTex, IN.mainTexUV * _SnowMaskTiling);
		float snowStrength = smoothstep(_SnowMinAngleCos, _SnowMaxAngleCos, o.Normal.y) * _SnowOpacity;
		snowStrength = saturate((snowStrength - snowMaskSample.r) * 8);
		float4 snowAlbedo = tex2D(_SnowAlbedoTex, IN.mainTexUV * _SnowTextureTiling);
		float3 snowNormalTS = UnpackNormal(tex2D(_SnowNormalTex, IN.mainTexUV * _SnowTextureTiling));
		float3 snowNormal = normalize(tangent * snowNormalTS.x + binormal * snowNormalTS.y + normal * snowNormalTS.z);
		//float snowSpecular = tex2D(_SnowSpecularTex, IN.mainTexUV * _SnowTextureTiling).r;

		o.Albedo = lerp(o.Albedo, snowAlbedo.rgb, snowStrength);
		o.Normal = lerp(o.Normal, snowNormal, snowStrength);
		//o.Specular = lerp(o.Specular, snowSpecular, snowStrength);
		//o.Smoothness = lerp(o.Smoothness, 0.2, snowStrength);
	#endif

#if (DEBUG_MODE_ON)
	switch (_DebugMode)
	{
	case DEBUG_LIGHTING_ONLY:
		o.Albedo = 0.5;
		break;
	case DEBUG_SHOW_NORMAL:
		o.Albedo = 0;
		o.Specular = 0;
		//o.Smoothness = 0;
		o.Emission = pow(o.Normal * 0.5 + 0.5, 2.2);
		break;
	case DEBUG_SHOW_ALBEDO:
		o.Emission = o.Albedo;
		o.Albedo = 0;
		o.Specular = 0;
		//o.Smoothness = 0;
		break;
	case DEBUG_SHOW_SPECULAR:
		o.Albedo = 0;
		break;
	case DEBUG_SHOW_SMOOTHNESS:
		o.Emission = 0;
		o.Albedo = 0;
		o.Specular = 0;
		//o.Smoothness = 0;
		break;
	case DEBUG_SHOW_POROSITY:
		o.Emission = DebugPorosity(CalculatePorosity(0));
		o.Albedo = 0;
		o.Specular = 0;
		//o.Smoothness = 0;
		break;
	case DEBUG_MIPMAPS:
		DebugMips(IN.mainTexUV * _MainTex_TexelSize.zw, o.Albedo);
		break;
	}
#endif
}

#endif // SPEEDTREE_COMMON_INCLUDED
