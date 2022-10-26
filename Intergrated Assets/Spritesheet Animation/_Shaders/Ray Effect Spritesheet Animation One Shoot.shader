// Edited version of Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "RayTracing/Effect/Spritesheet/Manual Animation"
{
Properties 
{
    _MainTex ("Sprite Sheet", 2D) = "white" {}
    _GridSize_Col ("Columns", Range(1,128)) = 1.0
    _GridSize_Row("Rows", Range(1,128)) = 1.0
     _Emission ("Emission", Range(0,1)) = 0

     [Toggle(_MOTION_VECTORS)] motVect("Has Flow Motion Vectors", Float) = 0
      _MotionVectorsMap("Flow Motion Vetors", 2D) = "white" {}
      _FlowIntensity("Flow Intensity", Range(0,1)) = 0
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
            #pragma shader_feature_local __ _MOTION_VECTORS

			#include "Assets/Ray-Marching/Shaders/PrimitivesScene_Sampler.cginc"
			#include "Assets/Ray-Marching/Shaders/Signed_Distance_Functions.cginc"
			#include "Assets/Ray-Marching/Shaders/RayMarching_Forward_Integration.cginc"
			#include "Assets/Ray-Marching/Shaders/Sampler_TopDownLight.cginc"

            #include "UnityCG.cginc"

            sampler2D _MainTex;
         
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
                float2 texcoordInternal : TEXCOORD1;
                float2 blend : TEXCOORD2;
                float3 viewDir	: TEXCOORD3;
                float3 worldPos : TEXCOORD4;
                float4 screenPos : TEXCOORD5;

#if _MOTION_VECTORS
                float4 motionVectorSampling : TEXCOORD6;
#endif

                UNITY_VERTEX_OUTPUT_STEREO
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float, _Frame)
            UNITY_INSTANCING_BUFFER_END(Props)

            float _GridSize_Col;
            float _GridSize_Row;

            sampler2D _MotionVectorsMap;
            float _FlowIntensity;

            float2 FrameToUv (float frameIndex)
            {
                float row = frameIndex % _GridSize_Row;
                float column = floor(frameIndex / _GridSize_Row);
                float2 uv = (float2(row, column)) / float2(_GridSize_Row, _GridSize_Col);
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
              
                float maxFrames = _GridSize_Col * _GridSize_Row;

                float frame = frame01 * maxFrames;

                float firstFrame = floor(frame);
                float secondFrame = min(maxFrames - 1, firstFrame + 1);// floor(min(maxFrames - 1, frame + 1));

                o.texcoord.xy = FrameToUv(firstFrame);
                o.texcoord.zw =FrameToUv(secondFrame); 

                float2 deGrid = 1 / float2(_GridSize_Col, _GridSize_Row);

                o.texcoordInternal =  v.texcoord * deGrid;
                o.texcoordInternal.y = - o.texcoordInternal.y;

                o.blend.x = frame - firstFrame;

#if _MOTION_VECTORS
                o.motionVectorSampling = MotionVectorsVertex(_FlowIntensity, o.blend.x, deGrid);
#endif

                return o;
            }

         

            float4 frag(v2f i) : SV_Target
            {

                float2 uvCurrent = i.texcoord.xy + i.texcoordInternal;
                float2 uvNext = i.texcoord.zw + i.texcoordInternal;

#if _MOTION_VECTORS
                OffsetByMotionVectors(uvCurrent, uvNext, i.motionVectorSampling, _MotionVectorsMap);
#endif

                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                 i.viewDir.xyz = normalize(i.viewDir.xyz);

          
                float frameBlend = i.blend.x;

                float4 colA = tex2D(_MainTex, uvCurrent);
                float4 colB = tex2D(_MainTex, uvNext);
                float4 col =  LerpTransparent(colA, colB, frameBlend);

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

              //  ColorCorrect(col.rgb);

                col.rgb = ambientCol * (0.25f + col.rgb*0.75f) + col.rgb * brightness*0.5;

                col.a = smoothstep(0,1, col.a * toCamera * fade);

                return col ;
            }
            ENDCG
        }
    }
}
}
