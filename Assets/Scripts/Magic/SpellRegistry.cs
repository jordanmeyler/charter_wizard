using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Turns a catalog name into a <see cref="SpellId"/>. Built-in names
    /// match the enum. New names get a stable runtime id so a JSON recipe
    /// can be a lock key without a code change.
    /// </summary>
    public static class SpellRegistry
    {
        static readonly Dictionary<string, SpellId> ByName = new();
        static readonly Dictionary<SpellId, string> ById = new();
        static int Next = 1000;

        public static SpellId Parse(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return SpellId.None;
            }

            var key = Normalize(name);
            if (ByName.TryGetValue(key, out var known))
            {
                return known;
            }

            if (key.Equals("wind", System.StringComparison.OrdinalIgnoreCase))
            {
                Remember(key, SpellId.Gust);
                return SpellId.Gust;
            }

            if (key.Equals("vinerise", System.StringComparison.OrdinalIgnoreCase)
                || key.Equals("grow", System.StringComparison.OrdinalIgnoreCase))
            {
                Remember(key, SpellId.Grow);
                return SpellId.Grow;
            }

            if (key.Equals("grotto", System.StringComparison.OrdinalIgnoreCase)
                || key.Equals("wither", System.StringComparison.OrdinalIgnoreCase))
            {
                Remember(key, SpellId.Wither);
                return SpellId.Wither;
            }

            if (key.Equals("poisonspray", System.StringComparison.OrdinalIgnoreCase))
            {
                Remember(key, SpellId.Poison);
                return SpellId.Poison;
            }

            if (System.Enum.TryParse(key, true, out SpellId enumerated) && enumerated != SpellId.None)
            {
                Remember(key, enumerated);
                return enumerated;
            }

            var minted = (SpellId)Next++;
            Remember(key, minted);
            return minted;
        }

        public static string NameOf(SpellId spell)
        {
            if (spell == SpellId.None)
            {
                return "none";
            }

            return ById.TryGetValue(spell, out var name) ? name : spell.ToString();
        }

        static void Remember(string key, SpellId id)
        {
            ByName[key] = id;
            if (!ById.ContainsKey(id))
            {
                ById[id] = key;
            }
        }

        static string Normalize(string name)
        {
            var chars = new char[name.Length];
            var n = 0;
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (c == ' ' || c == '-' || c == '_')
                {
                    continue;
                }

                chars[n++] = c;
            }

            return new string(chars, 0, n);
        }
    }
}
