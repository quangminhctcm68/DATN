// Model du lieu block catalog (prefab) va placement tren board.
// Tham chieu: TAI_LIEU_THIET_KE_TOOL_PIXEL_TO_RAW_LEVEL.txt muc 3, 3.RE1.1-4, muc 8.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape.PixelLevelGenerator.Model
{
    /// <summary>
    /// Trung voi DataCollection.directionData: 1=Up,2=Down,3=Left,4=Right (0/khac = None).
    /// </summary>
    public enum BlockMovement
    {
        None = 0,
        Up = 1,
        Down = 2,
        Left = 3,
        Right = 4
    }

    [Flags]
    public enum DirectionMask
    {
        None = 0,
        Up = 1 << 0,
        Down = 1 << 1,
        Left = 1 << 2,
        Right = 1 << 3,
        All = Up | Down | Left | Right
    }

    public static class DirectionUtil
    {
        public static Vector2Int WorldStepFor(BlockMovement move)
        {
            // Giong SetupMoveDirection() cua MoveableBlock: Up(0,1) Down(0,-1) Left(-1,0) Right(1,0) trong world.
            switch (move)
            {
                case BlockMovement.Up: return new Vector2Int(0, 1);
                case BlockMovement.Down: return new Vector2Int(0, -1);
                case BlockMovement.Left: return new Vector2Int(-1, 0);
                case BlockMovement.Right: return new Vector2Int(1, 0);
                default: return Vector2Int.zero;
            }
        }

        /// <summary>
        /// Buoc di theo he toa do GRID cua raw data (x phai, y XUONG, giong RawLevel row/col).
        /// The gioi world Y = -y*CellSize nen Up (world +Y) tuong ung grid -Y (row giam).
        /// </summary>
        public static Vector2Int GridStepFor(BlockMovement move)
        {
            switch (move)
            {
                case BlockMovement.Up: return new Vector2Int(0, -1);
                case BlockMovement.Down: return new Vector2Int(0, 1);
                case BlockMovement.Left: return new Vector2Int(-1, 0);
                case BlockMovement.Right: return new Vector2Int(1, 0);
                default: return Vector2Int.zero;
            }
        }

        public static DirectionMask ToMask(BlockMovement move)
        {
            switch (move)
            {
                case BlockMovement.Up: return DirectionMask.Up;
                case BlockMovement.Down: return DirectionMask.Down;
                case BlockMovement.Left: return DirectionMask.Left;
                case BlockMovement.Right: return DirectionMask.Right;
                default: return DirectionMask.None;
            }
        }

        public static IEnumerable<BlockMovement> AllDirections()
        {
            yield return BlockMovement.Up;
            yield return BlockMovement.Down;
            yield return BlockMovement.Left;
            yield return BlockMovement.Right;
        }
    }

    [Serializable]
    public class JointDefinition
    {
        public int JointIndex1Based;
        public Vector2Int LocalGridPosition; // vi tri cell trong footprint, goc la origin (0,0)
    }

    /// <summary>
    /// ArrowProfile: path dai nhat cua block theo mot huong, va joint anchor tuong ung.
    /// Sinh tu BlockCatalogBuilder theo quy tac muc 3 (Revision 1.1) cua tai lieu thiet ke.
    /// </summary>
    [Serializable]
    public class ArrowProfile
    {
        public BlockMovement Direction;
        public int AnchorJointIndex1Based;
        public int LongestLengthInCells;
        public Vector2Int[] DirectedPath;
        public bool IsTieBreak;
    }

    /// <summary>
    /// Dinh nghia mot block prefab da duoc profile: footprint, joint, arrow profile theo huong.
    /// </summary>
    [Serializable]
    public class BlockDefinition
    {
        public int PrefabId;
        public string Name;
        public Vector2Int[] OccupiedOffsets;         // footprint cell, relative origin (0,0)
        public List<JointDefinition> Joints = new List<JointDefinition>();
        public DirectionMask AllowedDirections;
        public Dictionary<BlockMovement, ArrowProfile> ArrowProfiles = new Dictionary<BlockMovement, ArrowProfile>();
        public Bounds LocalBounds;
        public float Weight = 1f;
        public bool SourcedFromPrefab;
        public bool SourcedFromFallbackCatalog;
        public List<string> CatalogWarnings = new List<string>();

        public bool HasArrowProfile(BlockMovement move) => ArrowProfiles.ContainsKey(move) && ArrowProfiles[move] != null;

        public Vector2Int FootprintSize()
        {
            int minX = int.MaxValue, minY = int.MaxValue, maxX = int.MinValue, maxY = int.MinValue;
            foreach (var o in OccupiedOffsets)
            {
                minX = Mathf.Min(minX, o.x); maxX = Mathf.Max(maxX, o.x);
                minY = Mathf.Min(minY, o.y); maxY = Mathf.Max(maxY, o.y);
            }
            return new Vector2Int(maxX - minX + 1, maxY - minY + 1);
        }
    }

    /// <summary>
    /// Mot block da duoc dat vao candidate: origin + huong + arrow da resolve tu catalog.
    /// </summary>
    [Serializable]
    public class BlockPlacement
    {
        public int PlacementId;
        public int PrefabId;
        public Vector2Int Origin;      // grid coord (x=col-1, y=row)
        public int ColorId;
        public BlockMovement Move;
        public int ArrowStartIndex;    // 1-based
        public int ArrowLength;
        public Vector2Int[] OccupiedCells; // cache footprint da dich chuyen theo Origin

        public int MoveId => (int)Move;
    }
}