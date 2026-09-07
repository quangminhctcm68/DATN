using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawAvatar 
{
	public AvatarID AvatarID;
	public int LevelComplete;
	public UnlockType UnlockType;
}

public class RawAvatarData : ScriptableObject 
{
    [TableList]
    public List<RawAvatar> rawAvatars = new List<RawAvatar>();
}
