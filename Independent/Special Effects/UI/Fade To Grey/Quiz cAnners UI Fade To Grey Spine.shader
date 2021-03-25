Shader "Quiz cAnners/UI/Effects/Fade To Grey - Spine"{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}

		_ShowColor("Colored", Range(0,1)) = 1

		[Toggle(_STRAIGHT_ALPHA_INPUT)] _StraightAlphaInput("Straight Alpha Texture", Int) = 0
		[Toggle(_CANVAS_GROUP_COMPATIBLE)] _CanvasGroupCompatible("CanvasGroup Compatible", Int) = 0
		_Color("Grey Color", Color) = (0.5,0.5,0.5,1)
		_GreyBrightness("Gray brightness", Range(0,4)) = 1

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
			//[Toggle(HIGHLIGHT)] trimmed("Highlight", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
			"CanUseSpriteAtlas" = "True"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend One OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Pass
		{
			Name "Default"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile _ USE_NOISE_TEXTURE
			#pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
			#pragma shader_feature _ _CANVAS_GROUP_COMPATIBLE
			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

		struct appdata_t
		{
			float4 vertex   : POSITION;
			float4 color    : COLOR;
			float2 texcoord : TEXCOORD0;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f
		{
			float4 vertex   : SV_POSITION;
			float4 color : COLOR;
			float2 texcoord  : TEXCOORD0;
			float4 worldPosition : TEXCOORD1;
			float4 screenPos : 	TEXCOORD2;
			UNITY_VERTEX_OUTPUT_STEREO
		};

		sampler2D _MainTex;
		float4 _Color;
		float4 _TextureSampleAdd;
		float4 _ClipRect;
		float4 _MainTex_ST;
		float _GreyBrightness;
		float _ShowColor;
		sampler2D _Global_Noise_Lookup;

		v2f vert(appdata_t v)
		{
			v2f OUT;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

			OUT.worldPosition = v.vertex;
			OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
			OUT.texcoord = v.texcoord;

			#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw - 1.0) * float2(-1, 1);
			#endif

			OUT.screenPos = ComputeScreenPos(OUT.vertex);

			OUT.color = v.color;
			return OUT;
		}

		fixed4 frag(v2f IN) : SV_Target
		{
			half4 texColor = tex2D(_MainTex, IN.texcoord);

			#if defined(_STRAIGHT_ALPHA_INPUT)
			texColor.rgb *= texColor.a;
			#endif

			half4 color = (texColor + _TextureSampleAdd) * IN.color;
			#ifdef _CANVAS_GROUP_COMPATIBLE
			// CanvasGroup alpha sets vertex color alpha, but does not premultiply it to rgb components.
			color.rgb *= IN.color.a;
			#endif

			color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);

			#ifdef UNITY_UI_ALPHACLIP
			clip(color.a - 0.001);
			#endif

#if USE_NOISE_TEXTURE
			float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(IN.screenPos.xy * 13.5 + float2(_SinTime.w, _CosTime.w) * 32, 0, 0));
			noise.rgb -= 0.5;
#endif

			float grey = (color.r + color.g + color.b) * 0.33;

			color.rgb = color.rgb * _ShowColor + _Color.rgb * (color.a * (1 - _GreyBrightness) + grey * _GreyBrightness) * (1 - _ShowColor);

#if USE_NOISE_TEXTURE
			color.rgb += noise.rgb * 0.02;
#endif

			return color;
		}
		ENDCG
	}
}
}