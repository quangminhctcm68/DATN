using System;
using BrickEscape.BrickEscape;
using NabaGame.Core.Runtime.EventManager;
using MoreMountains.NiceVibrations;
using UnityEngine;
using BMH.Ads;
using JetBrains.Annotations;

namespace BrickEscape
{
    public class PlayerProfile
    {
        public int Coin;
        public int Star;
        public int HintBoosterUseCount;
        public int HammerBoosterUseCount;
        public int MagicWandBoosterUseCount;
        public int isNoAds;

        public int lastClaimDay;   
        public int rewardDailyIndex;
        public int rewardPackIndex;

        public bool JoinedRace;
        public int currRemainPlayer;
        public int CurrentRaceLv;

        public float RaceCoolDown;



        public bool isFirstLogin;
        public bool isShowFirstTimeInGame;
        public bool buySupperOffer;





        public int trackingDailyReward;
        public string epicBundleStartDay;

        public bool SoundSetting;
        public bool MusicSetting;
        public bool VibrationSetting;
        public bool isDarkTheme;

        public bool FinishedTutorialType_1;
        public bool FinishedTutorialType_2;
        public FrameID currentFrame;
        public AvatarID currentAvatar;

        public LevelProfile LevelProfile;
        public QuestProfile QuestProfile;
        public AvatarProfile avatarProfile;
        public HeartProfile HeartProfile;
        public PiggyBankProfile PiggyBankProfile;
        public BattlepassProfile BattlepassProfile;

        private MoneyChange moneyChangeEvent;
        private StarChange starChangeEvent;
        private SoundChange soundChangeEvent;
        private MusicChange musicChangeEvent;
        private ThemeChange themeChangeEvent;
        private VibrationChange vibrationChangeEvent;
        private HintBoosterUseCountChange hintBoosterUseCountChangeEvent;
        private HammerBoosterUseCountChange hammerBoosterUseCountChangeEvent;
        private MagicWandBoosterUseCountChange magicWandBoosterUseCountChangeEvent;
        private UpdateBoosterCount updateBoosterCount;
      
        
        public PlayerProfile()
        {
            LevelProfile = new LevelProfile();
            avatarProfile = new AvatarProfile();
            HeartProfile = new HeartProfile();
            PiggyBankProfile = new PiggyBankProfile();
            BattlepassProfile = new BattlepassProfile();
            QuestProfile = new QuestProfile();
            LoadData();
        }
        
        #region Change Data
        
        //Level
        // public void ChangeLevel(int specificNumber)
        // {
        //     if (specificNumber < 0)
        //         CurrentLevel = 0;
        //     else
        //         CurrentLevel = specificNumber;
        //     if (levelChangeEvent == null) levelChangeEvent = new LevelChange();
        //     levelChangeEvent.CurrentLevel = CurrentLevel;
        //     
        //     EventManager.Instance.Raise(levelChangeEvent);
        // }
        
        //Money
        public void ChangeCoin(int amount)
        {
            Coin += amount;
            if (Coin < 0)
                Coin = 0;

            if (moneyChangeEvent == null) moneyChangeEvent = new MoneyChange();
            moneyChangeEvent.CurrentMoney = Coin;

            if (amount > 0) 
            {
                QuestProfile.AddDailyCollectCointCount(amount);
            }
            EventManager.Instance.Raise(moneyChangeEvent);
            //UIMainManager.Instance.homePanel.UpdateCoin();
        }

        public void ChangeStar(int amount)
        {
            Star += amount;
            if (Star < 0)
                Star = 0;
            if (starChangeEvent == null) starChangeEvent = new StarChange();
            starChangeEvent.CurrentStar = Star;
            EventManager.Instance.Raise(starChangeEvent);
            UIMainManager.Instance.homePanel.UpdateStar();
        }
        
        public void SetStar(int amount)
        {
            Star = amount;
            if (Star < 0)
                Star = 0;
            if (starChangeEvent == null) starChangeEvent = new StarChange();
            starChangeEvent.CurrentStar = Star;
            EventManager.Instance.Raise(starChangeEvent);
            UIMainManager.Instance.homePanel.UpdateStar();
        }

        //Settings
        public void ChangeSoundSetting()
        {
            SoundSetting = !SoundSetting;
            if (soundChangeEvent == null) soundChangeEvent = new SoundChange(SoundSetting);
            soundChangeEvent.currentSoundStatus = SoundSetting;
            EventManager.Instance.Raise(soundChangeEvent);
        }
        
        public void ChangeMusicSetting()
        {
            MusicSetting = !MusicSetting;
            if (musicChangeEvent == null) musicChangeEvent = new MusicChange(MusicSetting);
            musicChangeEvent.currentMusicStatus = MusicSetting;
            EventManager.Instance.Raise(musicChangeEvent);
            
        }
        
        public void ChangeVibrationSetting()
        {
            VibrationSetting = !VibrationSetting;
            if (vibrationChangeEvent == null) vibrationChangeEvent = new VibrationChange(VibrationSetting);
            vibrationChangeEvent.currentVibrationStatus = VibrationSetting;
            MMVibrationManager.SetHapticsActive(VibrationSetting);
            EventManager.Instance.Raise(vibrationChangeEvent);
        }

        public void ChangeThemeSetting()
        {
            isDarkTheme = !isDarkTheme;
            if (themeChangeEvent == null) themeChangeEvent = new ThemeChange(isDarkTheme);
            else
                themeChangeEvent.currentTheme = isDarkTheme;
            EventManager.Instance.Raise(themeChangeEvent);
            
        }




        //Tutorial
        public void ChangeTutorialType_1Status(bool status)
        {
            FinishedTutorialType_1 = status;
        }

        public void ChangeTutorialType_2Status(bool status)
        {
            FinishedTutorialType_2 = status;
        }



        public void ResetMoonRace() 
        {
            JoinedRace = false;
            currRemainPlayer = 100;
            CurrentRaceLv = 0;
        }

        
        //Booster
        public void ChangeHintBoosterUseCount(int changeAmount)
        {
            HintBoosterUseCount += changeAmount;
            if (HintBoosterUseCount < 0)
                HintBoosterUseCount = 0;
            if (hintBoosterUseCountChangeEvent == null) hintBoosterUseCountChangeEvent = new HintBoosterUseCountChange(HintBoosterUseCount);
            hintBoosterUseCountChangeEvent.currentHintBoosterUseCount = HintBoosterUseCount;
            if (changeAmount < 0)
                QuestProfile.AddDailyUseBoosterCount(-changeAmount);

            EventManager.Instance.Raise(hintBoosterUseCountChangeEvent);
            if (updateBoosterCount == null) updateBoosterCount = new UpdateBoosterCount();
            EventManager.Instance.Raise(updateBoosterCount);
        }
        
        public void ChangeHammerBoosterUseCount(int changeAmount)
        {
            HammerBoosterUseCount += changeAmount;
            if (HammerBoosterUseCount < 0)
                HammerBoosterUseCount = 0;
            if (hammerBoosterUseCountChangeEvent == null) hammerBoosterUseCountChangeEvent = new HammerBoosterUseCountChange(HammerBoosterUseCount);
            hammerBoosterUseCountChangeEvent.currentHammerBoosterUseCount = HammerBoosterUseCount;
            if (changeAmount < 0)
                QuestProfile.AddDailyUseBoosterCount(-changeAmount);
            EventManager.Instance.Raise(hammerBoosterUseCountChangeEvent);
            if (updateBoosterCount == null) updateBoosterCount = new UpdateBoosterCount();
            EventManager.Instance.Raise(updateBoosterCount);
        
        }
        
        public void ChangeMagicWandBoosterUseCount(int changeAmount)
        {
            MagicWandBoosterUseCount += changeAmount;
            if (MagicWandBoosterUseCount < 0)
                MagicWandBoosterUseCount = 0;
            if (magicWandBoosterUseCountChangeEvent == null) magicWandBoosterUseCountChangeEvent = new MagicWandBoosterUseCountChange(MagicWandBoosterUseCount);
            magicWandBoosterUseCountChangeEvent.currentMagicWandBoosterUseCount = MagicWandBoosterUseCount;

            if (changeAmount < 0)
                QuestProfile.AddDailyUseBoosterCount(-changeAmount);

            EventManager.Instance.Raise(magicWandBoosterUseCountChangeEvent);
            if (updateBoosterCount == null) updateBoosterCount = new UpdateBoosterCount();
            EventManager.Instance.Raise(updateBoosterCount);

        }
        
        // Epic Bundle
        public void UpdateStartDay()
        {
            epicBundleStartDay = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
        }
        
        public long EpicBundleStartDay => long.Parse(epicBundleStartDay);
        
        #endregion
        
        #region Load, Save
        
        public void SaveData()
        {



            //PlayerPrefs.SetInt(StringConsts.CURRENT_LEVEL, CurrentLevel);
            PlayerPrefs.SetInt(StringConsts.COIN, Coin);
            PlayerPrefs.SetInt(StringConsts.STAR, Star);
            PlayerPrefs.SetInt(StringConsts.SOUND_SETTING, SoundSetting ? 1 : 0);
            PlayerPrefs.SetInt(StringConsts.MUSIC_SETTING, MusicSetting ? 1 : 0);
            PlayerPrefs.SetInt(StringConsts.VIBRATION_SETTING, VibrationSetting ? 1 : 0);
            PlayerPrefs.SetInt(StringConsts.THEME_SETTING, isDarkTheme ? 1 : 0);
            PlayerPrefs.SetInt(StringConsts.FINISHED_TUTORIAL_TYPE_1, FinishedTutorialType_1 ? 1 : 0);
            PlayerPrefs.SetInt(StringConsts.FINISHED_TUTORIAL_TYPE_2, FinishedTutorialType_2 ? 1 : 0);
            


            PlayerPrefs.SetInt(StringConsts.HINT_BOOSTER_USE_COUNT, HintBoosterUseCount);
            PlayerPrefs.SetInt(StringConsts.HAMMER_BOOSTER_USE_COUNT, HammerBoosterUseCount);
            PlayerPrefs.SetInt(StringConsts.MAGIC_WAND_BOOSTER_USE_COUNT, MagicWandBoosterUseCount);
            PlayerPrefs.SetInt(StringConsts.NOADS, isNoAds);
            PlayerPrefs.SetInt(StringConsts.AVA_KEY,(int)currentAvatar);
            PlayerPrefs.SetInt(StringConsts.FRAME_KEY, (int)currentFrame);
            //PlayerPrefs.SetInt(StringConsts.RATED_5_STARS, Rated5Stars ? 1 : 0);
            PlayerPrefs.SetInt(StringConsts.LAST_CLAIM_DAY,lastClaimDay);
            PlayerPrefs.SetInt(StringConsts.REWARD_PACK_INDEX,rewardPackIndex);
            PlayerPrefs.SetInt(StringConsts.TRACK_REWARD_DAILY,trackingDailyReward);
            PlayerPrefs.SetInt(StringConsts.REWARD_DAILY_INDEX, rewardDailyIndex);
            PlayerPrefs.SetString(StringConsts.EPIC_BUNDLE_START_DAY, epicBundleStartDay);

            PlayerPrefs.SetInt(StringConsts.JOINED_RACE, JoinedRace ? 1 : 0);
            PlayerPrefs.SetInt(StringConsts.CURRENT_RACE_LV,CurrentRaceLv);
            PlayerPrefs.SetInt(StringConsts.REMAIN_PLAYER, currRemainPlayer);

            PlayerPrefs.SetFloat(StringConsts.RACE_COOLDOWN, RaceCoolDown);


            PlayerPrefs.SetInt(StringConsts.BUYED_SUPPER_OFFER, buySupperOffer ? 1 : 0);

            PlayerPrefs.SetInt(StringConsts.SHOW_SUPPER_OFFER_FIST_TIME,isShowFirstTimeInGame ? 1 : 0);



            LevelProfile.SaveLevelData();
            LevelProfile.SaveMisc();
            avatarProfile.Save();
            PiggyBankProfile.SavePiggyBankData();
            BattlepassProfile.Save();

            HeartProfile.SetLastLoginTime();
            HeartProfile.Save();
            QuestProfile.Save();
            PlayerPrefs.Save();
        }
        
        public void LoadData()
        {
            //CurrentLevel = PlayerPrefs.GetInt(StringConsts.CURRENT_LEVEL, 1);
            isFirstLogin = !PlayerPrefs.HasKey(StringConsts.FINISHED_TUTORIAL_TYPE_1);



            Coin = PlayerPrefs.GetInt(StringConsts.COIN, 0);
            Star = PlayerPrefs.GetInt(StringConsts.STAR, 0);
            SoundSetting = PlayerPrefs.GetInt(StringConsts.SOUND_SETTING, 1) == 1;
            MusicSetting = PlayerPrefs.GetInt(StringConsts.MUSIC_SETTING, 1) == 1;
            VibrationSetting = PlayerPrefs.GetInt(StringConsts.VIBRATION_SETTING, 1) == 1;
            isDarkTheme = PlayerPrefs.GetInt(StringConsts.THEME_SETTING, 1) == 1;

            FinishedTutorialType_1 = PlayerPrefs.GetInt(StringConsts.FINISHED_TUTORIAL_TYPE_1, 0) == 1;
            FinishedTutorialType_2 = PlayerPrefs.GetInt(StringConsts.FINISHED_TUTORIAL_TYPE_2, 0) == 1;
            currentAvatar = (AvatarID)PlayerPrefs.GetInt(StringConsts.AVA_KEY, (int)AvatarID.Parrot);
            currentFrame = (FrameID)PlayerPrefs.GetInt(StringConsts.FRAME_KEY, (int)FrameID.Frame_1);
            HintBoosterUseCount = PlayerPrefs.GetInt(StringConsts.HINT_BOOSTER_USE_COUNT, 0);
            HammerBoosterUseCount = PlayerPrefs.GetInt(StringConsts.HAMMER_BOOSTER_USE_COUNT, 0);
            MagicWandBoosterUseCount = PlayerPrefs.GetInt(StringConsts.MAGIC_WAND_BOOSTER_USE_COUNT, 0);
            isNoAds = PlayerPrefs.GetInt(StringConsts.NOADS,0);
            lastClaimDay = PlayerPrefs.GetInt(StringConsts.LAST_CLAIM_DAY,-1);
            rewardDailyIndex = PlayerPrefs.GetInt(StringConsts.REWARD_DAILY_INDEX, 0);
            rewardPackIndex = PlayerPrefs.GetInt(StringConsts.REWARD_PACK_INDEX, 0);
            trackingDailyReward = PlayerPrefs.GetInt(StringConsts.TRACK_REWARD_DAILY, 0);
            
            RaceCoolDown = PlayerPrefs.GetFloat(StringConsts.RACE_COOLDOWN,0);
            if (PlayerPrefs.HasKey(StringConsts.EPIC_BUNDLE_START_DAY))
                epicBundleStartDay = PlayerPrefs.GetString(StringConsts.EPIC_BUNDLE_START_DAY);
            else
                UpdateStartDay();
            
            buySupperOffer = PlayerPrefs.GetInt(StringConsts.BUYED_SUPPER_OFFER, 0) == 1;

            isShowFirstTimeInGame = PlayerPrefs.GetInt(StringConsts.SHOW_SUPPER_OFFER_FIST_TIME, 0) == 1;




            JoinedRace =PlayerPrefs.GetInt(StringConsts.JOINED_RACE,0)==1;
            CurrentRaceLv =PlayerPrefs.GetInt(StringConsts.CURRENT_RACE_LV,0);
            currRemainPlayer=PlayerPrefs.GetInt(StringConsts.REMAIN_PLAYER,100);
            //Rated5Stars = PlayerPrefs.GetInt(StringConsts.RATED_5_STARS, 0) == 1;
            avatarProfile.Load();
            LevelProfile.LoadLevelData();
            HeartProfile.Load();
            PiggyBankProfile.LoadPiggyBankData();
            QuestProfile.Load();
            
            BattlepassProfile.Load();
        }

        
        #endregion
    }
}
