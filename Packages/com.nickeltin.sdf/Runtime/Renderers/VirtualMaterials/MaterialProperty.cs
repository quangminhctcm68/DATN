using System;

namespace nickeltin.SDF.Runtime
{
    [Serializable]
    public struct MaterialProperty<T>
    {
        public string Name;
        public T Value;
    }
}