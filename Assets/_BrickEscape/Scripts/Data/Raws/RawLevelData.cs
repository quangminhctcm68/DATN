using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawLevel 
{
	public int LevelID;
	public bool Hard;
	public float OffsetCamea;
	public float CellSize;
	public bool Time;
	public float LevelTime;
	public  List<int>Col1;
	public  List<int>Col2;
	public  List<int>Col3;
	public  List<int>Col4;
	public  List<int>Col5;
	public  List<int>Col6;
	public  List<int>Col7;
	public  List<int>Col8;
	public  List<int>Col9;
	public  List<int>Col10;
	public  List<int>Col11;
	public  List<int>Col12;
	public  List<int>Col13;
	public  List<int>Col14;
	public  List<int>Col15;
	public  List<int>Col16;
	public  List<int>Col17;
	public  List<int>Col18;
	public  List<int>Col19;
	public  List<int>Col20;
	public  List<int>Col21;
	public  List<int>Col22;
	public  List<int>Col23;
	public  List<int>Col24;
	public  List<int>Col25;
	public  List<int>Col26;
	public  List<int>Col27;
	public  List<int>Col28;
	public  List<int>Col29;
	public  List<int>Col30;
	public  List<int>Col31;
	public  List<int>Col32;
	public  List<int>Col33;
	public  List<int>Col34;
	public  List<int>Col35;
	public  List<int>Col36;
	public  List<int>Col37;
	public  List<int>Col38;
	public  List<int>Col39;
	public  List<int>Col40;
	public  List<int>Col41;
	public  List<int>Col42;
	public  List<int>Col43;
	public  List<int>Col44;
	public  List<int>Col45;
	public  List<int>Col46;
	public  List<int>Col47;
	public  List<int>Col48;
	public  List<int>Col49;
	public  List<int>Col50;
}

public class RawLevelData : ScriptableObject 
{
    [TableList]
    public List<RawLevel> rawLevels = new List<RawLevel>();
}
