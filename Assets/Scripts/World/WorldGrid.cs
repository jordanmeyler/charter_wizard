using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public sealed class WorldGrid : MonoBehaviour
    {
        public const float TileSize = 1f;

        readonly Dictionary<Vector2Int, WorldTile> _tiles = new();

        public IEnumerable<WorldTile> All => _tiles.Values;

        public WorldTile Get(int x, int y) => Get(new Vector2Int(x, y));

        public WorldTile Get(Vector2Int coord)
        {
            return _tiles.TryGetValue(coord, out var tile) ? tile : null;
        }

        public WorldTile TileAtWorld(Vector3 world)
        {
            return Get(WorldWork.CoordOf(world));
        }

        public WorldTile EnsureOpenPit(int x, int y)
        {
            var existing = Get(x, y);
            if (existing != null)
            {
                return existing;
            }

            var tile = Set(x, y, TileKind.Pit, MaterialId.Void);
            tile.MarkOpenVoid();
            return tile;
        }

        public WorldTile Set(int x, int y, TileKind kind, MaterialId material)
        {
            var coord = new Vector2Int(x, y);
            if (_tiles.TryGetValue(coord, out var existing) && existing != null)
            {
                DestroyImmediate(existing.gameObject);
            }

            var tileObject = new GameObject($"Tile_{kind}_{material}_{x}_{y}");
            tileObject.transform.SetParent(transform, false);
            var tile = tileObject.AddComponent<WorldTile>();
            tile.Bind(coord, new TileDef(kind, material));
            _tiles[coord] = tile;
            return tile;
        }

        public WorldTile Set(int x, int y, TileKind kind, TileSubstance substance)
        {
            return Set(x, y, kind, MaterialCatalog.FromLegacy(substance));
        }

        public WorldTile Set(int x, int y, TileKind kind, RuneId element)
        {
            return Set(x, y, kind, MaterialCatalog.FromElement(element));
        }

        public void Fill(int x0, int y0, int x1, int y1, TileKind kind, MaterialId material)
        {
            for (var y = y0; y <= y1; y++)
            {
                for (var x = x0; x <= x1; x++)
                {
                    Set(x, y, kind, material);
                }
            }
        }

        public void Fill(int x0, int y0, int x1, int y1, TileKind kind, TileSubstance substance)
        {
            Fill(x0, y0, x1, y1, kind, MaterialCatalog.FromLegacy(substance));
        }

        public void Fill(int x0, int y0, int x1, int y1, TileKind kind, RuneId element)
        {
            Fill(x0, y0, x1, y1, kind, MaterialCatalog.FromElement(element));
        }

        public void RoomShell(int x0, int y0, int x1, int y1, MaterialId wall, MaterialId floor)
        {
            Fill(x0, y0, x1, y1, TileKind.Wall, wall);
            Fill(x0 + 1, y0 + 1, x1 - 1, y1 - 1, TileKind.Floor, floor);
        }

        public void RoomShell(int x0, int y0, int x1, int y1, TileSubstance wall, TileSubstance floor)
        {
            RoomShell(x0, y0, x1, y1, MaterialCatalog.FromLegacy(wall), MaterialCatalog.FromLegacy(floor));
        }

        public void RoomShell(int x0, int y0, int x1, int y1, RuneId wall, RuneId floor)
        {
            RoomShell(x0, y0, x1, y1, MaterialCatalog.FromElement(wall), MaterialCatalog.FromElement(floor));
        }

        public void DressLooks()
        {
            foreach (var tile in _tiles.Values)
            {
                tile?.DressNeighborhood(this);
            }
        }

        public List<WorldTile> Neighbors(Vector2Int coord)
        {
            var list = new List<WorldTile>(4);
            TryAdd(list, coord.x + 1, coord.y);
            TryAdd(list, coord.x - 1, coord.y);
            TryAdd(list, coord.x, coord.y + 1);
            TryAdd(list, coord.x, coord.y - 1);
            return list;
        }

        public List<WorldTile> TilesInRadius(Vector3 world, float radius)
        {
            var list = new List<WorldTile>();
            var center = WorldWork.CoordOf(world);
            var reach = Mathf.Max(1, Mathf.CeilToInt(radius));
            for (var y = center.y - reach; y <= center.y + reach; y++)
            {
                for (var x = center.x - reach; x <= center.x + reach; x++)
                {
                    var tile = Get(x, y);
                    if (tile != null && Vector2.Distance(world, tile.transform.position) <= radius + 0.15f)
                    {
                        list.Add(tile);
                    }
                }
            }

            return list;
        }

        void TryAdd(List<WorldTile> list, int x, int y)
        {
            var tile = Get(x, y);
            if (tile != null)
            {
                list.Add(tile);
            }
        }

        /// <summary>
        /// A spell-watered plant may take neighboring tiles that
        /// already hold water — a water floor or a water covering.
        /// Budget is the spell's reach. Forest also stays on screen.
        /// </summary>
        public bool SpreadPlant(WorldTile from)
        {
            return SpreadPlant(from, 1) > 0;
        }

        public int SpreadPlant(WorldTile from, int budget, bool visibleOnly = false)
        {
            if (from == null || budget <= 0)
            {
                return 0;
            }

            if (!from.HasWaterSource
                && !from.IsPlantish
                && !from.HasPlantCover
                && !from.HasPlantishDetail
                && !from.IsDeepWater
                && !from.HasWaterCover)
            {
                return 0;
            }

            var grown = from.Growth >= 2 || from.Material == MaterialId.Grove
                ? MaterialId.Grove
                : MaterialId.Plant;
            var used = 0;
            var seen = new HashSet<Vector2Int>();
            var queue = new Queue<WorldTile>();
            EnqueueWater(from, seen, queue, visibleOnly);
            var startNeighbors = Neighbors(from.Coord);
            for (var i = 0; i < startNeighbors.Count; i++)
            {
                EnqueueWater(startNeighbors[i], seen, queue, visibleOnly);
            }

            while (queue.Count > 0 && used < budget)
            {
                var tile = queue.Dequeue();
                if (tile.HasAshCover)
                {
                    continue;
                }

                if (tile.IsPlantish || tile.HasPlantCover)
                {
                    var next = Neighbors(tile.Coord);
                    for (var n = 0; n < next.Count; n++)
                    {
                        EnqueueWater(next[n], seen, queue, visibleOnly);
                    }

                    continue;
                }

                var took = false;
                if (tile.IsDeepWater)
                {
                    took = tile.GrowOverWater(grown);
                }
                else if (tile.HasWaterCover)
                {
                    tile.PaintCover(TileCover.Vine);
                    took = true;
                }

                if (!took)
                {
                    continue;
                }

                used++;
                var around = Neighbors(tile.Coord);
                for (var n = 0; n < around.Count; n++)
                {
                    EnqueueWater(around[n], seen, queue, visibleOnly);
                }
            }

            return used;
        }

        void EnqueueWater(
            WorldTile tile,
            HashSet<Vector2Int> seen,
            Queue<WorldTile> queue,
            bool visibleOnly)
        {
            if (tile == null || !seen.Add(tile.Coord))
            {
                return;
            }

            if (visibleOnly && !PlantLaw.OnScreen(tile.WorldOrigin))
            {
                return;
            }

            if (tile.IsDeepWater || tile.HasWaterCover || tile.HasPlantCover || tile.IsPlantish)
            {
                queue.Enqueue(tile);
            }
        }

        public static Vector3 Center(int x, int y) => new(x + 0.5f, y + 0.5f, 0f);
    }
}
