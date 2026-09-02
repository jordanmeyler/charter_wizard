namespace RuneMagic
{
    /// <summary>
    /// When the world clock holds, and when the adept may walk
    /// while a sentence is aimed or released. Opening the Charter
    /// stills the room. Casting roots the adept by default; items
    /// and conditions later grant motion during a cast.
    /// </summary>
    public static class CastLaw
    {
        public static bool HoldsWorld(PlayMode mode, bool naming = false)
        {
            return naming || mode == PlayMode.Paused || mode == PlayMode.Charter;
        }

        public static bool IsCasting(PlayMode mode, bool busy)
        {
            return busy || mode == PlayMode.Aiming;
        }

        /// <summary>
        /// The adept walks only while exploring, or while aiming /
        /// releasing if something has granted motion during a cast.
        /// The Charter holds the clock, so there is no walk there.
        /// </summary>
        public static bool AllowsMove(PlayMode mode, bool busy, bool naming, bool moveWhileCasting)
        {
            if (HoldsWorld(mode, naming))
            {
                return false;
            }

            if (mode != PlayMode.Exploring && mode != PlayMode.Aiming)
            {
                return false;
            }

            return !IsCasting(mode, busy) || moveWhileCasting;
        }
    }
}
