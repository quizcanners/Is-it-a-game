Shader "RayTracing/Effect/Blood Atlas Animation" 
{
	Properties 
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

		[Toggle(_MOTION_VECTORS)] motVect("Has Flow Motion Vectors", Float) = 0
		_MotionVectorsMap("Flow Motion Vetors", 2D) = "white" {}
		_FlowIntensity("Flow Intensity", Range(0,1)) = 0

		_GridSize_Col("Columns", Range(1,128)) = 1.0
		_GridSize_Row("Rows", Range(1,128)) = 1.0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" 
		}

	
		Blend SrcAlpha OneMinusSrcAlpha
		Cull Off 
		Lighting Off
		ZWrite Off

		Pass 
		{
		
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "Assets/Ray-Marching/Shaders/PrimitivesScene_Sampler.cginc"
			#include "Assets/Ray-Marching/Shaders/Signed_Distance_Functions.cginc"
			#include "Assets/Ray-Marching/Shaders/RayMarching_Forward_Integration.cginc"
			#include "Assets/Ray-Marching/Shaders/Sampler_TopDownLight.cginc"

			sampler2D _MainTex;
			fixed4 _TintColor;
			
			#pragma shader_feature_local __ _MOTION_VECTORS

			struct appdata_t 
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;

				 UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f 
			{
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float4 texcoord : TEXCOORD0;
				float4 projPos : TEXCOORD1;
				float3 worldPos : TEXCOORD2;
				float3 worldNormal : TEXCOORD3;
                float4 screenPos : TEXCOORD4;
                float3 viewDir : TEXCOORD5;
				float interpolation : TEXCOORD6;

#if _MOTION_VECTORS
				float4 motionVectorSampling : TEXCOORD7;
#endif

				UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
			};
			
			float4 _MainTex_ST;
			sampler2D _MotionVectorsMap;
			float _FlowIntensity;
			float _GridSize_Col;
			float _GridSize_Row;

			UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _SpriteSegment)
				UNITY_DEFINE_INSTANCED_PROP(float4, InterpolationValue)
            UNITY_INSTANCING_BUFFER_END(Props)

			v2f vert (appdata_t v)
			{
				v2f o;

				UNITY_INITIALIZE_OUTPUT(v2f, o);

				UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.vertex = UnityObjectToClipPos(v.vertex);
			
				o.projPos = ComputeScreenPos (o.vertex);
				COMPUTE_EYEDEPTH(o.projPos.z);
				o.color = v.color * _TintColor;


				o.viewDir = WorldSpaceViewDir(v.vertex);
                o.screenPos = ComputeGrabScreenPos(o.vertex);


				 o.worldPos = mul(unity_ObjectToWorld, v.vertex);

				float4 segment = UNITY_ACCESS_INSTANCED_PROP(Props, _SpriteSegment);// : 1;
				float4 interpolation = UNITY_ACCESS_INSTANCED_PROP(Props, InterpolationValue);

				o.texcoord.xy = v.texcoord.xy * segment.xy + segment.zw;
				o.texcoord.zw = v.texcoord.xy * segment.xy + interpolation.zw;

				o.interpolation = interpolation.x;

#if _MOTION_VECTORS
				float2 deGrid = 1 / float2(_GridSize_Col, _GridSize_Row);
				o.motionVectorSampling = MotionVectorsVertex(_FlowIntensity, o.interpolation, deGrid);
#endif

				//o.texcoord = flaot4(v.texcoord.xy, 0,0); // TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			float _InvFade;
			
			      
			float4 _qc_BloodColor;

			fixed4 frag (v2f i) : COLOR
			{
				UNITY_SETUP_INSTANCE_ID(i);

				float4 interpolation = UNITY_ACCESS_INSTANCED_PROP(Props, InterpolationValue);// : 1;


#if _MOTION_VECTORS
				OffsetByMotionVectors(i.texcoord.xy, i.texcoord.zw, i.motionVectorSampling, _MotionVectorsMap);
#endif


                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);
			//	float2 screenUV = i.screenPos.xy / i.screenPos.w;

				fixed4 col = //i.color *
					LerpTransparent( tex2D(_MainTex, i.texcoord.xy), tex2D(_MainTex, i.texcoord.zw), i.interpolation );

				float  outOfBounds;	
				float4 vol = SampleVolume(i.worldPos, outOfBounds);
				TopDownSample(i.worldPos, vol.rgb, outOfBounds);
				float3 ambientCol = lerp(vol, _RayMarchSkyColor.rgb * MATCH_RAY_TRACED_SKY_COEFFICIENT, outOfBounds);

			

			

				//float sceneZ = LinearEyeDepth (UNITY_SAMPLE_DEPTH(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos))));
				//float partZ = i.projPos.z;
				//float fade = saturate (_InvFade * (sceneZ-partZ));
				//col.a *= fade;
		
				col.rgb =_qc_BloodColor.rgb * (1 - col.a*col.a) * ambientCol; // * (3 - col.a * 2);

				col.a = smoothstep(0, 0.5, col.a);

				//ApplyBottomFog(col.rgb, i.worldPos.xyz, i.viewDir.y);

				return col;
			}
			ENDCG 
		}
	}	
}

