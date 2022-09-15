Shader "Mushrooms/Effects/SDF Light With Mist" {
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_FogColor("Fog Color (RGB)", 2D) = "white" {}
		_Cutoff("Shape Cutoff", Range(0.01, 0.99)) = 0.5
		_ShapeSoftness("Shape Softness", Range(1, 500)) = 50
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
		//	Blend SrcAlpha OneMinusSrcAlpha

			Pass{

				CGPROGRAM

				#include "UnityCG.cginc"

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing
				#pragma multi_compile_fwdbase // useful to have shadows 
				#pragma shader_feature_local ____ _DEBUG 

				#pragma target 3.0

				struct v2f {
					float4 pos : 		SV_POSITION;
					float3 worldPos : 	TEXCOORD0;
					float3 normal : 	TEXCOORD1;
					float2 texcoord : 	TEXCOORD2;
					float3 viewDir: 	TEXCOORD3;
					float4 screenPos : 	TEXCOORD4;
					float3 tangentViewDir : 	TEXCOORD5;
					float4 color: 		COLOR;
				};


				uniform float4 _MainTex_ST;
				sampler2D _MainTex;
				sampler2D _FogColor;
				float _ShapeSoftness;
				float _Cutoff;

				v2f vert(appdata_full v) {
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);

					o.normal.xyz = UnityObjectToWorldNormal(v.normal);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
					o.viewDir.xyz = WorldSpaceViewDir(v.vertex);
					o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
					o.screenPos = ComputeScreenPos(o.pos);
					o.color = v.color;


					float3x3 objectToTangent = float3x3(
						v.tangent.xyz,
						cross(v.normal, v.tangent.xyz) * v.tangent.w,
						v.normal
						);

					o.tangentViewDir = mul(objectToTangent, ObjSpaceViewDir(v.vertex));



					return o;
				}

				float2 Rot(float2 uv, float angle) {
					float si = sin(angle);
					float co = cos(angle);
					return float2(co * uv.x - si * uv.y, si * uv.x + co * uv.y);
				}

				float4 frag(v2f o) : COLOR{

					o.texcoord.x += _SinTime.x * 0.01;

					o.viewDir.xyz = normalize(o.viewDir.xyz);

					float sdf = tex2D(_MainTex, o.viewDir.xy * 2).x;

					float w = length(abs(fwidth(o.viewDir.xy)));

					o.tangentViewDir = normalize(o.tangentViewDir);
					o.tangentViewDir.xy /= (abs(o.tangentViewDir.z) + 0.42);

					float2 paralUv = o.texcoord - o.tangentViewDir.xy * 1.2;

					float4 fog = tex2D(_FogColor, paralUv);
					float4 fog2 = tex2D(_FogColor, o.viewDir.xy);


					float fogA = (fog.a * 0.7 + fog.b * 0.3);// +fog2.a * (1 - fog.a);

					fog.rgb *= fog.a;
					fog2.rgb *= fog2.a;

					fog = fog2 * (1 - fog.a) + fog;

					float seeTrough = 1 - fog.a;

					float scatteredlight = fogA * fogA * fogA;

					float range = (_ShapeSoftness * (1 + scatteredlight * 8)) * 0.001;

					//float bright = 0.1 / (1.01-sdf);

					//return bright;

					float star = range / (1.01 - sdf ); //smoothstep(_Cutoff  - range, _Cutoff - range * 0.5, sdf);

					//return star * star;
					
					float seeStars = 1 - fogA; // smoothstep(0.2, 0, fogA);

				//	return seeStars * star;

					float4 col = 
						pow(seeStars *star,3) * 8

						 + (1 - seeStars) * ((star * 2 + 0.5) * fog)
						
						;
						
					return col;
				}
				ENDCG
			}
		}
			Fallback "Legacy Shaders/Transparent/VertexLit"

					//CustomEditor "CircleDrawerGUI"
}