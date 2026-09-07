using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
[CreateAssetMenu(fileName = "SpriteCollection", menuName = "GameData/SpriteCollection")]

public class SpriteCollection : SerializedScriptableObject
{
    public Dictionary<FontColor,Material> fontDict = new Dictionary<FontColor, Material>();
    public Dictionary<ButtonColor, Color> ColorDict = new Dictionary<ButtonColor,Color>();
    public Dictionary<SpriteUI,Sprite> spriteDic = new Dictionary<SpriteUI,Sprite>();
    public Dictionary<FrameID, Sprite> frameDic = new Dictionary<FrameID, Sprite>();
    public Dictionary<AvatarID, Sprite> avatarDic = new Dictionary<AvatarID, Sprite>();
    public Dictionary<BoosterType, Sprite> BoostetDic = new Dictionary<BoosterType, Sprite>();
    public Dictionary<IconType,Sprite> Icon = new Dictionary<IconType,Sprite>();
    public Dictionary<QuestType, Sprite> QuesrIcon = new Dictionary<QuestType, Sprite>();

    [FoldoutGroup("Theme")]
    public float lightOpacity;
    [FoldoutGroup("Theme")]
    public float darkOpacity;
    [FoldoutGroup("Theme")]
    public Color lightColor;
    [FoldoutGroup("Theme")]
    public Color darkColor;    


    public Sprite GetAvatarSprite(AvatarID avatarID)
    {
        if (avatarDic.ContainsKey(avatarID)) return avatarDic[avatarID];
        else return null;
    }
    
    public Sprite GetFrameSprite(FrameID frameID)
    {
        if (frameDic.ContainsKey(frameID)) return frameDic[frameID];
        else return null;
    }
}

public enum SpriteUI 
{
    Button_Down_Gold,
    Button_Down_Silver,  
    Star_Empty,
    Star_Full,
    Setting_On,
    Setting_Off,
}


public enum ButtonColor 
{
      Orange_Up = 0,
      Orange_Down = 1,
      Gray_Up = 2,  
      Gray_Down = 3,
      Green_Up = 4,
      Green_Down = 5,
      Red_Up = 6,
      Red_Down = 7,
}
public enum FontColor
{
    Black = 0,
    Blue = 1,
    Gray = 2,
    Orange = 3,
    Green = 4,
    Red = 5,
    Pink = 6,
}