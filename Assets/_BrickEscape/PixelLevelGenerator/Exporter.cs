// Chuyen candidate da chon thanh RawLevelData asset dung format cell cu.
// Tham chieu: TAI_LIEU_THIET_KE_TOOL_PIXEL_TO_RAW_LEVEL.txt muc 12; TAI_LIEU_HE_THONG_LEVEL_GENERATION.txt muc 3.
//
// GHI CHU TICH HOP: gia dinh RawLevelData co List<RawLevel> "rawLevels", moi RawLevel co field
// LevelID, Hard, CellSize, Time, LevelTime, OffsetCamea va Col1..Col50 kieu List<int>. Day la
// class production khong nam trong bo tai lieu duoc cung cap, nen exporter dung reflection de
// gan gia tri field theo dung ten quy uoc trong TAI_LIEU_HE_THONG_LEVEL_GENERATION.txt muc 3.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using BrickEscape.PixelLevelGenerator.Model;
using System.Reflection;
namespace BrickEscape.PixelLevelGenerator
{
    public enum OverwriteChoice
    {
        Cancel,
        CreateCopy,
        Overwrite
    }

    public class ExportRequest
    {
        public LevelCandidate Candidate;
        public int LevelId;
        public string OutputFolder = "Assets/_BrickEscape/Data/Raws";
        public string AssetName; // vd RawLevel71.asset
        public float CellSize = 1f;
        public bool Hard;
        public OverwriteChoice OverwriteChoice = OverwriteChoice.Cancel;
        public Type RawLevelDataType;
        public Type RawLevelRowType;
    }

    public class ExportResult
    {
        public bool Success;
        public string AssetPath;
        public List<string> Log = new List<string>();
    }

    public static class RawLevelExporter
    {
        private const int MaxColumns = LevelValidator.MaxColumns;

        public static bool AssetExists(string folder, string assetName)
        {
            string path = CombinePath(folder, assetName);
            return AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null;
        }

        public static string CombinePath(string folder, string assetName) => folder.TrimEnd('/') + "/" + assetName;

        public static ExportResult Export(ExportRequest request)
        {
            var result = new ExportResult();
            var candidate = request.Candidate;

            if (!candidate.CanExport())
            {
                result.Log.Add("Candidate chua PASS validator (solver/schema). Export bi tu choi.");
                return result;
            }

            if (request.RawLevelDataType == null || request.RawLevelRowType == null)
            {
                result.Log.Add("ERROR: Chua cau hinh Type cho RawLevelData/RawLevel. " +
                                "Gan RawLevelExporter.ExportRequest.RawLevelDataType/RawLevelRowType truoc khi goi Export " +
                                "(vd typeof(RawLevelData), typeof(RawLevel)).");
                return result;
            }

            string path = CombinePath(request.OutputFolder, request.AssetName);

            if (!Directory.Exists(request.OutputFolder))
                Directory.CreateDirectory(request.OutputFolder);

            ScriptableObject asset;
            bool assetExists = AssetExists(request.OutputFolder, request.AssetName);

            if (assetExists)
            {
                if (request.OverwriteChoice == OverwriteChoice.Cancel)
                {
                    result.Log.Add("Export huy: file da ton tai va GD chua chon Overwrite/Create Copy.");
                    return result;
                }
                if (request.OverwriteChoice == OverwriteChoice.CreateCopy)
                {
                    string baseName = Path.GetFileNameWithoutExtension(request.AssetName);
                    string ext = Path.GetExtension(request.AssetName);
                    string candidateName = $"{baseName}_Copy{ext}";
                    int suffix = 2;
                    while (AssetExists(request.OutputFolder, candidateName))
                    {
                        candidateName = $"{baseName}_Copy{suffix}{ext}";
                        suffix++;
                    }
                    path = CombinePath(request.OutputFolder, candidateName);
                    asset = ScriptableObject.CreateInstance(request.RawLevelDataType);
                    AssetDatabase.CreateAsset(asset, path);
                }
                else // Overwrite
                {
                    asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    Undo.RecordObject(asset, "Overwrite RawLevelData (Pixel Level Generator)");
                    ClearRawLevels(asset);
                }
            }
            else
            {
                asset = ScriptableObject.CreateInstance(request.RawLevelDataType);
                AssetDatabase.CreateAsset(asset, path);
            }

            var rawLevelsList = GetOrCreateRawLevelsList(asset, request.RawLevelRowType);

            int height = candidate.GridSize.y;
            int width = candidate.GridSize.x;

            // Tao mot dictionary origin -> BlockPlacement de tra cuu nhanh khi duyet grid.
            var placementByOrigin = candidate.Blocks.ToDictionary(b => b.Origin, b => b);

            for (int y = 0; y < height; y++)
            {
                object row = Activator.CreateInstance(request.RawLevelRowType);
                ReflectionUtil.TrySetFieldOrPropertyValue(row, "LevelID", request.LevelId);
                ReflectionUtil.TrySetFieldOrPropertyValue(row, "Hard", request.Hard);
                ReflectionUtil.TrySetFieldOrPropertyValue(row, "CellSize", request.CellSize);
                ReflectionUtil.TrySetFieldOrPropertyValue(row, "Time", false);
                ReflectionUtil.TrySetFieldOrPropertyValue(row, "LevelTime", 0f);
                ReflectionUtil.TrySetFieldOrPropertyValue(row, "OffsetCamea", 0f);

                for (int x = 0; x < MaxColumns; x++)
                {
                    List<int> values;
                    if (x >= width)
                    {
                        values = new List<int> { -1 };
                    }
                    else if (placementByOrigin.TryGetValue(new Vector2Int(x, y), out var placement))
                    {
                        values = new List<int>
                        {
                            placement.PrefabId, placement.ColorId, placement.MoveId,
                            placement.ArrowStartIndex, placement.ArrowLength
                        };
                    }
                    else
                    {
                        bool isBoard = candidate.BoardMask[x, y];
                        values = isBoard ? new List<int> { 0, 0, 0, 0, 0 } : new List<int> { -1, 0, 0, 0, 0 };
                    }

                    string colFieldName = $"Col{x + 1}";
                    if (!ReflectionUtil.TrySetFieldOrPropertyValue(row, colFieldName, values))
                    {
                        result.Log.Add($"WARNING: Khong tim thay field {colFieldName} tren RawLevel row type.");
                    }
                }

                rawLevelsList.Add(row);
            }

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            result.Success = true;
            result.AssetPath = path;
            result.Log.Add($"Da tao/ghi de RawLevelData tai {path} voi {height} hang, {MaxColumns} cot.");
            return result;
        }

        private static System.Collections.IList GetOrCreateRawLevelsList(ScriptableObject asset, Type rowType)
        {
            object listObj = ReflectionUtil.GetFieldOrPropertyValue(asset, "rawLevels") ??
                              ReflectionUtil.GetFieldOrPropertyValue(asset, "RawLevels");

            if (listObj is System.Collections.IList existingList)
                return existingList;

            var listType = typeof(List<>).MakeGenericType(rowType);
            var newList = (System.Collections.IList)Activator.CreateInstance(listType);
            ReflectionUtil.TrySetFieldOrPropertyValue(asset, "rawLevels", newList);
            return newList;
        }

        private static void ClearRawLevels(ScriptableObject asset)
        {
            object listObj = ReflectionUtil.GetFieldOrPropertyValue(asset, "rawLevels") ??
                              ReflectionUtil.GetFieldOrPropertyValue(asset, "RawLevels");
            if (listObj is System.Collections.IList list)
                list.Clear();
        }
    }
}