Shader "Quiz cAnners/UI/Blurred Screens Wih Bump"
{
    Properties
    {
        [PerRendererData]
        _MainTex("Sprite Texture", 2D) = "white" {}
        _BumpMap("Bump Map", 2D) = "bump" {}
        _Color("Tint", Color) = (1,1,1,1)

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _ColorMask("Color Mask", Float) = 15

        [Toggle(_USE_SCREEN_SPACE)] _useUv("Use Screen Space", Float) = 0
        [Toggle(TOUCH_REACTION)] _ReactOnTouch("React To Touch", Float) = 0
        [Toggle(TOUCH_SHOW)] _ShowOnTouch("Show Only on touch", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass
        {
            Name "Default"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature_local __ _USE_SCREEN_SPACE
            #pragma shader_feature_local ___ TOUCH_REACTION
            #pragma shader_feature_local ___ TOUCH_SHOW

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                half4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                float4 screenPos : 	TEXCOORD2;
#if _USE_SCREEN_SPACE
                float2 stretch		: TEXCOORD3;
#endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _BumpMap;
            float4 _BumpMap_TexelSize;

            float _Effect_Time;
            sampler2D _qcPp_Global_Screen_Effect;
            fixed4 _Color;
            float4 _qcPp_MousePosition;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.screenPos = ComputeScreenPos(OUT.vertex);
                OUT.color = v.color * _Color;

#if _USE_SCREEN_SPACE
                float screenAspect = _ScreenParams.x * (_ScreenParams.w - 1);
                float texAspect = _BumpMap_TexelSize.y * _BumpMap_TexelSize.z;
                float2 aspectCorrection = float2(1, 1);
                if (screenAspect > texAspect)
                    aspectCorrection.y = (texAspect / screenAspect);
                else
                    aspectCorrection.x = (screenAspect / texAspect);
                OUT.stretch = aspectCorrection;
#endif

                return OUT;
            }


            fixed4 frag(v2f IN) : SV_Target {

                float2 screenPos = IN.screenPos.xy / IN.screenPos.w;
                float2 bumpUv;

                #if _USE_SCREEN_SPACE
                    bumpUv = ((screenPos - 0.5) * IN.stretch.xy + 0.5) * 4.25; 
                    // Bump is always better when mipmapped for some reason
                #else
                    bumpUv = IN.texcoord;
                #endif


                float3 tnormal = UnpackNormal(tex2D(_BumpMap, bumpUv));


                float alpha = tex2D(_MainTex, IN.texcoord).a * IN.color.a;
            

                #if TOUCH_REACTION
              
                    half2 fromMouse = (screenPos - _qcPp_MousePosition.xy);
                    fromMouse.x *= _qcPp_MousePosition.w;
                    float lenM = length(fromMouse);// *4;

                    float bumpAlpha = //alpha * saturate((0.99 - (lenM) * 0.9) * _qcPp_MousePosition.z);

                    alpha * smoothstep(0.2, 0, lenM + sin(lenM * 50 - _Effect_Time * 50) * 0.02) * _qcPp_MousePosition.z;

                    tnormal.rg *= bumpAlpha * bumpAlpha;

                        #if TOUCH_SHOW
                            alpha = bumpAlpha;
                        #endif



                #endif

                screenPos += (tnormal.rg) * 0.05;

                float3 p = float3(screenPos * 30, _Time.y);
                float gyroid = dot(sin(p), cos(p.zxy));
                screenPos += (gyroid) * 0.002;

                fixed4 color = tex2D(_qcPp_Global_Screen_Effect, float4(screenPos, 0, 0));

               // color.a = smoothstep((lenM) * 0.4, 1, IN.color.a);

                color *= IN.color;

                color.a = alpha;//smoothstep(0, 0.5, alpha);

                return color;
            }
        ENDCG
        }
    }
    Fallback "Legacy Shaders/Transparent/VertexLit"
}