using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawDailyQuest 
{
	public int QuestNo;
	public QuestType QuestType;
	public int Qty;
	public int Reward_DailyBadge;
}

public class RawDailyQuestData : ScriptableObject 
{
    [TableList]
    public List<RawDailyQuest> rawDailyQuests = new List<RawDailyQuest>();
}
