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
        Energy,
        SparkStorm,
        Weather,
        Water,
        Stone,
        Life,
        Mind,
        LightDark,
        Manifest
    }

    public readonly struct CodexEntry
    {
        public CodexEntry(
            int number,
            SpellBook book,
            string name,
            string recipe,
            string form,
            SpellOutcome outcome,
            string effect)
        {
            Number = number;
            Book = book;
            Name = name;
            Recipe = recipe;
            Form = form;
            Outcome = outcome;
            Effect = effect;
        }

        public int Number { get; }
        public SpellBook Book { get; }
        public string Name { get; }
        public string Recipe { get; }
        public string Form { get; }
        public SpellOutcome Outcome { get; }
        public string Effect { get; }
    }

    /// <summary>
    /// The starting sixty. Ice uses Death (stilling), not Cold.
    /// Playable locks still resolve through <see cref="SpellGrammar"/>.
    /// </summary>
    public static class SpellCodex
    {
        static readonly CodexEntry[] Entries =
        {
            E(1, SpellBook.Energy, "Fireball", "Fire · Mercury", "Shot", SpellOutcome.Kill, "Fire thrown. Doused by a fall of water."),
            E(2, SpellBook.Energy, "Flame-pillar", "Fire · Salt", "Pillar", SpellOutcome.Kill, "Standing fire. Lights a wick a bolt cannot reach."),
            E(3, SpellBook.Energy, "Inferno", "Fire · Sulphur", "Spread", SpellOutcome.Kill, "Erupts from the feet. Eats connected plant. Leaves Ash."),
            E(4, SpellBook.Energy, "Ignite", "Fire · Sulphur", "Remote", SpellOutcome.Neither, "A distant wick, oil, or gas takes."),
            E(5, SpellBook.Energy, "Snuff", "Fire · Death", "Remote", SpellOutcome.Neither, "Death of a flame. Turns a fire-lock without feeding it."),
            E(6, SpellBook.Energy, "Smother", "Fire · Dark", "Remote", SpellOutcome.Neither, "Dark laid over fire."),
            E(7, SpellBook.Energy, "Cinder", "Fire · Death", "Shot", SpellOutcome.Kill, "A dying coal. Ends small fire-life."),
            E(8, SpellBook.Energy, "Sun-lance", "Fire · Light", "Shot", SpellOutcome.Kill, "Light riding fire. Hates Dark-locks."),
            E(9, SpellBook.Energy, "Drive", "Fire · Animus", "Shot", SpellOutcome.Kill, "Projective fire. Goes out. Will not warm you."),
            E(10, SpellBook.SparkStorm, "Lightning bolt", "Spark · Mercury", "Shot", SpellOutcome.Kill, "An arc. Water makes the whole pool the lock."),
            E(11, SpellBook.SparkStorm, "Live-floor", "Spark · Salt", "Spread", SpellOutcome.Kill, "Charged ground around you."),
            E(12, SpellBook.SparkStorm, "Jolt", "Spark · Sulphur", "Remote", SpellOutcome.Restrain, "Stun. Mind of spark."),
            E(13, SpellBook.SparkStorm, "Blackout", "Spark · Death", "Shot", SpellOutcome.Neither, "Death of a spark. Kills a live rod."),
            E(14, SpellBook.SparkStorm, "Brilliant-arc", "Spark · Light", "Shot", SpellOutcome.Kill, "Spark with Light. Reveals as it strikes."),
            E(15, SpellBook.SparkStorm, "Chain", "Lightning · Water", "Remote", SpellOutcome.Kill, "Wet the floor and call charge."),
            E(16, SpellBook.SparkStorm, "Storm-strike", "Storm · Mercury", "Remote", SpellOutcome.Kill, "The cloud strikes where you point."),
            E(17, SpellBook.SparkStorm, "Thunderclap", "Storm · Sulphur", "Spread", SpellOutcome.Restrain, "Mind of the storm. They drop."),
            E(18, SpellBook.Weather, "Gale", "Air · Mercury", "Shot", SpellOutcome.Restrain, "Wind on a line. Pushes. Spreads fire."),
            E(19, SpellBook.Weather, "Updraft", "Air · Mercury", "Remote", SpellOutcome.Neither, "Lift at a point. The jump."),
            E(20, SpellBook.Weather, "Bound", "Air · Mercury · Salt", "Spread", SpellOutcome.Neither, "Your body given air-motion. A hop."),
            E(21, SpellBook.Weather, "Levitation", "Air · Salt", "Spread", SpellOutcome.Neither, "Body held in air. Stand over a pit."),
            E(22, SpellBook.Weather, "Flight", "Air · Mercury · Life", "Self", SpellOutcome.Neither, "Sustained lift. Life makes the motion yours."),
            E(23, SpellBook.Weather, "Fog", "Water · Dark", "Spread", SpellOutcome.Neither, "Dark water as cover."),
            E(24, SpellBook.Weather, "Rain", "Cloud · Water · Mercury", "Remote", SpellOutcome.Neither, "Douses fire. Wets a floor for Chain."),
            E(25, SpellBook.Weather, "Snowfall", "Water · Air · Salt · Death", "Remote", SpellOutcome.Restrain, "Soft stilling from a cloud. Blankets and slows."),
            E(26, SpellBook.Weather, "Day-wake", "Air · Light", "Spread", SpellOutcome.Neither, "Light from the feet. Night-locks fail."),
            E(27, SpellBook.Weather, "Gloom", "Air · Dark", "Spread", SpellOutcome.Neither, "Dark air. Lights die down."),
            E(28, SpellBook.Water, "Water-jet", "Water · Mercury", "Shot", SpellOutcome.Restrain, "A line of water. Pushes, douses, fills a channel."),
            E(29, SpellBook.Water, "Flood", "Water · Mercury", "Spread", SpellOutcome.Restrain, "Water from the feet. Bogs and conducts."),
            E(30, SpellBook.Water, "Spring", "Water · Life", "Spread", SpellOutcome.Neither, "A well. Grows what is already Plant."),
            E(31, SpellBook.Water, "Ice-pillar", "Water · Salt · Death", "Pillar", SpellOutcome.Restrain, "Water given body, then stilled. Bridge, pin, freeze a fall."),
            E(32, SpellBook.Water, "Ice-sheet", "Water · Salt · Death", "Remote", SpellOutcome.Neither, "A pool becomes walkable. A fall becomes a wall."),
            E(33, SpellBook.Water, "Still", "Water · Death", "Remote", SpellOutcome.Restrain, "The pool stops. It stays water. No Salt, no ice."),
            E(34, SpellBook.Water, "Lull", "Water · Sulphur", "Remote", SpellOutcome.Restrain, "Sleep. Mind of water."),
            E(35, SpellBook.Water, "Draw", "Water · Anima", "Remote", SpellOutcome.Neither, "Receptive pull. Calls water or a floating key."),
            E(36, SpellBook.Water, "Scald", "Steam · Mercury", "Shot", SpellOutcome.Kill, "Violent Fire·Water in motion."),
            E(37, SpellBook.Stone, "Hurled stone", "Earth · Mercury", "Shot", SpellOutcome.Kill, "Earth given motion. Fills a pit or unmakes a brittle lock."),
            E(38, SpellBook.Stone, "Stone pillar", "Earth · Salt", "Pillar", SpellOutcome.Neither, "Standing earth. Bridge, wall, pin."),
            E(39, SpellBook.Stone, "Raised earth", "Earth · Salt", "Remote", SpellOutcome.Neither, "Earth away from you. The pit from the far lip."),
            E(40, SpellBook.Stone, "Dread", "Earth · Sulphur", "Remote", SpellOutcome.Restrain, "Weight and fear. They will not enter."),
            E(41, SpellBook.Stone, "Menhir", "Earth · Life", "Pillar", SpellOutcome.Neither, "Earth asked to live as a standing stone."),
            E(42, SpellBook.Stone, "Grave-dust", "Earth · Death", "Spread", SpellOutcome.Kill, "Unmakes earth-life and golem-bodies."),
            E(43, SpellBook.Stone, "Obsidian path", "Lava · Water", "Remote", SpellOutcome.Neither, "Quench. Walk the hazard that blocked you."),
            E(44, SpellBook.Stone, "Shadow-well", "Earth · Dark", "Remote", SpellOutcome.Restrain, "A dark hollow. Prelude to Dark matter."),
            E(45, SpellBook.Life, "Sprout", "Plant · Life", "Spread", SpellOutcome.Neither, "Growth from the feet. Plant is Water·Earth·Salt."),
            E(46, SpellBook.Life, "Vine-rise", "Plant · Life", "Pillar", SpellOutcome.Neither, "A climb, or a bind if it closes."),
            E(47, SpellBook.Life, "Bind", "Plant · Life · Salt", "Remote", SpellOutcome.Restrain, "Plant given lasting hold."),
            E(48, SpellBook.Life, "Call-growth", "Plant · Anima", "Remote", SpellOutcome.Neither, "Plant invited at a distance."),
            E(49, SpellBook.Life, "Blight", "Plant · Death", "Spread", SpellOutcome.Kill, "Rot. Ends plant-locks. No soul."),
            E(50, SpellBook.Life, "Mend", "Life · Salt · Water", "Spread", SpellOutcome.Neither, "The heal. Restores a body that still has its pattern."),
            E(51, SpellBook.Life, "Cleanse", "Life · Water · Light", "Spread", SpellOutcome.Neither, "Blight, surface taint, poison-mind."),
            E(52, SpellBook.Mind, "Rage", "Fire · Sulphur", "Remote", SpellOutcome.Neither, "They break their own ward or flee into the pit."),
            E(53, SpellBook.Mind, "Terror", "Dark · Sulphur", "Remote", SpellOutcome.Restrain, "They flee the room or freeze."),
            E(54, SpellBook.Mind, "Daze", "Air · Sulphur", "Spread", SpellOutcome.Restrain, "Confusion. They miss a mechanism."),
            E(55, SpellBook.Mind, "Command", "Sulphur · Animus", "Remote", SpellOutcome.Restrain, "A will goes out. Ensouled: Charter fizzles."),
            E(56, SpellBook.LightDark, "Gleam", "Light · Mercury", "Shot", SpellOutcome.Neither, "A line of reveal."),
            E(57, SpellBook.LightDark, "Veil", "Dark · Salt", "Spread", SpellOutcome.Neither, "A cloak on the self."),
            E(58, SpellBook.LightDark, "Dark matter", "Dark · Death · Salt · Aether", "Remote", SpellOutcome.Kill, "A body of ended dark. The well unmakes. Divine."),
            E(59, SpellBook.Manifest, "Clay servant", "Earth · Salt · Life", "Remote", SpellOutcome.Neither, "Homunculus. Soulless body of earth."),
            E(60, SpellBook.Manifest, "Shade", "Dark · Death · Anima", "Remote", SpellOutcome.Neither, "No body. Free only.")
        };

        public static IReadOnlyList<CodexEntry> All => Entries;

        public static string BookName(SpellBook book)
        {
            switch (book)
            {
                case SpellBook.Energy: return "Energy";
                case SpellBook.SparkStorm: return "Spark & storm";
                case SpellBook.Weather: return "Weather & air";
                case SpellBook.Water: return "Water & ice";
                case SpellBook.Stone: return "Stone & earth";
                case SpellBook.Life: return "Life & heal";
                case SpellBook.Mind: return "Mind";
                case SpellBook.LightDark: return "Light, Dark & high";
                case SpellBook.Manifest: return "Manifest";
                default: return book.ToString();
            }
        }

        static CodexEntry E(
            int number,
            SpellBook book,
            string name,
            string recipe,
            string form,
            SpellOutcome outcome,
            string effect)
        {
            return new CodexEntry(number, book, name, recipe, form, outcome, effect);
        }
    }
}
