Shader "Quiz cAnners/Effects/Thin Lines/Circle" {
	Properties{
		[NoScaleOffset] _MainTex("Albedo (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Hardness("Hardness", Range(0.25,4)) = 0.4
	}

	Category{
		Tags{
			"Queue" = "Transparent"
			"PreviewType" = "Plane"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}

		Cull Off
		ZWrite Off
		Blend One One

		SubShader{

			Pass{

				CGPROGRAM

				#include "UnityCG.cginc"

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fwdbase
				#pragma multi_compile_instancing
				#pragma target 3.0

				sampler2D _MainTex;
				float4 _Color;
				float _Hardness;
				sampler2D _Global_Noise_Lookup;

				struct v2f {
					float4 pos : SV_POSITION;
					float4 screenPos : TEXCOORD1;                   // v2f (TEXCOORD can be 0,1,2, etc - the obly rule is to avoid duplication)
					float2 texcoord : TEXCOORD2;
					float4 color: 		COLOR;
			
				};

				v2f vert(appdata_full v) {
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.texcoord = v.texcoord.xy;
					o.color = v.color * _Color;
					o.screenPos = ComputeScreenPos(o.pos);       	// vert
			
					return o;
				}

				float4 frag(v2f i) : COLOR
				{
					float2 width2 = fwidth(i.texcoord.xy);
					float width = width2.x + width2.y;

					float2 uv = i.texcoord.xy - 0.5;

					float signedDist = length(uv) - 0.4;

					float dist = abs(signedDist);

					float brightness = width * _Hardness / (0.000001 + dist);

					brightness *= smoothstep(0.1, 0, dist);

					float4 col = i.color;

					float2 screenUV = i.screenPos.xy / i.screenPos.w;    

					const float pii = 3.14159265359;
					const float pi2 = pii * 2;

					float angle = (atan2(uv.x, uv.y) + pii) / pi2;

					float2 sampleTex = float2(pow(0.5 - dist, 6) + step(signedDist, 0) * 0.25, angle);

					float2 animOffset = 0.2 * float2(_Time.x, -_Time.x);

					float4 tex =
						tex2D(_MainTex, sampleTex + animOffset * 0.82)
						* 
						tex2D(_MainTex, sampleTex - animOffset*0.75)
						;

					brightness *= 0.25 + smoothstep(0.05, 0.5, tex.r * brightness*2);

					col.rgb *= brightness * col.a;

					float3 mix = col.gbr + col.brg;
					col.rgb += mix * mix * 0.05;


#if USE_NOISE_TEXTURE
					float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(screenUV.xy * 13.5 + float2(_SinTime.w, _CosTime.w) * 32, 0, 0));
#ifdef UNITY_COLORSPACE_GAMMA
					col.rgb += (noise.rgb - 0.5) * 0.02;
#else
					col.rgb += (noise.rgb - 0.5) * 0.0075;
#endif
#endif


					return col;
				}
				ENDCG
			}
		}
		Fallback "Legacy Shaders/Transparent/VertexLit"
	}
}

