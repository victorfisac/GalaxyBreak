Shader "Fisac/Gradient Cartoon"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_MainTexPower  ("Main Texture Power", Range(0,1)) = 1
		_LayoutTexture ("Layout Texture", 2D) = "white" {}
		_LayoutTexturePower ("Layout Texture Power", Range(0,1)) = 0.5
		_LightRampTexture ("Distance Light Texture", 2D) = "white" {}

 		_TopColor ("Top Color", Color) = (1,1,1,0)
 		_TopColorBottom ("Top Color Bottom", Color) = (1,1,1,0)

 		_RightColor ("Right Color", Color) = (0.9,0.9,0.9,0)
 		_RightColorBottom ("Right Color Bottom", Color) = (0.9,0.9,0.9,0)

 		_FrontColor ("Front Color", Color) = (0.7,0.7,0.7,0)
 		_FrontColorBottom ("Front Color Bottom", Color) = (0.7,0.7,0.7,0)

 		_MainColor ("Main Color Top", Color) = (0.7,0.7,0.7,0)
 		_MainColorBottom ("Main Color Bottom", Color) = (1,0.73,0.117,0)

 		_TopLight ("Top Light", Float) = 1
 		_RightLight ("Right Light", Float) = 0.9
 		_FrontLight ("Front Light", Float) = 0.7

 		_GradientStartY ("Gradient Start Y", Float) = 0
 		_GradientHeight ("Gradient Height", Float) = 1

 		_LightMaxDistance ("Light Max Distance", Float) = 10
		_LightPos ("Light position", Vector) = (0,0,0,1)
		_additiveBlend ("Additive blend", Float) = 0

 		_RimColor ("Rim Color", Color) = (0,0,0,0)
 		_RimColorBottom ("Rim Color Bottom", Color) = (0,0,0,0)
 		_RimPower ("Rim Power", Float) = 0.0
 		
 		_LightTint ("Light Tint", Color) = (1,1,1,0)
 		_AmbientColor ("Ambient Color", Color) = (0.5,0.1,0.2,0.0)
 		_AmbientPower ("Ambient Power", Float) = 0.0

 		_FogColor ("Fog color", Color) = (1,1,1,1)
		_FogYStartPos ("Fog Y-start pos", Float) = 0
		_FogHeight ("Fog Height", Float) = 0.1
		_FogAnimationHeight ("Fog Animation Height", Float) = 0.1
		_FogAnimationFreq ("Fog Animation Frequency", Float) = 0.1
		_UseFogDistance ("Distance Fog", Float) = 0

		_FogStart ("Distance Start", Float) = 0
 		_FogEnd ("Distance End", Float) = 50
 		_FogDensity ("Distance Density", Float) = 1

 		_FogStaticStartPos ("Fog static start", Vector) = (0,0,0)
 		[Toggle(FOG_STATIC_START_POS)] _UseFogStaticStart ("Fog static start", Float) = 0

		_LightmapColor ("Lightmap Tint", Color) = (0,0,0,0)
		_LightmapPower ("Lightmap Power", Float) = 1
		_ShadowPower ("Shadow Light", Float) = 0
		[Toggle(USE_LIGHTMAP)] _UseLightMap ("Lightmap Enabled", Float) = 0


		[HideInInspector]_LightDirF ("Front Light Direction", Vector) = (0,0,-1)
		[HideInInspector]_LightDirT ("Top Light Direction", Vector) = (0,1,0)
		[HideInInspector]_LightDirR ("Right Light Direction", Vector) = (1,0,0)
		[Toggle(USE_DIR_LIGHT)] _UseDirLight ("Directional Light", Float) = 0

		_Alpha ("Alpha", Range(0,1)) = 0

		_LightProbePower  ("Light Probe Power", Range(0,1)) = 0.5

		_SpecColorc ("Specular Color", Color) = (1,1,1,0)
		_Shininess ("Gloss", Range(0,8)) = 0
		_Specular ("Specular", Range(0,65)) = 0.01

		_LightMaxDistance ("Light Max Distance", Float) = 10
		_LightPos ("Light position", Vector) = (0,0,0,1)
		_additiveBlend ("Additive blend", Float) = 0

		// Blending state	
		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0

	}

	SubShader 
	{
		Tags { "QUEUE"="Geometry" "RenderType"="Opaque" }
		LOD 200

		Pass 
		{
			ZWrite [_ZWrite]
			Blend [_SrcBlend] [_DstBlend]
			Tags { "LIGHTMODE"="ForwardBase" "QUEUE"="Geometry" "RenderType"="Opaque" }

			CGPROGRAM

				#pragma shader_feature USE_LIGHTMAP
				#pragma shader_feature USE_DIR_LIGHT
				#pragma shader_feature USE_MAIN_TEX
				#pragma shader_feature USE_LAYOUT_TEXTURE
				#pragma shader_feature USE_GRADIENT
				#pragma shader_feature GRADIENT_LOCAL_SPACE
				#pragma shader_feature LIGHTING_FULL
				#pragma shader_feature USE_FOG
				#pragma shader_feature USE_DIST_FOG
				#pragma shader_feature TRANSPARENT
				#pragma shader_feature USE_DIST_LIGHT
				#pragma shader_feature DIST_LIGHT_ADDITIVE
				#pragma shader_feature USE_REALTIME_SHADOWS
				#pragma shader_feature USE_LIGHT_PROBES
				#pragma shader_feature USE_SPECULAR
				#pragma shader_feature USE_SPECULAR_PIXEL_SHADING
				#pragma shader_feature FOG_STATIC_START_POS

				#pragma multi_compile_fwdbase
				#pragma fragmentoption ARB_precision_hint_fastest
				
				#pragma vertex vert
				#pragma fragment frag

				#define MASTER_SHADER

				uniform half _MainTexPower;
				uniform half3 _RightColor;
				uniform half3 _TopColor;
				uniform half3 _FrontColor;

				uniform half3 _MainColor;
				uniform half _TopLight;
				uniform half _RightLight;
				uniform half _FrontLight;

				uniform half3 _MainColorBottom;
				uniform half3 _RightColorBottom;
				uniform half3 _TopColorBottom;
				uniform half3 _FrontColorBottom;
				uniform half3 _RimColorBottom;

				uniform half _GradientStartY;
				uniform half _GradientHeight;

				uniform half _UseFog;
				uniform half3 _RimColor;
				uniform half _RimPower;

				uniform half3 _AmbientColor;
				uniform half _AmbientPower;
				uniform half _UseLightMap;

				uniform half _LightmapPower;
				uniform half3 _LightTint;
				uniform half3 _LightmapColor;
				uniform half _ShadowPower;
				
				uniform half _LayoutTexturePower;

				uniform half3 _FogColor;
				uniform half _FogYStartPos;
				uniform half _FogHeight;
				uniform half _FogAnimationHeight;
				uniform half _FogAnimationFreq;	

				uniform half3 _FogStaticStartPos;
				uniform half _UseFogStaticStart;


				uniform half _FogStart;
				uniform half _FogEnd;
				uniform half _FogDensity;

				uniform half _LightMaxDistance;
				uniform half3 _LightPos;

				uniform half _Alpha;

				uniform half _LightProbePower;

				uniform half3 _SpecColorc;
				uniform half _Shininess;
				uniform half _Specular;

				#include "shader_include.cginc"


				CL_OUT_WPOS vert(CL_IN v)
				{
					
					#ifndef LIGHTING_FULL
						#ifdef USE_FOG
							#ifndef USE_DIST_FOG
								return customLightingSimpleSoftFogVert(v, _MainColor, _RimColor, _RimPower, _RightLight, _FrontLight, _TopLight, _AmbientColor, _AmbientPower,_FogYStartPos, _FogAnimationHeight, _FogAnimationFreq);
							#else // USE_DIST_FOG
								CL_OUT_WPOS o=customLightingSimpleSoftFogVert(v, _MainColor, _RimColor, _RimPower, _RightLight, _FrontLight, _TopLight, _AmbientColor, _AmbientPower,_FogYStartPos, 1, 1);
								#ifndef FOG_STATIC_START_POS 
								float cameraVertDist = length(_WorldSpaceCameraPos - o.wpos)*_FogDensity; 
								#else
								float cameraVertDist = length(_FogStaticStartPos - o.wpos)*_FogDensity; 
								#endif
								o.color.w = saturate((_FogEnd - cameraVertDist) / (_FogEnd - _FogStart));	
								return o;		
							#endif // USE_DIST_FOG
						#else // USE_FOG
							return customLightingWPosVertSimple(v, _MainColor, _RimColor, _RimPower, _RightLight, _FrontLight, _TopLight, _AmbientColor, _AmbientPower);
						#endif // USE_FOG
					#else // LIGHTING_FULL
						#ifdef USE_FOG
							#ifndef USE_DIST_FOG
								return customLightingSoftFogVert(v, _RimColor, _RimPower, _RightColor, _FrontColor, _TopColor, _AmbientColor, _AmbientPower,_FogYStartPos, _FogAnimationHeight, _FogAnimationFreq);
							#else
								CL_OUT_WPOS o=customLightingSoftFogVert(v, _RimColor, _RimPower, _RightColor, _FrontColor, _TopColor, _AmbientColor, _AmbientPower,_FogYStartPos, _FogAnimationHeight, _FogAnimationFreq);
								#ifndef FOG_STATIC_START_POS 
								float cameraVertDist = length(_WorldSpaceCameraPos - o.wpos)*_FogDensity; 
								#else
								float cameraVertDist = length(_FogStaticStartPos - o.wpos)*_FogDensity; 
								#endif 
								o.color.w = saturate((_FogEnd - cameraVertDist) / (_FogEnd - _FogStart));	
								return o;		
							#endif
						#else
							return customLightingWPosVert(v, _RimColor, _RimPower,_RightColor, _FrontColor, _TopColor, _AmbientColor, _AmbientPower);
						#endif
					#endif // LIGHTING_FULL


				}
				
				fixed4 frag(CL_OUT_WPOS v) : COLOR 
				{
					fixed4 o = fixed4(1,1,1,1);
					#ifdef USE_FOG
						#ifndef USE_DIST_FOG
							o = customLightingSoftFogFrag(v, _FogColor, _FogHeight, _LightTint, _UseLightMap, _LightmapPower, _LightmapColor, _ShadowPower);
						#else
							fixed4 c = customLightingSoftFogFrag(v, _FogColor, _FogHeight, _LightTint, _UseLightMap, _LightmapPower, _LightmapColor, _ShadowPower);
							o = lerp(half4(_FogColor,1),c,v.color.w);
						#endif
					#else
						o = customLightingFrag(v, _LightTint, _UseLightMap, _LightmapPower, _LightmapColor, _ShadowPower);
					#endif

					#ifdef TRANSPARENT
						o.a *=  1 - _Alpha;
					#endif 

					return o;
				}

			ENDCG
		}
	}

	FallBack "Diffuse"
	CustomEditor "GradientCartoonGUI"
}
