namespace RuneMagic
{
    /// <summary>
    /// The four roots in a square. Adjacent sides have a winner.
    /// Opposites do not touch — Fire never beats Air, Earth never beats Water.
    /// </summary>
    public enum Essence
    {
        None = 0,
        Fire,
        Water,
        Earth,
        Air,
        Physical,
        Mind,
        Poison,
        Plant
    }

    /// <summary>
    /// The square still has winners for how matter unmakes matter.
    /// Wards no longer wear the winner: a ward keeps its own element
    /// out, and the roots that constructed that element.
    /// </summary>
    public static class ElementalLaw
    {
        public static bool Beats(Essence a, Essence b)
        {
            if (a == Essence.None || b == Essence.None || a == b)
            {
                return false;
            }

            return (a == Essence.Water && b == Essence.Fire)
                || (a == Essence.Fire && b == Essence.Earth)
                || (a == Essence.Earth && b == Essence.Air)
                || (a == Essence.Air && b == Essence.Water);
        }

        /// <summary>
        /// A ward (or form) turns work of its own essence, and of
        /// the roots that built that essence. Plant is Water · Salt ·
        /// Earth, so a plant ward also turns yield and rest.
        /// </summary>
        public static bool WardsAgainst(Essence ward, Essence incoming)
        {
            if (ward == Essence.None || incoming == Essence.None)
            {
                return false;
            }

            if (ward == incoming)
            {
                return true;
            }

            switch (ward)
            {
                case Essence.Plant:
                    return incoming == Essence.Water || incoming == Essence.Earth;
                case Essence.Poison:
                    return incoming == Essence.Plant;
                default:
                    return false;
            }
        }

        public static Essence Opposite(Essence essence)
        {
            switch (essence)
            {
                case Essence.Fire: return Essence.Water;
                case Essence.Water: return Essence.Fire;
                case Essence.Earth: return Essence.Air;
                case Essence.Air: return Essence.Earth;
                default: return Essence.None;
            }
        }

        public static bool IsWard(StatusId id) =>
            StatusSpec.Of(id).IsWard;

        public static bool IsForm(StatusId id) =>
            StatusSpec.Of(id).IsForm;

        public static bool IsStance(StatusId id) =>
            StatusSpec.Of(id).IsStance;

        public static Essence Of(StatusId id) => StatusSpec.Of(id).Element;

        public static Essence Of(ProjectileKind kind)
        {
            switch (kind)
            {
                case ProjectileKind.Fireball:
                    return Essence.Fire;
                case ProjectileKind.Wood:
                    return Essence.Plant;
                case ProjectileKind.Arrow:
                    return Essence.Physical;
                default:
                    return Essence.None;
            }
        }

        /// <summary>
        /// A wooden shaft is plant matter and a crushing missile.
        /// Stoneskin and plant ward both turn it. A rack arrow is
        /// only crushing. Vine and briar stay plant-only.
        /// </summary>
        public static bool Carries(ProjectileKind kind, Essence essence)
        {
            if (essence == Essence.None)
            {
                return false;
            }

            if (Of(kind) == essence)
            {
                return true;
            }

            return kind == ProjectileKind.Wood && essence == Essence.Physical;
        }

        public static Essence Of(SpellId spell)
        {
            return Of(SpellVerb.Of(spell).Status);
        }
    }
}
