using System;
using UnityEngine;

namespace RuneMagic
{
    [Serializable]
    public sealed class MapIndex
    {
        public string startup = "sanctum";
        public string[] maps;
    }

    [Serializable]
    public sealed class MapCoord
    {
        public int x;
        public int y;

        public MapCoord()
        {
        }

        public MapCoord(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2Int Cell => new(x, y);
    }

    [Serializable]
    public sealed class MapStamp
    {
        public string kind;
        public string material;
        public string aura;
        public string cover;
        public int[] cells;
    }

    [Serializable]
    public sealed class MapProp
    {
        public string type;
        public int x;
        public int y;
        public string text;
        public string[] runes;
        public string dir;
        public string displayName;
        public string formulaId;
        public string[] formula;
        public string[] keys;
        public string sprite;
        public string item;
        public int[] cells;
        public int[] cover;
        public string grant;
        public string[] requires;
        public string clearMaterial;
        public string note;
        public string spell;
        public bool blocking;
        public bool finishes;
        public bool ensouled;
        public string attack;
        public float castSeconds;
        public string[] cast;
    }

    [Serializable]
    public sealed class MapRoom
    {
        public string id;
        public string name;
        public MapCoord origin;
        public int width = 13;
        public int height = 11;
        public string wall = "Stone";
        public string floor = "Stone";
        public string hint;
        public string exit;
        public MapStamp[] stamps;
        public MapProp[] props;
    }

    [Serializable]
    public sealed class MapHall
    {
        public string from;
        public string to;
        public string material = "Stone";
        public string hazard;
    }

    [Serializable]
    public sealed class MapFile
    {
        public string id = "untitled";
        public string name = "Untitled map";
        public MapCoord spawn = new(2, 5);
        public MapRoom[] rooms;
        public MapHall[] halls;

        public static MapFile FromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            return JsonUtility.FromJson<MapFile>(json);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this, true);
        }

        public static MapFile Load(string resourceName)
        {
            var asset = Resources.Load<TextAsset>("Maps/" + resourceName);
            return asset != null ? FromJson(asset.text) : null;
        }

        public static MapFile LoadStartup()
        {
            var indexAsset = Resources.Load<TextAsset>("Maps/index");
            var startup = "sanctum";
            if (indexAsset != null)
            {
                var index = JsonUtility.FromJson<MapIndex>(indexAsset.text);
                if (index != null && !string.IsNullOrEmpty(index.startup))
                {
                    startup = index.startup;
                }
            }

            return Load(startup);
        }

        public static TileKind ParseKind(string value, TileKind fallback = TileKind.Floor)
        {
            return Enum.TryParse(value, true, out TileKind kind) ? kind : fallback;
        }

        public static MaterialId ParseMaterial(string value, MaterialId fallback = MaterialId.Stone)
        {
            return Enum.TryParse(value, true, out MaterialId material) ? material : fallback;
        }

        public static RuneId ParseRune(string value)
        {
            return RuneCatalog.TryParseName(value, out var rune) ? rune : RuneId.None;
        }

        public static SpellId ParseSpell(string value)
        {
            return SpellRegistry.Parse(value);
        }

        public static Vector3 HeadingOf(string dir)
        {
            switch ((dir ?? string.Empty).ToLowerInvariant())
            {
                case "up":
                case "north":
                    return Vector3.up;
                case "down":
                case "south":
                    return Vector3.down;
                case "left":
                case "west":
                    return Vector3.left;
                default:
                    return Vector3.right;
            }
        }
    }
}
