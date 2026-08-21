using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A body that occupies more than a pin. Fog, ice cages, and pits
    /// are hit if the work crosses any of their cells — not only the label.
    /// </summary>
    public interface ISpellVolume
    {
        float DistanceTo(Vector3 point);
        Vector3 ClosestPoint(Vector3 point);
        bool Touches(Vector3 point, float radius);
        bool Crosses(Vector3 from, Vector3 to, float width);
        bool OccupiesCell(Vector2Int cell);
    }

    /// <summary>
    /// Shared geometry for anything stamped on tiles. Not Unity physics —
    /// a corridor and a disk against cell centers.
    /// </summary>
    public static class CellVolume
    {
        public const float TileRadius = 0.58f;

        public static float DistanceTo(Vector3 point, Vector3 origin, IList<Vector2Int> cells)
        {
            var best = Vector2.Distance(point, origin);
            if (cells == null || cells.Count == 0)
            {
                return best;
            }

            for (var i = 0; i < cells.Count; i++)
            {
                var gap = Vector2.Distance(point, WorldGrid.Center(cells[i].x, cells[i].y)) - TileRadius;
                if (gap < best)
                {
                    best = Mathf.Max(0f, gap);
                }
            }

            return best;
        }

        public static Vector3 ClosestPoint(Vector3 point, Vector3 origin, IList<Vector2Int> cells)
        {
            var best = origin;
            var bestDistance = Vector2.Distance(point, origin);
            if (cells == null)
            {
                return best;
            }

            for (var i = 0; i < cells.Count; i++)
            {
                var center = WorldGrid.Center(cells[i].x, cells[i].y);
                var distance = Vector2.Distance(point, center);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = center;
                }
            }

            return best;
        }

        public static bool Touches(Vector3 point, float radius, Vector3 origin, IList<Vector2Int> cells)
        {
            return DistanceTo(point, origin, cells) <= Mathf.Max(0.05f, radius);
        }

        public static bool Crosses(Vector3 from, Vector3 to, float width, Vector3 origin, IList<Vector2Int> cells)
        {
            var reach = Mathf.Max(0.2f, width);
            if (SegmentDistance(from, to, origin) <= reach + 0.35f)
            {
                return true;
            }

            if (cells == null)
            {
                return false;
            }

            for (var i = 0; i < cells.Count; i++)
            {
                if (SegmentDistance(from, to, WorldGrid.Center(cells[i].x, cells[i].y)) <= reach + TileRadius)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Occupies(IList<Vector2Int> cells, Vector2Int cell, Vector3 origin)
        {
            if (WorldWork.CoordOf(origin) == cell)
            {
                return true;
            }

            if (cells == null)
            {
                return false;
            }

            for (var i = 0; i < cells.Count; i++)
            {
                if (cells[i] == cell)
                {
                    return true;
                }
            }

            return false;
        }

        public static float SegmentDistance(Vector3 from, Vector3 to, Vector3 point)
        {
            var a = (Vector2)from;
            var b = (Vector2)to;
            var p = (Vector2)point;
            var span = b - a;
            var lengthSq = span.sqrMagnitude;
            if (lengthSq < 0.0001f)
            {
                return Vector2.Distance(p, a);
            }

            var t = Mathf.Clamp01(Vector2.Dot(p - a, span) / lengthSq);
            return Vector2.Distance(p, a + span * t);
        }
    }
}
