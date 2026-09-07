using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawFrame 
{
	public FrameID FrameID;
	public int LevelComplete;
	public UnlockType UnlockType;
}

public class RawFrameData : ScriptableObject 
{
    [TableList]
    public List<RawFrame> rawFrames = new List<RawFrame>();
}
