using System;
using System.Collections.Generic;
using UnityEngine;

namespace nickeltin.SDF.Runtime
{
    /// <summary>
    /// TODO: material serialization
    /// </summary>
    [Serializable]
    public struct VirtualMaterial
    {
        public bool Enabled;
        public Shader Shader;
        public List<MaterialProperty<int>> Ints;
        public List<MaterialProperty<float>> Floats;
        public List<MaterialProperty<Vector4>> Vectors;
        public List<MaterialProperty<Texture>> Textures;
    }
}