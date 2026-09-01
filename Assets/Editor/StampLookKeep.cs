#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// Floor / wall palette stamps keep the tileset already on the
    /// cell. Pack art on Floor-Stone / Floor-Plant is only a chip
    /// preview — it must not replace the look you painted.
    /// </summary>
    [InitializeOnLoad]
    static class StampLookKeep
    {
        static readonly Dictionary<int, Dictionary<Vector3Int, Sprite>> Seen = new();
        static bool _writing;
        static double _nextCache;

        static StampLookKeep()
        {
            SceneView.duringSceneGui += OnScene;
            Tilemap.tilemapTileChanged += OnTilesChanged;
            EditorApplication.delayCall += WarmCache;
        }

        static void OnScene(SceneView _)
        {
            if (EditorApplication.timeSinceStartup < _nextCache)
            {
                return;
            }

            _nextCache = EditorApplication.timeSinceStartup + 0.2d;
            WarmCache();
        }

        static void WarmCache()
        {
            if (_writing)
            {
                return;
            }

            var maps = Object.FindObjectsByType<Tilemap>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (var i = 0; i < maps.Length; i++)
            {
                CacheMap(maps[i]);
            }
        }

        static void CacheMap(Tilemap map)
        {
            if (map == null)
            {
                return;
            }

            var id = map.GetInstanceID();
            if (!Seen.TryGetValue(id, out var cells))
            {
                cells = new Dictionary<Vector3Int, Sprite>();
                Seen[id] = cells;
            }

            var bounds = map.cellBounds;
            for (var y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (var x = bounds.xMin; x < bounds.xMax; x++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    var tile = map.GetTile(pos);
                    var sprite = map.GetSprite(pos);
                    if (sprite == null && tile is Tile painted)
                    {
                        sprite = painted.sprite;
                    }

                    if (sprite == null)
                    {
                        continue;
                    }

                    // First look on the cell wins. A later Floor stamp
                    // must not teach the cache its pack preview.
                    if (tile is WorldPaintTile paint && paint.IsQualityStamp && cells.ContainsKey(pos))
                    {
                        continue;
                    }

                    cells[pos] = sprite;
                }
            }
        }

        static void OnTilesChanged(Tilemap map, Tilemap.SyncTile[] tiles)
        {
            if (_writing || map == null || tiles == null)
            {
                return;
            }

            var cover = IsCoverMap(map);
            for (var i = 0; i < tiles.Length; i++)
            {
                var paint = tiles[i].tile as WorldPaintTile;
                if (paint == null || !paint.IsQualityStamp)
                {
                    continue;
                }

                var pos = tiles[i].position;
                if (cover)
                {
                    if (paint.sprite != null)
                    {
                        Replace(map, pos, paint, null, paint.ResolvedCover());
                    }

                    continue;
                }

                var keep = CachedSprite(map, pos);
                if (keep != null && paint.sprite != keep)
                {
                    Replace(map, pos, paint, keep, stampCover: null);
                }
            }
        }

        static Sprite CachedSprite(Tilemap map, Vector3Int pos)
        {
            return Seen.TryGetValue(map.GetInstanceID(), out var cells) && cells.TryGetValue(pos, out var sprite)
                ? sprite
                : null;
        }

        static void Replace(
            Tilemap map,
            Vector3Int pos,
            WorldPaintTile stamp,
            Sprite sprite,
            TileCover? stampCover)
        {
            var authored = TilePropertyPaint.KeepLook(sprite, stamp, stampCover);
            if (authored == null || authored == map.GetTile(pos))
            {
                return;
            }

            _writing = true;
            try
            {
                Undo.RecordObject(map, "Keep stamped tileset");
                map.SetTile(pos, authored);
                EditorUtility.SetDirty(map);
            }
            finally
            {
                _writing = false;
            }
        }

        static bool IsCoverMap(Tilemap map)
        {
            var name = map != null ? map.gameObject.name.ToLowerInvariant() : string.Empty;
            return name.Contains("cover") || name.Contains("overlay") || name.Contains("veil");
        }
    }
}
#endif
