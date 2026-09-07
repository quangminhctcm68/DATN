using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

[System.Serializable]
public class RawShop 
{
	public ShopPackageType ShopPackageType;
	public string PackageName;
	public string PackageID;
	public float Amout;
	public string Price;
	public string FakePrice;
	public int GoldBonus;
	public int InfinityHeartBonus;
	public int HeartBonus;
	public int MagicWandBonus;
	public int HintBonus;
	public int HammerBonus;
	public string FakeSaleTag;
	public bool IsNoads;
}

public class RawShopData : ScriptableObject 
{
    [TableList]
    public List<RawShop> rawShops = new List<RawShop>();
}
