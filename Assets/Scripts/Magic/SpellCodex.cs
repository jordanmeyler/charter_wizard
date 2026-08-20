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
            SpellId spell,
            string want,
            string name,
            string recipe,
            string via,
            string form,
            SpellOutcome outcome,
            string gate,
            SpellId work = SpellId.None)
        {
            Number = number;
            Book = book;
            Spell = spell;
            Want = want;
            Name = name;
            Recipe = recipe;
            Via = via;
            Form = form;
            Outcome = outcome;
            Gate = gate;
            Work = work == SpellId.None ? spell : work;
            Shape = ChainBook.TryParseShape(form, out var shape) ? shape : SpellShape.None;
            RecipeRunes = ChainBook.Parse(recipe);
            ViaRunes = ChainBook.Parse(via);
        }

        public int Number { get; }
        public SpellBook Book { get; }
        public SpellId Spell { get; }
        public string Want { get; }
        public string Name { get; }
        public string Recipe { get; }
        public string Via { get; }
        public string Form { get; }
        public SpellOutcome Outcome { get; }
        public string Gate { get; }
        public SpellId Work { get; }
        public SpellShape Shape { get; }
        public IReadOnlyList<RuneId> RecipeRunes { get; }
        public IReadOnlyList<RuneId> ViaRunes { get; }

        public bool FreeOnly => Gate == "Free";
    }

    /// <summary>
    /// Written story-chains. 1–40 are the ordinary book (no Death).
    /// 41–50 are Death / Free. 51 is Time-stop, a longer Free working.
    /// Life only marks a living recipe.
    /// </summary>
    public static class SpellCodex
    {
        static CodexEntry[] Entries = BuiltIn();

        static CodexEntry[] BuiltIn() => new[]
        {
            E(1, SpellBook.End, SpellId.Fireball, "A seed of heat that flies.", "Fireball", "Fire · Air · Mercury", "Spark · Mercury", "Shot", SpellOutcome.Kill),
            E(2, SpellBook.End, SpellId.FlamePillar, "Hunger given a standing body and asked to rest. It stands.", "Flame-pillar", "Fire · Salt · Earth", "Flame · Earth", "Pillar", SpellOutcome.Kill),
            E(3, SpellBook.Cross, SpellId.Melt, "Hunger sent into a thing. No breath, so it does not fly.", "Melt", "Fire · Mercury", "", "Remote", SpellOutcome.Neither),
            E(4, SpellBook.End, SpellId.Smother, "Hunger needs breath; that breath is withheld.", "Smother", "Fire · Air · Dark", "Spark · Dark", "Remote", SpellOutcome.Neither),
            E(5, SpellBook.End, SpellId.SunLance, "Hunger shown, given breath, sent as a clean line.", "Sun-lance", "Fire · Light · Air · Mercury", "Spark · Light · Mercury", "Shot", SpellOutcome.Kill),
            E(6, SpellBook.End, SpellId.Ignite, "Hunger’s wildcard given a standing body — a wick that stays.", "Ignite", "Fire · Sulphur · Salt", "", "Remote", SpellOutcome.Neither),
            E(7, SpellBook.End, SpellId.LightningBolt, "The seed stretched through more breath and sent. A path, not a body.", "Lightning", "Fire · Air · Air · Mercury", "Spark · Air · Mercury", "Shot", SpellOutcome.Kill),
            E(8, SpellBook.End, SpellId.ChainLightning, "That path finds yield given a body. The pool is what dies.", "Chain", "Fire · Air · Air · Mercury · Water · Salt", "Lightning · Mercury · Water · Salt", "Remote", SpellOutcome.Kill),
            E(9, SpellBook.Hold, SpellId.LiveFloor, "The seed given a body around your feet. They cannot step.", "Live-floor", "Fire · Air · Salt", "Spark · Salt", "Spread", SpellOutcome.Kill),
            E(10, SpellBook.Hold, SpellId.Jolt, "The moving spark, turned by Sulphur, reaches a mind.", "Jolt", "Fire · Air · Sulphur · Mercury", "Spark · Sulphur · Mercury", "Remote", SpellOutcome.Restrain),
            E(11, SpellBook.Hold, SpellId.Thunderclap, "The arc meets rest, then every mind around you.", "Thunderclap", "Fire · Air · Air · Earth · Sulphur", "Lightning · Earth · Sulphur", "Spread", SpellOutcome.Restrain),
            E(12, SpellBook.Weather, SpellId.StormCall, "Breath holds yield; a seed is inside. Weather arrives.", "Storm", "Air · Water · Fire · Air", "Cloud · Spark", "Remote", SpellOutcome.Kill),
            E(13, SpellBook.Weather, SpellId.Rain, "The hanging veil is drawn down. Fire drowns.", "Rain", "Air · Water · Earth", "Cloud · Earth", "Remote", SpellOutcome.Neither),
            E(14, SpellBook.SeeHide, SpellId.Fog, "The hanging veil is withheld and given a body.", "Fog", "Air · Water · Dark · Salt", "Cloud · Dark · Salt", "Spread", SpellOutcome.Neither),
            E(15, SpellBook.End, SpellId.Scald, "Hunger forced through yield and sent.", "Scald", "Fire · Water · Mercury", "Steam · Mercury", "Shot", SpellOutcome.Kill),
            E(16, SpellBook.Weather, SpellId.WaterJet, "Yield learns breath so it can leave the vessel, then is sent.", "Water-jet", "Water · Air · Mercury", "", "Shot", SpellOutcome.Restrain),
            E(17, SpellBook.Hold, SpellId.Flood, "Yield going, more yield, given a body. They bog.", "Flood", "Water · Mercury · Water · Salt", "Current · Water · Salt", "Spread", SpellOutcome.Restrain),
            E(18, SpellBook.Cross, SpellId.IcePillar, "Yield given a body and asked to rest. Hard water. It will thaw.", "Ice-pillar", "Water · Salt · Earth", "Ice", "Pillar", SpellOutcome.Restrain),
            E(19, SpellBook.Hold, SpellId.IceSpear, "Hard water going — not stood as a pillar.", "Ice-spear", "Water · Earth · Mercury", "", "Shot", SpellOutcome.Restrain),
            E(20, SpellBook.Hold, SpellId.Snowfall, "The veil is given ice’s story and sent softly.", "Snowfall", "Air · Water · Salt · Earth · Mercury", "Cloud · Ice · Mercury", "Remote", SpellOutcome.Restrain),
            E(21, SpellBook.Cross, SpellId.Thaw, "The hard water-body meets hunger and remembers yield.", "Thaw", "Water · Salt · Earth · Fire", "Ice · Fire", "Remote", SpellOutcome.Neither),
            E(22, SpellBook.End, SpellId.HurledStone, "Rest asked to go. Earth flies.", "Hurled stone", "Earth · Mercury", "", "Shot", SpellOutcome.Kill),
            E(23, SpellBook.Cross, SpellId.Wall, "A body of rest asked to rest as more rest. A wall.", "Wall", "Earth · Salt · Earth", "Stone · Earth", "Pillar", SpellOutcome.Neither),
            E(24, SpellBook.Cross, SpellId.Pit, "Rest asked to go, given breath so it leaves a hollow.", "Pit", "Earth · Mercury · Air", "", "Remote", SpellOutcome.Neither),
            E(25, SpellBook.Cross, SpellId.Bridge, "A body of rest given breath and sent across.", "Bridge", "Earth · Salt · Air · Mercury", "Stone · Air · Mercury", "Remote", SpellOutcome.Neither),
            E(26, SpellBook.Hold, SpellId.Quagmire, "Rest meeting yield, given a body. It holds them.", "Quagmire", "Earth · Water · Salt", "Mud · Salt", "Spread", SpellOutcome.Restrain),
            E(27, SpellBook.End, SpellId.LavaFlood, "Hungry earth asked to go.", "Lava-flood", "Fire · Earth · Mercury", "Lava · Mercury", "Remote", SpellOutcome.Kill),
            E(28, SpellBook.Cross, SpellId.ObsidianPath, "Hungry earth quenched and given a body. A path.", "Obsidian path", "Fire · Earth · Water · Salt", "Lava · Water · Salt", "Remote", SpellOutcome.Neither),
            E(29, SpellBook.GrowHeal, SpellId.Sprout, "Wet rest given a vegetable body, then marked living.", "Sprout", "Water · Earth · Salt · Life", "Plant · Life", "Spread", SpellOutcome.Neither),
            E(30, SpellBook.Hold, SpellId.Vine, "That living plant is sent. It holds them, or it climbs.", "Vine", "Water · Earth · Salt · Life · Mercury", "Grove · Mercury", "Remote", SpellOutcome.Restrain),
            E(31, SpellBook.GrowHeal, SpellId.VineRise, "That living plant is asked to stand.", "Vine-rise", "Water · Earth · Salt · Life · Earth", "Grove · Earth", "Pillar", SpellOutcome.Neither),
            E(32, SpellBook.GrowHeal, SpellId.Mend, "A living body, yield and rest, sent into the living.", "Mend", "Life · Salt · Water · Earth · Mercury", "", "Spread", SpellOutcome.Neither),
            E(33, SpellBook.Cross, SpellId.Hop, "Breath given a body, then more breath, kept on you. A leap.", "Hop", "Air · Salt · Air", "Air · Salt · Mercury", "Self", SpellOutcome.Neither),
            E(34, SpellBook.Cross, SpellId.Flight, "Breath going, given a body, kept on you. You fly.", "Flight", "Air · Mercury · Salt", "Air · Mercury · Salt · Life · Mercury", "Self", SpellOutcome.Neither),
            E(35, SpellBook.Mind, SpellId.Rage, "Melt turned by Sulphur: hunger’s wildcard sent into a mind.", "Rage", "Fire · Sulphur · Mercury", "", "Remote", SpellOutcome.Neither),
            E(36, SpellBook.Mind, SpellId.Terror, "The withheld reaches a mind. They flee or freeze.", "Terror", "Dark · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(37, SpellBook.Mind, SpellId.Lull, "Yield reaches a mind. They sleep. They can be woken.", "Lull", "Water · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(38, SpellBook.Weather, SpellId.Gale, "Breath going, more breath, so it can push.", "Gale", "Air · Mercury · Air", "Wind · Air", "Shot", SpellOutcome.Restrain),
            E(39, SpellBook.SeeHide, SpellId.Veil, "The withheld, a living body, as breath. Hard to see.", "Veil", "Dark · Life · Salt · Air", "", "Spread", SpellOutcome.Neither),
            E(40, SpellBook.Call, SpellId.CallBeast, "Flesh, marked living, given a mind, sent here. Know the formula.", "Call beast", "Earth · Water · Salt · Life · Sulphur · Mercury", "", "Remote", SpellOutcome.Neither),
            E(41, SpellBook.Grave, SpellId.Blight, "A living plant, then the grave. Verdure rots. No soul.", "Blight", "Water · Earth · Salt · Life · Death", "Grove · Death", "Spread", SpellOutcome.Kill, "Either"),
            E(42, SpellBook.Grave, SpellId.Shade, "Withheld, given a body, marked by the grave, and sent.", "Shade", "Dark · Death · Salt · Mercury", "Shade · Mercury", "Remote", SpellOutcome.Neither, "Free"),
            E(43, SpellBook.Grave, SpellId.Unmake, "The grave is sent into a living body.", "Unmake", "Death · Mercury · Life · Salt", "", "Remote", SpellOutcome.Kill, "Free"),
            E(44, SpellBook.Grave, SpellId.GraveSleep, "The waking passion is given to the grave. Sleep as if dead.", "Grave-sleep", "Life · Sulphur · Death", "", "Remote", SpellOutcome.Restrain, "Free"),
            E(45, SpellBook.Grave, SpellId.CorpseCall, "The four as a body, marked by the grave, and sent.", "Corpse-call", "Salt · Water · Earth · Fire · Death · Mercury", "", "Remote", SpellOutcome.Neither, "Free"),
            E(46, SpellBook.Grave, SpellId.GraveDust, "Rest marked by the grave. Earth-life and golems come apart.", "Grave-dust", "Earth · Death · Salt", "", "Spread", SpellOutcome.Kill, "Either"),
            E(47, SpellBook.Grave, SpellId.Snuff, "Hunger marked by the grave and sent into a flame.", "Snuff", "Fire · Death · Mercury", "Ember · Mercury", "Remote", SpellOutcome.Neither, "Either"),
            E(48, SpellBook.Grave, SpellId.Blackout, "The seed marked by the grave and sent. A live rod dies.", "Blackout", "Fire · Air · Death · Mercury", "Spark · Death · Mercury", "Shot", SpellOutcome.Neither, "Either"),
            E(49, SpellBook.Grave, SpellId.GraveIce, "Yield given a body, then the grave. Ice that will not thaw.", "Grave-ice", "Water · Salt · Death", "", "Remote", SpellOutcome.Restrain, "Either"),
            E(50, SpellBook.Grave, SpellId.LastBreath, "Living breath, then the grave, sent. The breath leaves them.", "Last breath", "Air · Life · Death · Mercury", "", "Remote", SpellOutcome.Kill, "Free"),
            E(51, SpellBook.Hold, SpellId.TimeStop, "Yield and rest are withheld. The living stay; the motion of instants leaves; the mind cannot hurry.", "Time-stop", "Water · Earth · Dark · Life · Death · Sulphur · Salt", "Mud · Dark · Life · Death · Sulphur · Salt", "Spread", SpellOutcome.Restrain, "Free")
        };

        public static IReadOnlyList<CodexEntry> All
        {
            get
            {
                CatalogBook.EnsureLoaded();
                return Entries;
            }
        }

        public static void Replace(CodexEntry[] entries)
        {
            if (entries != null && entries.Length > 0)
            {
                Entries = entries;
            }
        }

        public static SpellId WorkOf(SpellId spell)
        {
            return TryGet(spell, out var entry) && entry.Work != SpellId.None
                ? entry.Work
                : spell;
        }

        public static string Validate()
        {
            CatalogBook.EnsureLoaded();
            var broken = new List<string>();
            foreach (var entry in Entries)
            {
                if (entry.RecipeRunes.Count == 0)
                {
                    broken.Add($"{entry.Number} {entry.Name}: recipe did not parse");
                    continue;
                }

                if (!ChainBook.Matches(entry, entry.RecipeRunes))
                {
                    broken.Add($"{entry.Number} {entry.Name}: recipe does not match itself");
                }

                if (entry.ViaRunes.Count > 0 && !ChainBook.Matches(entry, entry.ViaRunes))
                {
                    broken.Add($"{entry.Number} {entry.Name}: via does not match recipe");
                }

                if (entry.Shape == SpellShape.None)
                {
                    broken.Add($"{entry.Number} {entry.Name}: form '{entry.Form}' unknown");
                }
            }

            ValidateFills(broken);

            return broken.Count == 0
                ? string.Empty
                : string.Join("; ", broken);
        }

        static void ValidateFills(List<string> broken)
        {
            if (!TryGet(SpellId.Fireball, out var fireballEntry) ||
                !ChainBook.SameStory(fireballEntry.RecipeRunes, ChainBook.Parse("Fire · Air · Salt · Mercury")))
            {
                return;
            }

            var salt = Composition.FromSequence(new[] { RuneId.Fire, RuneId.Salt });
            if (ChainBook.CollectExact(salt, SpellShape.None).Count != 0)
            {
                broken.Add("Fire · Salt should not be an exact catalog sentence");
            }

            var filled = ChainBook.CollectFillable(salt, SpellShape.None, FreeAttunement.DefaultFillBudget);
            if (filled.Count < 2)
            {
                broken.Add("Fire · Salt should clash between at least two fillable chains");
            }

            var fireball = Composition.FromSequence(new[] { RuneId.Fire, RuneId.Air, RuneId.Mercury });
            var exact = ChainBook.CollectExact(fireball, SpellShape.None);
            if (exact.Count == 0)
            {
                broken.Add("Fireball exact match failed");
            }

            var free = ChainBook.CollectForFree(fireball, SpellShape.None, 2);
            if (free.Count != exact.Count)
            {
                broken.Add("A finished sentence must not fill toward a longer chain");
            }

            if (ChainBook.CollectFillable(salt, SpellShape.None, 0).Count != 0)
            {
                broken.Add("A zero fill budget must not complete a missing rune");
            }
        }

        public static bool TryGet(int number, out CodexEntry entry)
        {
            CatalogBook.EnsureLoaded();
            foreach (var candidate in Entries)
            {
                if (candidate.Number == number)
                {
                    entry = candidate;
                    return true;
                }
            }

            entry = default;
            return false;
        }

        public static bool TryGet(SpellId spell, out CodexEntry entry)
        {
            CatalogBook.EnsureLoaded();
            foreach (var candidate in Entries)
            {
                if (candidate.Spell == spell)
                {
                    entry = candidate;
                    return true;
                }
            }

            entry = default;
            return false;
        }

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
            SpellId spell,
            string want,
            string name,
            string recipe,
            string via,
            string form,
            SpellOutcome outcome,
            string gate = "")
        {
            return new CodexEntry(number, book, spell, want, name, recipe, via, form, outcome, gate);
        }
    }
}
