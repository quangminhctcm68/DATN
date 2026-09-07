// PixelLevelGeneratorWindow.cs
//
// EditorWindow chinh: dieu phoi UI, session, preview, palette va export.
//
// Pipeline:
// Image
//   -> ImageGrid
//   -> PaletteEntry
//   -> RawColorData.rawColors
//   -> Resolve SourceColor -> Color ID
//   -> Quota
//   -> Generate Candidates
//   -> Validate
//   -> Export RawLevelData
//
// IMPORTANT:
// Generator KHONG dung ColorPaletteService de quyet dinh palette da resolve hay chua.
// RawColorData.rawColors la source of truth cho Color ID production.
//
// RawColor production:
// [System.Serializable]
// public class RawColor
// {
//     public int ID;
//     public string Color;
// }
//
// public class RawColorData : ScriptableObject
// {
//     public List<RawColor> rawColors = new List<RawColor>();
// }

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;
using UnityEngine;

using BrickEscape.PixelLevelGenerator.Model;

namespace BrickEscape.PixelLevelGenerator
{
    public class PixelLevelGeneratorWindow : EditorWindow
    {
        // =====================================================================================
        // PRODUCTION ASSETS
        // =====================================================================================

        [SerializeField]
        private Texture2D _sourceTexture;

        [SerializeField]
        private RawColorData _rawColorDataAsset;

        [SerializeField]
        private ScriptableObject _dataCollectionAsset;

        [SerializeField]
        private ScriptableObject _blocksHolderForBlockDictionary;

        [SerializeField]
        private string _rawLevelDataTypeName = "RawLevelData";

        [SerializeField]
        private string _rawLevelRowTypeName = "RawLevel";


        // =====================================================================================
        // IMPORT
        // =====================================================================================

        private PixelGridImportConfig _importConfig = new PixelGridImportConfig();

        private ImageGrid _imageGrid;

        private List<string> _importErrors = new List<string>();


        // =====================================================================================
        // PALETTE
        // =====================================================================================

        private List<PaletteEntry> _paletteEntries = new List<PaletteEntry>();

        // Service van dung de BUILD PaletteEntry tu ImageGrid.
        // Khong dung no lam source of truth cho Color ID.
        private ColorPaletteService _paletteService = new ColorPaletteService();


        // =====================================================================================
        // BLOCK CATALOG
        // =====================================================================================

        private Dictionary<int, BlockDefinition> _catalog =
            new Dictionary<int, BlockDefinition>();

        private List<ValidationIssue> _catalogIssues =
            new List<ValidationIssue>();


        // =====================================================================================
        // LEVEL METADATA / GENERATION CONFIG
        // =====================================================================================

        private int _levelId = 71;

        private string _outputFolder =
            "Assets/_BrickEscape/Data/Raws";

        private string _assetName =
            "RawLevel71.asset";

        private bool _assetNameManuallyEdited;

        private float _cellSize = 1f;

        private bool _hardLevel;

        private GenerationConfig _config =
            new GenerationConfig();


        // =====================================================================================
        // CANDIDATES
        // =====================================================================================

        private List<LevelCandidate> _candidates =
            new List<LevelCandidate>();

        private int _selectedCandidateIndex = -1;

        private string _lastGenerateStatus = "--";

        private bool _isGenerating;
        

        // =====================================================================================
        // PREVIEW
        // =====================================================================================

        private PreviewTab _activeTab =
            PreviewTab.SourcePixels;

        private PreviewViewState _view =
            new PreviewViewState();

        private int? _selectedPlacementId;


        // =====================================================================================
        // SESSION
        // =====================================================================================

        private string _sessionName =
            "Untitled";

        private bool _dirty;


        private Vector2 _leftScroll;
        private Vector2 _rightScroll;


        // =====================================================================================
        // FOLDOUTS
        // =====================================================================================
        private bool _foldProduction = true;
        private bool _foldImage = true;
        private bool _foldPalette = true;
        private bool _foldMeta = true;
        private bool _foldDifficulty = true;
        private bool _foldGenerate = true;
        private bool _foldAdvanced;


        // =====================================================================================
        // OPEN
        // =====================================================================================

        [MenuItem("Tools/Brick Escape/Pixel Level Generator")]
        public static void Open()
        {
            var win = GetWindow<PixelLevelGeneratorWindow>(
                "Pixel Level Generator");

            win.minSize = new Vector2(1280, 760);

            win.Show();
        }


        // =====================================================================================
        // GUI
        // =====================================================================================

        private void OnGUI()
        {
            DrawToolbar();

            EditorGUILayout.BeginHorizontal();

            DrawLeftPanel();

            DrawCenterPanel();

            DrawRightPanel();

            EditorGUILayout.EndHorizontal();

            DrawStatusBar();
        }


        // =====================================================================================
        // TOOLBAR
        // =====================================================================================

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(
                    "New Session",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(100)))
            {
                NewSession();
            }

            if (GUILayout.Button(
                    "Load",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(60)))
            {
                LoadSession();
            }

            if (GUILayout.Button(
                    "Save",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(60)))
            {
                SaveSession(false);
            }

            if (GUILayout.Button(
                    "Save As",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(80)))
            {
                SaveSession(true);
            }

            GUILayout.Space(12);

            bool hasUnresolved =
                _paletteEntries.Any(e => !e.IsResolved);

            var previousGuiColor = GUI.color;

            if (hasUnresolved)
            {
                GUI.color = new Color(1f, 0.6f, 0.6f);
            }

            if (GUILayout.Button(
                    "Palette Manager",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(130)))
            {
                OpenPaletteManager();
            }

            GUI.color = previousGuiColor;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(
                    "Help",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(60)))
            {
                EditorUtility.DisplayDialog(
                    "Pixel Level Generator - Help",
                    "1. Import anh pixel.\n" +
                    "2. Resolve mau trong Palette Manager.\n" +
                    "3. Cau hinh Blocks per Color + level metadata.\n" +
                    "4. Generate Candidates.\n" +
                    "5. Review/Validate candidate.\n" +
                    "6. Export RawLevelData.\n" +
                    "7. Load All Raw Levels + Convert Level tren LevelData.asset.",
                    "OK");
            }

            EditorGUILayout.EndHorizontal();
        }


        // =====================================================================================
        // LEFT PANEL
        // =====================================================================================

        private void DrawLeftPanel()
        {
            EditorGUILayout.BeginVertical(
                GUILayout.Width(340));

            _leftScroll =
                EditorGUILayout.BeginScrollView(_leftScroll);

            DrawProductionAssetsFoldout();   // <-- MỚI THÊM

            DrawInputImageFoldout();

            DrawPaletteSummaryFoldout();

            DrawLevelMetadataFoldout();

            DrawDifficultyFoldout();

            DrawGenerateFoldout();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }

        // =====================================================================================
        // 0. PRODUCTION ASSETS
        // =====================================================================================

        private void DrawProductionAssetsFoldout()
        {
            _foldProduction = EditorGUILayout.Foldout(
                _foldProduction,
                "0. Production Assets",
                true);

            if (!_foldProduction)
                return;

            EditorGUILayout.BeginVertical("box");

            var newRawColorData =
                (RawColorData)EditorGUILayout.ObjectField(
                    "Raw Color Data",
                    _rawColorDataAsset,
                    typeof(RawColorData),
                    false);

            if (newRawColorData != _rawColorDataAsset)
            {
                _rawColorDataAsset = newRawColorData;
                _dirty = true;

                RefreshPaletteFromImage();
            }

            if (_rawColorDataAsset == null)
            {
                EditorGUILayout.HelpBox(
                    "Chua gan RawColorData. Khong the resolve mau / mapping Color ID.",
                    MessageType.Warning);
            }

            var newDataCollection =
                (ScriptableObject)EditorGUILayout.ObjectField(
                    "Data Collection",
                    _dataCollectionAsset,
                    typeof(ScriptableObject),
                    false);

            if (newDataCollection != _dataCollectionAsset)
            {
                _dataCollectionAsset = newDataCollection;
                _dirty = true;

                RebuildCatalog();
            }

            var newBlocksHolder =
                (ScriptableObject)EditorGUILayout.ObjectField(
                    "Blocks Holder",
                    _blocksHolderForBlockDictionary,
                    typeof(ScriptableObject),
                    false);

            if (newBlocksHolder != _blocksHolderForBlockDictionary)
            {
                _blocksHolderForBlockDictionary = newBlocksHolder;
                _dirty = true;
            }

            EditorGUILayout.EndVertical();
        }

        // =====================================================================================
        // 1. INPUT IMAGE
        // =====================================================================================

        private void DrawInputImageFoldout()
        {
            _foldImage = EditorGUILayout.Foldout(
                _foldImage,
                "1. Input Image",
                true);

            if (!_foldImage)
                return;

            EditorGUILayout.BeginVertical("box");

            var newTexture =
                (Texture2D)EditorGUILayout.ObjectField(
                    "Texture",
                    _sourceTexture,
                    typeof(Texture2D),
                    false);

            if (newTexture != _sourceTexture)
            {
                _sourceTexture = newTexture;
                _dirty = true;
            }

            if (_sourceTexture != null)
            {
                EditorGUILayout.LabelField(
                    "Source",
                    $"{_sourceTexture.width} x {_sourceTexture.height} px");

                var problems =
                    PixelGridImporter.CheckImportSettings(
                        _sourceTexture);

                if (problems.Count > 0)
                {
                    EditorGUILayout.HelpBox(
                        string.Join("\n", problems),
                        MessageType.Warning);

                    if (GUILayout.Button("Fix Import Settings"))
                    {
                        if (EditorUtility.DisplayDialog(
                                "Fix Import Settings",
                                "Se sua Read/Write, FilterMode=Point, tat mipmap va Uncompressed cho texture nay. Tiep tuc?",
                                "Fix",
                                "Cancel"))
                        {
                            PixelGridImporter.FixImportSettings(
                                _sourceTexture);
                        }
                    }
                }

                _importConfig.Mode =
                    (ImportMode)EditorGUILayout.EnumPopup(
                        "Import mode",
                        _importConfig.Mode);

                if (_importConfig.Mode ==
                    ImportMode.Downsample)
                {
                    _importConfig.DownsampleFactor =
                        EditorGUILayout.IntField(
                            "Downsample factor",
                            Mathf.Max(
                                1,
                                _importConfig.DownsampleFactor));
                }

                if (_importConfig.Mode ==
                    ImportMode.Crop)
                {
                    var r = _importConfig.CropRect;

                    r.x = EditorGUILayout.IntField(
                        "Crop X",
                        r.x);

                    r.y = EditorGUILayout.IntField(
                        "Crop Y",
                        r.y);

                    r.width = EditorGUILayout.IntField(
                        "Crop W",
                        r.width);

                    r.height = EditorGUILayout.IntField(
                        "Crop H",
                        r.height);

                    _importConfig.CropRect = r;
                }

                _importConfig.AlphaThreshold =
                    EditorGUILayout.Slider(
                        "Alpha threshold",
                        _importConfig.AlphaThreshold,
                        0f,
                        1f);

                _importConfig.BackgroundRule =
                    (BackgroundRule)EditorGUILayout.EnumPopup(
                        "Background rule",
                        _importConfig.BackgroundRule);

                if (_importConfig.BackgroundRule ==
                    BackgroundRule.SelectedColorIsVoid)
                {
                    _importConfig.VoidBackgroundColor =
                        EditorGUILayout.ColorField(
                            "Void color",
                            _importConfig.VoidBackgroundColor);
                }

                if (GUILayout.Button(
                        "Import / Reload Image",
                        GUILayout.Height(24)))
                {
                    ImportImage();
                }

                if (_imageGrid != null)
                {
                    EditorGUILayout.LabelField(
                        "Result grid",
                        $"{_imageGrid.Width} cols x {_imageGrid.Height} rows");

                    EditorGUILayout.LabelField(
                        "Board / Void",
                        $"{_imageGrid.BoardCellCount()} / {_imageGrid.VoidCellCount()}");
                }

                foreach (var err in _importErrors)
                {
                    EditorGUILayout.HelpBox(
                        err,
                        MessageType.Error);
                }
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "Chon anh pixel de bat dau.",
                    MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }


        // =====================================================================================
        // 2. PALETTE SUMMARY
        // =====================================================================================

        private void DrawPaletteSummaryFoldout()
        {
            _foldPalette = EditorGUILayout.Foldout(
                _foldPalette,
                "2. Color Palette",
                true);

            if (!_foldPalette)
                return;

            EditorGUILayout.BeginVertical("box");

            int total =
                _paletteEntries.Count;

            int resolved =
                _paletteEntries.Count(
                    e => e.IsResolved);

            EditorGUILayout.LabelField(
                "Image colors",
                total.ToString());

            EditorGUILayout.LabelField(
                "Resolved",
                $"{resolved}/{total}");

            if (total > 0 && resolved < total)
            {
                EditorGUILayout.HelpBox(
                    $"{total - resolved} mau chua duoc mapping.",
                    MessageType.Warning);
            }
            else if (total > 0)
            {
                EditorGUILayout.HelpBox(
                    "Tat ca mau da duoc mapping vao RawColorData.",
                    MessageType.Info);
            }

            EditorGUILayout.Space(4);

            foreach (var e in _paletteEntries.Take(6))
            {
                EditorGUILayout.BeginHorizontal();

                var rect =
                    GUILayoutUtility.GetRect(
                        16,
                        16,
                        GUILayout.Width(24));

                EditorGUI.DrawRect(
                    rect,
                    e.SourceColor);

                GUILayout.Label(
                    e.EditableHex,
                    GUILayout.Width(80));

                if (e.IsResolved)
                {
                    GUILayout.Label(
                        $"-> ID {e.ResolvedColorId}");
                }
                else
                {
                    GUILayout.Label(
                        "Needs mapping");
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(4);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Reload & Resolve"))
            {
                RefreshPaletteFromImage();

                _dirty = true;
            }

            if (GUILayout.Button("Open Palette Manager"))
            {
                OpenPaletteManager();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }


        // =====================================================================================
        // 3. LEVEL METADATA
        // =====================================================================================

        private void DrawLevelMetadataFoldout()
        {
            _foldMeta = EditorGUILayout.Foldout(
                _foldMeta,
                "3. Level Metadata",
                true);

            if (!_foldMeta)
                return;

            EditorGUILayout.BeginVertical("box");

            int newLevelId =
                EditorGUILayout.IntField(
                    "Level ID",
                    _levelId);

            if (newLevelId != _levelId)
            {
                _levelId =
                    Mathf.Max(
                        1,
                        newLevelId);

                if (!_assetNameManuallyEdited)
                {
                    _assetName =
                        $"RawLevel{_levelId}.asset";
                }

                _dirty = true;
            }

            EditorGUILayout.BeginHorizontal();

            _outputFolder =
                EditorGUILayout.TextField(
                    "Output folder",
                    _outputFolder);

            if (GUILayout.Button(
                    "Browse",
                    GUILayout.Width(60)))
            {
                string picked =
                    EditorUtility.OpenFolderPanel(
                        "Chon output folder",
                        _outputFolder,
                        "");

                if (!string.IsNullOrEmpty(picked))
                {
                    _outputFolder =
                        ToProjectRelativePath(
                            picked);
                }
            }

            EditorGUILayout.EndHorizontal();

            string newAssetName =
                EditorGUILayout.TextField(
                    "Asset name",
                    _assetName);

            if (newAssetName != _assetName)
            {
                _assetName =
                    newAssetName;

                _assetNameManuallyEdited = true;
            }

            _cellSize =
                EditorGUILayout.FloatField(
                    "Cell size",
                    _cellSize);

            _hardLevel =
                EditorGUILayout.Toggle(
                    "Hard level",
                    _hardLevel);

            if (RawLevelExporter.AssetExists(
                    _outputFolder,
                    _assetName))
            {
                EditorGUILayout.HelpBox(
                    $"{_assetName} da ton tai trong {_outputFolder}. Export se hoi Overwrite/Create Copy.",
                    MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }


        // =====================================================================================
        // 4. DIFFICULTY & BLOCKS
        // =====================================================================================

        private void DrawDifficultyFoldout()
        {
            _foldDifficulty = EditorGUILayout.Foldout(
                _foldDifficulty,
                "4. Difficulty & Blocks",
                true);

            if (!_foldDifficulty)
                return;

            EditorGUILayout.BeginVertical("box");

            _config.Preset =
                (DifficultyPreset)EditorGUILayout.EnumPopup(
                    "Preset",
                    _config.Preset);

            EditorGUILayout.LabelField(
                "BLOCKS PER COLOR",
                EditorStyles.boldLabel);

            SyncQuotasWithPalette();

            EditorGUILayout.BeginHorizontal(
                EditorStyles.toolbar);

            GUILayout.Label(
                "Color",
                GUILayout.Width(50));

            GUILayout.Label(
                "Src px",
                GUILayout.Width(50));

            GUILayout.Label(
                "ID",
                GUILayout.Width(30));

            GUILayout.Label(
                "Required",
                GUILayout.Width(70));

            GUILayout.Label(
                "Actual",
                GUILayout.Width(60));

            EditorGUILayout.EndHorizontal();

            foreach (var q in _config.Quotas)
            {
                EditorGUILayout.BeginHorizontal();

                var rect =
                    GUILayoutUtility.GetRect(
                        16,
                        16,
                        GUILayout.Width(50));

                EditorGUI.DrawRect(
                    rect,
                    q.PreviewColor);

                GUILayout.Label(
                    q.SourcePixelCount.ToString(),
                    GUILayout.Width(50));

                GUILayout.Label(
                    q.ColorId.ToString(),
                    GUILayout.Width(30));

                q.RequiredBlockCount =
                    EditorGUILayout.IntField(
                        q.RequiredBlockCount,
                        GUILayout.Width(70));

                q.RequiredBlockCount =
                    Mathf.Max(
                        0,
                        q.RequiredBlockCount);

                GUILayout.Label(
                    q.ActualBlockCount >= 0
                        ? q.ActualBlockCount.ToString()
                        : "--",
                    GUILayout.Width(60));

                if (q.MergedSourceColors.Count > 1)
                {
                    EditorGUILayout.LabelField(
                        new GUIContent(
                            "(gop)",
                            string.Join(
                                ", ",
                                q.MergedSourceColors)));
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.LabelField(
                "Total blocks (calculated)",
                _config.TotalBlocks.ToString());

            _config.CandidateCount =
                Mathf.Clamp(
                    EditorGUILayout.IntField(
                        "Candidate count",
                        _config.CandidateCount),
                    1,
                    50);

            EditorGUILayout.BeginHorizontal();

            _config.BaseSeed =
                EditorGUILayout.IntField(
                    "Base seed",
                    _config.BaseSeed);

            if (GUILayout.Button(
                    "Randomize",
                    GUILayout.Width(80)))
            {
                _config.BaseSeed =
                    UnityEngine.Random.Range(
                        0,
                        999999);
            }

            EditorGUILayout.EndHorizontal();

            _config.MinVisualFidelity =
                EditorGUILayout.Slider(
                    "Visual fidelity min",
                    _config.MinVisualFidelity,
                    0f,
                    1f);

            _foldAdvanced =
                EditorGUILayout.Foldout(
                    _foldAdvanced,
                    "Advanced solver/difficulty",
                    true);

            if (_foldAdvanced)
            {
                EditorGUILayout.BeginVertical("box");

                _config.ForceMoves =
                    (ForceMoveLevel)EditorGUILayout.EnumPopup(
                        "Forced moves",
                        _config.ForceMoves);

                _config.Branching =
                    (BranchingLevel)EditorGUILayout.EnumPopup(
                        "Branching",
                        _config.Branching);

                _config.SolverBudgetNodes =
                    EditorGUILayout.IntField(
                        "Solver nodes budget",
                        _config.SolverBudgetNodes);

                _config.SolverBudgetSeconds =
                    EditorGUILayout.FloatField(
                        "Solver sec budget",
                        _config.SolverBudgetSeconds);

                _config.UseHiddenScenePhysicsVerification =
                    EditorGUILayout.ToggleLeft(
                        "Use hidden-scene physics verification before export",
                        _config.UseHiddenScenePhysicsVerification);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }


        // =====================================================================================
        // QUOTA SYNC
        // =====================================================================================

        private void SyncQuotasWithPalette()
        {
            var resolved =
                _paletteEntries
                    .Where(e => e.IsResolved)
                    .ToList();

            var byColorId =
                resolved
                    .GroupBy(e => e.ResolvedColorId)
                    .ToList();

            var existingByColorId =
                _config.Quotas
                    .GroupBy(q => q.ColorId)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First());

            var newQuotas =
                new List<ColorBlockQuota>();

            foreach (var g in byColorId.OrderByDescending(
                         g => g.Sum(e => e.PixelCount)))
            {
                existingByColorId.TryGetValue(
                    g.Key,
                    out var existing);

                var quota =
                    new ColorBlockQuota
                    {
                        ColorId =
                            g.Key,

                        PreviewColor =
                            g.First().SourceColor,

                        SourcePixelCount =
                            g.Sum(e => e.PixelCount),

                        RequiredBlockCount =
                            existing != null
                                ? existing.RequiredBlockCount
                                : 0,

                        ActualBlockCount = -1,

                        MergedSourceColors =
                            g.Select(
                                e => (Color32)e.SourceColor)
                             .ToList()
                    };

                newQuotas.Add(quota);
            }

            _config.Quotas =
                newQuotas;
        }


        // =====================================================================================
        // 5. GENERATE
        // =====================================================================================

        private void DrawGenerateFoldout()
        {
            _foldGenerate =
                EditorGUILayout.Foldout(
                    _foldGenerate,
                    "5. Generate",
                    true);

            if (!_foldGenerate)
                return;

            EditorGUILayout.BeginVertical("box");

            bool imageOk =
                _imageGrid != null &&
                _importErrors.Count == 0;

            bool paletteOk =
                _paletteEntries.Count > 0 &&
                _paletteEntries.All(
                    e => e.IsResolved);

            bool catalogOk =
                _catalog.Count > 0;

            bool widthOk =
                _imageGrid == null ||
                _imageGrid.Width <= LevelValidator.MaxColumns;

            DrawCheck(
                "Image imported",
                imageOk);

            DrawCheck(
                "Palette fully resolved",
                paletteOk);

            DrawCheck(
                "Block catalog valid",
                catalogOk);

            DrawCheck(
                "Grid width <= 50",
                widthOk);

            bool canGenerate =
                imageOk &&
                paletteOk &&
                catalogOk &&
                widthOk &&
                !_isGenerating;

            GUI.enabled = canGenerate;

            if (GUILayout.Button(
                    _isGenerating
                        ? "Cancel Generation"
                        : $"Generate {_config.CandidateCount} Candidates",
                    GUILayout.Height(28)))
            {
                if (!_isGenerating)
                {
                    RunGenerate();
                }
            }

            GUI.enabled = true;

            EditorGUILayout.LabelField(
                "Last run",
                _lastGenerateStatus);

            EditorGUILayout.EndVertical();
        }


        private void DrawCheck(
            string label,
            bool ok)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(
                ok ? "\u2713" : "\u2717",
                GUILayout.Width(16));

            GUILayout.Label(label);

            EditorGUILayout.EndHorizontal();
        }


        // =====================================================================================
        // CENTER PANEL
        // =====================================================================================

        private void DrawCenterPanel()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal(
                EditorStyles.toolbar);

            _activeTab =
                (PreviewTab)GUILayout.Toolbar(
                    (int)_activeTab,
                    new[]
                    {
                        "Source Pixels",
                        "Level Layout",
                        "Difference",
                        "Solution Replay"
                    },
                    GUILayout.Width(420));

            GUILayout.Space(12);

            if (GUILayout.Button(
                    "-",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(24)))
            {
                _view.Zoom =
                    Mathf.Max(
                        2f,
                        _view.Zoom - 2f);
            }

            GUILayout.Label(
                $"{_view.Zoom:F0}px/cell",
                GUILayout.Width(70));

            if (GUILayout.Button(
                    "+",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(24)))
            {
                _view.Zoom += 2f;
            }

            if (GUILayout.Button(
                    "Fit",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(40)))
            {
                FitZoom();
            }

            _view.ShowGrid =
                GUILayout.Toggle(
                    _view.ShowGrid,
                    "Grid",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(50));

            _view.ShowOrigin =
                GUILayout.Toggle(
                    _view.ShowOrigin,
                    "Origin",
                    EditorStyles.toolbarButton,
                    GUILayout.Width(60));

            GUILayout.FlexibleSpace();

            GUILayout.Label(
                _selectedCandidateIndex >= 0
                    ? $"Selected: #{_selectedCandidateIndex:D2}"
                    : "Selected: --");

            EditorGUILayout.EndHorizontal();

            Rect canvasRect =
                GUILayoutUtility.GetRect(
                    10,
                    10,
                    GUILayout.ExpandHeight(true),
                    GUILayout.ExpandWidth(true));

            GUI.Box(
                canvasRect,
                GUIContent.none);

            HandleCanvasInput(
                canvasRect);

            var selected =
                GetSelectedCandidate();

            switch (_activeTab)
            {
                case PreviewTab.SourcePixels:

                    if (_imageGrid == null)
                    {
                        GUI.Label(
                            canvasRect,
                            "Import a pixel image to preview the board.");
                    }
                    else
                    {
                        CandidatePreviewRenderer.DrawSourcePixels(
                            canvasRect,
                            _imageGrid,
                            _view);
                    }

                    break;


                case PreviewTab.LevelLayout:

                    if (selected == null)
                    {
                        GUI.Label(
                            canvasRect,
                            "Resolve palette, then generate candidates.");
                    }
                    else
                    {
                        CandidatePreviewRenderer.DrawLevelLayout(
                            canvasRect,
                            selected,
                            _catalog,
                            BuildColorLookup(),
                            _view,
                            _selectedPlacementId);
                    }

                    break;


                case PreviewTab.Difference:

                    if (selected == null ||
                        _imageGrid == null)
                    {
                        GUI.Label(
                            canvasRect,
                            "Chua co candidate de so sanh.");
                    }
                    else
                    {
                        _view.OverlayOpacity =
                            EditorGUILayout.Slider(
                                "Overlay opacity",
                                _view.OverlayOpacity,
                                0f,
                                1f);

                        _view.OnlyMismatches =
                            EditorGUILayout.Toggle(
                                "Only mismatches",
                                _view.OnlyMismatches);

                        CandidatePreviewRenderer.DrawDifference(
                            canvasRect,
                            selected,
                            _imageGrid,
                            _view);
                    }

                    break;


                case PreviewTab.SolutionReplay:

                    if (selected == null)
                    {
                        GUI.Label(
                            canvasRect,
                            "Chua co candidate.");
                    }
                    else
                    {
                        DrawSolutionReplayControls(
                            canvasRect,
                            selected);
                    }

                    break;
            }

            EditorGUILayout.EndVertical();
        }


        // =====================================================================================
        // SOLUTION REPLAY
        // =====================================================================================

        private void DrawSolutionReplayControls(
            Rect canvasRect,
            LevelCandidate candidate)
        {
            if (candidate.Solution == null ||
                !candidate.Solution.IsSolved)
            {
                GUI.Label(
                    canvasRect,
                    $"Solver chua PASS: {candidate.Solution?.FailureReason}");

                return;
            }

            CandidatePreviewRenderer.DrawSolutionReplay(
                canvasRect,
                candidate,
                BuildColorLookup(),
                _view);

            var controlRect =
                new Rect(
                    canvasRect.x,
                    canvasRect.yMax - 28,
                    canvasRect.width,
                    26);

            GUI.Box(
                controlRect,
                GUIContent.none);

            GUILayout.BeginArea(controlRect);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "|<",
                    GUILayout.Width(30)))
            {
                _view.SolutionStep = 0;
            }

            if (GUILayout.Button(
                    "<",
                    GUILayout.Width(30)))
            {
                _view.SolutionStep =
                    Mathf.Max(
                        0,
                        _view.SolutionStep - 1);
            }

            GUILayout.Label(
                $"Step {_view.SolutionStep}/{candidate.Solution.RemovalOrder.Count}",
                GUILayout.Width(120));

            if (GUILayout.Button(
                    ">",
                    GUILayout.Width(30)))
            {
                _view.SolutionStep =
                    Mathf.Min(
                        candidate.Solution.RemovalOrder.Count,
                        _view.SolutionStep + 1);
            }

            if (GUILayout.Button(
                    ">|",
                    GUILayout.Width(30)))
            {
                _view.SolutionStep =
                    candidate.Solution.RemovalOrder.Count;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.EndArea();
        }


        // =====================================================================================
        // CANVAS INPUT
        // =====================================================================================

        private void HandleCanvasInput(
            Rect canvasRect)
        {
            Event e = Event.current;

            if (!canvasRect.Contains(
                    e.mousePosition))
            {
                return;
            }

            if (e.type == EventType.ScrollWheel)
            {
                _view.Zoom =
                    Mathf.Clamp(
                        _view.Zoom - e.delta.y,
                        2f,
                        64f);

                e.Use();
            }
            else if (
                e.type == EventType.MouseDrag &&
                (e.button == 2 || e.alt))
            {
                _view.Pan += e.delta;

                e.Use();
            }
            else if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.F)
                {
                    FitZoom();
                    e.Use();
                }

                if (e.keyCode == KeyCode.G)
                {
                    _view.ShowGrid =
                        !_view.ShowGrid;

                    e.Use();
                }

                if (e.keyCode == KeyCode.O)
                {
                    _view.ShowOrigin =
                        !_view.ShowOrigin;

                    e.Use();
                }
            }
        }


        private void FitZoom()
        {
            _view.Pan =
                Vector2.zero;

            if (_imageGrid == null)
                return;

            _view.Zoom = 16f;
        }


        // =====================================================================================
        // RIGHT PANEL
        // =====================================================================================

        private void DrawRightPanel()
        {
            EditorGUILayout.BeginVertical(
                GUILayout.Width(360));

            _rightScroll =
                EditorGUILayout.BeginScrollView(
                    _rightScroll);

            int solved =
                _candidates.Count(
                    c => c.Status ==
                         CandidateStatus.Solved);

            EditorGUILayout.LabelField(
                $"CANDIDATES ({solved} solved / {_candidates.Count} generated)",
                EditorStyles.boldLabel);

            for (int i = 0;
                 i < _candidates.Count;
                 i++)
            {
                DrawCandidateCard(i);
            }

            EditorGUILayout.Space();

            DrawSelectedCandidateActions();

            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndVertical();
        }


        private void DrawCandidateCard(
            int index)
        {
            var c =
                _candidates[index];

            bool selected =
                index == _selectedCandidateIndex;

            var style =
                selected
                    ? new GUIStyle("box")
                    {
                        normal =
                        {
                            textColor = Color.white
                        }
                    }
                    : new GUIStyle("box");

            EditorGUILayout.BeginVertical(
                style);

            EditorGUILayout.BeginHorizontal();

            string statusLabel =
                c.Status switch
                {
                    CandidateStatus.Solved =>
                        "PASS",

                    CandidateStatus.Unsolved =>
                        "FAIL",

                    CandidateStatus.Unknown =>
                        "UNKNOWN",

                    CandidateStatus.Invalid =>
                        "INVALID",

                    _ => "..."
                };

            GUILayout.Label(
                $"#{index:D2}  {statusLabel}  Score {c.Score.Total * 100f:F0}",
                GUILayout.Width(220));

            if (c.Pinned)
            {
                GUILayout.Label(
                    "\u2605",
                    GUILayout.Width(20));
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Label(
                $"Quota fit {c.Score.ColorQuotaFit * 100f:F0}% | " +
                $"Fidelity {c.Score.VisualFidelity * 100f:F0}% | " +
                $"D {c.Score.DifficultyScore:F0}");

            GUILayout.Label(
                $"{c.Score.SolutionSteps} steps | " +
                $"seed {c.Seed} | " +
                $"{c.Blocks.Count} blocks");

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(
                    "Select",
                    GUILayout.Width(70)))
            {
                _selectedCandidateIndex =
                    index;

                _view.SolutionStep = 0;
            }

            c.Pinned =
                GUILayout.Toggle(
                    c.Pinned,
                    "Pin",
                    "Button",
                    GUILayout.Width(50));

            if (GUILayout.Button(
                    "Reject",
                    GUILayout.Width(60)))
            {
                c.Rejected = true;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }


        // =====================================================================================
        // SELECTED CANDIDATE
        // =====================================================================================

        private void DrawSelectedCandidateActions()
        {
            var candidate =
                GetSelectedCandidate();

            EditorGUILayout.LabelField(
                "SELECTED CANDIDATE",
                EditorStyles.boldLabel);

            if (candidate == null)
            {
                EditorGUILayout.HelpBox(
                    "Chua chon candidate.",
                    MessageType.Info);

                return;
            }

            foreach (var issue in candidate.Issues)
            {
                var msgType =
                    issue.Severity switch
                    {
                        ValidationSeverity.Error =>
                            MessageType.Error,

                        ValidationSeverity.Warning =>
                            MessageType.Warning,

                        _ =>
                            MessageType.Info
                    };

                EditorGUILayout.HelpBox(
                    issue.ToString(),
                    msgType);
            }

            if (GUILayout.Button(
                    "Validate Selected",
                    GUILayout.Height(24)))
            {
                RevalidateSelected(candidate);
            }

            GUI.enabled =
                candidate.Solution != null &&
                !candidate.Solution.IsSolved;

            if (GUILayout.Button(
                    "Solve Longer",
                    GUILayout.Height(22)))
            {
                var budget =
                    new SolverBudget
                    {
                        MaxNodes =
                            _config.SolverBudgetNodes * 4,

                        MaxSeconds =
                            _config.SolverBudgetSeconds * 4
                    };

                candidate.Solution =
                    LevelSolver.Solve(
                        candidate,
                        budget);

                RevalidateSelected(
                    candidate);
            }

            GUI.enabled = true;

            bool canExport =
                candidate.CanExport();

            GUI.enabled =
                canExport;

            if (GUILayout.Button(
                    "Export RawLevelData",
                    GUILayout.Height(28)))
            {
                OpenExportDialog(
                    candidate);
            }

            GUI.enabled = true;

            if (!canExport)
            {
                EditorGUILayout.HelpBox(
                    $"Fix {candidate.Issues.Count(i => i.Severity == ValidationSeverity.Error)} errors before export.",
                    MessageType.Warning);
            }
        }


        // =====================================================================================
        // STATUS BAR
        // =====================================================================================

        private void DrawStatusBar()
        {
            EditorGUILayout.BeginHorizontal(
                EditorStyles.toolbar);

            GUILayout.Label(
                $"Session: {_sessionName}{(_dirty ? "*" : "")}",
                GUILayout.Width(160));

            GUILayout.Label(
                _imageGrid != null
                    ? $"Grid: {_imageGrid.Width}x{_imageGrid.Height}"
                    : "Grid: --",
                GUILayout.Width(120));

            int resolved =
                _paletteEntries.Count(
                    e => e.IsResolved);

            GUILayout.Label(
                $"Palette: {resolved}/{_paletteEntries.Count} resolved",
                GUILayout.Width(160));

            int passCount =
                _candidates.Count(
                    c => c.Status ==
                         CandidateStatus.Solved);

            GUILayout.Label(
                $"Candidates: {passCount} pass / {_candidates.Count}",
                GUILayout.Width(150));

            GUILayout.Label(
                _selectedCandidateIndex >= 0 &&
                _selectedCandidateIndex < _candidates.Count
                    ? $"Selected: #{_selectedCandidateIndex:D2} {_candidates[_selectedCandidateIndex].Status}"
                    : "Selected: --");

            EditorGUILayout.EndHorizontal();
        }


        // =====================================================================================
        // ACTION: IMPORT IMAGE
        // =====================================================================================

        private void ImportImage()
        {
            if (_sourceTexture == null)
                return;

            _imageGrid =
                PixelGridImporter.Import(
                    _sourceTexture,
                    _importConfig,
                    out _importErrors);

            RefreshPaletteFromImage();

            RebuildCatalog();

            _dirty = true;

            Repaint();
        }


        // =====================================================================================
        // PALETTE REFRESH
        // =====================================================================================

        private void RefreshPaletteFromImage()
        {
            if (_imageGrid == null)
            {
                _paletteEntries.Clear();

                return;
            }

            // ---------------------------------------------------------------------
            // STEP 1:
            // Build PaletteEntry tu ImageGrid.
            // ---------------------------------------------------------------------

            _paletteService.RawColorDataAsset =
                _rawColorDataAsset;

            var existing =
                _paletteService.LoadCurrentColorData();

            _paletteEntries =
                _paletteService.BuildPaletteEntries(
                    _imageGrid,
                    existing);


            // ---------------------------------------------------------------------
            // STEP 2:
            // RawColorData la source of truth.
            // Resolve truc tiep tu rawColors.
            // ---------------------------------------------------------------------

            ResolvePaletteEntriesFromRawColorData();


            // ---------------------------------------------------------------------
            // STEP 3:
            // Update quota sau khi resolve.
            // ---------------------------------------------------------------------

            SyncQuotasWithPalette();


            Repaint();
        }


        // =====================================================================================
        // RAW COLOR LOOKUP
        // =====================================================================================

        private Dictionary<int, Color32> LoadRawColorLookup()
        {
            var lookup =
                new Dictionary<int, Color32>();

            if (_rawColorDataAsset == null)
            {
                Debug.LogWarning(
                    "[Pixel Level Generator] RawColorDataAsset is null.");

                return lookup;
            }

            if (_rawColorDataAsset.rawColors == null)
            {
                Debug.LogWarning(
                    "[Pixel Level Generator] RawColorData.rawColors is null.");

                return lookup;
            }

            foreach (var raw in _rawColorDataAsset.rawColors)
            {
                if (raw == null)
                    continue;

                if (string.IsNullOrWhiteSpace(raw.Color))
                    continue;

                if (!TryParseColor(
                        raw.Color,
                        out Color32 color))
                {
                    Debug.LogWarning(
                        $"[Pixel Level Generator] Invalid RawColor: ID={raw.ID}, Color={raw.Color}");

                    continue;
                }

                lookup[raw.ID] = color;
            }

            return lookup;
        }


        // =====================================================================================
        // RESOLVE PALETTE
        // =====================================================================================

        private void ResolvePaletteEntriesFromRawColorData()
        {
            if (_paletteEntries == null ||
                _paletteEntries.Count == 0)
            {
                return;
            }

            if (_rawColorDataAsset == null)
            {
                Debug.LogWarning(
                    "[Pixel Level Generator] Cannot resolve palette: RawColorDataAsset is null.");

                return;
            }

            var rawColors =
                _rawColorDataAsset.rawColors;

            if (rawColors == null ||
                rawColors.Count == 0)
            {
                Debug.LogWarning(
                    "[Pixel Level Generator] RawColorData contains no colors.");

                return;
            }

            int resolvedCount = 0;


            foreach (var entry in _paletteEntries)
            {
                Color32 sourceColor =
                    (Color32)entry.SourceColor;

                RawColor matched =
                    null;


                foreach (var raw in rawColors)
                {
                    if (raw == null)
                        continue;

                    if (string.IsNullOrWhiteSpace(
                            raw.Color))
                    {
                        continue;
                    }

                    if (!TryParseColor(
                            raw.Color,
                            out Color32 rawColor))
                    {
                        continue;
                    }

                    if (ColorsApproxEqual(
                            sourceColor,
                            rawColor))
                    {
                        matched = raw;

                        break;
                    }
                }


                if (matched != null)
                {
                    entry.ExistingColorId =
                        matched.ID;

                    entry.ResolvedColorId =
                        matched.ID;

                    // Dong bo hex theo RawColorData.
                    entry.EditableHex =
                        NormalizeHex(
                            matched.Color);

                    resolvedCount++;
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
                $"[Pixel Level Generator] Palette resolved: " +
                $"{resolvedCount}/{_paletteEntries.Count}");
        }


        // =====================================================================================
        // COLOR PARSE
        // =====================================================================================

        private static bool TryParseColor(
            string hex,
            out Color32 color)
        {
            color = default;

            if (string.IsNullOrWhiteSpace(
                    hex))
            {
                return false;
            }

            hex =
                hex.Trim();

            if (!hex.StartsWith("#"))
            {
                hex =
                    "#" + hex;
            }

            if (!ColorUtility.TryParseHtmlString(
                    hex,
                    out Color parsed))
            {
                return false;
            }

            color =
                (Color32)parsed;

            return true;
        }


        private static string NormalizeHex(
            string hex)
        {
            if (string.IsNullOrWhiteSpace(
                    hex))
            {
                return "#FFFFFF";
            }

            hex =
                hex.Trim();

            if (!hex.StartsWith("#"))
            {
                hex =
                    "#" + hex;
            }

            if (TryParseColor(
                    hex,
                    out Color32 color))
            {
                return
                    $"#{color.r:X2}" +
                    $"{color.g:X2}" +
                    $"{color.b:X2}";
            }

            return hex;
        }


        private static bool ColorsApproxEqual(
            Color32 a,
            Color32 b,
            byte tolerance = 1)
        {
            return
                Mathf.Abs(
                    a.r - b.r) <= tolerance &&

                Mathf.Abs(
                    a.g - b.g) <= tolerance &&

                Mathf.Abs(
                    a.b - b.b) <= tolerance;
        }


        // =====================================================================================
        // PALETTE MANAGER
        // =====================================================================================

        private void OpenPaletteManager()
        {
            if (_rawColorDataAsset == null)
            {
                EditorUtility.DisplayDialog(
                    "Palette Manager",
                    "Chua gan RawColorData asset.",
                    "OK");

                return;
            }

            if (_imageGrid == null)
            {
                EditorUtility.DisplayDialog(
                    "Palette Manager",
                    "Chua import image.",
                    "OK");

                return;
            }


            PaletteManagerWindow.Open(
                _paletteEntries,
                _rawColorDataAsset,
                _dataCollectionAsset,

                () =>
                {
                    // =========================================================
                    // QUAN TRONG:
                    // Palette Manager vua sua RawColorData.
                    // Khong tin list cu nua.
                    // Doc lai asset production.
                    // =========================================================

                    EditorUtility.SetDirty(
                        _rawColorDataAsset);

                    AssetDatabase.SaveAssets();

                    AssetDatabase.Refresh();


                    // Build lai PaletteEntry
                    // + resolve lai tu rawColors.
                    RefreshPaletteFromImage();


                    // Update quota.
                    SyncQuotasWithPalette();


                    _dirty = true;


                    Debug.Log(
                        "[Pixel Level Generator] " +
                        "Palette Manager applied. " +
                        "Palette reloaded from RawColorData.");


                    Repaint();
                });
        }


        // =====================================================================================
        // BLOCK CATALOG
        // =====================================================================================

        private void RebuildCatalog()
        {
            var buildResult =
                BlockCatalogBuilder.Build(
                    _dataCollectionAsset,
                    _cellSize);

            _catalog =
                buildResult.Catalog;

            _catalogIssues =
                buildResult.Issues;

            foreach (var issue in _catalogIssues)
            {
                Debug.LogWarning(
                    "[Pixel Level Generator] " +
                    issue);
            }
        }


        // =====================================================================================
        // GENERATE
        // =====================================================================================

        private void RunGenerate()
        {
            if (_imageGrid == null)
                return;

            if (_paletteEntries.Count == 0)
                return;

            if (!_paletteEntries.All(
                    e => e.IsResolved))
            {
                EditorUtility.DisplayDialog(
                    "Generate Candidates",
                    "Palette chua resolve het mau.",
                    "OK");

                return;
            }


            if (_candidates.Count > 0)
            {
                int choice =
                    EditorUtility.DisplayDialogComplex(
                        "Generate Candidates",
                        "Da co candidate tu lan generate truoc. Ban muon lam gi?",
                        "Replace",
                        "Cancel",
                        "Append");

                if (choice == 1)
                    return;

                if (choice == 0)
                {
                    _candidates.Clear();

                    _selectedCandidateIndex =
                        -1;
                }
            }


            var boardMask =
                new bool[
                    _imageGrid.Width,
                    _imageGrid.Height];

            var desired =
                new int[
                    _imageGrid.Width,
                    _imageGrid.Height];


            var colorIdBySource =
                _paletteEntries
                    .Where(e => e.IsResolved)
                    .GroupBy(
                        e => (Color32)e.SourceColor)
                    .ToDictionary(
                        g => g.Key,
                        g => g.First().ResolvedColorId);


            for (int x = 0;
                 x < _imageGrid.Width;
                 x++)
            {
                for (int y = 0;
                     y < _imageGrid.Height;
                     y++)
                {
                    var cell =
                        _imageGrid.Cells[x, y];

                    boardMask[x, y] =
                        !cell.IsVoid;

                    if (!cell.IsVoid &&
                        colorIdBySource.TryGetValue(
                            cell.SourceColor,
                            out int colorId))
                    {
                        desired[x, y] =
                            colorId;
                    }
                    else
                    {
                        desired[x, y] =
                            -1;
                    }
                }
            }


            _config.GridWidth =
                _imageGrid.Width;

            _config.GridHeight =
                _imageGrid.Height;

            _config.LevelId =
                _levelId;

            _config.CellSize =
                _cellSize;

            _config.Hard =
                _hardLevel;


            var generator =
                new LevelCandidateGenerator(
                    _imageGrid,
                    boardMask,
                    desired,
                    _catalog,
                    _config);


            _isGenerating = true;


            try
            {
                var generated =
                    generator.GenerateAll(
                        (cur, total, msg) =>
                        {
                            _lastGenerateStatus =
                                $"{cur}/{total} - {msg}";

                            if (EditorUtility.DisplayCancelableProgressBar(
                                    "Generating candidates",
                                    msg,
                                    cur / (float)total))
                            {
                                throw new OperationCanceledException();
                            }
                        });


                var knownColorIds =
                    new HashSet<int>(
                        _paletteEntries
                            .Where(e => e.IsResolved)
                            .Select(
                                e => e.ResolvedColorId));


                foreach (var c in generated)
                {
                    var report =
                        LevelValidator.ValidateCandidate(
                            c,
                            _catalog,
                            knownColorIds,
                            null,
                            _levelId);

                    c.Issues =
                        report.Issues;

                    if (report.Verdict ==
                            ValidationVerdict.Fail &&
                        c.Status ==
                            CandidateStatus.Solved)
                    {
                        c.Status =
                            CandidateStatus.Invalid;
                    }
                }


                _candidates.AddRange(
                    generated);

                _candidates =
                    _candidates
                        .OrderByDescending(
                            c => c.Pinned)
                        .ThenByDescending(
                            c => c.Score.Total)
                        .ToList();


                if (_selectedCandidateIndex < 0 &&
                    _candidates.Count > 0)
                {
                    _selectedCandidateIndex =
                        0;
                }


                _lastGenerateStatus =
                    $"Done: " +
                    $"{_candidates.Count(c => c.Status == CandidateStatus.Solved)} " +
                    $"solved / " +
                    $"{_candidates.Count} total";
            }
            catch (OperationCanceledException)
            {
                _lastGenerateStatus =
                    "Cancelled (candidate da xong van duoc giu).";
            }
            catch (Exception ex)
            {
                _lastGenerateStatus =
                    "Generation failed.";

                Debug.LogException(ex);
            }
            finally
            {
                EditorUtility.ClearProgressBar();

                _isGenerating =
                    false;
            }
        }


        // =====================================================================================
        // REVALIDATE
        // =====================================================================================

        private void RevalidateSelected(
            LevelCandidate candidate)
        {
            if (candidate == null)
                return;

            var knownColorIds =
                new HashSet<int>(
                    _paletteEntries
                        .Where(e => e.IsResolved)
                        .Select(
                            e => e.ResolvedColorId));


            var report =
                LevelValidator.ValidateCandidate(
                    candidate,
                    _catalog,
                    knownColorIds,
                    null,
                    _levelId);


            candidate.Issues =
                report.Issues;


            if (candidate.Solution != null &&
                candidate.Solution.IsSolved &&
                report.Verdict !=
                    ValidationVerdict.Fail)
            {
                candidate.Status =
                    CandidateStatus.Solved;
            }
        }


        // =====================================================================================
        // SELECTED CANDIDATE
        // =====================================================================================

        private LevelCandidate GetSelectedCandidate()
        {
            if (_selectedCandidateIndex < 0)
                return null;

            if (_selectedCandidateIndex >=
                _candidates.Count)
            {
                return null;
            }

            return
                _candidates[
                    _selectedCandidateIndex];
        }


        // =====================================================================================
        // COLOR LOOKUP
        // =====================================================================================

        private Dictionary<int, Color32> BuildColorLookup()
        {
            return
                _paletteEntries
                    .Where(e => e.IsResolved)
                    .GroupBy(
                        e => e.ResolvedColorId)
                    .ToDictionary(
                        g => g.Key,
                        g => (Color32)g.First().SourceColor);
        }


        // =====================================================================================
        // EXPORT
        // =====================================================================================

        private void OpenExportDialog(
            LevelCandidate candidate)
        {
            var request =
                new ExportRequest
                {
                    Candidate =
                        candidate,

                    LevelId =
                        _levelId,

                    OutputFolder =
                        _outputFolder,

                    AssetName =
                        _assetName,

                    CellSize =
                        _cellSize,

                    Hard =
                        _hardLevel,

                    RawLevelDataType =
                        ResolveProductionType(
                            _rawLevelDataTypeName),

                    RawLevelRowType =
                        ResolveProductionType(
                            _rawLevelRowTypeName)
                };


            if (RawLevelExporter.AssetExists(
                    _outputFolder,
                    _assetName))
            {
                int choice =
                    EditorUtility.DisplayDialogComplex(
                        $"{_assetName} already exists",
                        "( ) Cancel\n" +
                        "( ) Create copy\n" +
                        "( ) Overwrite existing asset\n\n" +
                        "Chon huong xu ly.",
                        "Overwrite",
                        "Cancel",
                        "Create Copy");


                request.OverwriteChoice =
                    choice switch
                    {
                        0 =>
                            OverwriteChoice.Overwrite,

                        2 =>
                            OverwriteChoice.CreateCopy,

                        _ =>
                            OverwriteChoice.Cancel
                    };


                if (request.OverwriteChoice ==
                    OverwriteChoice.Cancel)
                {
                    return;
                }
            }


            var result =
                RawLevelExporter.Export(
                    request);


            foreach (var log in result.Log)
            {
                Debug.Log(
                    "[Pixel Level Generator] " +
                    log);
            }


            if (result.Success)
            {
                var asset =
                    AssetDatabase.LoadAssetAtPath<ScriptableObject>(
                        result.AssetPath);

                EditorGUIUtility.PingObject(
                    asset);

                EditorUtility.DisplayDialog(
                    "Export complete",
                    $"Created: {result.AssetPath}\n\n" +
                    "Next required step:\n" +
                    "1. Chon Data/LevelData.asset.\n" +
                    "2. Bam Load All Raw Levels.\n" +
                    "3. Bam Convert Level.\n" +
                    "4. Chay level va kiem tra solution.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Export failed",
                    string.Join(
                        "\n",
                        result.Log),
                    "OK");
            }
        }


        // =====================================================================================
        // RESOLVE PRODUCTION TYPE
        // =====================================================================================

        private static Type ResolveProductionType(
            string typeName)
        {
            foreach (var asm in
                     AppDomain.CurrentDomain.GetAssemblies())
            {
                Type t =
                    asm.GetType(typeName);

                if (t != null)
                    return t;


                Type[] types;

                try
                {
                    types =
                        asm.GetTypes();
                }
                catch
                {
                    continue;
                }


                foreach (var candidate in types)
                {
                    if (candidate.Name ==
                        typeName)
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }


        // =====================================================================================
        // SESSION - NEW
        // =====================================================================================

        private void NewSession()
        {
            if (_dirty &&
                !EditorUtility.DisplayDialog(
                    "New Session",
                    "Session dang co thay doi chua luu. Tiep tuc?",
                    "Tiep tuc",
                    "Huy"))
            {
                return;
            }


            _sourceTexture =
                null;

            _imageGrid =
                null;

            _paletteEntries.Clear();

            _candidates.Clear();

            _selectedCandidateIndex =
                -1;

            _sessionName =
                "Untitled";

            _dirty =
                false;

            _lastGenerateStatus =
                "--";
        }


        // =====================================================================================
        // SESSION - SAVE
        // =====================================================================================

        private void SaveSession(
            bool saveAs)
        {
            string path =
                EditorUtility.SaveFilePanelInProject(
                    "Save Session",
                    saveAs
                        ? "NewSession"
                        : _sessionName,
                    "asset",
                    "Chon vi tri luu session");


            if (string.IsNullOrEmpty(path))
                return;


            var session =
                ScriptableObject.CreateInstance<
                    PixelLevelGenerationSession>();


            session.SourceTexture =
                _sourceTexture;

            session.ImportMode =
                _importConfig.Mode;

            session.AlphaThreshold =
                _importConfig.AlphaThreshold;

            session.BackgroundRule =
                _importConfig.BackgroundRule;

            session.DownsampleFactor =
                _importConfig.DownsampleFactor;

            session.CropRect =
                _importConfig.CropRect;

            session.LevelId =
                _levelId;

            session.OutputFolder =
                _outputFolder;

            session.CellSize =
                _cellSize;

            session.Hard =
                _hardLevel;

            session.Preset =
                _config.Preset;

            session.CandidateCount =
                _config.CandidateCount;

            session.BaseSeed =
                _config.BaseSeed;

            session.MinVisualFidelity =
                _config.MinVisualFidelity;


            foreach (var e in _paletteEntries)
            {
                session.PaletteRows.Add(
                    new SerializablePaletteRow
                    {
                        SourceColor =
                            e.SourceColor,

                        EditableHex =
                            e.EditableHex,

                        ExistingColorId =
                            e.ExistingColorId
                                ?? -1,

                        ResolvedColorId =
                            e.ResolvedColorId,

                        WillAddToRawColorData =
                            e.WillAddToRawColorData,

                        PixelCount =
                            e.PixelCount
                    });
            }


            foreach (var q in _config.Quotas)
            {
                session.QuotaRows.Add(
                    new SerializableQuotaRow
                    {
                        ColorId =
                            q.ColorId,

                        RequiredBlockCount =
                            q.RequiredBlockCount
                    });
            }


            AssetDatabase.CreateAsset(
                session,
                path);

            AssetDatabase.SaveAssets();


            _sessionName =
                System.IO.Path.GetFileNameWithoutExtension(
                    path);

            _dirty =
                false;


            EditorGUIUtility.PingObject(
                session);
        }


        // =====================================================================================
        // SESSION - LOAD
        // =====================================================================================

        private void LoadSession()
        {
            string path =
                EditorUtility.OpenFilePanel(
                    "Load Session",
                    "Assets",
                    "asset");


            if (string.IsNullOrEmpty(path))
                return;


            path =
                ToProjectRelativePath(
                    path);


            var session =
                AssetDatabase.LoadAssetAtPath<
                    PixelLevelGenerationSession>(
                        path);


            if (session == null)
            {
                EditorUtility.DisplayDialog(
                    "Load Session",
                    "File khong phai PixelLevelGenerationSession hop le.",
                    "OK");

                return;
            }


            _sourceTexture =
                session.SourceTexture;

            _importConfig.Mode =
                session.ImportMode;

            _importConfig.AlphaThreshold =
                session.AlphaThreshold;

            _importConfig.BackgroundRule =
                session.BackgroundRule;

            _importConfig.DownsampleFactor =
                session.DownsampleFactor;

            _importConfig.CropRect =
                session.CropRect;

            _levelId =
                session.LevelId;

            _outputFolder =
                session.OutputFolder;

            _cellSize =
                session.CellSize;

            _hardLevel =
                session.Hard;


            _config.ApplyPreset(
                session.Preset);

            _config.CandidateCount =
                session.CandidateCount;

            _config.BaseSeed =
                session.BaseSeed;

            _config.MinVisualFidelity =
                session.MinVisualFidelity;


            _config.Quotas =
                session.QuotaRows
                    .Select(
                        q =>
                            new ColorBlockQuota
                            {
                                ColorId =
                                    q.ColorId,

                                RequiredBlockCount =
                                    q.RequiredBlockCount
                            })
                    .ToList();


            if (_sourceTexture != null)
            {
                ImportImage();
            }


            _sessionName =
                System.IO.Path.GetFileNameWithoutExtension(
                    path);

            _dirty =
                false;


            // RawColorData la source of truth.
            ResolvePaletteEntriesFromRawColorData();

            SyncQuotasWithPalette();

            Repaint();
        }


        // =====================================================================================
        // PATH
        // =====================================================================================

        private static string ToProjectRelativePath(
            string absolutePath)
        {
            if (absolutePath.StartsWith(
                    Application.dataPath))
            {
                return
                    "Assets" +
                    absolutePath.Substring(
                        Application.dataPath.Length);
            }

            return absolutePath;
        }
    }
}