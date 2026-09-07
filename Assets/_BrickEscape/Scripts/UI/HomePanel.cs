using DG.Tweening;
using EasyTransition;
using GamesTan.UI;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.UI;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using NabaGame.Tracking;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using BMH.Ads;

namespace BrickEscape
{
    public class HomePanel : BaseUI
    {
        [FoldoutGroup("Play Area")]
        public Button PlayBtn;

        [FoldoutGroup("Play Area")]
        public Image PlayUp;

        [FoldoutGroup("Play Area")]
        public Image PlayDown;

        [FoldoutGroup("Play Area")]
        public Image Shadow;

        [FoldoutGroup("Play Area")]
        public Image InfiniteHeartIcon;

        [FoldoutGroup("Play Area")]
        public GameObject Tag;

        [FoldoutGroup("Play Area")]
        public TextMeshProUGUI State;

        [FoldoutGroup("Play Area")]
        public TextMeshProUGUI Level;



        [FoldoutGroup("Player Info")]
        public TextMeshProUGUI Heart;

        [FoldoutGroup("Player Info")]
        public TextMeshProUGUI HeartTimer;

        [FoldoutGroup("Player Info")]
        public TextMeshProUGUI Coin;

        [FoldoutGroup("Player Info")]
        public TextMeshProUGUI Star;

        [FoldoutGroup("Player Info")]
        public TextMeshProUGUI moonRaceText;


        [FoldoutGroup("Level Map")]
        public List<LevelButton> LevelButtons;

        [FoldoutGroup("Level Map")]
        public RectTransform fakeLine;

        [FoldoutGroup("Level Map")]
        public RectTransform Center;

        [FoldoutGroup("Level Map")]
        public LevelButton currentLevel;

        [FoldoutGroup("Level Map")]
        public ScrollRect scrollRect;

        [FoldoutGroup("Level Map")]
        public InfiniteVerticalLoopScroll _scroll;
        //public GridLayoutGroup grid;



        [FoldoutGroup("Top Buttons")]
        public Button AvatarBtn;

        [FoldoutGroup("Top Buttons")]
        public Button SettingBtn;

        [FoldoutGroup("Top Buttons")]
        public Button NoAdsBtn;

        [FoldoutGroup("Top Buttons")]
        public Button OpenPiggyBankBtn;

        [FoldoutGroup("Top Buttons")]
        public Button OpenBattlepassBtn;
        //public GameObject HomeTab;

        [FoldoutGroup("Top Buttons")]
        public Button DailyRewardBtn;

        [FoldoutGroup("Top Buttons")]
        public Button DailyQuestBtn;
        
        [FoldoutGroup("Top Buttons")]
        public Button EpicBundleBtn;

        [FoldoutGroup("Top Buttons")]
        public Button MoonRaceBtn;

        [FoldoutGroup("Battlepass Info")]
        public Image battlepassTimerFillImage;
        
        [FoldoutGroup("Battlepass Info")]
        public TextMeshProUGUI battlepassTimerText;
        
        [FoldoutGroup("Battlepass Info")]
        public SkeletonGraphic battlepassIconSkeletonGraphic;
        
        [FoldoutGroup("Battlepass Info")]
        public Image battlepassSunIcon;
        
        [FoldoutGroup("Battlepass Info")]
        public TextMeshProUGUI battlepassSunGainAmountText;

        [FoldoutGroup("Battlepass Info")]
        public bool showBattlepassGainAnimationOnStartup;


        [FoldoutGroup("Battlepass Info")]
        [SerializeField] private GameObject LockIcon;


        [FoldoutGroup("Battlepass Info")]
        public int gainAmount = 0;

        [FoldoutGroup("Battlepass Info")]
        public Transform animationPath_startPoint;
        
        [FoldoutGroup("Battlepass Info")]
        public Transform animationPath_middlePoint;
        
        [FoldoutGroup("Battlepass Info")]
        public Transform animationPath_endPoint;
        
        [FoldoutGroup("Battlepass Info")]
        public Vector3[] animationPath = new Vector3[3];
        
        [FoldoutGroup("Battlepass Info")]
        public ParticleSystem expReceiveEffect;
        
        [FoldoutGroup("Piggybank Info")]
        public GameObject breakBtn;
        
        
        [FoldoutGroup("Epic Bundle Info")]
        public TextMeshProUGUI epicBundleTimerText;
        

        [FoldoutGroup("Avatar")]
        public Image Avatar;

        [FoldoutGroup("Avatar")]
        public Image Frame;



        [FoldoutGroup("Tab System")]
        public GameObject HomeTab;

        [FoldoutGroup("Tab System")]
        public GameObject NoAdsPanel;

        [FoldoutGroup("Tab System")]
        public GameObject Top;

        [FoldoutGroup("Tab System")]
        public TabBarItem[] tabBarItems = new TabBarItem[3];


        [FoldoutGroup("Notification")]
        public GameObject Notify;

        [FoldoutGroup("Notification")]
        public GameObject DailyRewardNotify;
        
        [FoldoutGroup("Notification")]
        public GameObject DailyQuestNotify;
        
        [FoldoutGroup("Notification")]
        public GameObject battlepassNotificationIcon;

        [FoldoutGroup("Notification")]
        public GameObject moonRaceNotificationIcon;

        [FoldoutGroup("Notification")]
        [SerializeField] private GameObject Lock;

        [FoldoutGroup("Reward System")]
        public TimerReward timerReward;
        
       
        [FoldoutGroup("IAP")]
        public List<IAPItem> iAPItems;

        [FoldoutGroup("IAP")]
        public AnimButton[] AnimNoAds;

        [FoldoutGroup("IAP")]
        public GameObject NoAds1;

        [FoldoutGroup("IAP")]
        public GameObject NoAds2;


        [FoldoutGroup("Animation Cache")]
        private Sequence _animSeq;

        [FoldoutGroup("Animation Cache")]
        private Vector2 _upStartPos;

        [FoldoutGroup("Animation Cache")]
        private Vector2 _shadowStartPos;

        [FoldoutGroup("Callback")]
        private UpdateBoosterCount updateBoosterCount;

        private Sequence battlepassGainAnimation;

        PlayerProfile profile;
        GameManager gameManager;
        public void OnValidate()
        {
            iAPItems.Clear();
            iAPItems = GetComponentsInChildren<IAPItem>(true).ToList();
            timerReward = GetComponentInChildren<TimerReward>(true);
        }
        private void Awake()
        {
            _upStartPos = PlayUp.rectTransform.anchoredPosition;
            _shadowStartPos = Shadow.rectTransform.anchoredPosition;
           
        }

        public void CheckNoAds() 
        {
            if(GameManager.Instance.PlayerProfile.isNoAds == 1) 
            {
                NoAdsBtn.gameObject.SetActive(false);
                NoAds1.transform.SetAsLastSibling();
                NoAds2.transform.SetAsLastSibling();
                foreach(var item in AnimNoAds) 
                {
                    item.autoPlayOnAwake = false;
                    item.SetColor(ButtonColor.Gray_Up, ButtonColor.Gray_Down, FontColor.Gray);
                    item.ResetAnim();
                   
                }
            }
            UIMainManager.Instance.NoAdsPanel.CheckNoAds();
        }


        public LevelButton GetSeclectLevel()
        {
            if (LevelButtons == null || LevelButtons.Count == 0)
                return null;

            float fakeY = fakeLine.position.y;

            LevelButton bestAbove = null;
            float bestAboveDelta = float.MaxValue;

            LevelButton bestBelow = null;
            float bestBelowDelta = float.MaxValue;

            int currentID = GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel;

            for (int i = 0; i < LevelButtons.Count; i++)
            {
                LevelButton btn = LevelButtons[i];
                if (btn == null || btn.level > currentID)
                    continue;

                float y = btn.trans.position.y;
                float delta = y - fakeY;

                if (delta >= 0f)
                {
                    // nằm trên hoặc ngang fakeLine
                    if (delta < bestAboveDelta)
                    {
                        bestAboveDelta = delta;
                        bestAbove = btn;
                    }
                }
                else
                {
                    // nằm dưới fakeLine
                    float absDelta = -delta;
                    if (absDelta < bestBelowDelta)
                    {
                        bestBelowDelta = absDelta;
                        bestBelow = btn;
                    }
                }
            }

            // Ưu tiên trên trước
            return bestAbove != null ? bestAbove : bestBelow;
        }

        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            Frame.sprite = GameManager.Instance.spriteCollection.frameDic[GameManager.Instance.PlayerProfile.currentFrame];
            Avatar.sprite = GameManager.Instance.spriteCollection.avatarDic[GameManager.Instance.PlayerProfile.currentAvatar];
            UpdateSelect();
            UpdateCoin();
            UpdateStar();
            UIMainManager.Instance.dailyRewardPanel.CheckDay();
            UIMainManager.Instance.dailyQuestPanel.CheckNotify();
            if (GameController.Instance.boosterManager.hammerBoosterIsActive)
            {
                GameController.Instance.boosterManager.DeactivateHammerBooster(false);
                GameController.Instance.ChangePlayerControlState(true);
            }
            currentLevelID = GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel;
            DOVirtual.DelayedCall(0.25f, () => _scroll.ScrollToIndex(GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel,false))
                .OnComplete(
                delegate
                {
                    if (showBattlepassGainAnimationOnStartup)
                    {
                        showBattlepassGainAnimationOnStartup = false;
                        BattlepassExpAnimation();
                    }
                });
            AdManager.Instance.Hide(AdsType.Banner);
            UIMainManager.Instance.adsPanel.CheckOnShowNoAds();
            CheckMoonRaceNotify();
            CheckShowPack();
        }

        public void UpdateCoin()
        {
            int c =GameManager.Instance.PlayerProfile.Coin;
            Coin.text = FormatNumber.FormatNumberInt(c);
        }
        public void UpdateStar()
        {
            int s = GameManager.Instance.PlayerProfile.Star;
            Star.text = FormatNumber.FormatNumberInt(s);
        }

        public void SetInfor()
        {
            gameManager = GameManager.Instance;

            profile = GameManager.Instance.PlayerProfile;
            OnTapItemBar(0);
            for (int i = 0; i < LevelButtons.Count; i++)
            {
                 LevelSaveData data = GameManager.Instance.PlayerProfile.LevelProfile.GetLevelData(i+1);
                LevelButtons[i].SetInfor(data);
                LevelButtons[i].SetUpTween();
            }
            Frame.sprite = gameManager.spriteCollection.frameDic[profile.currentFrame];
            Avatar.sprite = gameManager.spriteCollection.avatarDic[profile.currentAvatar];
            AvatarBtn.onClick.AddListener(Profile);
            SettingBtn.onClick.AddListener(Setting);
            PlayBtn.onClick.AddListener(Play);
            NoAdsBtn.onClick.AddListener(BtnNoAds);
            OpenPiggyBankBtn.onClick.AddListener(BtnOpenPiggyBank);
            OpenBattlepassBtn.onClick.AddListener(BtnOpenBattlepass);
            DailyRewardBtn.onClick.AddListener(BtnOpeDailyReward);
            DailyQuestBtn.onClick.AddListener(BtnOpenDailyQuest);
            EpicBundleBtn.onClick.AddListener(BtnOpenEpicBundle);
            MoonRaceBtn.onClick.AddListener(OpenMoonRace);
            scrollRect.onValueChanged.AddListener(_ => UpdateSelect());

            UIMainManager.Instance.homePanel.EpicBundleBtn.gameObject.SetActive(!profile.buySupperOffer);
            UpdateCoin();
            UpdateStar();
            _scroll.Init(OnSetItem);
           

            foreach (var item in iAPItems)
            {
                item.SetInfor();
            }
            CheckNoAds();
            _scroll.ScrollToIndex(profile.LevelProfile.CurrentLevel, false);
            CheckMoonRaceNotify();

            animationPath_endPoint = OpenBattlepassBtn.transform;
            //CheckForBattlepassNotificationVisibility();
        }
        public void CheckShowPack() 
        {
            if(profile.LevelProfile.currentLevel >= 10 && !profile.isShowFirstTimeInGame && !profile.buySupperOffer) 
            {
                UIMainManager.Instance.epicBundlePanel.Show();
                profile.isShowFirstTimeInGame = true;
            }
        }

        public void Play() 
        {
            GameController.Instance.audioManager.PlayButtonSound();
            if (currentLevel.starCount >= 3) return;
            if (GameManager.Instance.PlayerProfile.HeartProfile.CurrentHeart <= 0 &&
                GameController.Instance.heartManager.GetRemainingUnlimitedHeartTime() <= 0)
            {
                UIMainManager.Instance.outOfHeartPanel.Open();
                return;
            }
            ResetAnim();
            Sequence seq = DOTween.Sequence();

            seq.AppendInterval(0.2f)
               .AppendCallback(() =>
               {
                   UIMainManager.Instance.homePanel.Hide();
               })
               .AppendInterval(0.3f)
               .AppendCallback(() =>
               {
                   GameController.Instance.levelGenerator.StartGeneratingLevel(currentLevelID);
                   UIMainManager.Instance.gamePlayPanel.SetText(currentLevelID);
                   UIMainManager.Instance.gamePlayPanel.Show();
                   TrackingManager.TrackEvent(TrackingEvent.StartLevel, TrackingParamter.Level, currentLevelID.ToString());
               });
            if (updateBoosterCount == null) updateBoosterCount = new UpdateBoosterCount();
            EventManager.Instance.Raise(updateBoosterCount);
        } 
        public void OnTapItemBar(int id) 
        {
            foreach(var item in tabBarItems) 
            {
                if(item.index != id) 
                {
                    item.OnUnSelected();
                }
                else 
                {
                    item.OnSelected();
                    if (item.isReady) 
                    {
                        ScrollCenter(id);   
                    }
                }
            }
            UIMainManager.Instance.dailyRewardPanel.CheckDay();
            UIMainManager.Instance.dailyQuestPanel.CheckNotify();
        }
        
        public void ScrollToLevel(ScrollRect scrollRect, GridLayoutGroup grid, int levelIndex, int totalLevelCount, bool smooth = false, float duration = 0.25f)
        {
            if (scrollRect == null || grid == null) return;

            // Clamp index
            levelIndex = Mathf.Clamp(levelIndex, 0, totalLevelCount - 1);

            // Vì startCorner = LowerRight
            // index 0 ở dưới → đảo ngược index

            float normalizedY;

            if (totalLevelCount <= 1)
            {
                normalizedY = 0f;
            }
            else
            {
                normalizedY = levelIndex / (float)(totalLevelCount - 1);
            }
            if(levelIndex == 0) 
            {
                normalizedY = 0f;
            }
            else 
            {
                normalizedY = Mathf.Clamp01(normalizedY + 0.013f);
            }
                

            if (smooth)
            {
                scrollRect.StopMovement();
                DG.Tweening.DOTweenModuleUI.DOVerticalNormalizedPos(
                       scrollRect,
                       normalizedY,
                        duration
                ).SetEase(Ease.OutCubic);
            }
            else
            {
                scrollRect.verticalNormalizedPosition = normalizedY;
            }
            
        }

        private Tween _scrollTween;
        public void ScrollCenter(int index) 
        {
            if(_scrollTween != null) 
            {
                _scrollTween.Kill();
            }
            float TargetX = 1404;
            foreach (var item in tabBarItems)
            {
                if (item?.TabPanel == null) continue;
                    item.TabPanel.SetActive(true);
            }
            _scrollTween = DG.Tweening.DOTweenModuleUI.DOAnchorPosX(
                    Center,
                    -TargetX*index,
                    0.3f,
                    false
                ).SetEase(Ease.OutSine).OnComplete(() => 
                {
                    foreach (var item in tabBarItems)
                    {
                        if (item.index != index)
                        {
                            if (item?.TabPanel == null) continue;
                                item.TabPanel.SetActive(false);
                        }
                    }

                });

        }

        public void Setting() 
        {

            UIMainManager.Instance.settingPanel.Show();
            GameController.Instance.audioManager.PlayButtonSound();
        }
        public void Profile()
        {
            UIMainManager.Instance.profilePanel.Show();
            GameController.Instance.audioManager.PlayButtonSound();
            TrackingManager.TrackEvent(TrackingEvent.Button_Click,
                TrackingParamter.Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Type,"avatar_btn");
        }
        public int currentLevelID;
        public void UpdateSelect() 
        {
            if (currentLevel != GetSeclectLevel()) 
            {
                if(currentLevel!= null) 
                {
                    currentLevel.UnSelect();
                }
                currentLevel = GetSeclectLevel();

                if (currentLevel == null) return;
                currentLevel.OnSelect();
                currentLevelID = currentLevel.level;
                PlayAnimBtn();
            }
        
        }

        public void OpenMoonRace() 
        {
            GameController.Instance.audioManager.PlayButtonSound();
            if(GameManager.Instance.PlayerProfile.LevelProfile.currentLevel < 10|| profile.RaceCoolDown > 0) return;
            if (!GameManager.Instance.PlayerProfile.JoinedRace) 
            {
                UIMainManager.Instance.joinRacePanel.Show();
            }
            else 
            {
                UIMainManager.Instance.racePanel.Show();
            }
           
        }

      


        private const float COOLDOWN_TIME = 600f;


        public void StartCooldown()
        {
            profile.RaceCoolDown = COOLDOWN_TIME;
            Lock.SetActive(true);
        }
        public void UpdateCoolDown()
        {
            if (profile.RaceCoolDown <= 0f || profile.LevelProfile.currentLevel < 10 || profile.JoinedRace) return;

            profile.RaceCoolDown -= Time.unscaledDeltaTime;
            UpdateUI(profile.RaceCoolDown);

            if (profile.RaceCoolDown <= 0f)
            {
                profile.RaceCoolDown = 0f;
                OnCooldownDone();
            }

     
        }
        void UpdateUI(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);

            moonRaceText.text = $"{minutes:D2}:{seconds:D2}";
        }

        public void OnCooldownDone()
        {
            CheckMoonRaceNotify();
            Lock.SetActive(false);
        }
        public void CheckMoonRaceNotify()
        {
            if (profile.RaceCoolDown <= 0) 
            {
                bool canJoin = GameManager.Instance.PlayerProfile.LevelProfile.currentLevel >= 25 && !GameManager.Instance.PlayerProfile.JoinedRace;
                moonRaceNotificationIcon.SetActive(canJoin);
                if (GameManager.Instance.PlayerProfile.LevelProfile.currentLevel >= 25)
                {
                    moonRaceText.text = "Moon Race";
                    Lock.SetActive(false);
                }
                else
                {
                    moonRaceText.text = "LV.25";
                    Lock.SetActive(true);
                }

            }
            else 
            {
                moonRaceNotificationIcon.SetActive(false);
            }
          
        }


        private void OnSetItem(ScrollItem itemRT, int index)
        {
            // Lấy component item và gọi SetInfo
            itemRT.SetInfo(index);
        }

        public void BtnNoAds() 
        {
            UIMainManager.Instance.NoAdsPanel.Show();
            GameController.Instance.audioManager.PlayButtonSound();
            TrackingManager.TrackEvent(TrackingEvent.Button_Click,
                TrackingParamter.Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Type,"noads_btn");
        }

        
        public void BtnOpenPiggyBank() 
        {
            GameController.Instance.audioManager.PlayButtonSound();
            UIMainManager.Instance.piggyBankPanel.Open();
            TrackingManager.TrackEvent(TrackingEvent.Button_Click,
                TrackingParamter.Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Type,"piggy_bank_btn");
        }

        public void BtnOpenBattlepass()
        {
            GameController.Instance.audioManager.PlayButtonSound();
            if (gameManager.PlayerProfile.LevelProfile.currentLevel < GameController.Instance.battlepassManager.LevelUnlock) return;
            UIMainManager.Instance.battlepassPanel.Open();
            Hide();
            TrackingManager.TrackEvent(TrackingEvent.Button_Click,
                TrackingParamter.Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Type,"battle_pass_btn");
        }
        public void BtnOpeDailyReward()
        {
            GameController.Instance.audioManager.PlayButtonSound();
            UIMainManager.Instance.dailyRewardPanel.Show();
            TrackingManager.TrackEvent(TrackingEvent.Button_Click,
                TrackingParamter.Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Type,"daily_reward_btn");
        }

        public void BtnOpenDailyQuest()
        {
            GameController.Instance.audioManager.PlayButtonSound();
            UIMainManager.Instance.dailyQuestPanel.Show();
            TrackingManager.TrackEvent(TrackingEvent.Button_Click,
                TrackingParamter.Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Type,"daily_quest_btn");
        }

        public void BtnOpenEpicBundle()
        {
            GameController.Instance.audioManager.PlayButtonSound();
            UIMainManager.Instance.epicBundlePanel.Open();
            TrackingManager.TrackEvent(TrackingEvent.Button_Click,
                TrackingParamter.Level,GameManager.Instance.PlayerProfile.LevelProfile.CurrentLevel.ToString(),
                TrackingParamter.Type,"epic_bundle_btn");
        }
        
        public void SetInforPlayBtn(LevelButton lvBtn) 
        {
              Level.text = "LEVEL " + lvBtn.level.ToString();
            if (lvBtn.starCount >= 3) 
            {
                State.text = "COMPLETE";
                Tag.SetActive(true);
            }
            else 
            {
                State.text = lvBtn.isHardMode ? "HARD" : "";
                Tag.SetActive(lvBtn.isHardMode);
            }
           
            SetColor(lvBtn.isCurrentLevel(),lvBtn.isHardMode, lvBtn.starCount >= 3);
        }
        public void PlayAnimBtn()
        {
            _animSeq?.Kill();

            _animSeq = DOTween.Sequence();

            // ===== Phase 1: Overshoot =====
            _animSeq.Append(
                DG.Tweening.DOTweenModuleUI.DOAnchorPosY(
                    PlayUp.rectTransform,
                    60f,
                    0.3f,
                    false
                ).SetEase(Ease.OutSine)
            );

            _animSeq.Join(
                DG.Tweening.DOTweenModuleUI.DOAnchorPosY(
                    Shadow.rectTransform,
                    -20f,
                    0.3f,
                    false
                ).SetEase(Ease.OutSine)
            );

            

            // ===== Phase 3: Về vị trí gốc =====
            _animSeq.Append(
                DG.Tweening.DOTweenModuleUI.DOAnchorPosY(
                    PlayUp.rectTransform,
                    _upStartPos.y-10,
                    0.25f,
                    false
                ).SetEase(Ease.OutQuad)
            );

            _animSeq.Join(
                DG.Tweening.DOTweenModuleUI.DOAnchorPosY(
                    Shadow.rectTransform,
                    _shadowStartPos.y-5,
                    -0.25f,
                    false
                ).SetEase(Ease.OutQuad)
            );



            // ===== Phase 3: Về vị trí gốc =====
            _animSeq.Append(
                DG.Tweening.DOTweenModuleUI.DOAnchorPosY(
                    PlayUp.rectTransform,
                    _upStartPos.y,
                    0.15f,
                    false
                ).SetEase(Ease.OutQuad)
            );

            _animSeq.Join(
                DG.Tweening.DOTweenModuleUI.DOAnchorPosY(
                    Shadow.rectTransform,
                    _shadowStartPos.y,
                    -0.15f,
                    false
                ).SetEase(Ease.OutQuad)
            );
            // ===== Delay giữa mỗi loop =====
            _animSeq.AppendInterval(1.5f);

            // ===== Loop vô hạn =====
            _animSeq.SetLoops(-1,LoopType.Restart);

        }
        public void ResetAnim()
        {
            // Kill sequence nếu đang tồn tại
            if (_animSeq != null)
            {
                _animSeq.Kill();
                _animSeq = null;
            }

            // Reset vị trí về ban đầu
            if (PlayUp != null)
            {
                PlayUp.rectTransform.anchoredPosition = _upStartPos;
            }

            if (Shadow != null)
            {
                Shadow.rectTransform.anchoredPosition = _shadowStartPos;
            }
        }
        public void SetColor(bool isGreen, bool isHard, bool isFullStar) 
        {
            if (isFullStar) 
            {
                PlayUp.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Gray_Up];
                PlayDown.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Gray_Down];
                Level.fontMaterial = GameManager.Instance.spriteCollection.fontDict[FontColor.Gray];
            }
            else 
            {
                if (isHard) 
                {
                    PlayUp.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Red_Up];
                    PlayDown.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Red_Down];
                    Level.fontMaterial = GameManager.Instance.spriteCollection.fontDict[FontColor.Red];
                }
                else 
                {
                    if (isGreen) 
                    {
                        PlayUp.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Green_Up];
                        PlayDown.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Green_Down];
                        Level.fontMaterial = GameManager.Instance.spriteCollection.fontDict[FontColor.Green];
                    }
                    else 
                    {
                        PlayUp.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Orange_Up];
                        PlayDown.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Orange_Down];
                        Level.fontMaterial = GameManager.Instance.spriteCollection.fontDict[FontColor.Orange];
                    }
                }
            }    
        }

        public void ChangeInfiniteHeartIconVisibility(bool status)
        {
            InfiniteHeartIcon.gameObject.SetActive(status);
            Heart.color = status ? Color.clear : Color.white;
        }

        public void ChangePiggyButtonVisibility(bool status)
        {
            breakBtn.gameObject.SetActive(status);
        }
        
        #region Battlepass UI Logic
        
        public void UpdateFillAmount(float fillAmount)
        {
            if (!Mathf.Approximately(battlepassTimerFillImage.fillAmount, fillAmount))
            {
                battlepassTimerFillImage.fillAmount = fillAmount;
            }
        }
        
        public void UpdateBattlepassTimerText(string text)
        {
            battlepassTimerText.SetText(text);
            if (LockIcon != null && LockIcon.activeSelf) LockIcon.SetActive(false);
        }

        public void CheckForBattlepassNotificationVisibility()
        {
            //GameManager.Instance.PlayerProfile.BattlepassProfile.
            battlepassNotificationIcon.SetActive(GameController.Instance.battlepassManager.IsThereUnclaimedReward());
        }
        public void LockBattlePass()
        {
            battlepassTimerText.SetText($"LV{GameController.Instance.battlepassManager.LevelUnlock}");
            if (LockIcon != null && !LockIcon.activeSelf) LockIcon.SetActive(true);
        }
        void BattlepassExpAnimation()
        {
            StopBattlepassExpAnimation();
            PrepareForBattlepassExpAnimation();
            
            battlepassGainAnimation = DOTween.Sequence();

            battlepassGainAnimation
                .Append(battlepassSunIcon.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack))
                .AppendInterval(0.5f)
                .Append(battlepassSunIcon.transform.DOPath(animationPath, 0.5f, PathType.CatmullRom))
                .Join(battlepassSunIcon.transform.DOScale(Vector3.zero, 0.25f).SetDelay(0.25f).SetEase(Ease.Linear))
                .AppendCallback(delegate
                {
                    PlayBattlepassReceiveAnimation();
                    AudioManager.Instance.PlaySFX(SFXID.Star_3);
                    expReceiveEffect.Play();
                    CheckForBattlepassNotificationVisibility();
                });

            battlepassGainAnimation.OnComplete(delegate
            {
                battlepassGainAnimation = null;
            });
        }

        void PrepareForBattlepassExpAnimation()
        {
            battlepassSunIcon.transform.localScale = Vector3.zero;
            battlepassSunIcon.transform.position = animationPath_startPoint.transform.position;
            battlepassSunGainAmountText.SetText($"+{gainAmount}");
            animationPath[0] = animationPath_startPoint.transform.position;
            animationPath[1] = animationPath_middlePoint.transform.position;
            animationPath[2] = animationPath_endPoint.transform.position;
        }

        void StopBattlepassExpAnimation()
        {
            if (battlepassGainAnimation != null)
            {
                battlepassGainAnimation.Complete();
                battlepassGainAnimation = null;
            }
        }

        void PlayBattlepassReceiveAnimation()
        {
            battlepassIconSkeletonGraphic.AnimationState.SetAnimation(0, "BattlePass_Icon_CollectPoint", false);
            battlepassIconSkeletonGraphic.AnimationState.AddAnimation(0, "BattlePass_Icon_Idle", true, 0);
        }

        public void ChangeBattlepassGainAmount(int num)
        {
            gainAmount = num;
        }

        public void ChangeAllowBattlepassAnimationOnStartupStatus(bool status)
        {
            showBattlepassGainAnimationOnStartup = status;
        }
        
        #endregion
        
        #region Epic Bundle UI Logic

        public void UpdateEpicBundleTimerText(string text)
        {
            epicBundleTimerText.SetText(text);
        }
        
        #endregion
    }
}
