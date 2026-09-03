using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// The working an altar teaches. Authors set the recipe runes —
    /// names are not locked, and the same spell can be written more
    /// than one way. The shown chain is what prayer offers to cast.
    /// </summary>
    public readonly struct PrayerWorking
    {
        public PrayerWorking(
            CodexEntry entry,
            IReadOnlyList<RuneId> recipe,
            IReadOnlyList<RuneId> via)
        {
            Entry = entry;
            Recipe = recipe ?? System.Array.Empty<RuneId>();
            Via = via ?? System.Array.Empty<RuneId>();
        }

        public CodexEntry Entry { get; }
        public IReadOnlyList<RuneId> Recipe { get; }
        public IReadOnlyList<RuneId> Via { get; }

        public bool HasRecipe => HasMarks(Recipe);
        public bool HasVia => HasMarks(Via) && !WorkingNames.SameComposition(Recipe, Via);

        public static bool HasMarks(IReadOnlyList<RuneId> runes)
        {
            if (runes == null)
            {
                return false;
            }

            for (var i = 0; i < runes.Count; i++)
            {
                if (runes[i] != RuneId.None)
                {
                    return true;
                }
            }

            return false;
        }

        public static RuneId[] Copy(IReadOnlyList<RuneId> runes)
        {
            if (runes == null || runes.Count == 0)
            {
                return System.Array.Empty<RuneId>();
            }

            var copy = new RuneId[runes.Count];
            var n = 0;
            for (var i = 0; i < runes.Count; i++)
            {
                if (runes[i] != RuneId.None)
                {
                    copy[n++] = runes[i];
                }
            }

            if (n == 0)
            {
                return System.Array.Empty<RuneId>();
            }

            if (n != copy.Length)
            {
                System.Array.Resize(ref copy, n);
            }

            return copy;
        }
    }

    /// <summary>
    /// Resolves the working an altar shows. Set the recipe runes
    /// (and an optional second writing). A leftover catalog name or
    /// written chain still resolves. Leave everything empty and
    /// prayer offers a written spell that is not already in the book.
    /// </summary>
    public static class PrayerReveal
    {
        /// <param name="showOther">
        /// When false, prayer keeps only the authored recipe —
        /// Earth-pillar stays Earth · Salt, without Stone.
        /// </param>
        public static bool TryResolve(
            IReadOnlyList<RuneId> recipe,
            IReadOnlyList<RuneId> via,
            string authored,
            Grimoire book,
            out PrayerWorking working,
            bool showOther = true)
        {
            var shown = PrayerWorking.Copy(recipe);
            var other = showOther ? PrayerWorking.Copy(via) : System.Array.Empty<RuneId>();
            if (shown.Length == 0 && other.Length > 0)
            {
                shown = other;
                other = System.Array.Empty<RuneId>();
            }

            if (shown.Length > 0)
            {
                TryMatchRunes(shown, out var entry);
                if (showOther && other.Length == 0)
                {
                    other = OtherWriting(entry, shown);
                }

                working = new PrayerWorking(entry, shown, other);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(authored))
            {
                if (TryNamed(authored, out var named) || TryRecipe(authored, out named))
                {
                    working = Shown(named, showOther);
                    return true;
                }

                working = default;
                return false;
            }

            if (!TryUnkept(book, out var unkept))
            {
                working = default;
                return false;
            }

            working = Shown(unkept, showOther);
            return true;
        }

        public static bool TryResolve(string authored, Grimoire book, out CodexEntry entry)
        {
            if (!TryResolve(null, null, authored, book, out var working) || !working.HasRecipe)
            {
                entry = default;
                return false;
            }

            entry = working.Entry;
            return working.Entry.Spell != SpellId.None || PrayerWorking.HasMarks(working.Entry.RecipeRunes);
        }

        public static PrayerWorking FromEntry(CodexEntry entry)
        {
            return Shown(entry, true);
        }

        static PrayerWorking Shown(CodexEntry entry, bool showOther)
        {
            return showOther
                ? new PrayerWorking(entry, entry.RecipeRunes, entry.ViaRunes)
                : new PrayerWorking(entry, entry.RecipeRunes, System.Array.Empty<RuneId>());
        }

        public static bool TryMatchRunes(IReadOnlyList<RuneId> runes, out CodexEntry entry)
        {
            entry = default;
            if (!PrayerWorking.HasMarks(runes))
            {
                return false;
            }

            return ChainBook.TryMatch(Composition.FromSequence(runes), SpellShape.None, out entry);
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

        static RuneId[] OtherWriting(CodexEntry entry, IReadOnlyList<RuneId> shown)
        {
            if (entry.RecipeRunes == null || entry.RecipeRunes.Count == 0)
            {
                return System.Array.Empty<RuneId>();
            }

            if (!WorkingNames.SameComposition(shown, entry.RecipeRunes))
            {
                return PrayerWorking.Copy(entry.RecipeRunes);
            }

            return entry.ViaRunes != null && entry.ViaRunes.Count > 0
                ? PrayerWorking.Copy(entry.ViaRunes)
                : System.Array.Empty<RuneId>();
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
