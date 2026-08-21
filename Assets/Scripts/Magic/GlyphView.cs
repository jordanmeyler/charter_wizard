using UnityEngine;

namespace RuneMagic
{
    public enum GlyphMode
    {
        Play,
        Develop
    }

    /// <summary>
    /// Two ways to see the same system. Play is the game: marks only,
    /// no letters, names, or colours that give the elements away.
    /// Develop is the working ledger — names, recipes, and the full book.
    /// Toggle in play with F1 or the bar button. The last choice is kept.
    /// </summary>
    public static class GlyphView
    {
        const string PrefsKey = "RuneMagic.GlyphMode";

        public static readonly Color Slate = new(0.16f, 0.16f, 0.18f, 0.92f);
        public static readonly Color Ink = new(0.92f, 0.88f, 0.74f, 1f);
        public static readonly Color DimInk = new(0.55f, 0.52f, 0.46f, 0.55f);
        public static readonly Color JoinWash = new(0.18f, 0.16f, 0.14f, 0.92f);

        public static GlyphMode Mode { get; private set; } = GlyphMode.Play;

        public static bool IsDevelop => Mode == GlyphMode.Develop;
        public static bool IsPlay => Mode == GlyphMode.Play;

        static GlyphView()
        {
            Mode = (GlyphMode)PlayerPrefs.GetInt(PrefsKey, (int)GlyphMode.Play);
            if (Mode != GlyphMode.Play && Mode != GlyphMode.Develop)
            {
                Mode = GlyphMode.Play;
            }
        }

        public static void Set(GlyphMode mode)
        {
            if (Mode == mode)
            {
                return;
            }

            Mode = mode;
            PlayerPrefs.SetInt(PrefsKey, (int)mode);
            PlayerPrefs.Save();
        }

        public static void Toggle()
        {
            Set(IsDevelop ? GlyphMode.Play : GlyphMode.Develop);
        }

        public static string Speak(string develop, string play) =>
            IsDevelop ? develop : play;

        public static string RuneName(RuneId rune) =>
            IsDevelop ? RuneCatalog.NameOf(rune) : string.Empty;

        public static Color Wash(RuneId rune) =>
            IsDevelop ? RunePalette.Of(rune) : Slate;

        public static string HeldName(StoredSpell held)
        {
            if (!held.Occupied)
            {
                return IsDevelop ? "empty" : "nothing held";
            }

            return IsDevelop ? held.Name : "a held working";
        }

        public static string WorkLog(CastOutcome outcome)
        {
            if (IsDevelop)
            {
                return outcome.Log;
            }

            if (outcome.Fizzled || outcome.Spell == SpellId.None)
            {
                return "The string unravels. Nothing holds.";
            }

            if (outcome.Resolved)
            {
                return "The working turns the lock.";
            }

            return "The working holds, but this lock does not take it.";
        }
    }
}
