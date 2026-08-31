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
            E(1, SpellBook.End, SpellId.Fireball, "Hunger sent. Fire that flies.", "Fireball", "Fire · Mercury", "", "Shot", SpellOutcome.Kill),
            E(2, SpellBook.End, SpellId.FlamePillar, "Hunger given a standing body and asked to rest. It stands.", "Flame-pillar", "Fire · Salt · Earth", "", "Pillar", SpellOutcome.Kill),
            E(3, SpellBook.Cross, SpellId.Melt, "A stood fire-body sent into a thing. Salt keeps it from flying. Stone and steel remember they were liquid. Obsidian will not.", "Melt", "Fire · Salt · Mercury", "", "Remote", SpellOutcome.Neither),
            E(4, SpellBook.End, SpellId.Smother, "Hunger needs breath; that breath is withheld.", "Smother", "Fire · Air · Dark", "Spark · Dark", "Remote", SpellOutcome.Neither),
            E(5, SpellBook.End, SpellId.SunLance, "Hunger shown, given breath, sent as a clean line.", "Sun-lance", "Fire · Light · Air · Mercury", "Spark · Light · Mercury", "Shot", SpellOutcome.Kill),
            E(6, SpellBook.End, SpellId.Ignite, "Hunger’s wildcard given a standing body — a wick that stays.", "Ignite", "Fire · Sulphur · Salt", "", "Remote", SpellOutcome.Neither),
            E(7, SpellBook.End, SpellId.LightningBolt, "Hunger given breath and sent. A bolt, not a body.", "Lightning", "Fire · Air · Mercury", "Lightning · Mercury", "Shot", SpellOutcome.Kill),
            E(8, SpellBook.End, SpellId.ChainLightning, "That bolt finds yield given a body. The pool is what dies.", "Chain", "Fire · Air · Mercury · Water · Salt", "Lightning · Mercury · Water · Salt", "Remote", SpellOutcome.Kill),
            E(9, SpellBook.Hold, SpellId.LiveFloor, "The seed given a body around your feet. They cannot step.", "Live-floor", "Fire · Air · Salt", "Spark · Salt", "Spread", SpellOutcome.Kill),
            E(10, SpellBook.Hold, SpellId.Jolt, "The bolt, turned by Sulphur, reaches a mind.", "Jolt", "Fire · Air · Sulphur · Mercury", "Spark · Sulphur · Mercury", "Remote", SpellOutcome.Restrain),
            E(11, SpellBook.Hold, SpellId.Thunderclap, "The bolt meets rest, then every mind around you.", "Thunderclap", "Fire · Air · Earth · Sulphur", "Lightning · Earth · Sulphur", "Spread", SpellOutcome.Restrain),
            E(12, SpellBook.Weather, SpellId.StormCall, "The hanging veil given a body, then the bolt is sent from it.", "Storm", "Air · Water · Salt · Fire · Air · Mercury", "Cloud · Salt · Lightning · Mercury", "Remote", SpellOutcome.Kill),
            E(13, SpellBook.Weather, SpellId.Rain, "The hanging veil yields more and is sent down.", "Rain", "Air · Water · Water · Mercury", "Cloud · Water · Mercury", "Remote", SpellOutcome.Neither),
            E(14, SpellBook.SeeHide, SpellId.Fog, "The hanging veil is drawn to the ground.", "Fog", "Air · Water · Earth", "Cloud · Earth", "Spread", SpellOutcome.Neither),
            E(15, SpellBook.End, SpellId.Scald, "Hunger forced through yield and sent.", "Scald", "Fire · Water · Mercury", "Steam · Mercury", "Shot", SpellOutcome.Kill),
            E(16, SpellBook.Weather, SpellId.WaterJet, "Yield learns breath so it can leave the vessel, then is sent.", "Water-jet", "Water · Air · Mercury", "", "Shot", SpellOutcome.Restrain),
            E(17, SpellBook.Hold, SpellId.Flood, "Yield going, more yield, given a body. They bog.", "Flood", "Water · Mercury · Water · Salt", "Current · Water · Salt", "Spread", SpellOutcome.Restrain),
            E(18, SpellBook.Cross, SpellId.IcePillar, "Hard water asked to rest as a column. Over a pit it must join two floors, or it falls. On water it freezes without banks. It will thaw.", "Ice-pillar", "Water · Earth · Salt · Earth", "Ice · Salt · Earth", "Pillar", SpellOutcome.Restrain),
            E(19, SpellBook.Hold, SpellId.IceSpear, "Hard water sent.", "Ice-spear", "Water · Earth · Mercury", "Ice · Mercury", "Shot", SpellOutcome.Restrain),
            E(20, SpellBook.Hold, SpellId.Snowfall, "The veil is given ice’s story and sent softly.", "Snowfall", "Air · Water · Water · Earth · Mercury", "Cloud · Ice · Mercury", "Remote", SpellOutcome.Restrain),
            E(21, SpellBook.Cross, SpellId.Thaw, "The hard water-body meets hunger and remembers yield.", "Thaw", "Water · Earth · Fire", "Ice · Fire", "Remote", SpellOutcome.Neither),
            E(22, SpellBook.End, SpellId.HurledStone, "Rest given a body and sent. Earth flies.", "Hurled stone", "Earth · Salt · Mercury", "Stone · Mercury", "Shot", SpellOutcome.Kill),
            E(23, SpellBook.Cross, SpellId.Wall, "A body of rest asked to rest as more rest. A wall. Across a pit it is a two-tile span that must find floor or wall at each end, or it falls. Water takes mud, not a bridge.", "Wall", "Earth · Salt · Earth", "Stone · Earth", "Pillar", SpellOutcome.Neither),
            E(24, SpellBook.Cross, SpellId.Pit, "Rest asked to go, given breath so it leaves a hollow.", "Pit", "Earth · Mercury · Air", "", "Remote", SpellOutcome.Neither),
            E(25, SpellBook.Cross, SpellId.Bridge, "A body of rest given breath and sent across. A two-tile span that must find floor or wall at each end, or it falls. Water takes mud, not a bridge.", "Bridge", "Earth · Salt · Air · Mercury", "Stone · Air · Mercury", "Remote", SpellOutcome.Neither),
            E(26, SpellBook.Hold, SpellId.Quagmire, "Rest meeting yield, given a body. It holds them.", "Quagmire", "Earth · Water · Salt", "Mud · Salt", "Spread", SpellOutcome.Restrain),
            E(27, SpellBook.End, SpellId.LavaFlood, "Hungry earth asked to go.", "Lava-flood", "Fire · Earth · Mercury", "Lava · Mercury", "Remote", SpellOutcome.Kill),
            E(28, SpellBook.Cross, SpellId.ObsidianPath, "Hungry earth quenched and given a body. A path.", "Obsidian path", "Fire · Earth · Salt · Water", "Lava · Salt · Water", "Remote", SpellOutcome.Neither),
            E(29, SpellBook.GrowHeal, SpellId.Sprout, "A vegetable body marked living, from the feet.", "Sprout", "Water · Salt · Earth · Life", "Plant · Life", "Spread", SpellOutcome.Neither),
            E(30, SpellBook.Hold, SpellId.Vine, "The vegetable body sent. It holds them, or it climbs.", "Vine", "Water · Salt · Earth · Mercury", "Plant · Mercury", "Remote", SpellOutcome.Restrain),
            E(31, SpellBook.GrowHeal, SpellId.VineRise, "The sent plant asked to stand.", "Vine-rise", "Water · Salt · Earth · Mercury · Earth", "Plant · Mercury · Earth", "Pillar", SpellOutcome.Neither),
            E(32, SpellBook.GrowHeal, SpellId.Mend, "A living body, yield and rest, sent into the living.", "Mend", "Life · Salt · Water · Earth · Mercury", "", "Spread", SpellOutcome.Neither),
            E(33, SpellBook.Cross, SpellId.Hop, "Breath given a body, then more breath, kept on you. A leap.", "Hop", "Air · Salt · Air", "", "Self", SpellOutcome.Neither),
            E(34, SpellBook.Cross, SpellId.Flight, "Breath going, given a body, kept on you. You fly.", "Flight", "Air · Mercury · Salt", "Air · Mercury · Salt · Life · Mercury", "Self", SpellOutcome.Neither),
            E(35, SpellBook.Mind, SpellId.Rage, "Fire sent, turned by Sulphur, into a mind.", "Rage", "Fire · Sulphur · Mercury", "", "Remote", SpellOutcome.Neither),
            E(36, SpellBook.Mind, SpellId.Terror, "The withheld reaches a mind. They flee or freeze.", "Terror", "Dark · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(37, SpellBook.Mind, SpellId.Lull, "Yield reaches a mind. They sleep. They can be woken.", "Lull", "Water · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(38, SpellBook.Weather, SpellId.Gale, "Breath going, more breath, so it can push.", "Gale", "Air · Mercury · Air", "", "Shot", SpellOutcome.Restrain),
            E(39, SpellBook.SeeHide, SpellId.Veil, "The withheld, a living body, as breath. Hard to see.", "Veil", "Dark · Life · Salt · Air", "", "Spread", SpellOutcome.Neither),
            E(40, SpellBook.Call, SpellId.CallBeast, "Flesh, marked living, given a mind, sent here. Know the formula.", "Call beast", "Earth · Water · Salt · Life · Sulphur · Mercury", "", "Remote", SpellOutcome.Neither),
            E(41, SpellBook.Grave, SpellId.Blight, "A vegetable body, then the grave, given a body. Verdure rots.", "Blight", "Water · Salt · Earth · Death · Salt", "Poison · Salt", "Spread", SpellOutcome.Kill, "Either"),
            E(42, SpellBook.Grave, SpellId.Shade, "Withheld, given a body, marked by the grave, and sent.", "Shade", "Dark · Death · Salt · Mercury", "Shade · Mercury", "Remote", SpellOutcome.Neither, "Free"),
            E(43, SpellBook.Grave, SpellId.Unmake, "The grave is sent into a living body.", "Unmake", "Death · Mercury · Life · Salt", "", "Remote", SpellOutcome.Kill, "Free"),
            E(44, SpellBook.Grave, SpellId.GraveSleep, "The waking passion is given to the grave. Sleep as if dead.", "Grave-sleep", "Life · Sulphur · Death", "", "Remote", SpellOutcome.Restrain, "Free"),
            E(45, SpellBook.Grave, SpellId.CorpseCall, "The four as a body, marked by the grave, and sent.", "Corpse-call", "Salt · Water · Earth · Fire · Death · Mercury", "", "Remote", SpellOutcome.Neither, "Free"),
            E(46, SpellBook.Grave, SpellId.GraveDust, "Rest marked by the grave. Earth-life and golems come apart.", "Grave-dust", "Earth · Death · Salt", "", "Spread", SpellOutcome.Kill, "Either"),
            E(47, SpellBook.Grave, SpellId.Snuff, "Hunger marked by the grave and sent into a flame.", "Snuff", "Fire · Death · Mercury", "Ember · Mercury", "Remote", SpellOutcome.Neither, "Either"),
            E(48, SpellBook.Grave, SpellId.Blackout, "The seed marked by the grave and sent. A live rod dies.", "Blackout", "Fire · Air · Death · Mercury", "Spark · Death · Mercury", "Shot", SpellOutcome.Neither, "Either"),
            E(49, SpellBook.Grave, SpellId.GraveIce, "Yield given a body, then the grave. Ice that will not thaw.", "Grave-ice", "Water · Salt · Death", "", "Remote", SpellOutcome.Restrain, "Either"),
            E(50, SpellBook.Grave, SpellId.LastBreath, "Living breath, then the grave, sent. The breath leaves them.", "Last breath", "Air · Life · Death · Mercury", "", "Remote", SpellOutcome.Kill, "Free"),
            E(51, SpellBook.Hold, SpellId.TimeStop, "Yield and rest are withheld. The living stay; the motion of instants leaves; the mind cannot hurry.", "Time-stop", "Water · Earth · Dark · Life · Death · Sulphur · Salt", "Ice · Dark · Life · Death · Sulphur · Salt", "Spread", SpellOutcome.Restrain, "Free"),
            E(52, SpellBook.Weather, SpellId.Douse, "Yield sent. Water thrown. Hunger ends.", "Douse", "Water · Mercury", "", "Shot", SpellOutcome.Neither),
            E(53, SpellBook.Mind, SpellId.Command, "A standing body given a mind and sent. They obey.", "Command", "Salt · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(54, SpellBook.Weather, SpellId.Gust, "Breath sent. Wind.", "Wind", "Air · Mercury", "", "Shot", SpellOutcome.Neither),
            E(55, SpellBook.Cross, SpellId.EarthPillar, "Rest given a body. A column of earth. Over a pit it must join two floors, or it falls. Water takes mud, not a span.", "Earth-pillar", "Earth · Salt", "Stone", "Pillar", SpellOutcome.Neither, "", SpellId.StonePillar),
            E(56, SpellBook.Mind, SpellId.Stoneskin, "Rest given a body, then the mind holds it on you. Arrows break. Hunger sent still finds you.", "Stoneskin", "Earth · Salt · Sulphur", "", "Self", SpellOutcome.Neither),
            E(57, SpellBook.Mind, SpellId.Watershield, "Yield given a body, then the mind holds it on you. Hunger breaks.", "Water ward", "Water · Salt · Sulphur", "", "Self", SpellOutcome.Neither),
            E(58, SpellBook.Mind, SpellId.Flameward, "Hunger given a body, then the mind holds it on you. Rest thrown breaks.", "Flame ward", "Fire · Salt · Sulphur", "", "Self", SpellOutcome.Neither),
            E(59, SpellBook.Mind, SpellId.Windward, "Breath given a body, then the mind holds it on you. Yield thrown breaks. Foul breath also breaks.", "Wind ward", "Air · Salt · Sulphur", "", "Self", SpellOutcome.Neither),
            E(60, SpellBook.End, SpellId.LavaPillar, "Hungry earth given a body and asked to rest. It stands. Yield cools it to rock.", "Lava-pillar", "Fire · Earth · Salt · Earth", "Lava · Salt · Earth", "Pillar", SpellOutcome.Kill),
            E(61, SpellBook.Cross, SpellId.Shatter, "A stood wall given breath and sent. Matter comes apart.", "Shatter", "Earth · Salt · Earth · Air · Mercury", "Stone · Earth · Air · Mercury", "Remote", SpellOutcome.Neither),
            E(62, SpellBook.Mind, SpellId.Confuse, "Breath turned by Sulphur, into a mind. They lose the thread.", "Confuse", "Air · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(63, SpellBook.Cross, SpellId.IceWall, "A body of ice asked to stand as more ice. A wall. Across a pit it is a two-tile span that must find floor or wall at each end, or it falls. On water it freezes without banks. It will thaw.", "Ice-wall", "Water · Earth · Salt · Water · Earth", "Ice · Salt · Ice", "Pillar", SpellOutcome.Restrain),
            E(64, SpellBook.Hold, SpellId.Freeze, "Hard water held as a condition. They freeze.", "Freeze", "Water · Earth · Sulphur", "Ice · Sulphur", "Remote", SpellOutcome.Restrain),
            E(65, SpellBook.Weather, SpellId.Snowstorm, "The veil given ice’s story, then driven. They freeze.", "Snowstorm", "Air · Water · Water · Earth · Air · Mercury", "Cloud · Ice · Air · Mercury", "Remote", SpellOutcome.Restrain),
            E(66, SpellBook.Weather, SpellId.Push, "Breath given a body and sent. Wind that pushes the person.", "Push", "Air · Salt · Mercury", "", "Shot", SpellOutcome.Restrain),
            E(67, SpellBook.End, SpellId.LightningStrike, "A spark given form from the air, moving at something. It falls from the sky.", "Lightning strike", "Fire · Air · Salt · Air · Mercury", "Spark · Salt · Air · Mercury", "Remote", SpellOutcome.Kill),
            E(68, SpellBook.Mind, SpellId.Charm, "A living mind is reached and sent. They fetch, and they fight what you have marked.", "Charm", "Life · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(69, SpellBook.Hold, SpellId.Swamp, "Rest meeting yield, going, given a body around your feet. A watery swamp.", "Swamp", "Earth · Water · Mercury · Salt", "Mud · Mercury · Salt", "Spread", SpellOutcome.Restrain),
            E(70, SpellBook.End, SpellId.Witchfire, "Fire given logos and its own perpetuity, then sent. Witchfire. It eats what ordinary hunger cannot.", "Witchfire", "Fire · Animus · Fire · Mercury", "Flame · Mercury", "Remote", SpellOutcome.Kill),
            E(71, SpellBook.Cross, SpellId.Grotto, "The vegetable body is withheld. Rest opens a damp cave.", "Grotto", "Water · Salt · Earth · Dark", "Plant · Dark", "Remote", SpellOutcome.Neither),
            E(72, SpellBook.Weather, SpellId.Thunder, "The arc meeting rest.", "Thunder", "Fire · Air · Earth", "Lightning · Earth", "Remote", SpellOutcome.Neither),
            E(73, SpellBook.SeeHide, SpellId.Darkness, "The hanging veil is withheld. Nothing in the vicinity can see.", "Darkness", "Air · Water · Dark", "Cloud · Dark", "Remote", SpellOutcome.Neither),
            E(74, SpellBook.Weather, SpellId.Blizzard, "The veil given ice, a body, going, and wind.", "Blizzard", "Air · Water · Salt · Water · Earth · Mercury · Air · Mercury", "Cloud · Salt · Ice · Mercury · Air · Mercury", "Remote", SpellOutcome.Restrain),
            E(75, SpellBook.Weather, SpellId.Sandstorm, "Breath going, driving grit.", "Sandstorm", "Air · Mercury · Air · Earth · Mercury", "Air · Mercury · Dust · Mercury", "Remote", SpellOutcome.Neither),
            E(76, SpellBook.Cross, SpellId.WaterPillar, "Yield and rest given a standing body. A column of water.", "Water-pillar", "Water · Earth · Salt", "Ice · Salt", "Pillar", SpellOutcome.Neither),
            E(77, SpellBook.End, SpellId.OilShot, "Fuel sent. Surfaces hold flame. Fire already standing grows.", "Oil shot", "Water · Salt · Earth · Fire · Earth · Mercury", "Oil · Mercury", "Shot", SpellOutcome.Neither),
            E(78, SpellBook.End, SpellId.OilPillar, "A stood wick. A later fire sentence would make it a bomb.", "Oil-pillar", "Water · Salt · Earth · Fire · Earth · Salt · Earth", "Oil · Salt · Earth", "Pillar", SpellOutcome.Neither),
            E(79, SpellBook.Grave, SpellId.Poison, "The grave of a plant, sent.", "Poison", "Water · Salt · Earth · Death · Mercury", "Poison · Mercury", "Shot", SpellOutcome.Kill, "Either"),
            E(80, SpellBook.SeeHide, SpellId.Miasma, "The hanging veil forced through acid. Foul breath.", "Miasma", "Cloud · Acid", "", "Spread", SpellOutcome.Kill),
            E(81, SpellBook.End, SpellId.Plasma, "Witchfire joined to the bolt and sent. Ordinary matter ends. Obsidian and warded stone refuse it.", "Plasma", "Fire · Animus · Fire · Fire · Air · Air · Mercury", "Plasma · Mercury", "Shot", SpellOutcome.Kill),
            E(82, SpellBook.GrowHeal, SpellId.Forest, "The vegetable body waking as a mass.", "Forest", "Water · Salt · Earth · Life · Earth", "Plant · Life · Earth", "Remote", SpellOutcome.Neither),
            E(83, SpellBook.Weather, SpellId.Monsoon, "Yield given a body and sent. A remote flood. The monsoon.", "Monsoon", "Water · Salt · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(84, SpellBook.Cross, SpellId.DirtToss, "Rest sent without a body. Loose dirt. It smothers ground-fire and leaves Earth speaking where it lands.", "Dirt toss", "Earth · Mercury", "", "Shot", SpellOutcome.Neither),
            E(85, SpellBook.Cross, SpellId.MetalPillar, "Hungry earth given spark and asked to stand. A column of iron. It hangs without a far bank.", "Metal-pillar", "Fire · Earth · Fire · Air · Earth · Salt · Earth", "Metal · Salt · Earth", "Pillar", SpellOutcome.Neither),
            E(86, SpellBook.Cross, SpellId.MetalWall, "A body of iron asked to stand as more iron. A wall. Over a gap it needs no far rest.", "Metal-wall", "Fire · Earth · Fire · Air · Earth · Salt · Fire · Earth · Fire · Air · Earth", "Metal · Salt · Metal", "Pillar", SpellOutcome.Neither),
            E(87, SpellBook.Cross, SpellId.ObsidianWall, "A body of black glass asked to stand as more black glass. A wall. Melt, Shatter, and hunger's thaw will not take it. Over a gap it needs no far rest.", "Obsidian-wall", "Fire · Earth · Salt · Water · Salt · Fire · Earth · Salt · Water", "Obsidian · Salt · Obsidian", "Pillar", SpellOutcome.Neither),
            E(88, SpellBook.GrowHeal, SpellId.Balm, "Care sent. Yield given mind and rest, then sent. It heals.", "Balm", "Water · Sulphur · Earth · Mercury", "Anima · Mercury", "Spread", SpellOutcome.Neither),
            E(89, SpellBook.GrowHeal, SpellId.Chorus, "Care given a body around the feet. The work opens to many.", "Chorus", "Water · Sulphur · Earth · Salt", "Anima · Salt", "Spread", SpellOutcome.Neither),
            E(90, SpellBook.End, SpellId.Drive, "Hunger given mind and breath, then sent. Logos sent. It goes out and does not return.", "Drive", "Fire · Sulphur · Air · Mercury", "Animus · Mercury", "Shot", SpellOutcome.Kill)
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
                !ChainBook.SameStory(fireballEntry.RecipeRunes, ChainBook.Parse("Fire · Mercury")))
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

            var earthSalt = Composition.FromSequence(new[] { RuneId.Earth, RuneId.Salt });
            var earthPillar = ChainBook.CollectExact(earthSalt, SpellShape.None);
            if (earthPillar.Count == 0 || earthPillar[0].Spell != SpellId.EarthPillar)
            {
                broken.Add("Earth · Salt should be Earth-pillar");
            }

            var ice = Composition.FromSequence(new[] { RuneId.Ice });
            if (ChainBook.CollectExact(ice, SpellShape.None).Count != 0)
            {
                broken.Add("Ice alone is a join, not Ice-pillar");
            }

            var waterPillar = Composition.FromSequence(new[] { RuneId.Water, RuneId.Earth, RuneId.Salt });
            var waterPillarExact = ChainBook.CollectExact(waterPillar, SpellShape.None);
            if (waterPillarExact.Count == 0 || waterPillarExact[0].Spell != SpellId.WaterPillar)
            {
                broken.Add("Water · Earth · Salt should be Water-pillar");
            }

            var plantJoin = Composition.FromSequence(new[] { RuneId.Water, RuneId.Salt, RuneId.Earth });
            if (ChainBook.CollectExact(plantJoin, SpellShape.None).Count != 0)
            {
                broken.Add("Water · Salt · Earth joins to Plant, not a spell");
            }

            var iceWall = Composition.FromSequence(new[] { RuneId.Ice, RuneId.Salt, RuneId.Ice });
            var iceWallExact = ChainBook.CollectExact(iceWall, SpellShape.None);
            if (iceWallExact.Count == 0 || iceWallExact[0].Spell != SpellId.IceWall)
            {
                broken.Add("Ice · Salt · Ice should be Ice-wall");
            }

            var iceWallRoots = Composition.FromSequence(new[]
            {
                RuneId.Water, RuneId.Earth, RuneId.Salt, RuneId.Water, RuneId.Earth
            });
            var iceWallFromRoots = ChainBook.CollectExact(iceWallRoots, SpellShape.None);
            if (iceWallFromRoots.Count == 0 || iceWallFromRoots[0].Spell != SpellId.IceWall)
            {
                broken.Add("Water · Earth · Salt · Water · Earth should be Ice-wall");
            }

            var metalWall = Composition.FromSequence(new[] { RuneId.Metal, RuneId.Salt, RuneId.Metal });
            var metalWallExact = ChainBook.CollectExact(metalWall, SpellShape.None);
            if (metalWallExact.Count == 0 || metalWallExact[0].Spell != SpellId.MetalWall)
            {
                broken.Add("Metal · Salt · Metal should be Metal-wall");
            }

            var metalPillar = Composition.FromSequence(new[] { RuneId.Metal, RuneId.Salt, RuneId.Earth });
            var metalPillarExact = ChainBook.CollectExact(metalPillar, SpellShape.None);
            if (metalPillarExact.Count == 0 || metalPillarExact[0].Spell != SpellId.MetalPillar)
            {
                broken.Add("Metal · Salt · Earth should be Metal-pillar");
            }

            var fireball = Composition.FromSequence(new[] { RuneId.Fire, RuneId.Mercury });
            var exact = ChainBook.CollectExact(fireball, SpellShape.None);
            if (exact.Count == 0 || exact[0].Spell != SpellId.Fireball)
            {
                broken.Add("Fire · Mercury should be Fireball");
            }

            var scrambled = Composition.FromSequence(new[] { RuneId.Mercury, RuneId.Fire });
            if (ChainBook.CollectExact(scrambled, SpellShape.None).Count != 0)
            {
                broken.Add("Mercury · Fire must fizzle under Charter — order is the sentence");
            }

            var unscrambled = ChainBook.CollectUnscrambled(scrambled, SpellShape.None);
            if (unscrambled.Count == 0 || unscrambled[0].Spell != SpellId.Fireball)
            {
                broken.Add("Free should unscramble Mercury · Fire into Fireball");
            }

            var lightning = Composition.FromSequence(new[] { RuneId.Fire, RuneId.Air, RuneId.Mercury });
            var bolt = ChainBook.CollectExact(lightning, SpellShape.None);
            if (bolt.Count == 0 || bolt[0].Spell != SpellId.LightningBolt)
            {
                broken.Add("Fire · Air · Mercury should be Lightning");
            }

            var hop = Composition.FromSequence(new[] { RuneId.Air, RuneId.Salt, RuneId.Air });
            var hopExact = ChainBook.CollectExact(hop, SpellShape.None);
            if (hopExact.Count == 0 || hopExact[0].Spell != SpellId.Hop)
            {
                broken.Add("Air · Salt · Air should be Hop");
            }

            var push = Composition.FromSequence(new[] { RuneId.Air, RuneId.Salt, RuneId.Mercury });
            var pushExact = ChainBook.CollectExact(push, SpellShape.None);
            if (pushExact.Count == 0 || pushExact[0].Spell != SpellId.Push)
            {
                broken.Add("Air · Salt · Mercury should be Push");
            }

            var strike = Composition.FromSequence(new[]
            {
                RuneId.Fire, RuneId.Air, RuneId.Salt, RuneId.Air, RuneId.Mercury
            });
            var strikeExact = ChainBook.CollectExact(strike, SpellShape.None);
            if (strikeExact.Count == 0 || strikeExact[0].Spell != SpellId.LightningStrike)
            {
                broken.Add("Fire · Air · Salt · Air · Mercury should be Lightning strike");
            }

            var sparkStrike = Composition.FromSequence(new[]
            {
                RuneId.Spark, RuneId.Salt, RuneId.Air, RuneId.Mercury
            });
            var sparkStrikeExact = ChainBook.CollectExact(sparkStrike, SpellShape.None);
            if (sparkStrikeExact.Count == 0 || sparkStrikeExact[0].Spell != SpellId.LightningStrike)
            {
                broken.Add("Spark · Salt · Air · Mercury should be Lightning strike");
            }

            var free = ChainBook.CollectForFree(fireball, SpellShape.None, 2);
            if (free.Count != exact.Count)
            {
                broken.Add("A finished sentence must not fill toward a longer chain");
            }

            var freeScramble = ChainBook.CollectForFree(scrambled, SpellShape.None, 1);
            if (freeScramble.Count == 0 || freeScramble[0].Spell != SpellId.Fireball)
            {
                broken.Add("Free must prefer an unscrambled finished sentence over filling a longer one");
            }

            if (ChainBook.CollectFillable(salt, SpellShape.None, 0).Count != 0)
            {
                broken.Add("A zero fill budget must not complete a missing rune");
            }

            if (!FocusLaw.Breaks(StatusId.Watershield, SpellId.Douse))
            {
                broken.Add("Douse must drop a water ward — both use Water");
            }

            if (FocusLaw.Breaks(StatusId.Watershield, SpellId.Fireball))
            {
                broken.Add("Fireball must not drop a water ward");
            }

            if (!FocusLaw.Breaks(StatusId.Flameward, SpellId.Fireball))
            {
                broken.Add("Fireball must drop a flame ward — both use Fire");
            }

            if (!FocusLaw.Breaks(StatusId.Sleeping, SpellId.Rage))
            {
                broken.Add("Rage must drop sleep — both reuse Sulphur and Mercury");
            }

            if (!FocusLaw.Breaks(StatusId.Sleeping, SpellId.Fireball))
            {
                broken.Add("Fireball must drop sleep — Lull and Fireball both send Mercury");
            }

            if (FocusLaw.Breaks(StatusId.Sleeping, SpellId.Hop))
            {
                broken.Add("Hop must not drop sleep — no shared marks");
            }

            if (!FocusLaw.Breaks(StatusId.Charmed, SpellId.Fireball))
            {
                broken.Add("Fireball must drop charm — both send Mercury");
            }

            if (!FocusLaw.Breaks(StatusId.Charmed, SpellId.Command))
            {
                broken.Add("Command must drop charm — another mind sentence reuses Sulphur and Mercury");
            }

            if (FocusLaw.Breaks(StatusId.Charmed, SpellId.Wall))
            {
                broken.Add("A wall must not drop charm — earth stands without those marks");
            }

            if (FocusLaw.Breaks(StatusId.Stoneskin, SpellId.Fireball))
            {
                broken.Add("Fireball must not drop stoneskin");
            }

            if (!FocusLaw.Breaks(StatusId.Stoneskin, SpellId.Wall))
            {
                broken.Add("Wall must drop stoneskin — both use Earth and Salt");
            }

            if (FocusLaw.Breaks(StatusId.Burning, SpellId.Douse))
            {
                broken.Add("Burning is elemental and does not need concentration");
            }

            if (SpellVerb.Of(SpellId.IceSpear).Status == StatusId.Frozen
                || SpellVerb.Of(SpellId.IcePillar).Status == StatusId.Frozen
                || SpellVerb.Of(SpellId.IceWall).Status == StatusId.Frozen)
            {
                broken.Add("Ice-spear, ice-pillar, and ice-wall must not freeze a living body");
            }

            if (SpellVerb.Of(SpellId.Snowfall).Status != StatusId.Frozen
                || SpellVerb.Of(SpellId.Freeze).Status != StatusId.Frozen
                || SpellVerb.Of(SpellId.Snowstorm).Status != StatusId.Frozen)
            {
                broken.Add("Snowfall, Freeze, and Snowstorm must freeze");
            }

            if (SpellVerb.Of(SpellId.Blight).Status != StatusId.Poisoned)
            {
                broken.Add("Blight must poison");
            }

            if (!WorldWork.StopsOnWalls(SpellId.Fireball)
                || !WorldWork.StopsOnWalls(SpellId.IceSpear)
                || !WorldWork.StopsOnWalls(SpellId.HurledStone)
                || !WorldWork.StopsOnWalls(SpellId.DirtToss)
                || !WorldWork.StopsOnWalls(SpellId.WaterJet)
                || !WorldWork.StopsOnWalls(SpellId.Douse)
                || !WorldWork.StopsOnWalls(SpellId.Gust)
                || !WorldWork.StopsOnWalls(SpellId.Push)
                || !WorldWork.StopsOnWalls(SpellId.LightningBolt))
            {
                broken.Add("A flying shot must stop on a wall");
            }

            var dirtToss = Composition.FromSequence(new[] { RuneId.Earth, RuneId.Mercury });
            var dirtExact = ChainBook.CollectExact(dirtToss, SpellShape.None);
            if (dirtExact.Count == 0 || dirtExact[0].Spell != SpellId.DirtToss)
            {
                broken.Add("Earth · Mercury should be Dirt toss");
            }

            if (SpellVerb.Of(SpellId.DirtToss).Status != StatusId.None
                || (TryGet(SpellId.DirtToss, out var dirtEntry) && dirtEntry.Outcome != SpellOutcome.Neither))
            {
                broken.Add("Dirt toss must not harm a living body");
            }

            var hurled = Composition.FromSequence(new[] { RuneId.Earth, RuneId.Salt, RuneId.Mercury });
            var hurledExact = ChainBook.CollectExact(hurled, SpellShape.None);
            if (hurledExact.Count == 0 || hurledExact[0].Spell != SpellId.HurledStone)
            {
                broken.Add("Earth · Salt · Mercury should be Hurled stone");
            }

            var monsoon = Composition.FromSequence(new[] { RuneId.Water, RuneId.Salt, RuneId.Mercury });
            var monsoonExact = ChainBook.CollectExact(monsoon, SpellShape.None);
            if (monsoonExact.Count == 0 || monsoonExact[0].Spell != SpellId.Monsoon)
            {
                broken.Add("Water · Salt · Mercury should be Monsoon");
            }

            if (WorldWork.StopsOnWalls(SpellId.LightningStrike))
            {
                broken.Add("Lightning strike falls from the sky and must not be stopped by a wall");
            }

            if (WorldWork.StopsOnWalls(SpellId.Melt)
                || WorldWork.StopsOnWalls(SpellId.Rain)
                || WorldWork.StopsOnWalls(SpellId.Hop)
                || WorldWork.StopsOnWalls(SpellId.Wall)
                || WorldWork.StopsOnWalls(SpellId.Grotto))
            {
                broken.Add("Remote, hop, and stood work must not be treated as flying shots");
            }

            if (!ChainBook.TryBirth(RuneId.Plant, out var plantBirth)
                || plantBirth.Count != 3
                || plantBirth[0] != RuneId.Water
                || plantBirth[1] != RuneId.Salt
                || plantBirth[2] != RuneId.Earth)
            {
                broken.Add("Plant must be the Water · Salt · Earth rune");
            }

            if (!ChainBook.TryBirth(RuneId.Ice, out var iceBirth)
                || iceBirth.Count != 2
                || iceBirth[0] != RuneId.Water
                || iceBirth[1] != RuneId.Earth)
            {
                broken.Add("Ice must be Water · Earth");
            }

            if (!ChainBook.TryBirth(RuneId.Metal, out var metalBirth)
                || metalBirth.Count != 3
                || metalBirth[0] != RuneId.Lava
                || metalBirth[1] != RuneId.Spark
                || metalBirth[2] != RuneId.Earth)
            {
                broken.Add("Metal must be Lava · Spark · Earth — Fire · Earth · Fire · Air · Earth");
            }

            if (RuneCatalog.TryParseName("Grotto", out _)
                || RuneCatalog.TryParseName("Storm", out _)
                || RuneCatalog.TryParseName("Thunder", out _)
                || RuneCatalog.TryParseName("Rain", out _)
                || RuneCatalog.TryParseName("Vine", out _)
                || RuneCatalog.TryParseName("Wind", out _)
                || RuneCatalog.TryParseName("Inferno", out _)
                || RuneCatalog.TryParseName("Grove", out var groveId) && groveId != RuneId.Plant)
            {
                broken.Add("Weather names, Vine, Wind, Inferno, and Grove must not be runes — those are spells or unused");
            }

            if (!RuneCatalog.TryParseName("Sand", out var sandId) || sandId != RuneId.Dust)
            {
                broken.Add("Sand must be the same grit as Dust");
            }

            if (ChainBook.TryBirth(RuneId.Vine, out _)
                || ChainBook.TryBirth(RuneId.Wind, out _)
                || ChainBook.TryBirth(RuneId.Inferno, out _)
                || ChainBook.TryBirth(RuneId.Sand, out _))
            {
                broken.Add("Vine, Wind, Inferno, and Sand must not be wrought joins");
            }

            if (!ChainBook.TryBirth(RuneId.Mud, out var mudBirth)
                || mudBirth.Count != 2
                || mudBirth[0] != RuneId.Earth
                || mudBirth[1] != RuneId.Water)
            {
                broken.Add("Mud must be Earth · Water — Water · Earth is Ice");
            }

            if (!ChainBook.TryBirth(RuneId.Obsidian, out var obsidianBirth)
                || obsidianBirth.Count != 3
                || obsidianBirth[0] != RuneId.Lava
                || obsidianBirth[1] != RuneId.Salt
                || obsidianBirth[2] != RuneId.Water)
            {
                broken.Add("Obsidian must be Lava · Salt · Water");
            }

            if (!ChainBook.TryBirth(RuneId.Flame, out var flameBirth)
                || flameBirth.Count != 3
                || flameBirth[0] != RuneId.Fire
                || flameBirth[1] != RuneId.Animus
                || flameBirth[2] != RuneId.Fire)
            {
                broken.Add("Flame must be Fire · Animus · Fire");
            }

            if (!ChainBook.TryBirth(RuneId.Glacier, out var glacierBirth)
                || glacierBirth.Count != 3
                || glacierBirth[0] != RuneId.Ice
                || glacierBirth[1] != RuneId.Animus
                || glacierBirth[2] != RuneId.Ice)
            {
                broken.Add("Glacier must be Ice · Animus · Ice");
            }

            if (!ChainBook.TryBirth(RuneId.Plasma, out var plasmaBirth)
                || plasmaBirth.Count != 2
                || plasmaBirth[0] != RuneId.Flame
                || plasmaBirth[1] != RuneId.Lightning)
            {
                broken.Add("Plasma must be Flame · Lightning");
            }

            var mudJoin = Composition.FromSequence(new[] { RuneId.Earth, RuneId.Water });
            if (ChainBook.CollectExact(mudJoin, SpellShape.None).Count != 0)
            {
                broken.Add("Earth · Water joins to Mud, not a spell");
            }

            var quagmire = Composition.FromSequence(new[] { RuneId.Earth, RuneId.Water, RuneId.Salt });
            var quagmireExact = ChainBook.CollectExact(quagmire, SpellShape.None);
            if (quagmireExact.Count == 0 || quagmireExact[0].Spell != SpellId.Quagmire)
            {
                broken.Add("Earth · Water · Salt should be Quagmire");
            }

            var wind = Composition.FromSequence(new[] { RuneId.Air, RuneId.Mercury });
            var windExact = ChainBook.CollectExact(wind, SpellShape.None);
            if (windExact.Count == 0 || windExact[0].Spell != SpellId.Gust)
            {
                broken.Add("Air · Mercury should be Wind");
            }

            var obsidianWall = Composition.FromSequence(new[] { RuneId.Obsidian, RuneId.Salt, RuneId.Obsidian });
            var obsidianWallExact = ChainBook.CollectExact(obsidianWall, SpellShape.None);
            if (obsidianWallExact.Count == 0 || obsidianWallExact[0].Spell != SpellId.ObsidianWall)
            {
                broken.Add("Obsidian · Salt · Obsidian should be Obsidian-wall");
            }

            var obsidianWallRoots = Composition.FromSequence(new[]
            {
                RuneId.Lava, RuneId.Salt, RuneId.Water, RuneId.Salt,
                RuneId.Lava, RuneId.Salt, RuneId.Water
            });
            var obsidianWallFromRoots = ChainBook.CollectExact(obsidianWallRoots, SpellShape.None);
            if (obsidianWallFromRoots.Count == 0 || obsidianWallFromRoots[0].Spell != SpellId.ObsidianWall)
            {
                broken.Add("Lava · Salt · Water · Salt · Lava · Salt · Water should be Obsidian-wall");
            }

            var plasma = Composition.FromSequence(new[] { RuneId.Flame, RuneId.Lightning, RuneId.Mercury });
            var plasmaExact = ChainBook.CollectExact(plasma, SpellShape.None);
            if (plasmaExact.Count == 0 || plasmaExact[0].Spell != SpellId.Plasma)
            {
                broken.Add("Flame · Lightning · Mercury should be Plasma");
            }

            if (!TryGet(SpellId.Grotto, out var grotto)
                || !ChainBook.SameStory(grotto.RecipeRunes, ChainBook.Parse("Water · Salt · Earth · Dark"))
                || !ChainBook.SameStory(grotto.ViaRunes, ChainBook.Parse("Plant · Dark")))
            {
                broken.Add("Grotto must be Water · Salt · Earth · Dark, via Plant · Dark");
            }

            if (!TryGet(SpellId.StormCall, out var storm)
                || !ChainBook.SameStory(storm.ViaRunes, ChainBook.Parse("Cloud · Salt · Lightning · Mercury")))
            {
                broken.Add("Storm must be Cloud · Salt · Lightning · Mercury");
            }

            if (!TryGet(SpellId.Fog, out var fog)
                || !ChainBook.SameStory(fog.RecipeRunes, ChainBook.Parse("Cloud · Earth")))
            {
                broken.Add("Fog must be Cloud · Earth");
            }

            if (!TryGet(SpellId.Darkness, out var dark)
                || !ChainBook.SameStory(dark.ViaRunes, ChainBook.Parse("Cloud · Dark")))
            {
                broken.Add("Darkness must be Cloud · Dark");
            }

            WorldPhysics.Audit(broken);
            SpanLaw.Audit(broken);
            FocusLaw.Audit(broken);
            RuneCatalog.AuditLedger(broken);

            if (!TryGet(SpellId.MetalPillar, out _) || !TryGet(SpellId.MetalWall, out _) || !TryGet(SpellId.ObsidianWall, out _))
            {
                broken.Add("Metal-pillar, Metal-wall, and Obsidian-wall must be written in the developer book");
            }

            if (!ChainBook.TryBirth(RuneId.Anima, out var animaBirth)
                || animaBirth.Count != 3
                || animaBirth[0] != RuneId.Water
                || animaBirth[1] != RuneId.Sulphur
                || animaBirth[2] != RuneId.Earth)
            {
                broken.Add("Anima must be Water · Sulphur · Earth");
            }

            if (!ChainBook.TryBirth(RuneId.Animus, out var animusBirth)
                || animusBirth.Count != 3
                || animusBirth[0] != RuneId.Fire
                || animusBirth[1] != RuneId.Sulphur
                || animusBirth[2] != RuneId.Air)
            {
                broken.Add("Animus must be Fire · Sulphur · Air");
            }

            if (!RuneCatalog.TryParseName("Male", out var maleId) || maleId != RuneId.Animus
                || !RuneCatalog.TryParseName("Female", out var femaleId) || femaleId != RuneId.Anima)
            {
                broken.Add("Male and Female must be Animus and Anima");
            }

            var balm = Composition.FromSequence(new[] { RuneId.Anima, RuneId.Mercury });
            var balmExact = ChainBook.CollectExact(balm, SpellShape.None);
            if (balmExact.Count == 0 || balmExact[0].Spell != SpellId.Balm)
            {
                broken.Add("Anima · Mercury should be Balm");
            }

            var chorus = Composition.FromSequence(new[] { RuneId.Anima, RuneId.Salt });
            var chorusExact = ChainBook.CollectExact(chorus, SpellShape.None);
            if (chorusExact.Count == 0 || chorusExact[0].Spell != SpellId.Chorus)
            {
                broken.Add("Anima · Salt should be Chorus");
            }

            var drive = Composition.FromSequence(new[] { RuneId.Animus, RuneId.Mercury });
            var driveExact = ChainBook.CollectExact(drive, SpellShape.None);
            if (driveExact.Count == 0 || driveExact[0].Spell != SpellId.Drive)
            {
                broken.Add("Animus · Mercury should be Drive");
            }

            if (Entries.Length < 90)
            {
                broken.Add("The written book must keep every catalog spell, including 88–90 Anima and Animus");
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
            string gate = "",
            SpellId work = SpellId.None)
        {
            return new CodexEntry(number, book, spell, want, name, recipe, via, form, outcome, gate, work);
        }
    }
}
