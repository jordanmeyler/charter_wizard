#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// Floor / wall palette stamps keep the tileset already on the
    /// cell. Cover-* / Aura-* sit on that same tileset. Pack art on
    /// Floor-Stone or Cover-Ice is a chip or sheen — it must not
    /// replace the look you painted.
    /// </summary>
    [InitializeOnLoad]
    static class StampLookKeep
    {
        struct CellLook
        {
            public Sprite Sprite;
            public TileKind Kind;
            public MaterialId Material;
            public bool HasPaint;
        }

        static readonly Dictionary<int, Dictionary<Vector3Int, CellLook>> Seen = new();
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
                cells = new Dictionary<Vector3Int, CellLook>();
                Seen[id] = cells;
            }

            var bounds = map.cellBounds;
            for (var y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (var x = bounds.xMin; x < bounds.xMax; x++)
                {
                    Remember(map, new Vector3Int(x, y, 0), cells, overwriteStamp: false);
                }
            }
        }

        static void Remember(
            Tilemap map,
            Vector3Int pos,
            Dictionary<Vector3Int, CellLook> cells,
            bool overwriteStamp)
        {
            var tile = map.GetTile(pos);
            if (tile is WorldPaintTile stamp && stamp.IsOverlayBrush)
            {
                return;
            }

            var sprite = map.GetSprite(pos);
            if (sprite == null && tile is Tile painted)
            {
                sprite = painted.sprite;
            }

            if (sprite == null)
            {
                return;
            }

            // First look on the cell wins. A later Floor or Cover stamp
            // must not teach the cache its pack preview.
            if (!overwriteStamp
                && tile is WorldPaintTile quality
                && quality.KeepsExistingLook
                && cells.ContainsKey(pos))
            {
                return;
            }

            var look = new CellLook { Sprite = sprite };
            if (tile is WorldPaintTile paint && !paint.IsOverlayBrush)
            {
                look.HasPaint = true;
                look.Kind = paint.kind;
                look.Material = paint.material;
            }

            cells[pos] = look;
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
                var pos = tiles[i].position;
                var paint = tiles[i].tile as WorldPaintTile;
                if (paint == null || !paint.KeepsExistingLook)
                {
                    if (Seen.TryGetValue(map.GetInstanceID(), out var cells))
                    {
                        Remember(map, pos, cells, overwriteStamp: true);
                    }

                    continue;
                }

                if (cover)
                {
                    if (paint.IsOverlayBrush || paint.sprite != null)
                    {
                        Replace(
                            map,
                            pos,
                            paint,
                            sprite: null,
                            stampCover: paint.ResolvedCover(),
                            kind: TileKind.None,
                            material: paint.material);
                    }

                    continue;
                }

                var prior = Cached(map, pos);
                if (paint.IsOverlayBrush)
                {
                    if (prior.Sprite == null)
                    {
                        continue;
                    }

                    Replace(
                        map,
                        pos,
                        paint,
                        prior.Sprite,
                        paint.ResolvedCover(),
                        prior.HasPaint ? prior.Kind : TileKind.Floor,
                        prior.HasPaint ? prior.Material : (MaterialId?)null);
                    continue;
                }

                if (prior.Sprite != null && paint.sprite != prior.Sprite)
                {
                    Replace(map, pos, paint, prior.Sprite, stampCover: null, kind: null, material: null);
                }
                else if (Seen.TryGetValue(map.GetInstanceID(), out var cells))
                {
                    Remember(map, pos, cells, overwriteStamp: false);
                }
            }
        }

        static CellLook Cached(Tilemap map, Vector3Int pos)
        {
            return Seen.TryGetValue(map.GetInstanceID(), out var cells) && cells.TryGetValue(pos, out var look)
                ? look
                : default;
        }

        static void Replace(
            Tilemap map,
            Vector3Int pos,
            WorldPaintTile stamp,
            Sprite sprite,
            TileCover? stampCover,
            TileKind? kind,
            MaterialId? material)
        {
            var authored = TilePropertyPaint.KeepLook(sprite, stamp, stampCover, kind, material);
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
