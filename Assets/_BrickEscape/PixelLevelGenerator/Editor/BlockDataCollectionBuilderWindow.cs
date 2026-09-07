// BlockDataCollectionBuilderWindow.cs
//
// Editor tool de build/populate PixelGeneratorBlockDataCollection tu 1 list
// prefab MoveableBlock (hoac bat ky GameObject nao co Collider2D + blockJoints,
// vi BlockCatalogBuilder chi can 2 thu do).
//
// Cach dung:
// 1. Tools/Brick Escape/Pixel Level Generator/Build Block Data Collection.
// 2. Keo tha cac prefab MoveableBlock vao list (hoac bam "Scan Folder" de tu dong
//    quet tat ca prefab trong 1 thu muc, loc theo component MoveableBlock/BlockBase
//    neu tim thay type do trong project).
// 3. Chon ID cho tung block:
//      - "Auto-assign sequential" -> danh so 0..N-1 theo thu tu list.
//      - "Try read from component" -> doc field "PrefabId"/"BlockId"/"Id" tren
//        component cua prefab (neu co) bang reflection, fallback ve sequential
//        neu khong tim thay.
// 4. Bam "Create / Update Asset" -> chon noi luu (.asset) hoac ghi de asset dang co.
//
// Sau khi co asset, keo no vao o "Data Collection" cua PixelLevelGeneratorWindow.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BrickEscape.PixelLevelGenerator
{
    public class BlockDataCollectionBuilderWindow : EditorWindow
    {
        [Serializable]
        private class DraftEntry
        {
            public GameObject Prefab;
            public int Id;
            public bool IdManuallyEdited;
        }

        private enum IdAssignMode
        {
            AutoSequential,
            TryReadFromComponent
        }

        // =================================================================
        // STATE
        // =================================================================

        private readonly List<DraftEntry> _drafts = new List<DraftEntry>();

        private PixelGeneratorBlockDataCollection _targetAsset;

        private IdAssignMode _idMode = IdAssignMode.TryReadFromComponent;

        private Vector2 _scroll;

        private string _lastLog = "--";


        // =================================================================
        // OPEN
        // =================================================================

        [MenuItem("Tools/Brick Escape/Build Block Data Collection")]
        public static void Open()
        {
            var win = GetWindow<BlockDataCollectionBuilderWindow>(
                "Block Data Collection Builder");

            win.minSize = new Vector2(560, 480);

            win.Show();
        }


        // =================================================================
        // GUI
        // =================================================================

        private void OnGUI()
        {
            DrawTargetAssetSection();

            EditorGUILayout.Space(6);

            DrawSourceListSection();

            EditorGUILayout.Space(6);

            DrawIdAssignmentSection();

            EditorGUILayout.Space(6);

            DrawEntriesTable();

            EditorGUILayout.Space(6);

            DrawBuildSection();
        }


        // =================================================================
        // TARGET ASSET
        // =================================================================

        private void DrawTargetAssetSection()
        {
            EditorGUILayout.LabelField(
                "Target Asset (optional - de trong se hoi vi tri luu khi Create)",
                EditorStyles.boldLabel);

            _targetAsset =
                (PixelGeneratorBlockDataCollection)EditorGUILayout.ObjectField(
                    "Existing asset",
                    _targetAsset,
                    typeof(PixelGeneratorBlockDataCollection),
                    false);

            if (_targetAsset != null &&
                GUILayout.Button("Load entries from this asset"))
            {
                LoadFromAsset(_targetAsset);
            }
        }


        // =================================================================
        // SOURCE LIST
        // =================================================================

        private void DrawSourceListSection()
        {
            EditorGUILayout.LabelField(
                "Source Prefabs",
                EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("+ Add Prefab Slot", GUILayout.Width(140)))
            {
                _drafts.Add(new DraftEntry());
            }

            if (GUILayout.Button("Scan Folder...", GUILayout.Width(120)))
            {
                ScanFolder();
            }

            if (GUILayout.Button("Clear List", GUILayout.Width(90)))
            {
                if (EditorUtility.DisplayDialog(
                        "Clear List",
                        "Xoa toan bo danh sach dang nhap?",
                        "Xoa",
                        "Huy"))
                {
                    _drafts.Clear();
                }
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "Ban co the keo-tha nhieu prefab cung luc vao khung ben duoi.",
                MessageType.Info);

            Rect dropRect =
                GUILayoutUtility.GetRect(
                    0, 40,
                    GUILayout.ExpandWidth(true));

            GUI.Box(dropRect, "Keo prefab vao day");

            HandleDragAndDrop(dropRect);
        }

        private void HandleDragAndDrop(Rect dropRect)
        {
            Event evt = Event.current;

            if (!dropRect.Contains(evt.mousePosition))
                return;

            if (evt.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                evt.Use();
            }
            else if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();

                foreach (var obj in DragAndDrop.objectReferences)
                {
                    if (obj is GameObject go)
                    {
                        _drafts.Add(new DraftEntry { Prefab = go });
                    }
                }

                AssignIds();

                evt.Use();
            }
        }

        private void ScanFolder()
        {
            string folder =
                EditorUtility.OpenFolderPanel(
                    "Scan Folder for Block Prefabs",
                    "Assets",
                    "");

            if (string.IsNullOrEmpty(folder))
                return;

            if (!folder.StartsWith(Application.dataPath))
            {
                EditorUtility.DisplayDialog(
                    "Scan Folder",
                    "Chon thu muc ben trong Assets/.",
                    "OK");

                return;
            }

            string relative =
                "Assets" + folder.Substring(Application.dataPath.Length);

            string[] guids =
                AssetDatabase.FindAssets("t:Prefab", new[] { relative });

            int added = 0;

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                var prefab =
                    AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (prefab == null)
                    continue;

                // Loc so bo: chi lay prefab co Collider2D (dieu kien toi thieu
                // ma BlockCatalogBuilder can), tranh keo nhung prefab khong lien quan.
                if (prefab.GetComponentInChildren<Collider2D>(true) == null)
                    continue;

                _drafts.Add(new DraftEntry { Prefab = prefab });

                added++;
            }

            AssignIds();

            _lastLog =
                $"Scan xong: them {added} prefab tu '{relative}'.";
        }


        // =================================================================
        // ID ASSIGNMENT
        // =================================================================

        private void DrawIdAssignmentSection()
        {
            EditorGUILayout.LabelField(
                "ID Assignment",
                EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            _idMode =
                (IdAssignMode)EditorGUILayout.EnumPopup(
                    "Mode",
                    _idMode);

            if (GUILayout.Button("Re-assign IDs now", GUILayout.Width(140)))
            {
                AssignIds();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.HelpBox(
                "TryReadFromComponent: doc field 'PrefabId'/'BlockId'/'Id' tren " +
                "component cua prefab (neu co). Neu khong tim thay se fallback " +
                "ve so thu tu (sequential). ID da sua tay se khong bi ghi de.",
                MessageType.None);
        }

        private void AssignIds()
        {
            int sequential = 0;

            foreach (var draft in _drafts)
            {
                if (draft.IdManuallyEdited)
                {
                    sequential = Mathf.Max(sequential, draft.Id + 1);
                    continue;
                }

                int resolvedId = sequential;

                if (_idMode == IdAssignMode.TryReadFromComponent &&
                    draft.Prefab != null)
                {
                    if (TryReadIdFromPrefab(draft.Prefab, out int fromComponent))
                    {
                        resolvedId = fromComponent;
                    }
                }

                draft.Id = resolvedId;

                sequential = Mathf.Max(sequential, resolvedId + 1);
            }

            Repaint();
        }

        private static bool TryReadIdFromPrefab(GameObject prefab, out int id)
        {
            id = 0;

            Component[] components =
                prefab.GetComponentsInChildren<Component>(true);

            string[] candidateNames =
                { "PrefabId", "BlockId", "Id" };

            foreach (Component component in components)
            {
                if (component == null)
                    continue;

                foreach (string name in candidateNames)
                {
                    object value =
                        GetFieldOrPropertyValue(component, name);

                    if (value is int intValue)
                    {
                        id = intValue;
                        return true;
                    }
                }
            }

            return false;
        }

        private static object GetFieldOrPropertyValue(object target, string name)
        {
            const BindingFlags flags =
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            Type type = target.GetType();

            FieldInfo field = type.GetField(name, flags);

            if (field != null)
                return field.GetValue(target);

            PropertyInfo property = type.GetProperty(name, flags);

            if (property != null && property.CanRead)
                return property.GetValue(target);

            return null;
        }


        // =================================================================
        // ENTRIES TABLE
        // =================================================================

        private void DrawEntriesTable()
        {
            EditorGUILayout.LabelField(
                $"Entries ({_drafts.Count})",
                EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("Prefab", GUILayout.Width(260));
            GUILayout.Label("ID", GUILayout.Width(60));
            GUILayout.Label("Collider2D", GUILayout.Width(80));
            GUILayout.Label("blockJoints", GUILayout.Width(90));
            GUILayout.FlexibleSpace();

            EditorGUILayout.EndHorizontal();

            _scroll = EditorGUILayout.BeginScrollView(
                _scroll,
                GUILayout.Height(220));

            for (int i = 0; i < _drafts.Count; i++)
            {
                DrawEntryRow(i);
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawEntryRow(int index)
        {
            var draft = _drafts[index];

            EditorGUILayout.BeginHorizontal();

            var newPrefab =
                (GameObject)EditorGUILayout.ObjectField(
                    draft.Prefab,
                    typeof(GameObject),
                    false,
                    GUILayout.Width(260));

            if (newPrefab != draft.Prefab)
            {
                draft.Prefab = newPrefab;
                AssignIds();
            }

            int newId =
                EditorGUILayout.IntField(
                    draft.Id,
                    GUILayout.Width(60));

            if (newId != draft.Id)
            {
                draft.Id = newId;
                draft.IdManuallyEdited = true;
            }

            bool hasCollider =
                draft.Prefab != null &&
                draft.Prefab.GetComponentInChildren<Collider2D>(true) != null;

            GUILayout.Label(
                hasCollider ? "OK" : "Missing",
                GUILayout.Width(80));

            bool hasJoints =
                draft.Prefab != null &&
                HasNonEmptyBlockJoints(draft.Prefab);

            GUILayout.Label(
                hasJoints ? "OK" : "Missing",
                GUILayout.Width(90));

            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                _drafts.RemoveAt(index);

                GUIUtility.ExitGUI();
            }

            EditorGUILayout.EndHorizontal();
        }

        private static bool HasNonEmptyBlockJoints(GameObject prefab)
        {
            Component[] components =
                prefab.GetComponentsInChildren<Component>(true);

            foreach (Component component in components)
            {
                if (component == null)
                    continue;

                object value =
                    GetFieldOrPropertyValue(component, "blockJoints")
                    ?? GetFieldOrPropertyValue(component, "BlockJoints");

                if (value is System.Collections.IEnumerable enumerable &&
                    !(value is string))
                {
                    foreach (var _ in enumerable)
                        return true;
                }
            }

            return false;
        }


        // =================================================================
        // BUILD / SAVE
        // =================================================================

        private void DrawBuildSection()
        {
            EditorGUILayout.Space(4);

            var duplicateIds =
                _drafts
                    .GroupBy(d => d.Id)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

            var missingPrefabs =
                _drafts.Count(d => d.Prefab == null);

            if (duplicateIds.Count > 0)
            {
                EditorGUILayout.HelpBox(
                    "ID trung nhau: " +
                    string.Join(", ", duplicateIds),
                    MessageType.Error);
            }

            if (missingPrefabs > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{missingPrefabs} slot chua gan prefab.",
                    MessageType.Warning);
            }

            bool canBuild =
                _drafts.Count > 0 &&
                duplicateIds.Count == 0 &&
                missingPrefabs == 0;

            GUI.enabled = canBuild;

            if (GUILayout.Button(
                    _targetAsset != null
                        ? "Update Existing Asset"
                        : "Create New Asset",
                    GUILayout.Height(30)))
            {
                BuildAsset();
            }

            GUI.enabled = true;

            EditorGUILayout.LabelField("Log", _lastLog);
        }

        private void BuildAsset()
        {
            PixelGeneratorBlockDataCollection asset = _targetAsset;

            if (asset == null)
            {
                string path =
                    EditorUtility.SaveFilePanelInProject(
                        "Create Block Data Collection",
                        "PixelGeneratorBlockDataCollection",
                        "asset",
                        "Chon vi tri luu asset");

                if (string.IsNullOrEmpty(path))
                    return;

                asset =
                    ScriptableObject.CreateInstance<PixelGeneratorBlockDataCollection>();

                AssetDatabase.CreateAsset(asset, path);

                _targetAsset = asset;
            }
            else
            {
                Undo.RecordObject(
                    asset,
                    "Update Block Data Collection");
            }

            asset.blocks.Clear();

            foreach (var draft in _drafts.OrderBy(d => d.Id))
            {
                asset.blocks.Add(new BlockPrefabEntry
                {
                    Id = draft.Id,
                    Prefab = draft.Prefab
                });
            }

            EditorUtility.SetDirty(asset);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(asset);

            _lastLog =
                $"Da luu {asset.blocks.Count} block vao '" +
                AssetDatabase.GetAssetPath(asset) + "'.";
        }

        private void LoadFromAsset(PixelGeneratorBlockDataCollection asset)
        {
            _drafts.Clear();

            foreach (var entry in asset.blocks)
            {
                if (entry == null)
                    continue;

                _drafts.Add(new DraftEntry
                {
                    Prefab = entry.Prefab,
                    Id = entry.Id,
                    IdManuallyEdited = true
                });
            }

            _lastLog =
                $"Da load {_drafts.Count} entry tu asset.";

            Repaint();
        }
    }
}
