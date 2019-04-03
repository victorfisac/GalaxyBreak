// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

#ifndef MARVELOUS_INCLUDED
#define MARVELOUS_INCLUDED

#include "UnityCG.cginc"
#include "AutoLight.cginc"

#if !defined (SHADOWS_SCREEN) && !defined (SHADOWS_DEPTH) && !defined (SHADOWS_CUBE) || !defined (USE_REALTIME_SHADOWS)
#define COPY_SHADOW_COORDS(o2,o)
#else
#define COPY_SHADOW_COORDS(o2,o) o2._ShadowCoord=o._ShadowCoord;
#endif

#if USE_DIR_LIGHT
uniform half3 _LightDirF;
uniform half3 _LightDirT;
uniform half3 _LightDirR;
#endif

#ifdef USE_MAIN_TEX
sampler2D _MainTex;
float4 _MainTex_ST;
#endif

#ifdef USE_LAYOUT_TEXTURE
sampler2D _LayoutTexture;
float4 _LayoutTexture_ST;
#endif

#ifdef USE_DIST_LIGHT
sampler2D _LightRampTexture;
float4 _LightRampTexture_ST;
#endif


struct CL_IN{
	half4 vertex : POSITION;
	half3 normal : NORMAL;
	half3 color : COLOR;
#ifdef USE_MAIN_TEX
	half4 texcoord : TEXCOORD0;
#endif
#ifdef USE_LIGHTMAP
	half4 texcoord1 : TEXCOORD1;
#endif
#ifdef GRADIENT_LOCAL_SPACE
	half2 local_height : TEXCOORD2; // local height info
#endif
#ifdef USE_LAYOUT_TEXTURE
	half4 texcoord3 : TEXCOORD3;
#endif

};

struct CL_OUT_WPOS {
	half4 pos : SV_POSITION;
#ifdef USE_MAIN_TEX
	#ifdef USE_LIGHTMAP
		half4 uv : TEXCOORD0;
	#else
		half2 uv : TEXCOORD0;
	#endif
#else
	#ifdef USE_LIGHTMAP
		half2 uv : TEXCOORD0;
	#endif
#endif	
	
	half3 lighting : TEXCOORD2;
#if defined(USE_FOG) || defined(USE_DIST_LIGHT) || defined(USE_GRADIENT) || (defined(USE_SPECULAR) && defined(USE_SPECULAR_PIXEL_SHADING))
	half4 wpos: TEXCOORD3; 
#endif
#ifdef USE_LAYOUT_TEXTURE
	half2 layouttexture_uv : TEXCOORD4;
#endif	

#if (defined(USE_SPECULAR) && defined(USE_SPECULAR_PIXEL_SHADING))
	half3 normal : TEXCOORD5;
#endif

#ifdef USE_REALTIME_SHADOWS
	 SHADOW_COORDS(6)
#endif

#ifdef USE_FOG
	half4 color : TEXCOORD7;
#else
	half3 color : TEXCOORD7;
#endif
	
};

inline half3 calculateFinalLighting(half ypos,half3 f_color, half3 r_color, half3 t_color, half f_d, half r_d, half t_d,half3 rimColor,half rim,half rimPower){

#ifdef USE_GRADIENT
	half gradient = saturate((ypos - _GradientStartY) / _GradientHeight);
	f_color = lerp(_FrontColorBottom,f_color,gradient);
	r_color = lerp(_RightColorBottom,r_color,gradient);
	t_color = lerp(_TopColorBottom,t_color,gradient);
	rimColor  = lerp(_RimColorBottom,rimColor,gradient);
#endif

	return (f_color*f_d) + (r_color*r_d) + (t_color*t_d)+(rimColor*rim*rimPower);
}

#ifdef USE_SPECULAR
inline half3 specularLight(half3 viewDirection, half3 normalDirection,half3 lightDirection ){
	half3 halfdir = normalize (lightDirection + viewDirection);
	float nh = max (0.0, dot (normalDirection, halfdir));

	return pow (nh, 16/_Specular)*_Shininess*_SpecColorc;
}
#endif

CL_OUT_WPOS calculateLighting(CL_IN v,half3 rimColor,half rimPower,half3 f_color,half3 r_color,half3 t_color){
	
	CL_OUT_WPOS o;
	o.pos = UnityObjectToClipPos (v.vertex);
	half4 wpos = mul( unity_ObjectToWorld, half4(v.vertex.xyz,1) );
#if defined(USE_FOG) || defined(USE_DIST_LIGHT) || defined(USE_GRADIENT) || (defined(USE_SPECULAR)&& defined(USE_SPECULAR_PIXEL_SHADING))
	o.wpos = wpos;
#endif
	half3 normal =  normalize(mul(unity_ObjectToWorld,half4(v.normal,0))).xyz;
#if (defined(USE_SPECULAR) && defined(USE_SPECULAR_PIXEL_SHADING))
	o.normal = normal;
#endif
	o.color.rgb=v.color.rgb;

#ifdef USE_FOG
	o.color.a =1;	
#endif

#if USE_DIR_LIGHT
	half f_d = acos((dot(_LightDirF,normal)))/1.5708;
	half r_d = acos((dot(_LightDirR,normal)))/1.5708;
	half t_d = acos((dot(_LightDirT,normal)))/1.5708;
	
	f_d = lerp(0,f_d -1 ,f_d >1);
	r_d = lerp(0,r_d -1,r_d >1);
	t_d = lerp(0,t_d -1,t_d >1);
	
#else
	half f_d = acos(clamp(dot(half3(0,0,-1),half3(0,0,normal.z)),-1,1))/1.5708;
	half r_d = acos(clamp(dot(half3(1,0,0),half3(normal.x,0,0)),-1,1))/1.5708;
	half t_d = acos(clamp(dot(half3(0,1,0),half3(0,normal.y,0)),-1,1))/1.5708;
	
	f_d = lerp(0,1-f_d,half(normal.z<0));
	r_d = lerp(0,1-r_d,half(normal.x>0));
	t_d = lerp(0,1-t_d,half(normal.y>0));
	
#endif

	
#ifdef USE_LAYOUT_TEXTURE	
	o.layouttexture_uv = TRANSFORM_TEX (v.texcoord3, _LayoutTexture);
#endif
	
#ifdef USE_MAIN_TEX
	o.uv.xy = TRANSFORM_TEX (v.texcoord, _MainTex);
	#ifdef USE_LIGHTMAP
		o.uv.zw = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	#endif
#else
	#ifdef USE_LIGHTMAP
		o.uv.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
	#endif
#endif

	half rim = 1-(f_d+r_d+t_d);
	half lpos = wpos.y;
#ifdef GRADIENT_LOCAL_SPACE
	lpos = v.local_height.y;
#endif
	
	o.lighting = calculateFinalLighting(lpos, f_color,r_color,t_color,f_d,r_d,t_d,rimColor,rim,rimPower);

#ifdef USE_LIGHT_PROBES
	o.lighting *= lerp(half3(1,1,1),ShadeSH9 (half4 (normal, 1.0)), _LightProbePower);
#endif

#if defined(USE_SPECULAR) && !defined(USE_SPECULAR_PIXEL_SHADING)
	fixed3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
  	fixed3 viewDirection = normalize(UnityWorldSpaceViewDir(wpos));
  	o.lighting += specularLight(viewDirection, normal, lightDirection );
#endif

#ifdef USE_REALTIME_SHADOWS
	TRANSFER_SHADOW(o);
#endif
	
	return o;
}

CL_OUT_WPOS customLightingWPosVert(CL_IN v, half3 rimColor, half rimPower, half3 r_color, half3 f_color, half3 t_color, half ambientColor, float ambientPower){
			
	CL_OUT_WPOS o = calculateLighting(v,rimColor, rimPower,f_color,r_color,t_color);
	o.lighting += (ambientColor*ambientPower);
	
	return o;
}

CL_OUT_WPOS customLightingWPosVertSimple(CL_IN v, half3 mainColor,half3 rimColor, half rimPower,half rightLight, half frontLight, half topLight, half3 ambientColor, float ambientPower){
	
	CL_OUT_WPOS o=calculateLighting(v,rimColor, rimPower,frontLight,rightLight,topLight);
#ifdef USE_GRADIENT
	half lpos = o.wpos.y;
	#ifdef GRADIENT_LOCAL_SPACE
		lpos = v.local_height.y;
	#endif
	half gradient = saturate((lpos - _GradientStartY) / _GradientHeight);
	o.lighting = lerp(_MainColorBottom,mainColor,half3(gradient,gradient,gradient))*o.lighting+(ambientColor*ambientPower);
#else
	o.lighting = mainColor*o.lighting+(ambientColor*ambientPower);
#endif
	return o;
}

#ifdef USE_FOG
CL_OUT_WPOS customLightingSimpleSoftFogVert(CL_IN v,half3 mainColor, half3 rimColor, half rimPower, half3 rightLight, half3 frontLight, half3 topLight, half ambientColor,
	half ambientPower,half fogStartY, half animationHeight, half fogAnimationFreq){
			
		CL_OUT_WPOS o=customLightingWPosVertSimple(v,mainColor, rimColor, rimPower, rightLight, frontLight, topLight, ambientColor, ambientPower);
		_FogYStartPos+=((sin( _Time * 10 * fogAnimationFreq))+1)*0.5* animationHeight;
		return o;
}

CL_OUT_WPOS customLightingSoftFogVert(CL_IN v, half3 rimColor, half rimPower, half3 rightColor, half3 frontColor, half3 topColor, half ambientColor,
	half ambientPower,half fogStartY, half animationHeight, half fogAnimationFreq){
			
		CL_OUT_WPOS o=customLightingWPosVert(v, rimColor, rimPower,rightColor, frontColor, topColor, ambientColor, ambientPower);
		_FogYStartPos+=((sin( _Time * 10 * fogAnimationFreq))+1)*0.5* animationHeight;
		return o;
}
#endif

fixed shadowAttenuation(CL_OUT_WPOS v){
	#ifdef USE_REALTIME_SHADOWS
	return SHADOW_ATTENUATION(v);
	#else
	return 1;
	#endif
}

#ifdef USE_DIST_LIGHT
fixed4 calculteDistanceLight(fixed4 currentLight,CL_OUT_WPOS v){
	half l =length(_LightPos - v.wpos);
 	l = clamp(min(l,_LightMaxDistance)/_LightMaxDistance,0,1);

 	half f = 1-l;
 	half4 c1 =tex2D (_LightRampTexture, half2(0,f));

 #ifdef DIST_LIGHT_ADDITIVE
 	return currentLight*0.5 +c1*0.5;
 #else
 	return currentLight*c1;
 #endif
}
#endif

fixed4 customLightingFrag(CL_OUT_WPOS v, half3 lightTint, half useLightmap, half lightmapPower, half3 lightmapColor,half _ShadowPower){
	fixed4 outColor = fixed4(0.0, 0.0, 0.0, 1.0);

#ifdef USE_MAIN_TEX
	half4 textureColor = tex2D (_MainTex, v.uv.xy);
	#ifdef MASTER_SHADER
		textureColor.xyz = lerp(half3(1,1,1),textureColor.xyz,_MainTexPower);
	#endif
	outColor.w=textureColor.w;
#else
	half4 textureColor = half4(1,1,1,1);
#endif

#ifdef USE_LAYOUT_TEXTURE
		half4 cMap = tex2D (_LayoutTexture, v.layouttexture_uv);
		textureColor.xyz = ((lightTint * v.lighting) * textureColor.xyz)*lerp(half3(1,1,1),cMap,_LayoutTexturePower);
#else
		textureColor.xyz = ((lightTint * v.lighting) * textureColor.xyz);
#endif
	
#if USE_LIGHTMAP
		half3 realLightmapPower;

		#ifdef USE_MAIN_TEX
			half2 lm_uv =  v.uv.zw;
		#else
			half2 lm_uv =  v.uv.xy;
		#endif

		fixed4 bakedColorTex  = UNITY_SAMPLE_TEX2D(unity_Lightmap, lm_uv)*shadowAttenuation(v);
		realLightmapPower = lerp(half3(1,1,1),clamp(DecodeLightmap(bakedColorTex)+(_ShadowPower),0,1),lightmapPower);
		outColor.xyz = lerp (lightmapColor.xyz,half3(1,1,1), realLightmapPower)*textureColor.xyz;
#else
		outColor = textureColor*shadowAttenuation(v);
#endif

	outColor.xyz*=v.color;

#ifdef USE_DIST_LIGHT
  	outColor = calculteDistanceLight(outColor, v);
#endif

#if defined(USE_SPECULAR) && defined(USE_SPECULAR_PIXEL_SHADING)
	fixed3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
  	fixed3 viewDirection = normalize(UnityWorldSpaceViewDir(v.wpos));
  	outColor.xyz += specularLight(viewDirection, normalize(v.normal), lightDirection );
#endif

	return outColor;
}

#ifdef USE_FOG
fixed4 customLightingHardFogFrag(CL_OUT_WPOS v,float fogYStartPos, half3 fogColor, half3 lightTint, half useLightmap, half lightmapPower, half3 lightmapColor,half _ShadowPower){

	fixed4 outColor = customLightingFrag(v, lightTint, useLightmap, lightmapPower, lightmapColor, _ShadowPower);
	return  fixed4(lerp( fogColor, outColor.xyz, v.wpos.y > fogYStartPos),0);
}

fixed4 customLightingSoftFogFrag(CL_OUT_WPOS v, half3 fogColor, half fogHeight, half3 lightTint, half useLightmap, half lightmapPower, half3 lightmapColor,half _ShadowPower){
	fixed4 outColor = customLightingFrag(v, lightTint, useLightmap, lightmapPower, lightmapColor, _ShadowPower);
	half fogDensity = clamp((v.wpos.y - _FogYStartPos)/fogHeight,0,1);
  	outColor = fixed4(lerp ( fogColor, outColor.xyz, fogDensity),outColor.w);
  //#ifdef USE_DIST_LIGHT
  //	outColor = calculteDistanceLight(outColor, v);
  //#endif
  	return outColor;
}
#endif

/*
*
* Blend mode script by Aubergine
* http://forum.unity3d.com/threads/free-photoshop-blends.121661/
*
*/
fixed3 Darken (fixed3 a, fixed3 b) { return fixed3(min(a.rgb, b.rgb)); }
fixed3 Multiply (fixed3 a, fixed3 b) { return (a * b); }
fixed3 ColorBurn (fixed3 a, fixed3 b) { return (1-(1-a)/b); }
fixed3 LinearBurn (fixed3 a, fixed3 b) { return (a+b-1); }
fixed3 Lighten (fixed3 a, fixed3 b) { return fixed3(max(a.rgb, b.rgb)); }
fixed3 Screen (fixed3 a, fixed3 b) { return (1-(1-a)*(1-b)); }
fixed3 ColorDodge (fixed3 a, fixed3 b) { return (a/(1-b)); }
fixed3 LinearDodge (fixed3 a, fixed3 b) { return (a+b); }
fixed3 Overlay (fixed3 a, fixed3 b) {
    fixed3 r = fixed3(0,0,0);
    if (a.r > 0.5) { r.r = 1-(1-2*(a.r-0.5))*(1-b.r); }
    else { r.r = (2*a.r)*b.r; }
    if (a.g > 0.5) { r.g = 1-(1-2*(a.g-0.5))*(1-b.g); }
    else { r.g = (2*a.g)*b.g; }
    if (a.b > 0.5) { r.b = 1-(1-2*(a.b-0.5))*(1-b.b); }
    else { r.b = (2*a.b)*b.b; }
    return r;
}
fixed3 SoftLight (fixed3 a, fixed3 b) {
    fixed3 r = fixed3(0,0,0);
    if (b.r > 0.5) { r.r = a.r*(1-(1-a.r)*(1-2*(b.r))); }
    else { r.r = 1-(1-a.r)*(1-(a.r*(2*b.r))); }
    if (b.g > 0.5) { r.g = a.g*(1-(1-a.g)*(1-2*(b.g))); }
    else { r.g = 1-(1-a.g)*(1-(a.g*(2*b.g))); }
    if (b.b > 0.5) { r.b = a.b*(1-(1-a.b)*(1-2*(b.b))); }
    else { r.b = 1-(1-a.b)*(1-(a.b*(2*b.b))); }
    return r;
}
fixed3 HardLight (fixed3 a, fixed3 b) {
    fixed3 r = fixed3(0,0,0);
    if (b.r > 0.5) { r.r = 1-(1-a.r)*(1-2*(b.r)); }
    else { r.r = a.r*(2*b.r); }
    if (b.g > 0.5) { r.g = 1-(1-a.g)*(1-2*(b.g)); }
    else { r.g = a.g*(2*b.g); }
    if (b.b > 0.5) { r.b = 1-(1-a.b)*(1-2*(b.b)); }
    else { r.b = a.b*(2*b.b); }
    return r;
}
fixed3 VividLight (fixed3 a, fixed3 b) {
    fixed3 r = fixed3(0,0,0);
    if (b.r > 0.5) { r.r = 1-(1-a.r)/(2*(b.r-0.5)); }
    else { r.r = a.r/(1-2*b.r); }
    if (b.g > 0.5) { r.g = 1-(1-a.g)/(2*(b.g-0.5)); }
    else { r.g = a.g/(1-2*b.g); }
    if (b.b > 0.5) { r.b = 1-(1-a.b)/(2*(b.b-0.5)); }
    else { r.b = a.b/(1-2*b.b); }
    return r;
}
fixed3 LinearLight (fixed3 a, fixed3 b) {
    fixed3 r = fixed3(0,0,0);
    if (b.r > 0.5) { r.r = a.r+2*(b.r-0.5); }
    else { r.r = a.r+2*b.r-1; }
    if (b.g > 0.5) { r.g = a.g+2*(b.g-0.5); }
    else { r.g = a.g+2*b.g-1; }
    if (b.b > 0.5) { r.b = a.b+2*(b.b-0.5); }
    else { r.b = a.b+2*b.b-1; }
    return r;
}
fixed3 PinLight (fixed3 a, fixed3 b) {
    fixed3 r = fixed3(0,0,0);
    if (b.r > 0.5) { r.r = max(a.r, 2*(b.r-0.5)); }
    else { r.r = min(a.r, 2*b.r); }
    if (b.g > 0.5) { r.g = max(a.g, 2*(b.g-0.5)); }
    else { r.g = min(a.g, 2*b.g); }
    if (b.b > 0.5) { r.b = max(a.b, 2*(b.b-0.5)); }
    else { r.b = min(a.b, 2*b.b); }
    return r;
}
fixed3 Difference (fixed3 a, fixed3 b) { return (abs(a-b)); }
fixed3 Exclusion (fixed3 a, fixed3 b) { return (0.5-2*(a-0.5)*(b-0.5)); }
 
#endif