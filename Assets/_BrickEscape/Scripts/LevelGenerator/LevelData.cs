using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace ArrowMaze
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "_BrickEscape/LevelData")]
    public class LevelData : ScriptableObject
    {
        public List<RawLevelData> rawLevelDatas = new List<RawLevelData>();
        public List<LevelInfo> LevelInfos;
        public int maxLevelNumber;

#if UNITY_EDITOR
        [Button("Load All Raw Levels", ButtonSizes.Large)] // Button Odin
        public void LoadAllRawLevels()
        {
            string folderPath = "Assets/_BrickEscape/Data/Raws";

            // Tìm tất cả asset dạng RawLevel trong folder
            string[] guids = AssetDatabase.FindAssets("t:RawLevelData", new[] { folderPath });

            rawLevelDatas.Clear();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                RawLevelData level = AssetDatabase.LoadAssetAtPath<RawLevelData>(path);

                if (level != null)
                    rawLevelDatas.Add(level);
            }

            EditorUtility.SetDirty(this);

            Debug.Log($"[LevelData] Loaded {rawLevelDatas.Count} RawLevel assets!");
        }

        [Button("Convert Level", ButtonSizes.Large)]
        void LoadLevel()
        {
            LevelInfos.Clear();

            foreach (var rawData in rawLevelDatas)
            {
                if (rawData.rawLevels.Count == 0)
                    continue;

                LevelInfo info = new LevelInfo();
                info.LevelID = rawData.rawLevels[0].LevelID;
                //info.Move = rawData.rawLevels[0].Move;
                info.IsHard = rawData.rawLevels[0].Hard;
                info.CellSize = rawData.rawLevels[0].CellSize;
                info.IsTime = rawData.rawLevels[0].Time;
                info.LevelTime = rawData.rawLevels[0].LevelTime;

                // Tìm tất cả field dạng "Col1", "Col2", "Col3", ...
                var fields = typeof(RawLevel).GetFields();
                var colFields = new List<System.Reflection.FieldInfo>();

                foreach (var f in fields)
                {
                    if (f.Name.StartsWith("Col"))
                        colFields.Add(f);
                }

                // Tạo ColumInfos theo số lượng Col tự động
                for (int i = 0; i < colFields.Count; i++)
                    info.ColumInfos.Add(new ColumInfo());

                // Duyệt từng row của level
                foreach (var row in rawData.rawLevels)
                {
                    for (int c = 0; c < colFields.Count; c++)
                    {
                        // Lấy list<int> raw
                        var rawList = (List<int>)colFields[c].GetValue(row);

                        // Tạo wrapper để Unity serialize được
                        IntList wrapper = new IntList();
                        wrapper.Values.AddRange(rawList);

                        // Add vào column tương ứng
                        info.ColumInfos[c].Colums.Add(wrapper);
                    }
                }

                LevelInfos.Add(info);
            }

            // Lưu file
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("<color>LoadLevel (Optimized + Serializable) — DONE!</color>");

            int maxNum = -1;
            foreach (var level in LevelInfos)
            {
                if (level.LevelID > maxNum)
                    maxNum = level.LevelID;
            }
            
            maxLevelNumber = maxNum;
        }
#endif
        
        #region Get Level Data
        
        public LevelInfo GetLevelInfo(int levelID)
        {
            int targetID = levelID;
            if (levelID > maxLevelNumber)
            {
                int minLoop = 21;
                int maxLoop = 70;
                
                // int tempNum = (levelID + 21) % maxLevelNumber + 1;
                // tempNum = Mathf.Clamp(tempNum, 21, maxLevelNumber);
                // foreach (var level in LevelInfos)
                // {
                //     if (level.LevelID == tempNum)
                //         return level;
                // }
                //
                // return null;
                
                int rangeLength = maxLoop - minLoop + 1;
                targetID = minLoop + (levelID - minLoop) % rangeLength;
            }
            
            foreach (var level in LevelInfos)
            {
                if (level.LevelID == targetID)
                    return level;
            }
            
            return null;
        }
        
        #endregion
    }
}

[System.Serializable]
public class LevelInfo
{
    public int LevelID;
    public bool IsHard;
    public int Move;
    public float CellSize;
    public bool IsTime;
    public float LevelTime;
    public List<ColumInfo> ColumInfos;

    public LevelInfo()
    {
        ColumInfos = new List<ColumInfo>(20);
    }
}


[System.Serializable]
public class ColumInfo
{
    //[Sirenix.OdinInspector.ShowInInspector]
    public List<IntList> Colums = new List<IntList>();
}

[System.Serializable]
public class IntList
{
    public List<int> Values = new List<int>();
}