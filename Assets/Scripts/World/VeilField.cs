using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A hanging veil that stays: fog that withholds the room, or a poison
    /// mist that does not lift until another element tears it.
    /// </summary>
    public sealed class VeilField : MonoBehaviour
    {
        public VeilKind Kind { get; private set; }
        public Vector2Int Origin { get; private set; }
        public int Radius { get; private set; }

        static readonly List<VeilField> Live = new();

        readonly HashSet<Vector2Int> _cells = new();
        WorldGrid _grid;

        void OnEnable()
        {
            if (!Live.Contains(this))
            {
                Live.Add(this);
            }
        }

        void OnDisable()
        {
            Live.Remove(this);
        }

        public static VeilField Lay(WorldGrid grid, VeilKind kind, Vector3 world, int radius = 2)
        {
            if (grid == null || kind == VeilKind.None)
            {
                return null;
            }

            var origin = WorldWork.CoordOf(world);
            var existing = FindNear(grid, origin, radius + 1);
            if (existing != null)
            {
                existing.Refresh(kind, origin, radius);
                return existing;
            }

            var host = new GameObject(kind == VeilKind.Poison ? "PoisonMist" : "FogVeil");
            host.transform.SetParent(grid.transform, false);
            host.transform.position = WorldGrid.Center(origin.x, origin.y);
            var field = host.AddComponent<VeilField>();
            field.Begin(grid, kind, origin, radius);
            return field;
        }

        public static int ClearNear(WorldGrid grid, Vector3 world, VeilKind wanted, int radius = 2)
        {
            if (grid == null)
            {
                return 0;
            }

            var origin = WorldWork.CoordOf(world);
            var cleared = 0;
            for (var i = Live.Count - 1; i >= 0; i--)
            {
                var field = Live[i];
                if (field == null)
                {
                    continue;
                }

                if (wanted != VeilKind.None && field.Kind != wanted && wanted != VeilKind.Fog)
                {
                    continue;
                }

                if (field.Touches(origin, radius))
                {
                    Object.Destroy(field.gameObject);
                    cleared++;
                }
            }

            return cleared;
        }

        public static int ClearWhat(WorldGrid grid, Vector3 world, SpellId spell, int radius = 2)
        {
            if (!WorldWork.ClearsVeils(spell))
            {
                return 0;
            }

            var origin = WorldWork.CoordOf(world);
            var cleared = 0;
            for (var i = Live.Count - 1; i >= 0; i--)
            {
                var field = Live[i];
                if (field == null || !field.Touches(origin, radius))
                {
                    continue;
                }

                if (WorldWork.ClearsVeil(spell, field.Kind))
                {
                    Object.Destroy(field.gameObject);
                    cleared++;
                }
            }

            return cleared;
        }

        public static bool Covering(Vector3 world, out VeilKind kind)
        {
            kind = VeilKind.None;
            var coord = WorldWork.CoordOf(world);
            for (var i = 0; i < Live.Count; i++)
            {
                if (Live[i] != null && Live[i].Covers(coord))
                {
                    kind = Live[i].Kind;
                    return true;
                }
            }

            return false;
        }

        public static Color Wash(VeilKind kind)
        {
            return kind == VeilKind.Poison
                ? new Color(0.07f, 0.1f, 0.04f)
                : new Color(0.16f, 0.17f, 0.2f);
        }

        static VeilField FindNear(WorldGrid grid, Vector2Int origin, int radius)
        {
            for (var i = 0; i < Live.Count; i++)
            {
                if (Live[i] != null && Live[i]._grid == grid && Live[i].Touches(origin, radius))
                {
                    return Live[i];
                }
            }

            return null;
        }

        void Begin(WorldGrid grid, VeilKind kind, Vector2Int origin, int radius)
        {
            _grid = grid;
            Refresh(kind, origin, radius);
        }

        void Refresh(VeilKind kind, Vector2Int origin, int radius)
        {
            Kind = kind;
            Origin = origin;
            Radius = Mathf.Max(1, radius);
            transform.position = WorldGrid.Center(origin.x, origin.y);
            RebuildCells();
            RebuildVisual();
        }

        void RebuildCells()
        {
            _cells.Clear();
            var cells = WorldWork.Disk(Origin, Radius);
            for (var i = 0; i < cells.Count; i++)
            {
                if (_grid != null)
                {
                    var tile = _grid.Get(cells[i]);
                    if (tile == null || tile.Kind == TileKind.Wall || tile.Kind == TileKind.Door)
                    {
                        continue;
                    }
                }

                _cells.Add(cells[i]);
            }
        }

        void RebuildVisual()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            var look = ElementLook.Of(Kind == VeilKind.Poison ? ElementFamily.Poison : ElementFamily.Fog);
            ElementFx.VeilCloud(transform, look, Radius + 0.4f);

            foreach (var cell in _cells)
            {
                var puff = new GameObject("Puff");
                puff.transform.SetParent(transform, false);
                puff.transform.position = WorldGrid.Center(cell.x, cell.y) + new Vector3(0f, 0.15f, 0f);
                puff.transform.localScale = Vector3.one * 1.35f;
                var renderer = puff.AddComponent<SpriteRenderer>();
                renderer.sprite = SpriteFactory.Glow(look.Mist);
                renderer.sortingOrder = 13;
                renderer.color = look.Mist;
                puff.AddComponent<SpellLight>().Bind(look.Mist, 0.18f, 0f);
            }
        }

        public bool Covers(Vector2Int coord) => _cells.Contains(coord);

        public bool Touches(Vector2Int coord, int radius)
        {
            var reach = radius + Radius;
            var dx = coord.x - Origin.x;
            var dy = coord.y - Origin.y;
            return dx * dx + dy * dy <= reach * reach;
        }
    }
}
