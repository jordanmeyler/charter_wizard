using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Terrain verbs that follow a finished cast. Kept small on purpose:
    /// a pillar is one tile, a wall is a start-to-stop line, hop and flight
    /// stay on the caster. Stood bodies linger until another element unmakes them.
    /// </summary>
    public static class WorldWork
    {
        public const int MaxWallLength = 10;
        // A span over a hollow is two tiles wide. Basic earth and ice
        // must find floor or wall at each end, or they fall. Metal
        // hangs without a far bank. MaxWallLength stays the hard cap.
        public const int HopTiles = 4;
        public const float FlightSeconds = 10f;
        public const float TimeStopSeconds = 8f;
        public const int VeilRadius = 2;

        public static bool IsHop(SpellId spell) =>
            spell == SpellId.Hop;

        public static bool IsFlight(SpellId spell) =>
            spell == SpellId.Flight;

        public static bool IsPush(SpellId spell) =>
            spell == SpellId.Push || spell == SpellId.Gale;

        public static bool IsSkyStrike(SpellId spell) =>
            spell == SpellId.LightningStrike;

        public static bool StopsOnWalls(SpellId spell)
        {
            if (spell == SpellId.None || IsSkyStrike(spell) || IsHop(spell) || IsFlight(spell))
            {
                return false;
            }

            if (SpellCodex.TryGet(spell, out var entry) && entry.Shape != SpellShape.None)
            {
                return entry.Shape == SpellShape.Shot;
            }

            switch (spell)
            {
                case SpellId.Fireball:
                case SpellId.SunLance:
                case SpellId.Drive:
                case SpellId.WaterJet:
                case SpellId.Douse:
                case SpellId.IceSpear:
                case SpellId.LightningBolt:
                case SpellId.BrilliantArc:
                case SpellId.Blackout:
                case SpellId.HurledStone:
                case SpellId.DirtToss:
                case SpellId.Gust:
                case SpellId.Push:
                case SpellId.Gale:
                case SpellId.Scald:
                case SpellId.ScatterDust:
                case SpellId.OilShot:
                case SpellId.Poison:
                case SpellId.Plasma:
                case SpellId.Vine:
                    return true;
                default:
                    return false;
            }
        }

        public static bool BlocksTravel(WorldTile tile) =>
            tile != null && (tile.BlocksTravel || WorldDoor.BlocksCell(tile.Coord));

        public static bool BlocksCell(Vector2Int cell, WorldTile tile = null) =>
            (tile != null && tile.BlocksTravel) || WorldDoor.BlocksCell(cell);

        public static bool IsTimeStop(SpellId spell) =>
            spell == SpellId.TimeStop;

        public static bool NeedsSpan(SpellId spell) =>
            spell == SpellId.Wall
            || spell == SpellId.IceWall
            || spell == SpellId.MetalWall
            || spell == SpellId.ObsidianWall
            || spell == SpellId.WoodWall;

        public static bool IsPillar(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.FlamePillar:
                case SpellId.IcePillar:
                case SpellId.IceWall:
                case SpellId.Wall:
                case SpellId.MetalWall:
                case SpellId.ObsidianWall:
                case SpellId.WoodWall:
                case SpellId.VineRise:
                case SpellId.StonePillar:
                case SpellId.EarthPillar:
                case SpellId.MetalPillar:
                case SpellId.Tree:
                case SpellId.Menhir:
                case SpellId.LavaPillar:
                case SpellId.WaterPillar:
                case SpellId.OilPillar:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSinglePillar(SpellId spell) =>
            IsPillar(spell) && !NeedsSpan(spell);

        public static bool FillsGaps(SpellId spell)
        {
            return IsPillar(spell) || spell == SpellId.Bridge || spell == SpellId.ObsidianPath;
        }

        public static bool DriesWater(SpellId spell) =>
            MatterLaw.HeatOf(spell) >= Heat.Fire;

        public static bool FreezesWater(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.IcePillar:
                case SpellId.IceWall:
                case SpellId.IceSpear:
                case SpellId.Snowfall:
                case SpellId.GraveIce:
                case SpellId.Blizzard:
                    return true;
                default:
                    return false;
            }
        }

        public static bool RaisesBarrier(SpellId spell) =>
            IsPillar(spell);

        public static bool LeavesGapsWhenCrossing(SpellId spell) =>
            IsHop(spell) || IsFlight(spell);

        public static bool IsSightVeil(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Fog:
                case SpellId.Gloom:
                case SpellId.Veil:
                case SpellId.Darkness:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsPoisonVeil(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Blight:
                case SpellId.GraveDust:
                case SpellId.Miasma:
                case SpellId.Poison:
                    return true;
                default:
                    return false;
            }
        }

        public static bool LaysVeil(SpellId spell) =>
            IsSightVeil(spell) || IsPoisonVeil(spell);

        public static VeilKind VeilKindOf(SpellId spell)
        {
            if (IsPoisonVeil(spell))
            {
                return VeilKind.Poison;
            }

            if (spell == SpellId.Darkness)
            {
                return VeilKind.Darkness;
            }

            return IsSightVeil(spell) ? VeilKind.Fog : VeilKind.None;
        }

        public static bool IsWaterWork(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.WaterJet:
                case SpellId.Flood:
                case SpellId.Monsoon:
                case SpellId.Rain:
                case SpellId.Scald:
                case SpellId.Spring:
                case SpellId.Douse:
                case SpellId.Swamp:
                case SpellId.WaterPillar:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFireWork(SpellId spell) =>
            MatterLaw.HeatOf(spell) >= Heat.Fire;

        public static bool IsOilWork(SpellId spell) =>
            spell == SpellId.OilShot;

        public static bool IsVineWork(SpellId spell) =>
            spell == SpellId.Vine;

        public static bool IsPlasmaWork(SpellId spell) =>
            MatterLaw.IsPlasmaWork(spell);

        public static bool IsAirWork(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Gale:
                case SpellId.Gust:
                case SpellId.Push:
                case SpellId.StormCall:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsLightWork(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.SunLance:
                case SpellId.DayWake:
                case SpellId.BrilliantArc:
                    return true;
                default:
                    return false;
            }
        }

        public static bool ClearsVeils(SpellId spell) =>
            IsAirWork(spell) || IsFireWork(spell) || IsLightWork(spell);

        public static bool ClearsVeil(SpellId spell, VeilKind kind)
        {
            if (kind == VeilKind.None)
            {
                return false;
            }

            if (IsAirWork(spell) || IsFireWork(spell))
            {
                return true;
            }

            return (kind == VeilKind.Fog || kind == VeilKind.Darkness) && IsLightWork(spell);
        }

        public static bool IsBasicEarth(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Stone:
                case MaterialId.SaltCrust:
                case MaterialId.Scoured:
                case MaterialId.Damp:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsIceBody(MaterialId material) =>
            material == MaterialId.Ice || material == MaterialId.Snow || material == MaterialId.Glacier;

        public static bool IsFlameBody(MaterialId material) =>
            material == MaterialId.Hearth || material == MaterialId.Ember;

        public static bool IsLavaBody(MaterialId material) =>
            material == MaterialId.Lava;

        public static bool BurnsOccupants(WorldTile tile)
        {
            if (tile == null || !tile.IsConjured)
            {
                return false;
            }

            if (!IsFlameBody(tile.Material) && !IsLavaBody(tile.Material))
            {
                return false;
            }

            return tile.Kind == TileKind.Wall || tile.RaisedAs == RaisedForm.Pillar;
        }

        public static bool IsRockBody(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Stone:
                case MaterialId.SaltCrust:
                case MaterialId.Scoured:
                case MaterialId.Crystal:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsSolidMatter(MaterialId material)
        {
            return IsRockBody(material)
                || IsBasicEarth(material)
                || IsIceBody(material)
                || IsPlantBody(material);
        }

        public static bool IsShatterWork(SpellId spell) =>
            spell == SpellId.Shatter;

        public static bool IsBoulderWork(SpellId spell) =>
            spell == SpellId.HurledStone;

        public static bool IsPlantBody(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Grove:
                case MaterialId.Plant:
                case MaterialId.Timber:
                case MaterialId.Moss:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// A stood body only yields to an opposed element. Water melts a
        /// basic earth wall. Heat thaws ice — witchfire takes glacier.
        /// Melt bores stone and steel, even room masonry. Water ends a
        /// flame. Fire eats vine. A boulder or Shatter breaks rock.
        /// Obsidian will not take the work.
        /// </summary>
        public static bool Unmakes(SpellId spell, WorldTile tile)
        {
            if (tile == null || MatterLaw.ResistsMagic(tile.Material))
            {
                return false;
            }

            if (IsPlasmaWork(spell) && MatterLaw.IsAnnihilable(tile.Material))
            {
                return true;
            }

            if (!tile.IsConjured)
            {
                return false;
            }

            var material = tile.Material;
            if (IsWaterWork(spell) && IsBasicEarth(material))
            {
                return true;
            }

            if (IsIceBody(material) && MatterLaw.Melts(spell, material))
            {
                return true;
            }

            if ((IsWaterWork(spell) || spell == SpellId.Smother || spell == SpellId.Snuff)
                && IsFlameBody(material))
            {
                return true;
            }

            if (IsFireWork(spell) && IsPlantBody(material))
            {
                return true;
            }

            if (IsShatterWork(spell) && IsSolidMatter(material))
            {
                return true;
            }

            if (IsBoulderWork(spell) && IsRockBody(material))
            {
                return true;
            }

            return false;
        }

        public static bool QuenchesLava(SpellId spell, WorldTile tile)
        {
            return tile != null
                && tile.IsConjured
                && IsWaterWork(spell)
                && IsLavaBody(tile.Material);
        }

        public static string UnmakeNote(SpellId spell, WorldTile tile)
        {
            if (tile == null)
            {
                return string.Empty;
            }

            if (IsWaterWork(spell) && IsBasicEarth(tile.Material))
            {
                return tile.RaisedAs == RaisedForm.Pillar
                    ? "Water takes the earth column. Rest yields."
                    : "Water melts the earth wall. Rest yields.";
            }

            if (IsIceBody(tile.Material) || tile.Material == MaterialId.Glass)
            {
                return MatterLaw.MeltNote(tile.Material);
            }

            if (IsFlameBody(tile.Material))
            {
                return "Water ends the standing flame.";
            }

            if (IsPlantBody(tile.Material))
            {
                return "Hunger eats the vine. The column falls.";
            }

            if (IsShatterWork(spell))
            {
                return "The stood matter comes apart.";
            }

            if (IsBoulderWork(spell))
            {
                return "The hurled rest shatters the rock.";
            }

            return "The stood body comes apart.";
        }

        public static MaterialId MaterialFor(RuneId element, SpellId spell)
        {
            if (spell == SpellId.ObsidianPath || spell == SpellId.ObsidianWall)
            {
                return MaterialId.Obsidian;
            }

            if (spell == SpellId.IcePillar || spell == SpellId.IceWall)
            {
                return MaterialId.Ice;
            }

            if (spell == SpellId.MetalPillar || spell == SpellId.MetalWall)
            {
                return MaterialId.Metal;
            }

            if (spell == SpellId.FlamePillar)
            {
                return MaterialId.Hearth;
            }

            if (spell == SpellId.LavaPillar)
            {
                return MaterialId.Lava;
            }

            if (spell == SpellId.VineRise)
            {
                return MaterialId.Grove;
            }

            if (spell == SpellId.Tree || spell == SpellId.WoodWall)
            {
                return MaterialId.Timber;
            }

            if (spell == SpellId.WaterPillar)
            {
                return MaterialId.Water;
            }

            if (spell == SpellId.OilPillar)
            {
                return MaterialId.Oil;
            }

            var fromElement = MaterialCatalog.FromElement(element);
            return fromElement == MaterialId.None ? MaterialId.Stone : fromElement;
        }

        public static List<Vector2Int> Span(Vector2Int start, Vector2Int stop, int maxLength = MaxWallLength)
        {
            var cells = new List<Vector2Int>();
            var dx = Mathf.Abs(stop.x - start.x);
            var dy = Mathf.Abs(stop.y - start.y);
            var sx = start.x < stop.x ? 1 : -1;
            var sy = start.y < stop.y ? 1 : -1;
            var error = dx - dy;
            var cursor = start;
            var cap = Mathf.Max(1, maxLength);

            while (cells.Count < cap)
            {
                cells.Add(cursor);
                if (cursor == stop)
                {
                    break;
                }

                var doubled = error * 2;
                if (doubled > -dy)
                {
                    error -= dy;
                    cursor.x += sx;
                }

                if (doubled < dx)
                {
                    error += dx;
                    cursor.y += sy;
                }
            }

            return cells;
        }

        public static List<Vector2Int> Disk(Vector2Int center, int radius)
        {
            var cells = new List<Vector2Int>();
            var reach = Mathf.Max(0, radius);
            for (var y = -reach; y <= reach; y++)
            {
                for (var x = -reach; x <= reach; x++)
                {
                    if (x * x + y * y <= reach * reach)
                    {
                        cells.Add(new Vector2Int(center.x + x, center.y + y));
                    }
                }
            }

            return cells;
        }

        public static Vector2Int CoordOf(Vector3 world)
        {
            return new Vector2Int(Mathf.FloorToInt(world.x), Mathf.FloorToInt(world.y));
        }

        public static string Apply(
            WorldGrid grid,
            SpellId spell,
            RuneId element,
            Vector3 origin,
            Vector3 from,
            Vector3 to,
            SpellShape shape = SpellShape.None)
        {
            if (grid == null || spell == SpellId.None)
            {
                return string.Empty;
            }

            if (IsHop(spell) || IsFlight(spell))
            {
                return string.Empty;
            }

            if (IsTimeStop(spell))
            {
                return "The instant stands. Motion leaves the living; the mind cannot hurry.";
            }

            var notes = new List<string>();
            var sweep = WorldPhysics.Build(grid, spell, shape, origin, from, to);
            var cells = sweep.Cells.Count > 0 ? sweep.Cells : WorkCells(spell, origin, from, to);
            var quenchNote = QuenchAlong(grid, spell, cells, out var quenched);
            if (quenched > 0)
            {
                notes.Add(quenchNote);
            }

            var unmakeNote = UnmakeAlong(grid, spell, cells, out var undone);
            if (undone > 0)
            {
                notes.Add(unmakeNote);
            }

            var blown = WorldPhysics.Apply(grid, sweep);
            if (!string.IsNullOrEmpty(blown))
            {
                notes.Add(blown);
            }

            if (LaysVeil(spell))
            {
                var kind = VeilKindOf(spell);
                VeilField.Lay(grid, kind, origin, VeilRadius);
                notes.Add(kind == VeilKind.Poison
                    ? "A sick mist stands. Breath is unkind."
                    : "The hanging veil is given a body. The room is lost.");
            }

            if (FillsGaps(spell) || RaisesBarrier(spell) || DriesWater(spell) || FreezesWater(spell))
            {
                var built = RaiseBodies(grid, spell, element, origin, from, to);
                if (!string.IsNullOrEmpty(built))
                {
                    notes.Add(built);
                }
            }

            if (FreezesWater(spell))
            {
                var frozen = FreezeWaterAlong(grid, cells);
                if (frozen > 0)
                {
                    notes.Add(frozen == 1
                        ? "Yield given a body. That water is ice."
                        : "Hard water stands. The pool will hold you.");
                }
            }

            if (IsWaterWork(spell))
            {
                var filled = FillSmallPits(grid, cells);
                if (filled > 0)
                {
                    notes.Add(filled == 1
                        ? "Yield takes a small hollow. It will drown you until ice gives it a body."
                        : "Yield fills the small hollows. The water has no floor. Freeze it, or fall.");
                }
            }

            if (spell == SpellId.Swamp)
            {
                var slicked = SlickMud(grid, cells);
                if (slicked > 0)
                {
                    notes.Add("Yield meeting rest. A watery swamp stands from your feet.");
                }
            }

            if (spell == SpellId.DirtToss)
            {
                var tossed = LayDirt(grid, cells);
                if (tossed > 0)
                {
                    notes.Add("Loose rest lands. Ground-fire dies. Earth speaks here.");
                }
            }

            if (IsOilWork(spell))
            {
                var oiled = SlickOil(grid, cells);
                if (oiled > 0)
                {
                    notes.Add("Fuel finds the floor. Hunger will hold here.");
                }
            }

            if (IsVineWork(spell))
            {
                var climbed = LayVine(grid, cells);
                VineStrand.Lay(grid, origin, to);
                if (climbed > 0)
                {
                    notes.Add("The vegetable body climbs. Hunger can run this line as a wick.");
                }
            }

            return FirstFilled(notes);
        }

        public const int SmallPitSpan = 4;

        /// <summary>
        /// Water work fills a connected pit smaller than 4×4. The pool
        /// drowns until ice asks that water to stand.
        /// </summary>
        public static int FillSmallPits(WorldGrid grid, List<Vector2Int> seeds)
        {
            if (grid == null || seeds == null || seeds.Count == 0)
            {
                return 0;
            }

            var seen = new HashSet<Vector2Int>();
            var filled = 0;
            for (var i = 0; i < seeds.Count; i++)
            {
                var start = seeds[i];
                if (!seen.Add(start))
                {
                    continue;
                }

                var tile = grid.Get(start);
                if (tile == null || tile.Kind != TileKind.Pit)
                {
                    continue;
                }

                var cluster = FloodPits(grid, start, seen);
                if (!IsSmallPit(cluster))
                {
                    continue;
                }

                for (var c = 0; c < cluster.Count; c++)
                {
                    var pit = grid.Get(cluster[c]);
                    if (pit == null || pit.Kind != TileKind.Pit)
                    {
                        continue;
                    }

                    pit.BecomeWater();
                    filled++;
                }
            }

            return filled;
        }

        public static int SlickMud(WorldGrid grid, List<Vector2Int> cells)
        {
            if (grid == null || cells == null || cells.Count == 0)
            {
                return 0;
            }

            var changed = 0;
            for (var i = 0; i < cells.Count; i++)
            {
                var tile = grid.Get(cells[i]);
                if (tile != null && tile.SlickMud())
                {
                    changed++;
                }
            }

            return changed;
        }

        public static int LayDirt(WorldGrid grid, List<Vector2Int> cells)
        {
            if (grid == null || cells == null || cells.Count == 0)
            {
                return 0;
            }

            var changed = 0;
            for (var i = 0; i < cells.Count; i++)
            {
                var tile = grid.Get(cells[i]);
                if (tile != null && tile.LayDirt())
                {
                    changed++;
                }
            }

            return changed;
        }

        public static int SlickOil(WorldGrid grid, List<Vector2Int> cells)
        {
            if (grid == null || cells == null || cells.Count == 0)
            {
                return 0;
            }

            var changed = 0;
            for (var i = 0; i < cells.Count; i++)
            {
                var tile = grid.Get(cells[i]);
                if (tile != null && tile.SlickOil())
                {
                    changed++;
                }
            }

            return changed;
        }

        public static int LayVine(WorldGrid grid, List<Vector2Int> cells)
        {
            if (grid == null || cells == null || cells.Count == 0)
            {
                return 0;
            }

            var changed = 0;
            for (var i = 0; i < cells.Count; i++)
            {
                var tile = grid.Get(cells[i]);
                if (tile != null && tile.LayVine())
                {
                    changed++;
                }
            }

            return changed;
        }

        static List<Vector2Int> Merge(List<Vector2Int> left, List<Vector2Int> right)
        {
            var cells = left ?? new List<Vector2Int>();
            if (right == null || right.Count == 0)
            {
                return cells;
            }

            for (var i = 0; i < right.Count; i++)
            {
                if (!cells.Contains(right[i]))
                {
                    cells.Add(right[i]);
                }
            }

            return cells;
        }

        public static int GrowForest(WorldGrid grid, List<Vector2Int> cells)
        {
            if (grid == null || cells == null || cells.Count == 0)
            {
                return 0;
            }

            var changed = 0;
            for (var i = 0; i < cells.Count; i++)
            {
                var tile = grid.Get(cells[i]);
                if (tile == null)
                {
                    continue;
                }

                if (tile.CanTakePlant)
                {
                    tile.PlantHere();
                    changed++;
                }
                else if (tile.IsPlantish)
                {
                    tile.Grow(2);
                    grid.SpreadPlant(tile);
                    changed++;
                }
            }

            return changed;
        }

        public static int FreezeWaterAlong(WorldGrid grid, List<Vector2Int> cells)
        {
            if (grid == null || cells == null || cells.Count == 0)
            {
                return 0;
            }

            var frozen = 0;
            for (var i = 0; i < cells.Count; i++)
            {
                var tile = grid.Get(cells[i]);
                if (tile != null && tile.FreezeSolid())
                {
                    frozen++;
                }
            }

            return frozen;
        }

        static List<Vector2Int> FloodPits(WorldGrid grid, Vector2Int start, HashSet<Vector2Int> seen)
        {
            var cluster = new List<Vector2Int>();
            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            seen.Add(start);
            while (queue.Count > 0)
            {
                var cell = queue.Dequeue();
                var tile = grid.Get(cell);
                if (tile == null || tile.Kind != TileKind.Pit)
                {
                    continue;
                }

                cluster.Add(cell);
                TryEnqueuePit(grid, seen, queue, cell.x + 1, cell.y);
                TryEnqueuePit(grid, seen, queue, cell.x - 1, cell.y);
                TryEnqueuePit(grid, seen, queue, cell.x, cell.y + 1);
                TryEnqueuePit(grid, seen, queue, cell.x, cell.y - 1);
            }

            return cluster;
        }

        static void TryEnqueuePit(WorldGrid grid, HashSet<Vector2Int> seen, Queue<Vector2Int> queue, int x, int y)
        {
            var cell = new Vector2Int(x, y);
            if (!seen.Add(cell))
            {
                return;
            }

            var tile = grid.Get(cell);
            if (tile != null && tile.Kind == TileKind.Pit)
            {
                queue.Enqueue(cell);
            }
        }

        static bool IsSmallPit(List<Vector2Int> cluster)
        {
            if (cluster == null || cluster.Count == 0)
            {
                return false;
            }

            var minX = cluster[0].x;
            var maxX = cluster[0].x;
            var minY = cluster[0].y;
            var maxY = cluster[0].y;
            for (var i = 1; i < cluster.Count; i++)
            {
                minX = Mathf.Min(minX, cluster[i].x);
                maxX = Mathf.Max(maxX, cluster[i].x);
                minY = Mathf.Min(minY, cluster[i].y);
                maxY = Mathf.Max(maxY, cluster[i].y);
            }

            return (maxX - minX + 1) < SmallPitSpan && (maxY - minY + 1) < SmallPitSpan;
        }

        public static bool IsSpreadWork(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Fog:
                case SpellId.Gloom:
                case SpellId.Veil:
                case SpellId.Blight:
                case SpellId.GraveDust:
                case SpellId.Flood:
                case SpellId.Swamp:
                case SpellId.LiveFloor:
                case SpellId.Quagmire:
                case SpellId.Sprout:
                case SpellId.Grove:
                case SpellId.Darkness:
                case SpellId.Miasma:
                case SpellId.Monsoon:
                case SpellId.Thunderclap:
                case SpellId.DayWake:
                    return true;
                default:
                    return false;
            }
        }

        static List<Vector2Int> WorkCells(SpellId spell, Vector3 origin, Vector3 from, Vector3 to)
        {
            if (NeedsSpan(spell))
            {
                return Span(CoordOf(from), CoordOf(to));
            }

            if (IsSpreadWork(spell))
            {
                return Disk(CoordOf(origin), VeilRadius);
            }

            if (spell == SpellId.WaterJet || spell == SpellId.Fireball || spell == SpellId.Gale
                || spell == SpellId.Gust || spell == SpellId.Push || spell == SpellId.Scald
                || spell == SpellId.SunLance || spell == SpellId.HurledStone || spell == SpellId.Douse
                || spell == SpellId.IceSpear || spell == SpellId.LightningBolt
                || spell == SpellId.BrilliantArc || spell == SpellId.Blackout
                || spell == SpellId.Vine)
            {
                return Span(CoordOf(from), CoordOf(to));
            }

            if (spell == SpellId.DirtToss)
            {
                return Merge(Span(CoordOf(from), CoordOf(to)), Disk(CoordOf(to), 1));
            }

            if (IsShatterWork(spell))
            {
                return Disk(CoordOf(to), 1);
            }

            if (spell == SpellId.Rain || spell == SpellId.StormCall || spell == SpellId.Flood
                || spell == SpellId.Monsoon || spell == SpellId.Swamp || spell == SpellId.Snowfall
                || spell == SpellId.GraveIce)
            {
                return Disk(CoordOf(to), VeilRadius);
            }

            return new List<Vector2Int> { CoordOf(to) };
        }

        static string QuenchAlong(WorldGrid grid, SpellId spell, List<Vector2Int> cells, out int changed)
        {
            changed = 0;
            var note = string.Empty;
            for (var i = 0; i < cells.Count; i++)
            {
                var tile = grid.Get(cells[i]);
                if (!QuenchesLava(spell, tile))
                {
                    continue;
                }

                if (tile.Transmute(MaterialId.Stone))
                {
                    changed++;
                    if (string.IsNullOrEmpty(note))
                    {
                        note = tile.RaisedAs == RaisedForm.Pillar
                            ? "Yield finds the hungry earth. The column cools to rock."
                            : "Yield finds the hungry earth. The wall cools to rock.";
                    }
                }
            }

            return note;
        }

        static string UnmakeAlong(WorldGrid grid, SpellId spell, List<Vector2Int> cells, out int undone)
        {
            undone = 0;
            var note = string.Empty;
            for (var i = 0; i < cells.Count; i++)
            {
                var tile = grid.Get(cells[i]);
                if (tile != null && MatterLaw.Melts(spell, tile.Material))
                {
                    var before = tile.Material;
                    if (tile.MeltWith(spell))
                    {
                        if (string.IsNullOrEmpty(note))
                        {
                            note = MatterLaw.MeltNote(before);
                        }

                        undone++;
                        continue;
                    }
                }

                if (!Unmakes(spell, tile))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(note))
                {
                    note = IsPlasmaWork(spell)
                        ? "The work eats the ordinary matter."
                        : UnmakeNote(spell, tile);
                }

                if (IsPlasmaWork(spell) ? tile.Annihilate() : tile.RestoreFoundation())
                {
                    undone++;
                }
            }

            return note;
        }

        static string RaiseBodies(
            WorldGrid grid,
            SpellId spell,
            RuneId element,
            Vector3 origin,
            Vector3 from,
            Vector3 to)
        {
            var material = MaterialFor(element, spell);
            var grade = SpanLaw.GradeOf(spell, material);
            var cells = NeedsSpan(spell)
                ? Span(CoordOf(from), CoordOf(to))
                : new List<Vector2Int> { CoordOf(to) };
            if (DriesWater(spell) && !IsPillar(spell))
            {
                cells = CollectWet(grid, CoordOf(to), 2);
            }

            var caster = CoordOf(origin);
            var form = IsSinglePillar(spell) ? RaisedForm.Pillar : RaisedForm.Wall;
            var hasPit = false;
            var hasWater = false;
            for (var i = 0; i < cells.Count; i++)
            {
                var seat = SpanLaw.SeatOf(grid.Get(cells[i]));
                if (seat == SpanSeat.Pit)
                {
                    hasPit = true;
                }
                else if (seat == SpanSeat.Water)
                {
                    hasWater = true;
                }
            }

            var supported = SpanLaw.SpanIsSupported(cells, cell => SpanLaw.SeatOf(grid.Get(cell)));
            var dropSpan = hasPit
                && SpanLaw.NeedsEndAnchors(grade, hasWater, hasPit)
                && !supported;
            var work = SpanLaw.ShouldWiden(grade, hasWater, hasPit, dropSpan)
                ? SpanLaw.Widen(cells)
                : cells;
            var spanning = hasPit || (hasWater && (SpanLaw.WorksOnWater(grade) || SpanLaw.MudsWater(grade) || SpanLaw.LosesToWater(grade)));

            var filled = 0;
            var barred = 0;
            var mudded = 0;
            var frozen = 0;
            var refused = 0;
            var falling = dropSpan ? new List<WorldTile>() : null;

            for (var i = 0; i < work.Count; i++)
            {
                var tile = grid.Get(work[i]);
                var fillingPit = FillsGaps(spell)
                    && (tile == null || tile.Kind == TileKind.Pit)
                    && (tile == null || !tile.IsDeepWater);
                if (tile == null && fillingPit)
                {
                    tile = grid.EnsureOpenPit(work[i].x, work[i].y);
                }

                if (tile == null)
                {
                    continue;
                }

                if (tile.IsDeepWater && DriesWater(spell) && !IsPillar(spell) && !FreezesWater(spell))
                {
                    tile.BecomeWalkable(MaterialId.Stone);
                    filled++;
                    continue;
                }

                if (tile.IsDeepWater)
                {
                    if (SpanLaw.FreezesWater(grade) || FreezesWater(spell))
                    {
                        if (tile.FreezeSolid())
                        {
                            frozen++;
                            filled++;
                        }

                        continue;
                    }

                    if (SpanLaw.MudsWater(grade))
                    {
                        if (tile.LayMudCover())
                        {
                            mudded++;
                        }

                        continue;
                    }

                    if (SpanLaw.LosesToWater(grade))
                    {
                        refused++;
                        continue;
                    }

                    if (SpanLaw.GrowsOverWater(grade) && FillsGaps(spell))
                    {
                        if (tile.GrowOverWater(spell == SpellId.Tree || spell == SpellId.WoodWall
                            ? MaterialId.Grove
                            : MaterialId.Plant))
                        {
                            filled++;
                        }

                        continue;
                    }

                    if (SpanLaw.WorksOnWater(grade) && FillsGaps(spell))
                    {
                        tile.BecomeWalkable(material, conjured: true);
                        filled++;
                    }

                    continue;
                }

                if (tile.Kind == TileKind.Pit && FillsGaps(spell))
                {
                    tile.BecomeWalkable(material, conjured: true);
                    filled++;
                    if (dropSpan)
                    {
                        falling.Add(tile);
                    }

                    continue;
                }

                if (spanning)
                {
                    continue;
                }

                if (RaisesBarrier(spell) && work[i] != caster && tile.CanRaiseBarrier)
                {
                    tile.BecomeBarrier(material, form);
                    barred++;
                }
            }

            if (filled > 0)
            {
                grid.DressLooks();
            }

            if (falling != null && falling.Count > 0)
            {
                SpanFall.Begin(grid, falling);
                return form == RaisedForm.Pillar
                    ? "The column finds no rest at both ends. It falls."
                    : "The span finds no floor or wall at each end. It falls.";
            }

            if (dropSpan && filled == 0 && mudded == 0 && frozen == 0)
            {
                return form == RaisedForm.Pillar
                    ? "The column finds no rest at both ends. It falls."
                    : "The span finds no floor or wall at each end. It falls.";
            }

            if (refused > 0 && filled == 0 && barred == 0 && mudded == 0 && frozen == 0)
            {
                return "Hunger cannot stand on yield. The work goes out.";
            }

            if (mudded > 0 && filled == 0)
            {
                return "Rest meeting yield. Mud covers the water. It will not hold you.";
            }

            if (filled > 0 && barred > 0)
            {
                return spell == SpellId.OilPillar
                    ? "A stood wick. A later fire sentence would make it a bomb."
                    : "Rest stands where the floor was, and fills the hollow.";
            }

            if (filled > 0)
            {
                if (spell == SpellId.OilPillar)
                {
                    return "A stood wick. A later fire sentence would make it a bomb.";
                }

                if (DriesWater(spell) && !IsPillar(spell) && !FreezesWater(spell))
                {
                    return filled == 1
                        ? "Hunger drinks the water. The bed is left."
                        : "The channel boils dry. You can walk the bed.";
                }

                if (frozen > 0 || (FreezesWater(spell) && SpanLaw.FreezesWater(grade)))
                {
                    return filled == 1
                        ? "Yield given a body. That water is ice."
                        : "Hard water stands. The pool will hold you.";
                }

                if (hasWater && SpanLaw.GrowsOverWater(grade))
                {
                    return filled == 1
                        ? "Green covers the water. You can walk it."
                        : "A vegetable cover takes the pool. You can walk it.";
                }

                if (spell == SpellId.Tree || spell == SpellId.WoodWall)
                {
                    return filled == 1
                        ? "A tree takes the hollow and holds."
                        : "A line of trees settles into the drop.";
                }

                return filled == 1
                    ? "The hollow takes a body and holds."
                    : "The span settles into the drop.";
            }

            if (barred > 0)
            {
                if (spell == SpellId.OilPillar)
                {
                    return "A stood wick. A later fire sentence would make it a bomb.";
                }

                if (spell == SpellId.Tree)
                {
                    return "A tree stands.";
                }

                if (spell == SpellId.WoodWall)
                {
                    return barred == 1
                        ? "A tree stands."
                        : "A line of trees stands from end to end.";
                }

                if (form == RaisedForm.Pillar)
                {
                    return "A column stands in the way.";
                }

                return barred == 1
                    ? "A column stands in the way."
                    : "A wall stands from end to end.";
            }

            if (dropSpan)
            {
                return form == RaisedForm.Pillar
                    ? "The column finds no rest at both ends. It falls."
                    : "The span finds no floor or wall at each end. It falls.";
            }

            return string.Empty;
        }

        static string FirstFilled(List<string> notes)
        {
            for (var i = 0; i < notes.Count; i++)
            {
                if (!string.IsNullOrEmpty(notes[i]))
                {
                    return notes[i];
                }
            }

            return string.Empty;
        }

        public static Vector3 HopLanding(WorldGrid grid, Vector3 origin, Vector3 requested, Vector2 facing)
        {
            var start = CoordOf(origin);
            var aim = requested;
            aim.z = 0f;
            var delta = (Vector2)(aim - origin);
            if (delta.sqrMagnitude < 0.36f)
            {
                delta = facing.sqrMagnitude > 0.01f ? facing : Vector2.right;
            }

            var dest = start + ToStep(delta, HopTiles);
            if (grid == null)
            {
                return WorldGrid.Center(dest.x, dest.y);
            }

            var path = Span(start, dest, HopTiles + 1);
            var land = start;
            for (var i = 1; i < path.Count; i++)
            {
                var tile = grid.Get(path[i]);
                if (tile == null || tile.Kind == TileKind.Pit)
                {
                    continue;
                }

                if (!BlocksCell(path[i], tile))
                {
                    land = path[i];
                    continue;
                }

                break;
            }

            return WorldGrid.Center(land.x, land.y);
        }

        public const float PushTiles = 3.2f;

        public static Vector3 ClampShot(WorldGrid grid, Vector3 from, Vector3 to)
        {
            if (grid == null)
            {
                return to;
            }

            var path = Span(CoordOf(from), CoordOf(to));
            for (var i = 1; i < path.Count; i++)
            {
                var tile = grid.Get(path[i]);
                if (BlocksCell(path[i], tile))
                {
                    return WorldGrid.Center(path[i].x, path[i].y);
                }
            }

            return to;
        }

        public static bool HasWallBetween(WorldGrid grid, Vector3 from, Vector3 to)
        {
            return HasWallBetween(grid, from, to, null);
        }

        public static bool HasWallBetween(
            WorldGrid grid,
            Vector3 from,
            Vector3 to,
            System.Func<Vector2Int, bool> ignore)
        {
            if (grid == null)
            {
                return false;
            }

            var start = CoordOf(from);
            var stop = CoordOf(to);
            if (start == stop)
            {
                return false;
            }

            var path = Span(start, stop);
            for (var i = 1; i < path.Count; i++)
            {
                if (path[i] == stop)
                {
                    return false;
                }

                if (ignore != null && ignore(path[i]))
                {
                    continue;
                }

                var tile = grid.Get(path[i]);
                if (BlocksCell(path[i], tile))
                {
                    return true;
                }
            }

            return false;
        }

        public static Vector3 PushLanding(WorldGrid grid, Vector3 from, Vector3 body)
        {
            var delta = (Vector2)(body - from);
            if (delta.sqrMagnitude < 0.04f)
            {
                delta = Vector2.right;
            }

            delta.Normalize();
            var dest = (Vector2)body + delta * PushTiles;
            if (grid == null)
            {
                return dest;
            }

            var path = Span(CoordOf(body), CoordOf(dest), Mathf.CeilToInt(PushTiles) + 1);
            var land = CoordOf(body);
            for (var i = 1; i < path.Count; i++)
            {
                var tile = grid.Get(path[i]);
                if (tile == null)
                {
                    break;
                }

                if (BlocksCell(path[i], tile))
                {
                    break;
                }

                land = path[i];
            }

            return WorldGrid.Center(land.x, land.y);
        }

        static List<Vector2Int> CollectWet(WorldGrid grid, Vector2Int center, int radius)
        {
            var cells = new List<Vector2Int>();
            if (grid == null)
            {
                cells.Add(center);
                return cells;
            }

            for (var y = center.y - radius; y <= center.y + radius; y++)
            {
                for (var x = center.x - radius; x <= center.x + radius; x++)
                {
                    var tile = grid.Get(x, y);
                    if (tile != null && tile.IsDeepWater)
                    {
                        cells.Add(new Vector2Int(x, y));
                    }
                }
            }

            if (cells.Count == 0)
            {
                cells.Add(center);
            }

            return cells;
        }

        static Vector2Int ToStep(Vector2 delta, int tiles)
        {
            if (delta.sqrMagnitude < 0.0001f)
            {
                return new Vector2Int(tiles, 0);
            }

            delta.Normalize();
            return new Vector2Int(
                Mathf.RoundToInt(delta.x * tiles),
                Mathf.RoundToInt(delta.y * tiles));
        }
    }
}
