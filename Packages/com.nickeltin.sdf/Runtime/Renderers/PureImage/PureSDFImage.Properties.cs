using UnityEngine;

namespace nickeltin.SDF.Runtime
{
    public partial class PureSDFImage
    {
        public Layer LayerA
        {
            get => _layerA;
            set => SetLayer(ref _layerA, value);
        }

        public Layer LayerB
        {
            get => _layerB;
            set => SetLayer(ref _layerB, value);
        }

        public float Width
        {
            set
            {
                var l = _layerA;
                l.Width = value;
                SetLayer(ref _layerA, l);
                
                l = _layerB;
                l.Width = value;
                SetLayer(ref _layerB, l);
            }
        }

        private void SetLayer(ref Layer layer, Layer value)
        {
            // Sprite different, should dirty the material to update the textures
            if (layer.SDFSprite != value.SDFSprite) SetMaterialDirty();
            value.Softness = Mathf.Clamp01(value.Softness);
            value.Width = Mathf.Clamp01(value.Width);
            SDFRendererUtil.SetProperty(this, ref layer, value);
        }

        public bool LerpLayers
        {
            get => _lerpLayers;
            set => SDFRendererUtil.SetProperty(this, ref _lerpLayers, value);
        }
        
        public float LayersLerp
        {
            get => _layersLerp;
            set => SDFRendererUtil.SetFloat(this,ref _layersLerp, value, 0, 1);
        }

        public Vector2 Offset
        {
            get => _offset;
            set => SDFRendererUtil.SetProperty(this, ref _offset, value);
        }
    }
}