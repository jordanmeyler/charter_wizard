using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// What a cell offers a span. Floors and walls are rest a
    /// basic earth or ice bridge can grab. Water is a different
    /// vessel — ice covers it, earth only muds it.
    /// </summary>
    public enum SpanSeat
    {
        None,
        Floor,
        Wall,
        Pit,
        Water
    }

    /// <summary>
    /// How a stood body treats a gap. Basic earth and ice must
    /// join two banks. Metal and later work can hang. Fire loses
    /// to water.
    /// </summary>
    public enum SpanGrade
    {
        BasicEarth,
        Ice,
        Metal,
        Fire,
        Advanced
    }

    /// <summary>
    /// Shared rules for walls, columns, and dedicated spans.
    /// A wall over a hollow is a two-tile-wide bridge. A column
    /// uses the same law on one click.
    /// </summary>
    public static class SpanLaw
    {
        public const int BridgeWidth = 2;

        public static SpanGrade GradeOf(SpellId spell, MaterialId material)
        {
            if (spell == SpellId.IcePillar
                || spell == SpellId.IceWall
                || WorldWork.IsIceBody(material))
            {
                return SpanGrade.Ice;
            }

            if (spell == SpellId.MetalPillar
                || spell == SpellId.MetalWall
                || material == MaterialId.Metal)
            {
                return SpanGrade.Metal;
            }

            if (spell == SpellId.FlamePillar
                || spell == SpellId.LavaPillar
                || WorldWork.IsFlameBody(material)
                || WorldWork.IsLavaBody(material))
            {
                return SpanGrade.Fire;
            }

            if (spell == SpellId.Wall
                || spell == SpellId.EarthPillar
                || spell == SpellId.StonePillar
                || spell == SpellId.Bridge
                || WorldWork.IsBasicEarth(material))
            {
                return SpanGrade.BasicEarth;
            }

            return SpanGrade.Advanced;
        }

        /// <summary>
        /// Standard earth and ice over a pit must find floor or
        /// wall at each end. Ice over water does not. Metal never
        /// needs a far bank.
        /// </summary>
        public static bool NeedsEndAnchors(SpanGrade grade, bool overWater, bool overPit)
        {
            if (grade == SpanGrade.Metal || grade == SpanGrade.Advanced || grade == SpanGrade.Fire)
            {
                return false;
            }

            if (grade == SpanGrade.Ice && overWater && !overPit)
            {
                return false;
            }

            return overPit;
        }

        public static bool WorksOnWater(SpanGrade grade) =>
            grade == SpanGrade.Ice
            || grade == SpanGrade.Metal
            || grade == SpanGrade.Advanced;

        public static bool LosesToWater(SpanGrade grade) =>
            grade == SpanGrade.Fire;

        public static bool MudsWater(SpanGrade grade) =>
            grade == SpanGrade.BasicEarth;

        public static bool FreezesWater(SpanGrade grade) =>
            grade == SpanGrade.Ice;

        /// <summary>
        /// Structural bridges (a pit, or advanced work over water)
        /// grow two tiles wide. Ice on water keeps its own covering.
        /// </summary>
        public static bool ShouldWiden(SpanGrade grade, bool overWater, bool overPit, bool dropped)
        {
            if (overPit)
            {
                return true;
            }

            if (dropped)
            {
                return false;
            }

            return overWater && WorksOnWater(grade) && grade != SpanGrade.Ice;
        }

        public static bool IsAnchorSeat(SpanSeat seat) =>
            seat == SpanSeat.Floor || seat == SpanSeat.Wall;

        public static SpanSeat SeatOf(WorldTile tile)
        {
            if (tile == null)
            {
                return SpanSeat.Pit;
            }

            if (tile.IsDeepWater)
            {
                return SpanSeat.Water;
            }

            switch (tile.Kind)
            {
                case TileKind.Floor:
                case TileKind.Bridge:
                    return SpanSeat.Floor;
                case TileKind.Wall:
                case TileKind.Door:
                    return SpanSeat.Wall;
                case TileKind.Pit:
                    return SpanSeat.Pit;
                default:
                    return SpanSeat.None;
            }
        }

        public static bool CellIsAnchored(Vector2Int cell, Func<Vector2Int, SpanSeat> seat)
        {
            if (seat == null)
            {
                return false;
            }

            if (IsAnchorSeat(seat(cell)))
            {
                return true;
            }

            return CountAnchorsAround(cell, seat) > 0;
        }

        public static int CountAnchorsAround(Vector2Int cell, Func<Vector2Int, SpanSeat> seat)
        {
            if (seat == null)
            {
                return 0;
            }

            var count = 0;
            if (IsAnchorSeat(seat(cell + Vector2Int.right)))
            {
                count++;
            }

            if (IsAnchorSeat(seat(cell + Vector2Int.left)))
            {
                count++;
            }

            if (IsAnchorSeat(seat(cell + Vector2Int.up)))
            {
                count++;
            }

            if (IsAnchorSeat(seat(cell + Vector2Int.down)))
            {
                count++;
            }

            return count;
        }

        /// <summary>
        /// A drawn wall needs rest at each end (the cell itself
        /// or a neighbouring floor / wall). A single column over
        /// a hollow must touch two distinct banks.
        /// </summary>
        public static bool SpanIsSupported(IReadOnlyList<Vector2Int> cells, Func<Vector2Int, SpanSeat> seat)
        {
            if (cells == null || cells.Count == 0 || seat == null)
            {
                return false;
            }

            if (cells.Count == 1)
            {
                return CountAnchorsAround(cells[0], seat) >= 2;
            }

            return CellIsAnchored(cells[0], seat) && CellIsAnchored(cells[cells.Count - 1], seat);
        }

        public static Vector2Int PerpendicularOffset(IReadOnlyList<Vector2Int> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return Vector2Int.up;
            }

            var start = cells[0];
            var stop = cells[cells.Count - 1];
            var dx = Mathf.Abs(stop.x - start.x);
            var dy = Mathf.Abs(stop.y - start.y);
            return dx >= dy ? Vector2Int.up : Vector2Int.right;
        }

        public static List<Vector2Int> Widen(IReadOnlyList<Vector2Int> cells)
        {
            var result = new List<Vector2Int>();
            if (cells == null || cells.Count == 0)
            {
                return result;
            }

            var offset = PerpendicularOffset(cells);
            var seen = new HashSet<Vector2Int>();
            for (var i = 0; i < cells.Count; i++)
            {
                var cell = cells[i];
                if (seen.Add(cell))
                {
                    result.Add(cell);
                }

                var extra = cell + offset;
                if (seen.Add(extra))
                {
                    result.Add(extra);
                }
            }

            return result;
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (GradeOf(SpellId.Wall, MaterialId.Stone) != SpanGrade.BasicEarth
                || GradeOf(SpellId.EarthPillar, MaterialId.Stone) != SpanGrade.BasicEarth
                || GradeOf(SpellId.Bridge, MaterialId.Stone) != SpanGrade.BasicEarth)
            {
                broken.Add("Earth wall, column, and bridge must be basic rest");
            }

            if (GradeOf(SpellId.IceWall, MaterialId.Ice) != SpanGrade.Ice
                || GradeOf(SpellId.IcePillar, MaterialId.Ice) != SpanGrade.Ice)
            {
                broken.Add("Ice wall and column must share ice’s span grade");
            }

            if (GradeOf(SpellId.MetalWall, MaterialId.Metal) != SpanGrade.Metal
                || GradeOf(SpellId.MetalPillar, MaterialId.Metal) != SpanGrade.Metal)
            {
                broken.Add("Metal wall and column must not need a far bank");
            }

            if (GradeOf(SpellId.LavaPillar, MaterialId.Lava) != SpanGrade.Fire
                || GradeOf(SpellId.FlamePillar, MaterialId.Hearth) != SpanGrade.Fire)
            {
                broken.Add("Stood hunger must lose to water");
            }

            if (GradeOf(SpellId.ObsidianPath, MaterialId.Obsidian) != SpanGrade.Advanced
                || GradeOf(SpellId.ObsidianWall, MaterialId.Obsidian) != SpanGrade.Advanced
                || GradeOf(SpellId.VineRise, MaterialId.Grove) != SpanGrade.Advanced)
            {
                broken.Add("Later spans must work in water unless the square forbids it");
            }

            if (!NeedsEndAnchors(SpanGrade.BasicEarth, false, true)
                || !NeedsEndAnchors(SpanGrade.Ice, false, true))
            {
                broken.Add("Standard earth and ice over a pit must join two floors");
            }

            if (NeedsEndAnchors(SpanGrade.Ice, true, false))
            {
                broken.Add("Ice over water must not ask for a start or end bank");
            }

            if (NeedsEndAnchors(SpanGrade.Metal, false, true)
                || NeedsEndAnchors(SpanGrade.Advanced, true, true))
            {
                broken.Add("Metal and later work must hang without a far rest");
            }

            if (MudsWater(SpanGrade.BasicEarth) == false
                || WorksOnWater(SpanGrade.BasicEarth)
                || LosesToWater(SpanGrade.Ice)
                || !WorksOnWater(SpanGrade.Ice)
                || !WorksOnWater(SpanGrade.Metal)
                || !LosesToWater(SpanGrade.Fire))
            {
                broken.Add("Earth muds water, ice freezes it, metal crosses it, hunger goes out");
            }

            var line = new List<Vector2Int> { new(0, 0), new(1, 0), new(2, 0) };
            var wide = Widen(line);
            if (wide.Count != 6
                || !wide.Contains(new Vector2Int(0, 1))
                || !wide.Contains(new Vector2Int(2, 1)))
            {
                broken.Add("A span over a gap must be two tiles wide");
            }

            SpanSeat Seat(Vector2Int cell)
            {
                if (cell == new Vector2Int(0, 0) || cell == new Vector2Int(4, 0))
                {
                    return SpanSeat.Floor;
                }

                if (cell == new Vector2Int(5, 2))
                {
                    return SpanSeat.Wall;
                }

                return SpanSeat.Pit;
            }

            var joined = new List<Vector2Int> { new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0) };
            if (!SpanIsSupported(joined, Seat))
            {
                broken.Add("A span from floor to floor must hold");
            }

            var hanging = new List<Vector2Int> { new(1, 0), new(2, 0), new(3, 0) };
            if (SpanIsSupported(hanging, Seat))
            {
                broken.Add("A span that misses a floor at each end must fall");
            }

            var fromWall = new List<Vector2Int> { new(5, 2), new(5, 1), new(5, 0), new(4, 0) };
            if (!SpanIsSupported(fromWall, Seat))
            {
                broken.Add("A span must be able to grab a wall");
            }

            var column = new List<Vector2Int> { new(1, 1) };
            SpanSeat Narrow(Vector2Int cell)
            {
                if (cell == new Vector2Int(1, 0) || cell == new Vector2Int(1, 2))
                {
                    return SpanSeat.Floor;
                }

                return SpanSeat.Pit;
            }

            if (!SpanIsSupported(column, Narrow))
            {
                broken.Add("A column in a one-tile gap must join the two floors");
            }

            SpanSeat Cantilever(Vector2Int cell)
            {
                return cell == new Vector2Int(1, 0) ? SpanSeat.Floor : SpanSeat.Pit;
            }

            if (SpanIsSupported(column, Cantilever))
            {
                broken.Add("A basic column that only touches one bank must fall");
            }
        }
    }
}
