Shader "Mushrooms/Effects/Sdf Lights Gyro" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Parallax("Parallax", Range(-10,10)) = 0
		[Toggle(_DEBUG)] debugOn("Debug", Float) = 0
	}

	SubShader{

		Tags{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		ColorMask RGB
		Cull Off
		ZWrite Off
		ZTest Off
		Blend SrcAlpha One //MinusSrcAlpha

		Pass{

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile_fwdbase // useful to have shadows 
			#pragma shader_feature_local ____ _DEBUG 
			#pragma multi_compile ______ USE_NOISE_TEXTURE


			#pragma target 3.0

			struct v2f {
				float4 pos : 		SV_POSITION;
				float3 worldPos : 	TEXCOORD0;
				float3 normal : 	TEXCOORD1;
				float2 texcoord : 	TEXCOORD2;
				float3 viewDir: 	TEXCOORD3;
				float4 screenPos : 	TEXCOORD4;
				float4 color: 		COLOR;
			};


			uniform float4 _MainTex_ST;
			sampler2D _MainTex;
			sampler2D _Global_Noise_Lookup;
			float _Parallax;
			float4 qc_ParallaxOffset;

			v2f vert(appdata_full v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

				o.normal.xyz = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.viewDir.xyz = WorldSpaceViewDir(v.vertex);
				o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
				
				o.color = v.color;

				o.pos = UnityObjectToClipPos(v.vertex + float4(qc_ParallaxOffset.xy * _Parallax, 0, 0));
				o.screenPos = ComputeScreenPos(o.pos);

				return o;
			}

			float2 Rot(float2 uv, float angle) {
				float si = sin(angle);
				float co = cos(angle);
				return float2(co * uv.x - si * uv.y, si * uv.x + co * uv.y);
			}

			float4 frag(v2f o) : COLOR{

				float mask = tex2D(_MainTex, o.texcoord.xy).r;
			
				float w = length(abs(fwidth(o.texcoord.xy)));

				float4 col = 1 / ( (1.01- saturate((mask)*(1 + w*5))) * 20);  //smoothstep(0.99-w*400, 0.99 , mask);

				float dott = abs(dot(o.normal.xyz, normalize(o.viewDir.xyz)));


				#if USE_NOISE_TEXTURE

					o.screenPos.xy /= o.screenPos.w;

					float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(o.screenPos.xy * 13.5 + float2(_SinTime.w, _CosTime.w) * 32, 0, 0));
					#ifdef UNITY_COLORSPACE_GAMMA
						col.rgb += (noise.rgb - 0.5) * 0.02;
					#else
						col.rgb += (noise.rgb - 0.5) * 0.0075;
					#endif
				#endif

						return col * dott;
			}
			ENDCG
		}
	}

	Fallback "Legacy Shaders/Transparent/VertexLit"
}