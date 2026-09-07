using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace nickeltin.Core.Editor
{
    public static class EditorSerialization
    {
        /// <summary>
        /// Loads all assets at path, returns first that matches the type.
        /// </summary>
        public static T Load<T>(string path) where T : Object
        {
            return InternalEditorUtility.LoadSerializedFileAndForget(path).OfType<T>().FirstOrDefault();
        }

        public static T LoadFromString<T>(string serializedObject) where T : Object
        {
            var tempPath = FileUtil.GetUniqueTempPathInProject();
            File.WriteAllText(tempPath, serializedObject);
            var obj = Load<T>(tempPath);
            File.Delete(tempPath);
            return obj;
        }
        
        /// <summary>
        /// Loads all assets at path, finds first that matches the type, copies loaded instance to provider instance, destroys loaded instance. 
        /// </summary>
        public static void LoadObject<T>(string path, T obj) where T : ScriptableObject
        {
            var instance = Load<T>(path);
            EditorUtility.CopySerialized(instance, obj);
            Object.DestroyImmediate(instance);
        }
        
        /// <summary>
        /// Serializes object, writes it to file at path.
        /// </summary>
        public static void SaveObject<T>(string path, T obj) where T : ScriptableObject
        {
            InternalEditorUtility.SaveToSerializedFileAndForget(new Object[] { obj }, path,
                EditorSettings.serializationMode != SerializationMode.ForceBinary);
        }
        
        /// <summary>
        /// Serializes object to file, reads its data, returns bytes, deletes file.
        /// </summary>
        public static byte[] SerializeObject<T>(T obj) where T : ScriptableObject
        {
            var tempPath = FileUtil.GetUniqueTempPathInProject();
            SaveObject(tempPath, obj);
            var bytes = File.ReadAllBytes(tempPath);
            File.Delete(tempPath);
            return bytes;
        }
        
        /// <summary>
        /// Serializes object to bytes, then encodes bytes to string.
        /// </summary>
        public static string SerializeObjectToString<T>(T obj) where T : ScriptableObject
        {
            return Encoding.UTF8.GetString(SerializeObject(obj));
        }
        
        /// <summary>
        /// Serialized to string empty instance of object, destroys it after.
        /// </summary>
        public static string SerializeObjectToString<T>() where T : ScriptableObject
        {
            var instance = ScriptableObject.CreateInstance<T>();
            var str = SerializeObjectToString(instance);
            Object.DestroyImmediate(instance);
            return str;
        }
    }
}