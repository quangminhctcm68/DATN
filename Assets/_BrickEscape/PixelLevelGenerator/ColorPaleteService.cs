using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using BrickEscape.PixelLevelGenerator.Model;

namespace BrickEscape.PixelLevelGenerator
{
    // ============================================================
    // RAW COLOR ENTRY
    // ============================================================


    [Serializable]
    public class ColorRawEntry
    {
        public int ColorId;
        public Color32 Color;
    }
    // ============================================================
    // UPDATE RESULT
    // ============================================================


    public class PaletteUpdateResult
    {
        public bool Success;
        public List<string> Log = new List<string>();

        public int AddedCount;
        public int UpdatedCount;
    }
    // ============================================================
    // COLOR PALETTE SERVICE
    // ============================================================

    public class ColorPaletteService
    {
        public RawColorData RawColorDataAsset;

        public ScriptableObject DataCollectionAsset;

        // ============================================================
        // LOAD CURRENT RAW COLOR DATA
        // ============================================================

        public List<ColorRawEntry> LoadCurrentColorData()
        {
            var result = new List<ColorRawEntry>();

            if (RawColorDataAsset == null)
            {
                Debug.LogWarning(
                    "[Pixel Level Generator] RawColorDataAsset is null."
                );

                return result;
            }

            if (RawColorDataAsset.rawColors == null)
            {
                Debug.LogWarning(
                    "[Pixel Level Generator] RawColorData.rawColors is null."
                );

                return result;
            }

            foreach (RawColor raw in RawColorDataAsset.rawColors)
            {
                if (raw == null)
                    continue;

                if (!TryParseColor32(raw.Color, out Color32 color))
                {
                    Debug.LogWarning(
                        $"[Pixel Level Generator] Invalid color. " +
                        $"ID={raw.ID}, Color='{raw.Color}'"
                    );

                    continue;
                }

                result.Add(new ColorRawEntry
                {
                    ColorId = raw.ID,
                    Color = color
                });
            }

            return result;
        }

        // ============================================================
        // BUILD PALETTE ENTRIES
        // ============================================================

        public List<PaletteEntry> BuildPaletteEntries(
          ImageGrid grid,
          List<ColorRawEntry> existing,
          float matchTolerance01 = 0.5f / 255f)
        {
            var entries = new List<PaletteEntry>();

            if (grid == null)
                return entries;

            if (existing == null)
                existing = new List<ColorRawEntry>();

            var counts = new Dictionary<Color32, int>();

            for (int x = 0; x < grid.Width; x++)
            {
                for (int y = 0; y < grid.Height; y++)
                {
                    ImageCell cell = grid.Cells[x, y];

                    if (cell.IsVoid)
                        continue;

                    if (!counts.TryGetValue(cell.SourceColor, out int count))
                        count = 0;

                    counts[cell.SourceColor] = count + 1;
                }
            }

            foreach (var pair in counts)
            {
                Color32 sourceColor = pair.Key;

                var entry = new PaletteEntry
                {
                    SourceColor = sourceColor,
                    PixelCount = pair.Value,
                    EditableHex = "#" +
                                  ColorUtility.ToHtmlStringRGB(sourceColor),
                    ExistingColorId = null,
                    ResolvedColorId = -1,
                    WillAddToRawColorData = false
                };

                ColorRawEntry match = existing.FirstOrDefault(
                    x => ColorsApproxEqual(
                        x.Color,
                        sourceColor,
                        matchTolerance01
                    )
                );

                if (match != null)
                {
                    entry.ExistingColorId = match.ColorId;
                    entry.ResolvedColorId = match.ColorId;
                }

                entries.Add(entry);
            }

            return entries
                .OrderByDescending(x => x.PixelCount)
                .ToList();
        }


        // ============================================================
        // UPDATE RAW COLOR DATA
        // ============================================================


        public PaletteUpdateResult UpdateRawColorData(
            List<PaletteEntry> rowsToApply)
        {
            var result = new PaletteUpdateResult();

            if (RawColorDataAsset == null)
            {
                result.Log.Add("Chua gan RawColorData asset.");
                return result;
            }

            if (RawColorDataAsset.rawColors == null)
            {
                RawColorDataAsset.rawColors = new List<RawColor>();
            }

            if (rowsToApply == null || rowsToApply.Count == 0)
            {
                result.Log.Add("Khong co palette row nao can update.");
                return result;
            }

            // --------------------------------------------------------
            // Validate hex trước
            // --------------------------------------------------------

            var validRows = new List<(PaletteEntry entry, Color32 color)>();

            foreach (PaletteEntry entry in rowsToApply)
            {
                if (entry == null)
                    continue;

                if (!TryParseColor32(entry.EditableHex, out Color32 parsed))
                {
                    result.Log.Add(
                        $"Hex khong hop le, bo qua: " +
                        $"{entry.EditableHex}"
                    );

                    continue;
                }

                validRows.Add((entry, parsed));
            }

            if (validRows.Count == 0)
            {
                result.Log.Add("Khong co row nao co hex hop le.");
                return result;
            }

            // --------------------------------------------------------
            // Undo
            // --------------------------------------------------------

            Undo.RecordObject(
                RawColorDataAsset,
                "Update RawColorData - Pixel Level Generator"
            );

            if (DataCollectionAsset != null)
            {
                Undo.RecordObject(
                    DataCollectionAsset,
                    "Rebuild Color Data - Pixel Level Generator"
                );
            }

            // --------------------------------------------------------
            // Find max ID
            // --------------------------------------------------------

            int maxId = -1;

            foreach (RawColor raw in RawColorDataAsset.rawColors)
            {
                if (raw == null)
                    continue;

                maxId = Mathf.Max(maxId, raw.ID);
            }

            // --------------------------------------------------------
            // Apply
            // --------------------------------------------------------

            foreach (var item in validRows)
            {
                PaletteEntry entry = item.entry;
                Color32 color = item.color;

                // ----------------------------------------------------
                // UPDATE EXISTING
                // ----------------------------------------------------

                if (entry.ExistingColorId.HasValue)
                {
                    int id = entry.ExistingColorId.Value;

                    RawColor raw = RawColorDataAsset.rawColors
                        .FirstOrDefault(x =>
                            x != null &&
                            x.ID == id
                        );

                    if (raw == null)
                    {
                        result.Log.Add(
                            $"Khong tim thay RawColor ID={id} de update."
                        );

                        continue;
                    }

                    string newHex =
                        "#" +
                        ColorUtility.ToHtmlStringRGB(color);

                    raw.Color = newHex;

                    entry.ResolvedColorId = id;

                    result.UpdatedCount++;

                    result.Log.Add(
                        $"Updated Color ID={id} -> {newHex}"
                    );
                }

                // ----------------------------------------------------
                // ADD NEW
                // ----------------------------------------------------

                else if (entry.WillAddToRawColorData)
                {
                    int newId = ++maxId;

                    string hex =
                        "#" +
                        ColorUtility.ToHtmlStringRGB(color);

                    RawColor raw = new RawColor
                    {
                        ID = newId,
                        Color = hex
                    };

                    RawColorDataAsset.rawColors.Add(raw);

                    entry.ExistingColorId = newId;
                    entry.ResolvedColorId = newId;

                    result.AddedCount++;

                    result.Log.Add(
                        $"Added Color ID={newId} -> {hex}"
                    );
                }
            }

            // --------------------------------------------------------
            // Save RawColorData
            // --------------------------------------------------------

            EditorUtility.SetDirty(RawColorDataAsset);

            // --------------------------------------------------------
            // Rebuild DataCollection
            // --------------------------------------------------------

            RebuildDataCollection(result);

            // --------------------------------------------------------
            // Save
            // --------------------------------------------------------

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            result.Success = true;

            result.Log.Add(
                $"RawColorData update complete. " +
                $"Added={result.AddedCount}, " +
                $"Updated={result.UpdatedCount}"
            );

            return result;
        }


        // ============================================================
        // REBUILD DATA COLLECTION
        // ============================================================

        private void RebuildDataCollection(
            PaletteUpdateResult result)
        {
            if (DataCollectionAsset == null)
            {
                result.Log.Add(
                    "DataCollectionAsset chua gan. " +
                    "Bo qua rebuild DataCollection."
                );

                return;
            }

            // --------------------------------------------------------
            // Nếu production DataCollection có API này thì dùng trực tiếp
            // --------------------------------------------------------

            var method = DataCollectionAsset.GetType().GetMethod(
                "RebuildColorDataFromRaw"
            );

            if (method != null)
            {
                try
                {
                    method.Invoke(
                        DataCollectionAsset,
                        null
                    );

                    EditorUtility.SetDirty(
                        DataCollectionAsset
                    );

                    result.Log.Add(
                        "DataCollection.RebuildColorDataFromRaw() OK."
                    );

                    return;
                }
                catch (Exception ex)
                {
                    result.Log.Add(
                        "RebuildColorDataFromRaw failed: " +
                        ex.Message
                    );
                }
            }

            // --------------------------------------------------------
            // Fallback:
            // tìm Dictionary<int, Color32> hoặc Dictionary<int, Color>
            // --------------------------------------------------------

            var fields = DataCollectionAsset
                .GetType()
                .GetFields(
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic
                );

            foreach (var field in fields)
            {
                Type fieldType = field.FieldType;

                if (!fieldType.IsGenericType)
                    continue;

                Type genericDef = fieldType.GetGenericTypeDefinition();

                if (genericDef != typeof(Dictionary<,>))
                    continue;

                Type[] args = fieldType.GetGenericArguments();

                if (args.Length != 2)
                    continue;

                if (args[0] != typeof(int))
                    continue;

                // ----------------------------------------------------
                // Dictionary<int, Color32>
                // ----------------------------------------------------

                if (args[1] == typeof(Color32))
                {
                    var dictionary =
                        field.GetValue(DataCollectionAsset)
                        as Dictionary<int, Color32>;

                    if (dictionary == null)
                    {
                        dictionary =
                            new Dictionary<int, Color32>();

                        field.SetValue(
                            DataCollectionAsset,
                            dictionary
                        );
                    }

                    dictionary.Clear();

                    foreach (RawColor raw in RawColorDataAsset.rawColors)
                    {
                        if (raw == null)
                            continue;

                        if (!TryParseColor32(
                                raw.Color,
                                out Color32 color))
                            continue;

                        dictionary[raw.ID] = color;
                    }

                    EditorUtility.SetDirty(
                        DataCollectionAsset
                    );

                    result.Log.Add(
                        $"Rebuilt Dictionary<int, Color32> '{field.Name}' " +
                        $"with {dictionary.Count} colors."
                    );

                    return;
                }

                // ----------------------------------------------------
                // Dictionary<int, Color>
                // ----------------------------------------------------

                if (args[1] == typeof(Color))
                {
                    var dictionary =
                        field.GetValue(DataCollectionAsset)
                        as Dictionary<int, Color>;

                    if (dictionary == null)
                    {
                        dictionary =
                            new Dictionary<int, Color>();

                        field.SetValue(
                            DataCollectionAsset,
                            dictionary
                        );
                    }

                    dictionary.Clear();

                    foreach (RawColor raw in RawColorDataAsset.rawColors)
                    {
                        if (raw == null)
                            continue;

                        if (!ColorUtility.TryParseHtmlString(
                                NormalizeHex(raw.Color),
                                out Color color))
                            continue;

                        dictionary[raw.ID] = color;
                    }

                    EditorUtility.SetDirty(
                        DataCollectionAsset
                    );

                    result.Log.Add(
                        $"Rebuilt Dictionary<int, Color> '{field.Name}' " +
                        $"with {dictionary.Count} colors."
                    );

                    return;
                }
            }

            result.Log.Add(
                "WARNING: Khong tim thay Dictionary<int, Color>/" +
                "Dictionary<int, Color32> trong DataCollection."
            );
        }

        private static bool TryParseColor32(
         string hex,
         out Color32 color)
        {
            color = new Color32(255, 255, 255, 255);

            if (string.IsNullOrWhiteSpace(hex))
                return false;

            hex = NormalizeHex(hex);

            if (!ColorUtility.TryParseHtmlString(
                    hex,
                    out Color parsed))
            {
                return false;
            }

            color = parsed;
            return true;
        }

        private static bool ColorsApproxEqual(
            Color32 a,
            Color32 b,
            float tolerance01)
        {
            return Mathf.Abs(a.r - b.r) / 255f <= tolerance01 &&
                   Mathf.Abs(a.g - b.g) / 255f <= tolerance01 &&
                   Mathf.Abs(a.b - b.b) / 255f <= tolerance01;
        }
        // ============================================================
        // COLOR COMPARISON
        // ============================================================

        private static bool ColorsApproxEqual(
            Color a,
            Color32 b,
            float tolerance)
        {
            Color32 a32 = a;

            return
                Mathf.Abs(
                    a32.r - b.r
                ) / 255f <= tolerance &&

                Mathf.Abs(
                    a32.g - b.g
                ) / 255f <= tolerance &&

                Mathf.Abs(
                    a32.b - b.b
                ) / 255f <= tolerance;
        }

        // ============================================================
        // NORMALIZE HEX
        // ============================================================

        private static string NormalizeHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return "#FFFFFF";

            hex = hex.Trim();

            if (!hex.StartsWith("#"))
                hex = "#" + hex;

            return hex;
        }
    }

    // ================================================================
    // REFLECTION UTIL
    // ================================================================

   
}
internal static class ReflectionUtil
{
    private const BindingFlags Flags =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance;

    // ============================================================
    // GET FIELD / PROPERTY
    // ============================================================

    public static object GetFieldOrPropertyValue(
        object target,
        string name)
    {
        if (target == null)
            return null;

        Type type =
            target.GetType();

        FieldInfo field =
            type.GetField(
                name,
                Flags
            );

        if (field != null)
        {
            return field.GetValue(
                target
            );
        }

        PropertyInfo property =
            type.GetProperty(
                name,
                Flags
            );

        if (property != null &&
            property.CanRead)
        {
            return property.GetValue(
                target
            );
        }

        return null;
    }

    // ============================================================
    // SET FIELD / PROPERTY
    // ============================================================

    public static bool TrySetFieldOrPropertyValue(
        object target,
        string name,
        object value)
    {
        if (target == null)
            return false;

        Type type =
            target.GetType();

        // --------------------------------------------------------
        // FIELD
        // --------------------------------------------------------

        FieldInfo field =
            type.GetField(
                name,
                Flags
            );

        if (field != null)
        {
            if (!CanAssign(
                    field.FieldType,
                    value))
            {
                return false;
            }

            field.SetValue(
                target,
                value
            );

            return true;
        }

        // --------------------------------------------------------
        // PROPERTY
        // --------------------------------------------------------

        PropertyInfo property =
            type.GetProperty(
                name,
                Flags
            );

        if (property != null &&
            property.CanWrite)
        {
            if (!CanAssign(
                    property.PropertyType,
                    value))
            {
                return false;
            }

            property.SetValue(
                target,
                value
            );

            return true;
        }

        return false;
    }

    // ============================================================
    // TYPE CHECK
    // ============================================================

    private static bool CanAssign(
        Type targetType,
        object value)
    {
        // Reference type / nullable
        if (value == null)
        {
            return
                !targetType.IsValueType ||
                Nullable.GetUnderlyingType(
                    targetType
                ) != null;
        }

        Type valueType =
            value.GetType();

        return
            targetType.IsAssignableFrom(
                valueType
            );
    }
}
