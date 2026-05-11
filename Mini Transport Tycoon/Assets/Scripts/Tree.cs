using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityTilemap = UnityEngine.Tilemaps.Tilemap;

namespace MiniTransportTycoon
{
    public class Tree : MonoBehaviour
    {
        private static readonly Vector3Int[] NeighborOffsets =
        {
            new Vector3Int(-1, -1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0)
        };

        [Header("Tree Tilemaps")]
        [SerializeField] private UnityTilemap groundTilemap;
        [SerializeField] private UnityTilemap treeTilemap;
        [SerializeField] private TileBase treeTile;

        [Header("Spread Settings")]
        [SerializeField] private float spreadIntervalSeconds = 6f;
        [SerializeField, Range(0f, 1f)] private float spreadChancePerTree = 0.2f;
        [SerializeField] private int maxNewTreesPerTick = 3;
        [SerializeField] private bool seedWhenEmpty = true;
        [SerializeField] private int randomSeedAttempts = 128;

        private readonly List<Vector3Int> sourceTrees = new List<Vector3Int>();
        private readonly List<UnityTilemap> blockerTilemaps = new List<UnityTilemap>();
        private float spreadTimer;

        private void Awake()
        {
            CacheBlockerTilemaps();
        }

        private void Update()
        {
            if (!CanSpread())
                return;

            spreadTimer += Time.deltaTime;
            if (spreadTimer < spreadIntervalSeconds)
                return;

            spreadTimer = 0f;
            SpreadTrees();
        }

        private bool CanSpread()
        {
            if (blockerTilemaps.Count == 0)
            {
                CacheBlockerTilemaps();
            }

            return spreadIntervalSeconds > 0f
                && treeTilemap != null
                && groundTilemap != null
                && treeTile != null;
        }

        private void CacheBlockerTilemaps()
        {
            blockerTilemaps.Clear();

            UnityTilemap[] allTilemaps = FindObjectsOfType<UnityTilemap>(true);
            for (int i = 0; i < allTilemaps.Length; i++)
            {
                UnityTilemap candidate = allTilemaps[i];
                if (candidate == null)
                    continue;

                if (candidate == groundTilemap || candidate == treeTilemap)
                    continue;

                blockerTilemaps.Add(candidate);
            }
        }

        private void SpreadTrees()
        {
            sourceTrees.Clear();
            BoundsInt bounds = treeTilemap.cellBounds;

            foreach (Vector3Int cell in bounds.allPositionsWithin)
            {
                if (treeTilemap.HasTile(cell))
                {
                    sourceTrees.Add(cell);
                }
            }

            if (sourceTrees.Count == 0)
            {
                if (seedWhenEmpty)
                {
                    TrySpawnInitialSeed();
                }

                return;
            }

            int spawnedThisTick = 0;

            for (int i = 0; i < sourceTrees.Count; i++)
            {
                if (spawnedThisTick >= maxNewTreesPerTick)
                    return;

                if (Random.value > spreadChancePerTree)
                    continue;

                if (TryGrowNear(sourceTrees[i]))
                {
                    spawnedThisTick++;
                }
            }
        }

        private bool TryGrowNear(Vector3Int sourceCell)
        {
            int startOffsetIndex = Random.Range(0, NeighborOffsets.Length);

            for (int i = 0; i < NeighborOffsets.Length; i++)
            {
                int offsetIndex = (startOffsetIndex + i) % NeighborOffsets.Length;
                Vector3Int targetCell = sourceCell + NeighborOffsets[offsetIndex];

                if (!CanPlaceTreeAt(targetCell))
                    continue;

                treeTilemap.SetTile(targetCell, treeTile);
                return true;
            }

            return false;
        }

        private bool TrySpawnInitialSeed()
        {
            BoundsInt bounds = groundTilemap.cellBounds;
            int width = bounds.size.x;
            int height = bounds.size.y;

            if (width <= 0 || height <= 0)
                return false;

            int attempts = Mathf.Max(1, randomSeedAttempts);
            for (int i = 0; i < attempts; i++)
            {
                int x = Random.Range(bounds.xMin, bounds.xMax);
                int y = Random.Range(bounds.yMin, bounds.yMax);
                Vector3Int candidate = new Vector3Int(x, y, 0);

                if (!CanPlaceTreeAt(candidate))
                    continue;

                treeTilemap.SetTile(candidate, treeTile);
                return true;
            }

            foreach (Vector3Int cell in bounds.allPositionsWithin)
            {
                if (!CanPlaceTreeAt(cell))
                    continue;

                treeTilemap.SetTile(cell, treeTile);
                return true;
            }

            return false;
        }

        private bool CanPlaceTreeAt(Vector3Int cellPos)
        {
            if (!groundTilemap.HasTile(cellPos))
                return false;

            if (treeTilemap.HasTile(cellPos))
                return false;

            for (int i = 0; i < blockerTilemaps.Count; i++)
            {
                UnityTilemap blocker = blockerTilemaps[i];
                if (blocker != null && blocker.HasTile(cellPos))
                    return false;
            }

            return true;
        }
    }
}
