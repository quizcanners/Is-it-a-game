Shader "Quiz cAnners/UI/GriddedPixels"{
	Properties
	{
		[PerRendererData]
  		_MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)

		[Toggle(TOUCH_REACTION)] _ReactOnTouch("React To Touch", Float) = 0

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

			#pragma multi_compile __ _qcPp_FEED_MOUSE_POSITION
			#pragma shader_feature_local ___ TOUCH_REACTION

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
    
			struct appdata_t
			{
				float4 vertex   : POSITION;
				half4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				half4 color : COLOR;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				float4 screenPos : 	TEXCOORD2;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			sampler2D _MainTex;
			half4 _Color;
			float4 _TextureSampleAdd;
			float4 _ClipRect;
			float4 _MainTex_ST;
			float4 _qcPp_MousePosition;
			float4 _qcPp_MouseDynamics;
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

				OUT.color = v.color;

				return OUT;
			}

			float4 frag(v2f IN) : SV_Target {

				IN.screenPos.xy /= IN.screenPos.w;

				float4 color = (tex2Dlod(_MainTex, float4(IN.texcoord,0, 0)));

				color.a *= IN.color.a;

				 color.rgb *= IN.color.rgb;

				float2 fragCoord = IN.screenPos.xy * _ScreenParams.xy;

				const float cellSize = 4;

				const float divider = 2/cellSize;

				float2 grid = (fragCoord % cellSize) * divider;

				float mapCol = grid.x * 0.5;

				float len = grid.x * grid.y;

				color.rgb *= (len + 0.25)
				  * float3(2.2, 1.9, 2.2)
				  ;

				#if _qcPp_FEED_MOUSE_POSITION && TOUCH_REACTION

				float2 fromMouse = (IN.screenPos.xy - _qcPp_MousePosition.xy);
					fromMouse.x *= _qcPp_MousePosition.w;

					float d = length(fromMouse);

					color.a *= smoothstep(0.2, 0, d + sin(d*50 - _Effect_Time*50)*0.02) * _qcPp_MousePosition.z;
				#endif

				return color;
			}
		ENDCG
		}
	}

  Fallback "Legacy Shaders/Transparent/VertexLit"
}