// Session tam thoi de GD Save/Load lai image, config, palette mapping va candidate pin.
// Session KHONG thay the RawLevelData cua game (muc 4 tai lieu thiet ke tool).
using System;
using System.Collections.Generic;
using UnityEngine;
using BrickEscape.PixelLevelGenerator.Model;

namespace BrickEscape.PixelLevelGenerator
{
    [Serializable]
    public class SerializablePaletteRow
    {
        public Color32 SourceColor;
        public string EditableHex;
        public int ExistingColorId = -1;
        public int ResolvedColorId = -1;
        public bool WillAddToRawColorData;
        public int PixelCount;
    }

    [Serializable]
    public class SerializableQuotaRow
    {
        public int ColorId;
        public int RequiredBlockCount;
    }

    public class PixelLevelGenerationSession : ScriptableObject
    {
        public Texture2D SourceTexture;
        public ImportMode ImportMode = ImportMode.ExactPixel;
        public float AlphaThreshold = 0.5f;
        public BackgroundRule BackgroundRule = BackgroundRule.TransparentIsVoid;
        public int DownsampleFactor = 2;
        public RectInt CropRect;

        public List<SerializablePaletteRow> PaletteRows = new List<SerializablePaletteRow>();
        public List<SerializableQuotaRow> QuotaRows = new List<SerializableQuotaRow>();

        public int LevelId = 1;
        public string OutputFolder = "Assets/_BrickEscape/Data/Raws";
        public float CellSize = 1f;
        public bool Hard;

        public DifficultyPreset Preset = DifficultyPreset.Normal;
        public int CandidateCount = 12;
        public int BaseSeed = 12345;
        public float MinVisualFidelity = 0.75f;

        public List<int> PinnedCandidateIndices = new List<int>();
        public bool IsDirty;

        public GenerationConfig ToGenerationConfig()
        {
            var cfg = new GenerationConfig
            {
                Preset = Preset,
                CandidateCount = CandidateCount,
                BaseSeed = BaseSeed,
                MinVisualFidelity = MinVisualFidelity,
                LevelId = LevelId,
                CellSize = CellSize,
                Hard = Hard
            };
            cfg.ApplyPreset(Preset);
            foreach (var q in QuotaRows)
                cfg.Quotas.Add(new ColorBlockQuota { ColorId = q.ColorId, RequiredBlockCount = q.RequiredBlockCount });
            return cfg;
        }
    }
}