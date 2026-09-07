using System;
using System.Reflection;
using nickeltin.SDF.Runtime;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace nickeltin.SDF.Editor
{
    internal partial class SDFImageEditor
    {
        internal static class PreviewEngine
        {
            public enum PreviewType
            {
                Sprite,
                SDFSprite
            }

            private delegate void DrawSpriteDelegate(Sprite sprite, Rect drawArea, Color color);


            private static readonly GUIContent _regularSprite;
            private static readonly GUIContent _sdfSprite;


            private static readonly DrawSpriteDelegate _drawSprite;

            private static PreviewType _previewType;

            public static PreviewType CurrentPreviewType
            {
                get => _previewType;
                set
                {
                    if (_previewType == value) return;
                    _previewType = value;
                    SavePrefs();
                }
            }

            static PreviewEngine()
            {
                var drawSpriteUtilityType = typeof(ImageEditor).Assembly.GetType("UnityEditor.UI.SpriteDrawUtility");
                var drawSpriteMethodName = "DrawSprite";
                var drawSpriteMethod = drawSpriteUtilityType.GetMethod(drawSpriteMethodName,
                    BindingFlags.Static | BindingFlags.Public,
                    null, CallingConventions.Any, new Type[] { typeof(Sprite), typeof(Rect), typeof(Color) }, null);
                _drawSprite =
                    (DrawSpriteDelegate)Delegate.CreateDelegate(typeof(DrawSpriteDelegate), drawSpriteMethod!);

                _regularSprite = new GUIContent("Sprite");
                _sdfSprite = new GUIContent("SDF Sprite");

                LoadPrefs();
            }

            private const string PREFS_KEY = "SDFImageEditor.PreviewType";

            private static void LoadPrefs()
            {
                _previewType = (PreviewType)EditorPrefs.GetInt(PREFS_KEY, 0);
            }

            private static void SavePrefs()
            {
                EditorPrefs.SetInt(PREFS_KEY, (int)CurrentPreviewType);
            }

            private static void DrawSprite(Sprite sprite, Rect drawArea, Color color)
            {
                _drawSprite(sprite, drawArea, color);
            }


            /// <summary>
            /// Draws dropdown button with preview selection type
            /// </summary>
            public static void DrawPreviewSelector()
            {
                var content = CurrentPreviewType switch
                {
                    PreviewType.Sprite => _regularSprite,
                    PreviewType.SDFSprite => _sdfSprite,
                    _ => throw new ArgumentOutOfRangeException()
                };

                if (GUILayout.Button(content, EditorStyles.toolbarDropDown))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(_regularSprite, CurrentPreviewType == PreviewType.Sprite,
                        () => { CurrentPreviewType = PreviewType.Sprite; });
                    menu.AddItem(_sdfSprite, CurrentPreviewType == PreviewType.SDFSprite,
                        () => { CurrentPreviewType = PreviewType.SDFSprite; });
                    menu.ShowAsContext();
                }
            }

            public static void DrawPreview(SDFImage image, Rect rect)
            {
                if (image == null) return;

                switch (CurrentPreviewType)
                {
                    case PreviewType.Sprite:
                        var sprite = image.ActiveSprite;
                        if (sprite == null) return;
                        DrawSprite(sprite, rect, Color.white);
                        break;
                    case PreviewType.SDFSprite:
                        var sdfSprite = image.ActiveSDFSprite;
                        if (sdfSprite == null) return;
                        DrawSprite(sdfSprite, rect, Color.white);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public override bool HasPreviewGUI() => true;

        public override void OnPreviewGUI(Rect rect, GUIStyle background)
        {
            PreviewEngine.DrawPreview(target as SDFImage, rect);
        }

        public override void OnPreviewSettings()
        {
            base.OnPreviewSettings();
            PreviewEngine.DrawPreviewSelector();
        }

        public override string GetInfoString()
        {
            var image = target as SDFImage;
            var sprite = image?.ActiveSprite;

            var x = sprite != null ? Mathf.RoundToInt(sprite.rect.width) : 0;
            var y = sprite != null ? Mathf.RoundToInt(sprite.rect.height) : 0;

            return $"Image Size: {x}x{y}";
        }
    }
}