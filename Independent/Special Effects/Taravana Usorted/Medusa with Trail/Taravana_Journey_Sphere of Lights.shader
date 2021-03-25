Shader "Taravana/Journey/Sphere of Medusa"
{
	Properties{
		_MainTex("Albedo (RGB)", 2D) = "white" {}
	}

	SubShader{

		Tags{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
		}

		ColorMask RGB
		Cull Off
		ZWrite Off
		ZTest Off
		Blend One One //MinusSrcAlpha 

		Pass{

			CGPROGRAM

			#include "UnityCG.cginc"
			//#include "Assets/_Modules/Systems/Shaders/VertexDataProcessInclude.cginc"

			#pragma vertex vert
			#pragma fragment frag
         // #pragma multi_compile ___ _TaravanaLD

			struct vertexToFragment {
				float4 pos : 		SV_POSITION;
				float3 worldPos : 	TEXCOORD0;
				float2 texcoord : 	TEXCOORD1;
        float4 screenPos : 	TEXCOORD2;
        float3 normal : TEXCOORD3;
        float3 viewDir: TEXCOORD4;
				half4 color: 		COLOR;
			};

			sampler2D _Global_Water_Particles_Mask;
			uniform float4 _MainTex_ST;
			uniform float4 _MainTex_TexelSize;
			sampler2D _MainTex;
      half _Distance;

			vertexToFragment vert(appdata_full v) {
        vertexToFragment o;


        o.texcoord = v.texcoord;
     
        o.normal.xyz = UnityObjectToWorldNormal(v.normal);

        o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

        v.vertex += mul(unity_WorldToObject, float4(o.normal.xyz, 0) * sin((o.worldPos.y + o.worldPos.x*0.2))*(1.5 - o.texcoord.y) * 0.3 );


        o.pos = UnityObjectToClipPos(v.vertex);

       

        o.screenPos = ComputeScreenPos(o.pos);
       // float2 sp = o.screenPos.xy / o.screenPos.w;

    

        
        o.viewDir.xyz = WorldSpaceViewDir(v.vertex);

				o.color = v.color;

        o.color *= smoothstep(0.3, 0.4, o.texcoord.y);




				return o;
			}

      half4 frag(vertexToFragment o) : COLOR{

        o.viewDir.xyz = normalize(o.viewDir.xyz);

        float dotprod = dot(o.viewDir.xyz, o.normal.xyz);


				float2 screenUV = o.screenPos.xy / o.screenPos.w;

				float2 screenUV_Str = screenUV;// StretchedScreenUV(screenUV);


        float2 grid = o.texcoord.xy * float2(15, 20);

        float2 gridIndex = floor(grid);

         grid = grid - gridIndex;


        float3 gyrPos = float3(gridIndex * 351.345 , _Time.x);

        float gyroid = dot(sin(gyrPos), cos(gyrPos.zxy));

      //  grid.x += gyroid * 0.5;
       // o.texcoord.y += gyroid*0.01;

        //grid = grid % 1;

        float2 off = (o.texcoord.xy - 0.5);

        //half4 col = o.color * tex2D(_MainTex, o.texcoord.xy);

        grid -= 0.5;

        float bg = 0.03 / (length(grid + float2(sin(gyroid*4), cos(gyroid*3.12))*0.2));

        float vertLine = max(0,0.5-abs(grid.x));

        //bg *= max(0, 0.5 - length(off));

     //   float grad = DarkBrightGradient(screenUV, 0);// *value;

        //col.rgb *= (grad) * col.a;

        float4 col = (bg *  bg + vertLine* vertLine)  * dotprod * dotprod * o.color; // * grad

				return col;
			}
			ENDCG
		}
	}
		Fallback "Legacy Shaders/Transparent/VertexLit"
}