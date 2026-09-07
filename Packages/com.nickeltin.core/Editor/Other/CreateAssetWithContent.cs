using nickeltin.InternalBridge.Editor;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using Object = UnityEngine.Object;

namespace nickeltin.Core.Editor
{
    public class CreateAssetWithContent : EndNameEditAction
    {
        public delegate void ObjectCreatedDelegate(Object @object, string path); 
        
        public string FileContent;

        public ObjectCreatedDelegate ObjectCreated;
        
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            var instance = _ProjectWindowUtil.CreateScriptAssetWithContent(pathName, FileContent);
            ProjectWindowUtil.ShowCreatedAsset(instance);
            ObjectCreated?.Invoke(instance, pathName);
        }
        
        
        public static void Create(string filename, string content, ObjectCreatedDelegate objectCreated)
        {
            var instance = CreateInstance<CreateAssetWithContent>();
            instance.FileContent = content;
            instance.ObjectCreated = objectCreated;
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, instance, filename, null, null);
        }
    }
}