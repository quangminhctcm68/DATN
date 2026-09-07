using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawBattlePassRewardsPremium 
{
	public int LevelNumber;
	public RewardType RewardType;
	public int CoinAmount;
	public int BoosterAddAmount;
	public AvatarID AvatarIconID;
	public FrameID AvatarFrameID;
	public int InfiniteHeartDuration;
}

public class RawBattlePassRewardsPremiumData : ScriptableObject 
{
    [TableList]
    public List<RawBattlePassRewardsPremium> rawBattlePassRewardsPremiums = new List<RawBattlePassRewardsPremium>();
}
