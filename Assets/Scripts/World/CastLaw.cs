namespace RuneMagic
{
    /// <summary>
    /// The world clock holds only while a menu is up (the Charter,
    /// pause, naming, or a prayer reveal). Going to aim closes that
    /// menu: time runs again and the adept stands until the click
    /// lands, unless an item or condition later grants motion during
    /// a cast.
    /// </summary>
    public static class CastLaw
    {
        public static bool HoldsWorld(PlayMode mode, bool menu = false)
        {
            return menu || mode == PlayMode.Paused || mode == PlayMode.Charter;
        }

        public static bool IsCasting(PlayMode mode)
        {
            return mode == PlayMode.Aiming;
        }

        public static bool AllowsMove(PlayMode mode, bool naming, bool moveWhileCasting)
        {
            if (HoldsWorld(mode, naming) || mode == PlayMode.Grimoire || mode == PlayMode.Inventory)
            {
                return false;
            }

            if (IsCasting(mode))
            {
                return moveWhileCasting;
            }

            return mode == PlayMode.Exploring;
        }
    }
}
