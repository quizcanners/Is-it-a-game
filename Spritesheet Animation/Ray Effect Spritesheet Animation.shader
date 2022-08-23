// Edited version of Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "RayTracing/Effect/Spritesheet Animation"
{
Properties {
    [HDR]_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
    _MainTex ("Intro Outro", 2D) = "white" {}
    _LoopTex ("Loop", 2D) = "white" {}
    _InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
    _GridSize ("Grid Size", Range(1,128)) = 1.0
    //_Frame ("Frame", Range(0,2)) = 1.0
}

Category {
    Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }

    SubShader {
        Pass {

		    Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off 
            Lighting Off 
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
          //  #pragma multi_compile_particles

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _LoopTex;
            fixed4 _TintColor;
            float _GridSize;

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                float2 texcoord2 : TEXCOORD1;
                float2 texcoordInternal : TEXCOORD2;
                fixed blend : TEXCOORD3;
                UNITY_FOG_COORDS(3)
              //  #ifdef SOFTPARTICLES_ON
                float4 projPos : TEXCOORD4;
               // #endif
                UNITY_VERTEX_OUTPUT_STEREO
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Frame)
                UNITY_DEFINE_INSTANCED_PROP(float, _LoopVisibility)
            UNITY_INSTANCING_BUFFER_END(Props)

            float2 FrameToUv (float frameIndex)
            {
                float row = frameIndex % _GridSize;
                float column = floor(frameIndex / _GridSize);
                float2 uv = (float2(row, column)) / _GridSize;
                uv.y = 1-uv.y;
                return uv;
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
              //  #ifdef SOFTPARTICLES_ON
                o.projPos = ComputeScreenPos (o.vertex);
                COMPUTE_EYEDEPTH(o.projPos.z);
              //  #endif

                o.color = v.color * _TintColor;

                float frame01 = UNITY_ACCESS_INSTANCED_PROP(Props, _Frame);

                float maxFrames = _GridSize * _GridSize;

                float frame = frame01 * maxFrames;

                float firstFrame = floor(frame);
                float secondFrame = floor(min(maxFrames, frame + 1));

                o.texcoord = FrameToUv(firstFrame);

                o.texcoord2 =FrameToUv(secondFrame); 

                o.texcoordInternal =  v.texcoord / _GridSize;

                o.texcoordInternal.y = - o.texcoordInternal.y;

                o.blend = frame - firstFrame;
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            float _InvFade;

            
            float4 LerpTransparent(float4 col1, float4 col2, float transition)
            {
                float4 col;
                
                col.rgb = lerp(col1.rgb * col1.a, col2.rgb * col2.a, transition);
                col.a = lerp(col1.a, col2.a, transition);
                /*
	            float rgbAlpha = col2.a * transition;
	            rgbAlpha = smoothstep(0,1, rgbAlpha * 2 / (col1.a + rgbAlpha + 0.001));
	            col1.a =  lerp(col1.a, col2.a, transition);
	            col1.rgb = lerp(col1.rgb, col2.rgb, rgbAlpha);

                return col1;*/

                return col;
            }

            float4 frag (v2f i) : SV_Target
            {

               // #ifdef SOFTPARTICLES_ON
                float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
                float partZ = i.projPos.z;
                float fade = saturate (_InvFade * (sceneZ-partZ));
                i.color.a *= fade;
               // #endif

               float frame01 = UNITY_ACCESS_INSTANCED_PROP(Props, _LoopVisibility);

                float4 colA = tex2D(_MainTex, i.texcoord  + i.texcoordInternal);
                float4 colB = tex2D(_MainTex, i.texcoord2 + i.texcoordInternal);
				float4 colInto = lerp(colA, colB, i.blend);

                float brightness = length(colInto.rgb * colInto.gbr * colInto.brg) * smoothstep(0.01, 0.2, 1-_LoopVisibility); // Fire

              
                // TODO: Loop, upscale the next sprite from LoopVisibility
                //_LoopTex
                float4 colLA = tex2D(_LoopTex, i.texcoord+ i.texcoordInternal);
                float4 colLB = tex2D(_LoopTex, i.texcoord2+ i.texcoordInternal);
				float4 colLoop = lerp(colLA, colLB, i.blend);




              //  float introAnimation = (1-_LoopVisibility) * colInto.a;

                float4 col =  LerpTransparent(colInto, colLoop, _LoopVisibility)* i.color;

                col.a = min(1,col.a);

                return col ;
            }
            ENDCG
        }
    }
}
}
