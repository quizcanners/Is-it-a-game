Shader "Quiz cAnners/Buffer Blit/Blur"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "black"{}
    }

    SubShader{

        Tags { "Queue" = "Overlay" "RenderType" = "Overlay" }
        LOD 10
        ColorMask RGBA
        Cull Off
        ZTest Always
        ZWrite Off

        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
		    #pragma multi_compile ___ USE_NOISE_TEXTURE
            #pragma multi_compile ___ _qcPp_FEED_MOUSE_POSITION
		    #include "UnityCG.cginc"


            struct v2f {
              float4 pos : 		SV_POSITION;
              float2 texcoord : 	TEXCOORD1;
              float2 rate : TEXCOORD2;
            };

            v2f vert(appdata_full v) 
            {
              v2f o;
              o.pos = UnityObjectToClipPos(v.vertex);
              o.texcoord.xy = v.texcoord.xy;

              float screenAspect = _ScreenParams.x * (_ScreenParams.w - 1);
              float relation = _ScreenParams.x / _ScreenParams.y;

              o.rate.xy = float2(1, 1);

              if (screenAspect > 1)
                  o.rate.x = (1 / screenAspect);
              else
                  o.rate.y = (screenAspect);

              float pixelSc = min(_ScreenParams.z, _ScreenParams.w)-1;

              pixelSc *= 6;

              o.rate.x *= pixelSc;
              o.rate.y *= pixelSc;

              return o;
            }

            sampler2D _MainTex;
			uniform sampler2D _Global_Noise_Lookup;
            uniform float2 _qcPp_MousePosition;
            float _qcPp_Screen_Blur_Iteration;

            float4 frag(v2f o) : SV_TARGET{

                float2 uv = o.texcoord.xy;
              
#if USE_NOISE_TEXTURE
                //const float _BlurEffectPortion = 0.015;
                float4 noise = tex2Dlod(_Global_Noise_Lookup, float4(uv - float2(_Time.x * 4, _Time.x*7.23), 0, 0)) -0.5;
#else
               //const float _BlurEffectPortion = 0.015;
				float4 noise = 0.5;
#endif

                float4 sum = 0;

               // float relation = o.rate.z;

                float ker = (1 + _qcPp_Screen_Blur_Iteration) * 0.15;

                float xker = (1 * noise.b) * o.rate.x * ker;

                #define GRABPIXELX(weight,kernel) tex2Dlod( _MainTex, float4(uv + float2(kernel*xker, 0)  ,0,0)) * weight

                sum += GRABPIXELX(0.15, -2.0);
                sum += GRABPIXELX(0.30, -1.0);
                sum += GRABPIXELX(0.10, 0.0);
                sum += GRABPIXELX(0.30, +1.0);
                sum += GRABPIXELX(0.15, +2.0);

                float yker = noise.g * o.rate.y * ker;

                #define GRABPIXELY(weight,kernel) tex2Dlod( _MainTex, float4(uv + float2(0, kernel*yker)  ,0,0)) * weight

                sum += GRABPIXELY(0.15, -2.0);
                sum += GRABPIXELY(0.30, -1.0);
                sum += GRABPIXELY(0.10, 0.0);
                sum += GRABPIXELY(0.30, +1.0);
                sum += GRABPIXELY(0.15, +2.0);

                sum.rgb *= 0.5;

                return sum;
            }
             ENDCG
        }
    }
      FallBack Off
}
