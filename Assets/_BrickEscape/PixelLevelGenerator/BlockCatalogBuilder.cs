using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using BrickEscape.PixelLevelGenerator.Model;

namespace BrickEscape.PixelLevelGenerator
{
    public class BlockCatalogBuildResult
    {
        public Dictionary<int, BlockDefinition> Catalog =
            new Dictionary<int, BlockDefinition>();

        public List<ValidationIssue> Issues =
            new List<ValidationIssue>();
    }

    public static class BlockCatalogBuilder
    {
        /// <summary>
        /// Build BlockDefinition catalog directly from DataCollection.blocks.
        ///
        /// Pipeline:
        /// DataCollection.blocks
        ///     -> Prefab
        ///     -> Collider2D => OccupiedOffsets
        ///     -> blockJoints => Joints
        ///     -> ArrowProfiles
        ///     -> BlockDefinition
        ///
        /// CSV / BlockShapeCatalogData fallback has intentionally been removed.
        /// </summary>
        public static BlockCatalogBuildResult Build(
            ScriptableObject dataCollectionAsset,
            float cellSize)
        {
            var result = new BlockCatalogBuildResult();

            if (dataCollectionAsset == null)
            {
                result.Issues.Add(
                    new ValidationIssue(
                        ValidationSeverity.Error,
                        IssueCodes.UnknownBlockPrefabId,
                        "DataCollection asset is null.",
                        "Assign a valid DataCollection asset."
                    )
                );

                return result;
            }

            if (cellSize <= 0f)
            {
                result.Issues.Add(
                    new ValidationIssue(
                        ValidationSeverity.Error,
                        IssueCodes.MissingCellValues,
                        $"Invalid cellSize: {cellSize}.",
                        "cellSize must be greater than zero."
                    )
                );

                return result;
            }

            Dictionary<int, GameObject> prefabsById =
                TryReadBlocksDictionary(dataCollectionAsset);

            if (prefabsById == null || prefabsById.Count == 0)
            {
                result.Issues.Add(
                    new ValidationIssue(
                        ValidationSeverity.Error,
                        IssueCodes.UnknownBlockPrefabId,
                        "DataCollection does not contain any valid block prefabs.",
                        "Check the blocks field/property on DataCollection."
                    )
                );

                return result;
            }

            foreach (var pair in prefabsById.OrderBy(x => x.Key))
            {
                int id = pair.Key;
                GameObject prefab = pair.Value;

                if (prefab == null)
                {
                    result.Issues.Add(
                        new ValidationIssue(
                            ValidationSeverity.Warning,
                            IssueCodes.UnknownBlockPrefabId,
                            $"Block ID {id} has a null prefab.",
                            "Assign a valid prefab to this block.",
                            blockId: id
                        )
                    );

                    continue;
                }

                BlockDefinition definition =
                    TryBuildFromPrefab(
                        id,
                        prefab,
                        cellSize,
                        result.Issues
                    );

                if (definition == null)
                {
                    result.Issues.Add(
                        new ValidationIssue(
                            ValidationSeverity.Warning,
                            IssueCodes.PrefabBlockIdMismatch,
                            $"Could not build BlockDefinition for prefab '{prefab.name}' (ID {id}).",
                            "Check that the prefab has Collider2D and blockJoints.",
                            blockId: id
                        )
                    );

                    continue;
                }

                ComputeArrowProfiles(
                    definition,
                    result.Issues
                );

                result.Catalog[id] = definition;
            }

            return result;
        }

        // ---------------------------------------------------------------------
        // DataCollection
        // ---------------------------------------------------------------------

        private static Dictionary<int, GameObject> TryReadBlocksDictionary(
            ScriptableObject dataCollectionAsset)
        {
            if (dataCollectionAsset == null)
                return null;

            object raw =
                ReflectionUtil.GetFieldOrPropertyValue(
                    dataCollectionAsset,
                    "blocks"
                )
                ??
                ReflectionUtil.GetFieldOrPropertyValue(
                    dataCollectionAsset,
                    "Blocks"
                );

            if (raw == null)
                return null;

            var output = new Dictionary<int, GameObject>();

            // -------------------------------------------------------------
            // Dictionary<int, GameObject>
            // -------------------------------------------------------------

            if (raw is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (!TryConvertToInt(entry.Key, out int id))
                        continue;

                    if (entry.Value is GameObject prefab)
                        output[id] = prefab;
                }

                return output;
            }

            // -------------------------------------------------------------
            // List<BlockEntry>
            // Supports:
            // Id / BlockId / Key
            // Prefab / Value
            // -------------------------------------------------------------

            if (raw is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    if (item == null)
                        continue;

                    if (!TryReadInt(
                            item,
                            out int id,
                            "Id",
                            "BlockId",
                            "Key"))
                    {
                        continue;
                    }

                    GameObject prefab =
                        ReflectionUtil.GetFieldOrPropertyValue(
                            item,
                            "Prefab"
                        ) as GameObject
                        ??
                        ReflectionUtil.GetFieldOrPropertyValue(
                            item,
                            "Value"
                        ) as GameObject;

                    if (prefab != null)
                        output[id] = prefab;
                }

                return output;
            }

            return null;
        }

        private static bool TryConvertToInt(
            object value,
            out int result)
        {
            result = 0;

            if (value == null)
                return false;

            if (value is int intValue)
            {
                result = intValue;
                return true;
            }

            try
            {
                result = Convert.ToInt32(value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool TryReadInt(
            object target,
            out int value,
            params string[] names)
        {
            value = 0;

            foreach (string name in names)
            {
                object raw =
                    ReflectionUtil.GetFieldOrPropertyValue(
                        target,
                        name
                    );

                if (TryConvertToInt(raw, out value))
                    return true;
            }

            return false;
        }

        // ---------------------------------------------------------------------
        // Prefab -> BlockDefinition
        // ---------------------------------------------------------------------

        private static BlockDefinition TryBuildFromPrefab(
            int id,
            GameObject prefab,
            float cellSize,
            List<ValidationIssue> issues)
        {
            if (prefab == null)
                return null;

            // -------------------------------------------------------------
            // Collider -> footprint
            // -------------------------------------------------------------

            Collider2D[] colliders =
                prefab.GetComponentsInChildren<Collider2D>(true);

            if (colliders == null || colliders.Length == 0)
            {
                issues.Add(
                    new ValidationIssue(
                        ValidationSeverity.Warning,
                        IssueCodes.MissingCellValues,
                        $"Prefab '{prefab.name}' has no Collider2D.",
                        "Add at least one Collider2D to the block prefab.",
                        blockId: id
                    )
                );

                return null;
            }

            var offsets =
                new HashSet<Vector2Int>();

            Bounds localBounds = default;
            bool hasBounds = false;

            foreach (Collider2D collider in colliders)
            {
                if (collider == null)
                    continue;

                Bounds worldBounds = collider.bounds;

                if (!hasBounds)
                {
                    localBounds = worldBounds;
                    hasBounds = true;
                }
                else
                {
                    localBounds.Encapsulate(worldBounds);
                }

                Vector3 localMin =
                    prefab.transform.InverseTransformPoint(
                        worldBounds.min
                    );

                Vector3 localMax =
                    prefab.transform.InverseTransformPoint(
                        worldBounds.max
                    );

                int minX =
                    Mathf.FloorToInt(
                        localMin.x / cellSize
                    );

                int maxX =
                    Mathf.CeilToInt(
                        localMax.x / cellSize
                    ) - 1;

                // Pixel grid Y grows downward.
                // Unity world Y grows upward.
                int minY =
                    Mathf.FloorToInt(
                        -localMax.y / cellSize
                    );

                int maxY =
                    Mathf.CeilToInt(
                        -localMin.y / cellSize
                    ) - 1;

                if (maxX < minX)
                    maxX = minX;

                if (maxY < minY)
                    maxY = minY;

                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        offsets.Add(
                            new Vector2Int(x, y)
                        );
                    }
                }
            }

            if (offsets.Count == 0)
            {
                issues.Add(
                    new ValidationIssue(
                        ValidationSeverity.Warning,
                        IssueCodes.MissingCellValues,
                        $"Prefab '{prefab.name}' produced an empty footprint.",
                        "Check Collider2D bounds and cellSize.",
                        blockId: id
                    )
                );

                return null;
            }

            // -------------------------------------------------------------
            // BlockJoint -> joints
            // -------------------------------------------------------------

            var joints =
                new List<JointDefinition>();

            object jointList =
                FindBlockJointsList(prefab);

            if (jointList is IEnumerable enumerableJoints)
            {
                int index = 1;

                foreach (object joint in enumerableJoints)
                {
                    if (joint == null)
                    {
                        index++;
                        continue;
                    }

                    Vector3 worldPosition =
                        ExtractJointWorldPosition(joint);

                    Vector3 localPosition =
                        prefab.transform.InverseTransformPoint(
                            worldPosition
                        );

                    int gridX =
                        Mathf.RoundToInt(
                            localPosition.x / cellSize
                        );

                    int gridY =
                        Mathf.RoundToInt(
                            -localPosition.y / cellSize
                        );

                    joints.Add(
                        new JointDefinition
                        {
                            JointIndex1Based = index,
                            LocalGridPosition =
                                new Vector2Int(
                                    gridX,
                                    gridY
                                )
                        }
                    );

                    index++;
                }
            }

            if (joints.Count == 0)
            {
                issues.Add(
                    new ValidationIssue(
                        ValidationSeverity.Warning,
                        IssueCodes.InvalidArrowJoint,
                        $"Prefab '{prefab.name}' has no valid block joints.",
                        "Check the blockJoints list on the block prefab.",
                        blockId: id
                    )
                );

                return null;
            }

            // -------------------------------------------------------------
            // Normalize footprint
            // -------------------------------------------------------------

            Vector2Int[] normalizedOffsets =
                NormalizeOffsets(offsets);

            var definition =
                new BlockDefinition
                {
                    PrefabId = id,
                    Name = prefab.name,

                    OccupiedOffsets =
                        normalizedOffsets,

                    Joints =
                        joints,

                    LocalBounds =
                        localBounds,

                    SourcedFromPrefab = true,

                    AllowedDirections =
                        DirectionMask.All
                };

            return definition;
        }

        // ---------------------------------------------------------------------
        // BlockJoint reflection helpers
        // ---------------------------------------------------------------------

        private static object FindBlockJointsList(
            GameObject prefab)
        {
            if (prefab == null)
                return null;

            // Root first.
            Component[] rootComponents =
                prefab.GetComponents<Component>();

            foreach (Component component in rootComponents)
            {
                if (component == null)
                    continue;

                object value =
                    ReflectionUtil.GetFieldOrPropertyValue(
                        component,
                        "blockJoints"
                    )
                    ??
                    ReflectionUtil.GetFieldOrPropertyValue(
                        component,
                        "BlockJoints"
                    );

                if (value != null)
                    return value;
            }

            // Fallback: search children as well.
            Component[] allComponents =
                prefab.GetComponentsInChildren<Component>(true);

            foreach (Component component in allComponents)
            {
                if (component == null)
                    continue;

                object value =
                    ReflectionUtil.GetFieldOrPropertyValue(
                        component,
                        "blockJoints"
                    )
                    ??
                    ReflectionUtil.GetFieldOrPropertyValue(
                        component,
                        "BlockJoints"
                    );

                if (value != null)
                    return value;
            }

            return null;
        }

        private static Vector3 ExtractJointWorldPosition(
            object joint)
        {
            if (joint == null)
                return Vector3.zero;

            // Most likely case:
            // BlockJoint : Component
            if (joint is Component component)
                return component.transform.position;

            object position =
                ReflectionUtil.GetFieldOrPropertyValue(
                    joint,
                    "position"
                )
                ??
                ReflectionUtil.GetFieldOrPropertyValue(
                    joint,
                    "Position"
                );

            if (position is Vector3 vector)
                return vector;

            // Some implementations may expose Transform.
            object transform =
                ReflectionUtil.GetFieldOrPropertyValue(
                    joint,
                    "transform"
                )
                ??
                ReflectionUtil.GetFieldOrPropertyValue(
                    joint,
                    "Transform"
                );

            if (transform is Transform jointTransform)
                return jointTransform.position;

            return Vector3.zero;
        }

        // ---------------------------------------------------------------------
        // Footprint normalization
        // ---------------------------------------------------------------------

        private static Vector2Int[] NormalizeOffsets(
            IEnumerable<Vector2Int> offsets)
        {
            var list =
                offsets
                    .Distinct()
                    .ToList();

            if (list.Count == 0)
                return Array.Empty<Vector2Int>();

            int minX =
                list.Min(x => x.x);

            int minY =
                list.Min(x => x.y);

            return list
                .Select(
                    x => new Vector2Int(
                        x.x - minX,
                        x.y - minY
                    )
                )
                .Distinct()
                .OrderBy(x => x.y)
                .ThenBy(x => x.x)
                .ToArray();
        }

        // ---------------------------------------------------------------------
        // Arrow profiles
        // ---------------------------------------------------------------------

        /// <summary>
        /// Resolve ArrowProfile for each valid movement direction.
        ///
        /// Rules:
        /// - Path length = footprint width along movement axis.
        /// - Anchor = joint on the leading edge.
        /// - Prefer joint closest to the perpendicular center.
        /// - Tie-break by smaller joint index.
        /// - Direction is invalid if there is no joint on the leading edge.
        /// </summary>
        private static void ComputeArrowProfiles(
            BlockDefinition def,
            List<ValidationIssue> issues)
        {
            if (def == null)
                return;

            if (def.OccupiedOffsets == null ||
                def.OccupiedOffsets.Length == 0)
            {
                issues.Add(
                    new ValidationIssue(
                        ValidationSeverity.Warning,
                        IssueCodes.NoArrowProfile,
                        $"Block {def.PrefabId} ({def.Name}) has no occupied cells.",
                        "Check BlockDefinition footprint.",
                        blockId: def.PrefabId
                    )
                );

                return;
            }

            if (def.Joints == null ||
                def.Joints.Count == 0)
            {
                issues.Add(
                    new ValidationIssue(
                        ValidationSeverity.Warning,
                        IssueCodes.NoArrowProfile,
                        $"Block {def.PrefabId} ({def.Name}) has no joints.",
                        "Check blockJoints on the prefab.",
                        blockId: def.PrefabId
                    )
                );

                return;
            }

            // Clear in case Build() is called more than once
            // on the same definition instance.
            def.ArrowProfiles.Clear();

            int minX =
                def.OccupiedOffsets.Min(
                    offset => offset.x
                );

            int maxX =
                def.OccupiedOffsets.Max(
                    offset => offset.x
                );

            int minY =
                def.OccupiedOffsets.Min(
                    offset => offset.y
                );

            int maxY =
                def.OccupiedOffsets.Max(
                    offset => offset.y
                );

            foreach (var move in DirectionUtil.AllDirections())
            {
                Vector2Int step =
                    DirectionUtil.GridStepFor(move);

                bool horizontal =
                    step.x != 0;

                int length;
                int leadingEdgeValue;

                if (horizontal)
                {
                    length =
                        maxX - minX + 1;

                    leadingEdgeValue =
                        step.x > 0
                            ? maxX
                            : minX;
                }
                else
                {
                    length =
                        maxY - minY + 1;

                    leadingEdgeValue =
                        step.y > 0
                            ? maxY
                            : minY;
                }

                // ---------------------------------------------------------
                // Find joints on leading edge
                // ---------------------------------------------------------

                List<JointDefinition> candidates =
                    def.Joints
                        .Where(
                            joint =>
                                horizontal
                                    ? joint.LocalGridPosition.x ==
                                      leadingEdgeValue
                                    : joint.LocalGridPosition.y ==
                                      leadingEdgeValue
                        )
                        .ToList();

                if (candidates.Count == 0)
                {
                    // No joint on leading edge => direction unavailable.
                    continue;
                }

                // ---------------------------------------------------------
                // Find joint closest to perpendicular center
                // ---------------------------------------------------------

                float centerPerpendicular =
                    horizontal
                        ? (minY + maxY) * 0.5f
                        : (minX + maxX) * 0.5f;

                var ordered =
                    candidates
                        .OrderBy(
                            joint =>
                                Mathf.Abs(
                                    (
                                        horizontal
                                            ? joint.LocalGridPosition.y
                                            : joint.LocalGridPosition.x
                                    )
                                    -
                                    centerPerpendicular
                                )
                        )
                        .ThenBy(
                            joint =>
                                joint.JointIndex1Based
                        )
                        .ToList();

                JointDefinition anchor =
                    ordered[0];

                // ---------------------------------------------------------
                // Detect tie-break
                // ---------------------------------------------------------

                bool isTieBreak = false;

                if (ordered.Count > 1)
                {
                    float firstDistance =
                        Mathf.Abs(
                            (
                                horizontal
                                    ? ordered[0].LocalGridPosition.y
                                    : ordered[0].LocalGridPosition.x
                            )
                            -
                            centerPerpendicular
                        );

                    float secondDistance =
                        Mathf.Abs(
                            (
                                horizontal
                                    ? ordered[1].LocalGridPosition.y
                                    : ordered[1].LocalGridPosition.x
                            )
                            -
                            centerPerpendicular
                        );

                    isTieBreak =
                        Mathf.Approximately(
                            firstDistance,
                            secondDistance
                        );
                }

                // ---------------------------------------------------------
                // Build directed path
                // ---------------------------------------------------------

                var path =
                    new List<Vector2Int>(
                        length
                    );

                Vector2Int cursor =
                    anchor.LocalGridPosition;

                for (int i = 0; i < length; i++)
                {
                    path.Add(cursor);
                    cursor += step;
                }

                // ---------------------------------------------------------
                // Validate generated path against footprint
                // ---------------------------------------------------------

                var occupied =
                    new HashSet<Vector2Int>(
                        def.OccupiedOffsets
                    );

                bool pathValid = true;

                foreach (Vector2Int cell in path)
                {
                    if (occupied.Contains(cell))
                        continue;

                    pathValid = false;

                    issues.Add(
                        new ValidationIssue(
                            ValidationSeverity.Warning,
                            IssueCodes.InvalidArrowJoint,
                            $"Arrow path of block {def.PrefabId} ({def.Name}) " +
                            $"for direction {move} leaves the block footprint at {cell}.",
                            "Check joint position and block footprint.",
                            blockId: def.PrefabId,
                            gridPosition: cell
                        )
                    );

                    break;
                }

                if (!pathValid)
                    continue;

                // ---------------------------------------------------------
                // Save profile
                // ---------------------------------------------------------

                def.ArrowProfiles[move] =
                    new ArrowProfile
                    {
                        Direction = move,

                        AnchorJointIndex1Based =
                            anchor.JointIndex1Based,

                        LongestLengthInCells =
                            length,

                        DirectedPath =
                            path.ToArray(),

                        IsTieBreak =
                            isTieBreak
                    };
            }

            if (def.ArrowProfiles.Count == 0)
            {
                issues.Add(
                    new ValidationIssue(
                        ValidationSeverity.Warning,
                        IssueCodes.NoArrowProfile,
                        $"Block {def.PrefabId} ({def.Name}) has no valid ArrowProfile.",
                        "Check joint positions against the block footprint.",
                        blockId: def.PrefabId
                    )
                );
            }
        }
    }
}