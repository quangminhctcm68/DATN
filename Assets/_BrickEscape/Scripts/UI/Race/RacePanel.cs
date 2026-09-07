using DG.Tweening;
using JetBrains.Annotations;
using NabaGame.UI;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

namespace BrickEscape
{
    public class RacePanel : BaseUI
    {
        public List<Race_Avatar> avatars = new List<Race_Avatar>();
        public Race_Avatar playerAvatar;
        public List<Cloud> clouds;

        public float playerDelay = 0.3f;          // delay sau player
        public float minAIDelay = 0.05f;
        public float maxAIDelay = 0.15f;
        public Transform AvatarContainer;
        public Transform CloudContainer;
        public Transform FallingAvatarContainer;
        const int FAILED_BOT_COUNT = 3;

        //public int Remain_Player = 100;
        public TextMeshProUGUI Player_Count;
        public TextMeshProUGUI Level_Count;
        //int Level_Player = 0;

        public Button InforBtn;
        public Button CloseBtn;
        public PlayAnimRandomLoop anim;
        PlayerProfile profile;
        public List<DOTweenAnimation> animations = new List<DOTweenAnimation>();

        public void OnValidate()
        {
            animations = GetComponentsInChildren<DOTweenAnimation>().ToList();
        }
        public override void OnInAnimationStart()
        {
            base.OnInAnimationStart();
            foreach (var anim in animations)
            {
                if(anim.gameObject.activeSelf)
                    anim.DOPlay();
            }
            for(int i = 0; i < avatars.Count; i++) 
            {
                avatars[i].SetInfor();
            }
          
            playerAvatar.SetInfor();
            anim.isActive = true;
        }

        public override void OnOutAnimationStart()
        {
            base.OnOutAnimationStart();
            foreach (var anim in animations)
            {
                if (anim.gameObject.activeSelf)
                    anim.DOPause();

            }
            if (!GameManager.Instance.PlayerProfile.JoinedRace) 
            {
                SetCloudIndex();
                for (int i = 0; i < clouds.Count; i++)
                {
                    clouds[i].gameObject.SetActive(i >= GameManager.Instance.PlayerProfile.CurrentRaceLv);
                }
            }
            anim.isActive = false;  
        }


        public void SetInfor()
        {
            Player_Count.text = GameManager.Instance.PlayerProfile.currRemainPlayer.ToString();
            Level_Count.text = $"{GameManager.Instance.PlayerProfile.CurrentRaceLv}/7";
            InforBtn.onClick.AddListener(ShowInfor);
            CloseBtn.onClick.AddListener(Close);
            foreach (var anim in animations)
            {
                if (anim.gameObject.activeSelf)
                    anim.DOPause();

            }

            SetCloudIndex();
            for (int i = 0; i< clouds.Count; i++)
            {
                clouds[i].gameObject.SetActive(i >= GameManager.Instance.PlayerProfile.CurrentRaceLv);
            }
            anim.isActive = false;
            profile = GameManager.Instance.PlayerProfile; 

        }

        public void SetCloudIndex() 
        {
          
            for (int i = 0; i <avatars.Count; i++)
            {
                Race_Avatar racer = avatars[i];
                RectTransform target = clouds[GameManager.Instance.PlayerProfile.CurrentRaceLv].destination[i];
 
                // convert world → local UI
                RectTransform parent = racer.rect.parent as RectTransform;
                Vector2 localPos;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parent,
                    RectTransformUtility.WorldToScreenPoint(null, target.position),
                    null,
                    out localPos
                );

                racer.rect.anchoredPosition = localPos;
                racer.rect.localScale = Vector3.one;
                racer.rect.localRotation = Quaternion.identity;
            }
            // convert world → local UI
            RectTransform playerparent = playerAvatar.rect.parent as RectTransform;
            Vector2 playerlocalPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                playerparent,
                RectTransformUtility.WorldToScreenPoint(null, clouds[GameManager.Instance.PlayerProfile.CurrentRaceLv].PlayerDestiation.position),
                null,
                out playerlocalPos
            );
            playerAvatar.rect.anchoredPosition = playerlocalPos;
            playerAvatar.rect.localScale = Vector3.one;
            playerAvatar.rect.localRotation = Quaternion.identity;
            AvatarContainer.SetParent(clouds[GameManager.Instance.PlayerProfile.CurrentRaceLv].rect);
        }

        public void ResetCloud()
        {
            for (int i = 0; i < clouds.Count; i++)
            {
                clouds[i].gameObject.SetActive(true);
                clouds[i].Show();
            }
            SetCloudIndex();
            Player_Count.text = GameManager.Instance.PlayerProfile.currRemainPlayer.ToString();
            Level_Count.text = $"{GameManager.Instance.PlayerProfile.CurrentRaceLv}/7";
        }




       










        public void ShowInfor() 
        {
            AudioManager.Instance.PlayButtonSound();
            UIMainManager.Instance.raceInforPanel.Show();
        }
        public void Close() 
        {
            AudioManager.Instance.PlayButtonSound();
            UIMainManager.Instance.homePanel.CheckMoonRaceNotify();

            Hide();
        }   
        public void EliminatePlayer()
        {
            int rdResult = Random.Range(13, 15);

            int startValue = GameManager.Instance.PlayerProfile.currRemainPlayer;
            int endValue = GameManager.Instance.PlayerProfile.currRemainPlayer - rdResult;

            GameManager.Instance.PlayerProfile.currRemainPlayer = endValue;
            Level_Count.text = $"{GameManager.Instance.PlayerProfile.CurrentRaceLv}/7";

            DOTween.Kill(Player_Count);

            Sequence seq = DOTween.Sequence();

            // =========================
            // 1. POP SCALE
            // =========================
            seq.Append(
                Player_Count.transform.DOScale(1.3f, 0.15f)
                    .SetEase(Ease.OutBack)
            );

            // =========================
            // 2. COUNT ANIMATION
            // =========================
            seq.Append(
                DOTween.To(
                    () => startValue,
                    x =>
                    {
                        startValue = x;
                        Player_Count.text = x.ToString();
                    },
                    endValue,
                    0.5f
                ).SetEase(Ease.OutCubic)
            );

            // =========================
            // 3. SCALE BACK
            // =========================
            seq.Join(
                Player_Count.transform.DOScale(1f, 0.3f)
                    .SetEase(Ease.OutBack)
            );

            // =========================
            // 4. PUNCH nhẹ cho đã mắt
            // =========================
            seq.Append(
                Player_Count.transform.DOPunchScale(
                    Vector3.one * 0.2f,
                    0.3f,
                    5,
                    0.5f
                )
            );
        }




        [Button]
        public void PlayCeleb()
        {
            foreach (var avatar in avatars)
                avatar.PlayCelebrate();

            playerAvatar.PlayCelebrate();
            UIMainManager.Instance.winRacePanel.SetUp();
            DOVirtual.DelayedCall(1.5f, () =>
                {
 
                    UIMainManager.Instance.winRacePanel.Show();
                    GameManager.Instance.PlayerProfile.ResetMoonRace();
                    UIMainManager.Instance.homePanel.StartCooldown();
                }
            );
        }

        [Button]
        public void JumpToCloud(int index)
        {
            if (clouds == null || clouds.Count == 0) return;

            AvatarContainer.SetParent(CloudContainer);
            AvatarContainer.SetAsLastSibling();

            if (index > 0)
                SpawnAndAnimateFailedBots(clouds[index - 1]);

            AnimateJumpToCloud(clouds[index]);
        }

        // ─── Private Helpers ──────────────────────────────────────────────

        private void SpawnAndAnimateFailedBots(Cloud prevCloud)
        {
            if (prevCloud.destination == null || prevCloud.destination.Count < FAILED_BOT_COUNT)
                return;

            var destinations = new List<RectTransform>(prevCloud.destination);
            var spawnedBots = SpawnFailBots(destinations);

            BuildFailSequence(prevCloud, spawnedBots);
        }

        private List<Race_Avatar> SpawnFailBots(List<RectTransform> destinations)
        {
            var spawnedBots = new List<Race_Avatar>(FAILED_BOT_COUNT);
            int destCount = destinations.Count;

            for (int i = 0; i < FAILED_BOT_COUNT; i++)
            {
                // Fisher-Yates partial shuffle để chọn dest không trùng
                int rand = Random.Range(i, destCount);
                (destinations[i], destinations[rand]) = (destinations[rand], destinations[i]);

                Race_Avatar bot = GameManager.Instance.pooling.SpawnAvatar(FallingAvatarContainer);
                bot.SetInfor();
                bot.rect.anchoredPosition = WorldToLocalUI(destinations[i], bot.rect.parent as RectTransform);
                bot.rect.localScale = Vector3.one;
                bot.rect.localRotation = Quaternion.identity;
                bot.rect.SetAsFirstSibling();

                spawnedBots.Add(bot);
            }

            return spawnedBots;
        }

        private Vector2 WorldToLocalUI(RectTransform worldTarget, RectTransform localParent)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldTarget.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(localParent, screenPoint, null, out Vector2 localPos);
            return localPos;
        }

        private void BuildFailSequence(Cloud prevCloud, List<Race_Avatar> bots)
        {
            Sequence seq = DOTween.Sequence();

            seq.AppendInterval(1f);
            seq.AppendCallback(() => prevCloud.Disappear());
            seq.AppendInterval(0.5f);

            EliminatePlayer();

            foreach (var bot in bots)
            {
                var botRef = bot; // closure capture
                seq.AppendCallback(() => botRef.FallAnim());
                seq.AppendInterval(0.1f);
            }
        }

        private void AnimateJumpToCloud(Cloud cloud)
        {
            Sequence seq = DOTween.Sequence();
            float timeCursor = 0f;

            // Tạo mapping avatar → destination (shuffle cả hai)
            var botsShuffled = ShuffledBotsExcludingPlayer();
            var destsShuffled = new List<RectTransform>(cloud.destination);
            Shuffle(destsShuffled);

            int count = Mathf.Min(botsShuffled.Count, destsShuffled.Count);
            var map = BuildAvatarDestMap(botsShuffled, destsShuffled, count);

            SortBotsBySiblingOrder(map);

            // Player nhảy trước
            seq.Insert(timeCursor, playerAvatar.JumpToPos(cloud.PlayerDestiation));
            timeCursor += playerDelay;

            // AI nhảy lần lượt với random delay
            for (int i = 0; i < count; i++)
            {
                seq.Insert(timeCursor, botsShuffled[i].JumpToPos(destsShuffled[i]));
                timeCursor += Random.Range(minAIDelay, maxAIDelay);
            }

            DOVirtual.DelayedCall(2f, () =>
            {
                AvatarContainer.SetParent(cloud.rect);

                if (GameManager.Instance.PlayerProfile.CurrentRaceLv >= 7)
                    PlayCeleb();
            });
        }

        private List<Race_Avatar> ShuffledBotsExcludingPlayer()
        {
            var list = new List<Race_Avatar>(avatars);
            list.Remove(playerAvatar);
            Shuffle(list);
            return list;
        }

        private Dictionary<Race_Avatar, RectTransform> BuildAvatarDestMap(
            List<Race_Avatar> bots, List<RectTransform> dests, int count)
        {
            var map = new Dictionary<Race_Avatar, RectTransform>(count);
            for (int i = 0; i < count; i++)
                map[bots[i]] = dests[i];
            return map;
        }

        private void SortBotsBySiblingOrder(Dictionary<Race_Avatar, RectTransform> map)
        {
            var sorted = map
                .OrderByDescending(kv => kv.Value.anchoredPosition.y)
                .Select(kv => kv.Key)
                .ToList();

            for (int i = 0; i < sorted.Count; i++)
                sorted[i].rect.SetSiblingIndex(i);
        }

        void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int rand = Random.Range(i, list.Count);
                (list[i], list[rand]) = (list[rand], list[i]);
            }
        }


    }
}
