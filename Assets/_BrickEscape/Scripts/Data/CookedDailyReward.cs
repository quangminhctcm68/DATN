using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BrickEscape
{
    [CreateAssetMenu(fileName = "CookedDailyReward", menuName = "GameData/CookedDailyReward")]
    public class CookedDailyReward : SerializedScriptableObject
    {
       
        public List<DailyRewardPack> dailyRewardPacks;
        [SerializeField] RawDailyRewardData data;

        [Button]
        public void BuildData()
        {
            dailyRewardPacks.Clear();

            if (data == null || data.rawDailyRewards == null || data.rawDailyRewards.Count == 0)
            {
                Debug.LogWarning("RawDailyRewards is empty!");
                return;
            }

            const int packSize = 7;
            List<RawDailyReward> source = data.rawDailyRewards;

            for (int i = 0; i < source.Count; i += packSize)
            {
                DailyRewardPack pack = new DailyRewardPack();
                pack.dailyRewards = new List<RawDailyReward>();

                for (int j = 0; j < packSize && (i + j) < source.Count; j++)
                {
                    pack.dailyRewards.Add(source[i + j]);
                }

                dailyRewardPacks.Add(pack);
            }
        }




    }

 
}
[System.Serializable]
public class DailyRewardPack
{

    public List<RawDailyReward> dailyRewards;
}