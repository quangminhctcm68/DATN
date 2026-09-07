// PixelGeneratorBlockDataCollection.cs
//
// SO rieng cho Pixel Level Generator, dung de gan vao o "Data Collection"
// trong PixelLevelGeneratorWindow.
//
// BlockCatalogBuilder.TryReadBlocksDictionary() doc field ten "blocks" hoac "Blocks"
// tren BAT KY ScriptableObject nao (bang reflection), chap nhan 2 dang:
//   1) Dictionary<int, GameObject>
//   2) List<T> voi T co field/property: Id (hoac BlockId/Key) va Prefab (hoac Value)
//
// Class nay dung dang (2): List<BlockPrefabEntry>.
//
// KHONG dung de thay the asset "DataCollection" production that
// (asset do GameManager.Instance.dataCollection tro toi, con co GetColorByID(),
// va cac data khac khong lien quan block catalog). Day la asset RIENG,
// chi phuc vu Pixel Level Generator.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace BrickEscape.PixelLevelGenerator
{
    [Serializable]
    public class BlockPrefabEntry
    {
        [Tooltip("Trung voi PrefabId dung trong RawLevel / BlockDefinition.")]
        public int Id;

        [Tooltip("Prefab block (phai co Collider2D va blockJoints).")]
        public GameObject Prefab;
    }

    [CreateAssetMenu(
        fileName = "PixelGeneratorBlockDataCollection",
        menuName = "Brick Escape/Pixel Level Generator/Block Data Collection")]
    public class PixelGeneratorBlockDataCollection : ScriptableObject
    {
        // Ten field PHAI la "blocks" (chu thuong) de khop voi
        // BlockCatalogBuilder.TryReadBlocksDictionary().
        public List<BlockPrefabEntry> blocks = new List<BlockPrefabEntry>();

        // ---------------------------------------------------------------
        // Helper (tuy chon) cho code khac trong tool neu can tra cuu nhanh.
        // ---------------------------------------------------------------

        public GameObject GetPrefabById(int id)
        {
            foreach (var entry in blocks)
            {
                if (entry != null && entry.Id == id)
                    return entry.Prefab;
            }

            return null;
        }

        public bool HasDuplicateIds(out List<int> duplicateIds)
        {
            duplicateIds = new List<int>();

            var seen = new HashSet<int>();

            foreach (var entry in blocks)
            {
                if (entry == null)
                    continue;

                if (!seen.Add(entry.Id))
                {
                    duplicateIds.Add(entry.Id);
                }
            }

            return duplicateIds.Count > 0;
        }
    }
}
