using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Concentration. Wards and mind ailments are held by the sentence
    /// that wrote them, not by a clock. A later spell that reuses any
    /// of those marks lets the working go. Elemental work (a wall, a
    /// flame, burning, poison, ice) stands on its own.
    /// </summary>
    public static class FocusLaw
    {
        public static bool Breaks(StatusId held, SpellId next)
        {
            if (!StatusSpec.Of(held).NeedsConcentration || next == SpellId.None)
            {
                return false;
            }

            return Overlaps(DefaultRunes(held), UsedRunes(next, default));
        }

        public static int Release(Component caster, SpellId spell, Composition composition, StatusId incoming)
        {
            return StatusHost.ReleaseAll(caster, UsedRunes(spell, composition), incoming, spell);
        }

        public static IReadOnlyList<RuneId> DefaultRunes(StatusId id)
        {
            var spell = SpellOf(id);
            return spell == SpellId.None
                ? System.Array.Empty<RuneId>()
                : UsedRunes(spell, default);
        }

        public static List<RuneId> UsedRunes(SpellId spell, Composition composition)
        {
            var tokens = new List<RuneId>(12);
            if (SpellCodex.TryGet(spell, out var entry))
            {
                Add(tokens, entry.RecipeRunes);
                Add(tokens, entry.ViaRunes);
            }

            if (composition.Sequence != null && composition.Sequence.Length > 0)
            {
                Add(tokens, composition.Sequence);
            }
            else
            {
                Push(tokens, composition.MaterialA);
                Push(tokens, composition.MaterialB);
                Push(tokens, composition.Aspect);
            }

            var used = new List<RuneId>(tokens.Count * 2);
            for (var i = 0; i < tokens.Count; i++)
            {
                Push(used, tokens[i]);
                ChainBook.ExpandRecipe(tokens[i], used);
            }

            return Unique(used);
        }

        public static bool Overlaps(IReadOnlyList<RuneId> left, IReadOnlyList<RuneId> right)
        {
            if (left == null || right == null)
            {
                return false;
            }

            for (var i = 0; i < left.Count; i++)
            {
                if (Contains(right, left[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Contains(IReadOnlyList<RuneId> runes, RuneId wanted)
        {
            if (runes == null || wanted == RuneId.None)
            {
                return false;
            }

            for (var i = 0; i < runes.Count; i++)
            {
                if (runes[i] == wanted)
                {
                    return true;
                }
            }

            return false;
        }

        static SpellId SpellOf(StatusId id)
        {
            switch (id)
            {
                case StatusId.Sleeping: return SpellId.Lull;
                case StatusId.Raging: return SpellId.Rage;
                case StatusId.Charmed: return SpellId.Charm;
                case StatusId.Confused: return SpellId.Confuse;
                case StatusId.Frightened: return SpellId.Terror;
                case StatusId.Stoneskin: return SpellId.Stoneskin;
                case StatusId.Watershield: return SpellId.Watershield;
                case StatusId.Flameward: return SpellId.Flameward;
                case StatusId.Windward: return SpellId.Windward;
                default: return SpellId.None;
            }
        }

        static void Add(List<RuneId> dest, IReadOnlyList<RuneId> source)
        {
            if (source == null)
            {
                return;
            }

            for (var i = 0; i < source.Count; i++)
            {
                Push(dest, source[i]);
            }
        }

        static void Push(List<RuneId> dest, RuneId rune)
        {
            if (rune != RuneId.None)
            {
                dest.Add(rune);
            }
        }

        static List<RuneId> Unique(List<RuneId> runes)
        {
            var unique = new List<RuneId>(runes.Count);
            for (var i = 0; i < runes.Count; i++)
            {
                if (runes[i] != RuneId.None && !unique.Contains(runes[i]))
                {
                    unique.Add(runes[i]);
                }
            }

            return unique;
        }
    }
}
