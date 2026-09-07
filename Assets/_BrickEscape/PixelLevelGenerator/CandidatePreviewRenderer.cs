// Ve image goc, grid, footprint block, arrow, diff va solution step. Khong spawn GameObject.
// Tham chieu: TAI_LIEU_THIET_KE_TOOL_PIXEL_TO_RAW_LEVEL.txt muc "Revision 1.1" phan 2;
// TAI_LIEU_THIET_KE_UI_TOOL_PIXEL_TO_RAW_LEVEL.txt muc 5.
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BrickEscape.PixelLevelGenerator.Model;

namespace BrickEscape.PixelLevelGenerator
{
    public enum PreviewTab
    {
        SourcePixels,
        LevelLayout,
        Difference,
        SolutionReplay
    }

    public class PreviewViewState
    {
        public float Zoom = 16f; // px per cell
        public Vector2 Pan = Vector2.zero;
        public bool ShowGrid = true;
        public bool ShowOrigin = true;
        public bool ShowFootprint = true;
        public bool ShowExitRays;
        public bool ShowCoordinates;
        public int SolutionStep;
        public bool OnlyMismatches;
        public float OverlayOpacity = 0.7f;
    }

    public static class CandidatePreviewRenderer
    {
        private static readonly Color VoidColor = new Color(0.12f, 0.12f, 0.12f, 1f);
        private static readonly Color BoardEmptyColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        private static readonly Color OutlineColor = Color.cyan;
        private static readonly Color MatchColor = new Color(0.2f, 0.8f, 0.3f, 1f);
        private static readonly Color WrongColorColor = new Color(0.95f, 0.85f, 0.15f, 1f);
        private static readonly Color VoidOverlapColor = new Color(0.9f, 0.2f, 0.2f, 1f);
        private static readonly Color UncoveredColor = new Color(0.55f, 0.55f, 0.55f, 1f);

        public static void DrawSourcePixels(Rect area, ImageGrid grid, PreviewViewState view)
        {
            if (grid == null) return;
            GUI.BeginClip(area);
            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    var cell = grid.Cells[x, y];
                    var rect = CellRect(x, y, view);
                    EditorGUI.DrawRect(rect, cell.IsVoid ? VoidColor : (Color)cell.SourceColor);
                }
            }
            if (view.ShowGrid) DrawGridLines(grid.Width, grid.Height, view);
            GUI.EndClip();
        }

        public static void DrawLevelLayout(Rect area, LevelCandidate candidate, Dictionary<int, BlockDefinition> catalog,
            Dictionary<int, Color32> colorLookup, PreviewViewState view, int? selectedPlacementId)
        {
            if (candidate == null) return;
            GUI.BeginClip(area);

            int w = candidate.GridSize.x, h = candidate.GridSize.y;
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    var rect = CellRect(x, y, view);
                    bool isBoard = candidate.BoardMask[x, y];
                    EditorGUI.DrawRect(rect, isBoard ? BoardEmptyColor : VoidColor);
                }

            foreach (var placement in candidate.Blocks)
            {
                Color fill = colorLookup != null && colorLookup.TryGetValue(placement.ColorId, out var c32)
                    ? (Color)c32 : Color.magenta;

                Rect bbox = FootprintBoundingRect(placement, view);
                EditorGUI.DrawRect(bbox, new Color(fill.r, fill.g, fill.b, 0.85f));

                bool selected = selectedPlacementId.HasValue && selectedPlacementId.Value == placement.PlacementId;
                DrawOutline(bbox, selected ? Color.cyan : Color.black, selected ? 3f : 1.5f);

                if (view.ShowOrigin)
                {
                    var originRect = CellRect(placement.Origin.x, placement.Origin.y, view);
                    GUI.Label(originRect, placement.PlacementId.ToString(), CenteredLabelStyle());
                }

                DrawArrow(placement, view);
            }

            if (view.ShowGrid) DrawGridLines(w, h, view);
            GUI.EndClip();
        }

        public static void DrawDifference(Rect area, LevelCandidate candidate, ImageGrid sourceGrid, PreviewViewState view)
        {
            if (candidate == null || sourceGrid == null) return;
            GUI.BeginClip(area);

            int w = candidate.GridSize.x, h = candidate.GridSize.y;
            var coveredBy = new int?[w, h];
            foreach (var b in candidate.Blocks)
                foreach (var c in b.OccupiedCells)
                    if (c.x >= 0 && c.x < w && c.y >= 0 && c.y < h) coveredBy[c.x, c.y] = b.ColorId;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var rect = CellRect(x, y, view);
                    bool isBoard = candidate.BoardMask[x, y];
                    int desired = candidate.DesiredColorId[x, y];
                    int? actual = coveredBy[x, y];

                    Color c;
                    bool isMismatch;
                    if (!isBoard)
                    {
                        c = actual.HasValue ? VoidOverlapColor : VoidColor;
                        isMismatch = actual.HasValue;
                    }
                    else if (actual.HasValue && actual.Value == desired)
                    {
                        c = MatchColor;
                        isMismatch = false;
                    }
                    else if (actual.HasValue)
                    {
                        c = WrongColorColor;
                        isMismatch = true;
                    }
                    else
                    {
                        c = UncoveredColor;
                        isMismatch = desired >= 0;
                    }

                    if (view.OnlyMismatches && !isMismatch) continue;
                    c.a = view.OverlayOpacity;
                    EditorGUI.DrawRect(rect, c);
                }
            }
            if (view.ShowGrid) DrawGridLines(w, h, view);
            GUI.EndClip();
        }

        public static void DrawSolutionReplay(Rect area, LevelCandidate candidate, Dictionary<int, Color32> colorLookup, PreviewViewState view)
        {
            if (candidate?.Solution == null || !candidate.Solution.IsSolved)
            {
                GUI.Label(area, $"Khong the replay: {candidate?.Solution?.FailureReason ?? "chua solve."}");
                return;
            }

            GUI.BeginClip(area);
            int w = candidate.GridSize.x, h = candidate.GridSize.y;
            var removedUpToStep = candidate.Solution.RemovalOrder.Take(view.SolutionStep).ToHashSet();

            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    EditorGUI.DrawRect(CellRect(x, y, view), candidate.BoardMask[x, y] ? BoardEmptyColor : VoidColor);

            int? currentStepBlockId = view.SolutionStep < candidate.Solution.RemovalOrder.Count
                ? candidate.Solution.RemovalOrder[view.SolutionStep]
                : (int?)null;

            foreach (var placement in candidate.Blocks)
            {
                bool removed = removedUpToStep.Contains(placement.PlacementId);
                Color fill = colorLookup != null && colorLookup.TryGetValue(placement.ColorId, out var c32)
                    ? (Color)c32 : Color.magenta;
                float alpha = removed ? 0.2f : 0.9f;
                var bbox = FootprintBoundingRect(placement, view);
                EditorGUI.DrawRect(bbox, new Color(fill.r, fill.g, fill.b, alpha));

                bool isCurrent = currentStepBlockId.HasValue && currentStepBlockId.Value == placement.PlacementId;
                DrawOutline(bbox, isCurrent ? Color.blue : Color.black, isCurrent ? 3f : 1f);
            }

            if (view.ShowGrid) DrawGridLines(w, h, view);
            GUI.EndClip();
        }

        private static Rect CellRect(int x, int y, PreviewViewState view)
        {
            return new Rect(view.Pan.x + x * view.Zoom, view.Pan.y + y * view.Zoom, view.Zoom, view.Zoom);
        }

        private static Rect FootprintBoundingRect(BlockPlacement placement, PreviewViewState view)
        {
            int minX = placement.OccupiedCells.Min(c => c.x);
            int maxX = placement.OccupiedCells.Max(c => c.x);
            int minY = placement.OccupiedCells.Min(c => c.y);
            int maxY = placement.OccupiedCells.Max(c => c.y);
            var topLeft = CellRect(minX, minY, view).position;
            float width = (maxX - minX + 1) * view.Zoom;
            float height = (maxY - minY + 1) * view.Zoom;
            return new Rect(topLeft.x, topLeft.y, width, height);
        }

        private static void DrawOutline(Rect r, Color color, float thickness)
        {
            Handles.BeginGUI();
            Handles.color = color;
            Vector3 a = new Vector3(r.xMin, r.yMin);
            Vector3 b = new Vector3(r.xMax, r.yMin);
            Vector3 c = new Vector3(r.xMax, r.yMax);
            Vector3 d = new Vector3(r.xMin, r.yMax);
            Handles.DrawAAPolyLine(thickness, a, b, c, d, a);
            Handles.EndGUI();
        }

        private static void DrawGridLines(int width, int height, PreviewViewState view)
        {
            Handles.BeginGUI();
            Handles.color = new Color(1f, 1f, 1f, 0.15f);
            for (int x = 0; x <= width; x++)
            {
                float px = view.Pan.x + x * view.Zoom;
                Handles.DrawLine(new Vector3(px, view.Pan.y), new Vector3(px, view.Pan.y + height * view.Zoom));
            }
            for (int y = 0; y <= height; y++)
            {
                float py = view.Pan.y + y * view.Zoom;
                Handles.DrawLine(new Vector3(view.Pan.x, py), new Vector3(view.Pan.x + width * view.Zoom, py));
            }
            Handles.EndGUI();
        }

        private static void DrawArrow(BlockPlacement placement, PreviewViewState view)
        {
            var step = DirectionUtil.GridStepFor(placement.Move);
            if (step == Vector2Int.zero) return;

            // Anchor cell: joint index tra ve tu ArrowStartIndex, lay vi tri gan dung bang origin +
            // offset trung binh footprint theo huong arrow (xap xi hien thi, khong anh huong data export).
            var bbox = FootprintBoundingRect(placement, view);
            Vector2 center = bbox.center;
            Vector2 dir = new Vector2(step.x, step.y).normalized;
            Vector2 tip = center + dir * (Mathf.Max(bbox.width, bbox.height) * 0.5f + view.Zoom * 0.6f);

            Handles.BeginGUI();
            Handles.color = Color.white;
            Handles.DrawAAPolyLine(3f, (Vector3)center, (Vector3)tip);
            // dau mui ten don gian
            Vector2 perp = new Vector2(-dir.y, dir.x) * (view.Zoom * 0.15f);
            Vector2 back = tip - dir * (view.Zoom * 0.3f);
            Handles.DrawAAConvexPolygon((Vector3)tip, (Vector3)(back + perp), (Vector3)(back - perp));
            Handles.EndGUI();
        }

        private static GUIStyle _centeredLabelStyle;
        private static GUIStyle CenteredLabelStyle()
        {
            if (_centeredLabelStyle == null)
            {
                _centeredLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };
            }
            return _centeredLabelStyle;
        }
    }
}