using NabaGame.Core.Runtime.EventManager;

public class MoneyChange : GameEvent
{
    public int CurrentMoney;

    public MoneyChange(){}
    
    public MoneyChange(int currentMoney)
    {
        CurrentMoney = currentMoney;
    }
}

public class StarChange : GameEvent
{
    public int CurrentStar;

    public StarChange(){}
    
    public StarChange(int currentStar)
    {
        CurrentStar = currentStar;
    }
}

public class LevelChange : GameEvent
{
    public int CurrentLevel;
    
    public LevelChange(){}

    public LevelChange(int currentLevel)
    {
        CurrentLevel = currentLevel;
    }
}

public class MusicChange : GameEvent
{
    public bool currentMusicStatus;

    public MusicChange(bool currentMusicStatus)
    {
        this.currentMusicStatus = currentMusicStatus;
    }
}

public class SoundChange : GameEvent
{
    public bool currentSoundStatus;

    public SoundChange(bool currentSoundStatus)
    {
        this.currentSoundStatus = currentSoundStatus;
    }
}


public class VibrationChange : GameEvent
{
    public bool currentVibrationStatus;

    public VibrationChange(bool currentVibrationStatus)
    {
        this.currentVibrationStatus = currentVibrationStatus;
    }
}

public class ThemeChange : GameEvent
{
    public bool currentTheme;

    public ThemeChange(bool currentTheme)
    {
        this.currentTheme = currentTheme;
    }
}


public class HeartChange : GameEvent
{
    public int currentHeart;

    public HeartChange(int currentHeart)
    {
        this.currentHeart = currentHeart;
    }
}

public class MaxHeartChange : GameEvent
{
    public int maxHeart;

    public MaxHeartChange(int maxHeart)
    {
        this.maxHeart = maxHeart;
    }
}

public class UpdateBoosterCount : GameEvent
{
    public UpdateBoosterCount() { }
}


public class HintBoosterUseCountChange : GameEvent
{
    public int currentHintBoosterUseCount;

    public HintBoosterUseCountChange(int currentHintBoosterUseCount)
    {
        this.currentHintBoosterUseCount = currentHintBoosterUseCount;
    }
}

public class HammerBoosterUseCountChange : GameEvent
{
    public int currentHammerBoosterUseCount;

    public HammerBoosterUseCountChange(int currentHammerBoosterUseCount)
    {
        this.currentHammerBoosterUseCount = currentHammerBoosterUseCount;
    }
}

public class MagicWandBoosterUseCountChange : GameEvent
{
    public int currentMagicWandBoosterUseCount;

    public MagicWandBoosterUseCountChange(int currentMagicWandBoosterUseCount)
    {
        this.currentMagicWandBoosterUseCount = currentMagicWandBoosterUseCount;
    }
}

public class PiggyBankValueChange : GameEvent
{
    public int currentPiggyBankValue;

    public PiggyBankValueChange(){}
    
    public PiggyBankValueChange(int currentPiggyBankValue)
    {
        this.currentPiggyBankValue = currentPiggyBankValue;
    }
}