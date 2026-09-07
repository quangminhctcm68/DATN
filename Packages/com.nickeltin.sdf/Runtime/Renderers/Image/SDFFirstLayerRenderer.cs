using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Internal;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace nickeltin.SDF.Runtime
{
    /// <summary>
    /// Hidden game object that generates for rendering regular part of sdf image.
    /// This component does not saved in editor or build, it is instantiated at runtime just to hold UI mesh.
    /// Use of second CanvasRenderer allows to use single material with two different textures and still keep the batching.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer), typeof(RectTransform)), 
     ExcludeFromDocs, ExcludeFromPreset, ExecuteAlways]
    internal class SDFFirstLayerRenderer : MaskableGraphic
    {
        public SDFImage Owner { get; set; }
        
        
        protected override void UpdateGeometry()
        {
            // Owner might get destroyed before its nested object
            if (Owner == null)
            {
                return;
            }
            
            Owner.VerifyGeometry();
            
            Owner._firstLayerState.FillMesh(workerMesh);
            canvasRenderer.SetMesh(workerMesh);
        }

        protected override void UpdateMaterial()
        {
            if (Owner == null)
            {
                return;
            }
            
            base.UpdateMaterial();

            // check if this sprite has an associated alpha texture (generated when splitting RGBA = RGB + A as two textures without alpha)
            if (Owner.ActiveSprite == null)
            {
                canvasRenderer.SetAlphaTexture(null);
                return;
            }

            var alphaTex = Owner.ActiveSprite.associatedAlphaSplitTexture;

            if (alphaTex != null)
            {
                canvasRenderer.SetAlphaTexture(alphaTex);
            }
        }

        public override Material materialForRendering => Owner.materialForRendering;

        public override Texture mainTexture => Owner.mainTexture;

        public override bool raycastTarget
        {
            get => Owner != null && Owner.raycastTarget;
            set
            {
                if (Owner == null) return;
                Owner.raycastTarget = value;
            }
        }
        

        /// <summary>
        /// Creates an unmanaged instance of renderer inside parent hierarchy stretched to its size.
        /// This instance need to be disposed.
        /// For correct pivot update use <see cref="DrivenRectTransformTracker"/> and in <see cref="UIBehaviour.OnRectTransformDimensionsChange"/>
        /// update instance pivot to match parents.
        /// </summary>
        public static SDFFirstLayerRenderer Create(SDFImage owner)
        {
            GameObject go = null;
#if UNITY_EDITOR
            go = EditorUtility.CreateGameObjectWithHideFlags(owner.name + "_SDFRenderer", 
                HideFlags.HideAndDontSave);
                // HideFlags.NotEditable | HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild);
            // StageUtility.PlaceGameObjectInCurrentStage(go);
#else 
            go = new GameObject(owner.name + "_SDFRenderer");
#endif
            go.transform.SetParent(owner.transform);
            go.transform.SetSiblingIndex(0);
            go.transform.localScale = Vector3.one;
            go.transform.localRotation = Quaternion.identity;
            var inst = go.AddComponent<SDFFirstLayerRenderer>();
            inst.Owner = owner;
            return inst;
        }

        public static void Dispose(SDFFirstLayerRenderer renderer)
        {
            if (renderer == null) return;
            if (Application.isPlaying) Destroy(renderer.gameObject);
            else DestroyImmediate(renderer.gameObject);
        }
        
      
        
#if UNITY_EDITOR
        internal static readonly List<WeakReference<SDFFirstLayerRenderer>> activeRenderers = new List<WeakReference<SDFFirstLayerRenderer>>();
        internal static event Action activeRenderersChanged;
        
        [NonSerialized] private WeakReference<SDFFirstLayerRenderer> _ref;
        
#endif

#if UNITY_EDITOR
        /// <summary>
        /// For some reason this component causes crash (at lest I think it is)
        /// Maybe disposing it before compile will help.
        /// </summary>
        [InitializeOnLoadMethod]
        private static void Init()
        {
            foreach (var reference in activeRenderers)
            {
                if (reference.TryGetTarget(out var renderer)) Dispose(renderer);
                else Debug.LogError("Can't dispose SDFFirstLayerRenderer reference lost");
            }
            activeRenderers.Clear();
            activeRenderersChanged?.Invoke();
        }
        
        protected override void OnTransformParentChanged()
        {
            activeRenderersChanged?.Invoke();   
        }
#endif 

        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            _ref = new WeakReference<SDFFirstLayerRenderer>(this);
            activeRenderers.Add(_ref);
            activeRenderersChanged?.Invoke();
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
#if UNITY_EDITOR
            if (activeRenderers.Remove(_ref))
            {
                activeRenderersChanged?.Invoke();
            }
#endif
        }
    }
}