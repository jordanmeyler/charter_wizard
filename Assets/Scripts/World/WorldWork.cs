using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Terrain verbs that follow a finished cast. Kept small on purpose:
    /// a pillar is one tile, a wall is a start-to-stop line, hop and flight
    /// stay on the caster.
    /// </summary>
    public static class WorldWork
    {
        public const int MaxWallLength = 10;
        public const int HopTiles = 4;
        public const float FlightSeconds = 10f;
        public const float TimeStopSeconds = 8f;

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

        public static bool FillsGaps(SpellId spell)
        {
            return IsPillar(spell) || spell == SpellId.Bridge || spell == SpellId.ObsidianPath;
        }

        public static bool RaisesBarrier(SpellId spell) =>
            IsPillar(spell);

        public static bool LeavesGapsWhenCrossing(SpellId spell) =>
            IsHop(spell) || IsFlight(spell);

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

            if (!FillsGaps(spell) && !RaisesBarrier(spell))
            {
                return string.Empty;
            }

            var material = MaterialFor(element, spell);
            var cells = NeedsSpan(spell)
                ? Span(CoordOf(from), CoordOf(to))
                : new List<Vector2Int> { CoordOf(to) };
            var caster = CoordOf(origin);
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
                    tile.BecomeWalkable(material);
                    filled++;
                    continue;
                }

                if (crossesGap)
                {
                    continue;
                }

                if (RaisesBarrier(spell) && cells[i] != caster && tile.CanRaiseBarrier)
                {
                    tile.BecomeBarrier(material);
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
                return barred == 1
                    ? "A column stands in the way."
                    : "A wall stands from end to end.";
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
