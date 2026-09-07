using DG.Tweening;
using JetBrains.Annotations;
using NabaGame.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrickEscape
{
    public class WinRacePanel : BaseUI
    {
        public Image Avatar;
        public Image Frame;
        public TextMeshProUGUI gold;
        public TextMeshProUGUI WinnerCount;
        public List<Race_Avatar> avatars;
        public int goldResult;
        public int currentWinnerResult;
        public Animator animator;
        public Button closeBtn;
        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            animator.Play("WinRace");
        }
        public void SetInfor() 
        {
            closeBtn.onClick.AddListener(Close);
        }

        public void Close() 
        {
            AudioManager.Instance.PlayButtonSound();
            Hide();  
        }
        [Button]
        public void Test() 
        {
        
        }
        [Button]
        public void SetUp() 
        {
            goldResult = (int)(10000/(GameManager.Instance.PlayerProfile.currRemainPlayer+1));
            Debug.Log(goldResult);
            //gold.text = "0";
            currentWinnerResult = GameManager.Instance.PlayerProfile.currRemainPlayer;
            GameManager.Instance.PlayerProfile.ChangeCoin(goldResult);
            FrameID rdFrame = GameManager.Instance.PlayerProfile.currentFrame;
            AvatarID rdAva = GameManager.Instance.PlayerProfile.currentAvatar;
            Frame.sprite = GameManager.Instance.spriteCollection.GetFrameSprite(rdFrame);
            Avatar.sprite = GameManager.Instance.spriteCollection.GetAvatarSprite(rdAva);
            for (int i = 0; i < avatars.Count; i++)
            {
                avatars[i].gameObject.SetActive(false);
            }
            WinnerCount.text = $"You are sharing the reward with {GameManager.Instance.PlayerProfile.currRemainPlayer} other winners!";
          
           

        }

        [SerializeField] private float _duration = 1.5f;

        private int _currentGold;
        [Button]
        public void PlayGoldAnim()
        {
            // reset về 0
            _currentGold = 0;
            gold.text = "0";
            Debug.Log("Play");
            // kill tween cũ nếu có
            DOTween.Kill(gold);

            // tween số
            DOTween.To(() => _currentGold, x =>
            {
                _currentGold = x;
                gold.text = _currentGold.ToString();
            }, goldResult, _duration)
            .SetEase(Ease.OutCubic)
            .SetId(gold);

            // punch scale cho juicy
            gold.transform
                .DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1f);
        }


        [SerializeField] private float _avatarDelay = 0.08f;
        [Button]
        public void ActiveWinPlayer() 
        {
            Sequence seq = DOTween.Sequence();
            for (int i = 0; i < currentWinnerResult; i++)
            {
                int index = i;
                if(i >= avatars.Count) continue;
                avatars[index].gameObject.SetActive(true);
                avatars[index].SetInfor();
                avatars[index].rect.localScale = Vector3.zero;
                seq.Insert(i * _avatarDelay,
                    avatars[index].rect.DOScale(1f, 0.25f)
                        .SetEase(Ease.OutBack)
                );
            }

        }



    }
}
