using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// Turns a scene Tilemap you painted in the editor into a live WorldGrid.
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
            map = map != null ? map : spec != null && spec.tilemap != null ? spec.tilemap : FindPaintedMap();
            if (map == null)
            {
                return null;
            }

            HideEditors(map);
            var root = new GameObject(map.transform.parent != null ? map.transform.parent.name : "Painted Map");
            var grid = root.AddComponent<WorldGrid>();
            var bounds = map.cellBounds;
            var any = false;
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;

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
                    var tile = grid.Set(x, y, paint.kind, paint.material);
                    ApplyAura(tile, paint.aura);
                }
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

        static void ApplyAura(WorldTile tile, string aura)
        {
            if (tile == null || string.IsNullOrEmpty(aura))
            {
                return;
            }

            switch (aura.Trim().ToLowerInvariant())
            {
                case "miasma":
                case "poison":
                    tile.Foul(1f);
                    break;
                case "fog":
                    tile.Cloak(1f);
                    break;
                case "fire":
                    tile.Kindle();
                    break;
            }
        }
    }
}
