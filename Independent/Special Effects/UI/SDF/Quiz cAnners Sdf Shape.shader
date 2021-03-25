Shader "Quiz cAnners/UI/Effects/Sdf Shape"{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Cutoff("Mask Cutoff", Range(0,0.99)) = 0.5
		_Color("Tint", Color) = (1,1,1,1)

		_Texture("Screen Space Texture", 2D) = "white" {}

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		[Toggle(SHADOW)] _Shadow("Add Shadow", Float) = 0
		[Toggle(SUBTRACT)] _Subtract("Subtract", Float) = 0
		[Toggle(GRADIENT)] _Gradient("Gradient", Float) = 0
		
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
		ZTest [unity_GUIZTestMode]
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
			
			#pragma shader_feature_local _ SHADOW
			#pragma shader_feature_local _ SUBTRACT
			#pragma shader_feature_local _ GRADIENT
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
			sampler2D _Texture;

			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			float4 _Texture_TexelSize;
			float4 _Color;
			float4 _TextureSampleAdd;
			float _Cutoff;
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
				OUT.color = v.color;


			

				#if qc_USE_PARALLAX
		
					OUT.vertex = UnityObjectToClipPos(OUT.worldPosition + float4(qc_ParallaxOffset.xy * 256, 0, 0));

				/*#else

					float2 deCenter = OUT.texcoord - 0.5;
					float2 sp = OUT.screenPos.xy;
					float2 off = float2(sin((_Time.x * 3 + sp.y * 5 - sp.x) * 10), cos((_Time.x * 4.2 + sp.x * 3 + sp.y) * 10));
					float shake = max(0, (v.color.a - 0.75) * 4) * v.color.a;
					OUT.vertex = UnityObjectToClipPos(OUT.worldPosition + float4((off + deCenter * abs(off) * 0.5) * 2 * shake, 0, 0));
					*/
				#endif

		


				return OUT;
			}

			float4 frag(v2f o) : SV_Target {

				float mask = tex2Dlod(_MainTex,float4( o.texcoord,0,0)).r;
				
				#if SUBTRACT
					mask = 1 - mask;
				#endif

				float delta = abs(fwidth(o.texcoord));

				float size = _MainTex_TexelSize.x;

				delta = smoothstep(_Cutoff, _Cutoff + delta * 1024 * size + 0.001, mask);

				#if GRADIENT
					float4 color = delta * (o.color * o.texcoord.y + _Color * (1- o.texcoord.y) );
				#else
					float4 color = delta * o.color * _Color;
				#endif
					
				#if SHADOW
					float shadow = tex2Dlod(_MainTex, float4(o.texcoord + float2(0.04, 0.02) , 0, 2)).r;

					shadow = smoothstep(_Cutoff, _Cutoff   + 0.05, shadow);

					color.rgb *= color.a;

					color.a += shadow *  0.5 * (1 - color.a) * o.color.a ;
				#endif

				o.screenPos.xy = o.screenPos.xy / o.screenPos.w;

				float2 textUv = o.screenPos.xy* _ScreenParams.xy * _Texture_TexelSize.xy;

				float4 tex = tex2Dlod(_Texture, float4(textUv, 0, 0));
				
				color.rgb *= tex.rgb;


				return color;
			}
			ENDCG
		}
	}

  Fallback "Legacy Shaders/Transparent/VertexLit"
}