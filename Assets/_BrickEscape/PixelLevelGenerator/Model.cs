// Model du lieu anh pixel va palette.
// Tham chieu: TAI_LIEU_THIET_KE_TOOL_PIXEL_TO_RAW_LEVEL.txt muc 1, 4, 7.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape.PixelLevelGenerator.Model
{
    public enum ImportMode
    {
        ExactPixel,
        Downsample,
        Crop
    }

    public enum BackgroundRule
    {
        TransparentIsVoid,
        SelectedColorIsVoid
    }

    /// <summary>
    /// Mot o cua ImageGrid sau khi import/crop/downsample.
    /// Tuong ung 1:1 voi mot cell tren board (muc 0/1 cua tai lieu thiet ke).
    /// </summary>
    [Serializable]
    public struct ImageCell
    {
        public bool IsVoid;
        public Color32 SourceColor;
        public int SourcePaletteIndex; // index vao ImageGrid.DistinctColors, -1 neu void

        public static ImageCell Void => new ImageCell { IsVoid = true, SourceColor = default, SourcePaletteIndex = -1 };
    }

    /// <summary>
    /// Luoi anh sau import: dung lam "pixel grid" hien thi va lam nguon DesiredColorId cho candidate.
    /// </summary>
    [Serializable]
    public class ImageGrid
    {
        public int Width;
        public int Height;
        public ImageCell[,] Cells;
        public List<Color32> DistinctColors = new List<Color32>();
        public ImportMode Mode;

        public ImageGrid(int width, int height)
        {
            Width = width;
            Height = height;
            Cells = new ImageCell[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    Cells[x, y] = ImageCell.Void;
        }

        public bool InBounds(int x, int y) => x >= 0 && x < Width && y >= 0 && y < Height;

        public int BoardCellCount()
        {
            int count = 0;
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    if (!Cells[x, y].IsVoid) count++;
            return count;
        }

        public int VoidCellCount() => Width * Height - BoardCellCount();
    }

    /// <summary>
    /// Mot dong trong Palette Manager: mot mau distinct cua anh va mapping sang Color ID cua game.
    /// </summary>
    [Serializable]
    public class PaletteEntry
    {
        public Color SourceColor;
        public string EditableHex;

        public int PixelCount;

        public int? ExistingColorId;

        public int ResolvedColorId = -1;

        public bool WillAddToRawColorData;

        public bool IsResolved => ResolvedColorId >= 0;
    }

    public enum ValidationSeverity
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Mot dong bao loi/canh bao. Luon co code + message + huong xu ly ro rang,
    /// theo dac ta muc 13 cua tai lieu thiet ke tool va muc 15 cua tai lieu UI.
    /// </summary>
    [Serializable]
    public class ValidationIssue
    {
        public ValidationSeverity Severity;
        public string Code;
        public string Message;
        public string SuggestedAction;
        public Vector2Int? GridPosition;
        public int? BlockId;
        public string CandidateId;

        public ValidationIssue(ValidationSeverity severity, string code, string message, string suggestedAction = null,
            Vector2Int? gridPosition = null, int? blockId = null, string candidateId = null)
        {
            Severity = severity;
            Code = code;
            Message = message;
            SuggestedAction = suggestedAction;
            GridPosition = gridPosition;
            BlockId = blockId;
            CandidateId = candidateId;
        }

        public override string ToString()
        {
            string pos = GridPosition.HasValue ? $" @({GridPosition.Value.x},{GridPosition.Value.y})" : "";
            string blk = BlockId.HasValue ? $" [Block {BlockId.Value}]" : "";
            return $"{Severity} {Code}{pos}{blk} - {Message}" +
                   (string.IsNullOrEmpty(SuggestedAction) ? "" : $" -> {SuggestedAction}");
        }
    }

    /// <summary>
    /// Ma loi chuan hoa, dung xuyen suot tool de UI/log tra cuu nhat quan.
    /// Xem muc 13 cua TAI_LIEU_THIET_KE_TOOL_PIXEL_TO_RAW_LEVEL.txt.
    /// </summary>
    public static class IssueCodes
    {
        public const string ImageWidthExceeds50 = "IMAGE_WIDTH_EXCEEDS_50";
        public const string ImageHeightInvalid = "IMAGE_HEIGHT_INVALID";
        public const string UnmappedSourceColor = "UNMAPPED_SOURCE_COLOR";
        public const string UnknownBlockPrefabId = "UNKNOWN_BLOCK_PREFAB_ID";
        public const string InvalidArrowJoint = "INVALID_ARROW_JOINT";
        public const string BlockOverlap = "BLOCK_OVERLAP";
        public const string Unsolvable = "UNSOLVABLE";
        public const string SolverBudgetExceeded = "SOLVER_BUDGET_EXCEEDED";
        public const string ColorQuotaNotMet = "COLOR_BLOCK_QUOTA_NOT_MET";
        public const string LowVisualFidelity = "LOW_VISUAL_FIDELITY";
        public const string PhysicsApproximation = "PHYSICS_APPROXIMATION";
        public const string PrefabBlockIdMismatch = "PREFAB_BLOCK_ID_MISMATCH";
        public const string NoBoardCell = "NO_BOARD_CELL";
        public const string MissingCellValues = "MISSING_CELL_VALUES";
        public const string DuplicateLevelId = "DUPLICATE_LEVEL_ID";
        public const string QuotaInfeasible = "QUOTA_INFEASIBLE";
        public const string NoArrowProfile = "NO_ARROW_PROFILE_FOR_DIRECTION";
    }
}