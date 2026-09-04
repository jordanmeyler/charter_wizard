using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Wood arrow is Plant · Salt · Mercury. Mix another mark into
    /// the shaft and the same fly becomes a covering: fire, ice, or
    /// the grave of a plant. The table is the mix-in list — add a
    /// row and a catalog chain, not a new grammar pair.
    /// Plant · Fire is Ash; the fire arrow keeps Salt between them.
    /// </summary>
    public readonly struct ArrowMix
    {
        public ArrowMix(SpellId spell, RuneId mix, TileVerb tiles, StatusId status, float statusSeconds, StrikeKind kind, int power)
        {
            Spell = spell;
            Mix = mix;
            Tiles = tiles;
            Status = status;
            StatusSeconds = statusSeconds;
            Kind = kind;
            Power = power;
        }

        public SpellId Spell { get; }
        public RuneId Mix { get; }
        public TileVerb Tiles { get; }
        public StatusId Status { get; }
        public float StatusSeconds { get; }
        public StrikeKind Kind { get; }
        public int Power { get; }
    }

    public static class ArrowLaw
    {
        public static readonly ArrowMix[] Mixes =
        {
            new ArrowMix(SpellId.WoodArrow, RuneId.None, TileVerb.None, StatusId.None, 0f, StrikeKind.Plant, 3),
            new ArrowMix(SpellId.FireArrow, RuneId.Fire, TileVerb.Ignite, StatusId.Burning, 4.5f, StrikeKind.Fire, 3),
            new ArrowMix(SpellId.IceArrow, RuneId.Ice, TileVerb.Freeze, StatusId.Frozen, 5f, StrikeKind.Ice, 3),
            new ArrowMix(SpellId.PoisonArrow, RuneId.Poison, TileVerb.Poison, StatusId.Poisoned, StatusSpec.PoisonKillSeconds, StrikeKind.Poison, 3)
        };

        public static bool IsArrow(SpellId spell) =>
            TryGet(spell, out _);

        public static bool TryGet(SpellId spell, out ArrowMix mix)
        {
            for (var i = 0; i < Mixes.Length; i++)
            {
                if (Mixes[i].Spell == spell)
                {
                    mix = Mixes[i];
                    return true;
                }
            }

            mix = default;
            return false;
        }

        public static SpellVerb VerbOf(SpellId spell)
        {
            return TryGet(spell, out var mix)
                ? new SpellVerb(SpellTarget.Single, 0f, mix.Status, mix.StatusSeconds, mix.Tiles)
                : default;
        }

        public static StrikeLaw.Strike StrikeOf(SpellId spell)
        {
            return TryGet(spell, out var mix)
                ? new StrikeLaw.Strike(mix.Power, mix.Kind)
                : new StrikeLaw.Strike(0, StrikeKind.None);
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (!IsArrow(SpellId.WoodArrow)
                || !IsArrow(SpellId.FireArrow)
                || !IsArrow(SpellId.IceArrow)
                || !IsArrow(SpellId.PoisonArrow)
                || IsArrow(SpellId.Vine)
                || IsArrow(SpellId.HurledStone))
            {
                broken.Add("Wood, fire, ice, and poison arrows share the shaft table; vine and hurled stone do not");
            }

            if (VerbOf(SpellId.WoodArrow).Tiles != TileVerb.None
                || VerbOf(SpellId.FireArrow).Tiles != TileVerb.Ignite
                || VerbOf(SpellId.IceArrow).Tiles != TileVerb.Freeze
                || VerbOf(SpellId.PoisonArrow).Tiles != TileVerb.Poison)
            {
                broken.Add("A mixed arrow must lay that element's covering on the tile it strikes");
            }

            if (StrikeOf(SpellId.WoodArrow).Kind != StrikeKind.Plant
                || StrikeOf(SpellId.FireArrow).Kind != StrikeKind.Fire
                || StrikeOf(SpellId.IceArrow).Kind != StrikeKind.Ice
                || StrikeOf(SpellId.PoisonArrow).Kind != StrikeKind.Poison)
            {
                broken.Add("A mixed arrow strikes as the mixed element; the plain shaft stays plant");
            }
        }
    }
}
