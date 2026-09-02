using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Resolves the working an altar shows. Author a catalog name
    /// (Fireball) or a written chain (Fire · Mercury). Leave the
    /// field empty and prayer offers a written spell that is not
    /// already in the book.
    /// </summary>
    public static class PrayerReveal
    {
        public static bool TryResolve(string authored, Grimoire book, out CodexEntry entry)
        {
            if (!string.IsNullOrWhiteSpace(authored))
            {
                return TryNamed(authored, out entry) || TryRecipe(authored, out entry);
            }

            return TryUnkept(book, out entry);
        }

        public static bool TryNamed(string authored, out CodexEntry entry)
        {
            entry = default;
            if (string.IsNullOrWhiteSpace(authored))
            {
                return false;
            }

            var key = Fold(authored);
            foreach (var candidate in SpellCodex.All)
            {
                if (Fold(candidate.Name) == key || Fold(candidate.Spell.ToString()) == key)
                {
                    entry = candidate;
                    return true;
                }
            }

            return System.Enum.TryParse(key, true, out SpellId enumerated)
                && enumerated != SpellId.None
                && SpellCodex.TryGet(enumerated, out entry);
        }

        public static bool TryRecipe(string authored, out CodexEntry entry)
        {
            entry = default;
            var runes = ChainBook.Parse(authored);
            if (runes.Count == 0)
            {
                return false;
            }

            return ChainBook.TryMatch(Composition.FromSequence(runes), SpellShape.None, out entry);
        }

        public static bool TryUnkept(Grimoire book, out CodexEntry entry)
        {
            entry = default;
            foreach (var candidate in SpellCodex.All)
            {
                if (candidate.FreeOnly)
                {
                    continue;
                }

                if (book == null || !book.KeepsComposition(candidate.RecipeRunes))
                {
                    entry = candidate;
                    return true;
                }
            }

            if (SpellCodex.All.Count == 0)
            {
                return false;
            }

            entry = SpellCodex.All[0];
            return true;
        }

        public static IReadOnlyList<string> RolesOf(IReadOnlyList<RuneId> runes)
        {
            if (runes == null || runes.Count == 0)
            {
                return System.Array.Empty<string>();
            }

            var roles = new string[runes.Count];
            for (var i = 0; i < runes.Count; i++)
            {
                roles[i] = RuneCatalog.StringRole(runes[i]);
            }

            return roles;
        }

        static string Fold(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var chars = value.Trim().ToLowerInvariant().ToCharArray();
            var n = 0;
            for (var i = 0; i < chars.Length; i++)
            {
                var c = chars[i];
                if (c == '-' || c == '_' || c == ' ' || c == '·' || c == '.')
                {
                    continue;
                }

                chars[n++] = c;
            }

            return n == chars.Length ? new string(chars) : new string(chars, 0, n);
        }
    }
}
