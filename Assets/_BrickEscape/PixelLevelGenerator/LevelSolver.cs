// Solver doc lap Unity physics: mo phong dung rule Brick Escape o muc grid/footprint.
// Tham chieu: TAI_LIEU_THIET_KE_TOOL_PIXEL_TO_RAW_LEVEL.txt muc 10.
//
// LOP NAY LA C# THUONG: khong dung EditorWindow/GameObject/Physics2D, de co the unit test
// nhanh, deterministic, chay hang tram seed ma khong can mo scene (muc 3.2 tai lieu thiet ke).
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using BrickEscape.PixelLevelGenerator.Model;

namespace BrickEscape.PixelLevelGenerator
{
    public class SolverBudget
    {
        public int MaxNodes = 200_000;
        public float MaxSeconds = 2.0f;
    }

    public static class LevelSolver
    {
        private class SolverContext
        {
            public int Width;
            public int Height;
            public Dictionary<int, BlockPlacement> PlacementsById;
            public List<int> OrderedIds; // dinh nghia bit position
            public Dictionary<int, int> BitIndexByPlacementId;
            public Dictionary<int, List<Vector2Int>> LeadingFrontierByPlacementId;
            public Dictionary<int, Vector2Int> StepByPlacementId;
            public Dictionary<Vector2Int, int> OccupiedCellOwner; // cell -> placementId (full occupancy, static per placement)
            public Dictionary<int, HashSet<Vector2Int>> CellsByPlacementId;

            public Stopwatch Timer;
            public SolverBudget Budget;
            public int NodesVisited;
            public Dictionary<ulong, bool> Memo = new Dictionary<ulong, bool>();
            public bool UseBitmask;
        }

        public static SolutionProof Solve(LevelCandidate candidate, SolverBudget budget)
        {
            var proof = new SolutionProof();
            var placements = candidate.Blocks;

            if (placements.Count == 0)
            {
                proof.IsSolved = true; // board rong van la mot "level" hop le theo goc do solver
                proof.RemovalOrder = new List<int>();
                return proof;
            }

            if (placements.Count > 63)
            {
                proof.IsSolved = false;
                proof.FailureReason = "Qua nhieu block (>63) cho bitmask solver trong phien ban nay.";
                return proof;
            }

            var ctx = BuildContext(candidate, budget);

            ulong fullMask = 0;
            for (int i = 0; i < ctx.OrderedIds.Count; i++) fullMask |= (1UL << i);

            var removalOrder = new List<int>();
            bool solved = SolveRecursive(ctx, fullMask, removalOrder);

            proof.IsSolved = solved && !TimedOut(ctx);
            proof.SearchNodes = ctx.NodesVisited;
            proof.BudgetExceeded = TimedOut(ctx) && !solved;

            if (proof.IsSolved)
            {
                proof.RemovalOrder = removalOrder;
                proof.SolutionCountCapped = CountSolutionsCapped(ctx, fullMask, 20);
            }
            else if (proof.BudgetExceeded)
            {
                proof.FailureReason = $"Solver vuot budget ({ctx.NodesVisited} nodes / {ctx.Timer.Elapsed.TotalSeconds:F2}s).";
            }
            else
            {
                proof.FailureReason = "Khong tim thay chuoi click nao dua board ve rong (UNSOLVABLE).";
            }

            return proof;
        }

        private static bool TimedOut(SolverContext ctx) =>
            ctx.NodesVisited >= ctx.Budget.MaxNodes || ctx.Timer.Elapsed.TotalSeconds >= ctx.Budget.MaxSeconds;

        private static SolverContext BuildContext(LevelCandidate candidate, SolverBudget budget)
        {
            var ctx = new SolverContext
            {
                Width = candidate.GridSize.x,
                Height = candidate.GridSize.y,
                PlacementsById = candidate.Blocks.ToDictionary(b => b.PlacementId),
                OrderedIds = candidate.Blocks.Select(b => b.PlacementId).OrderBy(x => x).ToList(),
                Timer = Stopwatch.StartNew(),
                Budget = budget ?? new SolverBudget(),
                CellsByPlacementId = new Dictionary<int, HashSet<Vector2Int>>(),
                LeadingFrontierByPlacementId = new Dictionary<int, List<Vector2Int>>(),
                StepByPlacementId = new Dictionary<int, Vector2Int>(),
                BitIndexByPlacementId = new Dictionary<int, int>(),
                UseBitmask = true
            };

            for (int i = 0; i < ctx.OrderedIds.Count; i++)
                ctx.BitIndexByPlacementId[ctx.OrderedIds[i]] = i;

            foreach (var placement in candidate.Blocks)
            {
                var cells = new HashSet<Vector2Int>(placement.OccupiedCells ?? Array.Empty<Vector2Int>());
                ctx.CellsByPlacementId[placement.PlacementId] = cells;

                var step = DirectionUtil.GridStepFor(placement.Move);
                ctx.StepByPlacementId[placement.PlacementId] = step;

                var frontier = new List<Vector2Int>();
                foreach (var c in cells)
                {
                    if (!cells.Contains(c + step)) frontier.Add(c);
                }
                ctx.LeadingFrontierByPlacementId[placement.PlacementId] = frontier;
            }

            return ctx;
        }

        /// <summary>
        /// True neu placement co the thoat ngay bay gio, cho truoc tap active (bitmask).
        /// </summary>
        private static bool CanExit(SolverContext ctx, int placementId, ulong activeMask)
        {
            var step = ctx.StepByPlacementId[placementId];
            if (step == Vector2Int.zero) return false; // BlockMovement.None khong the thoat

            var ownCells = ctx.CellsByPlacementId[placementId];
            foreach (var start in ctx.LeadingFrontierByPlacementId[placementId])
            {
                var pos = start + step;
                while (InBounds(ctx, pos))
                {
                    // co block active khac dung o pos?
                    foreach (var otherId in ctx.OrderedIds)
                    {
                        if (otherId == placementId) continue;
                        int bit = ctx.BitIndexByPlacementId[otherId];
                        if ((activeMask & (1UL << bit)) == 0) continue; // block da bi remove
                        if (ctx.CellsByPlacementId[otherId].Contains(pos))
                            return false; // bi chan
                    }
                    pos += step;
                }
            }
            return true;
        }

        private static bool InBounds(SolverContext ctx, Vector2Int pos) =>
            pos.x >= 0 && pos.x < ctx.Width && pos.y >= 0 && pos.y < ctx.Height;

        private static bool SolveRecursive(SolverContext ctx, ulong activeMask, List<int> removalOrderOut)
        {
            if (activeMask == 0) return true;
            if (TimedOut(ctx)) return false;

            if (ctx.Memo.TryGetValue(activeMask, out bool cached) && !cached)
                return false; // biet chac fail tu state nay

            ctx.NodesVisited++;

            var movable = new List<int>();
            foreach (var id in ctx.OrderedIds)
            {
                int bit = ctx.BitIndexByPlacementId[id];
                if ((activeMask & (1UL << bit)) == 0) continue;
                if (CanExit(ctx, id, activeMask)) movable.Add(id);
            }

            // heuristic: uu tien block co exit ro rang truoc (forced move truoc), sort theo id de deterministic.
            foreach (var id in movable)
            {
                int bit = ctx.BitIndexByPlacementId[id];
                ulong nextMask = activeMask & ~(1UL << bit);
                if (SolveRecursive(ctx, nextMask, removalOrderOut))
                {
                    removalOrderOut.Add(id);
                    return true;
                }
                if (TimedOut(ctx)) return false;
            }

            ctx.Memo[activeMask] = false;
            return false;
        }

        /// <summary>
        /// Dem so loi giai (tu goc) toi cap gioi han, de uoc luong branching/uniqueness (muc 10.2).
        /// Khong enumerate vo han; dung DFS gioi han so ket qua tra ve.
        /// </summary>
        private static int CountSolutionsCapped(SolverContext ctx, ulong fullMask, int cap)
        {
            int found = 0;
            var stack = new Stack<ulong>();
            stack.Push(fullMask);
            var visitedForCount = new HashSet<ulong>();
            int guardNodes = 0;
            const int guardLimit = 50_000;

            void Recurse(ulong mask)
            {
                if (found >= cap || guardNodes >= guardLimit || TimedOut(ctx)) return;
                guardNodes++;
                if (mask == 0) { found++; return; }
                if (!visitedForCount.Add(mask)) { /* still allow revisits for counting different paths, no strict memo here */ }

                foreach (var id in ctx.OrderedIds)
                {
                    if (found >= cap || guardNodes >= guardLimit || TimedOut(ctx)) return;
                    int bit = ctx.BitIndexByPlacementId[id];
                    if ((mask & (1UL << bit)) == 0) continue;
                    if (!CanExit(ctx, id, mask)) continue;
                    Recurse(mask & ~(1UL << bit));
                }
            }

            Recurse(fullMask);
            return found;
        }
    }
}