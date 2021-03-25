Shader "Taravana/Journey/Small Fishes Line"
{
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
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
			//#include "Assets/_Modules/Systems/Shaders/VertexDataProcessInclude.cginc"

			#pragma vertex vert
			#pragma fragment frag
         // #pragma multi_compile ___ _TaravanaLD

			struct v2f {
				float4 pos : 		SV_POSITION;
				float3 worldPos : 	TEXCOORD0;
				float2 texcoord : 	TEXCOORD1;
        float4 screenPos : 	TEXCOORD2;
				half4 color: 		COLOR;
			};

			sampler2D _Global_Water_Particles_Mask;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			sampler2D _MainTex;
      half _Distance;

			v2f vert(appdata_full v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

				o.pos = UnityObjectToClipPos(v.vertex);
			
        o.screenPos = ComputeScreenPos(o.pos);

        o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

       // float3 gyrPos = o.worldPos.xyz; //float3((o.screenPos.xy/ o.screenPos.w) * 250, _Taravana_Time * 3);
       // float gyroid = dot(sin(gyrPos), cos(gyrPos.zxy));
        //o.texcoord.y += gyroid * 0.05;
        o.texcoord = v.texcoord;

      //  v.vertex += mul(unity_WorldToObject, float4(1.5,1.5, 0, 0) * gyroid) * o.texcoord.x;

       // o.pos = UnityObjectToClipPos(v.vertex);

       


			
      
				o.color = v.color;
				return o;
			}

      half4 frag(v2f o) : COLOR{

				float2 screenUV = o.screenPos.xy / o.screenPos.w;

				float2 screenUV_Str = screenUV;//StretchedScreenUV(screenUV);

       

        float2 off = (o.texcoord.xy - 0.5);

        float br = tex2D(_MainTex, o.texcoord.xy);

        br = 0.2 / (length(float2(o.texcoord.x, o.texcoord.y - 0.5)));

        br *= max(0, 0.5 - length(off));

     //   float grad = DarkBrightGradient(screenUV, 0);// *value;

        float4 col = br;

		col.rgb *= br;// (grad)*br;


				return col;
			}
			ENDCG
		}
	}
		Fallback "Legacy Shaders/Transparent/VertexLit"
}