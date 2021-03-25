Shader "Quiz cAnners/UI/Hologram Cybepunk" 
{
	Properties
	{
		[PerRendererData]
		_MainTex("Sprite Texture", 2D) = "white" {}
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
			#pragma multi_compile ___ _qcPp_FEED_MOUSE_POSITION

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
			sampler2D _Global_Noise_Lookup;
			float4 _Color;
			float4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _MainTex_ST;
			float4 _ProjTexPos;
			float _Aboration;
			float4 _qcPp_MousePosition;

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

				return OUT;
			}

			float4 frag(v2f IN) : SV_Target {

				float4 color = tex2Dlod(_MainTex, float4(IN.texcoord, 0, 0)) * IN.color;

				float2 linesUV = IN.screenPos.xy/ IN.screenPos.w;

				float downLine = tan(linesUV.y*8 - _Time.y);

				float upLine = tan(linesUV.y*3 + _Time.y*1.5);

				float3 movingLine = float3(0.75,0.5,0.75) * saturate(downLine) + float3(0.25,0.5,0.25) * saturate(upLine);

				#if _qcPp_FEED_MOUSE_POSITION
			
					half2 fromMouse = (linesUV - _qcPp_MousePosition.xy);

					fromMouse.x *= _qcPp_MousePosition.w;

					half pressPower = max(0, (1 - length(fromMouse) * 5) * _qcPp_MousePosition.z);
				#else 
					half pressPower = 0.1;
				#endif

        		float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(linesUV*(0.2  + sin(_Time.w + pressPower*0.01)*0.15) + float2(_SinTime.w, _CosTime.w) * 32, 0, 0));

				noise.rgb -= 0.5;

          		float staticLine = (sin((linesUV.y)*(800)) + 1)*0.5;

				color.rgb = color.rgb * (1 + noise.rgb * pressPower * 0.4 + staticLine*staticLine*(0.1 + movingLine*0.2));

				return color;
			}
			ENDCG
		}
	}
}