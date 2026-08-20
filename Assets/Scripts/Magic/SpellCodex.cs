using System.Collections.Generic;

namespace RuneMagic
{
    public enum SpellOutcome
    {
        Kill,
        Restrain,
        Neither
    }

    public enum SpellBook
    {
        End,
        Hold,
        Cross,
        Weather,
        GrowHeal,
        Mind,
        SeeHide,
        Call,
        Grave
    }

    public readonly struct CodexEntry
    {
        public CodexEntry(
            int number,
            SpellBook book,
            string want,
            string name,
            string recipe,
            string via,
            string form,
            SpellOutcome outcome,
            string gate)
        {
            Number = number;
            Book = book;
            Want = want;
            Name = name;
            Recipe = recipe;
            Via = via;
            Form = form;
            Outcome = outcome;
            Gate = gate;
        }

        public int Number { get; }
        public SpellBook Book { get; }
        public string Want { get; }
        public string Name { get; }
        public string Recipe { get; }
        public string Via { get; }
        public string Form { get; }
        public SpellOutcome Outcome { get; }
        public string Gate { get; }
    }

    /// <summary>
    /// Fifty story-chains. 1–40 are the ordinary book (no Death).
    /// 41–50 are Death / Free. Life only marks a living recipe.
    /// </summary>
    public static class SpellCodex
    {
        static readonly CodexEntry[] Entries =
        {
            E(1, SpellBook.End, "A seed of heat that is a thing, and flies.", "Fireball", "Fire · Air · Salt · Mercury", "Spark · Salt · Mercury", "Shot", SpellOutcome.Kill),
            E(2, SpellBook.End, "Hunger given a body and asked to rest. It stands.", "Flame-pillar", "Fire · Salt · Earth", "Flame · Earth", "Pillar", SpellOutcome.Kill),
            E(3, SpellBook.Cross, "A fire-body sent into a thing. No breath, so it does not fly.", "Melt", "Fire · Salt · Mercury", "Flame · Mercury", "Remote", SpellOutcome.Neither),
            E(4, SpellBook.End, "Hunger needs breath; that breath is withheld.", "Smother", "Fire · Air · Dark", "Spark · Dark", "Remote", SpellOutcome.Neither),
            E(5, SpellBook.End, "Hunger shown, given breath, sent as a clean line.", "Sun-lance", "Fire · Light · Air · Mercury", "Spark · Light · Mercury", "Shot", SpellOutcome.Kill),
            E(6, SpellBook.End, "Hunger’s passion placed on a distant wick or oil.", "Ignite", "Fire · Sulphur · Mercury", "", "Remote", SpellOutcome.Neither),
            E(7, SpellBook.End, "The seed stretched through more breath and sent. A path, not a body.", "Lightning", "Fire · Air · Air · Mercury", "Spark · Air · Mercury", "Shot", SpellOutcome.Kill),
            E(8, SpellBook.End, "That path finds yield given a body. The pool is what dies.", "Chain", "Fire · Air · Air · Mercury · Water · Salt", "Lightning · Mercury · Water · Salt", "Remote", SpellOutcome.Kill),
            E(9, SpellBook.Hold, "The seed given a body around your feet. They cannot step.", "Live-floor", "Fire · Air · Salt", "Spark · Salt", "Spread", SpellOutcome.Kill),
            E(10, SpellBook.Hold, "The seed reaches a mind at a point. They lock.", "Jolt", "Fire · Air · Sulphur · Mercury", "Spark · Sulphur · Mercury", "Remote", SpellOutcome.Restrain),
            E(11, SpellBook.Hold, "The arc meets rest, then every mind around you.", "Thunderclap", "Fire · Air · Air · Earth · Sulphur", "Lightning · Earth · Sulphur", "Spread", SpellOutcome.Restrain),
            E(12, SpellBook.Weather, "Breath holds yield; a seed is inside. Weather arrives.", "Storm", "Air · Water · Fire · Air", "Cloud · Spark", "Remote", SpellOutcome.Kill),
            E(13, SpellBook.Weather, "The hanging veil is drawn down. Fire drowns.", "Rain", "Air · Water · Earth", "Cloud · Earth", "Remote", SpellOutcome.Neither),
            E(14, SpellBook.SeeHide, "The hanging veil is withheld and given a body.", "Fog", "Air · Water · Dark · Salt", "Cloud · Dark · Salt", "Spread", SpellOutcome.Neither),
            E(15, SpellBook.End, "Hunger forced through yield, given a body, sent.", "Scald", "Fire · Water · Salt · Mercury", "Steam · Salt · Mercury", "Shot", SpellOutcome.Kill),
            E(16, SpellBook.Weather, "Yield learns breath so it can leave the vessel, then is sent.", "Water-jet", "Water · Air · Salt · Mercury", "", "Shot", SpellOutcome.Restrain),
            E(17, SpellBook.Hold, "Yield going, more yield, given a body. They bog.", "Flood", "Water · Mercury · Water · Salt", "Current · Water · Salt", "Spread", SpellOutcome.Restrain),
            E(18, SpellBook.Cross, "Yield given a body and asked to rest. Hard water. It will thaw.", "Ice-pillar", "Water · Salt · Earth", "Ice", "Pillar", SpellOutcome.Restrain),
            E(19, SpellBook.Hold, "That hard water-body is sent.", "Ice-spear", "Water · Salt · Earth · Mercury", "Ice · Mercury", "Shot", SpellOutcome.Restrain),
            E(20, SpellBook.Hold, "The veil is given ice’s story and sent softly.", "Snowfall", "Air · Water · Salt · Earth · Mercury", "Cloud · Ice · Mercury", "Remote", SpellOutcome.Restrain),
            E(21, SpellBook.Cross, "The hard water-body meets hunger and remembers yield.", "Thaw", "Water · Salt · Earth · Fire", "Ice · Fire", "Remote", SpellOutcome.Neither),
            E(22, SpellBook.End, "Rest given a body and sent. Earth flies.", "Hurled stone", "Earth · Salt · Mercury", "Stone · Mercury", "Shot", SpellOutcome.Kill),
            E(23, SpellBook.Cross, "A body of rest asked to rest as more rest. A wall.", "Wall", "Earth · Salt · Earth", "Stone · Earth", "Pillar", SpellOutcome.Neither),
            E(24, SpellBook.Cross, "Rest asked to go, given breath so it leaves a hollow.", "Pit", "Earth · Mercury · Air", "", "Remote", SpellOutcome.Neither),
            E(25, SpellBook.Cross, "A body of rest given breath and sent across.", "Bridge", "Earth · Salt · Air · Mercury", "Stone · Air · Mercury", "Remote", SpellOutcome.Neither),
            E(26, SpellBook.Hold, "Rest meeting yield, given a body. It holds them.", "Quagmire", "Earth · Water · Salt", "Mud · Salt", "Spread", SpellOutcome.Restrain),
            E(27, SpellBook.End, "Hungry earth given a body and sent.", "Lava-flood", "Fire · Earth · Salt · Mercury", "Lava · Salt · Mercury", "Remote", SpellOutcome.Kill),
            E(28, SpellBook.Cross, "Hungry earth quenched and given a body. A path.", "Obsidian path", "Fire · Earth · Water · Salt", "Lava · Water · Salt", "Remote", SpellOutcome.Neither),
            E(29, SpellBook.GrowHeal, "Wet rest given a vegetable body, then marked living.", "Sprout", "Water · Earth · Salt · Life", "Plant · Life", "Spread", SpellOutcome.Neither),
            E(30, SpellBook.Hold, "That living plant is sent. It holds them, or it climbs.", "Vine", "Water · Earth · Salt · Life · Mercury", "Grove · Mercury", "Remote", SpellOutcome.Restrain),
            E(31, SpellBook.GrowHeal, "That living plant is asked to stand.", "Vine-rise", "Water · Earth · Salt · Life · Earth", "Grove · Earth", "Pillar", SpellOutcome.Neither),
            E(32, SpellBook.GrowHeal, "A living body, yield and rest, sent into the living.", "Mend", "Life · Salt · Water · Earth · Mercury", "", "Spread", SpellOutcome.Neither),
            E(33, SpellBook.Cross, "Breath given a body, marked living, sent. A leap.", "Hop", "Air · Salt · Life · Mercury", "", "Spread", SpellOutcome.Neither),
            E(34, SpellBook.Cross, "Breath going; a body of it; marked living; going again.", "Flight", "Air · Mercury · Salt · Life · Mercury", "Wind · Salt · Life · Mercury", "Self", SpellOutcome.Neither),
            E(35, SpellBook.Mind, "Hunger’s passion is sent into a mind.", "Rage", "Fire · Sulphur · Mercury", "", "Remote", SpellOutcome.Neither),
            E(36, SpellBook.Mind, "The withheld reaches a mind. They flee or freeze.", "Terror", "Dark · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(37, SpellBook.Mind, "Yield reaches a mind. They sleep. They can be woken.", "Lull", "Water · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(38, SpellBook.Weather, "Breath going, more breath, given a body so it can push.", "Gale", "Air · Mercury · Air · Salt", "Wind · Air · Salt", "Shot", SpellOutcome.Restrain),
            E(39, SpellBook.SeeHide, "The withheld, a living body, as breath. Hard to see.", "Veil", "Dark · Life · Salt · Air", "", "Spread", SpellOutcome.Neither),
            E(40, SpellBook.Call, "Flesh, marked living, given a mind, sent here. Know the formula.", "Call beast", "Earth · Water · Salt · Life · Sulphur · Mercury", "", "Remote", SpellOutcome.Neither),
            E(41, SpellBook.Grave, "A living plant, then the grave. Verdure rots. No soul.", "Blight", "Water · Earth · Salt · Life · Death", "Grove · Death", "Spread", SpellOutcome.Kill, "Either"),
            E(42, SpellBook.Grave, "Withheld, given a body, marked by the grave, and sent.", "Shade", "Dark · Death · Salt · Mercury", "Shade · Mercury", "Remote", SpellOutcome.Neither, "Free"),
            E(43, SpellBook.Grave, "The grave is sent into a living body.", "Unmake", "Death · Mercury · Life · Salt", "", "Remote", SpellOutcome.Kill, "Free"),
            E(44, SpellBook.Grave, "The waking passion is given to the grave. Sleep as if dead.", "Grave-sleep", "Life · Sulphur · Death", "", "Remote", SpellOutcome.Restrain, "Free"),
            E(45, SpellBook.Grave, "The four as a body, marked by the grave, and sent.", "Corpse-call", "Salt · Water · Earth · Fire · Death · Mercury", "", "Remote", SpellOutcome.Neither, "Free"),
            E(46, SpellBook.Grave, "Rest marked by the grave. Earth-life and golems come apart.", "Grave-dust", "Earth · Death · Salt", "", "Spread", SpellOutcome.Kill, "Either"),
            E(47, SpellBook.Grave, "Hunger marked by the grave and sent into a flame.", "Snuff", "Fire · Death · Mercury", "Ember · Mercury", "Remote", SpellOutcome.Neither, "Either"),
            E(48, SpellBook.Grave, "The seed marked by the grave and sent. A live rod dies.", "Blackout", "Fire · Air · Death · Mercury", "Spark · Death · Mercury", "Shot", SpellOutcome.Neither, "Either"),
            E(49, SpellBook.Grave, "Yield given a body, then the grave. Ice that will not thaw.", "Grave-ice", "Water · Salt · Death", "", "Remote", SpellOutcome.Restrain, "Either"),
            E(50, SpellBook.Grave, "Living breath, then the grave, sent. The breath leaves them.", "Last breath", "Air · Life · Death · Mercury", "", "Remote", SpellOutcome.Kill, "Free")
        };

        public static IReadOnlyList<CodexEntry> All => Entries;

        public static string BookName(SpellBook book)
        {
            switch (book)
            {
                case SpellBook.End: return "End / unmake";
                case SpellBook.Hold: return "Hold / stop";
                case SpellBook.Cross: return "Cross / move";
                case SpellBook.Weather: return "Weather";
                case SpellBook.GrowHeal: return "Living (Life marks the recipe)";
                case SpellBook.Mind: return "Mind";
                case SpellBook.SeeHide: return "See / hide";
                case SpellBook.Call: return "Call a being";
                case SpellBook.Grave: return "Death / Free — reserved";
                default: return book.ToString();
            }
        }

        static CodexEntry E(
            int number,
            SpellBook book,
            string want,
            string name,
            string recipe,
            string via,
            string form,
            SpellOutcome outcome,
            string gate = "")
        {
            return new CodexEntry(number, book, want, name, recipe, via, form, outcome, gate);
        }
    }
}
