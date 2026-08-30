using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A span that found no rest. It stands long enough to be seen,
    /// then falls back to the hollow. HoldSeconds is the placeholder
    /// beat — later a fall animation can play in that window, then
    /// call Collapse.
    /// </summary>
    public sealed class SpanFall : MonoBehaviour
    {
        public const float HoldSeconds = 0.45f;

        public static void Begin(WorldGrid grid, IReadOnlyList<WorldTile> tiles, float hold = HoldSeconds)
        {
            if (grid == null || tiles == null || tiles.Count == 0)
            {
                return;
            }

            var host = grid.GetComponent<SpanFall>();
            if (host == null)
            {
                host = grid.gameObject.AddComponent<SpanFall>();
            }

            host.Queue(tiles, hold);
        }

        public static void Collapse(IReadOnlyList<WorldTile> tiles)
        {
            if (tiles == null)
            {
                return;
            }

            WorldGrid grid = null;
            for (var i = 0; i < tiles.Count; i++)
            {
                var tile = tiles[i];
                if (tile == null || !tile.IsConjured || tile.RaisedAs != RaisedForm.Span)
                {
                    continue;
                }

                grid = grid != null ? grid : tile.GetComponentInParent<WorldGrid>();
                tile.RestoreFoundation();
            }

            grid?.DressLooks();
        }

        void Queue(IReadOnlyList<WorldTile> tiles, float hold)
        {
            var copy = new List<WorldTile>(tiles.Count);
            for (var i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] != null)
                {
                    copy.Add(tiles[i]);
                }
            }

            if (copy.Count > 0)
            {
                StartCoroutine(FallRoutine(copy, hold));
            }
        }

        IEnumerator FallRoutine(List<WorldTile> tiles, float hold)
        {
            if (hold > 0f)
            {
                yield return new WaitForSeconds(hold);
            }

            Collapse(tiles);
        }
    }
}
