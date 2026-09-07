
public static class EnumHelper
{
    public static T GetRandomEnum<T>()
    {
        T[] values = (T[])System.Enum.GetValues(typeof(T));
        return values[UnityEngine.Random.Range(0, values.Length)];
    }
}



public enum BlockMovement : byte
{
    None,
    Up,
    Down,
    Left,
    Right,
}

public enum BlockJointConnectType : byte
{
    None,
    Connect_Upward,
    Connect_Downward,
    Connect_Leftward,
    Connect_Rightward,
}

public enum EffectID
{
    Click,
    HammerHit,
    MagicWandTrail,
}

public enum PathStatus : byte
{
    None,
    Clear,
    Blocked,
}
public enum BoosterType
{
    Hammer,
    Hint,
    Magic,
}


public enum FrameID 
{
    Frame_1,
    Frame_2,
    Frame_3,
    Frame_4,
    Frame_5,
    Frame_6,
    Frame_7,
    Frame_8,
    Frame_9,
    Frame_10,
    Frame_11,
    Frame_12,
    Frame_Sunny,
    Frame_Flower
}

public enum AvatarID
{
    Rabit,
    Lion,
    Bee,
    Dear,
    Cat,
    Monkey,
    Giraffe,
    Pig,
    Panda,
    Dog,
    Penguin,
    Crocodile,
    Dolphin,
    Parrot,
    Fox,
    Sunny,
    SunFlower,
}
public enum ShopPackageType
{
    NoAds,
    VipNoAds,
    VIPOffer,
    Coin_500,
    Coin_2500,
    Coin_10000,
    Coin_25000,
    Coin_50000,
    Coin_100000,
    StarterBundle,
    EliteBundle,
    EpicBundle,
    PiggyBank,
    SpringBattlePass,
    SuperOffer
}

public enum RewardType
{
    Coin,
    Hint_Booster,
    Hammer_Booster,
    MagicWand_Booster,
    Avatar_Icon,
    Avatar_Frame,
    Infinite_Heart,
    EpicBundle,
}

public enum UnlockType 
{
    Level,
    Sparkling_Summer_Reward
}
public enum IconType
{
    Coin,
    Infinite_Heart,
    Gift
}

public enum QuestType 
{
    Clear_Block,
    Collect_Coin,
    Watch_Ads,
    Use_Booster,
    Collect_Daily_Badge,
    Collect_Weekly_Badge,
}