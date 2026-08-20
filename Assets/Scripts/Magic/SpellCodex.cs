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
        Death,
        Manifest
    }

    public readonly struct CodexEntry
    {
        public CodexEntry(
            SpellBook book,
            string name,
            string recipe,
            string form,
            SpellOutcome outcome,
            string stance,
            string effect)
        {
            Book = book;
            Name = name;
            Recipe = recipe;
            Form = form;
            Outcome = outcome;
            Stance = stance;
            Effect = effect;
        }

        public SpellBook Book { get; }
        public string Name { get; }
        public string Recipe { get; }
        public string Form { get; }
        public SpellOutcome Outcome { get; }
        public string Stance { get; }
        public string Effect { get; }
    }

    /// <summary>
    /// Review ledger of written and proposed spells. Playable locks still
    /// use <see cref="SpellGrammar"/>. Families mix runes in different ways.
    /// </summary>
    public static class SpellCodex
    {
        static readonly CodexEntry[] Entries =
        {
            E(SpellBook.Energy, "Fireball", "Fire · Mercury", "Shot", SpellOutcome.Kill, "Charter", "Fire thrown. Doused by falling water."),
            E(SpellBook.Energy, "Flame-pillar", "Fire · Salt", "Pillar", SpellOutcome.Kill, "Charter", "Standing fire. Lights a wick the bolt cannot reach."),
            E(SpellBook.Energy, "Inferno", "Fire · Sulphur", "Spread", SpellOutcome.Kill, "Charter", "All-consuming from the feet. Eats connected plant. Leaves Ash."),
            E(SpellBook.Energy, "Ignite", "Fire · Sulphur", "Remote", SpellOutcome.Neither, "Charter", "A distant wick, oil, or gas goes."),
            E(SpellBook.Energy, "Snuff", "Fire · Death", "Remote", SpellOutcome.Neither, "Charter", "Death of a flame. Turns a fire-lock without feeding it."),
            E(SpellBook.Energy, "Smother", "Fire · Dark", "Remote", SpellOutcome.Neither, "Charter", "Dark laid over fire."),
            E(SpellBook.Energy, "Cinder", "Fire · Death", "Shot", SpellOutcome.Kill, "Charter", "A dying coal. Ends small fire-life."),
            E(SpellBook.Energy, "Sun-lance", "Fire · Light", "Shot", SpellOutcome.Kill, "Charter", "Light riding fire. Hates Dark-locks."),
            E(SpellBook.Energy, "Drive", "Fire · Animus", "Shot", SpellOutcome.Kill, "Charter", "Projective fire. Goes out and will not warm you."),
            E(SpellBook.Energy, "Wildfire", "Inferno · Mercury", "Spread", SpellOutcome.Kill, "Free", "Inferno given travel. Can leave the tile you needed."),
            E(SpellBook.Energy, "Plasma-arc", "Plasma · Mercury", "Shot", SpellOutcome.Kill, "Divine", "Hottest line. Cuts metal and stone gates."),

            E(SpellBook.SparkStorm, "Lightning bolt", "Spark · Mercury  or  Lightning · Mercury", "Shot", SpellOutcome.Kill, "Charter", "Arc along a line. Water makes the whole pool the lock."),
            E(SpellBook.SparkStorm, "Live-floor", "Spark · Salt", "Spread", SpellOutcome.Kill, "Charter", "Charged ground. Things that stand, end or cannot step."),
            E(SpellBook.SparkStorm, "Jolt", "Spark · Sulphur", "Remote", SpellOutcome.Restrain, "Charter", "Stun. Mind of spark, placed."),
            E(SpellBook.SparkStorm, "Blackout", "Spark · Death", "Shot", SpellOutcome.Neither, "Charter", "Death of a spark. Kills a live rod."),
            E(SpellBook.SparkStorm, "Brilliant-arc", "Spark · Light", "Shot", SpellOutcome.Kill, "Charter", "Spark with Light. Reveals as it strikes."),
            E(SpellBook.SparkStorm, "Chain", "Lightning · Water", "Remote", SpellOutcome.Kill, "Charter", "Wet the floor and call charge. The puddle-kill."),
            E(SpellBook.SparkStorm, "Storm-strike", "Storm · Mercury", "Remote", SpellOutcome.Kill, "Charter", "The cloud strikes where you point."),
            E(SpellBook.SparkStorm, "Thunderclap", "Storm · Sulphur", "Spread", SpellOutcome.Restrain, "Charter", "Mind of the storm. They drop."),

            E(SpellBook.Weather, "Gale", "Air · Mercury", "Shot", SpellOutcome.Restrain, "Charter", "A line of wind. Pushes bodies and gas. Spreads fire."),
            E(SpellBook.Weather, "Updraft", "Air · Mercury", "Remote", SpellOutcome.Neither, "Charter", "Lift at a point. The jump. Cross a gap."),
            E(SpellBook.Weather, "Bound", "Air · Mercury · Salt", "Spread", SpellOutcome.Neither, "Charter", "Your body given air-motion. A short hop."),
            E(SpellBook.Weather, "Levitation", "Air · Salt", "Spread", SpellOutcome.Neither, "Charter", "Body held in air. Stand over a pit."),
            E(SpellBook.Weather, "Flight", "Air · Mercury · Life", "Self", SpellOutcome.Neither, "Charter", "Sustained lift. Life makes the motion yours."),
            E(SpellBook.Weather, "True flight", "Air · Mercury · Aether", "Self", SpellOutcome.Neither, "Divine", "Lift the matter does not owe you."),
            E(SpellBook.Weather, "Storm-ride", "Storm · Mercury · Animus", "Self", SpellOutcome.Neither, "Free", "You go out on the storm."),
            E(SpellBook.Weather, "Still-air", "Air · Salt", "Pillar", SpellOutcome.Neither, "Charter", "A standing quiet. Holds gas or a surge."),
            E(SpellBook.Weather, "Cloudveil", "Cloud · Salt", "Spread", SpellOutcome.Neither, "Charter", "Mist on you. Sight-locks fail."),
            E(SpellBook.Weather, "Fog", "Water · Dark", "Spread", SpellOutcome.Neither, "Charter", "Dark water as cover."),
            E(SpellBook.Weather, "Rain", "Rain · Mercury", "Remote", SpellOutcome.Neither, "Charter", "Douses fire. Wets a floor for Chain."),
            E(SpellBook.Weather, "Snowfall", "Snow · Salt", "Remote", SpellOutcome.Restrain, "Charter", "Soft cold. Slows and blankets."),
            E(SpellBook.Weather, "Blizzard", "Blizzard · Mercury", "Spread", SpellOutcome.Restrain, "Charter", "White-out. They cannot aim."),
            E(SpellBook.Weather, "Sandstorm", "Gale · Dust", "Spread", SpellOutcome.Restrain, "Charter", "Blind and abrade. Clears soft cover."),
            E(SpellBook.Weather, "Day-wake", "Air · Light", "Spread", SpellOutcome.Neither, "Charter", "Light from the feet. Night-locks fail."),
            E(SpellBook.Weather, "Gloom", "Air · Dark", "Spread", SpellOutcome.Neither, "Charter", "Dark air. Lights die down; they do not Snuff."),

            E(SpellBook.Water, "Water-jet", "Water · Mercury", "Shot", SpellOutcome.Restrain, "Charter", "A line of water. Pushes, douses, fills a channel."),
            E(SpellBook.Water, "Flood", "Water · Mercury", "Spread", SpellOutcome.Restrain, "Charter", "Water from the feet. Bogs and conducts."),
            E(SpellBook.Water, "Spring", "Water · Life", "Spread", SpellOutcome.Neither, "Charter", "A well. Wets. Grows what is already Plant."),
            E(SpellBook.Water, "Ice-pillar", "Water · Salt · Cold", "Pillar", SpellOutcome.Restrain, "Charter", "Standing ice. Bridge, pin, freeze a fall."),
            E(SpellBook.Water, "Ice-sheet", "Water · Salt · Cold", "Remote", SpellOutcome.Neither, "Charter", "A pool becomes walkable."),
            E(SpellBook.Water, "Glacier", "Ice · Stone", "Pillar", SpellOutcome.Restrain, "Charter", "Permanent cold mass. Shuts a pass."),
            E(SpellBook.Water, "Lull", "Water · Sulphur", "Remote", SpellOutcome.Restrain, "Charter", "Sleep. Mind of water."),
            E(SpellBook.Water, "Draw", "Water · Anima", "Remote", SpellOutcome.Neither, "Charter", "Receptive pull. Calls water. Does not strike."),
            E(SpellBook.Water, "Still-font", "Water · Anima", "Pillar", SpellOutcome.Neither, "Charter", "A standing well that receives."),
            E(SpellBook.Water, "Steam-veil", "Steam · Mercury", "Shot", SpellOutcome.Restrain, "Charter", "Scalding fill. They will not walk it."),
            E(SpellBook.Water, "Scald", "Steam · Mercury", "Shot", SpellOutcome.Kill, "Charter", "Violent Fire+Water. Ends soft life."),
            E(SpellBook.Water, "Acid etch", "Acid · Mercury", "Shot", SpellOutcome.Neither, "Charter", "Corrodes metal locks and stone gates."),

            E(SpellBook.Stone, "Hurled stone", "Earth · Mercury", "Shot", SpellOutcome.Kill, "Charter", "Earth given motion. Fills a pit or unmakes a brittle lock."),
            E(SpellBook.Stone, "Stone pillar", "Earth · Salt", "Pillar", SpellOutcome.Neither, "Charter", "Standing earth. Bridge, wall, pin."),
            E(SpellBook.Stone, "Raised earth", "Earth · Salt", "Remote", SpellOutcome.Neither, "Charter", "Earth away from you. The pit from the far lip."),
            E(SpellBook.Stone, "Quake", "Earth · Mercury", "Spread", SpellOutcome.Restrain, "Charter", "Ground will not be stood on."),
            E(SpellBook.Stone, "Dread", "Earth · Sulphur", "Remote", SpellOutcome.Restrain, "Charter", "Weight and fear. They will not enter."),
            E(SpellBook.Stone, "Menhir", "Earth · Life", "Pillar", SpellOutcome.Neither, "Charter", "Earth asked to live as a standing stone."),
            E(SpellBook.Stone, "Grave-dust", "Earth · Death", "Spread", SpellOutcome.Kill, "Charter", "Unmakes earth-life and golem-bodies."),
            E(SpellBook.Stone, "Shadow-well", "Earth · Dark", "Remote", SpellOutcome.Restrain, "Charter", "A dark hollow. Prelude to Dark matter."),
            E(SpellBook.Stone, "Obsidian path", "Lava · Water", "Remote", SpellOutcome.Neither, "Charter", "Quench. Walk the hazard."),
            E(SpellBook.Stone, "Metal-bar", "Metal · Salt", "Pillar", SpellOutcome.Neither, "Charter", "A forged standing piece. Conducts. Cage."),
            E(SpellBook.Stone, "Glass pane", "Glass · Salt", "Remote", SpellOutcome.Neither, "Charter", "Brittle sheet. Sight through; Spark shatters."),
            E(SpellBook.Stone, "Crystal store", "Crystal · Aether", "Remote", SpellOutcome.Neither, "Divine", "Holds a rune for later."),

            E(SpellBook.Life, "Sprout", "Plant · Life", "Spread", SpellOutcome.Neither, "Charter", "Growth from the feet."),
            E(SpellBook.Life, "Vine-rise", "Plant · Life", "Pillar", SpellOutcome.Neither, "Charter", "A climb, or a bind if it closes."),
            E(SpellBook.Life, "Bind", "Vine · Salt", "Remote", SpellOutcome.Restrain, "Charter", "Plant given lasting hold."),
            E(SpellBook.Life, "Call-growth", "Plant · Anima", "Remote", SpellOutcome.Neither, "Charter", "Plant invited at a distance."),
            E(SpellBook.Life, "Forest", "Plant · Water · Life", "Spread", SpellOutcome.Neither, "Charter", "Dense cover. Feeds Inferno."),
            E(SpellBook.Life, "Thorn", "Plant · Dry", "Shot", SpellOutcome.Restrain, "Charter", "A specific plant. They stop."),
            E(SpellBook.Life, "Nightshade", "Plant · Dark · Sulphur", "Remote", SpellOutcome.Restrain, "Charter", "Sleep that is not Lull."),
            E(SpellBook.Life, "Blight", "Plant · Death", "Spread", SpellOutcome.Kill, "Charter", "Rot. Ends plant-locks. No soul."),
            E(SpellBook.Life, "Ashen field", "Fire · Plant", "Remote", SpellOutcome.Neither, "Charter", "What burning leaves. Smothers or fertilises."),
            E(SpellBook.Life, "Mend", "Life · Salt · Water", "Spread", SpellOutcome.Neither, "Charter", "Body restored. The heal."),
            E(SpellBook.Life, "Cleanse", "Life · Water · Light", "Spread", SpellOutcome.Neither, "Charter", "Blight, surface taint, poison-mind."),
            E(SpellBook.Life, "Greater mend", "Life · Anima · Water · Aether", "Self", SpellOutcome.Neither, "Divine", "Life that was not left in the body."),
            E(SpellBook.Life, "Last breath", "Life · Death · Aether", "Self", SpellOutcome.Neither, "Free", "A stalled heart goes again. Charter calls it trespass."),

            E(SpellBook.Mind, "Rage", "Fire · Sulphur", "Remote", SpellOutcome.Neither, "Charter", "They break their own ward or flee into the pit."),
            E(SpellBook.Mind, "Lull", "Water · Sulphur", "Remote", SpellOutcome.Restrain, "Charter", "Sleep."),
            E(SpellBook.Mind, "Dread", "Earth · Sulphur", "Remote", SpellOutcome.Restrain, "Charter", "They will not pass."),
            E(SpellBook.Mind, "Daze", "Air · Sulphur", "Spread", SpellOutcome.Restrain, "Charter", "Confusion. They miss a mechanism."),
            E(SpellBook.Mind, "Terror", "Dark · Sulphur", "Remote", SpellOutcome.Restrain, "Charter", "They flee the room or freeze."),
            E(SpellBook.Mind, "Awe", "Light · Sulphur", "Remote", SpellOutcome.Restrain, "Charter", "They kneel."),
            E(SpellBook.Mind, "Command", "Sulphur · Animus", "Remote", SpellOutcome.Restrain, "Free", "A will goes out. Ensouled: Charter fizzles."),
            E(SpellBook.Mind, "Charm", "Sulphur · Anima", "Remote", SpellOutcome.Restrain, "Free", "A will is received. Same soul rule."),
            E(SpellBook.Mind, "Silence", "Air · Death · Sulphur", "Spread", SpellOutcome.Neither, "Charter", "Minds around you cannot speak runes."),

            E(SpellBook.LightDark, "Gleam", "Light · Mercury", "Shot", SpellOutcome.Neither, "Charter", "A line of reveal."),
            E(SpellBook.LightDark, "Lantern", "Light · Salt", "Pillar", SpellOutcome.Neither, "Charter", "Standing light."),
            E(SpellBook.LightDark, "Veil", "Dark · Salt", "Spread", SpellOutcome.Neither, "Charter", "A cloak on the self."),
            E(SpellBook.LightDark, "Umbral pinch", "Dark · Mercury", "Shot", SpellOutcome.Restrain, "Charter", "Moving dark. Holds a limb, a door, a flame."),
            E(SpellBook.LightDark, "Dark matter", "Dark · Death · Salt · Aether", "Remote", SpellOutcome.Kill, "Divine", "A body of ended dark. The well unmakes."),
            E(SpellBook.LightDark, "Event well", "Dark · Death · Earth · Aether", "Remote", SpellOutcome.Restrain, "Divine", "Gravity without mass. They cannot leave."),
            E(SpellBook.LightDark, "Nigredo", "Dark · Death · Aether", "Spread", SpellOutcome.Kill, "Free", "The blackening from the feet. Taint."),

            E(SpellBook.Death, "Still", "Water · Death", "Remote", SpellOutcome.Restrain, "Charter", "A body of water stops. Add Salt and it is a kill — ensouled, Charter refuses."),
            E(SpellBook.Death, "Reaping", "Death · Mercury", "Shot", SpellOutcome.Kill, "Free", "Death given travel."),
            E(SpellBook.Death, "Unmake", "Death · Aether", "Remote", SpellOutcome.Kill, "Divine", "The pattern comes apart."),

            E(SpellBook.Manifest, "Clay servant", "Earth · Salt · Life", "Remote", SpellOutcome.Neither, "Charter", "Homunculus. Soulless body of earth."),
            E(SpellBook.Manifest, "Call mite", "Fire · Salt · Life", "Remote", SpellOutcome.Neither, "Charter", "Emberkin class + mite formula {Fire · Salt}."),
            E(SpellBook.Manifest, "Raise hound", "Water · Earth · Salt · Life + formula", "Remote", SpellOutcome.Neither, "Charter", "Beast class plus the specific dog."),
            E(SpellBook.Manifest, "Fetch-bird", "Air · Salt · Life + formula", "Remote", SpellOutcome.Neither, "Charter", "Flies a key across a gap."),
            E(SpellBook.Manifest, "Plant-thing", "Plant · Life · Aether", "Remote", SpellOutcome.Neither, "Charter", "Walking verdure. Codex plant, animated."),
            E(SpellBook.Manifest, "Spark-wisp", "Spark · Life · Air", "Remote", SpellOutcome.Neither, "Charter", "Soulless charge."),
            E(SpellBook.Manifest, "Shade", "Dark · Death · Anima", "Remote", SpellOutcome.Neither, "Free", "No body. Charter cannot."),
            E(SpellBook.Manifest, "Wight", "Death · Aether · Salt · Anima", "Remote", SpellOutcome.Neither, "Free", "Dead body, bound. Soul-work."),
            E(SpellBook.Manifest, "Elemental", "[root] · Life · Aether", "Remote", SpellOutcome.Neither, "Divine", "A temporary root, aware.")
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
                case SpellBook.Stone: return "Stone & metal";
                case SpellBook.Life: return "Life, heal, plant";
                case SpellBook.Mind: return "Mind";
                case SpellBook.LightDark: return "Light & Dark";
                case SpellBook.Death: return "Death";
                case SpellBook.Manifest: return "Manifestation";
                default: return book.ToString();
            }
        }

        static CodexEntry E(
            SpellBook book,
            string name,
            string recipe,
            string form,
            SpellOutcome outcome,
            string stance,
            string effect)
        {
            return new CodexEntry(book, name, recipe, form, outcome, stance, effect);
        }
    }
}
