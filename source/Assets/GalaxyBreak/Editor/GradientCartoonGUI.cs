using UnityEngine;
using System;
using UnityEditor;


public class GradientCartoonGUI : ShaderGUI
{	
	public static void Separator()
	{
		DrawHorzLine ( 1, 1 );
	}

	public static void DrawHorzLine( int height, int padding )
	{

		GUILayout.Space( padding );

		var savedColor = GUI.color;
		GUI.color = new Color( 0.70f, 0.70f, 0.70f );
		GUILayout.Box( "", GUILayout.ExpandWidth( true ), GUILayout.Height( height ) );
		GUI.color = savedColor;

		GUILayout.Space( padding );
	}

	public static void Header(string title){
		GUIStyle style = new GUIStyle();
		style.fontStyle = FontStyle.Bold;
		style.alignment = TextAnchor.UpperLeft;
		EditorGUILayout.LabelField(new GUIContent(title),style);
		GUILayout.Space( 2 );
	}

	public static void HeaderSeparator(string title){
		Separator();
		Header(title);
	}

	private static class Styles{
		public static GUIContent mainTexText = new GUIContent("Main Texture", "Main Texture");
		public static GUIContent layoutTexText = new GUIContent("Layout Texture", "Layout Texture");
		public static GUIContent distanceLightTexText = new GUIContent("Distance Ramp Texture", "Distance Ramp Texture");
	}

	static string USE_MAIN_TEX = "USE_MAIN_TEX";
	static string USE_LAYOUT_TEXTURE = "USE_LAYOUT_TEXTURE";
	static string LIGHTING_FULL = "LIGHTING_FULL";
	static string USE_GRADIENT = "USE_GRADIENT";
	static string GRADIENT_LOCAL_SPACE = "GRADIENT_LOCAL_SPACE";
	static string USE_FOG = "USE_FOG";
	static string USE_DIST_FOG = "USE_DIST_FOG";
	static string USE_LIGHTMAP = "USE_LIGHTMAP";
	static string TRANSPARENT = "TRANSPARENT";
	static string USE_DIST_LIGHT = "USE_DIST_LIGHT";
	static string DIST_LIGHT_ADDITIVE = "DIST_LIGHT_ADDITIVE";
	static string USE_REALTIME_SHADOWS = "USE_REALTIME_SHADOWS";
	static string USE_LIGHT_PROBES = "USE_LIGHT_PROBES";
	static string USE_SPECULAR = "USE_SPECULAR";
	static string USE_SPECULAR_PIXEL_SHADING = "USE_SPECULAR_PIXEL_SHADING";
	static string USE_DIR_LIGHT = "USE_DIR_LIGHT";
	static string FOG_STATIC_START_POS = "FOG_STATIC_START_POS";
		
	Material material;
	MaterialEditor materialEditor;

	MaterialProperty mainTexture = null;
	MaterialProperty mainTexturePower = null;
	MaterialProperty layoutTexture = null;
	MaterialProperty layoutTexturePower = null;

	MaterialProperty topColor = null;
	MaterialProperty topColorBottom = null;
	MaterialProperty frontColor = null;
	MaterialProperty frontColorBottom = null;
	MaterialProperty rightColor = null;
	MaterialProperty rightColorBottom = null;

	MaterialProperty mainColor = null;
	MaterialProperty mainColorBottom = null;

	MaterialProperty topLight = null;
	MaterialProperty frontLight = null;
	MaterialProperty rightLight = null;

	MaterialProperty gradientStartY = null;
	MaterialProperty gradientHeight = null;

	MaterialProperty rimColor = null;
	MaterialProperty rimColorBottom = null;
	MaterialProperty rimPower = null;

	MaterialProperty ambientColor = null;
	MaterialProperty ambientPower = null;

	MaterialProperty tintColor = null;

	MaterialProperty lightmapColor = null;
	MaterialProperty lightmapPower = null;
	MaterialProperty lightmapShadowLight = null;

	MaterialProperty fogColor = null;
	MaterialProperty fogStartY = null;
	MaterialProperty fogHeight = null;

	MaterialProperty fogDistanceStart = null;
	MaterialProperty fogDistanceEnd = null;
	MaterialProperty fogDistanceDensity = null;

	MaterialProperty alpha = null;

	MaterialProperty distanceLightTexture = null;
	MaterialProperty distanceLightDistance = null;
	MaterialProperty distanceLightPosition = null;

	MaterialProperty lightProbePower = null;

	MaterialProperty specularColor = null;
	MaterialProperty specularGloss = null;
	MaterialProperty specularPower = null;


	bool fullLightingMode = false;
	bool gradientMode = false;
	bool gradientLocalMode = false;
	bool lightmapMode = false;
	bool fogMode = false;
	bool distanceFogMode = false;
	bool transparentMode = false;
	bool distanceLightingMode = false;
	bool distanceLightingAdditiveMode =false;
	bool realtimeShadowsMode = false;
	bool lightProbesMode = false;
	bool specularMode = false;
	bool specularPixelMode = false;
	bool useDirLightMode=false;
	bool useFogStaticStartPos = false;

	void initModes(Material m){
		fullLightingMode = m.IsKeywordEnabled (LIGHTING_FULL);
		gradientMode = m.IsKeywordEnabled (USE_GRADIENT);
		gradientLocalMode = m.IsKeywordEnabled (GRADIENT_LOCAL_SPACE);
		lightmapMode = m.IsKeywordEnabled (USE_LIGHTMAP);
		fogMode =  m.IsKeywordEnabled (USE_FOG);
		distanceFogMode = m.IsKeywordEnabled (USE_DIST_FOG);
		transparentMode = m.IsKeywordEnabled (TRANSPARENT);
		distanceLightingMode = m.IsKeywordEnabled (USE_DIST_LIGHT);
		distanceLightingAdditiveMode = m.IsKeywordEnabled (DIST_LIGHT_ADDITIVE);
		realtimeShadowsMode = m.IsKeywordEnabled (USE_REALTIME_SHADOWS);
		lightProbesMode = m.IsKeywordEnabled (USE_LIGHT_PROBES);
		specularMode = m.IsKeywordEnabled (USE_SPECULAR);
		specularPixelMode = m.IsKeywordEnabled (USE_SPECULAR_PIXEL_SHADING);
		useDirLightMode = m.IsKeywordEnabled (USE_DIR_LIGHT);
		useFogStaticStartPos = m.IsKeywordEnabled (FOG_STATIC_START_POS);
	}

	void findProperties (MaterialProperty[] props) {
		mainTexture = FindProperty ("_MainTex", props);
		mainTexturePower = FindProperty ("_MainTexPower", props);
		layoutTexture = FindProperty ("_LayoutTexture", props);
		layoutTexturePower = FindProperty ("_LayoutTexturePower", props);
		distanceLightTexture = FindProperty ("_LightRampTexture", props);

		mainColor =  FindProperty ("_MainColor", props);
		mainColorBottom =  FindProperty ("_MainColorBottom", props);

		topLight =  FindProperty ("_TopLight", props);
		frontLight =  FindProperty ("_FrontLight", props);
		rightLight =  FindProperty ("_RightLight", props);

		topColor =  FindProperty ("_TopColor", props);
		topColorBottom =  FindProperty ("_TopColorBottom", props);
		frontColor =  FindProperty ("_FrontColor", props);
		frontColorBottom =  FindProperty ("_FrontColorBottom", props);
		rightColor =  FindProperty ("_RightColor", props);
		rightColorBottom =  FindProperty ("_RightColorBottom", props);

		gradientStartY  =  FindProperty ("_GradientStartY", props);
		gradientHeight  =  FindProperty ("_GradientHeight", props);

		rimColor =  FindProperty ("_RimColor", props);
		rimColorBottom =  FindProperty ("_RimColorBottom", props);
		rimPower =  FindProperty ("_RimPower", props);

		ambientColor =  FindProperty ("_AmbientColor", props);
		ambientPower =  FindProperty ("_AmbientPower", props);

		tintColor =  FindProperty ("_LightTint", props);

		lightmapColor = FindProperty ("_LightmapColor", props);
		lightmapPower = FindProperty ("_LightmapPower", props);
		lightmapShadowLight = FindProperty ("_ShadowPower", props);

		fogColor = FindProperty ("_FogColor", props);
		fogStartY = FindProperty ("_FogYStartPos", props);
		fogHeight = FindProperty ("_FogHeight", props);

		fogDistanceStart = FindProperty ("_FogStart", props);
		fogDistanceEnd = FindProperty ("_FogEnd", props);
		fogDistanceDensity = FindProperty ("_FogDensity", props);

		distanceLightDistance = FindProperty ("_LightMaxDistance", props);
		distanceLightPosition = FindProperty ("_LightPos", props);

		alpha = FindProperty ("_Alpha", props);

		lightProbePower = FindProperty ("_LightProbePower", props);

		specularColor = FindProperty ("_SpecColorc", props);
		specularGloss = FindProperty ("_Shininess", props);
		specularPower = FindProperty ("_Specular", props);

	}

	public override void OnGUI (MaterialEditor me, MaterialProperty[] props) {
		materialEditor = me;
		material = materialEditor.target as Material;

		initModes (material);
		findProperties (props);

		Header ("Textures");

		float oldWidth = EditorGUIUtility.labelWidth;
		//EditorGUIUtility.labelWidth = Screen.width - 200f;

		materialEditor.TexturePropertySingleLine(Styles.mainTexText, mainTexture);
		if (mainTexture.textureValue != null) {
			EditorGUI.indentLevel++;
			addProperty (mainTexturePower,"Main Texture Power");
			EditorGUI.indentLevel--;
			material.EnableKeyword (USE_MAIN_TEX);
		}
		else {
			material.DisableKeyword (USE_MAIN_TEX);
		}

		materialEditor.TexturePropertySingleLine(Styles.layoutTexText, layoutTexture);
		if (layoutTexture.textureValue != null) {
			
			material.EnableKeyword (USE_LAYOUT_TEXTURE);
			EditorGUI.indentLevel++;
			addProperty (layoutTexturePower, "Layout Texture Power");
			EditorGUI.indentLevel--;
		}
		else {
			material.DisableKeyword (USE_LAYOUT_TEXTURE);
		}

		if (distanceLightingMode) {
			materialEditor.TexturePropertySingleLine(Styles.distanceLightTexText, distanceLightTexture);
		}
		EditorGUIUtility.labelWidth = oldWidth;
		HeaderSeparator ("Lighting");

		Rect r = Toggle (LIGHTING_FULL, 0, 0, ref fullLightingMode, 70,"Full ");
		r.x = 80;

		r.x = 90;
		gradientMode = GUI.Toggle (r, gradientMode, "Gradient");
		r.width = 120;
		if (gradientMode) {
			r.x += 12;
			r.y += 18;
			GUILayout.Space(15);
			gradientLocalMode = GUI.Toggle (r, gradientLocalMode, "Local (UV3)");
			if (gradientLocalMode) {
				material.EnableKeyword (GRADIENT_LOCAL_SPACE);
			}
			else {
				material.DisableKeyword (GRADIENT_LOCAL_SPACE);
			}
			r.x -= 12;
			r.y -= 18;
		}
		r.x = 200;
		distanceLightingMode = GUI.Toggle (r, distanceLightingMode, "Distance Light");

		if (distanceLightingMode) {
			r.x += 12;
			r.y += 18;
			material.EnableKeyword (USE_DIST_LIGHT);
			GUILayout.Space(15);
			distanceLightingAdditiveMode = GUI.Toggle (r, distanceLightingAdditiveMode, "Additive Blending");
			if (distanceLightingAdditiveMode) {
				material.EnableKeyword (DIST_LIGHT_ADDITIVE);
			}
			else {
				material.DisableKeyword (DIST_LIGHT_ADDITIVE);
			}

		}
		else {
			material.DisableKeyword (USE_DIST_LIGHT);
		}

		GUILayout.Space(10);

		if (fullLightingMode) {
			material.EnableKeyword (LIGHTING_FULL);
			addProperty(topColor, "Top Color");

			if (gradientMode) {
				addBottomColor(topColorBottom, "Top Color Bottom");
			}
			addProperty (frontColor, "Front Color");
			if (gradientMode) {
				addBottomColor(frontColorBottom, "Front Color Bottom");
			}
			addProperty(rightColor, "Right Color");
			if (gradientMode) {
				addBottomColor(rightColorBottom, "Right Color Bottom");
			}
		}
		else {
			material.DisableKeyword (LIGHTING_FULL);
			addProperty (mainColor, "Main Color");
			if (gradientMode) {
				addBottomColor(mainColorBottom, "Main Color Bottom");
			}

			addProperty (topLight, "Top Light");
			addProperty (frontLight, "Front Light");
			addProperty (rightLight, "Right Light");
		}
		GUILayout.Space(10);

		// RIM Color
		addProperty(rimColor, "Rim Color");

		if (gradientMode) {
			addBottomColor(rimColorBottom, "Rim Color Bottom");
		}	

		addProperty (rimPower, "Rim Power");

		GUILayout.Space(10);
		Toggle (USE_DIR_LIGHT, 0, 0, ref useDirLightMode, 120, "Directional Light");
		GUILayout.Space(10);

		// Ambient
		addProperty(ambientColor, "Ambient Color");
		addProperty (ambientPower, "Ambient Power");
		GUILayout.Space(5);
		addProperty(tintColor, "Tint Color");

		GUILayout.Space(10);

		if (gradientMode) {
			addProperty (gradientStartY, "Gradient Y Start Pos");
			addProperty (gradientHeight, "Gradient Height");
			material.EnableKeyword (USE_GRADIENT);
		}
		else {
			material.DisableKeyword (USE_GRADIENT);
		}

		GUILayout.Space(10);
		if (distanceLightingMode) {
			addProperty (distanceLightDistance, "Distance Light Max Distance");
			addProperty (distanceLightPosition, "Distance Light Position");
		}
			
		ToggleHeader ("Fog",USE_FOG, 0, 0, ref fogMode);
		if (fogMode) {
			Toggle (USE_DIST_FOG, 0, 0, ref distanceFogMode, 100, "Distance fog");


			if (distanceFogMode) {
				Toggle (FOG_STATIC_START_POS, 20, 0, ref useFogStaticStartPos, 100, "Static Pos");
				GUILayout.Space(5);
				EditorGUI.indentLevel++;
				addProperty (fogDistanceStart, "Start pos");
				addProperty (fogDistanceEnd, "End pos");
				addProperty (fogDistanceDensity, "Density");
				EditorGUI.indentLevel--;
			}
			addProperty (fogColor, "Fog Color");
			addProperty (fogStartY, "Fog Y-Start");
			addProperty (fogHeight, "Fog Height");
		}

		// Lightmap
		ToggleHeader ("Lightmap",USE_LIGHTMAP, 0, 0, ref lightmapMode);

		if (lightmapMode) {
			addProperty (lightmapColor, "Color");
			addProperty (lightmapPower, "Power");
			addProperty (lightmapShadowLight, "Shadow Light");
		}

		HeaderSeparator ("Misc");

		Toggle (TRANSPARENT, 0, 0, ref transparentMode, 120, "Transparent");
		enableTransparent (transparentMode);

		Toggle (USE_REALTIME_SHADOWS, 0, 0, ref realtimeShadowsMode, 120, "Realtime Shadows");

		Toggle (USE_LIGHT_PROBES, 0, 0, ref lightProbesMode, 120, "Light Probes");
		if (lightProbesMode) {
			EditorGUI.indentLevel++;
			addProperty (lightProbePower, "Power");
			EditorGUI.indentLevel--;
		}

		Toggle (USE_SPECULAR, 0, 0, ref specularMode, 120, "Specular");
		if (specularMode) {
			EditorGUI.indentLevel++;
			Toggle (USE_SPECULAR_PIXEL_SHADING, 15, 0, ref specularPixelMode, 120, "Pixel shading");

			addProperty (specularColor,"Color");
			addProperty (specularGloss,"Gloss");
			addProperty (specularPower,"Power");
			EditorGUI.indentLevel--;
		}
	}

	void addProperty(MaterialProperty property, string title, int labelWidth=150){
		GUILayout.BeginHorizontal ();
		float lw = EditorGUIUtility.labelWidth;
		EditorGUIUtility.labelWidth = labelWidth;//Screen.width - labelWidth;

		switch (property.type) {
		case MaterialProperty.PropType.Color:
			materialEditor.ColorProperty (property, title);
			break;
		case MaterialProperty.PropType.Float:
			materialEditor.FloatProperty (property, title);
			break;
		case MaterialProperty.PropType.Vector:
			materialEditor.VectorProperty(property, title);
			break;
		case MaterialProperty.PropType.Range:
			materialEditor.RangeProperty(property, title);
			break;
		}

		EditorGUIUtility.labelWidth = lw;
		GUILayout.EndHorizontal ();
	}

	void enableTransparent(bool enable){
		if (enable) {
			EditorGUI.indentLevel++;
			addProperty (alpha, "Alpha");
			EditorGUI.indentLevel--;
			//SrcAlpha OneMinusSrcAlpha
			material.SetOverrideTag ("RenderType", "Transparent");
			material.SetOverrideTag ("Queue", "Transparent");
			material.SetInt ("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			material.SetInt ("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			material.SetInt ("_ZWrite", 0);

			material.renderQueue = 3000;
		}
		else {
			material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
			material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
			material.SetInt("_ZWrite", 1);
		}
	}

	public override void AssignNewShaderToMaterial (Material material, Shader oldShader, Shader newShader) {
		base.AssignNewShaderToMaterial(material, oldShader, newShader);
	}

	public void addBottomColor(MaterialProperty prop,string title){
		float oldWidth = EditorGUIUtility.labelWidth;
		GUILayout.BeginHorizontal ();
		EditorGUIUtility.labelWidth = 160;//Screen.width - 140f;
		EditorGUI.indentLevel++;
		materialEditor.ColorProperty(prop, title);
		EditorGUI.indentLevel--;
		EditorGUIUtility.labelWidth = oldWidth;
		GUILayout.EndHorizontal ();	
	}

	public Rect ToggleHeader(string headerTitle,string shader_feature, int xPos, int yPos, ref bool result, int width=-1,string title = "Enable")
	{
		Separator();
		GUILayout.BeginHorizontal ();	

		GUIStyle style = new GUIStyle();
		style.alignment = TextAnchor.UpperLeft;
		style.fontStyle = FontStyle.Bold;
		EditorGUILayout.LabelField(new GUIContent(headerTitle),style);

		Rect r =  GUILayoutUtility.GetRect(70f, 16f, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		r.x += xPos;

		if (width > 0) {
			r.width = width;
		}

		result = GUI.Toggle (r, result, title);
		if (result) {
			material.EnableKeyword (shader_feature);
		}
		else {
			material.DisableKeyword (shader_feature);
		}



		GUILayout.EndHorizontal ();	
		return r;
	}

	public Rect Toggle(string shader_feature, int xPos, int yPos, ref bool result, int width=-1,string title = "Enable")
	{
		Rect r =  GUILayoutUtility.GetRect(60f, 16f, GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
		r.x += xPos;
		r.y += yPos;
		if (width > 0) {
			r.width = width;
		}
		result = GUI.Toggle (r, result, title);
		if (result) {
			material.EnableKeyword (shader_feature);
		}
		else {
			material.DisableKeyword (shader_feature);
		}

		return r;
	}
}
