using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class LevelButton : ScrollItem
    {
        public Image[] star = new Image[3];
        public Image[] starEmpty = new Image[3];
        public Image up;
        public Image down;
        public Image shadow;
        public Image Border;
        public TextMeshProUGUI textLevel;
        public int level;
        public RectTransform trans;
        public int starCount;
        public bool isHardMode = false;
        public Image Fillbar;
        private Tween _selectTween;
        private Tween _unselectTween;
        public ParticleSystem effect;
        private Vector2 _upStartPos;
        private Vector2 _shadowStartPos;

        private Sequence _animSeq;
        LevelSaveData data;
        public GameObject fakeProcess;
        public StarResultAnimation starAnim;
        private void Awake()
        {
            _upStartPos = up.rectTransform.anchoredPosition;
            _shadowStartPos = shadow.rectTransform.anchoredPosition;
        }
        private void OnValidate()
        {
            starAnim = GetComponent<StarResultAnimation>();
        }
        public override void SetInfo(int index)
        {
            base.SetInfo(index);
            LevelSaveData data = GameManager.Instance.PlayerProfile.LevelProfile.GetLevelData(index + 1);
            SetInfor(data);
            if (fakeProcess == null) return;
            fakeProcess.SetActive(index != 0);
        }
        public void PlayAnimNextLevel() 
        {
            Fillbar.fillAmount = 0f;
            starAnim.SetAction(FillAnim);
            starAnim.Play();    
        }


        public void FillAnim() 
        {
            Fillbar.DOFillAmount(1f, 0.2f)
                .SetEase(Ease.OutCubic);
        }


        public void SetInfor(LevelSaveData fake)
        {
            level = fake.levelID;
            starCount = fake.starGained;
            textLevel.text = level.ToString();
            if (fake.isFinished) 
            {
                for (int i = 0; i < star.Length; i++)
                {
                    star[i].gameObject.SetActive(true);
                    if (i <= starCount - 1)
                    {
                        star[i].gameObject.SetActive(true);
                    }
                    else 
                    {
                        star[i].gameObject.SetActive(false);
                    }
                        
                }
                for (int i = 0; i < starEmpty.Length; i++)
                {
                    starEmpty[i].gameObject.SetActive(true);
                }
                if (starCount <3) 
                {
                    SetOrange();
                }
                else 
                {
                    SetGreen();
                    Border.sprite = GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Button_Down_Silver];
                }
            }
            else 
            {
                for (int i = 0; i < starEmpty.Length; i++)
                {
                    starEmpty[i].gameObject.SetActive(false);
                }
                if (level == GameManager.Instance.PlayerProfile.LevelProfile.currentLevel) 
                {
                    SetGreen();
                    Border.sprite = GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Button_Down_Gold];
                }
                else 
                {
                    SetGray();
        
                }
            }
            LevelInfo levelInfo = GameManager.Instance.levelData.LevelInfos.Find(x => x.LevelID == fake.levelID);
            isHardMode = levelInfo != null  ? levelInfo.IsHard : false;

            Fillbar.gameObject.SetActive(fake.isFinished);


        }
       
        public void SetUpTween()
       {
            _selectTween = trans
                .DOScale(Vector3.one * 1.25f, 0.25f)
                .SetEase(Ease.OutQuad)
                .SetAutoKill(false)
                .Pause();

            _unselectTween = trans
                .DOScale(Vector3.one, 0.25f)
                .SetEase(Ease.OutCubic)
                .SetAutoKill(false)
                .Pause();
        }
        public void PlayAnimBtn()
        {
            _animSeq?.Kill();

            _animSeq = DOTween.Sequence();

            _animSeq.Append(
                up.rectTransform
                    .DOAnchorPosY(55f, 0.18f)      // overshoot
                    .SetEase(Ease.OutQuad)
            );

            _animSeq.Join(
                shadow.rectTransform
                    .DOAnchorPosY(-32f, 0.18f)     // overshoot
                    .SetEase(Ease.OutQuad)
            );

            // giật ngược nhẹ
            _animSeq.Append(
                up.rectTransform
                    .DOAnchorPosY(48f, 0.08f)
                    .SetEase(Ease.InOutQuad)
            );

            _animSeq.Join(
                shadow.rectTransform
                    .DOAnchorPosY(-26f, 0.08f)
                    .SetEase(Ease.InOutQuad)
            );

            // về vị trí gốc
            _animSeq.Append(
                up.rectTransform
                    .DOAnchorPosY(_upStartPos.y, 0.16f)
                    .SetEase(Ease.OutQuad)
            );

            _animSeq.Join(
                shadow.rectTransform
                    .DOAnchorPosY(_shadowStartPos.y, 0.16f)
                    .SetEase(Ease.OutQuad)
            );
        }
        public bool isCurrentLevel() 
        {
            return level == GameManager.Instance.PlayerProfile.LevelProfile.currentLevel;
        }

        //public bool CanPlay() 
        //{
        //    return starCount < 3;
        //}
        public void SetGreen() 
        {
            up.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Green_Up];
            down.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Green_Down];
            textLevel.fontMaterial = GameManager.Instance.spriteCollection.fontDict[FontColor.Green];

        }

        public void OnSelect()
        {
            _unselectTween.Pause();
            _selectTween.Restart();
            Border.sprite = GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Button_Down_Gold];
            UIMainManager.Instance.homePanel.SetInforPlayBtn(this);
            PlayAnimBtn();
            effect.Play();
        }

        public void UnSelect()
        {
            _selectTween.Pause();
            _unselectTween.Restart();
            effect.Stop();
            Border.sprite = GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Button_Down_Silver];
        }

        private void OnDestroy()
        {
            _selectTween?.Kill();
            _unselectTween?.Kill();
        }
        public void SetGray() 
        {
            up.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Gray_Up];
            down.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Gray_Down];
            textLevel.fontMaterial = GameManager.Instance.spriteCollection.fontDict[FontColor.Gray];
            Border.sprite = GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Button_Down_Silver];
        }
        public void SetOrange() 
        {
            up.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Orange_Up];
            down.color = GameManager.Instance.spriteCollection.ColorDict[ButtonColor.Orange_Down];
            textLevel.fontMaterial = GameManager.Instance.spriteCollection.fontDict[FontColor.Orange];
            Border.sprite = GameManager.Instance.spriteCollection.spriteDic[SpriteUI.Button_Down_Silver];
        }

    }
}
