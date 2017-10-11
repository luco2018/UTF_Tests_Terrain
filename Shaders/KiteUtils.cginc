#ifndef KITE_UTILS_INCLUDED
#define KITE_UTILS_INCLUDED

#include "UnityCG.cginc"

#ifdef DEBUG_MODE_ON

#define DEBUG_LIGHTING_ONLY 1
#define DEBUG_SHOW_NORMAL 2
#define DEBUG_SHOW_ALBEDO 3
#define DEBUG_SHOW_SPECULAR 4
#define DEBUG_SHOW_SMOOTHNESS 5
#define DEBUG_SHOW_POROSITY 6
#define DEBUG_MIPMAPS 7

int _DebugMode;
sampler2D _MipCheckTexture;

void DebugMips(float2 pixel_uv, inout fixed3 albedo)
{
	fixed4 mip_color = tex2D(_MipCheckTexture, pixel_uv * (1.0/16));
	albedo = lerp(albedo, mip_color.rgb, mip_color.a);
}

float3 DebugPorosity(float porosity)
{
	float3 porosityColor = float3(1, 1, 1);
	float3 oneMinusPorosityColor = float3(0, 0, 0);
	return lerp(oneMinusPorosityColor, porosityColor, porosity);
}

#endif

float CalculatePorosity(float smoothness)
{
	return saturate(saturate(((1 - smoothness) - 0.6) / 0.3));
}

float SampleR16(sampler2D tex, float2 uv)
{
#if defined(SHADER_API_OPENGL) || defined(SHADER_API_GLCORE) || defined(SHADER_API_METAL)
	return tex2Dlod(tex, float4(uv, 0, 0)).a;
#else
	return tex2Dlod(tex, float4(uv, 0, 0)).r;
#endif
}

sampler2D _HeightMap;
sampler2D _TerrainHeightMap;
float3 _TerrainOffset;
float SampleTerrainHeight(float3 pos)
{
	float2 heightMapUV = (pos.xz + float2(512.5, 512.5)) / 1025.0;
	float heightMapSample = SampleR16(_HeightMap, heightMapUV) * 600.0;
	return _TerrainOffset.y + heightMapSample;
}
float SampleRawTerrainHeight(float3 pos)
{
	float2 heightMapUV = (pos.xz + float2(512.5, 512.5)) / 1025.0;
	float heightMapSample = SampleR16(_TerrainHeightMap, heightMapUV) * 600.0;
	return _TerrainOffset.y + heightMapSample;
}

#endif