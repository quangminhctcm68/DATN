using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawWeeklyReward 
{
	public int RequireBadge;
	public RewardType RewardType;
	public int Qty;
}

public class RawWeeklyRewardData : ScriptableObject 
{
    [TableList]
    public List<RawWeeklyReward> rawWeeklyRewards = new List<RawWeeklyReward>();
}
