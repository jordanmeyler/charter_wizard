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
            string form,
            SpellOutcome outcome)
        {
            Number = number;
            Book = book;
            Want = want;
            Name = name;
            Recipe = recipe;
            Form = form;
            Outcome = outcome;
        }

        public int Number { get; }
        public SpellBook Book { get; }
        public string Want { get; }
        public string Name { get; }
        public string Recipe { get; }
        public string Form { get; }
        public SpellOutcome Outcome { get; }
    }

    /// <summary>
    /// Starting sixty, written from the effect backward.
    /// Life only when the subject or product is living.
    /// </summary>
    public static class SpellCodex
    {
        static readonly CodexEntry[] Entries =
        {
            E(1, SpellBook.End, "Throw fire along a line. A fall of water kills it.", "Fireball", "Fire · Mercury", "Shot", SpellOutcome.Kill),
            E(2, SpellBook.End, "Raise fire behind a fall, or stand a column of flame.", "Flame-pillar", "Fire · Salt", "Pillar", SpellOutcome.Kill),
            E(3, SpellBook.End, "Burn everything around your feet. Plant becomes Ash.", "Inferno", "Fire · Sulphur", "Spread", SpellOutcome.Kill),
            E(4, SpellBook.End, "Light a distant wick, oil, or gas.", "Ignite", "Fire · Sulphur", "Remote", SpellOutcome.Neither),
            E(5, SpellBook.End, "Put a flame out without feeding it.", "Snuff", "Fire · Death", "Remote", SpellOutcome.Neither),
            E(6, SpellBook.End, "Lay dark over a flame so it cannot breathe.", "Smother", "Fire · Dark", "Remote", SpellOutcome.Neither),
            E(7, SpellBook.End, "Throw a dying coal. Ends small fire-life.", "Cinder", "Fire · Death", "Shot", SpellOutcome.Kill),
            E(8, SpellBook.End, "A clean burning line of light-on-fire. Hates Dark.", "Sun-lance", "Fire · Light", "Shot", SpellOutcome.Kill),
            E(9, SpellBook.End, "Fire that goes out and will not come back or warm you.", "Drive", "Fire · Animus", "Shot", SpellOutcome.Kill),
            E(10, SpellBook.End, "An arc of spark. If it hits water, the whole pool dies.", "Lightning bolt", "Spark · Mercury", "Shot", SpellOutcome.Kill),
            E(11, SpellBook.Hold, "Charge the ground you stand on. They cannot step.", "Live-floor", "Spark · Salt", "Spread", SpellOutcome.Kill),
            E(12, SpellBook.Hold, "Stun a thing at a point.", "Jolt", "Spark · Sulphur", "Remote", SpellOutcome.Restrain),
            E(13, SpellBook.End, "Kill a live rod. Death of a spark.", "Blackout", "Spark · Death", "Shot", SpellOutcome.Neither),
            E(14, SpellBook.End, "Spark that also reveals as it strikes.", "Brilliant-arc", "Spark · Light", "Shot", SpellOutcome.Kill),
            E(15, SpellBook.End, "Wet a floor, then the charge takes everyone in it.", "Chain", "Lightning · Water", "Remote", SpellOutcome.Kill),
            E(16, SpellBook.End, "A thundercloud strikes where you point.", "Storm-strike", "Storm · Mercury", "Remote", SpellOutcome.Kill),
            E(17, SpellBook.Hold, "Thunder around you. They drop.", "Thunderclap", "Storm · Sulphur", "Spread", SpellOutcome.Restrain),
            E(18, SpellBook.Weather, "A line of wind. Push a body or a gas. Can spread fire.", "Gale", "Air · Mercury", "Shot", SpellOutcome.Restrain),
            E(19, SpellBook.Cross, "Lift at a point. Jump a gap.", "Updraft", "Air · Mercury", "Remote", SpellOutcome.Neither),
            E(20, SpellBook.Cross, "You hop from where you stand. A living body given air.", "Bound", "Air · Mercury · Salt · Life", "Spread", SpellOutcome.Neither),
            E(21, SpellBook.Cross, "Hold your living body in the air. Stand over a pit.", "Levitation", "Air · Salt · Life", "Spread", SpellOutcome.Neither),
            E(22, SpellBook.Cross, "You fly. Sustained. Life makes the motion yours.", "Flight", "Air · Mercury · Life", "Self", SpellOutcome.Neither),
            E(23, SpellBook.SeeHide, "Cover the room in dark water. Sight-locks fail.", "Fog", "Water · Dark", "Spread", SpellOutcome.Neither),
            E(24, SpellBook.Weather, "Rain on a point. Douses fire. Wets a floor for Chain.", "Rain", "Cloud · Water · Mercury", "Remote", SpellOutcome.Neither),
            E(25, SpellBook.Hold, "Soft stilling from the air. Blankets, slows, white.", "Snowfall", "Water · Air · Salt · Death", "Remote", SpellOutcome.Restrain),
            E(26, SpellBook.SeeHide, "Light from your feet. Night-locks fail.", "Day-wake", "Air · Light", "Spread", SpellOutcome.Neither),
            E(27, SpellBook.SeeHide, "Dark air. Lights die down. They do not go out.", "Gloom", "Air · Dark", "Spread", SpellOutcome.Neither),
            E(28, SpellBook.Weather, "A line of water. Push, douse, fill a channel.", "Water-jet", "Water · Mercury", "Shot", SpellOutcome.Restrain),
            E(29, SpellBook.Hold, "Water from your feet. Bog them. Conduct spark.", "Flood", "Water · Mercury", "Spread", SpellOutcome.Restrain),
            E(30, SpellBook.Cross, "Freeze a fall or raise a column of ice.", "Ice-pillar", "Water · Salt · Death", "Pillar", SpellOutcome.Restrain),
            E(31, SpellBook.Cross, "Freeze a pool into a walkway, or a fall into a wall.", "Ice-sheet", "Water · Salt · Death", "Remote", SpellOutcome.Neither),
            E(32, SpellBook.Hold, "Stop a pool. It stays water. No Salt, no ice.", "Still", "Water · Death", "Remote", SpellOutcome.Restrain),
            E(33, SpellBook.Mind, "Put them to sleep.", "Lull", "Water · Sulphur", "Remote", SpellOutcome.Restrain),
            E(34, SpellBook.Cross, "Pull water, or a floating key, toward a point.", "Draw", "Water · Anima", "Remote", SpellOutcome.Neither),
            E(35, SpellBook.End, "Violent steam along a line. Soft life ends.", "Scald", "Steam · Mercury", "Shot", SpellOutcome.Kill),
            E(36, SpellBook.End, "Throw earth. Fill a pit, or unmake something brittle.", "Hurled stone", "Earth · Mercury", "Shot", SpellOutcome.Kill),
            E(37, SpellBook.Cross, "Raise a wall or bridge of stone.", "Stone pillar", "Earth · Salt", "Pillar", SpellOutcome.Neither),
            E(38, SpellBook.Cross, "Call earth up away from you. The far lip of a pit.", "Raised earth", "Earth · Salt", "Remote", SpellOutcome.Neither),
            E(39, SpellBook.Mind, "They will not enter. Weight and fear.", "Dread", "Earth · Sulphur", "Remote", SpellOutcome.Restrain),
            E(40, SpellBook.End, "Unmake earth-life and golem-bodies from your feet.", "Grave-dust", "Earth · Death", "Spread", SpellOutcome.Kill),
            E(41, SpellBook.Cross, "Quench lava into a path you can walk.", "Obsidian path", "Lava · Water", "Remote", SpellOutcome.Neither),
            E(42, SpellBook.Hold, "Open a dark hollow. They will not step.", "Shadow-well", "Earth · Dark", "Remote", SpellOutcome.Restrain),
            E(43, SpellBook.GrowHeal, "Grow living cover from your feet.", "Sprout", "Water · Earth · Salt · Life", "Spread", SpellOutcome.Neither),
            E(44, SpellBook.GrowHeal, "Grow a living vine as a climb, or a bind if it closes.", "Vine-rise", "Water · Earth · Salt · Life", "Pillar", SpellOutcome.Neither),
            E(45, SpellBook.Hold, "Hold them with living plant. They stay.", "Bind", "Water · Earth · Salt · Life", "Remote", SpellOutcome.Restrain),
            E(46, SpellBook.GrowHeal, "Invite a living plant to grow over there. No fire.", "Call-growth", "Water · Earth · Salt · Life · Anima", "Remote", SpellOutcome.Neither),
            E(47, SpellBook.End, "Rot living verdure. Ends plant-locks. No soul.", "Blight", "Water · Earth · Salt · Life · Death", "Spread", SpellOutcome.Kill),
            E(48, SpellBook.GrowHeal, "Heal a living body that still has its pattern.", "Mend", "Life · Salt · Water", "Spread", SpellOutcome.Neither),
            E(49, SpellBook.GrowHeal, "Cleanse blight, taint, or poison-mind from the living.", "Cleanse", "Life · Water · Light", "Spread", SpellOutcome.Neither),
            E(50, SpellBook.Mind, "Heat their thoughts. They break their own ward, or run into the pit.", "Rage", "Fire · Sulphur", "Remote", SpellOutcome.Neither),
            E(51, SpellBook.Mind, "They flee the room or freeze. Stronger than Dread.", "Terror", "Dark · Sulphur", "Remote", SpellOutcome.Restrain),
            E(52, SpellBook.Mind, "Confuse everyone around you. They miss a mechanism.", "Daze", "Air · Sulphur", "Spread", SpellOutcome.Restrain),
            E(53, SpellBook.Mind, "A will goes out. Soulless beasts obey. Ensouled: Charter fizzles.", "Command", "Sulphur · Animus", "Remote", SpellOutcome.Restrain),
            E(54, SpellBook.SeeHide, "A line of reveal. Traps, ink, things under Dark.", "Gleam", "Light · Mercury", "Shot", SpellOutcome.Neither),
            E(55, SpellBook.SeeHide, "Cloak your living body. Sight-locks fail.", "Veil", "Air · Dark · Life", "Spread", SpellOutcome.Neither),
            E(56, SpellBook.End, "A well of ended dark. Things in it are unmade. Divine.", "Dark matter", "Dark · Death · Salt · Aether", "Remote", SpellOutcome.Kill),
            E(57, SpellBook.Hold, "Gravity without a swallow. They cannot leave. Divine.", "Event well", "Dark · Death · Earth · Aether", "Remote", SpellOutcome.Restrain),
            E(58, SpellBook.Call, "A soulless living servant of earth. Charter-legal.", "Clay servant", "Earth · Salt · Life", "Remote", SpellOutcome.Neither),
            E(59, SpellBook.Call, "Call a living creature. Needs Life, the class, and its formula.", "Call living", "[its matter] · Salt · Life", "Remote", SpellOutcome.Neither),
            E(60, SpellBook.Call, "Call a shade. Not living. Free only.", "Shade", "Dark · Death · Anima", "Remote", SpellOutcome.Neither)
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
            string form,
            SpellOutcome outcome)
        {
            return new CodexEntry(number, book, want, name, recipe, form, outcome);
        }
    }
}
