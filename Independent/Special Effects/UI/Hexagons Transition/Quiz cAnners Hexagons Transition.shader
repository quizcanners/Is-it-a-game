Shader "Quiz cAnners/UI/Hexagonal Transition"
{
    Properties{
      _MainTex("Albedo (RGB)", 2D) = "black" {}
     [KeywordEnum(SCREEN_SHOT, BLURRED_SCREEN)] _Target("Screen Grab Data", Float) = 0

    }

        Category{
            Tags{
              "Queue" = "Transparent"
              "PreviewType" = "Plane"
              "IgnoreProjector" = "True"
              "RenderType" = "Transparent"
            }

            ColorMask RGB
            Cull Off
            ZWrite On
            ZTest[unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha

            SubShader{
                Pass{

                    CGPROGRAM

                    #include "UnityCG.cginc"

                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma multi_compile_fwdbase
                    #pragma multi_compile_instancing
                    #pragma multi_compile __ _qcPp_FEED_MOUSE_POSITION
                    #pragma shader_feature_local _SCREEN_SHOT  _BLURRED_SCREEN 
                    

                      struct v2f {
                        float4 pos : SV_POSITION;
                        float2 screenParams : TEXCOORD1;
                        float2 texcoord : TEXCOORD2;
                        float4 screenPos : 	TEXCOORD3;
                        float4 color: COLOR;
                      };

                    #if _SCREEN_SHOT  
                        sampler2D _qcPp_Global_Screen_Read;
                    #else
                        sampler2D _qcPp_Global_Screen_Effect;
                    #endif
                        float4 _qcPp_MousePosition;

                      v2f vert(appdata_full v) {
                        v2f o;
                        UNITY_SETUP_INSTANCE_ID(v);

                        o.pos = UnityObjectToClipPos(v.vertex);
                        o.texcoord.xy = v.texcoord.xy;
                        float aspect = _ScreenParams.x / _ScreenParams.y;

                        o.screenParams = float2(max(_ScreenParams.x / _ScreenParams.y, 1), max(1, _ScreenParams.y / _ScreenParams.x));

                        o.screenPos = ComputeScreenPos(o.pos);

                        o.color = v.color;

                        return o;
                      }

                      float GetHexagon(float2 uv) {
                        uv = abs(uv);

                        const float2 toDot = normalize(float2(1, 1.73));

                        float c = dot(uv, toDot);

                        return max(c, uv.x);
                      }

                      inline float4 GetHexagons(float2 grid, float2 texelSize, out float2 uv)
                      {

                        grid = grid * 1.03 - float2(0.03, 0.06) * texelSize;

                        const float2 r = float2(1, 1.73);

                        const float2 h = r * 0.5;

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

                        const float pii = 3.141592653589793238462;

                        const float pi2 = 1.0 / 6.283185307179586476924;

                        float angle = 0;//(atan2(uv.x, uv.y) + pii) * pi2;

                        return float4(index, dist, angle);

                      }


                      float4 frag(v2f i) : COLOR{

                          i.screenPos.xy /= i.screenPos.w;

                          float2 fromMouse = (i.screenPos.xy - _qcPp_MousePosition.xy);

                          fromMouse.x *= _qcPp_MousePosition.w;

                          float proximity = max(0, (1 - length(fromMouse)));// *CLICK_POWER_SHARPNESS));

                          float pressPower = proximity * _qcPp_MousePosition.z;

                          pressPower *= 0.5 + sin(pressPower * 10) * 0.5;

                          float2 fitToScreen = i.screenPos.xy * i.screenParams;

                          float2 grid = fitToScreen *
                            #if APPEAR_TRANSITION  
                              15
                            #else
                              10
                            #endif
                            ;

                          float2 hexUv;

                          float4 hex = GetHexagons(grid, float2(512,512), hexUv);

                          float alpha = i.color.a;

                          float deAlpha = 1 - alpha;

                          alpha = saturate((alpha + length(fromMouse) - deAlpha * 2.5));

                          deAlpha = 1 - alpha;

                        float dist = hex.z;

                        float2 uv = hex.zw;

                          float4 col = tex2Dlod(
#if _SCREEN_SHOT  
                              _qcPp_Global_Screen_Read
#else 
                              _qcPp_Global_Screen_Effect
#endif
                              
                              , float4(i.screenPos.xy,0,0)); 

                          float sharpness = (9.5 - deAlpha * 9);

                         col.a = smoothstep(0,1,(alpha * 2 - dist) * sharpness);

    
                       //  col.rgb += float3(0.1, 0.3, 0.5) * 3 * (1 - col.a);

                     
                       return col;

                     }
                     ENDCG
                 }
             }
             Fallback "Legacy Shaders/Transparent/VertexLit"
      }
}
