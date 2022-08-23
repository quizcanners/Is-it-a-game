Shader "KriptoFX/BFX/BFX_Blood"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)

        _boundingMax("Bounding Max", Float) = 1.0
        _boundingMin("Bounding Min", Float) = 1.0
        _numOfFrames("Number Of Frames", int) = 240
        _speed("Speed", Float) = 0.33
        _HeightOffset("_Height Offset", Vector) = (0, 0, 0)
        //[MaterialToggle] _pack_normal("Pack Normal", Float) = 0
        _posTex("Position Map (RGB)", 2D) = "white" {}
        _nTex("Normal Map (RGB)", 2D) = "grey" {}
        _SunPos("Sun Pos", Vector) = (1, 0.5, 1, 0)


    }
    SubShader
    {

        Tags{ "Queue" = "AlphaTest+1"}

        CGINCLUDE
		   #include "Assets/Ray-Marching/Shaders/PrimitivesScene_Sampler.cginc"
			#include "Assets/Ray-Marching/Shaders/Signed_Distance_Functions.cginc"
			#include "Assets/Ray-Marching/Shaders/RayMarching_Forward_Integration.cginc"
			#include "Assets/Ray-Marching/Shaders/Sampler_TopDownLight.cginc"

		
            float3 ModifyPositionBySDF(float pos)
            {
                 float4 nrmAndDist = SdfNormalAndDistance(pos);

                 return pos;
            }

        ENDCG

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Back
        ZWrite On

        Pass
        {
            CGPROGRAM

         


            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile_instancing
            //#pragma multi_compile_fog
            #pragma multi_compile_fwdbase
            #pragma multi_compile ___ TOP_DOWN_LIGHT_AND_SHADOW

            #include "UnityCG.cginc"

            struct appdata_bl
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 tangent : TEXCOORD2;
               
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;

                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float3 viewDir : TEXCOORD3;
                float height : TEXCOORD4;
                 float3 worldPos : TEXCOORD5;

                 SHADOW_COORDS(6)

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _GrabTexture;
            sampler2D _posTex;
            sampler2D _nTex;
            uniform float _boundingMax;
            uniform float _boundingMin;
            uniform float _speed;
            uniform int _numOfFrames;
            half4 _Color;

            float4 _HeightOffset;
            float _HDRFix;
            float4 _SunPos;

            UNITY_INSTANCING_BUFFER_START(Props)
                //UNITY_DEFINE_INSTANCED_PROP(float, _UseCustomTime)
                UNITY_DEFINE_INSTANCED_PROP(float4, _TimeInFrames)
                //UNITY_DEFINE_INSTANCED_PROP(float, _LightIntencity)
            UNITY_INSTANCING_BUFFER_END(Props)

            v2f vert (appdata_bl v)
            {
                v2f o;

                UNITY_INITIALIZE_OUTPUT(v2f, o);

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 timeInFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _TimeInFrames);// : 1;

                float ceiled = ceil(timeInFrames.y);

                float frameUv = (ceiled + 1) / timeInFrames.w;
                float4 dataUv = float4(v.uv.x, (frameUv + v.uv.y), 0, 0);
                float4 texturePos = tex2Dlod(_posTex, dataUv);
                float3 textureN = tex2Dlod(_nTex,dataUv);


             

                //NextFrame
                //frameUv = (ceiled + 2) / timeInFrames.w;
                //dataUv.y = frameUv + v.uv.y;
                //float4 texturePos2 = tex2Dlod(_posTex, dataUv);
                //float3 textureN2 = tex2Dlod(_nTex,dataUv);


                //float toNext = 0.9; //  abs(ceiled - timeInFrames.y);

               // texturePos =  lerp(texturePos, texturePos2, toNext);
                //textureN =  lerp(textureN, textureN2, toNext);

                #if !UNITY_COLORSPACE_GAMMA
                    texturePos.xyz = LinearToGammaSpace(texturePos.xyz);
                    textureN = LinearToGammaSpace(textureN);
                #endif




                float expand = _boundingMax - _boundingMin;
                texturePos.xyz *= expand;
                texturePos.xyz += _boundingMin;
                texturePos.x *= -1;
                v.vertex.xyz = texturePos.xzy;
                v.vertex.xyz += _HeightOffset.xyz;


                 float4 worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1));
              //   worldPos.xyz = ModifyPositionBySDF(worldPos);
                 //v.vertex = mul(worldPos, unity_WorldToObject);//mul(UNITY_MATRIX_VP, worldSpacePosition);
               // o.pos = mul(UNITY_MATRIX_VP, worldPos);




                o.worldNormal = textureN.xzy * 2 - 1;
                o.worldNormal.x *= -1;
                o.viewDir = WorldSpaceViewDir(v.vertex);

               
                o.worldPos = worldPos;

                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeGrabScreenPos(o.pos);

                TRANSFER_SHADOW(o);
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);

                float3 normal = normalize(i.worldNormal);
                float3 viewDir = normalize(i.viewDir);
                float3 newPos = i.worldPos;

                float shadow = SHADOW_ATTENUATION(i);

               	float fresnel = smoothstep(-1, 1 , dot(viewDir, normal));

				float outOfBounds;
				float4 vol = SampleVolume(newPos, outOfBounds);
				TopDownSample(newPos, vol.rgb, outOfBounds);

				float3 ambientCol = lerp(vol, _RayMarchSkyColor.rgb * MATCH_RAY_TRACED_SKY_COEFFICIENT, outOfBounds);

				float direct = saturate((dot(normal, _RayMarchLightDirection.xyz)));
				float3 lightColor = _RayMarchLightColor.rgb * _RayMarchLightDirection.a * MATCH_RAY_TRACED_SUN_COEFFICIENT * direct;
				


				float4 col = float4(0.6 , 0.005, 0.005,1);

				col.rgb *=
				 
					(ambientCol * 0.5
					+ lightColor * shadow
					) ;

				float3 reflectionPos;
				float outOfBoundsRefl;
				float3 bakeReflected = SampleReflection(newPos, viewDir, normal, shadow, reflectionPos, outOfBoundsRefl);
				TopDownSample(reflectionPos, bakeReflected, outOfBoundsRefl);

				float outOfBoundsStraight;
				float3 straightHit;
				float3 bakeStraight = SampleRay(newPos, normalize(-viewDir - normal*0.2), shadow, straightHit, outOfBoundsStraight);
				TopDownSample(straightHit, bakeStraight, outOfBoundsStraight);

				float showStright = fresnel ;
	
				float world = SceneSdf(newPos, 0.1);

			/*	float3 trasparentPart =
				bakeStraight * lerp(  1, float3(1, 0.01, 0.01), saturate(world)) 
				*  showStright
				+ 
				bakeReflected *  float3(1, 0.02, 0.02) * (1.5 - showStright)
				;*/


				col.rgb = col.rgb 
				+ bakeStraight * 0.5 * lerp( float3(1, 0.3, 0.3), float3(1, 0.01, 0.01), smoothstep(0,0.05 + fresnel*0.05, world)) 
				 + bakeReflected * 0.5 *  float3(1, 0.02, 0.02) * (1.5 - showStright)
					//trasparentPart + col.rgb
					;

				ApplyBottomFog(col.rgb, newPos, viewDir.y);



     
                return col;

            }
            ENDCG
        }

        //you can optimize it by removing shadow rendering and depth writing
        //start remove line

        Pass
        {
            Tags {"LightMode" = "ShadowCaster"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            sampler2D _GrabTexture;
            sampler2D _posTex;
            sampler2D _nTex;
            uniform float _boundingMax;
            uniform float _boundingMin;
            uniform float _speed;
            uniform int _numOfFrames;
            half4 _Color;

            float4 _HeightOffset;

            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(float4, _TimeInFrames)
            UNITY_INSTANCING_BUFFER_END(Props)

            struct appdata_bl
            {
                float2 uv : TEXCOORD0;
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                V2F_SHADOW_CASTER;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata_bl v)
            {
                v2f o;

                UNITY_INITIALIZE_OUTPUT(v2f, o);

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                float4 timeInFrames = UNITY_ACCESS_INSTANCED_PROP(Props, _TimeInFrames);

                float frameUv = timeInFrames.x / timeInFrames.w;
                float4 dataUv = float4(v.uv.x, (frameUv + v.uv.y), 0, 0);

                float4 texturePos = tex2Dlod(_posTex, dataUv);


#if !UNITY_COLORSPACE_GAMMA
                texturePos.xyz = LinearToGammaSpace(texturePos.xyz);
#endif

                float expand = _boundingMax - _boundingMin;
                texturePos.xyz *= expand;
                texturePos.xyz += _boundingMin;
                texturePos.x *= -1;
                v.vertex.xyz = texturePos.xzy;
                v.vertex.xyz += _HeightOffset.xyz;

                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
        }

        //end remove light
    }
}
