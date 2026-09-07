// Model du lieu cho GenerationConfig, LevelCandidate, CandidateScore, SolutionProof.
// Tham chieu: TAI_LIEU_THIET_KE_TOOL_PIXEL_TO_RAW_LEVEL.txt muc 3.RE1.1-4, muc 6, 9, 10.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape.PixelLevelGenerator.Model
{
    public enum DifficultyPreset
    {
        Easy,
        Normal,
        Hard,
        Custom
    }

    public enum ForceMoveLevel { Low, Medium, High }
    public enum BranchingLevel { Low, Medium, High }

    /// <summary>
    /// Quota block bat buoc theo tung Color ID da resolve tu anh (Revision 1.1).
    /// KHONG con TargetBlockCount/tolerance toan cuc.
    /// </summary>
    [Serializable]
    public class ColorBlockQuota
    {
        public int ColorId;
        public Color32 PreviewColor;
        public int SourcePixelCount;
        public int RequiredBlockCount;
        public int ActualBlockCount;
        public List<Color32> MergedSourceColors = new List<Color32>(); // > 1 neu nhieu mau source cung map ID nay

        public bool IsMet => ActualBlockCount == RequiredBlockCount;
    }

    /// <summary>
    /// Config sinh candidate. Sau Revision 1.1, khong con TargetBlockCount/BlockCountTolerancePercent:
    /// tong block = tong ColorBlockQuota.RequiredBlockCount.
    /// </summary>
    [Serializable]
    public class GenerationConfig
    {
        public DifficultyPreset Preset = DifficultyPreset.Normal;
        public List<ColorBlockQuota> Quotas = new List<ColorBlockQuota>();
        public int TotalBlocks => SumQuota();
        private int SumQuota()
        {
            int s = 0;
            foreach (var q in Quotas) s += q.RequiredBlockCount;
            return s;
        }

        public int CandidateCount = 12;
        public int BaseSeed = 12345;
        public float MinVisualFidelity = 0.75f;

        // advanced
        public List<int> AllowedPrefabIds = new List<int>();
        public Dictionary<int, float> PrefabWeights = new Dictionary<int, float>();
        public int? DesiredSolutionStepsMin;
        public int? DesiredSolutionStepsMax;
        public ForceMoveLevel ForceMoves = ForceMoveLevel.Medium;
        public BranchingLevel Branching = BranchingLevel.Medium;
        public int SolverBudgetNodes = 200_000;
        public float SolverBudgetSeconds = 2.0f;
        public bool UseHiddenScenePhysicsVerification = false;

        public int GridWidth;
        public int GridHeight;
        public int LevelId;
        public float CellSize = 1f;
        public bool Hard;

        public void ApplyPreset(DifficultyPreset preset)
        {
            Preset = preset;
            switch (preset)
            {
                case DifficultyPreset.Easy:
                    ForceMoves = ForceMoveLevel.High;
                    Branching = BranchingLevel.Low;
                    DesiredSolutionStepsMin = null;
                    DesiredSolutionStepsMax = null;
                    break;
                case DifficultyPreset.Normal:
                    ForceMoves = ForceMoveLevel.Medium;
                    Branching = BranchingLevel.Medium;
                    break;
                case DifficultyPreset.Hard:
                    ForceMoves = ForceMoveLevel.Low;
                    Branching = BranchingLevel.High;
                    break;
                case DifficultyPreset.Custom:
                    break;
            }
        }
    }

    [Serializable]
    public class SolutionProof
    {
        public bool IsSolved;
        public List<int> RemovalOrder = new List<int>(); // PlacementId theo thu tu click
        public int SearchNodes;
        public int SolutionCountCapped;
        public string FailureReason;
        public bool BudgetExceeded;
    }

    [Serializable]
    public class CandidateScore
    {
        public float VisualFidelity;   // 0..1
        public float ColorQuotaFit;    // 1 chi khi tat ca quota dung, nguoc lai 0 => FAIL
        public float DifficultyScore;  // 0..100, thong tin review
        public float DifficultyPresetFit; // 0..1, dung cho cong thuc xep hang
        public float DiversityBonus;   // 0..1
        public int BlockCount;
        public int UsedColorCount;
        public int SolutionSteps;
        public int BranchingFactor;
        public int ForcedMoveCount;

        public float Total => 0.45f * VisualFidelity + 0.25f * ColorQuotaFit + 0.20f * DifficultyPresetFit + 0.10f * DiversityBonus;
    }

    public enum CandidateStatus
    {
        Pending,
        Solved,
        Unsolved,
        Unknown,   // budget exceeded
        Invalid    // validator error (schema/palette/overlap/quota)
    }

    [Serializable]
    public class LevelCandidate
    {
        public string CandidateId;
        public int Seed;
        public Vector2Int GridSize;
        public bool[,] BoardMask;         // true = board cell (khong void)
        public List<BlockPlacement> Blocks = new List<BlockPlacement>();
        public int[,] DesiredColorId;     // -1 neu khong co mau/void
        public CandidateScore Score = new CandidateScore();
        public SolutionProof Solution = new SolutionProof();
        public List<ValidationIssue> Issues = new List<ValidationIssue>();
        public CandidateStatus Status = CandidateStatus.Pending;
        public bool Pinned;
        public bool Rejected;
        public string GenerationLog;

        public bool HasErrors()
        {
            foreach (var i in Issues) if (i.Severity == ValidationSeverity.Error) return true;
            return false;
        }

        public bool CanExport() => Status == CandidateStatus.Solved && Solution.IsSolved && !HasErrors();
    }
}