Shader "Quiz cAnners/Effects/Thin Lines/Straight" {
	Properties{
		[NoScaleOffset] _MainTex("Albedo (RGB)", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Hardness("Hardness", Range(0.25,8)) = 0.4

		[KeywordEnum(Horisontal, Vertical)]	_DIR("Direction", Float) = 0
		


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

				#pragma shader_feature_local _DIR_HORISONTAL _DIR_VERTICAL  


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
				#if _DIR_HORISONTAL
					float2 uv = i.texcoord.xy;
				#elif _DIR_VERTICAL
					float2 uv = i.texcoord.yx;
				#endif

				float2 width = fwidth(uv);

				float2 off = abs(uv.xy - 0.5);

				float brightness = width.y  * _Hardness / (off.y + 0.00001);

				float4 col = i.color;

				brightness *= smoothstep(0.5, 0.25, off.y) * smoothstep(0.5, 0.5 - width.y * 10, off.x);

				float2 screenUV = i.screenPos.xy / i.screenPos.w;    

				float2 sampleTex = float2(pow(0.5 - off.y, 6) + step(uv.y, 0.5) * 0.25, uv.x);// +screenUV * 0.5;// *(1 / (0.1 + length(width) * 100));

				float4 tex =
					tex2D(_MainTex, sampleTex + float2(uv.x, 0) + float2(_Time.x, - _Time.x * 5))
					* 
					tex2D(_MainTex, sampleTex - float2(uv.x, 0) - float2(_Time.x, -_Time.x * 5))
					;

				//return tex;

				brightness *= 0.25 + smoothstep(0.05, 0.5, tex.r * brightness)*2;

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

