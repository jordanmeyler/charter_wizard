using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// Turns scene Tilemaps you painted in the editor into a live WorldGrid.
    /// The walk layer sets kind + material. An optional Cover layer only
    /// stamps aura and covering.
    /// </summary>
    public static class TilemapLevel
    {
        public static Tilemap FindPaintedMap()
        {
            var maps = Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            Tilemap best = null;
            var count = 0;
            for (var i = 0; i < maps.Length; i++)
            {
                var painted = CountPainted(maps[i]);
                if (painted > count)
                {
                    count = painted;
                    best = maps[i];
                }
            }

            return count > 0 ? best : null;
        }

        public static bool HasPaintedMap() => FindPaintedMap() != null;

        public static SanctumBuild Bake(LevelAuthoring spec, Tilemap map = null)
        {
            var floors = map != null
                ? map
                : spec != null && spec.tilemap != null
                    ? spec.tilemap
                    : FindPaintedMap();
            if (floors == null)
            {
                return null;
            }

            HideEditors(floors);
            var overlays = ResolveOverlays(spec, floors);
            if (overlays != null)
            {
                HideEditors(overlays);
            }

            var root = new GameObject(floors.transform.parent != null ? floors.transform.parent.name : "Painted Map");
            var grid = root.AddComponent<WorldGrid>();
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;
            var any = Stamp(grid, floors, true, ref minX, ref minY, ref maxX, ref maxY);
            if (overlays != null && overlays != floors)
            {
                any = Stamp(grid, overlays, false, ref minX, ref minY, ref maxX, ref maxY) || any;
            }

            if (!any)
            {
                Object.Destroy(root);
                return null;
            }

            grid.DressLooks();
            WorldSim.Ensure(grid);
            var roomBounds = new RectInt(minX, minY, maxX - minX + 1, maxY - minY + 1);
            var spawn = spec != null && spec.spawnPoint != null
                ? spec.spawnPoint.position
                : WorldGrid.Center(minX + 2, minY + roomBounds.height / 2);
            var name = spec != null && !string.IsNullOrEmpty(spec.roomName) ? spec.roomName : root.name;
            var room = new RoomInfo(name, name, roomBounds, spawn);
            return new SanctumBuild
            {
                Grid = grid,
                Spawn = spawn,
                Locks = System.Array.Empty<ISpellLock>(),
                Rooms = new[] { room },
                Charm = null
            };
        }

        static Tilemap ResolveOverlays(LevelAuthoring spec, Tilemap floors)
        {
            if (spec != null && spec.overlays != null)
            {
                return spec.overlays;
            }

            if (floors == null || floors.transform.parent == null)
            {
                return null;
            }

            var cover = floors.transform.parent.Find("Cover");
            return cover != null ? cover.GetComponent<Tilemap>() : null;
        }

        static bool Stamp(
            WorldGrid grid,
            Tilemap map,
            bool replace,
            ref int minX,
            ref int minY,
            ref int maxX,
            ref int maxY)
        {
            if (map == null)
            {
                return false;
            }

            var any = false;
            var bounds = map.cellBounds;
            for (var y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (var x = bounds.xMin; x < bounds.xMax; x++)
                {
                    var paint = map.GetTile<WorldPaintTile>(new Vector3Int(x, y, 0));
                    if (paint == null)
                    {
                        continue;
                    }

                    any = true;
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);

                    var tile = grid.Get(x, y);
                    if (tile == null || replace)
                    {
                        tile = grid.Set(x, y, paint.kind, paint.material);
                    }

                    ApplyAura(tile, paint.aura);
                    if (paint.cover != TileCover.None)
                    {
                        tile.PaintCover(paint.cover);
                    }
                }
            }

            return any;
        }

        static int CountPainted(Tilemap map)
        {
            if (map == null)
            {
                return 0;
            }

            var n = 0;
            foreach (var pos in map.cellBounds.allPositionsWithin)
            {
                if (map.GetTile<WorldPaintTile>(pos) != null)
                {
                    n++;
                }
            }

            return n;
        }

        static void HideEditors(Tilemap map)
        {
            var renderers = map.GetComponentsInChildren<TilemapRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }

        static void ApplyAura(WorldTile tile, TileAura aura)
        {
            if (tile == null)
            {
                return;
            }

            switch (aura)
            {
                case TileAura.Miasma:
                    tile.Foul(1f);
                    break;
                case TileAura.Fog:
                    tile.Cloak(1f);
                    break;
                case TileAura.Fire:
                    tile.Kindle();
                    break;
            }
        }
    }
}
