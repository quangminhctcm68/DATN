using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

namespace nickeltin.SDF.Runtime
{
    [DefaultExecutionOrder(-2000)]
    [RequireComponent(typeof(CanvasRenderer)), AddComponentMenu("UI/SDF Image")]
    public sealed partial class SDFImage : SDFGraphic, ISerializationCallbackReceiver, ILayoutElement, ICanvasRaycastFilter
    {
        private void ResetAlphaHitThresholdIfNeeded()
        {
            if (!SpriteSupportsAlphaHitTest() && _alphaHitTestMinimumThreshold > 0)
            {
                Debug.LogWarning("Sprite was changed for one not readable or with Crunch Compression. Resetting the AlphaHitThreshold to 0.", this);
                _alphaHitTestMinimumThreshold = 0;
            }
        }

        private bool SpriteSupportsAlphaHitTest()
        {
            var sp = Sprite;
            return sp != null && sp.texture != null && !GraphicsFormatUtility.IsCrunchFormat(sp.texture.format) 
                   && sp.texture.isReadable;
        }
        
        protected override void Awake()
        {
            base.Awake();
            EnsureFirstLayerRenderer();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            DisposeAllFirstLayerRenderers();
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Ensuring first layer render on enable to prevent its instantiation in graphics rebuild loop
            EnsureFirstLayerRenderer();
            TrackSprite();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (_tracked) UnTrackImage(this);
            
            // If disposing from on disable it introduces bug where FLR is missing when gameObject quickly changes its active state.
            // I believe this is due to unity Object lifecycle where its not destroyed immediately and might be not null even tho it is.
            // DisposeAllFirstLayerRenderers(); 
        }
        

        /// <summary>
        /// TODO: there is still bug with transform access, this is all part of the bigger bug of FLR lifecycle, its unreliable...
        /// </summary>
        protected override void OnRectTransformDimensionsChange()
        {
            // For some reason in editor in prefab mode this might trigger an error even tho called after Awake
            // First layer is created in OnEnable, and this callback can appear before Awake
            if (FLRNoVerify != null)
            {
                EnsureFirstLayerRendererProperties();
            }
            

            base.OnRectTransformDimensionsChange();
        }

        public override void CrossFadeColor(Color targetColor, float duration, bool ignoreTimeScale, bool useAlpha, bool useRGB)
        {
            base.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);
            if (FLR != null)
            {
                FLR.CrossFadeColor(targetColor, duration, ignoreTimeScale, useAlpha, useRGB);
            }
        }

#if UNITY_EDITOR
        internal override IEnumerable<string> GetUsedSpritesPropertyPaths()
        {
            yield return nameof(_sdfSpriteReference);
        }
#endif
    }
}