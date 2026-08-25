using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    [Serializable]
    public sealed class CatalogJoin
    {
        public string rune;
        public string[] sources;
    }

    [Serializable]
    public sealed class CatalogSpell
    {
        public int number;
        public string id;
        public string book;
        public string name;
        public string want;
        public string recipe;
        public string via;
        public string form;
        public string outcome;
        public string gate;
        public string work;
        public string target;
        public float radius;
        public string status;
        public float statusSeconds;
    }

    [Serializable]
    public sealed class CatalogFile
    {
        public string note;
        public CatalogJoin[] joins;
        public CatalogSpell[] spells;
    }

    [Serializable]
    public sealed class CatalogSprite
    {
        public string id;
        public int width = 32;
        public int height = 32;
        public float pixelsPerUnit = 16f;
        public string source;
        public string pivot;
        public int x;
        public int y;
        public int frames = 1;
        public float fps = 8f;
        public string[] colors;
        public string cells;
    }

    [Serializable]
    public sealed class CatalogItem
    {
        public string id;
        public string name;
        public string kind;
        public string sprite;
        public string spriteLit;
        public string[] formula;
        public string[] keys;
        public string teachesSpell;
        public string teachesFormula;
        public string note;
        public string look;
        public string matter;
        public bool fragile;
    }

    [Serializable]
    public sealed class ArtFile
    {
        public string note;
        public CatalogSprite[] sprites;
        public CatalogItem[] items;
    }

    /// <summary>
    /// Loads the master recipe book and the art/item catalog from Resources.
    /// </summary>
    public static class CatalogBook
    {
        static bool _loaded;
        static ArtFile _art = new();
        static readonly Dictionary<string, CatalogItem> Items = new(StringComparer.OrdinalIgnoreCase);
        static readonly Dictionary<string, Sprite> Sprites = new(StringComparer.OrdinalIgnoreCase);

        public static IReadOnlyDictionary<string, CatalogItem> AllItems => Items;

        public static void EnsureLoaded()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            LoadSpells();
            LoadArt();
        }

        public static bool TryItem(string id, out CatalogItem item)
        {
            EnsureLoaded();
            if (!string.IsNullOrEmpty(id) && Items.TryGetValue(id, out item))
            {
                return true;
            }

            item = null;
            return false;
        }

        public static bool TrySprite(string id, out Sprite sprite)
        {
            EnsureLoaded();
            if (!string.IsNullOrEmpty(id) && Sprites.TryGetValue(id, out sprite) && sprite != null)
            {
                return true;
            }

            sprite = LoadSheet(id);
            if (sprite != null)
            {
                Sprites[id] = sprite;
                return true;
            }

            sprite = null;
            return false;
        }

        static void LoadSpells()
        {
            var asset = Resources.Load<TextAsset>("Catalog/spells");
            if (asset == null)
            {
                return;
            }

            var file = JsonUtility.FromJson<CatalogFile>(asset.text);
            if (file == null)
            {
                return;
            }

            if (file.joins != null)
            {
                for (var i = 0; i < file.joins.Length; i++)
                {
                    var join = file.joins[i];
                    if (join == null || !RuneCatalog.TryParseName(join.rune, out var rune) || rune == RuneId.None)
                    {
                        continue;
                    }

                    var sources = ParseRunes(join.sources);
                    if (sources.Length > 0)
                    {
                        ChainBook.DefineBirth(rune, sources);
                    }
                }
            }

            if (file.spells == null || file.spells.Length == 0)
            {
                return;
            }

            var entries = new List<CodexEntry>(file.spells.Length);
            for (var i = 0; i < file.spells.Length; i++)
            {
                if (TryEntry(file.spells[i], out var entry))
                {
                    entries.Add(entry);
                }
            }

            if (entries.Count > 0)
            {
                SpellCodex.Replace(entries.ToArray());
            }
        }

        static bool TryEntry(CatalogSpell row, out CodexEntry entry)
        {
            entry = default;
            if (row == null || string.IsNullOrEmpty(row.recipe))
            {
                return false;
            }

            var spell = SpellRegistry.Parse(row.id);
            if (spell == SpellId.None)
            {
                spell = SpellRegistry.Parse(row.name);
            }

            if (spell == SpellId.None)
            {
                return false;
            }

            var book = Enum.TryParse(row.book, true, out SpellBook parsedBook) ? parsedBook : SpellBook.End;
            var outcome = Enum.TryParse(row.outcome, true, out SpellOutcome parsedOutcome)
                ? parsedOutcome
                : SpellOutcome.Neither;
            var work = string.IsNullOrEmpty(row.work) ? spell : SpellRegistry.Parse(row.work);
            var number = row.number > 0 ? row.number : 0;
            entry = new CodexEntry(
                number,
                book,
                spell,
                row.want ?? string.Empty,
                string.IsNullOrEmpty(row.name) ? row.id : row.name,
                row.recipe,
                row.via ?? string.Empty,
                row.form ?? "Shot",
                outcome,
                row.gate ?? string.Empty,
                work);
            return true;
        }

        static void LoadArt()
        {
            var asset = Resources.Load<TextAsset>("Catalog/art");
            if (asset == null)
            {
                return;
            }

            _art = JsonUtility.FromJson<ArtFile>(asset.text) ?? new ArtFile();
            if (_art.sprites != null)
            {
                for (var i = 0; i < _art.sprites.Length; i++)
                {
                    var def = _art.sprites[i];
                    var sprite = Bake(def);
                    if (def != null && !string.IsNullOrEmpty(def.id) && sprite != null)
                    {
                        Sprites[def.id] = sprite;
                    }
                }
            }

            if (_art.items == null)
            {
                return;
            }

            for (var i = 0; i < _art.items.Length; i++)
            {
                var item = _art.items[i];
                if (item != null && !string.IsNullOrEmpty(item.id))
                {
                    Items[item.id] = item;
                }
            }
        }

        static Sprite Bake(CatalogSprite def)
        {
            if (def == null)
            {
                return null;
            }

            var ppu = def.pixelsPerUnit > 0f ? def.pixelsPerUnit : 16f;
            var pivot = ParsePivot(def.pivot);
            if (!string.IsNullOrWhiteSpace(def.source))
            {
                var sliced = LoadSliced(def, ppu, pivot);
                if (sliced != null)
                {
                    return sliced;
                }
            }

            if (def.width <= 0 || def.height <= 0 || string.IsNullOrEmpty(def.cells))
            {
                return null;
            }

            var canvas = new PixelCanvas(def.width, def.height);
            canvas.Clear(new Color(0f, 0f, 0f, 0f));
            var palette = ParsePalette(def.colors);
            var cells = def.cells;
            var i = 0;
            for (var y = 0; y < def.height; y++)
            {
                for (var x = 0; x < def.width; x++)
                {
                    if (i >= cells.Length)
                    {
                        break;
                    }

                    var index = PaletteIndex(cells[i++]);
                    if ((uint)index < (uint)palette.Length)
                    {
                        canvas.Set(x, y, palette[index]);
                    }
                }
            }

            return canvas.ToSprite(ppu, pivot);
        }

        static Sprite LoadSliced(CatalogSprite def, float ppu, Vector2 pivot)
        {
            var texture = LoadTexture(def.source);
            if (texture == null)
            {
                return null;
            }

            var frameW = def.width > 0 ? def.width : texture.width;
            var frameH = def.height > 0 ? def.height : texture.height;
            var count = Mathf.Max(1, def.frames);
            var frames = new Sprite[count];
            for (var i = 0; i < count; i++)
            {
                var x = def.x + i * frameW;
                var y = def.y;
                if (x < 0 || y < 0 || x + frameW > texture.width || y + frameH > texture.height)
                {
                    frames[i] = frames[0];
                    continue;
                }

                frames[i] = Sprite.Create(texture, new Rect(x, y, frameW, frameH), pivot, ppu);
            }

            if (frames[0] == null)
            {
                return null;
            }

            if (count > 1)
            {
                SpriteSheetLibrary.Register(def.id, frames, def.fps);
            }

            return frames[0];
        }

        static Texture2D LoadTexture(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var key = id.Trim().Replace('\\', '/');
            if (key.StartsWith("Assets/Resources/", System.StringComparison.OrdinalIgnoreCase))
            {
                key = key.Substring("Assets/Resources/".Length);
            }

            if (key.StartsWith("Resources/", System.StringComparison.OrdinalIgnoreCase))
            {
                key = key.Substring("Resources/".Length);
            }

            var dot = key.LastIndexOf('.');
            if (dot > 0)
            {
                key = key.Substring(0, dot);
            }

            var texture = Resources.Load<Texture2D>(key)
                ?? Resources.Load<Texture2D>("Sprites/" + key)
                ?? Resources.Load<Texture2D>("Sprites/" + System.IO.Path.GetFileName(key));
            if (texture == null)
            {
                return null;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }

        static Sprite LoadSheet(string id, float pixelsPerUnit = 16f, Vector2? pivot = null)
        {
            var texture = LoadTexture(id);
            if (texture == null)
            {
                return null;
            }

            var ppu = pixelsPerUnit > 0f ? pixelsPerUnit : 16f;
            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                pivot ?? new Vector2(0.5f, 0.5f),
                ppu);
        }

        static Vector2 ParsePivot(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new Vector2(0.5f, 0.5f);
            }

            var parts = text.Split(new[] { ',', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2
                && float.TryParse(parts[0], out var x)
                && float.TryParse(parts[1], out var y))
            {
                return new Vector2(x, y);
            }

            return new Vector2(0.5f, 0.5f);
        }

        static Color[] ParsePalette(string[] colors)
        {
            if (colors == null || colors.Length == 0)
            {
                return new[] { new Color(0f, 0f, 0f, 0f), Color.white };
            }

            var palette = new Color[colors.Length];
            for (var i = 0; i < colors.Length; i++)
            {
                palette[i] = ParseColor(colors[i]);
            }

            return palette;
        }

        public static Color ParseColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
            {
                return new Color(0f, 0f, 0f, 0f);
            }

            hex = hex.Trim();
            if (hex.StartsWith("#"))
            {
                hex = hex.Substring(1);
            }

            if (ColorUtility.TryParseHtmlString("#" + hex, out var color))
            {
                if (hex.Length == 6)
                {
                    color.a = 1f;
                }

                return color;
            }

            return new Color(0f, 0f, 0f, 0f);
        }

        static int PaletteIndex(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }

            if (c >= 'a' && c <= 'z')
            {
                return 10 + (c - 'a');
            }

            if (c >= 'A' && c <= 'Z')
            {
                return 36 + (c - 'A');
            }

            return 0;
        }

        static RuneId[] ParseRunes(string[] names)
        {
            if (names == null || names.Length == 0)
            {
                return Array.Empty<RuneId>();
            }

            var runes = new List<RuneId>(names.Length);
            for (var i = 0; i < names.Length; i++)
            {
                if (RuneCatalog.TryParseName(names[i], out var rune) && rune != RuneId.None)
                {
                    runes.Add(rune);
                }
            }

            return runes.ToArray();
        }
    }
}
