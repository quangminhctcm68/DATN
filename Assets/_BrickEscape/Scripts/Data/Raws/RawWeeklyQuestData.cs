using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawWeeklyQuest 
{
	public int QuestNo;
	public int RequireDailyBadge;
	public int Reward_WeeklyBadge;
}

public class RawWeeklyQuestData : ScriptableObject 
{
    [TableList]
    public List<RawWeeklyQuest> rawWeeklyQuests = new List<RawWeeklyQuest>();
}
