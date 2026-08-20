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

            Shapes["shot"] = SpellShape.Shot;
            Shapes["pillar"] = SpellShape.Pillar;
            Shapes["spread"] = SpellShape.Spread;
            Shapes["remote"] = SpellShape.Remote;
            Shapes["self"] = SpellShape.Self;
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
            var shapes = new List<SpellShape>();
            var sequence = composition.Sequence;
            if (sequence == null || sequence.Length == 0)
            {
                return shapes;
            }

            foreach (var candidate in SpellCodex.All)
            {
                if (!Matches(candidate, sequence) || candidate.Shape == SpellShape.None)
                {
                    continue;
                }

                if (!shapes.Contains(candidate.Shape))
                {
                    shapes.Add(candidate.Shape);
                }
            }

            return shapes;
        }

        public static string Preview(Composition composition, SpellShape shape = SpellShape.None)
        {
            if (TryMatch(composition, shape, out var named) && shape != SpellShape.None)
            {
                var gate = string.IsNullOrEmpty(named.Gate) ? string.Empty : $" ({named.Gate})";
                return $"{named.Name}{gate}";
            }

            var shapes = ShapesFor(composition);
            if (shapes.Count == 0)
            {
                return string.Empty;
            }

            if (TryMatch(composition, shapes[0], out var first))
            {
                if (shapes.Count == 1)
                {
                    return $"{first.Name} · {SpellFormations.NameOf(first.Shape)} is written. Cast to aim.";
                }

                return $"{first.Name} and {shapes.Count - 1} more written form(s). Choose how it aims.";
            }

            return "A written chain. Cast to aim.";
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
