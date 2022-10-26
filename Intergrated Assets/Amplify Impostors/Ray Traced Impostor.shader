Shader "RayTracing/Amplify Impostor"
{
	Properties
	{
		[NoScaleOffset] _Albedo("Albedo & Alpha", 2D) = "white" {}
		[NoScaleOffset]_Normals("Normals & Depth", 2D) = "white" {}
		[NoScaleOffset]_Specular("Specular & Smoothness", 2D) = "black" {}
		[NoScaleOffset]_Emission("Emission & Occlusion", 2D) = "black" {}
		[HideInInspector]_Frames("Frames", Float) = 16
		[HideInInspector]_ImpostorSize("Impostor Size", Float) = 1
		[HideInInspector]_Offset("Offset", Vector) = (0,0,0,0)
		[HideInInspector]_AI_SizeOffset("Size & Offset", Vector) = (0,0,0,0)
		_TextureBias("Texture Bias", Float) = -1
		_Parallax("Parallax", Range(-1 , 1)) = 1
		[HideInInspector]_DepthSize("DepthSize", Float) = 1
		_ClipMask("Clip", Range(0 , 1)) = 0.5
		_AI_ShadowBias("Shadow Bias", Range(0 , 2)) = 0.25
		_AI_ShadowView("Shadow View", Range(0 , 1)) = 1
		[Toggle(_HEMI_ON)] _Hemi("Hemi", Float) = 0
		[Toggle(EFFECT_HUE_VARIATION)] _Hue("Use SpeedTree Hue", Float) = 0
		_HueVariation("Hue Variation", Color) = (0,0,0,0)
		[Toggle] _AI_AlphaToCoverage("Alpha To Coverage", Float) = 0
	}

	SubShader
	{
		CGINCLUDE

		#include "Assets/Ray-Marching/Shaders/PrimitivesScene_Sampler.cginc"
		#include "Assets/Ray-Marching/Shaders/Signed_Distance_Functions.cginc"
		#include "Assets/Ray-Marching/Shaders/RayMarching_Forward_Integration.cginc"
		#include "Assets/Ray-Marching/Shaders/Sampler_TopDownLight.cginc"

		#pragma target 3.0
		#define UNITY_SAMPLE_FULL_SH_PER_PIXEL 1

		#pragma shader_feature _HEMI_ON
		#pragma shader_feature EFFECT_HUE_VARIATION
		ENDCG

		Tags { "RenderType" = "Opaque" "Queue" = "Geometry" "DisableBatching" = "True" }
		Cull Back
		AlphaToMask[_AI_AlphaToCoverage]

		Pass
		{
			ZWrite On
			Name "ForwardBase"
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM
		// compile directives
		#pragma vertex vert_surf
		#pragma fragment frag_surf
		#pragma multi_compile_fog
		#pragma multi_compile_fwdbase
		#pragma multi_compile_instancing
		#pragma multi_compile __ LOD_FADE_CROSSFADE
		#include "HLSLSupport.cginc"
		#if !defined( UNITY_INSTANCED_LOD_FADE )
			#define UNITY_INSTANCED_LOD_FADE
		#endif
		#if !defined( UNITY_INSTANCED_SH )
			#define UNITY_INSTANCED_SH
		#endif
		#if !defined( UNITY_INSTANCED_LIGHTMAPSTS )
			#define UNITY_INSTANCED_LIGHTMAPSTS
		#endif
		#include "UnityShaderVariables.cginc"
		#include "UnityShaderUtilities.cginc"
		#ifndef UNITY_PASS_FORWARDBASE
		#define UNITY_PASS_FORWARDBASE
		#endif
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "UnityPBSLighting.cginc"
		#include "AutoLight.cginc"
		#include "UnityStandardUtils.cginc"

		#include "Assets\AmplifyImpostors\Plugins\EditorResources\Shaders\Runtime\AmplifyImpostors.cginc"

		struct v2f_surf 
		{
			UNITY_POSITION(pos);
			float4 uvsFrame1 : TEXCOORD1;
			float4 uvsFrame2 : TEXCOORD2;
			float4 uvsFrame3 : TEXCOORD3;
			float4 octaFrame : TEXCOORD4;
			float4 viewPos : TEXCOORD5;
			#if UNITY_VERSION >= 201810
				UNITY_LIGHTING_COORDS(6,7)
			#else
				UNITY_SHADOW_COORDS(6)
			#endif
			UNITY_FOG_COORDS(8)
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};


		v2f_surf vert_surf(appdata_full v) {
			UNITY_SETUP_INSTANCE_ID(v);
			v2f_surf o;
			UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
			UNITY_TRANSFER_INSTANCE_ID(v,o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			OctaImpostorVertex(v.vertex, v.normal, o.uvsFrame1, o.uvsFrame2, o.uvsFrame3, o.octaFrame, o.viewPos);

			o.pos = UnityObjectToClipPos(v.vertex);

			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			fixed3 worldNormal = UnityObjectToWorldNormal(v.normal);

			#if UNITY_VERSION >= 201810
				UNITY_TRANSFER_LIGHTING(o, v.texcoord1.xy);
			#else
				UNITY_TRANSFER_SHADOW(o, v.texcoord1.xy);
			#endif
			UNITY_TRANSFER_FOG(o,o.pos);
			return o;
		}

		inline void OctaImpostorFragment_My(inout SurfaceOutputStandardSpecular o, out float4 clipPos, out float3 worldPos, float4 uvsFrame1, float4 uvsFrame2, float4 uvsFrame3, float4 octaFrame, float4 interpViewPos)
		{
			float depthBias = -1.0;
			float textureBias = _TextureBias;

			// Weights
			float2 fraction = frac(octaFrame.xy);
			float2 invFraction = 1 - fraction;
			float3 weights;
			weights.x = min(invFraction.x, invFraction.y);
			weights.y = abs(fraction.x - fraction.y);
			weights.z = min(fraction.x, fraction.y);

			float4 parallaxSample1 = tex2Dbias(_Normals, float4(uvsFrame1.zw, 0, depthBias));
			float2 parallax1 = ((0.5 - parallaxSample1.a) * uvsFrame1.xy) + uvsFrame1.zw;
			float4 parallaxSample2 = tex2Dbias(_Normals, float4(uvsFrame2.zw, 0, depthBias));
			float2 parallax2 = ((0.5 - parallaxSample2.a) * uvsFrame2.xy) + uvsFrame2.zw;
			float4 parallaxSample3 = tex2Dbias(_Normals, float4(uvsFrame3.zw, 0, depthBias));
			float2 parallax3 = ((0.5 - parallaxSample3.a) * uvsFrame3.xy) + uvsFrame3.zw;

			// albedo alpha
			float4 albedo1 = tex2Dbias(_Albedo, float4(parallax1, 0, textureBias));
			float4 albedo2 = tex2Dbias(_Albedo, float4(parallax2, 0, textureBias));
			float4 albedo3 = tex2Dbias(_Albedo, float4(parallax3, 0, textureBias));
			float4 blendedAlbedo = albedo1 * weights.x + albedo2 * weights.y + albedo3 * weights.z;

			// early clip
			o.Alpha = (blendedAlbedo.a - _ClipMask);
			clip(o.Alpha);

#if AI_CLIP_NEIGHBOURS_FRAMES
			float t = ceil(fraction.x - fraction.y);
			float4 cornerDifference = float4(t, 1 - t, 1, 1);

			float2 step_1 = (parallax1 - octaFrame.zw) * _Frames;
			float4 step23 = (float4(parallax2, parallax3) - octaFrame.zwzw) * _Frames - cornerDifference;

			step_1 = step_1 * (1 - step_1);
			step23 = step23 * (1 - step23);

			float3 steps;
			steps.x = step_1.x * step_1.y;
			steps.y = step23.x * step23.y;
			steps.z = step23.z * step23.w;
			steps = step(-steps, 0);

			float final = dot(steps, weights);

			clip(final - 0.5);
#endif

#ifdef EFFECT_HUE_VARIATION
			half3 shiftedColor = lerp(blendedAlbedo.rgb, _HueVariation.rgb, interpViewPos.w);
			half maxBase = max(blendedAlbedo.r, max(blendedAlbedo.g, blendedAlbedo.b));
			half newMaxBase = max(shiftedColor.r, max(shiftedColor.g, shiftedColor.b));
			maxBase /= newMaxBase;
			maxBase = maxBase * 0.5f + 0.5f;
			shiftedColor.rgb *= maxBase;
			blendedAlbedo.rgb = saturate(shiftedColor);
#endif
			o.Albedo = blendedAlbedo.rgb;

			// Emission Occlusion
			/*float4 mask1 = tex2Dbias(_Emission, float4(parallax1, 0, textureBias));
			float4 mask2 = tex2Dbias(_Emission, float4(parallax2, 0, textureBias));
			float4 mask3 = tex2Dbias(_Emission, float4(parallax3, 0, textureBias));
			float4 blendedMask = mask1 * weights.x + mask2 * weights.y + mask3 * weights.z;*/
			o.Emission = 0; // blendedMask.rgb;
			o.Occlusion = 0; // blendedMask.a;

			// Specular Smoothness
		

			// Diffusion Features
#if defined(AI_HD_RENDERPIPELINE) && ( AI_HDRP_VERSION >= 50702 )
			float4 feat1 = _Features.SampleLevel(SamplerState_Point_Repeat, parallax1, 0);
			o.Diffusion = feat1.rgb;
			o.Features = feat1.a;
			float4 test1 = _Specular.SampleLevel(SamplerState_Point_Repeat, parallax1, 0);
			o.MetalTangent = test1.b;
#endif

			// normal depth
			float4 normals1 = tex2Dbias(_Normals, float4(parallax1, 0, textureBias));
			float4 normals2 = tex2Dbias(_Normals, float4(parallax2, 0, textureBias));
			float4 normals3 = tex2Dbias(_Normals, float4(parallax3, 0, textureBias));
			float4 blendedNormal = normals1 * weights.x + normals2 * weights.y + normals3 * weights.z;

			float3 localNormal = blendedNormal.rgb * 2.0 - 1.0;
			float3 worldNormal = normalize(mul((float3x3)ai_ObjectToWorld, localNormal));
			o.Normal = worldNormal;

			float3 viewPos = interpViewPos.xyz;
			float depthOffset = ((parallaxSample1.a * weights.x + parallaxSample2.a * weights.y + parallaxSample3.a * weights.z) - 0.5 /** 2.0 - 1.0*/) /** 0.5*/ * _DepthSize * length(ai_ObjectToWorld[2].xyz);

#if !defined(AI_RENDERPIPELINE) // no SRP
#if defined(SHADOWS_DEPTH)
			if (unity_LightShadowBias.y == 1.0) // get only the shadowcaster, this is a hack
			{
				viewPos.z += depthOffset * _AI_ShadowView;
				viewPos.z += -_AI_ShadowBias;
			}
			else // else add offset normally
			{
				viewPos.z += depthOffset;
			}
#else // else add offset normally
			viewPos.z += depthOffset;
#endif
#elif defined(AI_RENDERPIPELINE) // SRP
#if ( defined(SHADERPASS) && (SHADERPASS == SHADERPASS_SHADOWS) ) || defined(UNITY_PASS_SHADOWCASTER)
			viewPos.z += depthOffset * _AI_ShadowView;
			viewPos.z += -_AI_ShadowBias;
#else // else add offset normally
			viewPos.z += depthOffset;
#endif
#endif

			worldPos = mul(UNITY_MATRIX_I_V, float4(viewPos.xyz, 1)).xyz;
			clipPos = mul(UNITY_MATRIX_P, float4(viewPos, 1));

#if !defined(AI_RENDERPIPELINE) // no SRP
#if defined(SHADOWS_DEPTH)
			clipPos = UnityApplyLinearShadowBias(clipPos);
#endif
#elif defined(AI_RENDERPIPELINE) // SRP
#if defined(UNITY_PASS_SHADOWCASTER) && !defined(SHADERPASS)
#if UNITY_REVERSED_Z
			clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
#else
			clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
#endif
#endif
#endif

			clipPos.xyz /= clipPos.w;

			if (UNITY_NEAR_CLIP_VALUE < 0)
				clipPos = clipPos * 0.5 + 0.5;
		}


		fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target 
		{
			UNITY_SETUP_INSTANCE_ID(IN);
			SurfaceOutputStandardSpecular o;
			UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandardSpecular, o);

			float4 clipPos;
			float3 worldPos;
			OctaImpostorFragment_My(o, clipPos, worldPos, IN.uvsFrame1, IN.uvsFrame2, IN.uvsFrame3, IN.octaFrame, IN.viewPos);
			IN.pos.zw = clipPos.zw;

			outDepth = IN.pos.z;

			#ifndef USING_DIRECTIONAL_LIGHT
				fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
			#else
				fixed3 lightDir = _WorldSpaceLightPos0.xyz;
			#endif

			fixed3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

			UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
			//UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
			//UnityGI gi;
			//UNITY_INITIALIZE_OUTPUT(UnityGI, gi);

			float4 col = 0;

			float  outOfBounds;
			float4 vol = SampleVolume(worldPos, outOfBounds);
			TopDownSample(worldPos, vol.rgb, outOfBounds);
			float3 ambientCol = lerp(vol, _RayMarchSkyColor.rgb, outOfBounds);

			float direct = smoothstep(0, 1, dot(o.Normal, _WorldSpaceLightPos0.xyz));

			float3 lightColor = GetDirectional() * direct;

			col.rgb = o.Albedo * 1.5 * (ambientCol + lightColor);

			float3 viewDir = normalize(IN.viewPos.xyz);

			ApplyBottomFog(col.rgb, worldPos.xyz, viewDir.y);

			//col.rgb = viewDir;

			return col;
		}

		ENDCG
	}

	
	Pass
	{
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }
		ZWrite On

		CGPROGRAM
		#pragma vertex vert_surf
		#pragma fragment frag_surf
		#pragma multi_compile_shadowcaster
		#pragma multi_compile __ LOD_FADE_CROSSFADE
		#ifndef UNITY_PASS_SHADOWCASTER
		#define UNITY_PASS_SHADOWCASTER
		#endif
		#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
		#pragma multi_compile_instancing
		#include "HLSLSupport.cginc"
		#if !defined( UNITY_INSTANCED_LOD_FADE )
			#define UNITY_INSTANCED_LOD_FADE
		#endif
		#include "UnityShaderVariables.cginc"
		#include "UnityShaderUtilities.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "UnityPBSLighting.cginc"
		#include "UnityStandardUtils.cginc"
			#include "Assets\AmplifyImpostors\Plugins\EditorResources\Shaders\Runtime\AmplifyImpostors.cginc"

		struct v2f_surf {
			V2F_SHADOW_CASTER;
			float4 uvsFrame1 : TEXCOORD1;
			float4 uvsFrame2 : TEXCOORD2;
			float4 uvsFrame3 : TEXCOORD3;
			float4 octaFrame : TEXCOORD4;
			float4 viewPos : TEXCOORD5;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};

		v2f_surf vert_surf(appdata_full v) {
			UNITY_SETUP_INSTANCE_ID(v);
			v2f_surf o;
			UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
			UNITY_TRANSFER_INSTANCE_ID(v,o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			OctaImpostorVertex(v.vertex, v.normal, o.uvsFrame1, o.uvsFrame2, o.uvsFrame3, o.octaFrame, o.viewPos);

			TRANSFER_SHADOW_CASTER(o)
			return o;
		}

		fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target {
			UNITY_SETUP_INSTANCE_ID(IN);
			SurfaceOutputStandardSpecular o;
			UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandardSpecular, o);

			float4 clipPos;
			float3 worldPos;
			OctaImpostorFragment(o, clipPos, worldPos, IN.uvsFrame1, IN.uvsFrame2, IN.uvsFrame3, IN.octaFrame, IN.viewPos);
			IN.pos.zw = clipPos.zw;

			outDepth = IN.pos.z;

			UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
			SHADOW_CASTER_FRAGMENT(IN)
		}
		ENDCG
	}

	Pass
	{
		Name "SceneSelectionPass"
		Tags{ "LightMode" = "SceneSelectionPass" }
		ZWrite On
		ColorMask 0

		CGPROGRAM
		#pragma vertex vert_surf
		#pragma fragment frag_surf
		#pragma multi_compile __ LOD_FADE_CROSSFADE
		#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
		#pragma multi_compile_instancing
		#include "HLSLSupport.cginc"
		#if !defined( UNITY_INSTANCED_LOD_FADE )
			#define UNITY_INSTANCED_LOD_FADE
		#endif
		#include "UnityShaderVariables.cginc"
		#include "UnityShaderUtilities.cginc"
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "UnityPBSLighting.cginc"
		#include "UnityStandardUtils.cginc"

			#include "Assets\AmplifyImpostors\Plugins\EditorResources\Shaders\Runtime\AmplifyImpostors.cginc"

		int _ObjectId;
		int _PassValue;

		struct v2f_surf {
			UNITY_POSITION(pos);
			float4 uvsFrame1 : TEXCOORD1;
			float4 uvsFrame2 : TEXCOORD2;
			float4 uvsFrame3 : TEXCOORD3;
			float4 octaFrame : TEXCOORD4;
			float4 viewPos : TEXCOORD5;
			UNITY_VERTEX_INPUT_INSTANCE_ID
			UNITY_VERTEX_OUTPUT_STEREO
		};

		v2f_surf vert_surf(appdata_full v) {
			UNITY_SETUP_INSTANCE_ID(v);
			v2f_surf o;
			UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
			UNITY_TRANSFER_INSTANCE_ID(v,o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

			OctaImpostorVertex(v.vertex, v.normal, o.uvsFrame1, o.uvsFrame2, o.uvsFrame3, o.octaFrame, o.viewPos);

			o.pos = UnityObjectToClipPos(v.vertex);
			return o;
		}

		fixed4 frag_surf(v2f_surf IN, out float outDepth : SV_Depth) : SV_Target {
			UNITY_SETUP_INSTANCE_ID(IN);
			SurfaceOutputStandardSpecular o;
			UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandardSpecular, o);

			float4 clipPos;
			float3 worldPos;
			OctaImpostorFragment(o, clipPos, worldPos, IN.uvsFrame1, IN.uvsFrame2, IN.uvsFrame3, IN.octaFrame, IN.viewPos);
			IN.pos.zw = clipPos.zw;

			outDepth = IN.pos.z;

			UNITY_APPLY_DITHER_CROSSFADE(IN.pos.xy);
			return float4(_ObjectId, _PassValue, 1.0, 1.0);
		}
		ENDCG
	}
	}
				Fallback "Diffuse"
	}
