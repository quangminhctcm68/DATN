using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawMove 
{
	public int MoveID;
	public BlockMovement MoveType;
}

public class RawMoveData : ScriptableObject 
{
    [TableList]
    public List<RawMove> rawMoves = new List<RawMove>();
}
