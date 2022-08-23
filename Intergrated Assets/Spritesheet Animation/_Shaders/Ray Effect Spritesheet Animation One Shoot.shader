// Edited version of Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "RayTracing/Effect/Spritesheet/Simple"
{
Properties 
{
    _MainTex ("Sprite Sheet", 2D) = "white" {}
    _GridSize ("Grid Size", Range(1,128)) = 1.0
     _Emission ("Emission", Range(0,1)) = 0
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
            ZTest Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            	#pragma multi_compile_fwdbase
				#pragma multi_compile_instancing
				#pragma multi_compile ___ TOP_DOWN_LIGHT_AND_SHADOW

			 	#include "Assets/Ray-Marching/Shaders/PrimitivesScene_Sampler.cginc"
				#include "Assets/Ray-Marching/Shaders/Signed_Distance_Functions.cginc"
				#include "Assets/Ray-Marching/Shaders/RayMarching_Forward_Integration.cginc"
				#include "Assets/Ray-Marching/Shaders/Sampler_TopDownLight.cginc"

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float _GridSize;
            float _Emission;

            struct appdata_t {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float4 texcoord : TEXCOORD0;
                float2 texcoordInternal : TEXCOORD2;
                float2 blend : TEXCOORD3;
                float3 viewDir	: TEXCOORD4;
                float3 worldPos : TEXCOORD5;
                float4 screenPos : TEXCOORD6;

                UNITY_VERTEX_OUTPUT_STEREO
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Frame)
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
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                o.viewDir.xyz = WorldSpaceViewDir(v.vertex);
                 COMPUTE_EYEDEPTH(o.screenPos.z);
              // o.color = v.color * _TintColor;

                float frame01 = UNITY_ACCESS_INSTANCED_PROP(Props, _Frame)*0.99;
              


                float maxFrames = _GridSize * _GridSize;

                float frame = frame01 * maxFrames;

                float firstFrame = floor(frame);
                float secondFrame = floor(min(maxFrames-1, frame + 1));

                o.texcoord.xy = FrameToUv(firstFrame);
                o.texcoord.zw =FrameToUv(secondFrame); 

                o.texcoordInternal =  v.texcoord / _GridSize;
                o.texcoordInternal.y = - o.texcoordInternal.y;

                o.blend.x = frame - firstFrame;

                return o;
            }


            float4 frag (v2f i) : SV_Target
            {
              
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                 i.viewDir.xyz = normalize(i.viewDir.xyz);

               float frameBlend = i.blend.x;

                float4 colA = tex2D(_MainTex, i.texcoord.xy  + i.texcoordInternal);
                float4 colB = tex2D(_MainTex, i.texcoord.zw + i.texcoordInternal);
				float4 col = lerp(colA, colB, frameBlend);

                float brightness = _Emission * length(col.rgb * col.gbr * col.brg); // Fire
                float offCenter = length(i.texcoordInternal-0.5);

                float volumetriEdge =  offCenter  - brightness*0.1 -col.a * 2;

                float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
			    float sceneZ = LinearEyeDepth(UNITY_SAMPLE_DEPTH(depth));
			    float fade = smoothstep(volumetriEdge ,volumetriEdge + 2, (sceneZ - i.screenPos.z));
			    float toCamera = smoothstep(0,1, length(_WorldSpaceCameraPos - i.worldPos.xyz) - _ProjectionParams.y);

                float  outOfBounds;	
			    float4 vol = SampleVolume(i.worldPos, outOfBounds);
			    TopDownSample(i.worldPos + i.viewDir * (col.a - offCenter * 2), vol.rgb, outOfBounds);
			    float3 ambientCol = lerp(vol, _RayMarchSkyColor.rgb * MATCH_RAY_TRACED_SKY_COEFFICIENT, outOfBounds);

                col.rgb = ambientCol * (1 + col.rgb)*0.5 + col.rgb * brightness*0.5;

                col.a = smoothstep(0,1, col.a * toCamera * fade);

                return col ;
            }
            ENDCG
        }
    }
}
}
