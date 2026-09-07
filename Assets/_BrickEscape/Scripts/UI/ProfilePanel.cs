using DG.Tweening;
using NabaGame.UI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace BrickEscape
{
  
    public class ProfilePanel : BaseUI
    {
        public Image Avatar;
        public Image Frame;
        public AvatarID ChoosenAvatarID;
        public FrameID ChoosenFrameID;

        public Button ChangeName;
        public Frame[] Frames;
        public Avatar[] Avatars;
        public Button acceptBtn;

        public AnimButton AcceptBtnAnim;
        public bool CanAccept;
        public Button AvatarBtn;
        public Button FrameBtn;
        public Button RenameBtn;

        public GameObject AvatarContent;
        public GameObject FrameContent;
        public RectTransform Tab;
        private Tween MoveTabTween;
        [SerializeField] private bool isOn= false;
        [SerializeField] private float posOnX;
        [SerializeField] private float posOffX;
        [SerializeField] private float tweenDuration;

        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _nameText;

        private const string PLAYER_NAME_KEY = "NAME";
        private const int MAX_NAME_LENGTH = 16;

        private void Awake()
        {
            _inputField.onSubmit.AddListener(OnSubmitName);
            LoadName();
        }
        private void OnDestroy()
        {
            _inputField.onSubmit.RemoveListener(OnSubmitName);
        }
        public void OnToggleChange(bool isOn)
        {
            this.isOn = isOn;

            float targetX = isOn ? posOnX : posOffX;
            AvatarContent.SetActive(isOn);
            FrameContent.SetActive(!isOn);
            // Kill tween cũ nếu còn sống
            if (MoveTabTween != null && MoveTabTween.IsActive())
            {
                MoveTabTween.Kill();
            }
          
            MoveTabTween = DG.Tweening.DOTweenModuleUI.DOAnchorPosX(
                    Tab,
                    targetX,
                    tweenDuration,
                    false
                )
                .SetEase(Ease.OutQuad)
                .SetUpdate(true).OnComplete(() =>
                {
                     

                }); 


        }
        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            foreach (var Ava in Avatars)
            {
                Ava.SetInfor();
            }
            foreach (var Fr in Frames)
            {
                Fr.SetInfor();
            }
            warning.alpha = 0f;
            Frame.sprite = GameManager.Instance.spriteCollection.frameDic[GameManager.Instance.PlayerProfile.currentFrame];
            Avatar.sprite = GameManager.Instance.spriteCollection.avatarDic[GameManager.Instance.PlayerProfile.currentAvatar];
        }
        public void SetInfor()
        {
            AvatarBtn.onClick.AddListener(SelectAvatarTab);
            FrameBtn.onClick.AddListener(SelectFrameTab);
            acceptBtn.onClick.AddListener(AcceptBtn);
            SelectAvatarTab();
            RenameBtn.onClick.AddListener(OnClickRenameButton);
            foreach(var Ava in Avatars) 
            {
                Ava.SetInfor();
            }
            foreach (var Fr in Frames)
            {
                Fr.SetInfor();
            }
        }
        public void SelectAvatarTab() 
        {
            if(isOn)return;
            OnToggleChange(true);
            SetAcceptBtn(CurrentAvatar(ChoosenAvatarID).isUnlocked, ChoosenAvatarID == GameManager.Instance.PlayerProfile.currentAvatar);
        }
        public Frame CurrentFrame(FrameID frame) 
        {
            return System.Array.Find(Frames, f => f.frameID == frame);
        }
        public Avatar CurrentAvatar(AvatarID avatar)
        {
            return System.Array.Find(Avatars, a => a.avatarID == avatar);
        }
        public void SelectFrameTab()
        {
            if (!isOn) return;
            OnToggleChange(false);
            SetAcceptBtn(CurrentFrame(ChoosenFrameID).isUnlocked, ChoosenFrameID == GameManager.Instance.PlayerProfile.currentFrame);
        }
        public void SetFrame(FrameID id) 
        {
            for(int i = 0 ; i < Frames.Length; i ++) 
            {
               if(Frames[i].frameID != id) 
               {

                    Frames[i].OnUnselect();
               }
            }
            ChoosenFrameID = id;
            Frame.sprite = GameManager.Instance.spriteCollection.frameDic[id];
        }
        public void SetAvatar(AvatarID id)
        {
            for (int i = 0; i < Frames.Length; i++)
            {
                if (Avatars[i].avatarID != id)
                {
                    Avatars[i].OnUnselect();
                }
            }
            ChoosenAvatarID = id;
            Avatar.sprite = GameManager.Instance.spriteCollection.avatarDic[id];
        }
        public void SetActive(AvatarID aID, FrameID fID) 
        {
            for (int i = 0; i < Frames.Length; i++)
            {
                if (Avatars[i].avatarID == aID)
                {
                    Avatars[i].Tick.SetActive(true);
                }
                else
                {
                    Avatars[i].Tick.SetActive(false);
                }
            }
            for (int i = 0; i < Frames.Length; i++)
            {
                if (Frames[i].frameID == fID)
                {
                    Frames[i].Tick.SetActive(true);

                }
                else
                {
                    Frames[i].Tick.SetActive(false);
                }
            }
        }
        public void SetAcceptBtn(bool isUnlock,bool isEquiped) 
        {
            if (isUnlock) 
            {
                if (isEquiped)
                {
                    AcceptBtnAnim.SetColor(ButtonColor.Orange_Up,ButtonColor.Orange_Down,FontColor.Orange);
                    AcceptBtnAnim.SetText("EQUIPED");
                    AcceptBtnAnim.SetTextColor(Color.white);
                    AcceptBtnAnim.ResetAnim();
                }
                else
                {
                    AcceptBtnAnim.SetColor(ButtonColor.Green_Up, ButtonColor.Green_Down, FontColor.Green);
                    AcceptBtnAnim.SetText("OK");
                    AcceptBtnAnim.SetTextColor(Color.white);
                    AcceptBtnAnim.Play();
                }
            }
            else 
            {
                AcceptBtnAnim.SetColor(ButtonColor.Gray_Up, ButtonColor.Gray_Down, FontColor.Gray);
                AcceptBtnAnim.SetText("LOCKED");
                AcceptBtnAnim.SetTextColor(ButtonColor.Gray_Down);
                AcceptBtnAnim.ResetAnim();
            }
        }
        public void AcceptBtn() 
        {
            GameController.Instance.audioManager.PlayButtonSound();
            if (!CanAccept) return;
            SetAcceptBtn(true,true);
            Frame.sprite = GameManager.Instance.spriteCollection.frameDic[ChoosenFrameID];
            Avatar.sprite = GameManager.Instance.spriteCollection.avatarDic[ChoosenAvatarID];
            GameManager.Instance.PlayerProfile.currentFrame = ChoosenFrameID;
            GameManager.Instance.PlayerProfile.currentAvatar = ChoosenAvatarID;
            UIMainManager.Instance.homePanel.Frame.sprite = GameManager.Instance.spriteCollection.frameDic[ChoosenFrameID];
            UIMainManager.Instance.homePanel.Avatar.sprite = GameManager.Instance.spriteCollection.avatarDic[ChoosenAvatarID];
            SetActive(ChoosenAvatarID, ChoosenFrameID);
        }
        private void OnSubmitName(string value)
        {
            string newName = value.Trim();

            if (!IsValidName(newName))
                return;

            SaveName(newName);
            ApplyName(newName);

            // Tắt bàn phím mobile + bỏ focus input
            DOVirtual.DelayedCall(0.2f, () =>
            {
                _inputField.DeactivateInputField();
                EventSystem.current?.SetSelectedGameObject(null);
            });
            
        }
        private bool IsValidName(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            if (name.Length > MAX_NAME_LENGTH) return false;
            return true;
        }
        private void SaveName(string name)
        {
            PlayerPrefs.SetString(PLAYER_NAME_KEY, name);
            PlayerPrefs.Save();
        }
        private void LoadName()
        {
            string name = PlayerPrefs.GetString(PLAYER_NAME_KEY, "Player");
            ApplyName(name);
            _inputField.text = name;
        }
        private void ApplyName(string name)
        {
            _nameText.text = name;
        }
        public void OnClickRenameButton()
        {
            // Hiện panel (nếu có)
            GameController.Instance.audioManager.PlayButtonSound();
            //// Clear focus cũ (tránh bug mobile)
            EventSystem.current.SetSelectedGameObject(null);
            // Focus input
            _inputField.Select();
            _inputField.ActivateInputField();

        }

        public CanvasGroup warning;
        public RectTransform warningGroup;
        public TextMeshProUGUI WarningText;
        private Sequence _warningSeq;

        public void TweenShowWarning()
        {
            // Kill tween cũ (tránh overlap)
            _warningSeq?.Kill();

            warningGroup.gameObject.SetActive(true);

            // Reset trạng thái ban đầu
            warning.alpha = 0f;
            warningGroup.anchoredPosition =
                new Vector2(warningGroup.anchoredPosition.x, -400f);

            // Tạo sequence
            _warningSeq = DOTween.Sequence();

            _warningSeq
                .Join(DG.Tweening.DOTweenModuleUI.DOFade(warning, 1f, 0.75f))
                .Join(DG.Tweening.DOTweenModuleUI.DOAnchorPosY(warningGroup, 0f, 0.75f))
                .SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    DOVirtual.DelayedCall(0.25f, () =>
                    {
                        warningGroup.gameObject.SetActive(false);
                    });
                });

        }
        public void ShowWarning(FrameID id)
        {
            RawFrame rawdata = GameManager.Instance.rawFrameData.rawFrames
                .Find(b => b.FrameID == id);

            if (rawdata.UnlockType == UnlockType.Sparkling_Summer_Reward)
            {
                WarningText.text = $"Unlocks in BattlePass";
            }
            else if (rawdata.UnlockType == UnlockType.Level)
            {
                int lv = rawdata.LevelComplete;
                WarningText.text = $"Unlocks at Level {lv}";
            }

            TweenShowWarning();
        }
        public void ShowWarning(AvatarID id)
        {
            RawAvatar rawdata = GameManager.Instance.rawAvatarData.rawAvatars
                .Find(b => b.AvatarID == id);
        
            if (rawdata.UnlockType == UnlockType.Sparkling_Summer_Reward)
            {
                WarningText.text = $"Unlocks in BattlePass";
            }
            else if(rawdata.UnlockType == UnlockType.Level) 
            {
                int lv = rawdata.LevelComplete;
                WarningText.text = $"Unlocks at Level {lv}";
            }


            TweenShowWarning();
        }
    }
}
