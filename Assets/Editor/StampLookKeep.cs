#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// Plant / timber / water / fire palette stamps keep the tileset
    /// already on the cell. Pack art on Floor-Plant / Floor-Fire is
    /// only a chip preview — it must not replace the look you painted.
    /// </summary>
    [InitializeOnLoad]
    static class StampLookKeep
    {
        sealed class SeenCell
        {
            public Sprite sprite;
            public TileKind kind;
            public MaterialId material;
        }

        static readonly Dictionary<int, Dictionary<Vector3Int, SeenCell>> Seen = new();
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
                cells = new Dictionary<Vector3Int, SeenCell>();
                Seen[id] = cells;
            }

            var bounds = map.cellBounds;
            for (var y = bounds.yMin; y < bounds.yMax; y++)
            {
                for (var x = bounds.xMin; x < bounds.xMax; x++)
                {
                    var pos = new Vector3Int(x, y, 0);
                    var tile = map.GetTile(pos);
                    if (tile is WorldPaintTile quality && quality.IsQualityStamp)
                    {
                        continue;
                    }

                    var sprite = map.GetSprite(pos);
                    if (sprite == null && tile is Tile painted)
                    {
                        sprite = painted.sprite;
                    }

                    if (sprite == null && tile == null)
                    {
                        continue;
                    }

                    var prior = tile as WorldPaintTile;
                    cells[pos] = new SeenCell
                    {
                        sprite = sprite,
                        kind = prior != null ? prior.kind : TilemapLevel.GuessKindForEditor(tile),
                        material = prior != null ? prior.material : TilemapLevel.GuessMaterialForEditor(tile)
                    };
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
                var fire = paint.ResolvedCover() == TileCover.Fire;
                if (cover)
                {
                    if (paint.sprite != null)
                    {
                        Replace(
                            map,
                            pos,
                            paint,
                            null,
                            fire ? TileCover.Fire : TileCover.None);
                    }

                    continue;
                }

                var keep = Cached(map, pos);
                if (keep != null && keep.sprite != null && paint.sprite != keep.sprite)
                {
                    Replace(
                        map,
                        pos,
                        paint,
                        keep.sprite,
                        fire ? TileCover.Fire : (TileCover?)null,
                        paint.kind,
                        fire ? keep.material : (MaterialId?)null);
                }
            }
        }

        static SeenCell Cached(Tilemap map, Vector3Int pos)
        {
            return Seen.TryGetValue(map.GetInstanceID(), out var cells) && cells.TryGetValue(pos, out var cell)
                ? cell
                : null;
        }

        static void Replace(
            Tilemap map,
            Vector3Int pos,
            WorldPaintTile stamp,
            Sprite sprite,
            TileCover? stampCover,
            TileKind? kind = null,
            MaterialId? material = null)
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
