using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawDailyReward 
{
	public int ID;
	public RewardType RewardType;
	public int Qty;
}

public class RawDailyRewardData : ScriptableObject 
{
    [TableList]
    public List<RawDailyReward> rawDailyRewards = new List<RawDailyReward>();
}
