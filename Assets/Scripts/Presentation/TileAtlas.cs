using System;
using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Named slices from <c>Catalog/tiles.json</c> and the sprite sheets
    /// under <c>Sprites/</c>. Floors resolve to stone, dirt, or water.
    /// Ice, fire, and lightning are coverings or FX — they swap onto a base tile.
    /// </summary>
    public static class TileAtlas
    {
        [Serializable]
        sealed class TileDef
        {
            public string id;
            public string source;
            public int col;
            public int row;
            public int w = 1;
            public int h = 1;
            public int x;
            public int y;
            public int width;
            public int height;
            public string pivot;
            public string kind;
            public string note;
        }

        [Serializable]
        sealed class TileAlias
        {
            public string id;
            public string tile;
        }

        [Serializable]
        sealed class TileFile
        {
            public string source = "Sprites/Rogue/RA_Crypt";
            public int cell = 16;
            public float pixelsPerUnit = 16f;
            public TileDef[] tiles;
            public TileAlias[] aliases;
        }

        static bool _loaded;
        static readonly Dictionary<string, Sprite> ById = new(StringComparer.OrdinalIgnoreCase);
        static readonly Dictionary<string, string> Notes = new(StringComparer.OrdinalIgnoreCase);
        static readonly Dictionary<string, Texture2D> Textures = new(StringComparer.OrdinalIgnoreCase);

        public static bool Ready
        {
            get
            {
                Ensure();
                return ById.Count > 0;
            }
        }

        public static void Ensure()
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            var asset = Resources.Load<TextAsset>("Catalog/tiles");
            if (asset == null)
            {
                return;
            }

            var file = JsonUtility.FromJson<TileFile>(asset.text);
            if (file == null || file.tiles == null || file.tiles.Length == 0)
            {
                return;
            }

            var cell = file.cell > 0 ? file.cell : 16;
            var ppu = file.pixelsPerUnit > 0f ? file.pixelsPerUnit : 16f;

            for (var i = 0; i < file.tiles.Length; i++)
            {
                var def = file.tiles[i];
                if (def == null || string.IsNullOrEmpty(def.id))
                {
                    continue;
                }

                var source = string.IsNullOrWhiteSpace(def.source) ? file.source : def.source;
                var texture = LoadTexture(source);
                if (texture == null)
                {
                    continue;
                }

                var sprite = Slice(texture, def, cell, ppu);
                if (sprite == null)
                {
                    continue;
                }

                ById[def.id] = sprite;
                if (!string.IsNullOrEmpty(def.note))
                {
                    Notes[def.id] = def.note;
                }
            }

            if (file.aliases == null)
            {
                return;
            }

            for (var i = 0; i < file.aliases.Length; i++)
            {
                var alias = file.aliases[i];
                if (alias == null || string.IsNullOrEmpty(alias.id) || string.IsNullOrEmpty(alias.tile))
                {
                    continue;
                }

                if (ById.TryGetValue(alias.tile, out var sprite))
                {
                    ById[alias.id] = sprite;
                }
            }
        }

        public static bool TryGet(string id, out Sprite sprite)
        {
            Ensure();
            if (!string.IsNullOrEmpty(id) && ById.TryGetValue(id, out sprite) && sprite != null)
            {
                return true;
            }

            sprite = null;
            return false;
        }

        public static Sprite Get(string id) => TryGet(id, out var sprite) ? sprite : null;

        public static string NoteOf(string id)
        {
            Ensure();
            return Notes.TryGetValue(id, out var note) ? note : string.Empty;
        }

        public static Sprite Floor(MaterialId material, int x, int y, int frame = 0)
        {
            var id = FloorId(material, x, y, frame);
            return Get(id);
        }

        public static Sprite Cover(MaterialId material, int x, int y)
        {
            var id = CoverId(material, x, y);
            return string.IsNullOrEmpty(id) ? null : Get(id);
        }

        public static Sprite Wall(MaterialId material, int x, int y)
        {
            return Get(WallId(material, x, y));
        }

        /// <summary>
        /// One wall face per material. Position-hashed wall-b / wall-c /
        /// wall-crack used to spawn random brick on stamped stone.
        /// </summary>
        public static string WallId(MaterialId material, int x = 0, int y = 0)
        {
            _ = x;
            _ = y;
            if (material == MaterialId.Moss || material == MaterialId.Plant || material == MaterialId.Grove)
            {
                return "wall-moss";
            }

            if (material == MaterialId.Ice || material == MaterialId.Snow || material == MaterialId.Glacier)
            {
                return "wall-ice";
            }

            return "wall";
        }

        /// <summary>
        /// Ice columns share the ice-wall face (and the freeze sheen
        /// as a fallback). The sanctuary fountain prop is furniture,
        /// not a stood body of ice.
        /// </summary>
        public static string ColumnId(MaterialId material)
        {
            if (material == MaterialId.Ice || material == MaterialId.Snow || material == MaterialId.Glacier)
            {
                return "wall-ice";
            }

            return "pillar";
        }

        public static Sprite Column(MaterialId material)
        {
            var id = ColumnId(material);
            var sprite = Get(id);
            if (sprite != null)
            {
                return sprite;
            }

            return id == "wall-ice" ? Get("cover-ice") : null;
        }

        public static Sprite Door(bool open, bool leaf)
        {
            if (!leaf)
            {
                return Get(open ? "arch" : "arch-shut");
            }

            return Get(open ? "door-open" : "door");
        }

        public static string FloorId(MaterialId material, int x, int y, int frame = 0)
        {
            switch (FamilyOf(material))
            {
                case FloorFamily.Dirt:
                    if (material == MaterialId.Mud)
                    {
                        return "floor-mud";
                    }

                    if (material == MaterialId.Ash)
                    {
                        return "floor-ash";
                    }

                    return "floor-dirt";
                case FloorFamily.Water:
                    return (frame & 1) == 0 ? "floor-water" : "floor-water-b";
                default:
                    if (material == MaterialId.Void)
                    {
                        return "pit";
                    }

                    return "floor-stone";
            }
        }

        public static string CoverId(MaterialId material, int x, int y)
        {
            switch (material)
            {
                case MaterialId.Moss:
                    return Pick(x, y, "cover-moss", "cover-moss-b");
                case MaterialId.Plant:
                    return "cover-plant";
                case MaterialId.Grove:
                    return "cover-grove";
                case MaterialId.Timber:
                    return "cover-vine";
                case MaterialId.Ice:
                case MaterialId.Snow:
                case MaterialId.Glacier:
                    return "cover-ice";
                case MaterialId.Metal:
                    return "cover-metal";
                case MaterialId.Mud:
                    return "floor-mud";
                case MaterialId.Ash:
                    return "floor-ash";
                case MaterialId.Crystal:
                    return "cover-seal";
                case MaterialId.Ember:
                case MaterialId.Lava:
                case MaterialId.Hearth:
                    return "cover-fire";
                case MaterialId.Acid:
                case MaterialId.Miasma:
                    return "fx-poison";
                case MaterialId.Damp:
                    return "fx-wet";
                default:
                    return string.Empty;
            }
        }

        public static FloorFamily FamilyOf(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Dirt:
                case MaterialId.Sand:
                case MaterialId.Dust:
                case MaterialId.Mud:
                case MaterialId.Ash:
                    return FloorFamily.Dirt;
                case MaterialId.Water:
                case MaterialId.Rain:
                    return FloorFamily.Water;
                default:
                    return FloorFamily.Stone;
            }
        }

        public static void Audit(System.Collections.Generic.List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (FloorId(MaterialId.Dirt, 0, 0) != "floor-dirt"
                || FloorId(MaterialId.Dirt, 4, 9) != "floor-dirt"
                || FloorId(MaterialId.Dirt, 3, 7) == "floor-pebble"
                || FloorId(MaterialId.Dirt, 1, 2) == "floor-dirt-b")
            {
                broken.Add("Dirt must stay floor-dirt; do not spawn pebble or brown-line variants");
            }

            if (FloorId(MaterialId.Stone, 0, 0) != "floor-stone"
                || FloorId(MaterialId.Stone, 5, 3) != "floor-stone"
                || FloorId(MaterialId.Stone, 2, 8) == "floor-cracked")
            {
                broken.Add("Stone floors must stay floor-stone; do not spawn cracked or wall slices");
            }

            if (WallId(MaterialId.Stone, 0, 0) != "wall"
                || WallId(MaterialId.Stone, 6, 4) != "wall"
                || WallId(MaterialId.Stone, 1, 1) == "wall-crack"
                || WallId(MaterialId.Stone, 2, 3) == "wall-c")
            {
                broken.Add("Stone walls must stay the wall tile; do not spawn random brick");
            }

            if (WallId(MaterialId.Ice, 1, 0) != "wall-ice"
                || WallId(MaterialId.Snow, 0, 0) != "wall-ice"
                || WallId(MaterialId.Glacier, 2, 3) != "wall-ice"
                || ColumnId(MaterialId.Ice) != "wall-ice"
                || ColumnId(MaterialId.Snow) != "wall-ice"
                || ColumnId(MaterialId.Glacier) != "wall-ice"
                || ColumnId(MaterialId.Ice) == "ice-fountain"
                || ColumnId(MaterialId.Stone) != "pillar")
            {
                broken.Add("Ice wall and ice column must share the ice-wall face, not the fountain prop");
            }
        }

        static string Pick(int x, int y, params string[] ids)
        {
            if (ids == null || ids.Length == 0)
            {
                return "floor-stone";
            }

            return ids[Mathf.Abs(x * 19 + y * 11) % ids.Length];
        }

        static Sprite Slice(Texture2D texture, TileDef def, int cell, float ppu)
        {
            int x;
            int y;
            int width;
            int height;
            if (def.width > 0 && def.height > 0)
            {
                x = def.x;
                y = def.y;
                width = def.width;
                height = def.height;
            }
            else
            {
                width = Mathf.Max(1, def.w) * cell;
                height = Mathf.Max(1, def.h) * cell;
                x = def.col * cell;
                y = texture.height - (def.row + Mathf.Max(1, def.h)) * cell;
            }

            if (x < 0 || y < 0 || x + width > texture.width || y + height > texture.height)
            {
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(x, y, width, height),
                ParsePivot(def.pivot),
                ppu,
                0,
                SpriteMeshType.FullRect);
        }

        static Vector2 ParsePivot(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new Vector2(0.5f, 0.5f);
            }

            var parts = text.Split(',');
            if (parts.Length >= 2 &&
                float.TryParse(parts[0], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var px) &&
                float.TryParse(parts[1], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out var py))
            {
                return new Vector2(px, py);
            }

            return new Vector2(0.5f, 0.5f);
        }

        static Texture2D LoadTexture(string source)
        {
            var key = (source ?? "Sprites/Rogue/RA_Crypt").Trim().Replace('\\', '/');
            if (key.StartsWith("Assets/Resources/", StringComparison.OrdinalIgnoreCase))
            {
                key = key.Substring("Assets/Resources/".Length);
            }

            if (key.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase))
            {
                key = key.Substring("Resources/".Length);
            }

            var dot = key.LastIndexOf('.');
            if (dot > 0)
            {
                key = key.Substring(0, dot);
            }

            if (Textures.TryGetValue(key, out var cached) && cached != null)
            {
                return cached;
            }

            var texture = Resources.Load<Texture2D>(key)
                ?? Resources.Load<Texture2D>("Sprites/" + key)
                ?? Resources.Load<Texture2D>("Sprites/" + System.IO.Path.GetFileName(key));
            if (texture != null)
            {
                texture.filterMode = FilterMode.Point;
                texture.wrapMode = TextureWrapMode.Clamp;
                Textures[key] = texture;
            }

            return texture;
        }
    }

    public enum FloorFamily
    {
        Stone,
        Dirt,
        Water
    }

    public enum DoorFace
    {
        Leaf,
        Jamb
    }
}
