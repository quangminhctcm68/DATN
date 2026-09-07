using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawMonthlyQuest 
{
	public int QuestNo;
	public int RequireWeeklyBadge;
	public int Reward_MonthlyBadge;
}

public class RawMonthlyQuestData : ScriptableObject 
{
    [TableList]
    public List<RawMonthlyQuest> rawMonthlyQuests = new List<RawMonthlyQuest>();
}
