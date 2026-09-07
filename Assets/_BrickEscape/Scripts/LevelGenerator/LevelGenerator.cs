using ArrowMaze;
using BMH.Ads;
using DG.Tweening;
using NabaGame.Tracking;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape
{
    public class LevelGenerator : SerializedMonoBehaviour
    {
        public int levelID;
        [SerializeField] private LevelInfo levelData;
        
        [SerializeField] private List<GameDot> spawnedDots = new List<GameDot>();
        [SerializeField] private List<MoveableBlock> spawnedBlocks = new List<MoveableBlock>();
        [SerializeField] private Dictionary<int, MoveableBlock> blockColliders = new Dictionary<int, MoveableBlock>();

        [SerializeField] private int heartCount = 0;
        [SerializeField] private int remainingBlocks = 0;

        [SerializeField] private bool levelIsOver;
        
        List<SpriteRenderer> blockRenderers = new List<SpriteRenderer>();
        List<Vector3> dotsPositions = new List<Vector3>();

        private float distanceBetweenEachDot;
        
        private int columnCount = 0;
        private int rowCount = 0;
        public int CompletedLevelCount = 0;
        private Sequence dotColorAnimation;

        private int realLevelNumber = 0;
        
        #region Start, Update, Validate

        // public void Start()
        // {
        //     StartGeneratingLevel();
        // }

        // public void StartGeneratingLevel()
        // {
        //     levelID = GameManager.Instance.PlayerProfile.CurrentLevel;
        //     PrepareLevelData();
        // }
        
        public void StartGeneratingLevel(int levelID)
        {
            this.levelID = levelID;
            realLevelNumber = levelID;
            //Debug.Log(levelID);
            
            PrepareLevelData();
            DOVirtual.DelayedCall(0.5f, delegate { CheckTutorialProgress(levelID); });

        }
        
        public void StartGeneratingLevel(bool nextLevel)
        {
            if (nextLevel)
            {
                levelID++;
            }
            else
            {
                levelID--;
                levelID = Mathf.Clamp(levelID, 1, levelID);
                
            }
            
            PrepareLevelData();
            DOVirtual.DelayedCall(0.5f, delegate { CheckTutorialProgress(levelID); });
        }
        
        #endregion
        
        #region Level Generating Logic

        public void PrepareLevelData()
        {
            ClearEverything();
            levelIsOver = false;
            levelData = GameManager.Instance.levelData.GetLevelInfo(levelID);
            UIMainManager.Instance.gamePlayPanel.isHardLevel= levelData.IsHard;
            UIMainManager.Instance.gamePlayPanel.HardTag.SetActive(levelData.IsHard);
            GenerateDots();
            TrackingManager.TrackEvent(TrackingEvent.RestartLevel, TrackingParamter.Level, realLevelNumber.ToString());
        }

        void GenerateDots()
        {
            distanceBetweenEachDot = levelData.CellSize;

            columnCount = 0;
            rowCount = 0;

            GameDot tempDotRef;
            
            foreach (var columns in levelData.ColumInfos)
            {
                foreach (var row in columns.Colums)
                {
                    if (row.Values[0] > -1)
                    {
                        tempDotRef = GameManager.Instance.pooling.SpawnDot();
                        tempDotRef.transform.position = new Vector3(columnCount * distanceBetweenEachDot, -rowCount * distanceBetweenEachDot, 0.1f);
                        tempDotRef.ResetColor();
                        
                        spawnedDots.Add(tempDotRef);
                    }
                    rowCount++;
                }
                
                rowCount = 0;
                columnCount++;
            }

            GenerateBlocks();
        }

        void GenerateBlocks()
        {
            distanceBetweenEachDot = levelData.CellSize;

            columnCount = 0;
            rowCount = 0;

            MoveableBlock tempBlockRef;
            
            foreach (var columns in levelData.ColumInfos)
            {
                foreach (var row in columns.Colums)
                {
                    if (row.Values[0] > 0)
                    {
                        tempBlockRef = GameManager.Instance.pooling.SpawnBlock(row.Values[0]);
                        tempBlockRef.transform.position = new Vector3(columnCount * distanceBetweenEachDot, -rowCount * distanceBetweenEachDot, 0f);
                        
                        tempBlockRef.Init();
                        tempBlockRef.SetColor(row.Values[1]);
                        tempBlockRef.SetArrow(GameManager.Instance.dataCollection.GetMoveDirectionByID(row.Values[2]), row.Values[3], row.Values[4]);
                        tempBlockRef.SetupMoveDirection();
                        tempBlockRef.MakeBlockDisappear();
                        
                        spawnedBlocks.Add(tempBlockRef);
                        foreach (var col in tempBlockRef.BlockCollider)
                        {
                            blockColliders[col.GetInstanceID()] = tempBlockRef;
                        }
                        
                        tempBlockRef.MakeBlockAppear(true);
                    }
                    rowCount++;
                }
                
                rowCount = 0;
                columnCount++;
            }
            
            remainingBlocks = spawnedBlocks.Count;
            
            SetupHeart();
        }

        void SetupHeart()
        {
            //heartCount = levelData.IsHard? 5 : 3;
            heartCount = 3;
           
            CameraController.Instance.ChangeCameraControlStatus(true);
            GameController.Instance.ChangePlayerControlState(true);
            UIMainManager.Instance.gamePlayPanel._prevStar =-1 ;
            UIMainManager.Instance.gamePlayPanel.UpdateStar(heartCount);
            SetupCamera();
        }

        void SetupCamera()
        {
            blockRenderers.Clear();
            foreach (var block in spawnedBlocks)
            {
                //blockRenderers.Add(block.BlockRenderer);
                blockRenderers.AddRange(block.BlockRenderers);
            }
            
            // CameraController.Instance.FitCameraToBounds(CameraController.Instance.MainCamera, 
            //     CameraController.Instance.GetAllBlocksBound(blockRenderers));
            
            dotsPositions.Clear();
            foreach (var dot in spawnedDots)
            {
                dotsPositions.Add(dot.transform.position);
            }
            CameraController.Instance.SetupCameraLimitsAndZoom(dotsPositions);
        }

        public void ClearEverything()
        {
            foreach(var dot in spawnedDots)
            {
                GameManager.Instance.pooling.DespawnDot(dot);
            }
            
            foreach(var block in spawnedBlocks)
            {
                //block.ChangeBlockActiveStatus(false);
                if (block.IsBlockActive)
                {
                    block.StopAllAnimations();
                    block.ChangeBlockActiveStatus(false);
                    GameManager.Instance.pooling.DespawnBlock(block, block.BlockID);
                }
            }
            
            spawnedDots.Clear();
            spawnedBlocks.Clear();
            blockColliders.Clear();
        }
        
        #endregion
        
        #region Level Progress

        public void ReportBlockGotOut()
        {
            remainingBlocks--;
            if (IsVictory())
            {
                levelIsOver = true;
                CameraController.Instance.ChangeCameraControlStatus(false);
                GameController.Instance.ChangePlayerControlState(false);
                AdManager.Instance.Hide(AdsType.Banner);
                PlayWinSequence();
                CompletedLevelCount++;
                GameManager.Instance.PlayerProfile.ChangeCoin(10);
                GameManager.Instance.profileData.CheckUnlock(levelID);
                GameManager.Instance.PlayerProfile.LevelProfile.SaveThisLevelData(levelID, true, Mathf.Clamp(heartCount, 0, heartCount));
                if (heartCount >= 3 && GameManager.Instance.PlayerProfile.LevelProfile.currentLevel >= 15)
                {
                    GameController.Instance.battlepassManager.AddXP(levelData.IsHard? 2 : 1);
                    UIMainManager.Instance.homePanel.ChangeBattlepassGainAmount(levelData.IsHard? 2 : 1);
                    UIMainManager.Instance.homePanel.ChangeAllowBattlepassAnimationOnStartupStatus(true);
                }
                TrackingManager.TrackEvent(TrackingEvent.FinishLevel, TrackingParamter.Level, realLevelNumber.ToString());
                //GameManager.Instance.PlayerProfile.ChangeLevel(levelID + 1);
            }
        }
        
        public void ReportWrongMove()
        {
            if (levelIsOver) return;
            heartCount--;
            UIMainManager.Instance.gamePlayPanel.UpdateStar(heartCount);
            if (IsLose())
            {
                levelIsOver = true;
                CameraController.Instance.ChangeCameraControlStatus(false);
                GameController.Instance.ChangePlayerControlState(false);
                AdManager.Instance.Hide(AdsType.Banner);
                DOVirtual.DelayedCall(0.25f, () =>
                {
                    UIMainManager.Instance.outOfStarPanel.Show();
                });
                TrackingManager.TrackEvent(TrackingEvent.Failed_Level, TrackingParamter.Level, realLevelNumber.ToString());
            }
        }

        public void Revive(int heartToAdd)
        {
            if (!levelIsOver) return;
            
            heartCount += heartToAdd;
            UIMainManager.Instance.gamePlayPanel.UpdateStar(heartCount);
            
            levelIsOver = false;
            CameraController.Instance.ChangeCameraControlStatus(true);
            GameController.Instance.ChangePlayerControlState(true);
        }
        
        public void Revive(bool reviveToFull)
        {
            if (!levelIsOver) return;
            
            heartCount = 3;
            UIMainManager.Instance.gamePlayPanel.UpdateStar(heartCount);
            
            levelIsOver = false;
            CameraController.Instance.ChangeCameraControlStatus(true);
            GameController.Instance.ChangePlayerControlState(true);
        }

        bool IsVictory()
        {
            if (remainingBlocks <= 0)
            {
                //Win
                return true;
            }

            return false;
        }
        
        bool IsLose()
        {
            if (heartCount <= 0)
            {
                //Lose
                return true;
            }

            return false;
        }

        void PlayWinSequence()
        {
            StartCoroutine(WinSequence());

            IEnumerator WinSequence()
            {
                CameraController.Instance.MoveBackToCenter(true);
                yield return new WaitUntil(() => !CameraController.Instance.IsCameraAnimationRunning);
                WinAnimation();
            }
        }

        void WinAnimation()
        {
            if (dotColorAnimation != null)
            {
                dotColorAnimation.Kill();
                dotColorAnimation = null;
            }

            dotColorAnimation = DOTween.Sequence();

            dotColorAnimation.Append(DOVirtual.Float(0f, 1f, 0.75f, (hueVal) =>
            {
                Color c = Color.HSVToRGB(hueVal, 1f, 1f);

                foreach (var sr in spawnedDots)
                {
                    sr.DotRenderer.color = c;
                }
            }).SetEase(Ease.Linear));

            dotColorAnimation.Append(DOVirtual.Float(1f, 0f, 0.5f, (alphaVal) =>
            {
                Color baseColor = spawnedDots[0].DotDefaultColor;
                baseColor.a = alphaVal;

                foreach (var sr in spawnedDots)
                {
                    sr.DotRenderer.color = baseColor;
                }
            }).SetEase(Ease.Linear));

            dotColorAnimation.OnComplete(delegate
            {
                dotColorAnimation = null;
                UIMainManager.Instance.winPanel.Show();
            });
        }
        
        #endregion
        
        #region Tutorial

        void CheckTutorialProgress(int levelID)
        {
            switch (levelID)
            {
                case 1:
                    if (!GameManager.Instance.PlayerProfile.FinishedTutorialType_1)
                        GameController.Instance.tutorialManager.StartTutorial_Type1();
                    break;
                case 5:
                    if (!GameManager.Instance.PlayerProfile.FinishedTutorialType_2)
                        GameController.Instance.tutorialManager.StartTutorial_Type2();
                    break;
            }
        }
        
        #endregion
        
        #region Getters, Setters

        public MoveableBlock GetBlockByID(int blockID)
        {
            if (blockColliders.ContainsKey(blockID))
            {
                return blockColliders[blockID];
            }
            return null;
        }
        
        public List<MoveableBlock> SpawnedBlocks => spawnedBlocks;
        public List<SpriteRenderer> SpawnedBlockRenderers => blockRenderers;
        public int loadedLevel => levelID;
        public bool IsLevelOver => levelIsOver;

        public int HeartCount => heartCount;

        public int NumberOfActiveBlocks()
        {
            int num = 0;
            for (int i = 0; i < spawnedBlocks.Count; i++)
            {
                if (spawnedBlocks[i].IsBlockActive && !spawnedBlocks[i].IsBlockMoving)
                {
                    num++;
                }
            }

            return num;
        }

        public List<MoveableBlock> GetActiveBlocks()
        {
            List<MoveableBlock> result = new List<MoveableBlock>();
            for (int i = 0; i < spawnedBlocks.Count; i++)
            {
                if (spawnedBlocks[i].IsBlockActive && !spawnedBlocks[i].IsBlockMoving)
                {
                    result.Add(spawnedBlocks[i]);
                }
            }

            return result;
        }

        public bool IsCurrentLevelHard => levelData.IsHard;
        public bool IsThreeStarsRating => heartCount >= 3;
        
        #endregion
    }
}
