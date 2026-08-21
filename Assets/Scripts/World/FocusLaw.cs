using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Wards and mind ailments are held by focus, not by a clock.
    /// Elemental work (burning, poison, frozen) stands on its own.
    /// Using the held rune again lets the working go — a little fizzle.
    /// </summary>
    public static class FocusLaw
    {
        public static bool Breaks(StatusId held, SpellId next)
        {
            var spec = StatusSpec.Of(held);
            if (!spec.NeedsFocus || spec.FocusRune == RuneId.None || next == SpellId.None)
            {
                return false;
            }

            return Contains(UsedRunes(next, default), spec.FocusRune);
        }

        public static int Release(Component caster, SpellId spell, Composition composition, StatusId incoming)
        {
            return StatusHost.ReleaseAll(caster, UsedRunes(spell, composition), incoming);
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
