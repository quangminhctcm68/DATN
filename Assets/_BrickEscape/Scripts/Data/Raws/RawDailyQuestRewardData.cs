using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawDailyQuestReward 
{
	public int RequireBadge;
	public RewardType RewardType;
	public int Qty;
}

public class RawDailyQuestRewardData : ScriptableObject 
{
    [TableList]
    public List<RawDailyQuestReward> rawDailyQuestRewards = new List<RawDailyQuestReward>();
}
