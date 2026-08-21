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
        Poison
    }

    /// <summary>
    /// Water douses Fire. Fire scorches Earth. Earth stands against Air.
    /// Air dries Water. Wear the winner as a ward.
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

        public static bool IsWard(StatusId id)
        {
            return id == StatusId.Stoneskin
                || id == StatusId.Watershield
                || id == StatusId.Flameward
                || id == StatusId.Windward;
        }

        public static Essence Of(StatusId id) => StatusSpec.Of(id).Element;

        public static Essence Of(ProjectileKind kind)
        {
            switch (kind)
            {
                case ProjectileKind.Fireball:
                    return Essence.Fire;
                case ProjectileKind.Arrow:
                    return Essence.Physical;
                default:
                    return Essence.None;
            }
        }

        public static Essence Of(SpellId spell)
        {
            return Of(SpellVerb.Of(spell).Status);
        }
    }
}
