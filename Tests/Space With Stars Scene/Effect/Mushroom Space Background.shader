Shader "Mushrooms/Effects/Space Background" {
	Properties{
		[PerRendererData]_MainTex("Dummy (SDF)", 2D) = "white" {}
		_StarTex("Stars (SDF)", 2D) = "white" {}
		_CloudsTex("Clouds (SDF)", 2D) = "white" {}
		_CloudsTex2("Clouds 2 (SDF)", 2D) = "white" {}
		_Outline("Star Outline(RGB)", 2D) = "white" {}
		_Surface("Star Surface(RGB)", 2D) = "white" {}
		_DysonSurface("Dyson Sphere Surface(RGB)", 2D) = "white" {}
		_Parallax("Parallax", Range(-10,10)) = 0

		_StarsScale("Stars Scale", Range(0.2,15)) = 0

		//[Toggle(_BLACK_HOLE)] blackHole("Is Black Hole", Float) = 0

		//[Toggle(_DYSON_SPHERE)] dysonSphere("Have Dyson Sphere", Float) = 0

		[Toggle(_PROCEDURAL_STARS)] procedStarsOn("Procedural Stars", Float) = 1
		//[Toggle(_GYROID_FG)] gyroidFogOn("Gyroid Fog On", Float) = 0


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
		Blend One Zero // SrcAlpha One//MinusSrcAlpha

		Pass{

			CGPROGRAM

			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#pragma multi_compile_fwdbase // useful to have shadows 
			#pragma shader_feature_local ____ _DEBUG 
			#pragma multi_compile __ _BLACK_HOLE			
			#pragma multi_compile __ USE_NOISE_TEXTURE
			#pragma multi_compile __ _DYSON_SPHERE
			#pragma multi_compile __ _SPACE_FOG
			#pragma multi_compile ___ _GYROID_FG
			#pragma shader_feature_local ___ _PROCEDURAL_STARS
		

			#pragma target 3.0

			struct v2f {
				float4 pos : 		SV_POSITION;
				float3 worldPos : 	TEXCOORD0;
				//float3 normal : 	TEXCOORD1;
				//float3 viewDir: 	TEXCOORD2;
				float4 screenPos : 	TEXCOORD3;
				float2 stretch		: TEXCOORD4;
				//float4 color: 		COLOR;
			};

			sampler2D _StarTex;
			uniform float4 _StarTex_ST;
			uniform float4 _StarTex_TexelSize;


			sampler2D _Surface;
			sampler2D _Outline;
			sampler2D _DysonSurface;
			sampler2D _CloudsTex;
			sampler2D _CloudsTex2;

			sampler2D _Global_Noise_Lookup;
			uniform float4 _Global_Noise_Lookup_TexelSize;

			float4 _Effect_Time;
			float _Parallax;
			float _StarsScale;
			float _Mushroom_Light_Visibility;
			float4 qc_ParallaxOffset;
			float4 qc_RND_SEEDS;
			float4 _Mushroom_Star_Color;
			float4 _Mushroom_Clouds_Color;
			float4 _Mushroom_Clouds_Color_2;
			float4 _Mushroom_Background_Color;
			float4 _Mushroom_Star_Position;
			float4 _Mushroom_Scroll_Position;

			v2f vert(appdata_full v) {
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);

				//o.normal.xyz = UnityObjectToWorldNormal(v.normal);
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				//o.viewDir.xyz = WorldSpaceViewDir(v.vertex);
				//o.color = v.color;
				o.pos = UnityObjectToClipPos(v.vertex);// +float4(qc_ParallaxOffset.x, 0, -qc_ParallaxOffset.y, 0) * _Parallax );
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


			float GetHexagon(float2 uv) 
			{
				uv = abs(uv);
				const float2 toDot = normalize(float2(1, 1.73));
				float c = dot(uv, toDot);
				return max(c, uv.x);
			}

			inline float4 GetHexagons(float2 grid, float2 texelSize, out float2 uv)
			{
				const float2 r = float2(1, 1.73);
				const float2 h = r * 0.5;
				const float pii = 3.141592653589793238462;
				const float pi2 = 1.0 / 6.283185307179586476924;

				grid = grid * 1.03 - float2(0.03, 0.06) * texelSize;
				float2 gridB = grid + h;

				float2 floorA = floor(grid / r);
				float2 floorB = floor(gridB / r);
				float2 uvA = ((grid - floorA * r) - h);
				float2 uvB = ((gridB - floorB * r) - h);
				float distA = GetHexagon(uvA);
				float distB = GetHexagon(uvB);
				float isB = saturate((distA - distB) * 9999);
				float dist = (distB * isB + distA * (1 - isB)) * 2;
				const float2 deChecker = float2(1, 2);
				float2 index = floorA * deChecker * (1 - isB) + (floorB * deChecker - deChecker + 1) * isB;

				uv = uvA * (1 - isB) + uvB * isB;
				
				float angle = 0;//(atan2(uv.x, uv.y) + pii) * pi2;
				return float4(index, dist, angle);
			}

			float2 Rot(float2 uv, float angle) {
				float si = sin(angle);
				float co = cos(angle);
				return float2(co * uv.x - si * uv.y, si * uv.x + co * uv.y);
			}

			float StarFromSdf(float sdf, float w, float mod)
			{
				float dist = sdf * (1 + w * 100 * mod * mod);
				return 1  / ((1.01  - saturate(dist* dist)) * 100);
			}

			float sdGyroid(float3 pos) {
				return abs(dot(sin(pos), cos(pos.zxy)));
			}

			float4 StarFromNoiseMask(float2 starsUvTiled, float w, float visibility)
			{
				float2 noiseUv = (starsUvTiled) * 0.01 + 10 + qc_RND_SEEDS.w * 5;
				float2 starUv = (noiseUv * _Global_Noise_Lookup_TexelSize.zw) % 1 - 0.5;
				float4 starGrid = tex2Dlod(_Global_Noise_Lookup, float4(noiseUv, 0, 0));
				float brightness = starGrid.b;
				float2 uvOff = starGrid.rg - 0.5;
				float2 starOff = starUv + uvOff * 0.6;

				float ray = max(0., 1. - abs(starOff.x * starOff.y * 300.) * 300 * w ) * step(length(uvOff), 0.3); // *brightness;

				float starSdf =
					
					ray * (1 + sin(_Effect_Time.x * starGrid.r * 20)) * 0.1 +
					0.01  * (brightness  
						//+ w * 200
						) / length(starOff);

				starSdf *= 
				smoothstep(0.005, 0.1, starSdf) * visibility 
#if !_BLACK_HOLE
					* visibility
#endif
					;

				float3 starCol = float3(0.6, 0.3, 1) + starGrid.rgb;
				return float4(starCol 
					* starSdf * step(0.7, brightness), 1);

			}

			float4 AllStars(float2 starsUvTiled, float w, float visibility)
			{
				const float CLOSER_STARS = 0.8;
				const float MID_STARS = CLOSER_STARS + 0.8;
				const float FAR_STARS = MID_STARS + 1.5;

				float PARALLAX_FORCE = _Mushroom_Star_Position.w;
				qc_ParallaxOffset.xy *= PARALLAX_FORCE;

				return 
					
					saturate(
					StarFromNoiseMask(starsUvTiled * CLOSER_STARS, w, visibility)
					+ 
					StarFromNoiseMask(2 + starsUvTiled * MID_STARS - qc_ParallaxOffset.xy * 0.5, w, visibility)
					+ StarFromNoiseMask(10 + starsUvTiled * FAR_STARS - qc_ParallaxOffset.xy * 2.5 , w, visibility) 
					) * 0.4
					;
			}


			float4 frag(v2f o) : COLOR{

				o.screenPos.xy /= o.screenPos.w;
				float2 offCenterDirectionUStretched = (o.screenPos.xy - 0.5);
				float2 offCenterDirection = offCenterDirectionUStretched * o.stretch;

				float PARALLAX_FORCE = _Mushroom_Star_Position.w;
				qc_ParallaxOffset.xy *= PARALLAX_FORCE;

				//qc_ParallaxOffset.xy -= _Mushroom_Scroll_Position.xy;

				float offScreenCenter = 1 - length(o.screenPos.xy - 0.5);

				float2 screenPos = o.screenPos.xy;

				o.screenPos.xy = offCenterDirection + qc_ParallaxOffset.xy;

				float offScreenCenterStretched = 1 - length(offCenterDirectionUStretched);
				o.screenPos.xy += 0.5;

				float2 center = _Mushroom_Star_Position.xy;// +qc_ParallaxOffset.xy * 0.1;
				float2 offset = o.screenPos.xy - center -_Mushroom_Scroll_Position.xy * 2;
				float lenFromStar = length(offset);
				float deLen = 1 - lenFromStar;
				float w = length(abs(fwidth(o.screenPos.xy)));
				float cutoff = (1 - _Mushroom_Star_Position.z * 0.15 * _Mushroom_Light_Visibility);
				float starSky = smoothstep(cutoff + 0.02, cutoff, deLen);
				float2 uv = o.screenPos.xy - center;
			
				_Mushroom_Star_Color.a *= _Mushroom_Light_Visibility;
				
				const float STARS_PARALLAX = 5;
				const float DYSO_PARALLAX = 8;

			

				float4 col = _Mushroom_Background_Color * 0.25;
				col.a = 0;

#if USE_NOISE_TEXTURE
				float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(o.screenPos.xy * 13.5 + float2(_SinTime.w, _CosTime.w) * 32, 0, 0));
#else
				float4 noise = 0.5;
#endif


				float2 starsUvTiled = uv * _StarsScale + qc_ParallaxOffset.xy * STARS_PARALLAX;

			#if !_DYSON_SPHERE 
				
				#if _SPACE_FOG

					#if _GYROID_FG

						float gyrMask =  tex2D(_CloudsTex2, screenPos );

						const float COLOR_TRANSITION_THOLD = 3.5;

						float gyr = sdGyroid(float3(screenPos * (1.5 + qc_RND_SEEDS.w * 1.5) * _StarsScale + _Effect_Time.x - _Mushroom_Scroll_Position.xy * 20, sin(uv.x * 3 + uv.y * 3.7 + _Effect_Time.x) + qc_RND_SEEDS.w*5));
						float gyr2 = sdGyroid(float3(screenPos * 2 * _StarsScale + gyr,  sin(uv.x * 3.2 + uv.y * 2.7 + _Effect_Time.x) + qc_RND_SEEDS.w));

						const float SHARPNESS = 1.5;

						gyr = 
							(
								gyr * (1.5 - saturate(gyr2) )

								) * lenFromStar
							
							+ SHARPNESS - offScreenCenter * (SHARPNESS * 1.5);

						float color0 = smoothstep(0, COLOR_TRANSITION_THOLD , gyr);

						gyr = smoothstep(0, COLOR_TRANSITION_THOLD, gyr);

						float4 gyrCol = ((color0) * _Mushroom_Clouds_Color +
							(1 - color0) * _Mushroom_Clouds_Color_2)
							;

						gyrCol.a = smoothstep(0, 0.5, gyr);

						gyrCol.rgb *= gyrCol.a;

						col += gyrCol;
			
					#else


						float2 fogUV = (TRANSFORM_TEX(uv, _StarTex) 
							+ qc_ParallaxOffset.xy) * 0.2//* STARS_PARALLAX) * 0.2
							- offset  * lenFromStar * 0.4;

						float cloud1 = tex2Dlod(_CloudsTex, float4(Rot(fogUV, deLen * _SinTime.x*0.3)  - float2(0, 0.02) * _Effect_Time.x, 0, 0));
						float cloud2 = tex2Dlod(_CloudsTex2, float4(
							fogUV
							+ float2(0.03, 0) * _Effect_Time.x, 0, 0));

						float cloud = cloud1 * cloud2;

						offScreenCenterStretched *= offScreenCenterStretched;

						float mainCloud = offScreenCenterStretched * 1.5;
						float secondaryCloud = offScreenCenterStretched * 0.75;// *(1 - cloudOffset);

						float cloudSmall = smoothstep(mainCloud, mainCloud + w * 25, cloud);
						float cloudBig = smoothstep(secondaryCloud, secondaryCloud + w * 25, cloud);

						float showClouds = cloudBig * _Mushroom_Light_Visibility * 0.5 * _Mushroom_Clouds_Color.a;

						col =
							col * (1 - showClouds) +
							 showClouds * (
							cloudSmall * _Mushroom_Clouds_Color + (1 - cloudSmall) * _Mushroom_Clouds_Color_2 
								);

						col.a =  max(0, showClouds + cloudSmall);

						col.rgb *= col.a;
					#endif
				#endif
			#endif


			#if _BLACK_HOLE
				float fresnel = 0.1 / (abs(deLen - cutoff) * 10 + w * 1);

				float blackHole = smoothstep(cutoff-0.1, cutoff + w * 30, deLen) * _Mushroom_Star_Color.a;

				starsUvTiled = Rot(starsUvTiled, blackHole * blackHole) - offset * blackHole * blackHole * 8;

				float visibility = (lenFromStar + blackHole * 0.7) * _Mushroom_Star_Color.a + (1 - _Mushroom_Star_Color.a);

			#if _PROCEDURAL_STARS && USE_NOISE_TEXTURE
				float4 starColor = AllStars(starsUvTiled, w, visibility);

			#else
				float mask = tex2Dlod(_StarTex, float4(starsUvTiled, 0, 0)).r;
				float4 starColor = StarFromSdf(mask, w, visibility);
			#endif


				float4 orbitingDust = tex2D(_Outline, float2(starsUvTiled.y, -starsUvTiled.x) + float2(_Time.x, 0) + qc_ParallaxOffset.xy*0.1);

				float4 fogg = float4(orbitingDust.rgb, 0) * blackHole * 0.25;

				col = 
					col +
					(
					starColor  +
					((deLen * starSky * 0.5 + fogg) * _Mushroom_Star_Color
					+ fogg 
						) * _Mushroom_Star_Color.a
						+ _Mushroom_Star_Color * fresnel * _Mushroom_Star_Color.a
						) * starSky* (1 - col.a)
						; 
			#else
				
				float sun = smoothstep(cutoff - 0.05, cutoff + w * 30, deLen);

				float fresnel = 0.1 * sun / (abs(deLen - cutoff) * 10 + w * 1);

				const float PI = 3.14159265359;

				float angle = (atan2(offset.x, offset.y) + PI) / (PI * 2);

				float4 clouds = tex2Dlod(_Outline, float4(angle * 3, deLen * 0.3 + _Time.x * 0.1, 0, 0));

				float2 surfaceUV =
					(offset * 15 ) * (0.2 + lenFromStar)
					- qc_ParallaxOffset.xy 
					;

				float4 surfaceOfTheSun = tex2Dlod(_Surface, float4(surfaceUV, 0, 0));

				float starVisibility = lenFromStar * _Mushroom_Star_Color.a + (1 - _Mushroom_Star_Color.a);


			#if _DYSON_SPHERE

				const float HEXES_FILL = 0.9;
				float2 newUv;
				float4 hexagons = GetHexagons(uv * 5 * (2 + (1- _Mushroom_Light_Visibility) * 2)  + qc_ParallaxOffset.xy * DYSO_PARALLAX, _Global_Noise_Lookup_TexelSize, newUv);
				float4 hexNoise = tex2Dlod(_Global_Noise_Lookup, float4(hexagons.xy * _Global_Noise_Lookup_TexelSize.xy, 0, 0));
				float4 dysonSrf = tex2D(_DysonSurface, Rot(newUv * 0.15 * (1 + hexNoise.b), hexNoise.r * 5) + (hexNoise.rg - 0.5) * 0.25 + 0.5);
				float alpha = smoothstep(HEXES_FILL + w * 64, HEXES_FILL, hexagons.b) * step(hexNoise.b, 0.5) * _Mushroom_Light_Visibility;
				col = ((dysonSrf * alpha + (1 - alpha) * 2 * _Mushroom_Star_Color)) * alpha +col * (1 - alpha);

			#elif _PROCEDURAL_STARS && USE_NOISE_TEXTURE
				col.rgb += AllStars(starsUvTiled, w, starVisibility) * (1 - col.a);
			#else
				float2 starSurfaceUv = TRANSFORM_TEX(uv, _StarTex);
				float mask = tex2D(_StarTex, starSurfaceUv + qc_ParallaxOffset.xy * STARS_PARALLAX).r;
				float stars = StarFromSdf(mask, w, starVisibility);
				col.rgb += stars * starSky * (1- col.a);

			#endif


				col.rgb = 
					col.rgb * (smoothstep(0.6, 0.45, sun)) 
						+ ((sun * (0.2 + surfaceOfTheSun * sun + fresnel * 2) + deLen * 0.3 * (1- sun) ) * _Mushroom_Star_Color * _Mushroom_Star_Color.a
					)
					;

				float3 mix = col.gbr + col.brg;

				col.rgb += 
					_Mushroom_Light_Visibility * (
					mix * sun * sun * 0.1 * surfaceOfTheSun +
					(clouds.rgb * (_Mushroom_Star_Color.rgb + 1) * 0.5 * deLen * deLen * deLen * starSky) * (1-col.a)
						);
				#endif

				#if USE_NOISE_TEXTURE
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