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
        Call
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
            SpellOutcome outcome)
        {
            Number = number;
            Book = book;
            Want = want;
            Name = name;
            Recipe = recipe;
            Via = via;
            Form = form;
            Outcome = outcome;
        }

        public int Number { get; }
        public SpellBook Book { get; }
        public string Want { get; }
        public string Name { get; }
        public string Recipe { get; }
        public string Via { get; }
        public string Form { get; }
        public SpellOutcome Outcome { get; }
    }

    /// <summary>
    /// Starting catalog: story-chains from the eleven basic runes.
    /// Joins fold into wrought runes (Spark, Ice, Grove…). No primordials.
    /// Life only when the subject or product is living.
    /// </summary>
    public static class SpellCodex
    {
        static readonly CodexEntry[] Entries =
        {
            E(1, SpellBook.End, "A seed of heat that is a thing, and flies.", "Fireball", "Fire · Air · Salt · Mercury", "Spark · Salt · Mercury", "Shot", SpellOutcome.Kill),
            E(2, SpellBook.End, "Hunger given a body and asked to rest. It stands.", "Flame-pillar", "Fire · Salt · Earth", "Flame · Earth", "Pillar", SpellOutcome.Kill),
            E(3, SpellBook.Cross, "A fire-body sent into a thing. No breath, so it does not fly.", "Melt", "Fire · Salt · Mercury", "Flame · Mercury", "Remote", SpellOutcome.Neither),
            E(4, SpellBook.End, "Hunger stilled where it already burns.", "Snuff", "Fire · Death · Mercury", "Ember · Mercury", "Remote", SpellOutcome.Neither),
            E(5, SpellBook.End, "Hunger needs breath; that breath is withheld; then still.", "Smother", "Fire · Air · Dark · Death", "Spark · Dark · Death", "Remote", SpellOutcome.Neither),
            E(6, SpellBook.End, "Hunger shown, given breath, sent as a clean line.", "Sun-lance", "Fire · Light · Air · Mercury", "Spark · Light · Mercury", "Shot", SpellOutcome.Kill),
            E(7, SpellBook.Cross, "Grains, a fire-body, then still. They remember liquid.", "Glass", "Earth · Air · Salt · Fire · Salt · Death", "Sand · Flame · Death", "Remote", SpellOutcome.Neither),
            E(8, SpellBook.End, "The seed stretched through more breath and sent. A path, not a body.", "Lightning", "Fire · Air · Air · Mercury", "Spark · Air · Mercury", "Shot", SpellOutcome.Kill),
            E(9, SpellBook.End, "That path finds yield given a body. The pool is what dies.", "Chain", "Fire · Air · Air · Mercury · Water · Salt", "Lightning · Mercury · Water · Salt", "Remote", SpellOutcome.Kill),
            E(10, SpellBook.Hold, "The seed given a body around your feet. They cannot step.", "Live-floor", "Fire · Air · Salt", "Spark · Salt", "Spread", SpellOutcome.Kill),
            E(11, SpellBook.Hold, "The seed reaches a mind at a point. They lock.", "Jolt", "Fire · Air · Sulphur · Mercury", "Spark · Sulphur · Mercury", "Remote", SpellOutcome.Restrain),
            E(12, SpellBook.Hold, "The arc meets rest, then every mind around you. They drop.", "Thunderclap", "Fire · Air · Air · Earth · Sulphur", "Lightning · Earth · Sulphur", "Spread", SpellOutcome.Restrain),
            E(13, SpellBook.End, "The seed is stilled. A live rod dies.", "Blackout", "Fire · Air · Death · Mercury", "Spark · Death · Mercury", "Shot", SpellOutcome.Neither),
            E(14, SpellBook.Weather, "Breath holds yield; a seed is inside. Weather arrives.", "Storm", "Air · Water · Fire · Air", "Cloud · Spark", "Remote", SpellOutcome.Kill),
            E(15, SpellBook.Weather, "That weather is sent to a point.", "Call-storm", "Air · Water · Fire · Air · Mercury", "Storm · Mercury", "Remote", SpellOutcome.Kill),
            E(16, SpellBook.Weather, "The hanging veil is drawn down. Fire drowns.", "Rain", "Air · Water · Earth", "Cloud · Earth", "Remote", SpellOutcome.Neither),
            E(17, SpellBook.SeeHide, "The hanging veil is withheld and given a body.", "Fog", "Air · Water · Dark · Salt", "Cloud · Dark · Salt", "Spread", SpellOutcome.Neither),
            E(18, SpellBook.Hold, "The veil is given ice’s story and sent softly.", "Snowfall", "Air · Water · Salt · Death · Mercury", "Cloud · Ice · Mercury", "Remote", SpellOutcome.Restrain),
            E(19, SpellBook.End, "Hunger forced through yield, given a body, sent.", "Scald", "Fire · Water · Salt · Mercury", "Steam · Salt · Mercury", "Shot", SpellOutcome.Kill),
            E(20, SpellBook.Weather, "Yield learns breath so it can leave the vessel, then is sent.", "Water-jet", "Water · Air · Salt · Mercury", "", "Shot", SpellOutcome.Restrain),
            E(21, SpellBook.Hold, "Yield going, more yield, given a body. They bog.", "Flood", "Water · Mercury · Water · Salt", "Current · Water · Salt", "Spread", SpellOutcome.Restrain),
            E(22, SpellBook.Hold, "Yield in the ground is stilled. The pool stays water.", "Still", "Water · Earth · Death", "", "Remote", SpellOutcome.Restrain),
            E(23, SpellBook.Cross, "Yield given a body, then stilled, and asked to stand.", "Ice-pillar", "Water · Salt · Death · Earth", "Ice · Earth", "Pillar", SpellOutcome.Restrain),
            E(24, SpellBook.Hold, "That still water-body is sent.", "Ice-spear", "Water · Salt · Death · Mercury", "Ice · Mercury", "Shot", SpellOutcome.Restrain),
            E(25, SpellBook.Cross, "The still water-body meets hunger and remembers yield.", "Thaw", "Water · Salt · Death · Fire", "Ice · Fire", "Remote", SpellOutcome.Neither),
            E(26, SpellBook.End, "Rest given a body, stilled, and sent. Earth flies.", "Hurled stone", "Earth · Salt · Death · Mercury", "Stone · Mercury", "Shot", SpellOutcome.Kill),
            E(27, SpellBook.Cross, "The still earth-body asked to rest as more rest. A wall.", "Wall", "Earth · Salt · Death · Earth", "Stone · Earth", "Pillar", SpellOutcome.Neither),
            E(28, SpellBook.Cross, "Rest is asked to go, and stay gone. A pit.", "Pit", "Earth · Mercury · Death", "Quake · Death", "Remote", SpellOutcome.Neither),
            E(29, SpellBook.Cross, "The still earth-body is given breath and sent across.", "Bridge", "Earth · Salt · Death · Air · Mercury", "Stone · Air · Mercury", "Remote", SpellOutcome.Neither),
            E(30, SpellBook.Hold, "Rest meeting yield, given a body, stilled. It will not let go.", "Quagmire", "Earth · Water · Salt · Death", "Mud · Salt · Death", "Spread", SpellOutcome.Restrain),
            E(31, SpellBook.Hold, "Rest asked to go, then asked to rest again. The ground heaves.", "Quake", "Earth · Mercury · Earth", "Quake · Earth", "Spread", SpellOutcome.Restrain),
            E(32, SpellBook.End, "Hungry earth given a body and sent.", "Lava-flood", "Fire · Earth · Salt · Mercury", "Lava · Salt · Mercury", "Remote", SpellOutcome.Kill),
            E(33, SpellBook.Cross, "Hungry earth quenched and given a body. A path.", "Obsidian path", "Fire · Earth · Water · Salt", "Lava · Water · Salt", "Remote", SpellOutcome.Neither),
            E(34, SpellBook.Hold, "Rest withheld into a hollow, given a body, placed.", "Shadow-well", "Earth · Dark · Salt · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(35, SpellBook.Mind, "Rest reaches a mind. Weight. They will not enter.", "Dread", "Earth · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(36, SpellBook.GrowHeal, "Wet rest given a vegetable body, then woken at your feet.", "Sprout", "Water · Earth · Salt · Life", "Plant · Life", "Spread", SpellOutcome.Neither),
            E(37, SpellBook.Hold, "That waking plant is sent. It holds them, or it climbs.", "Vine", "Water · Earth · Salt · Life · Mercury", "Grove · Mercury", "Remote", SpellOutcome.Restrain),
            E(38, SpellBook.GrowHeal, "That waking plant is asked to stand.", "Vine-rise", "Water · Earth · Salt · Life · Earth", "Grove · Earth", "Pillar", SpellOutcome.Neither),
            E(39, SpellBook.GrowHeal, "That waking plant is withheld from here and called there.", "Call-growth", "Water · Earth · Salt · Life · Dark · Mercury", "Grove · Dark · Mercury", "Remote", SpellOutcome.Neither),
            E(40, SpellBook.End, "That waking plant is then stilled. Verdure rots. No soul.", "Blight", "Water · Earth · Salt · Life · Death", "Grove · Death", "Spread", SpellOutcome.Kill),
            E(41, SpellBook.GrowHeal, "A waking body, yield and rest, sent into the living.", "Mend", "Life · Salt · Water · Earth · Mercury", "", "Spread", SpellOutcome.Neither),
            E(42, SpellBook.Mind, "Hunger’s passion is sent into a mind.", "Rage", "Fire · Sulphur · Mercury", "", "Remote", SpellOutcome.Neither),
            E(43, SpellBook.Mind, "The withheld reaches a mind. They flee or freeze.", "Terror", "Dark · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(44, SpellBook.Mind, "Yield reaches a mind. They sleep.", "Lull", "Water · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(45, SpellBook.Mind, "The waking passion is stilled. Life remains.", "Sleep", "Life · Sulphur · Death", "", "Remote", SpellOutcome.Restrain),
            E(46, SpellBook.Mind, "Passion sent as a body. Soulless obey. Ensouled: Charter fizzles.", "Command", "Sulphur · Mercury · Salt", "", "Remote", SpellOutcome.Restrain),
            E(47, SpellBook.Mind, "Breath stilled in the passion. They cannot speak.", "Silence", "Air · Death · Sulphur", "", "Remote", SpellOutcome.Restrain),
            E(48, SpellBook.Weather, "Breath going, more breath, given a body so it can push.", "Gale", "Air · Mercury · Air · Salt", "Wind · Air · Salt", "Shot", SpellOutcome.Restrain),
            E(49, SpellBook.Cross, "Breath going, with hunger. It rises.", "Updraft", "Air · Mercury · Fire", "Wind · Fire", "Remote", SpellOutcome.Neither),
            E(50, SpellBook.Cross, "Breath given a body, waking, sent. A living leap.", "Hop", "Air · Salt · Life · Mercury", "", "Spread", SpellOutcome.Neither),
            E(51, SpellBook.Cross, "Breath going; a body of it; waking in it; going again.", "Flight", "Air · Mercury · Salt · Life · Mercury", "Wind · Salt · Life · Mercury", "Self", SpellOutcome.Neither),
            E(52, SpellBook.Cross, "Breath fixed as a body on the living. They hang.", "Levitation", "Air · Salt · Life", "", "Spread", SpellOutcome.Neither),
            E(53, SpellBook.Hold, "Breath stilled, sent, given a body. A pocket of no-breath.", "Vacuum", "Air · Death · Mercury · Salt", "", "Remote", SpellOutcome.Restrain),
            E(54, SpellBook.SeeHide, "Breath shown and given a body, from the feet.", "Day-wake", "Air · Light · Salt", "", "Spread", SpellOutcome.Neither),
            E(55, SpellBook.SeeHide, "The withheld, the waking body, as breath. Hard to see.", "Veil", "Dark · Life · Salt · Air", "", "Spread", SpellOutcome.Neither),
            E(56, SpellBook.Call, "Withheld, stilled, given a body, and sent. No waking. Free only.", "Shade", "Dark · Death · Salt · Mercury", "Shade · Mercury", "Remote", SpellOutcome.Neither),
            E(57, SpellBook.Call, "A body of the four, woken, given a mind.", "Homunculus", "Salt · Water · Earth · Fire · Life · Sulphur", "", "Remote", SpellOutcome.Neither),
            E(58, SpellBook.Call, "Flesh, waking, passion, sent here. You must know its formula.", "Call beast", "Earth · Water · Salt · Life · Sulphur · Mercury", "", "Remote", SpellOutcome.Neither),
            E(59, SpellBook.Call, "Rest given a body and woken, sent. Soulless clay. No mind.", "Clay servant", "Earth · Salt · Life · Mercury", "", "Remote", SpellOutcome.Neither),
            E(60, SpellBook.Hold, "A still earth-body laid on the living. They stay.", "Anchor", "Earth · Salt · Death · Life · Mercury", "Stone · Life · Mercury", "Remote", SpellOutcome.Restrain)
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
                case SpellBook.GrowHeal: return "Grow / heal (living)";
                case SpellBook.Mind: return "Mind";
                case SpellBook.SeeHide: return "See / hide";
                case SpellBook.Call: return "Call a being";
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
            SpellOutcome outcome)
        {
            return new CodexEntry(number, book, want, name, recipe, via, form, outcome);
        }
    }
}
