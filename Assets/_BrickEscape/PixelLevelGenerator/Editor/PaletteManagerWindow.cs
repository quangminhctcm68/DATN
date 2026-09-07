using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BrickEscape.PixelLevelGenerator.Model;

namespace BrickEscape.PixelLevelGenerator
{
    public class PaletteManagerWindow : EditorWindow
    {
        public List<PaletteEntry> Entries =
            new List<PaletteEntry>();

        public RawColorData RawColorDataAsset;

        public ScriptableObject DataCollectionAsset;

        public System.Action OnApplied;

        private Vector2 _scroll;

        private ColorPaletteService _service =
            new ColorPaletteService();

        // ============================================================
        // OPEN
        // ============================================================

        public static void Open(
            List<PaletteEntry> entries,
            RawColorData rawColorData,
            ScriptableObject dataCollection,
            System.Action onApplied)
        {
            var window =
                GetWindow<PaletteManagerWindow>(
                    true,
                    "Palette Manager"
                );

            window.Entries =
                entries ?? new List<PaletteEntry>();

            window.RawColorDataAsset =
                rawColorData;

            window.DataCollectionAsset =
                dataCollection;

            window.OnApplied =
                onApplied;

            window.minSize =
                new Vector2(850, 500);

            window.ShowUtility();

            window.Repaint();
        }

        // ============================================================
        // GUI
        // ============================================================

        private void OnGUI()
        {
            DrawAssetReferences();

            EditorGUILayout.Space(5);

            if (RawColorDataAsset == null)
            {
                EditorGUILayout.HelpBox(
                    "Chua gan RawColorData.\n\n" +
                    "Hay gan asset RawColorData vao ObjectField ben tren.",
                    MessageType.Error
                );
            }

            DrawToolbar();

            EditorGUILayout.Space(5);

            DrawHeader();

            _scroll =
                EditorGUILayout.BeginScrollView(
                    _scroll
                );

            if (Entries == null ||
                Entries.Count == 0)
            {
                EditorGUILayout.HelpBox(
                    "Khong co palette entry.\n" +
                    "Hay Import Image truoc.",
                    MessageType.Info
                );
            }
            else
            {
                foreach (PaletteEntry entry in Entries)
                {
                    if (entry == null)
                        continue;

                    DrawRow(entry);
                }
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(5);

            DrawBottomBar();
        }

        // ============================================================
        // ASSET REFERENCES
        // ============================================================

        private void DrawAssetReferences()
        {
            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField(
                "Production Data",
                EditorStyles.boldLabel
            );

            RawColorDataAsset =
                (RawColorData)EditorGUILayout.ObjectField(
                    "RawColorData",
                    RawColorDataAsset,
                    typeof(RawColorData),
                    false
                );

            DataCollectionAsset =
                (ScriptableObject)EditorGUILayout.ObjectField(
                    "DataCollection",
                    DataCollectionAsset,
                    typeof(ScriptableObject),
                    false
                );

            EditorGUILayout.EndVertical();
        }

        // ============================================================
        // TOOLBAR
        // ============================================================

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(
                EditorStyles.toolbar
            );

            GUI.enabled =
                RawColorDataAsset != null;

            if (GUILayout.Button(
                    "Load Current Color Data",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(190)))
            {
                LoadCurrentColorData();
            }

            GUI.enabled = true;

            GUILayout.FlexibleSpace();

            int resolved =
                Entries.Count(x => x != null &&
                                   x.IsResolved);

            int total =
                Entries.Count;

            GUILayout.Label(
                $"Resolved: {resolved}/{total}",
                EditorStyles.toolbarButton
            );

            EditorGUILayout.EndHorizontal();
        }

        // ============================================================
        // LOAD
        // ============================================================

        private void LoadCurrentColorData()
        {
            if (RawColorDataAsset == null)
            {
                EditorUtility.DisplayDialog(
                    "Palette Manager",
                    "Chua gan RawColorData.",
                    "OK"
                );

                return;
            }

            _service.RawColorDataAsset =
                RawColorDataAsset;

            _service.DataCollectionAsset =
                DataCollectionAsset;

            List<ColorRawEntry> existing =
                _service.LoadCurrentColorData();

            int matched = 0;

            foreach (PaletteEntry entry in Entries)
            {
                if (entry == null)
                    continue;

                ColorRawEntry match =
                    existing.FirstOrDefault(
                        x => ColorsApproxEqual(
                            x.Color,
                            entry.SourceColor
                        )
                    );

                if (match != null)
                {
                    entry.ExistingColorId =
                        match.ColorId;

                    entry.ResolvedColorId =
                        match.ColorId;

                    matched++;
                }
                else
                {
                    entry.ExistingColorId =
                        null;

                    entry.ResolvedColorId =
                        -1;
                }
            }

            Debug.Log(
                $"[Pixel Level Generator] " +
                $"Loaded {existing.Count} RawColors. " +
                $"Matched {matched}/{Entries.Count} palette entries."
            );

            Repaint();
        }

        // ============================================================
        // HEADER
        // ============================================================

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(
                EditorStyles.toolbar
            );

            GUILayout.Label(
                "Preview",
                GUILayout.Width(60)
            );

            GUILayout.Label(
                "Source",
                GUILayout.Width(90)
            );

            GUILayout.Label(
                "Pixels",
                GUILayout.Width(60)
            );

            GUILayout.Label(
                "Current ID",
                GUILayout.Width(80)
            );

            GUILayout.Label(
                "Resolved ID",
                GUILayout.Width(80)
            );

            GUILayout.Label(
                "Hex",
                GUILayout.Width(110)
            );

            GUILayout.Label(
                "Action",
                GUILayout.Width(110)
            );

            GUILayout.Label(
                "Status"
            );

            EditorGUILayout.EndHorizontal();
        }

        // ============================================================
        // ROW
        // ============================================================

        private void DrawRow(
            PaletteEntry entry)
        {
            EditorGUILayout.BeginHorizontal("box");

            // Preview
            Rect swatch =
                GUILayoutUtility.GetRect(
                    24,
                    18,
                    GUILayout.Width(60)
                );

            EditorGUI.DrawRect(
                swatch,
                entry.SourceColor
            );

            // Source
            GUILayout.Label(
                ColorUtility.ToHtmlStringRGB(
                    entry.SourceColor
                ),
                GUILayout.Width(90)
            );

            // Pixel count
            GUILayout.Label(
                entry.PixelCount.ToString(),
                GUILayout.Width(60)
            );

            // Existing ID
            GUILayout.Label(
                entry.ExistingColorId.HasValue
                    ? entry.ExistingColorId.Value.ToString()
                    : "--",
                GUILayout.Width(80)
            );

            // Resolved ID
            GUILayout.Label(
                entry.IsResolved
                    ? entry.ResolvedColorId.ToString()
                    : "--",
                GUILayout.Width(80)
            );

            // Hex
            string oldHex =
                entry.EditableHex;

            entry.EditableHex =
                EditorGUILayout.TextField(
                    entry.EditableHex,
                    GUILayout.Width(110)
                );

            if (oldHex != entry.EditableHex)
            {
                // User đang sửa hex.
                // Không được thay SourceColor.
                // SourceColor là màu gốc của image.
                entry.WillAddToRawColorData =
                    !entry.ExistingColorId.HasValue;
            }

            // Action
            if (entry.ExistingColorId.HasValue)
            {
                GUILayout.Label(
                    "Update existing",
                    GUILayout.Width(110)
                );
            }
            else
            {
                entry.WillAddToRawColorData =
                    EditorGUILayout.ToggleLeft(
                        "Add new",
                        entry.WillAddToRawColorData,
                        GUILayout.Width(110)
                    );
            }

            // Status
            string status;

            if (entry.IsResolved)
            {
                status =
                    $"Mapped ID {entry.ResolvedColorId}";
            }
            else if (entry.WillAddToRawColorData)
            {
                status =
                    "Will Add";
            }
            else
            {
                status =
                    "Needs Mapping";
            }

            Color statusColor;

            if (entry.IsResolved)
                statusColor = Color.green;
            else if (entry.WillAddToRawColorData)
                statusColor = Color.yellow;
            else
                statusColor = Color.red;

            Color oldGuiColor =
                GUI.color;

            GUI.color =
                statusColor;

            GUILayout.Label(status);

            GUI.color =
                oldGuiColor;

            EditorGUILayout.EndHorizontal();
        }

        // ============================================================
        // BOTTOM
        // ============================================================

        private void DrawBottomBar()
        {
            EditorGUILayout.BeginHorizontal();

            int unresolved =
                Entries.Count(
                    e => e != null &&
                         !e.IsResolved &&
                         !e.WillAddToRawColorData
                );

            GUI.enabled =
                RawColorDataAsset != null &&
                Entries.Count > 0 &&
                Entries.Any(
                    e => e != null &&
                         (e.IsResolved ||
                          e.WillAddToRawColorData)
                );

            if (GUILayout.Button(
                    "Update RawColorData",
                    GUILayout.Height(30)))
            {
                ApplyChanges();
            }

            GUI.enabled = true;

            if (GUILayout.Button(
                    "Reload / Re-map",
                    GUILayout.Height(30),
                    GUILayout.Width(120)))
            {
                LoadCurrentColorData();
            }

            if (GUILayout.Button(
                    "Close",
                    GUILayout.Height(30),
                    GUILayout.Width(80)))
            {
                Close();
            }

            EditorGUILayout.EndHorizontal();

            if (unresolved > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{unresolved} mau chua duoc mapping.",
                    MessageType.Warning
                );
            }
        }

        // ============================================================
        // APPLY
        // ============================================================

        private void ApplyChanges()
        {
            if (RawColorDataAsset == null)
            {
                EditorUtility.DisplayDialog(
                    "Palette Manager",
                    "Chua gan RawColorData.",
                    "OK"
                );

                return;
            }

            bool confirm =
                EditorUtility.DisplayDialog(
                    "Update RawColorData",
                    "Thao tac nay se:\n\n" +
                    "• Update mau hien tai theo ID\n" +
                    "• Add mau moi neu tick Add New\n" +
                    "• Rebuild DataCollection\n" +
                    "• Save Asset\n\n" +
                    "Tiep tuc?",
                    "Update",
                    "Cancel"
                );

            if (!confirm)
                return;

            _service.RawColorDataAsset =
                RawColorDataAsset;

            _service.DataCollectionAsset =
                DataCollectionAsset;

            List<PaletteEntry> rows =
                Entries
                    .Where(
                        e => e != null &&
                             (e.ExistingColorId.HasValue ||
                              e.WillAddToRawColorData)
                    )
                    .ToList();

            PaletteUpdateResult result =
                _service.UpdateRawColorData(
                    rows
                );

            foreach (string log in result.Log)
            {
                Debug.Log(
                    "[Pixel Level Generator] " +
                    log
                );
            }

            if (!result.Success)
            {
                EditorUtility.DisplayDialog(
                    "Palette Update Failed",
                    string.Join(
                        "\n",
                        result.Log
                    ),
                    "OK"
                );

                return;
            }

            // ========================================================
            // QUAN TRỌNG:
            // Reload lại từ RawColorData thật.
            // ========================================================

            LoadCurrentColorData();

            // Callback về main window
            OnApplied?.Invoke();

            Repaint();

            EditorUtility.DisplayDialog(
                "Palette Updated",
                $"Updated: {result.UpdatedCount}\n" +
                $"Added: {result.AddedCount}\n\n" +
                "Palette da duoc reload va mapping lai.",
                "OK"
            );
        }

        // ============================================================
        // COLOR MATCH
        // ============================================================

        private static bool ColorsApproxEqual(
            Color32 a,
            Color32 b,
            float tolerance = 0.5f / 255f)
        {
            return
                Mathf.Abs(a.r - b.r) / 255f <= tolerance &&
                Mathf.Abs(a.g - b.g) / 255f <= tolerance &&
                Mathf.Abs(a.b - b.b) / 255f <= tolerance;
        }
    }
}