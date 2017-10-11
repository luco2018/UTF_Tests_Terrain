Shader "Kite/SpeedTree"
{
	Properties
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0,0,0,0)
		_HueVariation ("Hue Variation", Color) = (1.0,0.5,0.0,0.1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.1
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		_DetailTex ("Detail", 2D) = "black" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_Cutoff ("Alpha Cutoff", Range(0,1)) = 0.333
		[MaterialEnum(Off,0,Front,1,Back,2)] _Cull ("Cull", Int) = 2
		[MaterialEnum(None,0,Fastest,1,Fast,2,Better,3,Best,4,Palm,5)] _WindQuality ("Wind Quality", Range(0,5)) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue"="Geometry"
			"IgnoreProjector"="True"
			"RenderType"="Opaque"
			"DisableBatching"="LODFading"
		}
		LOD 400
		Cull [_Cull]

		// ------------------------------------------------------------------
		//  Base forward pass (directional light, emission, lightmaps, ...)
		Pass
		{
			Name "FORWARD"
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_fwdbase
			#pragma multi_compile __ LOD_FADE_PERCENTAGE LOD_FADE_CROSSFADE
			#pragma multi_compile _ ENABLE_SNOW
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling lodfade maxcount:50
			#pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_BRANCH_DETAIL GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
			#pragma shader_feature EFFECT_BUMP
			#pragma shader_feature EFFECT_HUE_VARIATION
			#pragma shader_feature _ DEBUG_MODE_ON
			#define ENABLE_WIND
			#include "SpeedTreeCore.cginc"
			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile __ LOD_FADE_PERCENTAGE LOD_FADE_CROSSFADE
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling lodfade maxcount:50
			#pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_BRANCH_DETAIL GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
			#pragma multi_compile_shadowcaster
			#define ENABLE_WIND
			#include "SpeedTreeShadow.cginc"
			ENDCG
		}

		Pass
		{
			Name "DEFERRED"
			Tags{ "LightMode" = "Deferred" }

			CGPROGRAM
			#pragma vertex vertDeferred
			#pragma fragment fragDeferred
			#pragma target 3.0

			#pragma multi_compile ___ UNITY_HDR_ON
			#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
			#pragma multi_compile DIRLIGHTMAP_OFF DIRLIGHTMAP_COMBINED DIRLIGHTMAP_SEPARATE
			#pragma multi_compile DYNAMICLIGHTMAP_OFF DYNAMICLIGHTMAP_ON

			#pragma multi_compile __ LOD_FADE_PERCENTAGE LOD_FADE_CROSSFADE
			#pragma multi_compile _ ENABLE_SNOW
			#pragma multi_compile_instancing
			#pragma instancing_options assumeuniformscaling lodfade maxcount:50
			#pragma shader_feature GEOM_TYPE_BRANCH GEOM_TYPE_BRANCH_DETAIL GEOM_TYPE_FROND GEOM_TYPE_LEAF GEOM_TYPE_MESH
			#pragma shader_feature EFFECT_BUMP
			#pragma shader_feature EFFECT_HUE_VARIATION

			#pragma shader_feature _ DEBUG_MODE_ON
			#pragma shader_feature DISABLE_SCREEN_SPACE_AMBIENT

			#define ENABLE_WIND
			#include "SpeedTreeCore.cginc"
			ENDCG
		}
	}

	FallBack "Transparent/Cutout/VertexLit"
	CustomEditor "SpeedTreeMaterialInspector"
}
