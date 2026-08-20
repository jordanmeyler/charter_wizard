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

        public static Vector3 Center(int x, int y) => new(x + 0.5f, y + 0.5f, 0f);
    }
}
