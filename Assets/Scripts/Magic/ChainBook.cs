using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Matches a player string to a catalog story-chain.
    /// Joins fold; via-forms expand to the same sentence.
    /// </summary>
    public static class ChainBook
    {
        static readonly Dictionary<RuneId, RuneId[]> Births = new();
        static readonly Dictionary<string, SpellShape> Shapes = new();

        static ChainBook()
        {
            Births[RuneId.Spark] = new[] { RuneId.Fire, RuneId.Air };
            Births[RuneId.Lightning] = new[] { RuneId.Spark, RuneId.Air };
            Births[RuneId.Thunder] = new[] { RuneId.Lightning, RuneId.Earth };
            Births[RuneId.Cloud] = new[] { RuneId.Air, RuneId.Water };
            Births[RuneId.Storm] = new[] { RuneId.Spark, RuneId.Cloud };
            Births[RuneId.Rain] = new[] { RuneId.Cloud, RuneId.Earth };
            Births[RuneId.Steam] = new[] { RuneId.Fire, RuneId.Water };
            Births[RuneId.Lava] = new[] { RuneId.Fire, RuneId.Earth };
            Births[RuneId.Dust] = new[] { RuneId.Air, RuneId.Earth };
            Births[RuneId.Mud] = new[] { RuneId.Water, RuneId.Earth };
            Births[RuneId.Ice] = new[] { RuneId.Water, RuneId.Salt, RuneId.Earth };
            Births[RuneId.Stone] = new[] { RuneId.Earth, RuneId.Salt };
            Births[RuneId.Plant] = new[] { RuneId.Water, RuneId.Earth, RuneId.Salt };
            Births[RuneId.Grove] = new[] { RuneId.Plant, RuneId.Vita };
            Births[RuneId.Forest] = new[] { RuneId.Plant, RuneId.Vita };
            Births[RuneId.Flame] = new[] { RuneId.Fire, RuneId.Salt };
            Births[RuneId.Ember] = new[] { RuneId.Fire, RuneId.Mors };
            Births[RuneId.Wind] = new[] { RuneId.Air, RuneId.Mercury };
            Births[RuneId.Current] = new[] { RuneId.Water, RuneId.Mercury };
            Births[RuneId.Shade] = new[] { RuneId.Umbra, RuneId.Mors, RuneId.Salt };
            Births[RuneId.Ash] = new[] { RuneId.Fire, RuneId.Plant };
            Births[RuneId.Obsidian] = new[] { RuneId.Lava, RuneId.Water, RuneId.Salt };
            Births[RuneId.Sand] = new[] { RuneId.Dust, RuneId.Salt };
            Births[RuneId.Glass] = new[] { RuneId.Sand, RuneId.Flame, RuneId.Earth };
            Births[RuneId.Blight] = new[] { RuneId.Grove, RuneId.Mors };
            Births[RuneId.Snow] = new[] { RuneId.Cloud, RuneId.Ice };
            Births[RuneId.Vine] = new[] { RuneId.Grove, RuneId.Mercury };
            Births[RuneId.Metal] = new[] { RuneId.Lava, RuneId.Earth };
            Births[RuneId.Crystal] = new[] { RuneId.Stone, RuneId.Water };
            Births[RuneId.Glacier] = new[] { RuneId.Ice, RuneId.Stone };
            Births[RuneId.Acid] = new[] { RuneId.Steam, RuneId.Metal };
            Births[RuneId.Inferno] = new[] { RuneId.Fire, RuneId.Fire, RuneId.Salt };

            Shapes["shot"] = SpellShape.Shot;
            Shapes["pillar"] = SpellShape.Pillar;
            Shapes["spread"] = SpellShape.Spread;
            Shapes["remote"] = SpellShape.Remote;
            Shapes["self"] = SpellShape.Self;
        }

        public static bool IsWrought(RuneId rune)
        {
            return rune != RuneId.None && Births.ContainsKey(rune);
        }

        public static bool TryBirth(RuneId rune, out IReadOnlyList<RuneId> sources)
        {
            if (Births.TryGetValue(rune, out var parts))
            {
                sources = parts;
                return true;
            }

            sources = System.Array.Empty<RuneId>();
            return false;
        }

        public static string BirthText(RuneId rune)
        {
            if (!TryBirth(rune, out var sources) || sources.Count == 0)
            {
                return string.Empty;
            }

            var parts = new string[sources.Count];
            for (var i = 0; i < sources.Count; i++)
            {
                parts[i] = RuneCatalog.GlyphOf(sources[i]);
            }

            return string.Join(" · ", parts);
        }

        public static bool TryParseShape(string name, out SpellShape shape)
        {
            shape = SpellShape.None;
            return !string.IsNullOrWhiteSpace(name) &&
                   Shapes.TryGetValue(name.Trim().ToLowerInvariant(), out shape);
        }

        public static List<RuneId> Parse(string chain)
        {
            var list = new List<RuneId>();
            if (string.IsNullOrWhiteSpace(chain))
            {
                return list;
            }

            var parts = chain.Split('·');
            for (var i = 0; i < parts.Length; i++)
            {
                if (RuneCatalog.TryParseName(parts[i], out var rune) && rune != RuneId.None)
                {
                    list.Add(rune);
                }
            }

            return list;
        }

        public static List<RuneId> Normalize(IReadOnlyList<RuneId> tokens)
        {
            return Fold(Expand(tokens));
        }

        public static bool SameStory(IReadOnlyList<RuneId> left, IReadOnlyList<RuneId> right)
        {
            var a = Normalize(left);
            var b = Normalize(right);
            if (a.Count == 0 || a.Count != b.Count)
            {
                return false;
            }

            for (var i = 0; i < a.Count; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static bool Matches(CodexEntry entry, IReadOnlyList<RuneId> sequence)
        {
            if (sequence == null || sequence.Count == 0)
            {
                return false;
            }

            if (SameStory(sequence, entry.RecipeRunes))
            {
                return true;
            }

            return entry.ViaRunes.Count > 0 && SameStory(sequence, entry.ViaRunes);
        }

        public static bool TryMatch(Composition composition, SpellShape shape, out CodexEntry entry)
        {
            entry = default;
            var sequence = composition.Sequence;
            if (sequence == null || sequence.Length == 0)
            {
                return false;
            }

            foreach (var candidate in SpellCodex.All)
            {
                if (shape != SpellShape.None && candidate.Shape != shape)
                {
                    continue;
                }

                if (Matches(candidate, sequence))
                {
                    entry = candidate;
                    return true;
                }
            }

            return false;
        }

        public static List<SpellShape> ShapesFor(Composition composition)
        {
            return ShapesFor(CollectExact(composition, SpellShape.None));
        }

        public static List<SpellShape> ShapesFor(IReadOnlyList<CodexEntry> entries)
        {
            var shapes = new List<SpellShape>();
            if (entries == null)
            {
                return shapes;
            }

            for (var i = 0; i < entries.Count; i++)
            {
                var shape = entries[i].Shape;
                if (shape != SpellShape.None && !shapes.Contains(shape))
                {
                    shapes.Add(shape);
                }
            }

            return shapes;
        }

        public static List<CodexEntry> CollectExact(Composition composition, SpellShape shape)
        {
            var matches = new List<CodexEntry>();
            var sequence = composition.Sequence;
            if (sequence == null || sequence.Length == 0)
            {
                return matches;
            }

            foreach (var candidate in SpellCodex.All)
            {
                if (shape != SpellShape.None && candidate.Shape != shape)
                {
                    continue;
                }

                if (Matches(candidate, sequence))
                {
                    AddUnique(matches, candidate);
                }
            }

            return matches;
        }

        public static List<CodexEntry> CollectFillable(
            Composition composition,
            SpellShape shape,
            int fillBudget)
        {
            var matches = new List<CodexEntry>();
            var sequence = composition.Sequence;
            if (sequence == null || sequence.Length == 0 || fillBudget < 0)
            {
                return matches;
            }

            var player = Normalize(sequence);
            foreach (var candidate in SpellCodex.All)
            {
                if (shape != SpellShape.None && candidate.Shape != shape)
                {
                    continue;
                }

                if (FitsWithFills(player, candidate, fillBudget))
                {
                    AddUnique(matches, candidate);
                }
            }

            return matches;
        }

        public static List<CodexEntry> CollectForFree(
            Composition composition,
            SpellShape shape,
            int fillBudget)
        {
            var exact = CollectExact(composition, shape);
            return exact.Count > 0 ? exact : CollectFillable(composition, shape, fillBudget);
        }

        public static bool FitsWithFills(IReadOnlyList<RuneId> player, CodexEntry entry, int fillBudget)
        {
            var fills = FillsNeeded(player, entry);
            return fills >= 0 && fills <= fillBudget;
        }

        /// <summary>
        /// How many recipe tokens Free must supply. 0 is an exact sentence.
        /// -1 means the player string is not a subsequence of the recipe or via.
        /// Raise FillBudget later and this same count still decides the fit.
        /// </summary>
        public static int FillsNeeded(IReadOnlyList<RuneId> player, CodexEntry entry)
        {
            var normalized = Normalize(player);
            var recipe = CountFills(normalized, Normalize(entry.RecipeRunes));
            if (entry.ViaRunes.Count == 0)
            {
                return recipe;
            }

            var via = CountFills(normalized, Normalize(entry.ViaRunes));
            if (recipe < 0)
            {
                return via;
            }

            if (via < 0)
            {
                return recipe;
            }

            return recipe < via ? recipe : via;
        }

        public static int FillsNeeded(Composition composition, CodexEntry entry)
        {
            if (composition.Sequence == null || composition.Sequence.Length == 0)
            {
                return -1;
            }

            return FillsNeeded(composition.Sequence, entry);
        }

        public static string PreviewFree(Composition composition, int fillBudget, SpellShape shape = SpellShape.None)
        {
            var exact = CollectExact(composition, shape);
            var pool = exact.Count > 0 ? exact : CollectFillable(composition, shape, fillBudget);
            if (pool.Count == 0)
            {
                return string.Empty;
            }

            var names = JoinNames(pool);
            if (exact.Count > 0)
            {
                return pool.Count == 1
                    ? $"Free takes the finished sentence: {names}"
                    : $"Free takes a finished sentence among {names}";
            }

            var blank = fillBudget == 1 ? "a blank" : fillBudget + " blanks";
            return pool.Count == 1
                ? $"Free fills {blank} and the chain becomes {names}"
                : $"Free fills {blank} and picks among {names}";
        }

        static string JoinNames(IReadOnlyList<CodexEntry> pool)
        {
            const int cap = 4;
            var take = pool.Count < cap ? pool.Count : cap;
            var parts = new string[take];
            for (var i = 0; i < take; i++)
            {
                parts[i] = pool[i].Name;
            }

            var text = string.Join(", ", parts);
            return pool.Count > cap ? text + $" and {pool.Count - cap} more" : text;
        }

        static int CountFills(IReadOnlyList<RuneId> player, IReadOnlyList<RuneId> recipe)
        {
            if (player == null || recipe == null || player.Count == 0 || recipe.Count == 0)
            {
                return -1;
            }

            if (player.Count > recipe.Count)
            {
                return -1;
            }

            var i = 0;
            var skipped = 0;
            for (var j = 0; j < recipe.Count; j++)
            {
                if (i < player.Count && player[i] == recipe[j])
                {
                    i++;
                    continue;
                }

                skipped++;
            }

            return i == player.Count ? skipped : -1;
        }

        static void AddUnique(List<CodexEntry> matches, CodexEntry entry)
        {
            for (var i = 0; i < matches.Count; i++)
            {
                if (matches[i].Spell == entry.Spell)
                {
                    return;
                }
            }

            matches.Add(entry);
        }

        public static string Preview(Composition composition, SpellShape shape = SpellShape.None)
        {
            if (TryMatch(composition, shape, out var named))
            {
                var gate = string.IsNullOrEmpty(named.Gate) ? string.Empty : $" ({named.Gate})";
                return $"{named.Name}{gate} · {SpellFormations.NameOf(named.Shape)} is written.";
            }

            return string.Empty;
        }

        static List<RuneId> Expand(IReadOnlyList<RuneId> tokens)
        {
            var result = new List<RuneId>();
            if (tokens == null)
            {
                return result;
            }

            for (var i = 0; i < tokens.Count; i++)
            {
                AppendExpanded(result, tokens[i], 0);
            }

            return result;
        }

        static void AppendExpanded(List<RuneId> result, RuneId rune, int depth)
        {
            if (depth > 8 || !Births.TryGetValue(rune, out var parts))
            {
                result.Add(rune);
                return;
            }

            for (var i = 0; i < parts.Length; i++)
            {
                AppendExpanded(result, parts[i], depth + 1);
            }
        }

        static List<RuneId> Fold(List<RuneId> tokens)
        {
            var result = new List<RuneId>();
            for (var i = 0; i < tokens.Count; i++)
            {
                var next = tokens[i];
                if (result.Count > 0 &&
                    MaterialTree.TryBlend(result[result.Count - 1], next, out var blend))
                {
                    result[result.Count - 1] = blend.Result;
                }
                else
                {
                    result.Add(next);
                }
            }

            return result;
        }
    }
}
