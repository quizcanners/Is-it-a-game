
#include "UnityCG.cginc"
#include "UnityLightingCommon.cginc" 
#include "Lighting.cginc"
#include "AutoLight.cginc"

sampler2D _Global_Noise_Lookup;
float4 _Global_Noise_Lookup_TexelSize;

sampler2D _Taravana_UI_BG;
sampler2D _Taravana_UI_BG_Blurred;
half4 _Taravana_UI_BG_ScreenFillAspect;
sampler2D _Taravana_UI_PopUp;
half4 _Taravana_UI_PopUp_ScreenFillAspect;

sampler2D _Global_Screen_Read;
sampler2D _Global_Screen_Effect;

sampler2D _Global_Water_Particles_Mask_L;
sampler2D _Global_Water_Particles_Mask_D;
sampler2D _Global_Water_Caustics_Mask;

sampler2D _Taravana_SpiralMask;
sampler2D _Taravana_GodRays;
sampler2D _Flashlight_Mask;
sampler2D _LightBlick_Mask;
sampler2D _Terror_Mask;

sampler2D _Taravana_Journey_Background;
half4 _Taravana_Horizont_Color;
half4 _Taravana_Water_Color;
half4 _Taravana_Far_Color;

half4 _Taravana_FadedUiColor;

float _Taravana_Time;

half4 _Taravana_FloatingLightPos; //xy - Screen UV; z- size, w - terror
half4 _Taravana_FloatingLightColor;

half4 _Taravana_FloatingLightPos2;
half4 _Taravana_FloatingLightColor2;

half4 _Taravana_WaterAmbient;
float4 _Taravana_Caustics;
half4 _Taravana_Flashlight;
float4 _Taravana_TravelParticles;
half4 _IchtisLightsDynamics;

half4 _Taravana_MousePosition; // XY - UV, Z - Strength, W - Screen dimensions
half4 _Taravana_MousePosition_Prev; // XY - UV, Z - Strength One Directional
half4 _Taravana_ShakeOffset;
half4 _Taravana_GyroOffset;

half4 Taravana_FogDistance; //* 0.05
half _HoloMonochrome;
//static const float Taravana_FogDistanceDiv = 0.05f;
static const half CLICK_POWER_SHARPNESS = 3;

float3 Mix(float3 colA, float3 colB, float t)
{
  return (colA * (1 - t)) + colB * t;
}

half3 AddHologramInternal (half2 screenUV, half3 color, half3 holoColor, half override, half monoPortion)
{
    half colorShift = min(1, monoPortion * 2);
    half4 bg = tex2D(_Global_Screen_Effect, float4(screenUV, 0, 0)); 
    half grey = length(color.rgb);
    half showBg = colorShift * (0.5 + override * 0.5);
     color.rgb =
      (holoColor * (grey) * colorShift 
      + color.rgb * (1-colorShift)) * (1-showBg)
      + bg .rgb * showBg;

	    half2 linesUV = screenUV;
      half downLine = tan(linesUV.y*(8) - _Time.y);
      half upLine = tan(linesUV.y*(3) + _Time.y*1.5);
      half3 movingLine = float3(0.75,0.5,0.75) * saturate(downLine) + half3(0.25,0.5,0.25) * saturate(upLine);

      half2 fromMouse = (screenUV - _Taravana_MousePosition.xy);

      fromMouse.x *= _Taravana_MousePosition.w;

      half pressPower = max(0, (1 - length(fromMouse) * CLICK_POWER_SHARPNESS) * _Taravana_MousePosition.z + monoPortion*0.3);

      half4 noise = tex2Dlod(_Global_Noise_Lookup, half4(linesUV*(0.2
        + sin(_Time.w + pressPower*0.01)*0.15)
        + half2(_SinTime.w, _CosTime.w) * 32, 0, 0));

			 noise.rgb -= 0.5;
       half staticLine = (sin((linesUV.y)*(900)) + 1)*0.5;
       half3 resultColor = color.rgb * (0.8 + noise.rgb * pressPower * 0.4 + staticLine*staticLine*(0.1 + movingLine*0.2 + monoPortion*0.3));

        return resultColor * colorShift + (1-colorShift) * color.rgb;
}

half3 AddHologram (half2 screenUV, half3 color, half3 holoColor, half override)
{
    #if !_IsHologram
       return color.rgb;
    #endif

  return AddHologramInternal( screenUV,  color,  holoColor,  override, _HoloMonochrome);
 }

float2 Rot(float2 uv, float angle) {
  float si = sin(angle);
  float co = cos(angle);
  return float2(co * uv.x - si * uv.y, si * uv.x + co * uv.y);
}

inline float Remap(float a, float b, float x) {
  return saturate((x - a) / (b - a));
}


inline half GodRays(half2 uv, half speed) {

  uv *= 0.5;

  half angle = _Taravana_Time * speed;// +_Taravana_FloatingLightPos.x * 5;

  half si = sin(angle);
  half co = cos(angle);

  half2 rotUV;
	rotUV.x = (co * uv.x) - (si * uv.y);
	rotUV.y = (si * uv.x) + (co * uv.y);

	rotUV += 0.5;

  half rayOne = tex2Dlod(_Taravana_GodRays, float4(rotUV,0,0)).r;


  #if _TaravanaHD
	  angle = -_Taravana_Time*0.51 + _Taravana_FloatingLightPos.y*6;

	  si = sin(angle);
	  co = cos(angle);

	  rotUV.x = (co * uv.x) - (si * uv.y);
	  rotUV.y = (si * uv.x) + (co * uv.y);
	  rotUV += 0.5;

	  return rayOne * 1.5 * tex2Dlod(_Taravana_GodRays, float4(rotUV,0,0)).r;

  #else
    return rayOne;

  #endif
}


inline half TerrorCoefficient(half2 screenUV) {

	//screenUV += _Taravana_ShakeOffset.xy*0.005;

  half unCenter = abs(screenUV.y - 0.5) * 2;

	unCenter *= unCenter;

  half center = 1 - unCenter;

#if _TaravanaLD

  return center;

#endif

	screenUV.x *= _Taravana_MousePosition.w;

  half time = _Taravana_Time * 0.1;
  
	screenUV *= 0.3;

	
  half terror = tex2Dlod(_Terror_Mask, half4(screenUV*1.3 + half2(time*0.3, time*0.2), 0, 0)).r
    * tex2Dlod(_Terror_Mask, half4(screenUV + half2(time*0.1, -time * 0.6), 0, 0)).r;


  #if _TaravanaHD
	  screenUV *= 4;

    half terrorb = tex2Dlod(_Terror_Mask, half4(screenUV*1.3 + half2(-time * 3, time*0.2 + terror * 0.8), 0, 0)).r *
		  (1-tex2Dlod(_Terror_Mask, half4(screenUV + half2(-time * 1.3 - terror * 0.1, -time * 1.2), 0, 0)).r);

    half shaprness = (1 - terror)*unCenter;

	  terrorb = max(0, (terrorb - 0.3 * shaprness))* (1 + shaprness * 3);

	  terror *= (terror + terrorb);
	  return terror * center;
  #else
    return terror * center;

  #endif

}

inline half3 FloatingLightOne(half2 screenUV_Str, half distance) {

  half2 diff = screenUV_Str - _Taravana_FloatingLightPos.xy ;

  half len = length(diff);

	return _Taravana_FloatingLightColor.a  * _Taravana_FloatingLightColor.rgb
		* FloatingLightCombine(distance, len);

}

inline half4 FloatingLightTwo(half3 position)
{
	half size = _Taravana_FloatingLightPos2.w;

  half len = length(_Taravana_FloatingLightPos2.xyz - position);

  half a = size / (len + 0.02);

	return half4(_Taravana_FloatingLightColor2.rgb, min(1, _Taravana_FloatingLightColor2.a * a));
}

inline half2 StretchedScreenUV(half2 screenUV)
{
	screenUV = (screenUV - 0.5);
	screenUV.x *= _Taravana_MousePosition.w;
	return screenUV;
}

inline half3 Flashlight(half2 screenUV_Str, half distance)
{
	screenUV_Str += _IchtisLightsDynamics.xy*distance*4;

  half mask = tex2Dlod(_Flashlight_Mask, half4((screenUV_Str*0.75 + 0.5) - _Taravana_ShakeOffset.xy*0.04 ,0,0));

  half near = (1 - distance);

	return  (mask * saturate( (0.95 - distance * 0.3 - length(screenUV_Str))* (1 + 8 * near))
		* _Taravana_Flashlight.rgb * _Taravana_Flashlight.a * near * near * _IchtisLightsDynamics.z) * 4;
}

inline float3 SampleCaustics(float2 screenUV, float2 uv, float grey, float blur) {

  /*
  float3 p = float3(screenUV * (1+uv.y)
    , _Time.x*0.1) * 20;

  float gyroid = max(0,-dot(sin(p), cos(p.zxy)));

  return _Taravana_Caustics.rgb * _Taravana_Caustics.a * pow(gyroid,4) * 2;*/

#if _TaravanaLD



  return 0;

#endif

  float t = _Taravana_Time * 3;

  float uvy = screenUV.y;

	screenUV.x *= 1.5; // +grey;

	screenUV += uv * 0.5;

  float val = t * 30 + screenUV.x * 3 + screenUV.y * 8;

	//grey = grey * 0.1 + 1;

  float2 offA = screenUV * 1.123;//*grey;
  float2 offB = screenUV*1.456;// *grey;

  float caustic = tex2Dlod(_Global_Water_Caustics_Mask, float4( offA - float2(t*(0.1), t*0.06),0,0)).r *
		tex2Dlod(_Global_Water_Caustics_Mask, float4(offB + float2(t*(0.07), t*0.1), 0, 0)).r;

		return _Taravana_Caustics.rgb * _Taravana_Caustics.a * caustic * 8;

}

inline half PowerFromClick(half2 screenUV) {

  half2 fromMouse = (screenUV - _Taravana_MousePosition.xy);

	fromMouse.x *= _Taravana_MousePosition.w;

	return max(0, (1 - length(fromMouse) * CLICK_POWER_SHARPNESS) * _Taravana_MousePosition.z);
}

inline float DarkBrightGradient(float2 screenUV, float2 off, float pressPower) {

#if  _TaravanaLD
  return 0.6f;
#endif


  float t = _Taravana_Time;

  float2 clickOff = (screenUV - _Taravana_MousePosition).xy;

	screenUV.x *= _Taravana_MousePosition.w;

  float val = t * 15 + screenUV.x * 3 + screenUV.y * 8;



  float2 offA = screenUV * 1.3 + off;
  float2 offB = (screenUV + off) * 1.1;

  float brighter = tex2Dlod(_Global_Water_Particles_Mask_L, float4(offA - float2(t*0.07, 0), 0, 0)).r;

    #if  _TaravanaHD

      float power = max(0, 1 - length(clickOff) * 3);

      float2 clickOff1 = clickOff * power * 3 * _Taravana_MousePosition_Prev.z;

      float portion = saturate((cos(val + pressPower * 8) + 1)*0.5);
      float dePortion = 1 - portion;

      float brighterB = tex2Dlod(_Global_Water_Particles_Mask_L, float4(offB - float2(0, t*0.09), 0, 0)).r;

      float mix = brighter * brighterB;

      float darker = tex2Dlod(_Global_Water_Particles_Mask_D, float4(offA*2.5 - clickOff1 + float2(t*(0.13), 0), 0, 0)).r;

      float darkerB = tex2Dlod(_Global_Water_Particles_Mask_D, float4(offB * 4 - clickOff1 + float2(0, t*0.1), 0,0)).r;

      float dmix = darker * darkerB;

		    darker = darker * portion + darkerB * dePortion + dmix * dmix;

		    brighter = (brighter * portion + brighterB * dePortion) + 
			    mix * mix * mix
			    + 
			    darker + 0.25
			    ;

		    return brighter * (1.5 + pressPower * (1.5 + sin(pressPower * 10 + _Taravana_Time * 100)));

    #else

      return brighter * 2 + 0.2;

    #endif



}

inline float DarkBrightGradient(float2 screenUV, float pressPower) {
	return DarkBrightGradient(screenUV, 0 , pressPower);
}

inline float  DarkBrightGradientJourney(float2 screenUV, float pressPower) {


	#if _Taravana_Use_LightPoint
		return DarkBrightGradient(screenUV, _Taravana_ShakeOffset.xy
	
#if _Taravana_GotSeaFloor
			* 0.001
#else
			* 0.03
#endif
			, pressPower);
	#else
		return 1;
	#endif
}

inline half4 GetSeaFloor(half2 screenUV, half2 screenUV_Str, half godRays, half blickSpots, half noise) {

  half _Distance = saturate(screenUV.y*1.2);

  half deDist = (1 - _Distance);

	screenUV.xy += _Taravana_ShakeOffset.xy * deDist * deDist * 0.06;

  half distCoef = 0.05 + _Distance * 3;

  half4 col = tex2D(_Taravana_Journey_Background, screenUV);

  half horizont = screenUV.y;

  half near = 1 - horizont;

	col.rgb = col.rgb *	(_Taravana_WaterAmbient.rgb * near  + (_Taravana_Horizont_Color.rgb 

	#if _Taravana_Use_Flashlight 
			+ Flashlight(screenUV_Str, _Distance)
	#endif
		)*	( horizont)

		)
	#if _Taravana_Use_LightPoint
			+ FloatingLightOne(screenUV_Str, _Distance) * godRays *	(0.8 + blickSpots * 0.2)
	#endif


		;

	return col;
}

inline half4 SampleTerrorBg(half2 terrorUV, half terrorOffset, half terrorCoef) {

  half off = (terrorOffset)*terrorCoef;

  half horizon = saturate(terrorUV.y + off);

  //half center = smoothstep(0, 1.5, length(terrorUV + half2(0, -0.25) - 0.5));

	half3 col = sqrt(_Taravana_Horizont_Color.rgb*_Taravana_Horizont_Color.rgb*horizon
		+ _Taravana_Water_Color.rgb *_Taravana_Water_Color.rgb * (1 - horizon));

  return half4(col, off);// *center;
}


inline half3 JourneyMixGodrays(half3 sampled, half godRays, half mod, half blicks) {

	return sampled * godRays *	(0.25 + mod * godRays + blicks * 0.2);

}

inline half4 GetTerrorBackground(half2 terrorUV, half2 screenUV_Str, half godRays, half blickSpots) {

	#if _Taravana_UseTerror
  half terrorOffset = TerrorCoefficient(terrorUV);
	#else
  half terrorOffset = 0;
	#endif

  half terrorCoef = _Taravana_FloatingLightPos.w;

  half4 col = SampleTerrorBg(terrorUV, terrorOffset, terrorCoef);

	#if _Taravana_Use_LightPoint

    half catchLight = saturate(1 - abs(terrorOffset - 0.5) * 2)* abs(terrorCoef);

		col.rgb += JourneyMixGodrays(FloatingLightOne(screenUV_Str, 1), godRays, catchLight * catchLight * 2 + 1, blickSpots);
			
 	#endif

	return col;
}

