using UnityEditor;

namespace nickeltin.InternalBridge.Editor
{
    public class _SavedBool
    {
        private readonly SavedBool _instance;
        
        public _SavedBool(string name, bool value)
        {
            _instance = new SavedBool(name, value);
        }

        public _SavedBool(string name) : this(name, false)
        {
        }

        public bool Value
        {
            get => _instance.value;
            set => _instance.value = value;
        }
        
        public static implicit operator bool(_SavedBool s) => s.Value;
    }
}