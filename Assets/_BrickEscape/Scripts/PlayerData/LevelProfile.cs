using System;
using System.Collections;
using System.Collections.Generic;
using NabaGame.Core.Runtime.EventManager;
using Newtonsoft.Json;
using UnityEngine;

namespace BrickEscape
{
    [Serializable]
    public class LevelSaveData
    {
        [JsonProperty("i")] public int levelID = 0;
        [JsonProperty("f")] public bool isFinished = false;
        [JsonProperty("s")] public int starGained = 0;
        
        public LevelSaveData(){}

        public LevelSaveData(int levelID, bool isFinished, int starGained)
        {
            this.levelID = levelID;
            this.isFinished = isFinished;
            this.starGained = starGained;
        }
    }
    
    public class LevelProfile
    {
        public List<LevelSaveData> levelDatas = new List<LevelSaveData>();
        public int currentLevel = 0;
        private string levelDataJson;
        private LevelChange levelChangeEvent;

        #region Process Save Data

        public void SaveThisLevelData(int levelID, bool levelStatus, int starGained, bool autoSave = false)
        {
            LevelSaveData tempDataRef = GetLevelData(levelID);
            if (tempDataRef != null)
            {
                if (!tempDataRef.isFinished)
                    tempDataRef.isFinished = levelStatus;
                if (tempDataRef.starGained < starGained)
                {
                    GameManager.Instance.PlayerProfile.ChangeStar(starGained - tempDataRef.starGained);
                    tempDataRef.starGained = starGained;
                }
            }
            else
            {
                tempDataRef = new LevelSaveData(levelID, levelStatus, starGained);
                GameManager.Instance.PlayerProfile.ChangeStar(starGained);
                levelDatas.Add(tempDataRef);
            }

            UpdateCurrentLevel(levelID);

            if (autoSave)
            {
                SaveLevelData();
                SaveMisc();
            }
        }

        public void UpdateCurrentLevel(int levelID)
        {
            if (levelID >= currentLevel)
            {
                currentLevel++;
                if (levelChangeEvent == null) levelChangeEvent = new LevelChange();
                levelChangeEvent.CurrentLevel = currentLevel;
                EventManager.Instance.Raise(levelChangeEvent);
            }
        }

        public void CheatLevel(int levelID)
        {
            int starToSet = 0;
            foreach (var data in levelDatas)
            {
                if (data.levelID < levelID)
                {
                    data.isFinished = true;
                    data.starGained = 2;
                    starToSet += 2;
                }
                else
                {
                    data.isFinished = false;
                    data.starGained = 0;
                }
            }
            
            GameManager.Instance.PlayerProfile.SetStar(starToSet);
            currentLevel = levelID;
        }
        
        #endregion
        
        #region Getters, Setters

        public LevelSaveData GetLevelData(int levelID)
        {
            for (int i = 0; i < levelDatas.Count; i++)
            {
                if (levelDatas[i].levelID == levelID)
                    return levelDatas[i];
            }

            LevelSaveData tempDataRef = new  LevelSaveData(levelID, false, 0);
            levelDatas.Add(tempDataRef);
            return tempDataRef;
        }

        public int CurrentLevel => currentLevel;
        
        #endregion
        
        #region Save, Load

        public void SaveLevelData()
        {
            levelDataJson = JsonConvert.SerializeObject(levelDatas);
            PlayerPrefs.SetString(StringConsts.LEVEL_SAVE_DATA, levelDataJson);
            PlayerPrefs.Save();
        }

        public void SaveMisc()
        {
            PlayerPrefs.SetInt(StringConsts.CURRENT_LEVEL, currentLevel);
            PlayerPrefs.Save();
        }

        public void LoadLevelData()
        {
            if (PlayerPrefs.HasKey(StringConsts.LEVEL_SAVE_DATA))
                levelDatas = JsonConvert.DeserializeObject<List<LevelSaveData>>(PlayerPrefs.GetString(StringConsts.LEVEL_SAVE_DATA));
            else
                levelDatas = new List<LevelSaveData>();
            
            currentLevel = PlayerPrefs.GetInt(StringConsts.CURRENT_LEVEL, 1);
        }

        #endregion
    }
}
