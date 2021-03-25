Shader "Quiz cAnners/Effects/Ascending Particles"
{
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Speed("Speed", Range(0,4)) = 2
	}

	SubShader{

		Tags{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
		}

		ColorMask RGB
		Cull Off
		ZWrite Off
		ZTest Off
		Blend One One //MinusSrcAlpha 

		Pass{

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			struct vertexToFragment {
				float4 pos : 		SV_POSITION;
				float3 worldPos : 	TEXCOORD0;
				float2 texcoord : 	TEXCOORD1;
				float4 screenPos : 	TEXCOORD2;
				float3 normal : TEXCOORD3;
				float3 viewDir: TEXCOORD4;
				half4 color: 		COLOR;
			};

			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			sampler2D _MainTex;
			half _Distance;
			float _Effect_Time;
			float _Speed;
			float4 _Color;

			vertexToFragment vert(appdata_full v) 
			{
				vertexToFragment o;

				o.texcoord = v.texcoord;

				o.normal.xyz = UnityObjectToWorldNormal(v.normal);

				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

				o.pos = UnityObjectToClipPos(v.vertex);

				o.screenPos = ComputeScreenPos(o.pos);

				o.viewDir.xyz = WorldSpaceViewDir(v.vertex);

				o.color = v.color * _Color;

				return o;
			}

			float4 frag(vertexToFragment o) : COLOR
			{
				float yCut = o.texcoord.y;

				o.texcoord.xy -= 0.5;

				o.texcoord.x *= 1.5 - o.texcoord.y;

				float cut = smoothstep(0.5, 0.4, abs(o.texcoord.y)) * smoothstep(0, 0.1, (0.5 - abs(o.texcoord.x)));

				o.texcoord.y -= _Effect_Time * _Speed;

				o.viewDir.xyz = normalize(o.viewDir.xyz);

				float2 screenUV = o.screenPos.xy / o.screenPos.w;

				float2 screenUV_Str = screenUV; // Adjust for stretching  StretchedScreenUV(screenUV);

				float2 grid = o.texcoord.xy * float2(10, 14);

				float2 gridIndex = floor(grid);

				 grid = grid - gridIndex;

				float3 gyrPos = float3(gridIndex * 3351.345 , _Effect_Time *0.3);

				float gyroid = dot(sin(gyrPos), cos(gyrPos.zxy));

				float2 off = (o.texcoord.xy - 0.5);

				grid -= 0.5;

				float bg = smoothstep(yCut, 1.1, sin(gyroid*3.23)) * 0.03 / (0.01 + length(grid + float2(sin(gyroid*45.123), cos(gyroid*34.23))*0.2));

				float4 col = (bg *  bg)  * o.color;

				col.rgb *= cut;

				return col;
			}
			ENDCG
		}
	}
		Fallback "Legacy Shaders/Transparent/VertexLit"
}