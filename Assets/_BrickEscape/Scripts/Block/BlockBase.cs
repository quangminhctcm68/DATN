using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NabaGame.Core.Runtime.TickManager;
using Sirenix.OdinInspector;
using Tiny;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BrickEscape
{
    public abstract class BlockBase : SerializedMonoBehaviour
    {
        [FoldoutGroup("Block Info")] [SerializeField] protected int blockID;
        [FoldoutGroup("Block Info")] [SerializeField] protected Color blockOgColor;
        [FoldoutGroup("Block Info")] [SerializeField] protected Transform blockCenterPoint;
        [FoldoutGroup("Block Info")] [SerializeField] protected Vector3 blockCenterPointOGPos;
        [FoldoutGroup("Block Movement Info")] [SerializeField] protected BlockMovement blockMovementDirection;
        
        [FoldoutGroup("Block Visual")] [SerializeField] protected List<BlockJoint> blockJoints = new List<BlockJoint>();
        [FoldoutGroup("Block Visual")] [SerializeField] protected BlockJoint chosenBlockJoint;
        [FoldoutGroup("Block Visual")] [SerializeField] protected List<SpriteRenderer> blockRenderers;
        [FoldoutGroup("Block Visual")] [SerializeField] protected GameObject blockSpriteCenter;
        [FoldoutGroup("Block Visual")] [SerializeField] protected SpriteRenderer currentArrowBodyRenderer;
        [FoldoutGroup("Block Visual")] [SerializeField] protected Color arrowOgColor;
        [FoldoutGroup("Block Visual")] [SerializeField] protected Vector3 blockRendererOgScale;
        [FoldoutGroup("Block Visual")] [SerializeField] protected Trail blockTrail;
        [FoldoutGroup("Block Visual")] [SerializeField] protected Dictionary<BlockMovement, Vector3[]> blockTrailData = new Dictionary<BlockMovement, Vector3[]>();
        [FoldoutGroup("Block Visual")] [SerializeField] protected List<TrailData> trailDataList = new List<TrailData>();
        [FoldoutGroup("Block Visual")] [SerializeField] protected List<ParticleSystem> trailExtraEffectList = new List<ParticleSystem>();
        [FoldoutGroup("Block Visual")] [SerializeField] private int arrowStartIndex = -1;
        
        [FoldoutGroup("Block Collision")] [SerializeField] protected Rigidbody2D blockRb;
        [FoldoutGroup("Block Collision")] [SerializeField] protected List<Collider2D> blockCollider = new List<Collider2D>();
        [FoldoutGroup("Block Collision")] [SerializeField] protected Bounds blockBounds;
        [FoldoutGroup("Block Collision")] [SerializeField] protected Vector2 boundsOffset;

        #region Start, Update, Validate

        public abstract void Init();
        
        protected virtual void OnValidate()
        {
            blockJoints = new List<BlockJoint>(GetComponentsInChildren<BlockJoint>());
            blockCollider = new List<Collider2D>(GetComponentsInChildren<Collider2D>());
            blockRb = GetComponent<Rigidbody2D>();
            if (blockSpriteCenter != null)
            {
                blockRendererOgScale = blockSpriteCenter.transform.localScale;
                blockRenderers = blockSpriteCenter.transform.GetComponentsInChildren<SpriteRenderer>().ToList();
            }
            
            foreach (var col in blockCollider)
                col.isTrigger = true;
            blockCenterPointOGPos = blockCenterPoint.localPosition;
        }
        
        #endregion
        
        #region Public Functions

        public virtual void SetColor(int colorID)
        {
            // foreach (var joint in blockJoints)
            // {
            //     joint.SetColor(GameManager.Instance.dataCollection.GetColorByID(colorID));
            // }
            
            blockOgColor = GameManager.Instance.dataCollection.GetColorByID(colorID);
            //blockRenderers[0].color = blockOgColor;
            foreach (var blockRenderer in blockRenderers)
            {
                blockRenderer.color = blockOgColor;
            }
        }

        public void SetArrow(BlockMovement blockMovementDirection, int startIndex, int arrowLength)
        {
            if (startIndex - 1 < blockJoints.Count)
            {
                arrowStartIndex = startIndex - 1;
                chosenBlockJoint = blockJoints[startIndex - 1];
                chosenBlockJoint.ShowArrow(blockMovementDirection, arrowLength);
                chosenBlockJoint.FadeArrowAndArrowBody();
                
                currentArrowBodyRenderer = chosenBlockJoint.ArrowBodyRenderer;
                arrowOgColor = chosenBlockJoint.ArrowOgColor;
            }

            this.blockMovementDirection = blockMovementDirection;
        }

        public void ResetArrowVisibility()
        {
            foreach (var joint in blockJoints)
            {
                joint.HideArrowAndArrowBody();
            }
        }

        [Button]
        public void SetupBounds()
        {
            blockBounds = blockCollider[0].bounds;
            foreach (var col in blockCollider)
            {
                blockBounds.Encapsulate(col.bounds);
            }
            
            boundsOffset = blockBounds.center - transform.position;
        }
        
        #endregion
        
        #region Player Interaction Logic

        public abstract void OnClick();
        
        #endregion
        
        #region Getters, Setters
        
        public int BlockID => blockID;
        public List<Collider2D> BlockCollider => blockCollider;
        public Bounds BlockBounds => blockBounds;
        //public SpriteRenderer BlockRenderer => blockRenderer;
        public List<SpriteRenderer> BlockRenderers => blockRenderers;
        public Transform BlockCenterPoint => blockCenterPoint;

        [Button]
        public void ChangeCollisionStatus(bool status)
        {
            foreach (var col in blockCollider)
            {
                col.enabled = status;
            }
        }

        public Vector3 GetBlockBoundsCenter()
        {
            SetupBounds();
            return blockBounds.center;
        }
        
        #endregion
        
        #region Manual Saving

        [Button]
        void SaveTrailDataManually()
        {
            trailDataList.Clear();
            foreach (var trailData in blockTrailData)
            {
                trailDataList.Add(new TrailData(trailData.Key, trailData.Value));
            }
        }
        
        #endregion
    }
}

[Serializable]
public class TrailData
{
    public BlockMovement movementDirection;
    public Vector3[] directionData;

    public TrailData(BlockMovement movementDirection, Vector3[] directionData)
    {
        this.movementDirection = movementDirection;
        this.directionData = (Vector3[]) directionData.Clone(); 
    }
}
