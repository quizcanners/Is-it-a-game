Shader "Playtime Painter/UI/Outline Animation"{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}

		[Toggle(SPRITE_IS_RED)] _UseRedSprite("Use Red Channel", Float) = 0

		_Color("Tint", Color) = (1,1,1,1)
		 _SpiralMask("Spiral Mask (R)", 2D) = "black"{}

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
			"CanUseSpriteAtlas" = "False"
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
		Blend One One
		ColorMask[_ColorMask]


		Pass
		{
			Name "Default"
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#pragma shader_feature_local  ___  SPRITE_IS_RED

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

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
				float2 unchangedPosition : TEXCOORD2;
				float4 screenPos : 	TEXCOORD3;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			sampler2D _SpiralMask;
			float _Effect_Time;
			float4 _Color;
			float4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _MainTex_ST;

			v2f vert(appdata_t v)
			{
				v2f OUT;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
				OUT.worldPosition = v.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

				OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				OUT.unchangedPosition = v.texcoord;

				OUT.screenPos = ComputeScreenPos(OUT.vertex);

				OUT.color = v.color * _Color;
				return OUT;
			}

			float4 frag(v2f IN) : SV_Target
			{
				IN.screenPos.xy /= IN.screenPos.w;

				half alpha = tex2Dlod(_MainTex, float4( IN.texcoord,0,3)).
					#if SPRITE_IS_RED
							r
					#else
							a
					#endif
					;
	
				half2 uv = IN.unchangedPosition.xy;

				half offset = alpha * 0.1;

				half angle = _Effect_Time * 20 + offset ;

				half2 off = (uv - 0.5) / 2;
				half2 rotUV = off;
				half si = sin(angle);
				half co = cos(angle);

				half tx = rotUV.x;
			    half ty = rotUV.y;
				rotUV.x = (co * tx) - (si * ty);
				rotUV.y = (si * tx) + (co * ty);
				rotUV += 0.5;

				float circle = tex2Dlod(_SpiralMask, float4(rotUV,0,0)).r;

				float circulating = circle *  alpha; 

				float4 color = IN.color;

				color.rgb *= color.a * circulating;

				float3 mix = color.gbr*color.brg;

				color.rgb += color.rgb * 0.2;

				color.a = 1;

				return color;
			}
		ENDCG
		}
	}
}