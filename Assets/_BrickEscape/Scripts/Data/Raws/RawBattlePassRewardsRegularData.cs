using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawBattlePassRewardsRegular 
{
	public int LevelNumber;
	public int RequiredExp;
	public RewardType RewardType;
	public int CoinAmount;
	public int BoosterAddAmount;
	public AvatarID AvatarIconID;
	public FrameID AvatarFrameID;
	public int InfiniteHeartDuration;
}

public class RawBattlePassRewardsRegularData : ScriptableObject 
{
    [TableList]
    public List<RawBattlePassRewardsRegular> rawBattlePassRewardsRegulars = new List<RawBattlePassRewardsRegular>();
}
