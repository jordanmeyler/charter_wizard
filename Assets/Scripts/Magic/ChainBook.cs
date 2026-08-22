using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Matches a player string to a catalog story-chain.
    /// Charter wants the written order. Joins fold; via-forms
    /// expand. Free may also unscramble the same runes into a
    /// valid sentence, or fill missing ones up to a budget.
    /// </summary>
    public static class ChainBook
    {
        public readonly struct Birth
        {
            public Birth(RuneId rune, IReadOnlyList<RuneId> sources, string recipe, string extras)
            {
                Rune = rune;
                Sources = sources;
                Recipe = recipe;
                Extras = extras;
            }

            public RuneId Rune { get; }
            public IReadOnlyList<RuneId> Sources { get; }
            public string Recipe { get; }
            public string Extras { get; }
            public bool ElementalOnly => string.IsNullOrEmpty(Extras);
        }

        static readonly Dictionary<RuneId, RuneId[]> Births = new();
        static readonly Dictionary<string, SpellShape> Shapes = new();
        static readonly List<Birth> ElementalBirthCache = new();
        static readonly List<Birth> MixedBirthCache = new();
        static readonly List<Birth> AllBirthCache = new();
        static bool _birthCacheReady;

        static ChainBook()
        {
            SetBirth(RuneId.Spark, RuneId.Fire, RuneId.Air);
            SetBirth(RuneId.Lightning, RuneId.Spark, RuneId.Air);
            SetBirth(RuneId.Thunder, RuneId.Lightning, RuneId.Earth);
            SetBirth(RuneId.Cloud, RuneId.Air, RuneId.Water);
            SetBirth(RuneId.Storm, RuneId.Spark, RuneId.Cloud);
            SetBirth(RuneId.Rain, RuneId.Cloud, RuneId.Earth);
            SetBirth(RuneId.Steam, RuneId.Fire, RuneId.Water);
            SetBirth(RuneId.Lava, RuneId.Fire, RuneId.Earth);
            SetBirth(RuneId.Dust, RuneId.Air, RuneId.Earth);
            SetBirth(RuneId.Mud, RuneId.Water, RuneId.Earth);
            SetBirth(RuneId.Ice, RuneId.Water, RuneId.Salt, RuneId.Earth);
            SetBirth(RuneId.Stone, RuneId.Earth, RuneId.Salt);
            SetBirth(RuneId.Plant, RuneId.Water, RuneId.Earth, RuneId.Salt);
            SetBirth(RuneId.Grove, RuneId.Plant, RuneId.Vita);
            SetBirth(RuneId.Forest, RuneId.Plant, RuneId.Vita);
            SetBirth(RuneId.Flame, RuneId.Fire, RuneId.Sulphur, RuneId.Fire);
            SetBirth(RuneId.Ember, RuneId.Fire, RuneId.Mors);
            SetBirth(RuneId.Wind, RuneId.Air, RuneId.Mercury);
            SetBirth(RuneId.Current, RuneId.Water, RuneId.Mercury);
            SetBirth(RuneId.Shade, RuneId.Umbra, RuneId.Mors, RuneId.Salt);
            SetBirth(RuneId.Ash, RuneId.Fire, RuneId.Plant);
            SetBirth(RuneId.Obsidian, RuneId.Lava, RuneId.Water, RuneId.Salt);
            SetBirth(RuneId.Sand, RuneId.Dust, RuneId.Salt);
            SetBirth(RuneId.Glass, RuneId.Sand, RuneId.Flame, RuneId.Earth);
            SetBirth(RuneId.Blight, RuneId.Grove, RuneId.Mors);
            SetBirth(RuneId.Snow, RuneId.Cloud, RuneId.Ice);
            SetBirth(RuneId.Blizzard, RuneId.Wind, RuneId.Snow);
            SetBirth(RuneId.Vine, RuneId.Grove, RuneId.Mercury);
            SetBirth(RuneId.Metal, RuneId.Lava, RuneId.Earth);
            SetBirth(RuneId.Crystal, RuneId.Stone, RuneId.Water);
            SetBirth(RuneId.Glacier, RuneId.Ice, RuneId.Stone);
            SetBirth(RuneId.Acid, RuneId.Steam, RuneId.Metal);
            SetBirth(RuneId.Inferno, RuneId.Fire, RuneId.Fire, RuneId.Salt);
            SetBirth(RuneId.Plasma, RuneId.Inferno, RuneId.Spark);
            SetBirth(RuneId.Blizzard, RuneId.Wind, RuneId.Snow);
            SetBirth(RuneId.Sandstorm, RuneId.Wind, RuneId.Dust);
            SetBirth(RuneId.Aether, RuneId.Lumen, RuneId.Umbra);

            Shapes["shot"] = SpellShape.Shot;
            Shapes["pillar"] = SpellShape.Pillar;
            Shapes["spread"] = SpellShape.Spread;
            Shapes["remote"] = SpellShape.Remote;
            Shapes["self"] = SpellShape.Self;
        }

        static void SetBirth(RuneId rune, params RuneId[] sources)
        {
            if (rune == RuneId.None || sources == null || sources.Length == 0)
            {
                return;
            }

            Births[rune] = sources;
            _birthCacheReady = false;
        }

        public static void DefineBirth(RuneId rune, RuneId[] sources)
        {
            SetBirth(rune, sources);
        }

        public static IReadOnlyList<Birth> ElementalBirths
        {
            get
            {
                EnsureBirthCache();
                return ElementalBirthCache;
            }
        }

        public static IReadOnlyList<Birth> MixedBirths
        {
            get
            {
                EnsureBirthCache();
                return MixedBirthCache;
            }
        }

        public static IReadOnlyList<Birth> AllBirths
        {
            get
            {
                EnsureBirthCache();
                return AllBirthCache;
            }
        }

        public static int BirthCount
        {
            get
            {
                EnsureBirthCache();
                return ElementalBirthCache.Count + MixedBirthCache.Count;
            }
        }

        static void EnsureBirthCache()
        {
            if (_birthCacheReady)
            {
                return;
            }

            ElementalBirthCache.Clear();
            MixedBirthCache.Clear();
            AllBirthCache.Clear();
            foreach (var pair in Births)
            {
                var birth = DescribeBirth(pair.Key, pair.Value);
                if (birth.ElementalOnly)
                {
                    ElementalBirthCache.Add(birth);
                }
                else
                {
                    MixedBirthCache.Add(birth);
                }

                AllBirthCache.Add(birth);
            }

            ElementalBirthCache.Sort(CompareBirthName);
            MixedBirthCache.Sort(CompareBirthName);
            AllBirthCache.Sort(CompareBirthName);
            _birthCacheReady = true;
        }

        static Birth DescribeBirth(RuneId rune, IReadOnlyList<RuneId> sources)
        {
            return new Birth(rune, sources, JoinNames(sources), ExtraRoles(sources));
        }

        static int CompareBirthName(Birth left, Birth right)
        {
            return string.CompareOrdinal(RuneCatalog.NameOf(left.Rune), RuneCatalog.NameOf(right.Rune));
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

        public static string BirthNameText(RuneId rune)
        {
            return TryBirth(rune, out var sources) ? JoinNames(sources) : string.Empty;
        }

        public static string ExtraRoles(IReadOnlyList<RuneId> sources)
        {
            if (sources == null || sources.Count == 0)
            {
                return string.Empty;
            }

            var parts = new List<string>(sources.Count);
            for (var i = 0; i < sources.Count; i++)
            {
                var role = RuneCatalog.OperatorRole(sources[i]);
                if (role.Length > 0 && !parts.Contains(role))
                {
                    parts.Add(role);
                }
            }

            return string.Join(" · ", parts);
        }

        static string JoinNames(IReadOnlyList<RuneId> sources)
        {
            if (sources == null || sources.Count == 0)
            {
                return string.Empty;
            }

            var parts = new string[sources.Count];
            for (var i = 0; i < sources.Count; i++)
            {
                parts[i] = RuneCatalog.NameOf(sources[i]);
            }

            return string.Join(" · ", parts);
        }

        /// <summary>
        /// Unfolds a join to the eleven basics. Plant becomes Water, Earth,
        /// Salt. Ash becomes Fire, Water, Earth, Salt. Each ingredient is
        /// one column in the Charter weave.
        /// </summary>
        public static int ExpandRecipe(RuneId rune, List<RuneId> dest)
        {
            if (dest == null)
            {
                return 0;
            }

            var start = dest.Count;
            if (rune != RuneId.None)
            {
                AppendExpanded(dest, rune, 0);
            }

            return dest.Count - start;
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

        public static List<CodexEntry> CollectUnscrambled(Composition composition, SpellShape shape)
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
                    continue;
                }

                if (FillsNeeded(sequence, candidate) == 0)
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
            if (exact.Count > 0)
            {
                return exact;
            }

            var unscrambled = CollectUnscrambled(composition, shape);
            return unscrambled.Count > 0
                ? unscrambled
                : CollectFillable(composition, shape, fillBudget);
        }

        public static bool FitsWithFills(IReadOnlyList<RuneId> player, CodexEntry entry, int fillBudget)
        {
            var fills = FillsNeeded(player, entry);
            return fills > 0 && fills <= fillBudget;
        }

        public static bool IsScrambled(Composition composition, CodexEntry entry)
        {
            if (composition.Sequence == null || composition.Sequence.Length == 0)
            {
                return false;
            }

            return !Matches(entry, composition.Sequence) && FillsNeeded(composition.Sequence, entry) == 0;
        }

        /// <summary>
        /// How many recipe tokens Free must supply. 0 is a finished sentence
        /// (written in order, or the same runes in another order).
        /// -1 means the player string is not those runes.
        /// Raise FillBudget later and this same count still decides the fit.
        /// Free may also ignore order when counting fills, so Mercury · Air
        /// can still become Lightning if the budget covers Fire.
        /// </summary>
        public static int FillsNeeded(IReadOnlyList<RuneId> player, CodexEntry entry)
        {
            var recipe = CountFillsToward(player, entry.RecipeRunes);
            if (entry.ViaRunes.Count == 0)
            {
                return recipe;
            }

            var via = CountFillsToward(player, entry.ViaRunes);
            return BetterFit(recipe, via);
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
            var unscrambled = exact.Count > 0 ? null : CollectUnscrambled(composition, shape);
            var pool = exact.Count > 0
                ? exact
                : unscrambled != null && unscrambled.Count > 0
                    ? unscrambled
                    : CollectFillable(composition, shape, fillBudget);
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

            if (unscrambled != null && unscrambled.Count > 0)
            {
                return pool.Count == 1
                    ? $"Free unscrambles the runes into {names}"
                    : $"Free unscrambles the runes and picks among {names}";
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
                parts[i] = WorkingNames.RunePhrase(pool[i].RecipeRunes);
            }

            var text = string.Join(", ", parts);
            return pool.Count > cap ? text + $" and {pool.Count - cap} more" : text;
        }

        static int CountFillsToward(IReadOnlyList<RuneId> player, IReadOnlyList<RuneId> recipe)
        {
            var ordered = CountFills(Normalize(player), Normalize(recipe));
            var bag = CountBagFills(Expand(player), Expand(recipe));
            return BetterFit(ordered, bag);
        }

        static int BetterFit(int left, int right)
        {
            if (left < 0)
            {
                return right;
            }

            if (right < 0)
            {
                return left;
            }

            return left < right ? left : right;
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

        static int CountBagFills(IReadOnlyList<RuneId> player, IReadOnlyList<RuneId> recipe)
        {
            if (player == null || recipe == null || player.Count == 0 || recipe.Count == 0)
            {
                return -1;
            }

            if (player.Count > recipe.Count)
            {
                return -1;
            }

            var needed = new Dictionary<RuneId, int>();
            for (var i = 0; i < recipe.Count; i++)
            {
                needed.TryGetValue(recipe[i], out var count);
                needed[recipe[i]] = count + 1;
            }

            for (var i = 0; i < player.Count; i++)
            {
                if (!needed.TryGetValue(player[i], out var count) || count <= 0)
                {
                    return -1;
                }

                needed[player[i]] = count - 1;
            }

            return recipe.Count - player.Count;
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
                return $"{WorkingNames.RunePhrase(composition.Sequence)}{gate} · {SpellFormations.NameOf(named.Shape)} is written.";
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
