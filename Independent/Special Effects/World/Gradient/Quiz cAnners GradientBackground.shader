Shader "Quiz cAnners/Effects/Screen-SpaceGradient" {
	Properties{
		[PerRendererData]_MainTex("Mask (RGB)", 2D) = "white" {}
		
		_UPPER("Background Upper", Color) = (1,1,1,1)
		_LOWER("Background Lower", Color) = (1,1,1,1)

	}

	Category{
		Tags{
			"Queue" = "Transparent"
			"PreviewType" = "Plane"
			"IgnoreProjector" = "True"
		}

		ColorMask RGB
		Cull Off

		SubShader{
			Pass{

				CGPROGRAM
				#include "UnityCG.cginc"

				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_fog
				#pragma multi_compile_fwdbase
				#pragma multi_compile_instancing
				#pragma multi_compile ______ USE_NOISE_TEXTURE
				#pragma target 3.0

				struct v2f {
					float4 pos : SV_POSITION;
					float4 screenPos : TEXCOORD1;
				};

				v2f vert(appdata_full v) {
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					o.pos = UnityObjectToClipPos(v.vertex);
					o.screenPos = ComputeScreenPos(o.pos);
					return o;
				}


				sampler2D _Global_Noise_Lookup;
				float4 _UPPER;
				float4 _LOWER;

				float4 frag(v2f i) : COLOR{

					float2 screenUV = i.screenPos.xy / i.screenPos.w;

					float up = screenUV.y; 

					#ifdef UNITY_COLORSPACE_GAMMA
					float4 col = _UPPER * _UPPER *(up) + _LOWER * _LOWER *(1-up);
					col.rgb = sqrt(col.rgb);
					#else
					float4 col = _UPPER  *(up) +  _LOWER *(1-up);
					#endif

					#if USE_NOISE_TEXTURE
						float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(screenUV.xy * 13.5 + float2(_SinTime.w, _CosTime.w) * 32, 0, 0));
						#ifdef UNITY_COLORSPACE_GAMMA
							col.rgb += (noise.rgb - 0.5)*0.02;
						#else
							col.rgb += (noise.rgb - 0.5)*0.0075;
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
