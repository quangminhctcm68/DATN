using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace nickeltin.SDF.Editor
{
    /// <summary>
    /// SDF might be generated in multiple way, inherit this class to create a new type of sdf generation.
    /// Backend can be selected in <see cref="SDFImporterUnityInterface"/> GUI.
    /// </summary>
    public abstract class SDFGenerationBackend
    {
        /// <summary>
        /// Some data associated with backend
        /// </summary>
        public struct BackendBaseData
        {
            /// <summary>
            /// This identifier used to save current backend.
            /// </summary>
            public readonly string Identifier;
            
            public readonly string Description;
            
            /// <summary>
            /// Return properties names form <see cref="SDFImportSettings"/> that is used by this backend.
            /// </summary>
            public readonly string[] UsedProperties;

            public bool HideFromInspector;

            public bool Obsolete;

            public BackendBaseData(string identifier, string description, params string[] usedProperties)
            {
                Identifier = identifier;
                Description = description;
                UsedProperties = usedProperties;
                HideFromInspector = false;
                Obsolete = false;
            }
        }

        /// <summary>
        /// Access backends here. Creates all backends instances upon first access.
        /// </summary>
        public static class Provider
        {
            private static Dictionary<string, Context> _backends;

            static Provider() => Init();

            private static void Init()
            {
                _backends = new Dictionary<string, Context>();

                var backendsTypes = TypeCache.GetTypesDerivedFrom<SDFGenerationBackend>();
                foreach (var type in backendsTypes)
                {
                    if (type.IsAbstract || type.IsGenericType) continue;

                    var instance = (SDFGenerationBackend)Activator.CreateInstance(type);
                    var id = instance.BaseData.Identifier;
                    var context = new Context(instance);
                    if (!_backends.TryAdd(id, context))
                    {
                        Debug.LogError($"Multiple {nameof(SDFGenerationBackend)}'s with identifier: {id} registered");
                    }
                }
            }

            public static IEnumerable<Context> GetBackends() => _backends.Values;

            /// <summary>
            /// Returns instance of generation backend with corresponding id, if none find will return <see cref="GetDefaultBackend"/>
            /// </summary>
            /// <param name="backendId"></param>
            /// <returns></returns>
            public static Context GetBackend(string backendId)
            {
                if (!_backends.TryGetValue(backendId, out var backendContext))
                {
                    backendContext = GetDefaultBackend();
                }

                return backendContext;
            }
            
            public static bool HasBackend(string backendId)
            {
                return _backends.ContainsKey(backendId);
            }

            /// <summary>
            /// Returns default backend defined in <see cref="SDFEditorUtil.DEFAULT_GENERATION_BACKEND"/>
            /// </summary>
            /// <returns></returns>
            public static Context GetDefaultBackend() => _backends[SDFEditorUtil.DEFAULT_GENERATION_BACKEND];
        }
        
        /// <summary>
        /// Wrapper to make interaction with backend easier
        /// </summary>
        public readonly struct Context
        {
            private readonly HashSet<string> _usedProperties;
            private readonly HashSet<GUID> _artifactDependencies;

            public Context(SDFGenerationBackend backend)
            {
                Backend = backend;
                _usedProperties = new HashSet<string>();
                _artifactDependencies = new HashSet<GUID>();
                foreach (var prop in backend.BaseData.UsedProperties)
                {
                    _usedProperties.Add(prop);
                }

                foreach (var guid in backend.GetArtifactDependencies())
                {
                    _artifactDependencies.Add(new GUID(guid));
                }
            }

            public SDFGenerationBackend Backend { get; }

            public IEnumerable<GUID> ArtifactDependencies => _artifactDependencies;

            public bool IsPropertyUsed(string propName) => _usedProperties.Contains(propName);
        }

        /// <summary>
        /// Intermediate between <see cref="SDFImportSettings"/> and their adjusted version used directly in backend.
        /// </summary>
        public struct Settings
        {
            public SDFImportSettings OriginalSettings;
            public float ResolutionScale;
            public Vector4 BorderOffset;
            public float GradientSize;

            public Settings(SDFImportSettings originalSettings) : this(originalSettings, originalSettings.ResolutionScale, 
                Vector4.one * originalSettings.BorderOffset, originalSettings.GradientSize)
            {
            }

            public Settings(SDFImportSettings originalSettings, float resolutionScale, Vector4 borderOffset, float gradientSize)
            {
                OriginalSettings = originalSettings;
                ResolutionScale = resolutionScale;
                BorderOffset = borderOffset;
                GradientSize = gradientSize;
            }
        }
        
        internal SDFGenerationBackend()
        {
            
        }

        public abstract BackendBaseData BaseData { get; }

        /// <summary>
        /// Return source asset GUIDs that this importer is depends on. This will trigger re-import if any of source asset is changed.
        /// For example if shader used for generation then yield its guid saved in meta file here.
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<string> GetArtifactDependencies()
        {
            yield break;
        }

        public virtual string GetDisplayName()
        {
            return ObjectNames.NicifyVariableName(BaseData.Identifier);
        }

        /// <summary>
        /// Implement backend SDF generation logic here.
        /// </summary>
        /// <param name="rt">In/Out texture. Read from it and write the result to it. Texture should remain the same size.</param>
        /// <param name="settings"></param>
        public abstract void Generate(RenderTexture rt, Settings settings);
    }
}