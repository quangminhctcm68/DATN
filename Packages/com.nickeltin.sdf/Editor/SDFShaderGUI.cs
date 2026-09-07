using System;
using nickeltin.InternalBridge.Editor;
using UnityEditor;
using UnityEngine;


namespace nickeltin.SDF.Editor
{
	internal class SDFShaderGUI : ShaderGUI
	{
		private readonly struct ExpandableGroup
		{
			private readonly _SavedBool _isExpanded;
			private readonly string _title;

			public ExpandableGroup(string title)
			{
				_isExpanded = new _SavedBool($"{nameof(SDFShaderGUI)}.{title}", false); 
				_title = title;
			}
			
			public void Draw(Action drawer)
			{
				GUILayout.Space(10);
				_isExpanded.Value = EditorGUILayout.BeginFoldoutHeaderGroup(_isExpanded.Value, _title, Defaults.Title);
				if (_isExpanded)
				{
					EditorGUILayout.BeginVertical(Defaults.BG);
					drawer();
					EditorGUILayout.EndVertical();
				}
				EditorGUILayout.EndFoldoutHeaderGroup();
			}
		}
		
		private static class Defaults
		{
			public static readonly GUIStyle BG = "ShurikenModuleBg";
			public static readonly GUIStyle Title = "ShurikenModuleTitle";
			
			public static readonly ExpandableGroup Outline = new ExpandableGroup("Outline");
			public static readonly ExpandableGroup Shadow = new ExpandableGroup("Shadow");
			public static readonly ExpandableGroup Other = new ExpandableGroup("Other");

			public static readonly GUIContent MainColor = new GUIContent("Main Color",
				"Color of Outline layer of SDFImage, it can be appended with Second layer");
			
			public static readonly GUIContent SecondLayer = new GUIContent("Second Layer",
				"Second Layer will append additional layer to Main expanding from its edge.");

			public static readonly GUIContent SecondLayerColor = new GUIContent("Color", 
				"Second Layer color");
			
			public static readonly GUIContent SecondLayerSoftness = new GUIContent("Softness", 
				"Second Layer softness, how much its edges is blurred, Not practical for pixel art");
			
			public static readonly GUIContent SecondLayerWidth = new GUIContent("Width", 
				"Second Layer width, how much it extends from first layer edges");
			
			public static readonly GUIContent ShadowColor= new GUIContent("Color", 
				"Shadow color");
			
			public static readonly GUIContent ShadowSoftness = new GUIContent("Softness", 
				"Shadow softness, how much its edges is blurred. Not practical for pixel art");
			
			public static readonly GUIContent DistanceSoftness = new GUIContent("Distance Softness", 
				"How much sdf effect is blurred along edges, this is basically Anti-aliasing. " +
				"Effect noticeable at distance. For pixel art use 0");
			
			public static readonly GUIContent UseAlphaClip = new GUIContent("Use UI Alpha Clip", 
				"Built-in Unity property, not sure why it needed.");
			
		}
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
		{
			MaterialProperty FP(string id) => FindProperty(id, properties);


			Defaults.Outline.Draw(() =>
			{
				var _MainColor = FP("_MainColor");
				var _EnableOutline = FP("_EnableOutline");
				var _OutlineColor = FP("_OutlineColor");
				var _OutlineSoftness = FP("_OutlineSoftness");
				var _OutlineWidth = FP("_OutlineWidth");

				materialEditor.ShaderProperty(_MainColor, Defaults.MainColor);
				materialEditor.ShaderProperty(_EnableOutline, Defaults.SecondLayer);

				using (new EditorGUI.IndentLevelScope())
				using (new EditorGUI.DisabledScope(_EnableOutline.floatValue < 1))
				{
					materialEditor.ShaderProperty(_OutlineColor, Defaults.SecondLayerColor);
					materialEditor.ShaderProperty(_OutlineSoftness, Defaults.SecondLayerSoftness);
					materialEditor.ShaderProperty(_OutlineWidth, Defaults.SecondLayerWidth);
				}
			});
			
			Defaults.Shadow.Draw(() =>
			{
				var _ShadowColor = FP("_ShadowColor");
				var _ShadowSoftness = FP("_ShadowSoftness");
				materialEditor.ShaderProperty(_ShadowColor, Defaults.ShadowColor);
				materialEditor.ShaderProperty(_ShadowSoftness, Defaults.ShadowSoftness);
			});
			
			Defaults.Other.Draw(() =>
			{
				var _DistanceSoftness = FP("_DistanceSoftness");
				var _UseUIAlphaClip = FP("_UseUIAlphaClip");
				materialEditor.ShaderProperty(_DistanceSoftness, Defaults.DistanceSoftness);
				materialEditor.ShaderProperty(_UseUIAlphaClip, Defaults.UseAlphaClip);
			});
		}
	}
}



