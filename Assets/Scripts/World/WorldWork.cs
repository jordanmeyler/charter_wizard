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
            spell == SpellId.Wall;

        public static bool IsPillar(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.FlamePillar:
                case SpellId.IcePillar:
                case SpellId.Wall:
                case SpellId.VineRise:
                case SpellId.StonePillar:
                case SpellId.Menhir:
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
        /// Room masonry is not a spell-body and will not go.
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

            return false;
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

            return "The stood body comes apart.";
        }

        public static MaterialId MaterialFor(RuneId element, SpellId spell)
        {
            if (spell == SpellId.ObsidianPath)
            {
                return MaterialId.Obsidian;
            }

            if (spell == SpellId.IcePillar)
            {
                return MaterialId.Ice;
            }

            if (spell == SpellId.FlamePillar)
            {
                return MaterialId.Hearth;
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

            if (FillsGaps(spell) || RaisesBarrier(spell))
            {
                var built = RaiseBodies(grid, spell, element, origin, from, to);
                if (!string.IsNullOrEmpty(built))
                {
                    notes.Add(built);
                }
            }

            return FirstFilled(notes);
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
                || spell == SpellId.Scald || spell == SpellId.SunLance || spell == SpellId.HurledStone)
            {
                return Span(CoordOf(from), CoordOf(to));
            }

            return new List<Vector2Int> { CoordOf(to) };
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
            var caster = CoordOf(origin);
            var form = NeedsSpan(spell) ? RaisedForm.Wall : RaisedForm.Pillar;
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
