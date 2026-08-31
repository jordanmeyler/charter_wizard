using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// The vegetable body sent: a climbing line from the adept to the
    /// mark. It holds, and hunger can run it like a wick.
    /// </summary>
    public sealed class VineStrand : MonoBehaviour
    {
        static readonly Color Leaf = new(0.16f, 0.62f, 0.28f);
        static readonly List<VineStrand> Live = new();

        readonly List<SpriteRenderer> _leaves = new();
        readonly List<Vector2Int> _cells = new();
        WorldGrid _grid;

        public static VineStrand Lay(WorldGrid grid, Vector3 from, Vector3 to)
        {
            var host = new GameObject("VineStrand");
            host.transform.position = Vector3.Lerp(from, to, 0.5f);
            var strand = host.AddComponent<VineStrand>();
            strand.Build(grid, from, to);
            return strand;
        }

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

        void Build(WorldGrid grid, Vector3 from, Vector3 to)
        {
            _grid = grid;
            var delta = (Vector2)(to - from);
            var length = delta.magnitude;
            if (length < 0.12f)
            {
                length = 0.12f;
                delta = Vector2.right * length;
            }

            var dir = delta / length;
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            var steps = Mathf.Max(2, Mathf.CeilToInt(length / 0.38f));
            for (var i = 0; i <= steps; i++)
            {
                var t = i / (float)steps;
                var point = (Vector2)from + dir * (length * t);
                var leaf = new GameObject("VineLeaf");
                leaf.transform.SetParent(transform, false);
                leaf.transform.position = point;
                leaf.transform.rotation = Quaternion.Euler(0f, 0f, angle + Mathf.Sin(i * 1.7f) * 18f);
                var scale = 0.72f + (i % 2) * 0.18f;
                leaf.transform.localScale = new Vector3(scale, scale, 1f);
                var view = leaf.AddComponent<SpriteRenderer>();
                view.sprite = SpriteFactory.Leaf(Leaf);
                view.sortingOrder = 11;
                view.color = Color.Lerp(Leaf, new Color(0.08f, 0.38f, 0.16f), (i % 3) * 0.18f);
                _leaves.Add(view);

                if (grid != null)
                {
                    var tile = grid.TileAtWorld(point);
                    if (tile != null && tile.LayVine())
                    {
                        var coord = tile.Coord;
                        if (!_cells.Contains(coord))
                        {
                            _cells.Add(coord);
                        }
                    }
                }
            }
        }

        void Update()
        {
            if (_grid == null || _leaves.Count == 0)
            {
                return;
            }

            var live = 0;
            for (var i = 0; i < _leaves.Count; i++)
            {
                var leaf = _leaves[i];
                if (leaf == null)
                {
                    continue;
                }

                var tile = _grid.TileAtWorld(leaf.transform.position);
                if (tile == null || !tile.HasVine)
                {
                    leaf.enabled = false;
                    continue;
                }

                leaf.enabled = true;
                live++;
                if (tile.IsBurning)
                {
                    leaf.color = Color.Lerp(leaf.color, new Color(1f, 0.4f, 0.1f), 0.08f);
                }
            }

            if (live == 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
