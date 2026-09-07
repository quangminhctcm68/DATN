using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BrickEscape
{
    public class BoosterManager : MonoBehaviour
    {
        [FoldoutGroup("Hint Booster Data")] [SerializeField] private bool hintBoosterIsActive = false;
        [FoldoutGroup("Hint Booster Data")] [SerializeField] private MoveableBlock blockToDisableHintEffect;
        
        [FoldoutGroup("Hammer Booster Data")] [SerializeField] public bool hammerBoosterIsActive = false;
        [FoldoutGroup("Hammer Booster Data")] [SerializeField] private BoosterHammerControl boosterHammerControl;
        
        [FoldoutGroup("Magic Wand Booster Data")] [SerializeField] private bool magicWandBoosterIsActive = false;
        [FoldoutGroup("Magic Wand Booster Data")] [SerializeField] private int maxNumberOfBlocksForDeletion = 3;
        [FoldoutGroup("Magic Wand Booster Data")] [SerializeField] private int remainingBlocksToWait = 0;
        [FoldoutGroup("Magic Wand Booster Data")] [SerializeField] private List<MoveableBlock> blocksToDisappear = new List<MoveableBlock>();
        
        [SerializeField] private LevelGenerator levelGenerator;

        private Coroutine waitTillBlocksDisappear;
        
        #region Start, Update, Validate

        public void Init()
        {
            if (levelGenerator == null) levelGenerator = GameController.Instance.levelGenerator;
            boosterHammerControl.Init();
            ChangeHammerControlStatus(false);
            ChangeMagicWandBoosterStatus(false);
        }
        
        #endregion
        
        #region Hint Booster Logic

        [Button]
        public void ActivateHintBooster()
        {
           
            FindBlockWithClearPath();
            if (hammerBoosterIsActive)
            {
                DeactivateHammerBooster(false);
                GameController.Instance.ChangePlayerControlState(true);
            }
        }

        void FindBlockWithClearPath()
        {
            CameraController.Instance.HintEffect.Play();
            foreach (var block in levelGenerator.SpawnedBlocks)
            {
                if (block.IsBlockActive && !block.IsBlockMoving && !block.IsThereObstacleInFront())
                {
                    blockToDisableHintEffect = block;
                    block.PlayHintEffect();
                    AudioManager.Instance.PlaySFX(SFXID.Booster_Hint);
                    hintBoosterIsActive = true;
                    GameManager.Instance.PlayerProfile.ChangeHintBoosterUseCount(-1);
                    CameraController.Instance.LookAtThisBlock(block);
                    StartCoroutine(WaitForCameraAction());
                    break;
                }
            }

            IEnumerator WaitForCameraAction()
            {
                CameraController.Instance.ChangeCameraControlStatus(false);
                yield return new WaitUntil(() => !CameraController.Instance.IsCameraAnimationRunning);
                CameraController.Instance.ChangeCameraControlStatus(true);
            }
        }
        
        public void DeactivateHintBooster()
        {
            if (blockToDisableHintEffect != null)
            {
                blockToDisableHintEffect.StopColorFlashingEffect(true);
                blockToDisableHintEffect = null;
            }
            hintBoosterIsActive = false;
        }
        
        #endregion
        
        #region Hammer Booster Logic
        
        [Button]
        public void ActivateHammerBooster()
        {
            if (hammerBoosterIsActive)
            {
                DeactivateHammerBooster(false);
                GameController.Instance.ChangePlayerControlState(true);
                return;
            }
            GameController.Instance.ChangePlayerControlState(false);
            ChangeHammerBoosterStatus(true);
            ChangeHammerControlStatus(true);
            UIMainManager.Instance.gamePlayPanel.ShowDes();

            
        }
        
        public void DeactivateHammerBooster(bool hammerUsed)
        {
            ChangeHammerControlStatus(false);
            ChangeHammerBoosterStatus(false);
            if (hammerUsed) GameManager.Instance.PlayerProfile.ChangeHammerBoosterUseCount(-1);
            UIMainManager.Instance.gamePlayPanel.HideDes();
            //GameController.Instance.ChangePlayerControlState(true);
            //if (hammerUsed)
            //reduce use count
        }
        
        void ChangeHammerControlStatus(bool status)
        {
            boosterHammerControl.gameObject.SetActive(status);
        }

        public void ChangeHammerBoosterStatus(bool status)
        {
            hammerBoosterIsActive = status;
        }
        
        #endregion
        
        #region Magic Wand Booster Logic

        [Button]
        public void ActivateMagicWandBooster()
        {
            if (magicWandBoosterIsActive) return;
            CameraController.Instance.MagicWandLogic.SetTarget(GetBlocksForDeletion(GetMagicWandTargets()));
            GameManager.Instance.PlayerProfile.ChangeMagicWandBoosterUseCount(-1);
            CameraController.Instance.MagicWandLogic.PlayAnimation();
            if (hammerBoosterIsActive) 
            {
                DeactivateHammerBooster(false);
            }


        }
        public List<MoveableBlock> GetMagicWandTargets()
        {
            GameController.Instance.ChangePlayerControlState(false);
            ChangeMagicWandBoosterStatus(true);
            List<MoveableBlock> activeBlocks = levelGenerator.GetActiveBlocks();
            
            int j = 0;
            for (int i = activeBlocks.Count - 1; i > 0; i--)
            {
                j = Random.Range(0, i + 1);
                (activeBlocks[i], activeBlocks[j]) = (activeBlocks[j], activeBlocks[i]);
            }
            return activeBlocks;
            //ProcessDeletingBlocks();
        }

        List<MoveableBlock> GetBlocksForDeletion(List<MoveableBlock> blocks)
        {
            blocksToDisappear.Clear();
            
            if (blocks.Count > maxNumberOfBlocksForDeletion)
            {
                for (int i = 0; i < maxNumberOfBlocksForDeletion; i++)
                {
                    remainingBlocksToWait++;
                    //blocks[i].RemoveThisBlockMagically();
                    blocksToDisappear.Add(blocks[i]);
                }
            }
            else
            {
                foreach (var block in blocks)
                {
                    remainingBlocksToWait++;
                    //block.RemoveThisBlockMagically();
                    blocksToDisappear.Add(block);
                }
            }

            return blocksToDisappear;
        }

        public void ChangeMagicWandBoosterStatus(bool status)
        {
            magicWandBoosterIsActive = status;
        }
        
        #endregion
        
        #region Getters, Setters
        
        public bool HintBoosterIsActive => hintBoosterIsActive;
        public bool HammerBoosterIsActive => hammerBoosterIsActive;
        public bool MagicWandBoosterIsActive => magicWandBoosterIsActive;
        public MoveableBlock BlockToDisableHintEffect => blockToDisableHintEffect;
        public List<MoveableBlock> BlocksForRemoval => blocksToDisappear;

        #endregion
    }
}
