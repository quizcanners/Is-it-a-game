Shader "Quiz cAnners/UI/Blick"{
	Properties
	{
		[PerRendererData] _MainTex("Mask Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

		_Gap("Gap", Range(1,5)) = 1
		_Delay("Delay", Range(0,5)) = 0
		_Speed("Speed", Range(0,5)) = 1
		_Thinness("Thinness", Range(0.001,0.9)) = 0.5
		_Tilt("Tilt", Range(0,2.9)) = 0.5

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
		Blend SrcAlpha One 
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
			float _Delay;
			float _Effect_Time;
			float _Speed;
			float _Thinness;
			float _Tilt;
			float _Gap;

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

			half4 frag(v2f IN) : SV_Target{

				IN.screenPos.xy /= IN.screenPos.w;

				float blick = ((_Effect_Time * _Speed + _Delay) % (3)) - 1;

				blick *= (1 + _Gap);

				blick = abs (IN.screenPos.x - IN.screenPos.y *_Tilt - blick);

				float cutoff = fwidth(blick) * 2;

				float edge = 1-_Thinness;

				blick = smoothstep(edge, edge - cutoff, blick);

				half textureAlpha = tex2D(_MainTex, IN.texcoord).a;

				IN.color.a *= blick * textureAlpha;

				return IN.color;

			}
			ENDCG
		}
	}

  Fallback "Legacy Shaders/Transparent/VertexLit"
}