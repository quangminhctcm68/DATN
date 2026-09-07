using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Object = UnityEngine.Object;

namespace nickeltin.SDF.Editor
{
    internal sealed class SDFImporter
    {
        public const int VERSION = 39;
        
        private static readonly ImportStep[] DEFAULT_IMPORT_STEPS = {
            new PreprocessTexture(),
            new PostprocessTexture(),
            new PostprocessSprites()
        };
            
            
        private readonly Queue<ImportStep> _stepsQueue;
        private readonly AssetImportContext _astCtx;
        private readonly SDFImportContext _sdfCtx;
        private bool _importEnded;

        /// <summary>
        /// Macro for regular pipeline import.
        /// Target is Texture file. 
        /// </summary>
        public static SDFImporter CreateRegular(TextureImporter texImporter, AssetImportContext astCtx)
        {
            return new SDFImporter(texImporter, texImporter, astCtx, DEFAULT_IMPORT_STEPS, null);
        }

        /// <summary>
        /// Macro for decoupled pipeline import.
        /// Target is *.sdfasset (can be used for any other asset) file with dependency Texture file.
        /// Has custom steps.
        /// </summary>
        public static SDFImporter CreateDecoupled(TextureImporter texImporter, AssetImportContext astCtx,
            AssetImporter mainAssetImporter, SDFImportSettings settings, params ImportStep[] steps)
        {
            return new SDFImporter(texImporter, mainAssetImporter, astCtx, steps, settings);
        }
        
        private SDFImporter(TextureImporter texImporter, AssetImporter mainAssetImporter, AssetImportContext astCtx, 
            IEnumerable<ImportStep> steps, SDFImportSettings importSettings)
        {
            _stepsQueue = new Queue<ImportStep>(steps);
            _astCtx = astCtx;
            _sdfCtx = new SDFImportContext(texImporter, importSettings, mainAssetImporter);
        }
        
        
        public void SetTexture(Texture2D texOriginal)
        {
            _sdfCtx.Texture = texOriginal;
        }

        /// <summary>
        /// Its important to sprites in same order as sprite rects is returned. <see cref="SDFImportUtil.GetSpriteRects"/>
        /// </summary>
        /// <param name="sprites"></param>
        public void SetSprites(Sprite[] sprites)
        {
            _sdfCtx.Sprites = sprites;
        }
        
        /// <summary>
        /// Dequeues next step and executes it in try catch, when all steps is completed finishes the import.
        /// </summary>
        public bool Step()
        {
            if (_importEnded) return false;
            
            try
            {
                var step = _stepsQueue.Dequeue();
                var result = step.Process(_sdfCtx, _astCtx);
                if (_stepsQueue.Count == 0 || result.ImportEnded)
                {
                    FinishImport(result.Successful);
                }
            }
            catch (Exception e)
            {
                CleanUp();
                e = new Exception("<b>[SDFImporter]</b>", e);
                Debug.LogException(e, _sdfCtx.MainAssetImporter);
                return false;
            }

            return true;
        }
        
        
        private void FinishImport(bool successful)
        {
            _importEnded = true;

            if (successful)
            {
                while (_sdfCtx.TryDequeueSubAsset(out var subAsset))
                {
                    if (subAsset.Thumbnail != null)
                        _astCtx.AddObjectToAsset(subAsset.ID, subAsset.Asset, subAsset.Thumbnail);
                    else
                        _astCtx.AddObjectToAsset(subAsset.ID, subAsset.Asset);
                }
                
                SDFImportResult.Save(_astCtx, _sdfCtx.ResultArtifact);
            }
            
            CleanUp();
        }

        private void CleanUp()
        {
            // If import was not successful, destroying sub assets that still left in queue
            while (_sdfCtx.TryDequeueSubAsset(out var subAsset))
            {
                Object.DestroyImmediate(subAsset.Asset);
            }
            
            if (_sdfCtx.TextureCopy == null) return;
            
            if (_sdfCtx.TextureCopy is RenderTexture rt) RenderTexture.ReleaseTemporary(rt);
            else Object.DestroyImmediate(_sdfCtx.TextureCopy);

            _sdfCtx.TextureCopy = null;
        }
    }
}