Shader "Mushrooms/Effects/Halo" {
	Properties{
		_MainTex("Object To Highlight (With Mipmaps)", 2D) = "white" {}
		_Parallax("Parallax", Range(-10,10)) = 0
		_Brightness("Brightness", Range(0.05,2)) = 0
		[Toggle(_DEBUG)] debugOn("Debug", Float) = 0

		_Color("Tint", Color) = (1,1,1,1)

		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255

		_ColorMask("Color Mask", Float) = 15

	}

	SubShader{

		Tags{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			 "PreviewType" = "Plane"
		}

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		//ColorMask RGB
		Cull Off
		ZWrite Off
		ZTest Off
		Blend SrcAlpha One//MinusSrcAlpha
		ColorMask[_ColorMask]

		Pass{

			CGPROGRAM

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile_fwdbase // useful to have shadows 
			#pragma shader_feature_local ____ _DEBUG 		
			#pragma multi_compile __ USE_NOISE_TEXTURE



			#pragma target 3.0

			struct v2f {
				float4 pos : 		SV_POSITION;
				float3 worldPos : 	TEXCOORD0;
				float3 normal : 	TEXCOORD1;
				float2 texcoord : 	TEXCOORD2;
				float3 viewDir: 	TEXCOORD3;
				float4 screenPos : 	TEXCOORD4;
				float2 stretch		: TEXCOORD5;
				float4 color: 		COLOR;
			};

			sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;

			sampler2D _Global_Noise_Lookup;
			float _Parallax;
			float4 qc_ParallaxOffset;
			float4 _Mushroom_Star_Color;
			float4 _Mushroom_Star_Position;
			float _Mushroom_Light_Visibility;
			float _Brightness;

			v2f vert(appdata_full v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

				o.normal.xyz = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				o.viewDir.xyz = WorldSpaceViewDir(v.vertex);
				o.texcoord = v.texcoord; 
				o.color = v.color;
				o.pos = UnityObjectToClipPos(v.vertex + float4(qc_ParallaxOffset.x, 0, -qc_ParallaxOffset.y, 0) * _Parallax );
				o.screenPos = ComputeScreenPos(o.pos);

				float screenAspect = _ScreenParams.x * (_ScreenParams.w - 1);
				float texAspect = 1;
				float2 aspectCorrection = float2(1, 1);

				if (screenAspect > texAspect)
					aspectCorrection.y = (texAspect / screenAspect);
				else
					aspectCorrection.x = (screenAspect / texAspect);

				o.stretch = aspectCorrection;
				return o;
			}

			float2 Rot(float2 uv, float angle) {
				float si = sin(angle);
				float co = cos(angle);
				return float2(co * uv.x - si * uv.y, si * uv.x + co * uv.y);
			}

		
			/*float iCircle(in float2 ro, in float2 rd, float sphereRadius)
			{

				float b = dot(ro, rd);
				float c = dot(ro, ro) - sphereRadius * sphereRadius;
				float h = b * b - c;

				if (h < 0.)
				{
					return 1;
				}
				else
				{
					return 0;
				}
			}*/


			float4 frag(v2f o) : COLOR{

				o.screenPos.xy /= o.screenPos.w;
				o.screenPos.xy = (o.screenPos.xy - 0.5) * o.stretch;
				o.screenPos.xy += 0.5;

				float2 center = _Mushroom_Star_Position.xy + qc_ParallaxOffset.xy * 0.1;
				float2 offset = o.screenPos.xy - center;

				float2 planetVec = o.texcoord.xy - 0.5;

				float4 tex = tex2Dlod(_MainTex, float4(planetVec + 0.5, 0, 5));

				float rad = length(planetVec);
				float faceCircle = smoothstep(0.5, 0.4, rad);
				float shadow = smoothstep(0.4, 0.1, rad);
				float dott = dot(normalize(planetVec), normalize(offset));
				float rays = smoothstep( 1, -1 , dott);

				float4 col = _Mushroom_Star_Color * max(0, faceCircle * rays - shadow);


				#if USE_NOISE_TEXTURE
					float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(o.screenPos.xy * 13.5 + float2(_SinTime.w, _CosTime.w) * 32, 0, 0));
					#ifdef UNITY_COLORSPACE_GAMMA
						col.rgb += (noise.rgb - 0.5) * 0.02;
					#else
						col.rgb += (noise.rgb - 0.5) * 0.0075;
					#endif
				#endif

				return col * _Mushroom_Light_Visibility * _Brightness;
			}
			ENDCG
		}
	}

	Fallback "Legacy Shaders/Transparent/VertexLit"
}