using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Focus holds mind spells. Charm, command, lull, rage, terror,
    /// confuse, and the wards (they all write Sulphur) stay
    /// until another sentence reuses a mark from the held working.
    /// Burning is a contact meter. Poison is a slower meter that
    /// stays until Light cleanses it.
    /// Other elemental work (a wall, frost, stun) stands on its own clock.
    /// </summary>
    public static class FocusLaw
    {
        public static bool IsMindSpell(SpellId spell)
        {
            if (spell == SpellId.None)
            {
                return false;
            }

            var status = SpellVerb.Of(spell).Status;
            if (StatusSpec.IsMindAilment(status) || StatusSpec.Of(status).IsStance)
            {
                return true;
            }

            return SpellCodex.TryGet(spell, out var entry) && entry.Book == SpellBook.Mind;
        }

        public static bool Holds(StatusId id) =>
            StatusSpec.Of(id).NeedsFocus;

        public static bool Breaks(StatusId held, SpellId next)
        {
            if (!Holds(held) || next == SpellId.None)
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
                case StatusId.Plantward: return SpellId.Plantward;
                case StatusId.FlameForm: return SpellId.FlameForm;
                case StatusId.TideForm: return SpellId.TideForm;
                case StatusId.StoneForm: return SpellId.StoneForm;
                case StatusId.GaleForm: return SpellId.GaleForm;
                case StatusId.GroveForm: return SpellId.GroveForm;
                case StatusId.CloudForm: return SpellId.CloudForm;
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

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (!IsMindSpell(SpellId.Charm)
                || !IsMindSpell(SpellId.Command)
                || !IsMindSpell(SpellId.Lull)
                || !IsMindSpell(SpellId.Rage)
                || !IsMindSpell(SpellId.Terror)
                || !IsMindSpell(SpellId.Confuse))
            {
                broken.Add("Focus must hold the mind sentences");
            }

            if (!IsMindSpell(SpellId.Stoneskin)
                || !IsMindSpell(SpellId.Watershield)
                || !IsMindSpell(SpellId.Flameward)
                || !IsMindSpell(SpellId.Windward)
                || !IsMindSpell(SpellId.Plantward)
                || !IsMindSpell(SpellId.FlameForm)
                || !IsMindSpell(SpellId.GaleForm)
                || !IsMindSpell(SpellId.CloudForm)
                || IsMindSpell(SpellId.Wall)
                || IsMindSpell(SpellId.Fireball))
            {
                broken.Add("Wards and forms are mind spells; walls and fireballs are not");
            }

            if (!Holds(StatusId.Stoneskin)
                || !Holds(StatusId.Watershield)
                || Holds(StatusId.Burning)
                || Holds(StatusId.Frozen)
                || Holds(StatusId.Poisoned)
                || Holds(StatusId.Stunned))
            {
                broken.Add("Focus holds wards and mind ailments, not elemental clocks");
            }

            if (!Holds(StatusId.Charmed) || !Holds(StatusId.Sleeping))
            {
                broken.Add("Focus must hold charm and sleep");
            }

            if (!Breaks(StatusId.Stoneskin, SpellId.Wall)
                || !Breaks(StatusId.Watershield, SpellId.Douse)
                || !Breaks(StatusId.Flameward, SpellId.Fireball)
                || Breaks(StatusId.Stoneskin, SpellId.Fireball))
            {
                broken.Add("A shared mark drops a ward; Fireball must not drop stoneskin");
            }
        }
    }
}
