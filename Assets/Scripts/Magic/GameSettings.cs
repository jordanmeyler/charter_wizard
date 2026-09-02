using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Player-facing play options. Hiding stacked failures starts on;
    /// naming new spells starts off. The last choice is kept.
    /// </summary>
    public static class GameSettings
    {
        const string HideBadKey = "RuneMagic.HideBadRecipes";
        const string PromptNamesKey = "RuneMagic.PromptNewSpells";

        public static bool HideBadRecipes { get; private set; } = true;
        public static bool PromptNewSpells { get; private set; } = false;

        static GameSettings()
        {
            HideBadRecipes = PlayerPrefs.GetInt(HideBadKey, 1) != 0;
            PromptNewSpells = PlayerPrefs.GetInt(PromptNamesKey, 0) != 0;
        }

        public static void SetHideBadRecipes(bool value)
        {
            if (HideBadRecipes == value)
            {
                return;
            }

            HideBadRecipes = value;
            PlayerPrefs.SetInt(HideBadKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }

        public static void SetPromptNewSpells(bool value)
        {
            if (PromptNewSpells == value)
            {
                return;
            }

            PromptNewSpells = value;
            PlayerPrefs.SetInt(PromptNamesKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
