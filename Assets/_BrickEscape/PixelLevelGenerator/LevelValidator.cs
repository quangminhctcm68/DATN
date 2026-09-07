// Kiem schema RawLevel, overlap, palette, boundary, arrow, solver va output.
// Tham chieu: TAI_LIEU_THIET_KE_TOOL_PIXEL_TO_RAW_LEVEL.txt muc 1, 10.3, 13.
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BrickEscape.PixelLevelGenerator.Model;

namespace BrickEscape.PixelLevelGenerator
{
    public enum ValidationVerdict
    {
        Pass,
        PassWithWarnings,
        Fail,
        Unknown
    }

    public class ValidationReport
    {
        public ValidationVerdict Verdict;
        public List<ValidationIssue> Issues = new List<ValidationIssue>();
        public bool HasErrors => Issues.Any(i => i.Severity == ValidationSeverity.Error);
        public bool HasWarnings => Issues.Any(i => i.Severity == ValidationSeverity.Warning);
    }

    public static class LevelValidator
    {
        public const int MaxColumns = 50;

        /// <summary>
        /// Tang 1: validator "pure" (schema/overlap/palette/boundary/arrow/solver). Khong dung Unity Editor API.
        /// </summary>
        public static ValidationReport ValidateCandidate(LevelCandidate candidate, Dictionary<int, BlockDefinition> catalog,
            HashSet<int> knownColorIds, HashSet<int> existingLevelIdsInFolder, int levelIdBeingExported)
        {
            var report = new ValidationReport();
            var issues = report.Issues;

            if (candidate.GridSize.x < 1 || candidate.GridSize.y < 1)
                issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.ImageHeightInvalid,
                    "Grid width/height phai >= 1.", "Kiem tra lai import."));

            if (candidate.GridSize.x > MaxColumns)
                issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.ImageWidthExceeds50,
                    $"Grid rong {candidate.GridSize.x} cot, vuot gioi han {MaxColumns}.", "Crop/downsample image."));

            bool hasBoardCell = false;
            for (int x = 0; x < candidate.BoardMask.GetLength(0) && !hasBoardCell; x++)
                for (int y = 0; y < candidate.BoardMask.GetLength(1) && !hasBoardCell; y++)
                    if (candidate.BoardMask[x, y]) hasBoardCell = true;

            if (!hasBoardCell)
                issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.NoBoardCell,
                    "Khong co o board nao (Values[0] >= 0).", "Kiem tra alpha threshold/background rule khi import."));

            // Overlap + boundary + prefab/color/arrow id checks
            var occupancy = new Dictionary<Vector2Int, int>();
            foreach (var placement in candidate.Blocks)
            {
                if (!catalog.TryGetValue(placement.PrefabId, out var def))
                {
                    issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.UnknownBlockPrefabId,
                        $"Block prefab ID {placement.PrefabId} khong ton tai trong catalog.",
                        "Remove/map placement to a catalog prefab.", blockId: placement.PrefabId));
                    continue;
                }

                if (knownColorIds != null && !knownColorIds.Contains(placement.ColorId))
                {
                    issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.UnmappedSourceColor,
                        $"Color ID {placement.ColorId} tren block {placement.PlacementId} khong co trong RawColorData.",
                        "Open Palette Manager and resolve color.", blockId: placement.PrefabId));
                }

                if (placement.ArrowStartIndex < 1 || placement.ArrowStartIndex > def.Joints.Count)
                {
                    issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.InvalidArrowJoint,
                        $"arrowStartIndex {placement.ArrowStartIndex} khong hop le cho prefab {placement.PrefabId} " +
                        $"(co {def.Joints.Count} joint).", "Choose joint 1..N.", blockId: placement.PrefabId));
                }
                else if (!def.ArrowProfiles.TryGetValue(placement.Move, out var profile) ||
                         profile.AnchorJointIndex1Based != placement.ArrowStartIndex ||
                         profile.LongestLengthInCells != placement.ArrowLength)
                {
                    issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.InvalidArrowJoint,
                        $"Arrow cua block {placement.PlacementId} (prefab {placement.PrefabId}) khong trung " +
                        "voi ArrowProfile da resolve tu catalog cho huong nay.",
                        "De tool tu suy arrow lai tu catalog, khong tu chinh tay.", blockId: placement.PrefabId));
                }

                if (placement.OccupiedCells == null || placement.OccupiedCells.Length == 0)
                {
                    issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.MissingCellValues,
                        $"Block {placement.PlacementId} khong co footprint cell.", "Kiem tra BlockDefinition.",
                        blockId: placement.PrefabId));
                    continue;
                }

                foreach (var cell in placement.OccupiedCells)
                {
                    if (cell.x < 0 || cell.x >= candidate.GridSize.x || cell.y < 0 || cell.y >= candidate.GridSize.y)
                    {
                        issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.BlockOverlap,
                            $"Block {placement.PlacementId} co footprint nam ngoai bien board.",
                            "Move/remove placement.", gridPosition: cell, blockId: placement.PrefabId));
                        continue;
                    }

                    if (occupancy.TryGetValue(cell, out int otherId) && otherId != placement.PlacementId)
                    {
                        issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.BlockOverlap,
                            $"Block {placement.PlacementId} chong footprint voi block {otherId} tai ({cell.x},{cell.y}).",
                            "Move/remove placement.", gridPosition: cell, blockId: placement.PrefabId));
                    }
                    else
                    {
                        occupancy[cell] = placement.PlacementId;
                    }
                }
            }

            // Quota
            if (candidate.Score.ColorQuotaFit < 1f)
            {
                issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.ColorQuotaNotMet,
                    "Mot hoac nhieu Color ID chua dung quota bat buoc.", "Xem bang Blocks per Color de sua."));
            }

            // Solver
            if (!candidate.Solution.IsSolved)
            {
                if (candidate.Solution.BudgetExceeded)
                    issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.SolverBudgetExceeded,
                        candidate.Solution.FailureReason ?? "Solver budget exceeded.", "Run Solve Longer; do not export yet."));
                else
                    issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.Unsolvable,
                        candidate.Solution.FailureReason ?? "No exit order found by solver.", "No exit order found by solver."));
            }

            // Visual fidelity warning (khong phai loi cung)
            if (candidate.Score.VisualFidelity < 0.8f)
            {
                issues.Add(new ValidationIssue(ValidationSeverity.Warning, IssueCodes.LowVisualFidelity,
                    $"{candidate.Score.VisualFidelity:F2} below requested 0.80.", "Xem xet regenerate/mutate."));
            }

            // Prefab tu fallback catalog (khong doc duoc prefab that) -> physics approximation warning
            foreach (var placement in candidate.Blocks)
            {
                if (catalog.TryGetValue(placement.PrefabId, out var def) && !def.SourcedFromPrefab)
                {
                    issues.Add(new ValidationIssue(ValidationSeverity.Warning, IssueCodes.PhysicsApproximation,
                        $"Block {placement.PlacementId} (prefab {placement.PrefabId}) dung fallback catalog, " +
                        "chua doc duoc collider that.", "Run hidden-scene verification.", blockId: placement.PrefabId));
                    break; // mot canh bao chung la du, tranh spam log
                }
            }

            // Duplicate LevelID
            if (existingLevelIdsInFolder != null && existingLevelIdsInFolder.Contains(levelIdBeingExported))
            {
                issues.Add(new ValidationIssue(ValidationSeverity.Warning, IssueCodes.DuplicateLevelId,
                    $"LevelID {levelIdBeingExported} da ton tai trong folder dich.",
                    "Chon Overwrite/Create Copy trong Export Dialog neu day la chu dong."));
            }

            if (issues.Any(i => i.Severity == ValidationSeverity.Error))
                report.Verdict = candidate.Solution.BudgetExceeded && !candidate.Solution.IsSolved &&
                                  !issues.Any(i => i.Code != IssueCodes.SolverBudgetExceeded && i.Severity == ValidationSeverity.Error)
                    ? ValidationVerdict.Unknown
                    : ValidationVerdict.Fail;
            else if (issues.Any(i => i.Severity == ValidationSeverity.Warning))
                report.Verdict = ValidationVerdict.PassWithWarnings;
            else
                report.Verdict = ValidationVerdict.Pass;

            return report;
        }
    }

}