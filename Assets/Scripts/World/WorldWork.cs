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
        public const int HopTiles = 4;
        public const float FlightSeconds = 10f;
        public const float TimeStopSeconds = 8f;
        public const int VeilRadius = 2;

        public static bool IsHop(SpellId spell) =>
            spell == SpellId.Hop;

        public static bool IsFlight(SpellId spell) =>
            spell == SpellId.Flight;

        public static bool IsTimeStop(SpellId spell) =>
            spell == SpellId.TimeStop;

        public static bool NeedsSpan(SpellId spell) =>
            spell == SpellId.Wall || spell == SpellId.IceWall;

        public static bool IsPillar(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.FlamePillar:
                case SpellId.IcePillar:
                case SpellId.IceWall:
                case SpellId.Wall:
                case SpellId.VineRise:
                case SpellId.StonePillar:
                case SpellId.EarthPillar:
                case SpellId.Menhir:
                case SpellId.LavaPillar:
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

        public static bool DriesWater(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Fireball:
                case SpellId.FlamePillar:
                case SpellId.Melt:
                case SpellId.Ignite:
                case SpellId.SunLance:
                case SpellId.Scald:
                case SpellId.Thaw:
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

            return IsSightVeil(spell) ? VeilKind.Fog : VeilKind.None;
        }

        public static bool IsWaterWork(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.WaterJet:
                case SpellId.Flood:
                case SpellId.Rain:
                case SpellId.Scald:
                case SpellId.Spring:
                case SpellId.Douse:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFireWork(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Fireball:
                case SpellId.FlamePillar:
                case SpellId.Ignite:
                case SpellId.SunLance:
                case SpellId.LavaFlood:
                case SpellId.LavaPillar:
                case SpellId.LiveFloor:
                case SpellId.Melt:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsAirWork(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Gale:
                case SpellId.Gust:
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

            return kind == VeilKind.Fog && IsLightWork(spell);
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

        public static bool IsRockBody(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Stone:
                case MaterialId.SaltCrust:
                case MaterialId.Scoured:
                case MaterialId.Obsidian:
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
        /// basic earth wall. Fire thaws ice. Water ends a flame. Fire eats vine.
        /// A boulder or Shatter breaks rock. Room masonry is not a spell-body.
        /// </summary>
        public static bool Unmakes(SpellId spell, WorldTile tile)
        {
            if (tile == null || !tile.IsConjured)
            {
                return false;
            }

            var material = tile.Material;
            if (IsWaterWork(spell) && IsBasicEarth(material))
            {
                return true;
            }

            if ((IsFireWork(spell) || spell == SpellId.Thaw) && IsIceBody(material))
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

            if (spell == SpellId.Thaw && IsIceBody(material))
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

            if (IsIceBody(tile.Material))
            {
                return "Hunger finds the ice. It remembers yield.";
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
            if (spell == SpellId.ObsidianPath)
            {
                return MaterialId.Obsidian;
            }

            if (spell == SpellId.IcePillar || spell == SpellId.IceWall)
            {
                return MaterialId.Ice;
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
            Vector3 to)
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
            var reach = IsSpreadWork(spell) ? VeilRadius : 1;
            var cells = WorkCells(spell, origin, from, to);
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

            var cleared = VeilField.ClearWhat(grid, IsSpreadWork(spell) ? origin : to, spell, reach);
            if (cleared > 0)
            {
                notes.Add(IsLightWork(spell)
                    ? "Light lifts the hanging veil."
                    : IsAirWork(spell)
                        ? "Breath tears the hanging veil."
                        : "Hunger eats the hanging veil.");
            }

            if (LaysVeil(spell))
            {
                var kind = VeilKindOf(spell);
                VeilField.Lay(grid, kind, origin, VeilRadius);
                notes.Add(kind == VeilKind.Poison
                    ? "A sick mist stands. Breath is unkind."
                    : "The hanging veil is given a body. The room is lost.");
            }

            if (FillsGaps(spell) || RaisesBarrier(spell) || DriesWater(spell))
            {
                var built = RaiseBodies(grid, spell, element, origin, from, to);
                if (!string.IsNullOrEmpty(built))
                {
                    notes.Add(built);
                }
            }

            if (IsWaterWork(spell))
            {
                var filled = FillSmallPits(grid, cells);
                if (filled > 0)
                {
                    notes.Add(filled == 1
                        ? "Yield takes a small hollow and stands as a floor."
                        : "Yield fills the small hollows. Water · Salt holds as a floor.");
                }
            }

            return FirstFilled(notes);
        }

        public const int SmallPitSpan = 4;

        /// <summary>
        /// Water · Salt is a floor. A connected pit smaller than 4×4
        /// takes that floor when yield finds it.
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

                    pit.BecomeWalkable(MaterialId.Water);
                    pit.Drench(1f);
                    filled++;
                }
            }

            return filled;
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

        static bool IsSpreadWork(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Fog:
                case SpellId.Gloom:
                case SpellId.Veil:
                case SpellId.Blight:
                case SpellId.GraveDust:
                case SpellId.Flood:
                case SpellId.LiveFloor:
                case SpellId.Quagmire:
                case SpellId.Sprout:
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
                || spell == SpellId.Gust || spell == SpellId.Scald || spell == SpellId.SunLance
                || spell == SpellId.HurledStone || spell == SpellId.Douse)
            {
                return Span(CoordOf(from), CoordOf(to));
            }

            if (IsShatterWork(spell))
            {
                return Disk(CoordOf(to), 1);
            }

            if (spell == SpellId.Rain || spell == SpellId.StormCall || spell == SpellId.Flood)
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
                if (!Unmakes(spell, tile))
                {
                    continue;
                }

                if (string.IsNullOrEmpty(note))
                {
                    note = UnmakeNote(spell, tile);
                }

                if (tile.RestoreFoundation())
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
            var cells = NeedsSpan(spell)
                ? Span(CoordOf(from), CoordOf(to))
                : new List<Vector2Int> { CoordOf(to) };
            if (DriesWater(spell) && !IsPillar(spell))
            {
                cells = CollectWet(grid, CoordOf(to), 2);
            }
            var caster = CoordOf(origin);
            var form = IsSinglePillar(spell) ? RaisedForm.Pillar : RaisedForm.Wall;
            var crossesGap = false;
            for (var i = 0; i < cells.Count; i++)
            {
                var probe = grid.Get(cells[i]);
                if (probe != null && probe.Kind == TileKind.Pit)
                {
                    crossesGap = true;
                    break;
                }
            }

            var filled = 0;
            var barred = 0;

            for (var i = 0; i < cells.Count; i++)
            {
                var tile = grid.Get(cells[i]);
                if (tile == null)
                {
                    continue;
                }

                if (tile.Kind == TileKind.Pit && tile.Material == MaterialId.Water &&
                    DriesWater(spell) && !IsPillar(spell))
                {
                    tile.BecomeWalkable(MaterialId.Stone);
                    filled++;
                    continue;
                }

                if (tile.Kind == TileKind.Pit && FillsGaps(spell))
                {
                    tile.BecomeWalkable(material, conjured: true);
                    filled++;
                    continue;
                }

                if (crossesGap)
                {
                    continue;
                }

                if (RaisesBarrier(spell) && cells[i] != caster && tile.CanRaiseBarrier)
                {
                    tile.BecomeBarrier(material, form);
                    barred++;
                }
            }

            if (filled > 0 && barred > 0)
            {
                return "Rest stands where the floor was, and fills the hollow.";
            }

            if (filled > 0)
            {
                if (DriesWater(spell) && !IsPillar(spell))
                {
                    return filled == 1
                        ? "Hunger drinks the water. The bed is left."
                        : "The channel boils dry. You can walk the bed.";
                }

                return filled == 1
                    ? "The hollow takes a body and holds."
                    : "The span settles into the drop.";
            }

            if (barred > 0)
            {
                if (form == RaisedForm.Pillar)
                {
                    return "A column stands in the way.";
                }

                return barred == 1
                    ? "A column stands in the way."
                    : "A wall stands from end to end.";
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
                if (tile == null)
                {
                    break;
                }

                if (tile.Kind == TileKind.Pit)
                {
                    continue;
                }

                if (!tile.Def.BlocksMovement)
                {
                    land = path[i];
                    continue;
                }

                break;
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
                    if (tile != null && tile.Kind == TileKind.Pit && tile.Material == MaterialId.Water)
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
