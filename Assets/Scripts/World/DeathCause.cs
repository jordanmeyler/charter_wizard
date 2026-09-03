using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// What unmade the adept. A spell carries the marks that wrote it
    /// so the crystal can show the sentence that found them.
    /// </summary>
    public readonly struct DeathCause
    {
        public DeathCause(string detail, IReadOnlyList<RuneId> runes)
        {
            Detail = string.IsNullOrWhiteSpace(detail)
                ? "You fall."
                : detail.Trim();
            Runes = Copy(runes);
        }

        public string Detail { get; }
        public RuneId[] Runes { get; }
        public bool HasRunes => Runes != null && Runes.Length > 0;
        public bool Exists => !string.IsNullOrEmpty(Detail);

        public string Banner => "The crystal calls you back.";

        public string LogLine
        {
            get
            {
                if (!HasRunes)
                {
                    return $"{Detail} {Banner}";
                }

                if (GlyphView.IsDevelop)
                {
                    return $"{Detail} {WorkingNames.RunePhrase(Runes)}. {Banner}";
                }

                return $"{Detail} {Banner}";
            }
        }

        public static DeathCause Plain(string detail) =>
            new(detail, null);

        public static DeathCause Spell(string detail, IReadOnlyList<RuneId> runes) =>
            new(detail, runes);

        public static DeathCause OfSpell(SpellId spell, string detail)
        {
            return new DeathCause(detail, RecipeOf(spell));
        }

        public static DeathCause OfKind(ProjectileKind kind, IReadOnlyList<RuneId> written)
        {
            if (written != null && written.Count > 0)
            {
                return Spell(kind == ProjectileKind.Wood
                    ? "Wood sent finds you."
                    : kind == ProjectileKind.Arrow
                        ? "Rest sent finds you."
                        : "Hunger sent finds you.", written);
            }

            if (kind == ProjectileKind.Wood)
            {
                return OfSpell(SpellId.WoodArrow, "A wooden shaft finds you.");
            }

            return kind == ProjectileKind.Arrow
                ? Spell("An arrow finds you.", RecipeOf(SpellId.HurledStone))
                : OfSpell(SpellId.Fireball, "Hunger sent finds you.");
        }

        public static RuneId[] RecipeOf(SpellId spell)
        {
            if (spell != SpellId.None && SpellCodex.TryGet(spell, out var entry)
                && entry.RecipeRunes != null && entry.RecipeRunes.Count > 0)
            {
                return Copy(entry.RecipeRunes);
            }

            switch (spell)
            {
                case SpellId.WoodArrow:
                    return new[] { RuneId.Plant, RuneId.Salt, RuneId.Mercury };
                case SpellId.HurledStone:
                    return new[] { RuneId.Earth, RuneId.Mercury };
                case SpellId.FlamePillar:
                    return new[] { RuneId.Fire, RuneId.Salt, RuneId.Earth };
                case SpellId.FirePillar:
                    return new[] { RuneId.Fire, RuneId.Salt };
                case SpellId.Fireball:
                    return new[] { RuneId.Fire, RuneId.Mercury };
                default:
                    return System.Array.Empty<RuneId>();
            }
        }

        static RuneId[] Copy(IReadOnlyList<RuneId> runes)
        {
            if (runes == null || runes.Count == 0)
            {
                return System.Array.Empty<RuneId>();
            }

            var copy = new RuneId[runes.Count];
            for (var i = 0; i < runes.Count; i++)
            {
                copy[i] = runes[i];
            }

            return copy;
        }
    }
}
