using EasyTransition;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class UIMainManager : UIManagerSingleton<UIMainManager>
    {

        public HomePanel homePanel;

        //public MainMenuPanel mainMenuPanel;
        public GamePlayPanel gamePlayPanel;
        public SettingPanel settingPanel;
        public PausePanel pausePanel;
        public FailPanel failPanel;
        public WinPanel winPanel;
        public ProfilePanel profilePanel;   
        public RatingPanel ratingPanel;
        public NoNetworkPanel noNetworkPanel;
        public InterAdsPanel adsPanel;
        public MoreBoosterPanel moreBoosterPanel;
        //public TransitionSettings transition;
        public OutOfStarPanel outOfStarPanel;
        public ClickEffectPanel clickEffectPanel;
        public OutOfHeartPanel outOfHeartPanel;
        public PiggyBankPanel piggyBankPanel;
        public BattlepassPanel battlepassPanel;
        public DailyRewardPanel dailyRewardPanel;
        public DailyQuestPanel dailyQuestPanel;
        public ChestOpenPanel chestOpenPanel;
        public BattlepassConfirmPurchasePanel battlepassConfirmPurchasePanel;
        public RewardGetPanel rewardGetPanel;
        public EpicBundlePanel epicBundlePanel;
        public NoAdsPanel NoAdsPanel;
        public PurchaseCompletePanel purchaseCompletePanel;
        public RacePanel racePanel;
        public FindingPanel findingPanel;
        public RaceInforPanel raceInforPanel;
        public WinRacePanel winRacePanel;
        public JoinRacePanel joinRacePanel;
        private ThemeChange themeChangeEvent;
        public ReplayPanel replayPanel;
        public UnlockBoosterPanel unlockBoosterPanel;

        private void OnValidate()
        {
            homePanel = GetComponentInChildren<HomePanel>();
            gamePlayPanel = GetComponentInChildren<GamePlayPanel>();
            failPanel = GetComponentInChildren<FailPanel>();
            winPanel = GetComponentInChildren<WinPanel>();
            settingPanel = GetComponentInChildren<SettingPanel>();
            pausePanel = GetComponentInChildren<PausePanel>();
            profilePanel = GetComponentInChildren<ProfilePanel>();
            adsPanel = GetComponentInChildren<InterAdsPanel>();
            ratingPanel = GetComponentInChildren<RatingPanel>();
            outOfStarPanel = GetComponentInChildren<OutOfStarPanel>();  
            moreBoosterPanel = GetComponentInChildren<MoreBoosterPanel>();
            noNetworkPanel = GetComponentInChildren<NoNetworkPanel>();
            outOfHeartPanel = GetComponentInChildren<OutOfHeartPanel>();
            piggyBankPanel = GetComponentInChildren<PiggyBankPanel>();
            battlepassPanel = GetComponentInChildren<BattlepassPanel>();
            dailyRewardPanel = GetComponentInChildren<DailyRewardPanel>();
            dailyQuestPanel = GetComponentInChildren<DailyQuestPanel>();
            chestOpenPanel = GetComponentInChildren<ChestOpenPanel>();
            rewardGetPanel = GetComponentInChildren<RewardGetPanel>();
            battlepassConfirmPurchasePanel = GetComponentInChildren<BattlepassConfirmPurchasePanel>();
            epicBundlePanel = GetComponentInChildren<EpicBundlePanel>();
            NoAdsPanel = GetComponentInChildren<NoAdsPanel>();
            purchaseCompletePanel = GetComponentInChildren<PurchaseCompletePanel>();
            joinRacePanel = GetComponentInChildren<JoinRacePanel>();
            racePanel = GetComponentInChildren<RacePanel>();
            winRacePanel = GetComponentInChildren<WinRacePanel>();
            raceInforPanel = GetComponentInChildren<RaceInforPanel>();  
            findingPanel = GetComponentInChildren<FindingPanel>();  
            replayPanel = GetComponentInChildren<ReplayPanel>();
            unlockBoosterPanel = GetComponentInChildren<UnlockBoosterPanel>();
        }
        void Start()
        {
            SetInfo();
        }
        
        private void Update()
        {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                noNetworkPanel.SetShow();
            }
            else
            {
                noNetworkPanel.SetHide();
            }

            homePanel.UpdateCoolDown();


        }
        
        public void SetInfo()
        {
            dailyRewardPanel.SetInfor();
            homePanel.SetInfor();
            failPanel.SetInfor();
            gamePlayPanel.SetInfor();
            settingPanel.SetInfo();
            pausePanel.SetInfo();
            profilePanel.SetInfor();
            winPanel.SetInfor();
            ratingPanel.SetInfor();
            adsPanel.SetInfo();
            outOfStarPanel.SetInfor();
            moreBoosterPanel.SetInfor();
            piggyBankPanel.SetInfo();
            dailyQuestPanel.SetInfor();
            chestOpenPanel.SetInfor();
            battlepassPanel.SetInfo();
            battlepassConfirmPurchasePanel.SetInfo();
 
            joinRacePanel.SetInfor();
            findingPanel.SetInfor();
            racePanel.SetInfor();
            raceInforPanel.SetInfor();
            NoAdsPanel.SetInfor();
            winRacePanel.SetInfor();
            epicBundlePanel.SetInfo();
            replayPanel.SetInfor();
            unlockBoosterPanel.SetInfor();
            if (themeChangeEvent == null) themeChangeEvent = new ThemeChange(GameManager.Instance.PlayerProfile.isDarkTheme);
            else
                themeChangeEvent.currentTheme = GameManager.Instance.PlayerProfile.isDarkTheme;
            EventManager.Instance.Raise(themeChangeEvent);
        }

        public bool IsGamePause()
        {
            if (settingPanel.IsVisible() || failPanel.IsVisible() || winPanel.IsVisible() ||
                pausePanel.IsVisible() || ratingPanel.IsVisible() || outOfStarPanel.IsVisible()
                || moreBoosterPanel.IsVisible() || outOfHeartPanel.IsVisible() || piggyBankPanel.IsVisible()
                || battlepassPanel.IsVisible() || dailyRewardPanel.IsVisible() || battlepassConfirmPurchasePanel.IsVisible()
                || epicBundlePanel.IsVisible() || purchaseCompletePanel.IsVisible())
                return true;
            return false;
        }
    }
}