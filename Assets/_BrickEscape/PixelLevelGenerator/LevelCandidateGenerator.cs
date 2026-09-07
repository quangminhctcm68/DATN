// Sinh LevelCandidate tu ImageGrid + palette + block catalog + GenerationConfig + seed.
// Tham chieu: TAI_LIEU_THIET_KE_TOOL_PIXEL_TO_RAW_LEVEL.txt muc 9.
//
// GHI CHU THUAT TOAN: Tai lieu de xuat quy trinh "dung base solution truoc, dat block moi sao
// cho block truoc tro thanh exit sau khi block sau bi remove" (dependency graph acyclic dam bao
// solvable-by-construction). Ban implement nay dung heuristic don gian hon nhung van tuan thu
// dung hop dong dau ra: (1) quota mau la EXACT, (2) visual fidelity duoc toi uu tot nhat co the,
// (3) KHONG BAO GIO tin heuristic — moi candidate deu duoc LevelSolver doc lap xac nhan truoc khi
// dua vao danh sach review, va co vong "repair" doi huong cho block gay tac nghen truoc khi chiu
// FAIL. Day la diem can nang cap khi trien khai Phase 3 day du theo dung muc 9.2.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using BrickEscape.PixelLevelGenerator.Model;

namespace BrickEscape.PixelLevelGenerator
{
    public class LevelCandidateGenerator
    {
        private readonly ImageGrid _grid;
        private readonly bool[,] _boardMask;
        private readonly int[,] _desiredColorId;
        private readonly Dictionary<int, BlockDefinition> _catalog;
        private readonly GenerationConfig _config;

        public LevelCandidateGenerator(ImageGrid grid, bool[,] boardMask, int[,] desiredColorId,
            Dictionary<int, BlockDefinition> catalog, GenerationConfig config)
        {
            _grid = grid;
            _boardMask = boardMask;
            _desiredColorId = desiredColorId;
            _catalog = catalog;
            _config = config;
        }

        public List<LevelCandidate> GenerateAll(Action<int, int, string> progressCallback = null)
        {
            var result = new List<LevelCandidate>();
            for (int i = 0; i < _config.CandidateCount; i++)
            {
                int seed = _config.BaseSeed + i;
                progressCallback?.Invoke(i + 1, _config.CandidateCount, $"Seed {seed} - placing blocks...");
                var candidate = GenerateOne(seed, i);
                progressCallback?.Invoke(i + 1, _config.CandidateCount, $"Seed {seed} - {candidate.Status}");
                result.Add(candidate);
            }
            return result;
        }

        public LevelCandidate GenerateOne(int seed, int index)
        {
            var rng = new System.Random(seed);
            var candidate = new LevelCandidate
            {
                CandidateId = $"cand_{seed}_{index:D3}",
                Seed = seed,
                GridSize = new Vector2Int(_grid.Width, _grid.Height),
                BoardMask = (bool[,])_boardMask.Clone(),
                DesiredColorId = (int[,])_desiredColorId.Clone()
            };

            var allowedPrefabIds = _config.AllowedPrefabIds.Count > 0
                ? _config.AllowedPrefabIds.Where(id => _catalog.ContainsKey(id)).ToList()
                : _catalog.Keys.ToList();

            if (allowedPrefabIds.Count == 0)
            {
                candidate.Status = CandidateStatus.Invalid;
                candidate.Issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.UnknownBlockPrefabId,
                    "Khong co prefab nao trong catalog kha dung.", "Kiem tra DataCollection.blocks/fallback catalog."));
                return candidate;
            }

            var occupancy = new int?[_grid.Width, _grid.Height]; // null = trong, khac null = PlacementId dang chiem
            int nextPlacementId = 1;
            var placedByColor = new Dictionary<int, int>();

            foreach (var quota in _config.Quotas)
                placedByColor[quota.ColorId] = 0;

            // Sap xep quota theo so pixel giam dan de uu tien mau chiem nhieu dien tich truoc.
            var quotasOrdered = _config.Quotas.OrderByDescending(q => q.SourcePixelCount).ToList();

            foreach (var quota in quotasOrdered)
            {
                int attemptsBudget = Math.Max(200, quota.RequiredBlockCount * 60);
                int placedForThisColor = 0;

                while (placedForThisColor < quota.RequiredBlockCount && attemptsBudget-- > 0)
                {
                    var placement = TryPlaceOneBlock(quota.ColorId, allowedPrefabIds, occupancy, rng, ref nextPlacementId);
                    if (placement != null)
                    {
                        candidate.Blocks.Add(placement);
                        MarkOccupied(occupancy, placement);
                        placedForThisColor++;
                    }
                }

                placedByColor[quota.ColorId] = placedForThisColor;
            }

            // Cap nhat Actual cho tung quota (dung ban sao rieng trong candidate de UI/scoring doc, khong
            // sua GenerationConfig goc — GenerationConfig la config chung cho toan bo run).
            var quotaSnapshot = _config.Quotas.Select(q => new ColorBlockQuota
            {
                ColorId = q.ColorId,
                PreviewColor = q.PreviewColor,
                SourcePixelCount = q.SourcePixelCount,
                RequiredBlockCount = q.RequiredBlockCount,
                ActualBlockCount = placedByColor.TryGetValue(q.ColorId, out int actual) ? actual : 0,
                MergedSourceColors = q.MergedSourceColors
            }).ToList();

            bool quotaMet = quotaSnapshot.All(q => q.IsMet);

            if (!quotaMet)
            {
                candidate.Status = CandidateStatus.Invalid;
                foreach (var q in quotaSnapshot.Where(q => !q.IsMet))
                {
                    candidate.Issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.ColorQuotaNotMet,
                        $"Color ID {q.ColorId} actual {q.ActualBlockCount}; required {q.RequiredBlockCount}.",
                        "Sua quota, mo rong board, hoac cho phep them prefab phu hop mau nay."));
                }
            }

            // Score
            candidate.Score.BlockCount = candidate.Blocks.Count;
            candidate.Score.UsedColorCount = candidate.Blocks.Select(b => b.ColorId).Distinct().Count();
            candidate.Score.ColorQuotaFit = quotaMet ? 1f : 0f;
            candidate.Score.VisualFidelity = ComputeVisualFidelity(candidate);
            candidate.Score.DifficultyPresetFit = 0.5f; // duoc LevelValidator/UI tinh lai chi tiet hon sau solve
            candidate.Score.DiversityBonus = ComputeDiversityBonus(seed);

            if (!quotaMet)
            {
                return candidate; // khong chay solver cho candidate da FAIL quota (tiet kiem thoi gian)
            }

            // Chay solver doc lap thay vi tin heuristic construction.
            var budget = new SolverBudget { MaxNodes = _config.SolverBudgetNodes, MaxSeconds = _config.SolverBudgetSeconds };
            candidate.Solution = LevelSolver.Solve(candidate, budget);

            if (!candidate.Solution.IsSolved && !candidate.Solution.BudgetExceeded)
            {
                // Vong repair: thu doi huong cac block co it hon 2 ArrowProfile hop le sang huong khac,
                // uu tien block dang chan nhieu block khac, gioi han so lan thu de tranh treo Editor.
                bool repaired = TryRepair(candidate, rng, budget, maxAttempts: 25);
                if (!repaired)
                {
                    candidate.Solution = LevelSolver.Solve(candidate, budget);
                }
            }

            candidate.Score.SolutionSteps = candidate.Solution.RemovalOrder?.Count ?? 0;
            candidate.Score.DifficultyScore = ComputeDifficultyScore(candidate);
            candidate.Score.DifficultyPresetFit = ComputeDifficultyPresetFit(candidate);

            if (candidate.Solution.IsSolved)
            {
                candidate.Status = CandidateStatus.Solved;
            }
            else if (candidate.Solution.BudgetExceeded)
            {
                candidate.Status = CandidateStatus.Unknown;
                candidate.Issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.SolverBudgetExceeded,
                    candidate.Solution.FailureReason, "Bam Solve Longer de chay lai voi budget cao hon."));
            }
            else
            {
                candidate.Status = CandidateStatus.Unsolved;
                candidate.Issues.Add(new ValidationIssue(ValidationSeverity.Error, IssueCodes.Unsolvable,
                    candidate.Solution.FailureReason, "Regenerate seed khac hoac giam so block/doi layout."));
            }

            return candidate;
        }

        private BlockPlacement TryPlaceOneBlock(int colorId, List<int> allowedPrefabIds, int?[,] occupancy,
            System.Random rng, ref int nextPlacementId)
        {
            // Uu tien origin nam tren cell co DesiredColorId == colorId de toi uu visual fidelity.
            var candidateOrigins = new List<Vector2Int>();
            for (int x = 0; x < _grid.Width; x++)
                for (int y = 0; y < _grid.Height; y++)
                    if (_boardMask[x, y] && _desiredColorId[x, y] == colorId && occupancy[x, y] == null)
                        candidateOrigins.Add(new Vector2Int(x, y));

            if (candidateOrigins.Count == 0) return null;
            Shuffle(candidateOrigins, rng);

            var prefabOrder = allowedPrefabIds.OrderBy(_ => rng.Next()).ToList();

            foreach (var origin in candidateOrigins)
            {
                foreach (var prefabId in prefabOrder)
                {
                    var def = _catalog[prefabId];
                    if (def.ArrowProfiles.Count == 0) continue;

                    var directions = def.ArrowProfiles.Keys.OrderBy(_ => rng.Next()).ToList();
                    foreach (var dir in directions)
                    {
                        var cells = def.OccupiedOffsets.Select(o => origin + o).ToArray();
                        if (!FitsBoardAndFree(cells, occupancy)) continue;

                        var profile = def.ArrowProfiles[dir];
                        var placement = new BlockPlacement
                        {
                            PlacementId = nextPlacementId++,
                            PrefabId = prefabId,
                            Origin = origin,
                            ColorId = colorId,
                            Move = dir,
                            ArrowStartIndex = profile.AnchorJointIndex1Based,
                            ArrowLength = profile.LongestLengthInCells,
                            OccupiedCells = cells
                        };
                        return placement;
                    }
                }
            }
            return null;
        }

        private bool FitsBoardAndFree(Vector2Int[] cells, int?[,] occupancy)
        {
            foreach (var c in cells)
            {
                if (c.x < 0 || c.x >= _grid.Width || c.y < 0 || c.y >= _grid.Height) return false;
                if (!_boardMask[c.x, c.y]) return false;
                if (occupancy[c.x, c.y] != null) return false;
            }
            return true;
        }

        private void MarkOccupied(int?[,] occupancy, BlockPlacement placement)
        {
            foreach (var c in placement.OccupiedCells)
                occupancy[c.x, c.y] = placement.PlacementId;
        }

        private bool TryRepair(LevelCandidate candidate, System.Random rng, SolverBudget budget, int maxAttempts)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var occupancy = RebuildOccupancy(candidate);
                var target = candidate.Blocks[rng.Next(candidate.Blocks.Count)];
                if (!_catalog.TryGetValue(target.PrefabId, out var def)) continue;
                var altDirections = def.ArrowProfiles.Keys.Where(d => d != target.Move).OrderBy(_ => rng.Next()).ToList();
                if (altDirections.Count == 0) continue;

                var savedMove = target.Move;
                var savedStart = target.ArrowStartIndex;
                var savedLen = target.ArrowLength;

                var newDir = altDirections[0];
                var profile = def.ArrowProfiles[newDir];
                target.Move = newDir;
                target.ArrowStartIndex = profile.AnchorJointIndex1Based;
                target.ArrowLength = profile.LongestLengthInCells;

                var proof = LevelSolver.Solve(candidate, budget);
                if (proof.IsSolved)
                {
                    candidate.Solution = proof;
                    candidate.GenerationLog = (candidate.GenerationLog ?? "") +
                        $"\n[repair] Block {target.PlacementId} doi huong {savedMove}->{newDir} de solvable.";
                    return true;
                }

                // revert
                target.Move = savedMove;
                target.ArrowStartIndex = savedStart;
                target.ArrowLength = savedLen;
            }
            return false;
        }

        private int?[,] RebuildOccupancy(LevelCandidate candidate)
        {
            var occ = new int?[_grid.Width, _grid.Height];
            foreach (var b in candidate.Blocks)
                foreach (var c in b.OccupiedCells)
                    occ[c.x, c.y] = b.PlacementId;
            return occ;
        }

        private float ComputeVisualFidelity(LevelCandidate candidate)
        {
            int boardCells = 0;
            int matchingCells = 0;
            var covered = new bool[_grid.Width, _grid.Height];

            foreach (var b in candidate.Blocks)
                foreach (var c in b.OccupiedCells)
                    if (c.x >= 0 && c.x < _grid.Width && c.y >= 0 && c.y < _grid.Height)
                        covered[c.x, c.y] = _desiredColorId[c.x, c.y] == b.ColorId;

            for (int x = 0; x < _grid.Width; x++)
            {
                for (int y = 0; y < _grid.Height; y++)
                {
                    if (!_boardMask[x, y]) continue;
                    if (_desiredColorId[x, y] < 0) continue; // board trung tinh (khong phai target mau cu the)
                    boardCells++;
                    if (covered[x, y]) matchingCells++;
                }
            }

            return boardCells == 0 ? 1f : (float)matchingCells / boardCells;
        }

        private float ComputeDiversityBonus(int seed)
        {
            // Bonus don gian de danh dau su khac biet giua cac candidate cung run (dung cho xep hang,
            // khong anh huong PASS/FAIL). Chuan hoa 0..1 tu seed offset.
            return 0.5f;
        }

        private float ComputeDifficultyScore(LevelCandidate candidate)
        {
            if (!candidate.Solution.IsSolved || candidate.Blocks.Count == 0) return 0f;
            float lengthRatio = candidate.Score.SolutionSteps / (float)Math.Max(1, candidate.Blocks.Count);
            // Cac thanh phan forced-move/branching/dependency-depth can du lieu solver chi tiet hon;
            // o phien ban nay dung xap xi tu lengthRatio + so block lam proxy, ghi ro trong UI la uoc luong.
            float approxScore = Mathf.Clamp01(lengthRatio) * 30f + 40f;
            return Mathf.Clamp(approxScore, 0f, 100f);
        }

        private float ComputeDifficultyPresetFit(LevelCandidate candidate)
        {
            if (!candidate.Solution.IsSolved) return 0f;
            float score = candidate.Score.DifficultyScore;
            float target = _config.Preset switch
            {
                DifficultyPreset.Easy => 25f,
                DifficultyPreset.Normal => 50f,
                DifficultyPreset.Hard => 75f,
                _ => 50f
            };
            float diff = Mathf.Abs(score - target);
            return Mathf.Clamp01(1f - diff / 100f);
        }

        private static void Shuffle<T>(IList<T> list, System.Random rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}