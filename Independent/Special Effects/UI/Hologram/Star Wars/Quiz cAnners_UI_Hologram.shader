Shader "Quiz cAnners/UI/Hologram" 
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ProjTexPos("Screen Space Projector Position", Vector) = (0,0,0,0)
		_ColorMask("Color Mask", Float) = 15
		_Aboration("Chromatic Aboration", Float) = 1

		[Toggle(IS_SPINE)] _IsSpine("Show Transparent Areas", Float) = 0
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
			#pragma shader_feature_local __ IS_SPINE
     
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
			sampler2D _Global_Noise_Lookup;
			float4 _Color;
			float4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _MainTex_ST;
			float4 _ProjTexPos;
			float _Aboration;
			float _Effect_Time;
		

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
				return OUT;
			}

			float4 frag(v2f IN) : SV_Target {

				IN.screenPos.xy /= IN.screenPos.w;

				float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(IN.screenPos.xy*(1.1 + _SinTime.w) + float2(_SinTime.w, _CosTime.w) * 32, 0, 0));
				noise.rgb -= 0.5;

				const float off = 0.003 * _Aboration *  (1 + noise.b* _SinTime.z);

				float4 color = tex2Dlod(_MainTex, float4(IN.texcoord, 0, 0));// *IN.color;

				float4 colorR = tex2Dlod(_MainTex, float4(IN.texcoord - float2(off, 0), 0, noise.r *4));// *IN.color;

				float4 colorB = tex2Dlod(_MainTex, float4(IN.texcoord + float2(off, 0), 0, noise.g * 4));// *IN.color;

				color.r = colorR.r;

				color.b = colorB.b;

				float2 linesUV = IN.screenPos.xy;


				float2 rayUV = IN.screenPos.xy;

				float lines = (sin((linesUV.y*4 - _Effect_Time) * 100) + 1)*0.5;

				lines *= lines;

				float light = 0.4;


				color.rgb += color.rgb * noise.rgb * 0.2;

				light *= light * 16;

				color.rgb = color.rgb * (1 + lines)
					+ 
					length(color.rgb)*IN.color.rgb  * 0.3
					;

				color *=   min(1,light * IN.color.a * 0.3);

				float3 mix = color.gbr*color.brg*color.a;

				color.rgb += mix * IN.color.rgb * 0.5;

				color.a += color.a * (1 - color.a);

				#if !IS_SPINE
					color.rgb *= color.a;
				#endif

				return color;
			}
		ENDCG
		}
	}
}