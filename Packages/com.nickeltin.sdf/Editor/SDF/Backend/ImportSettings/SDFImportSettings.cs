using System;
using System.ComponentModel;
using nickeltin.Core.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace nickeltin.SDF.Editor
{
    /// <summary>
    /// Import settings is used as partial allowing for each backend to have its own settings.
    /// Border offset and resolution scale is built-in features that works with all backends.
    /// </summary>
    [Serializable]
    public partial class SDFImportSettings
    {
        private const string BORDER_OFFSET_DESC =
            "How many pixels will be added to sprite border. SDF needs space to generate between sprite edges (filled pixels) and actual texture edges." +
            "Not consistent with sprite size, use smaller values for smaller sprite.";
        
        public const float MIN_RES_SCALE = 0.001f;
        public const float MAX_RES_SCALE = 1f;

        public const int MIN_BORDER_OFFSET = 0;
        public const int MAX_BORDER_OFFSET = 256;

        [Tooltip("Is SDF texture should be generated?")]
        [SerializeField]
        public bool GenerateSDF = false;
        
        [SerializeField]
        public string SDFGenerationBackend = SDFEditorUtil.DEFAULT_GENERATION_BACKEND;
        
        // Marking with wrong serialization name to re-serialize with new settings.
        [Tooltip(BORDER_OFFSET_DESC)]
        [FormerlySerializedAs("obsolete_border_offset")] 
        [SerializeField, Range(MIN_BORDER_OFFSET, MAX_BORDER_OFFSET)] 
        public int BorderOffset = SDFEditorUtil.DEFAULT_BORDER_OFFSET;
        
        
        [Tooltip("Allow to save SDF texture with lower resolution, for simple shapes 10% (0.1) is usually good enough value.")]
        [SerializeField, Range(MIN_RES_SCALE, MAX_RES_SCALE)] 
        public float ResolutionScale = 0.1f;


        private const string KEY = "SDFImportSettings";
        
        public static SDFImportSettings Load(AssetImporter importer)
        {
            var data = ImportSettingsUserData.Load(importer);
            var settings = data.Read(KEY, new SDFImportSettings());
            Validate(settings);
            return settings;
        }

        public static void Save(AssetImporter importer, SDFImportSettings settings)
        {
            var data = ImportSettingsUserData.Load(importer);
            data.Write(KEY, settings);
            data.Save();
        }

        public static void Validate(SDFImportSettings settings)
        {
            settings.BorderOffset = Mathf.Clamp(settings.BorderOffset, MIN_BORDER_OFFSET, MAX_BORDER_OFFSET);
            settings.ResolutionScale = Mathf.Clamp(settings.ResolutionScale, MIN_RES_SCALE, MAX_RES_SCALE);
            settings.GradientSize = Mathf.Clamp(settings.GradientSize, MIN_GRAD_SIZE, MAX_GRAD_SIZE);
        }
    }
}