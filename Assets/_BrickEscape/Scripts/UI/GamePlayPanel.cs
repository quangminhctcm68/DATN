using BMH.Ads;
using DG.Tweening;
using EasyTransition;
using NabaGame.Core.Runtime.EventManager;
using NabaGame.UI;
using Sirenix.OdinInspector;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class GamePlayPanel : BaseUI
    {
        public Image[] star = new Image[3];
        public TextMeshProUGUI state;
        public Button pauseBtn;
        public Button Retry;
        public GameObject Des;
        public GameObject circle;
        public GameObject HardTag;
        public int _prevStar = -1;
        public Animator _animator;
        public bool isHardLevel;
        public TextMeshProUGUI Level_text;
        public Button ChangeTheme;
        public Button NoAds;
        [SerializeField] Toggle btnSettingTheme;
        public RectTransform BoosterPos;
        public void OnEnable()
        {
            EventManager.Instance.AddListener<ThemeChange>(OnChangeTheme);
        }
        public void OnDisable()
        {
            EventManager.Instance.RemoveListener<ThemeChange>(OnChangeTheme);
        }


        public override void OnInAnimationFinish()
        {
            base.OnInAnimationFinish();
            _animator.gameObject.SetActive(false);
            if (isHardLevel) 
            {
                PlayHardAnim();
            }
            btnSettingTheme.SetToggle(GameManager.Instance.PlayerProfile.isDarkTheme);
        }

        public void Open()
        {

            _animator.gameObject.SetActive(false);
            if (isHardLevel)
            {
                PlayHardAnim();
            }
            btnSettingTheme.SetToggle(GameManager.Instance.PlayerProfile.isDarkTheme);
        }

        public void SetText(int ID) 
        {
            Level_text.text = $"LEVEL {ID}";
           CheckNoAds();
        }

        public void CheckNoAds() 
        {
            if (GameManager.Instance.PlayerProfile.isNoAds == 1)
            {
                AdManager.Instance.Hide(AdsType.Banner);
                BoosterPos.anchoredPosition = new Vector2(0, 360);
                UIMainManager.Instance.moreBoosterPanel.BoosterPos.anchoredPosition = new Vector2(0, 360);
                Debug.Log("No Ads - Hide Banner");
                NoAds.gameObject.SetActive(false);   
            }
            else
            {
                AdManager.Instance.Show(AdsType.Banner);
                BoosterPos.anchoredPosition = new Vector2(0, 480);
                UIMainManager.Instance.moreBoosterPanel.BoosterPos.anchoredPosition = new Vector2(0, 480);
                NoAds.gameObject.SetActive(true);
                Debug.Log("Show Banner");
            }
        }
        public void SetInfor() 
        {
            pauseBtn.onClick.AddListener(PauseBtn);
            Retry.onClick.AddListener(RetryBtn);
            ChangeTheme.onClick.AddListener(ChangeThemeBtn);
            isHardLevel = false;
            _animator.gameObject.SetActive(false);
            NoAds.onClick.AddListener(NoAdBtn); 

        }

        public void NoAdBtn()
        {
            AudioManager.Instance.PlayButtonSound();
            UIMainManager.Instance.NoAdsPanel.Show();
        }
        
        [Button]
        public void PlayHardAnim() 
        {
            _animator.gameObject.SetActive(true);
            _animator.SetBool(AnimatorParameters.IS_PLAY, isHardLevel);
            StartCoroutine(DelayReset());
        }
        public IEnumerator DelayReset() 
        {
            yield return new WaitForSeconds(1f);
            isHardLevel = false;
            _animator.SetBool(AnimatorParameters.IS_PLAY, isHardLevel);
        }
        public void UpdateStar(int currStar)
        {
            currStar = Mathf.Clamp(currStar, 0, star.Length);
            // Init lần đầu
            if (_prevStar < 0)
            {
                for (int i = 0; i < star.Length; i++)
                {
                    ResetStarVisual(star[i]);
                }

                _prevStar = currStar;
                return;
            }

            // ⭐ MẤT SAO (giảm)
            if (currStar < _prevStar)
            {
                for (int i = currStar; i < _prevStar; i++)
                {
                    PlayEmptyEffect(star[i]); // chỉ rơi sao vừa mất
                }
            }
            // ⭐ TĂNG SAO (nếu có hồi máu, thưởng...)
        
            _prevStar = currStar;

            if (_prevStar == 0)
            {
                _prevStar = -1; // reset để lần sau vào level mới lại init lại
            }
        }

        private void ResetStarVisual(Image img)
        {
            RectTransform rect = img.rectTransform;

            DOTween.Kill(rect);
            DOTween.Kill(img);
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchoredPosition = Vector2.zero;
            img.color = Color.white;
            
        }
      
        public void ChangeThemeBtn() 
        {
            AudioManager.Instance.PlayButtonSound();
            GameManager.Instance.PlayerProfile.ChangeThemeSetting();
      
        }

        public void OnChangeTheme(ThemeChange e = null) 
        {
            btnSettingTheme.OnToggleChange(GameManager.Instance.PlayerProfile.isDarkTheme);
        }
    

        private void PlayEmptyEffect(Image img)
        {
            RectTransform rect = img.rectTransform;

            DOTween.Kill(rect);
            DOTween.Kill(img);

            rect.localScale = Vector3.one;
            rect.anchoredPosition = Vector2.zero;
            img.color = new Color(1f, 1f, 1f, 1f);
            rect.localRotation = Quaternion.identity;
            float rotateZ =60f ;

            float moveDown = -300f;

            Sequence seq = DOTween.Sequence();

            // scale phồng
            seq.Append(
                ShortcutExtensions.DOScale(
                    rect,
                    Vector3.one * 1.5f,
                    0.15f
                ).SetEase(Ease.OutBack)
            );

            // rơi xuống
            seq.Join(
                DG.Tweening.DOTweenModuleUI.DOAnchorPosY(
                    rect,
                    moveDown,
                    0.5f
                ).SetEase(Ease.InQuad)
            );

            // scale về 1
            seq.Join(
                ShortcutExtensions.DOScale(
                    rect,
                    Vector3.one,
                    0.5f
                ).SetEase(Ease.OutQuad)
            );
            // 🔥 rotation tạo cảm giác sao rơi
            seq.Join(
                ShortcutExtensions.DORotate(
                    rect,
                    new Vector3(0f, 0f, rotateZ),
                    0.5f,
                    RotateMode.FastBeyond360
                ).SetEase(Ease.OutCubic)
            );
            // fade mờ
            seq.Join(
                DG.Tweening.DOTweenModuleUI.DOFade(
                    img,
                    0f,
                    0.5f
                )
            );
        }
        public void RetryBtn() 
        {
            AudioManager.Instance.PlayButtonSound();
            UIMainManager.Instance.adsPanel.WatchAdsInter();
            DOVirtual.DelayedCall(0.3f, () => {
                GameController.Instance.levelGenerator.PrepareLevelData();
                UIMainManager.Instance.gamePlayPanel.Show();
            });
        }

        public void PauseBtn() 
        {
            AudioManager.Instance.PlayButtonSound();
            //UIMainManager.Instance.adsPanel.WatchAdsInter();
            UIMainManager.Instance.pausePanel.Show();

        }


        public void ShowDes() 
        {
            Des.SetActive(true);
            Des.transform.localScale = Vector3.zero;
            Des.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            circle.SetActive(true);
        }
        public void HideDes()
        {
            circle.SetActive(false);
            Des.transform.localScale = Vector3.one;
            Des.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
            {
                Des.SetActive(false);

            });
        }
    }
}
