using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Search and rune-filter for the Grimoire. Empty query and
    /// <see cref="RuneId.None"/> mean show everything.
    /// </summary>
    public static class GrimoireQuery
    {
        public static readonly RuneId[] FilterRunes =
        {
            RuneId.Fire, RuneId.Air, RuneId.Earth, RuneId.Water,
            RuneId.Salt, RuneId.Mercury, RuneId.Sulphur,
            RuneId.Vita, RuneId.Mors, RuneId.Lumen, RuneId.Umbra
        };

        public static bool TextMatches(string query, params string[] fields)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return true;
            }

            var needle = query.Trim();
            if (fields == null)
            {
                return false;
            }

            for (var i = 0; i < fields.Length; i++)
            {
                var field = fields[i];
                if (!string.IsNullOrEmpty(field)
                    && field.IndexOf(needle, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Involves(RuneId filter, RuneId rune)
        {
            if (filter == RuneId.None)
            {
                return true;
            }

            if (rune == filter)
            {
                return true;
            }

            if (ChainBook.TryBirth(rune, out var sources) && Contains(sources, filter))
            {
                return true;
            }

            return false;
        }

        public static bool Involves(RuneId filter, IReadOnlyList<RuneId> runes)
        {
            if (filter == RuneId.None)
            {
                return true;
            }

            if (runes == null)
            {
                return false;
            }

            for (var i = 0; i < runes.Count; i++)
            {
                if (Involves(filter, runes[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool MatchesRune(RuneId rune, string query, RuneId filter)
        {
            if (rune == RuneId.None || !Involves(filter, rune))
            {
                return false;
            }

            RuneCatalog.TryGet(rune, out var def);
            var meaning = def.Meaning ?? string.Empty;
            return TextMatches(
                query,
                RuneCatalog.NameOf(rune),
                meaning,
                ChainBook.BirthNameText(rune),
                ChainBook.BirthText(rune));
        }

        public static bool MatchesSpell(CodexEntry entry, string query, RuneId filter)
        {
            if (!Involves(filter, entry.RecipeRunes) && !Involves(filter, entry.ViaRunes))
            {
                return false;
            }

            return TextMatches(
                query,
                entry.Name,
                entry.Want,
                entry.Recipe,
                entry.Via,
                entry.Form,
                entry.Gate,
                SpellCodex.BookName(entry.Book),
                entry.Number.ToString(),
                WorkingNames.RunePhrase(entry.RecipeRunes),
                WorkingNames.RunePhrase(entry.ViaRunes));
        }

        public static bool MatchesMaterial(WorldMaterial material, string query, RuneId filter)
        {
            if (material == null)
            {
                return false;
            }

            if (filter != RuneId.None
                && !Involves(filter, material.Signature)
                && material.Manifestation != filter)
            {
                return false;
            }

            return TextMatches(
                query,
                material.Name,
                material.Note,
                material.SignatureNames(),
                material.SignatureText(),
                RuneCatalog.NameOf(material.Manifestation));
        }

        public static bool MatchesWorking(KeptWorking working, string query, RuneId filter)
        {
            if (!Involves(filter, working.Runes))
            {
                return false;
            }

            var spellName = string.Empty;
            if (working.Spell != SpellId.None && SpellCodex.TryGet(working.Spell, out var entry))
            {
                spellName = entry.Name;
            }

            return TextMatches(
                query,
                working.Label,
                working.GivenName,
                spellName,
                WorkingNames.RunePhrase(working.Runes));
        }

        static bool Contains(IReadOnlyList<RuneId> runes, RuneId needle)
        {
            if (runes == null)
            {
                return false;
            }

            for (var i = 0; i < runes.Count; i++)
            {
                if (runes[i] == needle)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
