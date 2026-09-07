using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawColor 
{
	public int ID;
	public string Color;
}

public class RawColorData : ScriptableObject 
{
    [TableList]
    public List<RawColor> rawColors = new List<RawColor>();
}
