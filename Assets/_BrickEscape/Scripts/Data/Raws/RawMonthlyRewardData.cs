using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawMonthlyReward 
{
	public int RequireBadge;
	public RewardType RewardType;
	public int Qty;
}

public class RawMonthlyRewardData : ScriptableObject 
{
    [TableList]
    public List<RawMonthlyReward> rawMonthlyRewards = new List<RawMonthlyReward>();
}
