Shader "Quiz cAnners/UI/Effects/Gyroscope Offset"{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_OffsetAmount("Offset Amount", float) = 1

		_Color("Tint", Color) = (1,1,1,1)
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
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
		Blend SrcAlpha OneMinusSrcAlpha
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

			#pragma multi_compile __  _qc_SCENE_RENDERING
			#pragma multi_compile __ qc_USE_PARALLAX

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
			float _OffsetAmount;
			uniform float4 qc_ParallaxOffset;


			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = v.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				OUT.screenPos = ComputeScreenPos(OUT.vertex);
				OUT.color = v.color * _Color;

				#if !_qc_SCENE_RENDERING && qc_USE_PARALLAX 

					float screenAspect = _ScreenParams.x * (_ScreenParams.w - 1);
					float2 aspectCorrection = float2(1, 1);

					if (screenAspect > 1)
						aspectCorrection.y = (1 / screenAspect);
					else
						aspectCorrection.x = (screenAspect / 1);

					OUT.vertex = UnityObjectToClipPos(OUT.worldPosition + float4(qc_ParallaxOffset.xy * aspectCorrection * max(_ScreenParams.x, _ScreenParams.y) * 0.1 * _OffsetAmount, 0, 0));
				#endif

				return OUT;
			}

			half4 frag(v2f IN) : SV_Target
			{
				half4 color = (tex2Dlod(_MainTex, half4(IN.texcoord,0, 0)) + _TextureSampleAdd) * IN.color;

				#ifdef UNITY_UI_CLIP_RECT
				color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
				#endif

				#ifdef UNITY_UI_ALPHACLIP
				clip(color.a - 0.001);
				#endif

				return color;
			}
			ENDCG
		}
	}

			Fallback "Legacy Shaders/Transparent/VertexLit"
}