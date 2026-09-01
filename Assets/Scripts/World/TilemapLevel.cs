using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// Turns scene Tilemaps you painted in the editor into a live WorldGrid.
    /// A cell is walkable floor only when a Floor brush or Kind = Floor
    /// stamp says so. Looks on any layer are not floor. Extra Floor /
    /// Tiles children merge — each Floor stamp still counts. Walls you
    /// never stamp stay walls on a Walls layer. Cover is overlay.
    /// Environment Details is a detail on that cell, and may also carry
    /// a Floor stamp if you want a second level to walk.
    /// </summary>
    public static class TilemapLevel
    {
        static readonly string[] WalkNames = { "tiles", "floor", "floors", "ground", "walk" };
        static readonly string[] WallNames = { "wall", "walls" };
        static readonly string[] CoverNames = { "cover", "covers", "covering", "coverings", "overlay", "overlays", "aura" };
        static readonly string[] DecorNames =
        {
            "environment details", "enviroment details", "environment", "enviroment",
            "decor", "decoration", "decorations", "prop", "props", "detail", "details"
        };

        public static Tilemap FindPaintedMap()
        {
            var maps = Object.FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
            Tilemap best = null;
            var count = 0;
            var bestWalk = false;
            for (var i = 0; i < maps.Length; i++)
            {
                if (IsDecor(maps[i]) || IsCover(maps[i]))
                {
                    continue;
                }

                var painted = CountPainted(maps[i]);
                var walk = IsWalk(maps[i]) || IsWall(maps[i]);
                if (painted > count || (painted == count && walk && !bestWalk))
                {
                    count = painted;
                    best = maps[i];
                    bestWalk = walk;
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

            var walks = WalkLayers(spec, floors);
            var covers = CoverLayers(spec, floors);
            var decors = DecorLayers(spec, floors);
            HideAll(walks);
            HideAll(covers);
            HideAll(decors);

            var root = new GameObject(floors.transform.parent != null ? floors.transform.parent.name : "Painted Map");
            var grid = root.AddComponent<WorldGrid>();
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;
            var any = false;
            for (var i = 0; i < walks.Count; i++)
            {
                var wallDefault = IsWall(walks[i]) ? TileKind.Wall : (TileKind?)null;
                any = Stamp(grid, walks[i], true, wallDefault, ref minX, ref minY, ref maxX, ref maxY) || any;
            }

            for (var i = 0; i < decors.Count; i++)
            {
                any = StampDetail(grid, decors[i], guessBlocks: true, ref minX, ref minY, ref maxX, ref maxY) || any;
            }

            for (var i = 0; i < covers.Count; i++)
            {
                any = Stamp(grid, covers[i], false, null, ref minX, ref minY, ref maxX, ref maxY, overlay: true) || any;
            }

            if (!any)
            {
                Object.Destroy(root);
                return null;
            }

            FillOpenVoid(grid, ref minX, ref minY, ref maxX, ref maxY);
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

        static List<Tilemap> WalkLayers(LevelAuthoring spec, Tilemap primary)
        {
            var list = new List<Tilemap>();
            AddUnique(list, spec != null ? spec.tilemap : null);
            AddUnique(list, spec != null ? spec.walls : null);
            if (IsWalk(primary) || IsWall(primary) || !HasNamedWalkSibling(primary))
            {
                AddUnique(list, primary);
            }

            CollectSiblings(primary, list, map => IsWalk(map) || IsWall(map));
            list.Sort(CompareLayers);
            return list;
        }

        static List<Tilemap> CoverLayers(LevelAuthoring spec, Tilemap primary)
        {
            var list = new List<Tilemap>();
            AddUnique(list, spec != null ? spec.overlays : null);
            CollectSiblings(primary, list, IsCover);
            return list;
        }

        static List<Tilemap> DecorLayers(LevelAuthoring spec, Tilemap primary)
        {
            var list = new List<Tilemap>();
            AddUnique(list, spec != null ? spec.decor : null);
            CollectSiblings(primary, list, IsLookLayer);
            list.Sort(CompareSorting);
            return list;
        }

        static bool IsLookLayer(Tilemap map)
        {
            return map != null && !IsWalk(map) && !IsWall(map) && !IsCover(map);
        }

        static bool HasNamedWalkSibling(Tilemap primary)
        {
            if (primary == null || primary.transform.parent == null)
            {
                return false;
            }

            var maps = primary.transform.parent.GetComponentsInChildren<Tilemap>(true);
            for (var i = 0; i < maps.Length; i++)
            {
                if (maps[i] != primary && (IsWalk(maps[i]) || IsWall(maps[i])))
                {
                    return true;
                }
            }

            return false;
        }

        static void CollectSiblings(Tilemap primary, List<Tilemap> list, System.Func<Tilemap, bool> match)
        {
            if (primary == null || primary.transform.parent == null)
            {
                return;
            }

            var maps = primary.transform.parent.GetComponentsInChildren<Tilemap>(true);
            for (var i = 0; i < maps.Length; i++)
            {
                if (match(maps[i]))
                {
                    AddUnique(list, maps[i]);
                }
            }
        }

        static void AddUnique(List<Tilemap> list, Tilemap map)
        {
            if (map != null && !list.Contains(map))
            {
                list.Add(map);
            }
        }

        static int WalkRank(Tilemap map)
        {
            return IsWall(map) ? 2 : 1;
        }

        static int CompareLayers(Tilemap a, Tilemap b)
        {
            var rank = WalkRank(a).CompareTo(WalkRank(b));
            return rank != 0 ? rank : CompareSorting(a, b);
        }

        static int CompareSorting(Tilemap a, Tilemap b)
        {
            return SortingOrder(a).CompareTo(SortingOrder(b));
        }

        static int SortingOrder(Tilemap map)
        {
            var renderer = map != null ? map.GetComponent<TilemapRenderer>() : null;
            return renderer != null ? renderer.sortingOrder : 0;
        }

        static void HideAll(List<Tilemap> maps)
        {
            for (var i = 0; i < maps.Count; i++)
            {
                HideEditors(maps[i]);
            }
        }

        static bool IsWalk(Tilemap map) => NameIs(map, WalkNames);

        static bool IsWall(Tilemap map) => NameIs(map, WallNames);

        static bool IsCover(Tilemap map) => NameIs(map, CoverNames);

        static bool IsDecor(Tilemap map) => NameIs(map, DecorNames);

        static bool NameIs(Tilemap map, string[] names)
        {
            if (map == null)
            {
                return false;
            }

            var n = map.gameObject.name.ToLowerInvariant();
            for (var i = 0; i < names.Length; i++)
            {
                if (n == names[i] || n.Contains(names[i]))
                {
                    return true;
                }
            }

            return false;
        }

        static bool Stamp(
            WorldGrid grid,
            Tilemap map,
            bool replace,
            TileKind? defaultKind,
            ref int minX,
            ref int minY,
            ref int maxX,
            ref int maxY,
            bool overlay = false)
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
                    var pos = new Vector3Int(x, y, 0);
                    var paint = map.GetTile<WorldPaintTile>(pos);
                    var raw = paint != null ? paint : map.GetTile(pos);
                    if (raw == null)
                    {
                        continue;
                    }

                    any = true;
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);

                    var look = LookOf(map, pos, paint, raw);
                    var kind = ResolveWalkKind(paint, raw, defaultKind);
                    var material = paint != null ? paint.material : GuessMaterial(raw);
                    var tile = grid.Get(x, y);
                    if (overlay)
                    {
                        if (tile == null)
                        {
                            continue;
                        }

                        var alpha = paint != null ? paint.ResolvedOpacity() : VeilOpacity(raw);
                        var sheen = CoverLookOf(map, pos, paint, raw);
                        if (sheen != null && (paint == null || paint.IsOverlayBrush || !paint.IsQualityStamp))
                        {
                            tile.AuthorCoverLook(sheen, alpha);
                        }

                        ApplyCoverWork(tile, ResolveCover(paint, raw, overlay: true), paint, raw, overlay: true);
                        continue;
                    }

                    if (paint != null && paint.IsOverlayBrush)
                    {
                        if (tile != null)
                        {
                            var sheen = CoverLookOf(map, pos, paint, raw);
                            if (sheen != null)
                            {
                                tile.AuthorCoverLook(sheen, paint.ResolvedOpacity());
                            }

                            ApplyCoverWork(tile, ResolveCover(paint, raw, overlay: true), paint, raw, overlay: true);
                        }

                        continue;
                    }

                    if (kind == null)
                    {
                        if (tile != null && look != null)
                        {
                            tile.AuthorLook(look);
                            if (paint != null)
                            {
                                ApplyCoverWork(tile, ResolveCover(paint, raw, overlay: false), paint);
                            }

                            continue;
                        }

                        ApplyLookOnly(grid, x, y, look, paint, raw, guessBlocks: false);
                        continue;
                    }

                    tile = ApplyWalkStamp(grid, tile, x, y, kind.Value, material, look, replace);

                    if (paint != null)
                    {
                        ApplyCoverWork(tile, ResolveCover(paint, raw, overlay: false), paint);
                    }
                }
            }

            return any;
        }

        /// <summary>
        /// Unpainted cells are the drop. Floor you never drew, holes
        /// you erased, and a rim past the painted island all become
        /// Pit / Void so walking off the ledge returns you.
        /// Painted walls and pillars keep their cells — they are how
        /// you cross.
        /// </summary>
        const int VoidMargin = 2;

        static void FillOpenVoid(
            WorldGrid grid,
            ref int minX,
            ref int minY,
            ref int maxX,
            ref int maxY)
        {
            if (grid == null)
            {
                return;
            }

            var x0 = minX - VoidMargin;
            var y0 = minY - VoidMargin;
            var x1 = maxX + VoidMargin;
            var y1 = maxY + VoidMargin;
            for (var y = y0; y <= y1; y++)
            {
                for (var x = x0; x <= x1; x++)
                {
                    if (grid.Get(x, y) != null)
                    {
                        continue;
                    }

                    grid.EnsureOpenPit(x, y);
                }
            }

            minX = x0;
            minY = y0;
            maxX = x1;
            maxY = y1;
        }

        /// <summary>
        /// Stamp kind / material onto a cell without discarding the
        /// tileset an earlier layer already authored. Floor-Stone pack
        /// art is a chip preview — it must not draw over that look.
        /// A later Walls layer still owns the wall face; the Tiles
        /// look becomes the cobble underlay, not the brick.
        /// </summary>
        static WorldTile ApplyWalkStamp(
            WorldGrid grid,
            WorldTile tile,
            int x,
            int y,
            TileKind kind,
            MaterialId material,
            Sprite look,
            bool replace)
        {
            var priorLook = tile != null ? tile.AuthoredLook : null;
            var underlay = KeepFloorUnder(kind, tile, out var underFloor);
            if (tile == null)
            {
                tile = grid.Set(x, y, kind, material);
            }
            else if (replace || tile.Kind != kind || tile.Material != material)
            {
                tile.AuthorKind(kind, material);
            }

            if (underlay != null)
            {
                tile.AuthorUnderlay(underlay, underFloor);
            }

            AuthorWalkLook(tile, kind, look, priorLook);
            return tile;
        }

        /// <summary>
        /// Floor stamps keep the tileset they sit on. Wall and door
        /// stamps keep a distinct wall face from this layer. Reusing
        /// the floor sprite as the brick hid every Walls-layer tile.
        /// </summary>
        static void AuthorWalkLook(WorldTile tile, TileKind kind, Sprite look, Sprite priorLook)
        {
            if (tile == null)
            {
                return;
            }

            if (kind == TileKind.Wall || kind == TileKind.Door)
            {
                tile.AuthorLook(look != null && look != priorLook ? look : null);
                return;
            }

            if (priorLook != null)
            {
                tile.AuthorLook(priorLook);
                return;
            }

            if (look != null)
            {
                tile.AuthorLook(look);
            }
        }

        static Sprite KeepFloorUnder(TileKind kind, WorldTile prior, out MaterialId floor)
        {
            floor = MaterialId.Stone;
            if (kind != TileKind.Wall && kind != TileKind.Door)
            {
                return null;
            }

            // Only keep a floor the author already placed. Do not invent
            // pack cobble under a wall or door they painted themselves.
            if (prior == null || (prior.Kind != TileKind.Floor && prior.Kind != TileKind.Bridge))
            {
                return null;
            }

            floor = WorldWork.IsIceBody(prior.Material) ? MaterialId.Stone : prior.Material;
            if (floor == MaterialId.None)
            {
                floor = MaterialId.Stone;
            }

            return prior.AuthoredLook;
        }

        static bool StampDetail(
            WorldGrid grid,
            Tilemap map,
            bool guessBlocks,
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
                    var pos = new Vector3Int(x, y, 0);
                    var paint = map.GetTile<WorldPaintTile>(pos);
                    var raw = paint != null ? paint : map.GetTile(pos);
                    if (raw == null)
                    {
                        continue;
                    }

                    any = true;
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);

                    var look = LookOf(map, pos, paint, raw);
                    var kind = ResolveWalkKind(paint, raw, null);
                    var material = paint != null
                        ? paint.material
                        : kind != null ? GuessMaterial(raw) : GuessDetailMaterial(raw);
                    if (paint != null && paint.IsOverlayBrush)
                    {
                        var existing = grid.Get(x, y);
                        if (existing != null)
                        {
                            var sheen = CoverLookOf(map, pos, paint, raw);
                            if (sheen != null)
                            {
                                existing.AuthorCoverLook(sheen, paint.ResolvedOpacity());
                            }

                            ApplyCoverWork(existing, ResolveCover(paint, raw, overlay: true), paint, raw, overlay: true);
                        }

                        continue;
                    }

                    if (kind != null)
                    {
                        var tile = grid.Get(x, y);
                        var replace = tile == null || tile.Kind == TileKind.Pit || kind == TileKind.Wall || kind == TileKind.Door;
                        tile = ApplyWalkStamp(grid, tile, x, y, kind.Value, material, look, replace);
                        if (paint != null)
                        {
                            ApplyCoverWork(tile, ResolveCover(paint, raw, overlay: false), paint);
                        }

                        continue;
                    }

                    ApplyLookOnly(grid, x, y, look, paint, raw, guessBlocks);
                }
            }

            return any;
        }

        /// <summary>
        /// Floor only when a Floor brush or Kind = Floor stamp says so.
        /// Name guesses may still mark wall / door / pit / bridge.
        /// A Walls layer still defaults unstamped cells to wall.
        /// </summary>
        static TileKind? ResolveWalkKind(WorldPaintTile paint, TileBase raw, TileKind? defaultKind)
        {
            if (paint != null)
            {
                return paint.StampsWalk ? paint.kind : (TileKind?)null;
            }

            var named = GuessNamedKind(raw);
            if (named != null)
            {
                return named;
            }

            return defaultKind;
        }

        static void ApplyLookOnly(
            WorldGrid grid,
            int x,
            int y,
            Sprite look,
            WorldPaintTile paint,
            TileBase raw,
            bool guessBlocks)
        {
            var tile = grid.Get(x, y) ?? grid.EnsureOpenPit(x, y);
            var material = paint != null ? paint.material : GuessDetailMaterial(raw);
            var blocks = paint != null
                ? paint.blocks
                : guessBlocks && GuessDetailBlocks(raw);
            if (tile.Kind == TileKind.Pit && tile.AuthoredLook == null && look != null && !blocks)
            {
                tile.AuthorLook(look);
            }
            else
            {
                tile.AuthorDetail(look, material, blocks);
            }

            if (paint != null)
            {
                ApplyCoverWork(tile, ResolveCover(paint, raw, overlay: false), paint);
            }
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
                if (map.GetTile(pos) != null)
                {
                    n++;
                }
            }

            return n;
        }

        public static TileKind GuessKindForEditor(TileBase tile) => GuessNamedKind(tile) ?? TileKind.None;

        public static MaterialId GuessMaterialForEditor(TileBase tile) => GuessMaterial(tile);

        static TileKind? GuessNamedKind(TileBase tile)
        {
            var name = tile != null ? tile.name : string.Empty;
            if (name.IndexOf("wall", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return TileKind.Wall;
            }

            if (name.IndexOf("door", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return TileKind.Door;
            }

            if (name.IndexOf("pit", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("hole", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("void", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return TileKind.Pit;
            }

            if (name.IndexOf("bridge", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return TileKind.Bridge;
            }

            return null;
        }

        static MaterialId GuessMaterial(TileBase tile)
        {
            var name = tile != null ? tile.name : string.Empty;
            if (name.IndexOf("water", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MaterialId.Water;
            }

            if (name.IndexOf("dirt", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("earth", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MaterialId.Dirt;
            }

            if (name.IndexOf("lava", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("hell", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MaterialId.Lava;
            }

            if (name.IndexOf("ice", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MaterialId.Ice;
            }

            if (name.IndexOf("oil", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MaterialId.Oil;
            }

            if (name.IndexOf("metal", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("iron", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("steel", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MaterialId.Metal;
            }

            if (name.IndexOf("plant", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("vine", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return MaterialId.Plant;
            }

            return MaterialId.Stone;
        }

        static TileCover ResolveCover(WorldPaintTile paint, TileBase raw, bool overlay)
        {
            if (paint != null)
            {
                var fromPaint = paint.ResolvedCover();
                if (fromPaint != TileCover.None)
                {
                    return fromPaint;
                }

                if (overlay)
                {
                    fromPaint = WorldPaintTile.CoverFromMaterial(paint.material);
                    if (fromPaint != TileCover.None)
                    {
                        return fromPaint;
                    }
                }

                // A stamped cell already said Cover = None. Do not guess
                // Water from "Floor_Water_…" — that overlay swaps the
                // painted pool for the atlas cover-water tile.
                return TileCover.None;
            }

            return GuessCover(raw);
        }

        static TileCover GuessCover(TileBase tile)
        {
            var name = tile != null ? tile.name : string.Empty;
            if (NameHas(name, "miasma", "gas"))
            {
                return TileCover.Miasma;
            }

            if (NameHas(name, "poison", "acid", "slick"))
            {
                return TileCover.Poison;
            }

            if (NameHas(name, "fog", "mist", "smoke", "haze"))
            {
                return TileCover.Fog;
            }

            if (NameHas(name, "ice", "frost", "glacier"))
            {
                return TileCover.Ice;
            }

            if (NameHas(name, "water", "wet"))
            {
                return TileCover.Water;
            }

            if (NameHas(name, "fire", "flame", "burn"))
            {
                return TileCover.Fire;
            }

            if (NameHas(name, "vine", "plant"))
            {
                return TileCover.Vine;
            }

            if (NameHas(name, "lightning", "spark"))
            {
                return TileCover.Lightning;
            }

            if (NameHas(name, "mud", "mire", "silt"))
            {
                return TileCover.Mud;
            }

            if (NameHas(name, "ash", "cinder"))
            {
                return TileCover.Ash;
            }

            return TileCover.None;
        }

        static float VeilOpacity(TileBase tile)
        {
            var cover = GuessCover(tile);
            if (cover == TileCover.Miasma || cover == TileCover.Fog)
            {
                return 0.42f;
            }

            if (cover == TileCover.Ice || cover == TileCover.Water)
            {
                return 0.7f;
            }

            return 1f;
        }

        static bool GuessDetailBlocks(TileBase tile)
        {
            var name = tile != null ? tile.name : string.Empty;
            return NameHas(name, "table", "statue", "crate", "barrel", "chest", "pillar",
                "shelf", "book", "desk", "cabinet", "bed", "stool", "urn", "fountain",
                "altar", "chair", "bench");
        }

        static MaterialId GuessDetailMaterial(TileBase tile)
        {
            var name = tile != null ? tile.name : string.Empty;
            if (NameHas(name, "plant", "grass", "bush", "moss", "vine", "fern", "flower", "leaf"))
            {
                return MaterialId.Plant;
            }

            if (NameHas(name, "table", "chair", "bench", "crate", "barrel", "chest",
                    "shelf", "book", "wood", "timber", "desk", "cabinet", "bed", "stool"))
            {
                return MaterialId.Timber;
            }

            return MaterialId.None;
        }

        static bool NameHas(string name, params string[] tokens)
        {
            for (var i = 0; i < tokens.Length; i++)
            {
                if (name.IndexOf(tokens[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        static Sprite LookOf(Tilemap map, Vector3Int pos, WorldPaintTile paint, TileBase raw)
        {
            // Cover-* / Aura-* pack art is a sheen. It must never become
            // the walk or wall sprite the author already placed.
            if (paint != null && paint.IsOverlayBrush)
            {
                return null;
            }

            if (paint != null && paint.sprite != null && !IsPaletteChip(paint))
            {
                return paint.sprite;
            }

            if (raw is Tile painted && painted.sprite != null && (paint == null || !paint.IsQualityStamp))
            {
                return painted.sprite;
            }

            if (map != null)
            {
                var shown = map.GetSprite(pos);
                if (shown != null && (paint == null || shown != paint.sprite || !IsPaletteChip(paint)))
                {
                    return shown;
                }
            }

            if (paint != null && !paint.KeepsPaintedLook)
            {
                return paint.PreviewSprite(pos.x, pos.y);
            }

            return paint != null && paint.IsQualityStamp ? null : SpriteOf(raw);
        }

        /// <summary>
        /// Shared Floor- / Wall- brushes carry pack preview art.
        /// Authored KeepLook tiles use Wall_Stone_… names and hold
        /// the tileset already painted on that cell.
        /// </summary>
        static bool IsPaletteChip(WorldPaintTile paint)
        {
            if (paint == null || !paint.IsQualityStamp)
            {
                return false;
            }

            var name = paint.name;
            return name.StartsWith("Floor-", System.StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Wall-", System.StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Cover-", System.StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Aura-", System.StringComparison.OrdinalIgnoreCase);
        }

        static Sprite CoverLookOf(Tilemap map, Vector3Int pos, WorldPaintTile paint, TileBase raw)
        {
            if (paint != null && paint.IsOverlayBrush && paint.sprite != null)
            {
                return paint.sprite;
            }

            if (paint != null && paint.IsQualityStamp)
            {
                return null;
            }

            if (raw is Tile painted && painted.sprite != null)
            {
                return painted.sprite;
            }

            return map != null ? map.GetSprite(pos) : null;
        }

        static Sprite SpriteOf(TileBase tile)
        {
            return tile is Tile painted ? painted.sprite : null;
        }

        static void HideEditors(Tilemap map)
        {
            var renderers = map.GetComponentsInChildren<TilemapRenderer>(true);
            for (var i = 0; i < renderers.Length; i++)
            {
                renderers[i].enabled = false;
            }
        }

        static void ApplyCoverWork(
            WorldTile tile,
            TileCover cover,
            WorldPaintTile paint = null,
            TileBase raw = null,
            bool overlay = false)
        {
            if (tile == null)
            {
                return;
            }

            if (cover != TileCover.None)
            {
                tile.PaintCover(cover);
                switch (cover)
                {
                    case TileCover.Miasma:
                        tile.Foul(1f);
                        break;
                    case TileCover.Poison:
                        tile.SlickPoison();
                        break;
                    case TileCover.Fog:
                        tile.Cloak(1f);
                        break;
                }
            }
            else if (overlay)
            {
                var material = paint != null ? paint.material : GuessMaterial(raw);
                if (CoverCatalog.IsOverlayMaterial(material))
                {
                    tile.AuthorCoverMaterial(material);
                }
            }

            if (paint != null && paint.aura == TileAura.Fire)
            {
                tile.Kindle();
            }
        }
    }
}
