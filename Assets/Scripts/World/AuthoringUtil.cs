using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Shared helpers for scene-placed props: visuals, catalog overlays,
    /// and parsing the same rune / spell names the JSON maps use.
    /// </summary>
    public static class AuthoringUtil
    {
        public static T GetOrAdd<T>(GameObject host) where T : Component
        {
            if (host == null)
            {
                return null;
            }

            var existing = host.GetComponent<T>();
            return existing != null ? existing : host.AddComponent<T>();
        }

        public static SpriteRenderer ApplyLook(
            GameObject host,
            int sortingOrder,
            string spriteId,
            Sprite portrait,
            Sprite[] frames,
            float fps = 4f)
        {
            var renderer = GetOrAdd<SpriteRenderer>(host);
            renderer.sortingOrder = sortingOrder;
            if (frames != null && frames.Length > 0)
            {
                SpriteAnim.On(host, renderer).Play(frames, fps, true, spriteId ?? "authored");
                return renderer;
            }

            if (portrait != null)
            {
                renderer.sprite = portrait;
                return renderer;
            }

            var id = string.IsNullOrEmpty(spriteId) ? "charm" : spriteId;
            renderer.sprite = SpriteFactory.Named(id);
            SpriteAnim.On(host, renderer).Play(id, fps);
            return renderer;
        }

        public static void PlayChange(GameObject host, SpriteRenderer renderer, Sprite[] frames, string clip, float fps, System.Action done)
        {
            Sprite[] play = frames;
            if ((play == null || play.Length == 0) && !string.IsNullOrEmpty(clip))
            {
                play = SpriteFactory.Clip(clip);
            }

            if (play == null || play.Length == 0)
            {
                done?.Invoke();
                return;
            }

            SpriteAnim.On(host, renderer).Play(play, fps, false, clip ?? "change", done);
        }

        public static RuneId[] ParseRunes(string[] names, params RuneId[] fallback)
        {
            if (names == null || names.Length == 0)
            {
                return fallback;
            }

            var runes = new List<RuneId>(names.Length);
            for (var i = 0; i < names.Length; i++)
            {
                var rune = MapFile.ParseRune(names[i]);
                if (rune != RuneId.None)
                {
                    runes.Add(rune);
                }
            }

            return runes.Count > 0 ? runes.ToArray() : fallback;
        }

        public static SpellId[] ParseKeys(string[] names, SpellId[] fallback)
        {
            if (names == null || names.Length == 0)
            {
                return fallback ?? System.Array.Empty<SpellId>();
            }

            var keys = new List<SpellId>(names.Length);
            for (var i = 0; i < names.Length; i++)
            {
                var spell = MapFile.ParseSpell(names[i]);
                if (spell != SpellId.None)
                {
                    keys.Add(spell);
                }
            }

            return keys.Count > 0 ? keys.ToArray() : fallback ?? System.Array.Empty<SpellId>();
        }

        public static CatalogItem ResolveItem(
            string catalogId,
            string displayName,
            string spriteId,
            string matter,
            bool fragile,
            string[] keys,
            string teachesSpell,
            string note,
            string look,
            string kind = "stone")
        {
            CatalogItem source = null;
            if (!string.IsNullOrEmpty(catalogId))
            {
                CatalogBook.TryItem(catalogId, out source);
            }

            var item = new CatalogItem
            {
                id = First(catalogId, source?.id, "authored-item"),
                name = First(displayName, source?.name, "Item"),
                kind = First(source?.kind, kind),
                sprite = First(spriteId, source?.sprite, "charm"),
                spriteLit = source != null ? source.spriteLit : null,
                formula = source != null ? source.formula : null,
                keys = keys != null && keys.Length > 0 ? keys : source?.keys,
                teachesSpell = First(teachesSpell, source?.teachesSpell),
                teachesFormula = source != null ? source.teachesFormula : null,
                note = First(note, source?.note),
                look = First(look, source?.look),
                matter = First(matter, source?.matter),
                fragile = fragile || (source != null && source.fragile)
            };
            return item;
        }

        public static Vector2Int CellOf(Vector3 world)
        {
            return new Vector2Int(Mathf.FloorToInt(world.x), Mathf.FloorToInt(world.y));
        }

        public static Vector3 Snap(Vector3 world)
        {
            return WorldGrid.Center(Mathf.FloorToInt(world.x), Mathf.FloorToInt(world.y));
        }

        public static Vector2Int[] CellsOrHere(Vector2Int[] cells, Vector3 world)
        {
            if (cells != null && cells.Length > 0)
            {
                return cells;
            }

            return new[] { CellOf(world) };
        }

        /// <summary>
        /// Inspector cells are offsets from this object. Empty means this tile.
        /// </summary>
        public static Vector2Int[] WorldCells(Vector2Int[] offsets, Vector3 world)
        {
            var origin = CellOf(world);
            if (offsets == null || offsets.Length == 0)
            {
                return new[] { origin };
            }

            var cells = new Vector2Int[offsets.Length];
            for (var i = 0; i < offsets.Length; i++)
            {
                cells[i] = origin + offsets[i];
            }

            return cells;
        }

        public static void DrawCellGizmos(Vector3 world, Vector2Int[] offsets, Color color, int radius = 0)
        {
            Gizmos.color = color;
            if (radius > 0 && (offsets == null || offsets.Length == 0))
            {
                var origin = CellOf(world);
                var disk = WorldWork.Disk(origin, radius);
                for (var i = 0; i < disk.Count; i++)
                {
                    Gizmos.DrawWireCube(WorldGrid.Center(disk[i].x, disk[i].y), Vector3.one * 0.92f);
                }

                return;
            }

            var cells = WorldCells(offsets, world);
            for (var i = 0; i < cells.Length; i++)
            {
                Gizmos.DrawWireCube(WorldGrid.Center(cells[i].x, cells[i].y), Vector3.one * 0.92f);
            }
        }

        static string First(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrEmpty(values[i]))
                {
                    return values[i];
                }
            }

            return string.Empty;
        }
    }
}
