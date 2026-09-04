using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Focus holds mind spells and worn buffs. Charm, command, lull,
    /// rage, terror, confuse, the wards, the forms, flight, float,
    /// and veil stay until another focus sentence reuses a mark from
    /// the held working, or until you recast the same sentence.
    /// Sulphur is the mind rune — it is how a hold is written, not a
    /// copied mark — so two holds can stand if the rest of the
    /// sentences do not share a rune. Flight writes logos and Salt,
    /// so a ward that stands with Salt lets the flight go. A fireball
    /// or a wall does not ask focus to let go.
    /// Burning is a contact meter. Poison is a slower meter that
    /// keeps its level off the slick until Light cleanses it.
    /// Other elemental work stands on its own clock.
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
            if (StatusSpec.Of(status).NeedsFocus)
            {
                return true;
            }

            return SpellCodex.TryGet(spell, out var entry) && entry.Book == SpellBook.Mind;
        }

        public static bool Holds(StatusId id) =>
            StatusSpec.Of(id).NeedsFocus;

        public static bool IsCopiedRune(RuneId rune) =>
            rune != RuneId.None && rune != RuneId.Sulphur;

        public static bool Breaks(StatusId held, SpellId next)
        {
            if (!Holds(held) || !IsMindSpell(next))
            {
                return false;
            }

            return Overlaps(DefaultRunes(held), UsedRunes(next, default));
        }

        public static int Release(Component caster, SpellId spell, Composition composition, StatusId incoming)
        {
            if (!IsMindSpell(spell))
            {
                return 0;
            }

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
                if (IsCopiedRune(left[i]) && Contains(right, left[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool Contains(IReadOnlyList<RuneId> runes, RuneId wanted)
        {
            if (runes == null || !IsCopiedRune(wanted))
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
                case StatusId.Flying: return SpellId.Flight;
                case StatusId.Floating: return SpellId.Float;
                case StatusId.Veiled: return SpellId.Veil;
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
                || !IsMindSpell(SpellId.Flight)
                || !IsMindSpell(SpellId.Float)
                || !IsMindSpell(SpellId.Veil)
                || IsMindSpell(SpellId.Hop)
                || IsMindSpell(SpellId.Wall)
                || IsMindSpell(SpellId.Fireball))
            {
                broken.Add("Wards, forms, flight, float, and veil are mind spells; hop, walls, and fireballs are not");
            }

            if (!Holds(StatusId.Stoneskin)
                || !Holds(StatusId.Watershield)
                || !Holds(StatusId.Flying)
                || !Holds(StatusId.Floating)
                || !Holds(StatusId.Veiled)
                || Holds(StatusId.Burning)
                || Holds(StatusId.Frozen)
                || Holds(StatusId.Poisoned)
                || Holds(StatusId.Stunned))
            {
                broken.Add("Focus holds wards, buffs, and mind ailments, not elemental clocks");
            }

            if (!Holds(StatusId.Charmed) || !Holds(StatusId.Sleeping))
            {
                broken.Add("Focus must hold charm and sleep");
            }

            if (!StatusSpec.Of(StatusId.CloudForm).RecastDismisses
                || !StatusSpec.Of(StatusId.Flameward).RecastDismisses
                || !StatusSpec.Of(StatusId.Watershield).RecastDismisses
                || !StatusSpec.Of(StatusId.Stoneskin).RecastDismisses
                || !StatusSpec.Of(StatusId.Charmed).RecastDismisses
                || !StatusSpec.Of(StatusId.Sleeping).RecastDismisses
                || !StatusSpec.Of(StatusId.Veiled).RecastDismisses
                || !StatusSpec.Of(StatusId.Flying).RecastDismisses
                || !StatusSpec.Of(StatusId.Floating).RecastDismisses
                || StatusSpec.Of(StatusId.Burning).RecastDismisses
                || StatusSpec.Of(StatusId.Frozen).RecastDismisses
                || StatusSpec.Of(StatusId.Poisoned).RecastDismisses
                || StatusSpec.Of(StatusId.Stunned).RecastDismisses)
            {
                broken.Add("Recasting lets go of a ward, form, flight, float, veil, or mind hold — not a meter or frost");
            }

            if (SpellVerb.Of(SpellId.Flight).Status != StatusId.Flying
                || SpellVerb.Of(SpellId.Float).Status != StatusId.Floating
                || SpellVerb.Of(SpellId.Hop).Status != StatusId.None
                || SpellVerb.Of(SpellId.CloudForm).Status != StatusId.CloudForm)
            {
                broken.Add("Flight and float are focus holds; hop is only a leap; cloud-form stays a form");
            }

            if (Breaks(StatusId.Stoneskin, SpellId.Wall)
                || Breaks(StatusId.Watershield, SpellId.Douse)
                || Breaks(StatusId.Flameward, SpellId.Fireball)
                || Breaks(StatusId.Sleeping, SpellId.Fireball)
                || Breaks(StatusId.Charmed, SpellId.Fireball)
                || Breaks(StatusId.Charmed, SpellId.Wall)
                || Breaks(StatusId.Stoneskin, SpellId.Hop)
                || Breaks(StatusId.Flying, SpellId.Fireball)
                || Breaks(StatusId.Flying, SpellId.Hop)
                || Breaks(StatusId.Flying, SpellId.Blink)
                || Breaks(StatusId.Floating, SpellId.Gust))
            {
                broken.Add("A non-focus sentence must not drop a hold");
            }

            if (!Breaks(StatusId.Stoneskin, SpellId.Flameward)
                || !Breaks(StatusId.Watershield, SpellId.Lull)
                || !Breaks(StatusId.Flameward, SpellId.Rage)
                || !Breaks(StatusId.Charmed, SpellId.Command)
                || !Breaks(StatusId.Charmed, SpellId.Plantward)
                || !Breaks(StatusId.Stoneskin, SpellId.Flight)
                || !Breaks(StatusId.Flying, SpellId.Stoneskin)
                || !Breaks(StatusId.Flying, SpellId.Float)
                || !Breaks(StatusId.Floating, SpellId.Windward)
                || Breaks(StatusId.Charmed, SpellId.Stoneskin)
                || Breaks(StatusId.Sleeping, SpellId.Flameward)
                || Breaks(StatusId.Stoneskin, SpellId.Charm))
            {
                broken.Add("Focus lets go only when another focus sentence reuses a mark other than Sulphur");
            }
        }
    }
}
