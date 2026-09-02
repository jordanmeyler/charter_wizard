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

            if (key.Equals("graveice", System.StringComparison.OrdinalIgnoreCase)
                || key.Equals("darkcrystal", System.StringComparison.OrdinalIgnoreCase)
                || key.Equals("dark-crystal", System.StringComparison.OrdinalIgnoreCase))
            {
                Remember(key, SpellId.DarkCrystal);
                return SpellId.DarkCrystal;
            }

            if (key.Equals("deathcloud", System.StringComparison.OrdinalIgnoreCase)
                || key.Equals("death-cloud", System.StringComparison.OrdinalIgnoreCase))
            {
                Remember(key, SpellId.DeathCloud);
                return SpellId.DeathCloud;
            }

            if (key.Equals("airwall", System.StringComparison.OrdinalIgnoreCase)
                || key.Equals("air-wall", System.StringComparison.OrdinalIgnoreCase))
            {
                Remember(key, SpellId.AirWall);
                return SpellId.AirWall;
            }

            if (key.Equals("wort", System.StringComparison.OrdinalIgnoreCase)
                || key.Equals("remedy", System.StringComparison.OrdinalIgnoreCase))
            {
                Remember(key, SpellId.Wort);
                return SpellId.Wort;
            }

            if (key.Equals("grovecure", System.StringComparison.OrdinalIgnoreCase)
                || key.Equals("grove-cure", System.StringComparison.OrdinalIgnoreCase))
            {
                Remember(key, SpellId.GroveCure);
                return SpellId.GroveCure;
            }

            if (key.Equals("sunorb", System.StringComparison.OrdinalIgnoreCase)
                || key.Equals("sun-orb", System.StringComparison.OrdinalIgnoreCase))
            {
                Remember(key, SpellId.SunOrb);
                return SpellId.SunOrb;
            }

            if (key.Equals("nightshade", System.StringComparison.OrdinalIgnoreCase)
                || key.Equals("venombloom", System.StringComparison.OrdinalIgnoreCase)
                || key.Equals("venom-bloom", System.StringComparison.OrdinalIgnoreCase))
            {
                Remember(key, SpellId.Nightshade);
                return SpellId.Nightshade;
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
