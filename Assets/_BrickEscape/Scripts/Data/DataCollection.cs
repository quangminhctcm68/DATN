using System.Collections;
using System.Collections.Generic;
using NabaGame.UI;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BrickEscape
{
    [CreateAssetMenu(fileName = "Data Collection", menuName = "GameData/DataCollection")]

    public class DataCollection : SerializedScriptableObject
    {
        [SerializeField] private Dictionary<int, Color> colorData = new Dictionary<int, Color>();
        [SerializeField] private Dictionary<int, BlockMovement> directionData = new Dictionary<int, BlockMovement>();
        [SerializeField] private Dictionary<EffectID, ParticleSystem> effectPrefabs = new Dictionary<EffectID, ParticleSystem>();
        [SerializeField] private Dictionary<RewardType, Sprite> rewardIcons = new Dictionary<RewardType, Sprite>();
        [SerializeField] public ClickEffect clickEffect;

        [Space]
        [SerializeField] private RawColorData rawColorData;
        [SerializeField] private RawMoveData rawMoveData;
        
        [Space]
        [SerializeField] private GameDot dotPrefab;
        [SerializeField] private Race_Avatar race_Avatar;
        [SerializeField] private GameObject trailObject;
        [SerializeField] private Dictionary<int, MoveableBlock> blocks = new Dictionary<int, MoveableBlock>();

#if UNITY_EDITOR

        [Button]
        private void GetColorData()
        {
            Color tempColor = Color.white;
            colorData.Clear();
            foreach (var rawColor in rawColorData.rawColors)
            {
                if (ColorUtility.TryParseHtmlString(rawColor.Color, out tempColor))
                {
                    colorData[rawColor.ID] = tempColor;
                }
            }
        }

        [Button]
        private void GetMoveDirectionData()
        {
            directionData.Clear();
            foreach (var rawMove in rawMoveData.rawMoves)
            {
                directionData[rawMove.MoveID] = rawMove.MoveType;
            }
        }

#endif
        
        #region Get Data
        
        public GameDot GetDotPrefab() => dotPrefab;
        public Race_Avatar GetRaceAvatarPrefab() => race_Avatar;

        public MoveableBlock GetBlockByID(int id)
        {
            if (blocks.ContainsKey(id))
            {
                return blocks[id];
            }

            return null;
        }
        
        public Color GetColorByID(int id)
        {
            if (colorData.ContainsKey(id))
            {
                return colorData[id];
            }

            return Color.white;
        }
        
        public BlockMovement GetMoveDirectionByID(int id)
        {
            if (directionData.ContainsKey(id))
            {
                return directionData[id];
            }

            return BlockMovement.None;
        }
        
        public ParticleSystem GetEffect(EffectID effectID)
        {
            if (effectPrefabs.ContainsKey(effectID)) return effectPrefabs[effectID];
            return null;
        }
        
        public Sprite GetRewardIcon(RewardType rewardType)
        {
            if (rewardIcons.ContainsKey(rewardType)) return rewardIcons[rewardType];
            return null;
        }
        
        public GameObject GetTrailObject() => trailObject;
        
        #endregion
    }
}
