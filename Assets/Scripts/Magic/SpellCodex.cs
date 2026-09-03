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
    /// 41–50 are Death / Free. 51 is Time-stop (Charter).
    /// 121–128 are plant-cure, light orbs, and living venom.
    /// 129 is Float.
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
            E(9, SpellBook.Hold, SpellId.LiveFloor, "The seed given a body around your feet. They cannot step.", "Live-floor", "Fire · Air · Salt", "Spark · Salt", "Grow", SpellOutcome.Kill),
            E(10, SpellBook.Hold, SpellId.Jolt, "The bolt, turned by Sulphur, reaches a mind.", "Jolt", "Fire · Air · Sulphur · Mercury", "Spark · Sulphur · Mercury", "Remote", SpellOutcome.Restrain),
            E(11, SpellBook.Hold, SpellId.Thunderclap, "The bolt meets rest, then every mind around you.", "Thunderclap", "Fire · Air · Earth · Sulphur", "Lightning · Earth · Sulphur", "Grow", SpellOutcome.Restrain),
            E(12, SpellBook.Weather, SpellId.StormCall, "The hanging veil given a body, then the bolt is sent from it.", "Storm", "Air · Water · Salt · Fire · Air · Mercury", "Cloud · Salt · Lightning · Mercury", "Remote", SpellOutcome.Kill),
            E(13, SpellBook.Weather, SpellId.Rain, "The hanging veil yields more and is sent down.", "Rain", "Air · Water · Water · Mercury", "Cloud · Water · Mercury", "Remote", SpellOutcome.Neither),
            E(14, SpellBook.SeeHide, SpellId.Fog, "The hanging veil is drawn to the ground.", "Fog", "Air · Water · Earth", "Cloud · Earth", "Grow", SpellOutcome.Neither),
            E(15, SpellBook.End, SpellId.Scald, "Hunger forced through yield and sent.", "Scald", "Fire · Water · Mercury", "Steam · Mercury", "Shot", SpellOutcome.Kill),
            E(16, SpellBook.Weather, SpellId.WaterJet, "Yield learns breath so it can leave the vessel, then is sent.", "Water-jet", "Water · Air · Mercury", "", "Shot", SpellOutcome.Restrain),
            E(17, SpellBook.Hold, SpellId.Flood, "Yield going, more yield, given a body. They bog.", "Flood", "Water · Mercury · Water · Salt", "Current · Water · Salt", "Grow", SpellOutcome.Restrain),
            E(18, SpellBook.Cross, SpellId.IcePillar, "Hard water asked to rest as a column. Over a pit it must join two floors, or it falls. On water it freezes without banks. It will thaw.", "Ice-pillar", "Water · Earth · Salt · Earth", "Ice · Salt · Earth", "Pillar", SpellOutcome.Restrain),
            E(19, SpellBook.Hold, SpellId.IceSpear, "Hard water sent.", "Ice-spear", "Water · Earth · Mercury", "Ice · Mercury", "Shot", SpellOutcome.Restrain),
            E(20, SpellBook.Hold, SpellId.Snowfall, "The veil is given ice’s story and sent softly.", "Snowfall", "Air · Water · Water · Earth · Mercury", "Cloud · Ice · Mercury", "Remote", SpellOutcome.Restrain),
            E(21, SpellBook.Cross, SpellId.Thaw, "The hard water-body meets hunger and remembers yield.", "Thaw", "Water · Earth · Fire", "Ice · Fire", "Remote", SpellOutcome.Neither),
            E(22, SpellBook.End, SpellId.HurledStone, "Rest given a body and sent. Earth flies.", "Hurled stone", "Earth · Salt · Mercury", "Stone · Mercury", "Shot", SpellOutcome.Kill),
            E(23, SpellBook.Cross, SpellId.Wall, "A body of rest asked to rest as more rest. A wall. Across a pit it is a two-tile span that must find floor or wall at each end, or it falls. Water takes mud, not a bridge.", "Wall", "Earth · Salt · Earth", "Stone · Earth", "Pillar", SpellOutcome.Neither),
            E(24, SpellBook.Cross, SpellId.Pit, "Rest asked to go, given breath so it leaves a hollow.", "Pit", "Earth · Mercury · Air", "", "Remote", SpellOutcome.Neither),
            E(25, SpellBook.Cross, SpellId.Bridge, "A body of rest given breath and sent across. A two-tile span that must find floor or wall at each end, or it falls. Water takes mud, not a bridge.", "Bridge", "Earth · Salt · Air · Mercury", "Stone · Air · Mercury", "Remote", SpellOutcome.Neither),
            E(26, SpellBook.Hold, SpellId.Quagmire, "Rest meeting yield, given a body. It holds them.", "Quagmire", "Earth · Water · Salt", "Mud · Salt", "Grow", SpellOutcome.Restrain),
            E(27, SpellBook.End, SpellId.LavaFlood, "Hungry earth asked to go.", "Lava-flood", "Fire · Earth · Mercury", "Lava · Mercury", "Remote", SpellOutcome.Kill),
            E(28, SpellBook.Cross, SpellId.ObsidianPath, "Hungry earth quenched and given a body. A path.", "Obsidian path", "Fire · Earth · Salt · Water", "Lava · Salt · Water", "Remote", SpellOutcome.Neither),
            E(29, SpellBook.GrowHeal, SpellId.Sprout, "A vegetable body marked living, from the feet. Plant cover in a three-tile disk.", "Sprout", "Water · Salt · Earth · Life", "Plant · Life", "Grow", SpellOutcome.Neither),
            E(30, SpellBook.Hold, SpellId.Vine, "The vegetable body sent. A climbing line from you to the mark. It holds them, and hunger can run it as a wick. A spell — the field speaks Plant.", "Vine", "Water · Salt · Earth · Mercury", "Plant · Mercury", "Shot", SpellOutcome.Restrain),
            E(31, SpellBook.GrowHeal, SpellId.Grow, "A living vegetable body sent. Plant cover at the mark, the way Sprout stands from the feet.", "Grow", "Water · Salt · Earth · Life · Mercury", "Plant · Life · Mercury", "Remote", SpellOutcome.Neither),
            E(32, SpellBook.GrowHeal, SpellId.Mend, "A living body, yield and rest, sent into the living.", "Mend", "Life · Salt · Water · Earth · Mercury", "", "Grow", SpellOutcome.Neither),
            E(33, SpellBook.Cross, SpellId.Hop, "Breath given a body, then more breath, kept on you. A leap.", "Hop", "Air · Salt · Air", "", "Self", SpellOutcome.Neither),
            E(34, SpellBook.Cross, SpellId.Flight, "Breath given logos and breath again, going, then stood on you. You fly.", "Flight", "Air · Animus · Air · Mercury · Salt", "", "Self", SpellOutcome.Neither),
            E(35, SpellBook.Mind, SpellId.Rage, "Fire sent, turned by Sulphur, into a mind.", "Rage", "Fire · Sulphur · Mercury", "", "Remote", SpellOutcome.Neither),
            E(36, SpellBook.Mind, SpellId.Terror, "The withheld reaches a mind. They flee or freeze.", "Terror", "Dark · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(37, SpellBook.Mind, SpellId.Lull, "Yield reaches a mind. They sleep. They can be woken.", "Lull", "Water · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(38, SpellBook.Weather, SpellId.Gale, "Breath going, more breath, so it can push.", "Gale", "Air · Mercury · Air", "", "Shot", SpellOutcome.Restrain),
            E(39, SpellBook.SeeHide, SpellId.Veil, "The withheld, a living body, as breath. Hard to see.", "Veil", "Dark · Life · Salt · Air", "", "Grow", SpellOutcome.Neither),
            E(40, SpellBook.Call, SpellId.CallBeast, "Flesh, marked living, given a mind, sent here. Know the formula.", "Call beast", "Earth · Water · Salt · Life · Sulphur · Mercury", "", "Remote", SpellOutcome.Neither),
            E(41, SpellBook.Grave, SpellId.Blight, "A vegetable body, then the grave, given a body. Verdure rots.", "Blight", "Water · Salt · Earth · Death · Salt", "Poison · Salt", "Grow", SpellOutcome.Kill, "Either"),
            E(42, SpellBook.Grave, SpellId.Shade, "Withheld, given a body, marked by the grave, and sent.", "Shade", "Dark · Death · Salt · Mercury", "Shade · Mercury", "Remote", SpellOutcome.Neither, "Free"),
            E(43, SpellBook.Grave, SpellId.Unmake, "The grave is sent into a living body.", "Unmake", "Death · Mercury · Life · Salt", "", "Remote", SpellOutcome.Kill, "Free"),
            E(44, SpellBook.Grave, SpellId.GraveSleep, "The waking passion is given to the grave. Sleep as if dead.", "Grave-sleep", "Life · Sulphur · Death", "", "Remote", SpellOutcome.Restrain, "Free"),
            E(45, SpellBook.Grave, SpellId.CorpseCall, "One grave is opened and a mind is sent. They rise and serve while you hold the sentence.", "Corpse-call", "Salt · Water · Earth · Fire · Death · Mercury", "", "Remote", SpellOutcome.Restrain, "Free"),
            E(46, SpellBook.Grave, SpellId.GraveDust, "Rest marked by the grave. Earth-life and golems come apart.", "Grave-dust", "Earth · Death · Salt", "", "Grow", SpellOutcome.Kill, "Either"),
            E(47, SpellBook.Grave, SpellId.Snuff, "Hunger marked by the grave and sent into a flame.", "Snuff", "Fire · Death · Mercury", "Ember · Mercury", "Remote", SpellOutcome.Neither, "Either"),
            E(48, SpellBook.Grave, SpellId.Blackout, "The seed marked by the grave and sent. A live rod dies.", "Blackout", "Fire · Air · Death · Mercury", "Spark · Death · Mercury", "Shot", SpellOutcome.Neither, "Either"),
            E(49, SpellBook.Grave, SpellId.DarkCrystal, "Crystal withheld and marked by the grave. They freeze in dark glass. Free masonry — easier than obsidian, notorious among Free sorcerers.", "Dark-crystal", "Crystal · Dark · Death", "Stone · Water · Dark · Death", "Remote", SpellOutcome.Restrain, "Free"),
            E(50, SpellBook.Grave, SpellId.LastBreath, "Living breath, then the grave, sent. The breath leaves them.", "Last breath", "Air · Life · Death · Mercury", "", "Remote", SpellOutcome.Kill, "Free"),
            E(51, SpellBook.Hold, SpellId.TimeStop, "Yield and rest are withheld. The living stay; the mind cannot hurry. The stopped moment stands.", "Time-stop", "Water · Earth · Dark · Life · Sulphur · Salt", "Ice · Dark · Life · Sulphur · Salt", "Grow", SpellOutcome.Restrain),
            E(52, SpellBook.Weather, SpellId.Douse, "Yield sent. Water thrown. Hunger ends.", "Douse", "Water · Mercury", "", "Shot", SpellOutcome.Neither),
            E(53, SpellBook.Mind, SpellId.Command, "A standing body given a mind and sent. They obey.", "Command", "Salt · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(54, SpellBook.Weather, SpellId.Gust, "Breath sent. Wind.", "Wind", "Air · Mercury", "", "Shot", SpellOutcome.Neither),
            E(55, SpellBook.Cross, SpellId.EarthPillar, "Rest given a body. A column of earth. Over a pit it must join two floors, or it falls. Water takes mud, not a span.", "Earth-pillar", "Earth · Salt", "Stone", "Pillar", SpellOutcome.Neither, "", SpellId.StonePillar),
            E(56, SpellBook.Mind, SpellId.Stoneskin, "Rest given a body, then the mind holds it on you. Earth and crushing — boulders, slams — break.", "Stoneskin", "Earth · Salt · Sulphur", "", "Self", SpellOutcome.Neither),
            E(57, SpellBook.Mind, SpellId.Watershield, "Yield given a body, then the mind holds it on you. Water breaks. You walk on yield.", "Water ward", "Water · Salt · Sulphur", "", "Self", SpellOutcome.Neither),
            E(58, SpellBook.Mind, SpellId.Flameward, "Hunger given a body, then the mind holds it on you. Fire breaks.", "Flame ward", "Fire · Salt · Sulphur", "", "Self", SpellOutcome.Neither),
            E(59, SpellBook.Mind, SpellId.Windward, "Breath given a body, then the mind holds it on you. Air breaks. Fog and foul breath leave.", "Wind ward", "Air · Salt · Sulphur", "", "Self", SpellOutcome.Neither),
            E(60, SpellBook.End, SpellId.LavaPillar, "Hungry earth given a standing body. It stands. Yield cools it to rock.", "Lava-pillar", "Fire · Earth · Salt", "Lava · Salt", "Pillar", SpellOutcome.Kill),
            E(61, SpellBook.Cross, SpellId.Shatter, "A stood wall given breath and sent. Matter comes apart.", "Shatter", "Earth · Salt · Earth · Air · Mercury", "Stone · Earth · Air · Mercury", "Remote", SpellOutcome.Neither),
            E(62, SpellBook.Mind, SpellId.Confuse, "Breath turned by Sulphur, into a mind. They lose the thread.", "Confuse", "Air · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(63, SpellBook.Cross, SpellId.IceWall, "A body of ice asked to stand as more ice. A wall. Across a pit it is a two-tile span that must find floor or wall at each end, or it falls. On water it freezes without banks. It will thaw.", "Ice-wall", "Water · Earth · Salt · Water · Earth", "Ice · Salt · Ice", "Pillar", SpellOutcome.Restrain),
            E(64, SpellBook.Hold, SpellId.Freeze, "Hard water held as a condition. They freeze.", "Freeze", "Water · Earth · Sulphur", "Ice · Sulphur", "Remote", SpellOutcome.Restrain),
            E(65, SpellBook.Weather, SpellId.Snowstorm, "The veil given ice’s story, then driven. They freeze.", "Snowstorm", "Air · Water · Water · Earth · Air · Mercury", "Cloud · Ice · Air · Mercury", "Remote", SpellOutcome.Restrain),
            E(66, SpellBook.Weather, SpellId.Push, "Breath given a body and sent. Wind that pushes the person.", "Push", "Air · Salt · Mercury", "", "Shot", SpellOutcome.Restrain),
            E(67, SpellBook.End, SpellId.LightningStrike, "A spark given form from the air, moving at something. It falls from the sky.", "Lightning strike", "Fire · Air · Salt · Air · Mercury", "Spark · Salt · Air · Mercury", "Remote", SpellOutcome.Kill),
            E(68, SpellBook.Mind, SpellId.Charm, "A living mind is reached and sent. They fetch, and they fight what you have marked.", "Charm", "Life · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(69, SpellBook.Hold, SpellId.Swamp, "Rest meeting yield, going, given a body around your feet. A watery swamp.", "Swamp", "Earth · Water · Mercury · Salt", "Mud · Mercury · Salt", "Grow", SpellOutcome.Restrain),
            E(70, SpellBook.End, SpellId.Witchfire, "Fire given logos and its own perpetuity, then sent. Witchfire. It eats what ordinary hunger cannot.", "Witchfire", "Fire · Animus · Fire · Mercury", "Flame · Mercury", "Remote", SpellOutcome.Kill),
            E(71, SpellBook.GrowHeal, SpellId.Wither, "The vegetable body is withheld. Plants around your feet die. What remains speaks Death.", "Wither", "Water · Salt · Earth · Dark", "Plant · Dark", "Grow", SpellOutcome.Kill),
            E(72, SpellBook.Weather, SpellId.Thunder, "The arc meeting rest.", "Thunder", "Fire · Air · Earth", "Lightning · Earth", "Remote", SpellOutcome.Neither),
            E(73, SpellBook.SeeHide, SpellId.Darkness, "The hanging veil is withheld. Nothing in the vicinity can see.", "Darkness", "Air · Water · Dark", "Cloud · Dark", "Remote", SpellOutcome.Neither),
            E(74, SpellBook.Weather, SpellId.Blizzard, "The veil given ice, a body, going, and wind.", "Blizzard", "Air · Water · Salt · Water · Earth · Mercury · Air · Mercury", "Cloud · Salt · Ice · Mercury · Air · Mercury", "Remote", SpellOutcome.Restrain),
            E(75, SpellBook.Weather, SpellId.Sandstorm, "Breath going, driving grit.", "Sandstorm", "Air · Mercury · Air · Earth · Mercury", "Air · Mercury · Dust · Mercury", "Remote", SpellOutcome.Neither),
            E(76, SpellBook.Cross, SpellId.WaterPillar, "Yield and rest given a standing body. A column of water.", "Water-pillar", "Water · Earth · Salt", "Ice · Salt", "Pillar", SpellOutcome.Neither),
            E(77, SpellBook.End, SpellId.OilShot, "Fuel sent. Surfaces hold flame. Fire already standing grows.", "Oil shot", "Water · Salt · Earth · Fire · Earth · Mercury", "Oil · Mercury", "Shot", SpellOutcome.Neither),
            E(78, SpellBook.End, SpellId.OilPillar, "A stood wick. A later fire sentence would make it a bomb.", "Oil-pillar", "Water · Salt · Earth · Fire · Earth · Salt · Earth", "Oil · Salt · Earth", "Pillar", SpellOutcome.Neither),
            E(79, SpellBook.Grave, SpellId.Poison, "The grave of a plant, sent as a stream. It poisons what it crosses.", "Poison spray", "Water · Salt · Earth · Death · Mercury", "Poison · Mercury", "Shot", SpellOutcome.Kill, "Either"),
            E(80, SpellBook.SeeHide, SpellId.Miasma, "The hanging veil forced through acid. Foul breath.", "Miasma", "Cloud · Acid", "", "Grow", SpellOutcome.Kill),
            E(81, SpellBook.End, SpellId.Plasma, "Witchfire joined to the bolt and sent. Ordinary matter ends. Obsidian and warded stone refuse it.", "Plasma", "Fire · Animus · Fire · Fire · Air · Air · Mercury", "Plasma · Mercury", "Shot", SpellOutcome.Kill),
            E(82, SpellBook.End, SpellId.FirePillar, "Hunger given a standing body. A column of fire. Without a source it goes out in a few seconds.", "Fire-pillar", "Fire · Salt", "", "Pillar", SpellOutcome.Kill),
            E(83, SpellBook.Weather, SpellId.Monsoon, "Yield given a body and sent. A remote flood. The monsoon.", "Monsoon", "Water · Salt · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(84, SpellBook.Cross, SpellId.DirtToss, "Rest sent without a body. Loose dirt. It smothers ground-fire and leaves Earth speaking where it lands.", "Dirt toss", "Earth · Mercury", "", "Shot", SpellOutcome.Neither),
            E(85, SpellBook.Cross, SpellId.MetalPillar, "Hungry earth given spark and asked to stand. A column of iron. It hangs without a far bank.", "Metal-pillar", "Fire · Earth · Fire · Air · Earth · Salt · Earth", "Metal · Salt · Earth", "Pillar", SpellOutcome.Neither),
            E(86, SpellBook.Cross, SpellId.MetalWall, "A body of iron asked to stand as more iron. A wall. Over a gap it needs no far rest.", "Metal-wall", "Fire · Earth · Fire · Air · Earth · Salt · Fire · Earth · Fire · Air · Earth", "Metal · Salt · Metal", "Pillar", SpellOutcome.Neither),
            E(87, SpellBook.Cross, SpellId.ObsidianWall, "A body of black glass asked to stand as more black glass. A wall. Melt, Shatter, and hunger's thaw will not take it. Over a gap it needs no far rest.", "Obsidian-wall", "Fire · Earth · Salt · Water · Salt · Fire · Earth · Salt · Water", "Obsidian · Salt · Obsidian", "Pillar", SpellOutcome.Neither),
            E(88, SpellBook.GrowHeal, SpellId.Balm, "Care sent. Yield given mind and rest, then sent. It heals.", "Balm", "Water · Sulphur · Earth · Mercury", "Anima · Mercury", "Grow", SpellOutcome.Neither),
            E(89, SpellBook.GrowHeal, SpellId.Chorus, "Care given a body around the feet. The work opens to many.", "Chorus", "Water · Sulphur · Earth · Salt", "Anima · Salt", "Grow", SpellOutcome.Neither),
            E(90, SpellBook.End, SpellId.Drive, "Hunger given mind and breath, then sent. Logos sent. It goes out and does not return.", "Drive", "Fire · Sulphur · Air · Mercury", "Animus · Mercury", "Shot", SpellOutcome.Kill),
            E(91, SpellBook.GrowHeal, SpellId.Tree, "A living vegetable body given a standing body. A tree. Over a pit it must join two floors, or it falls. On water it grows a walkable cover without banks. Hunger eats it.", "Tree", "Water · Salt · Earth · Life · Salt", "Plant · Life · Salt", "Pillar", SpellOutcome.Neither),
            E(92, SpellBook.GrowHeal, SpellId.WoodWall, "A living plant asked to stand as more living plant. A line of trees. Across a pit it is a two-tile span that must find floor or wall at each end, or it falls. On water it grows a walkable cover without banks. Hunger eats it.", "Wood-wall", "Water · Salt · Earth · Life · Salt · Water · Salt · Earth · Life", "Plant · Life · Salt · Plant · Life", "Pillar", SpellOutcome.Neither),
            E(93, SpellBook.End, SpellId.OilPuddle, "Fuel given a standing body. A puddle. Surfaces hold flame.", "Oil puddle", "Water · Salt · Earth · Fire · Earth · Salt", "Oil · Salt", "Remote", SpellOutcome.Neither),
            E(94, SpellBook.End, SpellId.OilGeyser, "A stood fountain of fuel, sent to a point. Hunger that finds it will not leave — it burns as hall-fire does, until yield is thrown.", "Oil geyser", "Water · Salt · Earth · Fire · Earth · Salt · Mercury", "Oil · Salt · Mercury", "Remote", SpellOutcome.Neither),
            E(95, SpellBook.End, SpellId.OilSlick, "Fuel given a body, then more fuel. It runs outward from a point and covers a wide floor.", "Oil slick", "Water · Salt · Earth · Fire · Earth · Salt · Water · Salt · Earth · Fire · Earth", "Oil · Salt · Oil", "Remote", SpellOutcome.Neither),
            E(96, SpellBook.GrowHeal, SpellId.Forest, "A living plant opened to many, then more living plant. It drinks every water still on the screen and covers the pool to the edge of what you can see.", "Forest", "Water · Salt · Earth · Life · Water · Sulphur · Earth · Water · Salt · Earth · Life", "Plant · Life · Anima · Plant · Life", "Remote", SpellOutcome.Neither),
            E(97, SpellBook.Grave, SpellId.TaintedTree, "The grave of a plant given a standing body and asked to rest. A tainted tree. It weeps poison onto adjacent tiles until it is destroyed.", "Tainted-tree", "Water · Salt · Earth · Death · Salt · Earth", "Poison · Salt · Earth", "Pillar", SpellOutcome.Kill, "Either"),
            E(98, SpellBook.Mind, SpellId.Plantward, "A living plant stood, then the mind holds it on you. Plant, yield, and rest break. Green springs from your feet as you walk.", "Plant ward", "Water · Salt · Earth · Life · Salt · Sulphur", "Plant · Life · Salt · Sulphur", "Self", SpellOutcome.Neither),
            E(99, SpellBook.Mind, SpellId.FlameForm, "Witchfire given a body, then the mind holds you as that body. Hunger cannot take you. The walk kindles.", "Flame-form", "Fire · Animus · Fire · Salt · Sulphur", "Flame · Salt · Sulphur", "Self", SpellOutcome.Neither),
            E(100, SpellBook.Mind, SpellId.TideForm, "Yield given eros and yield again, stood, held. Water cannot take you. You walk it, and the walk is wet.", "Tide-form", "Water · Anima · Water · Salt · Sulphur", "", "Self", SpellOutcome.Neither),
            E(101, SpellBook.Mind, SpellId.StoneForm, "Rest given eros and rest again, stood, held. Earth and crushing cannot take you.", "Stone-form", "Earth · Anima · Earth · Salt · Sulphur", "", "Self", SpellOutcome.Neither),
            E(102, SpellBook.Mind, SpellId.GaleForm, "Breath given logos and breath again, stood, held. You become invisible. Enemies lose your trail. Air cannot take you. Fog and foul breath leave as you walk.", "Gale-form", "Air · Animus · Air · Salt · Sulphur", "", "Self", SpellOutcome.Neither),
            E(103, SpellBook.Mind, SpellId.GroveForm, "A living plant given eros and the living plant again, stood, held. You become the grove. Green springs as you walk.", "Grove-form", "Water · Salt · Earth · Life · Water · Sulphur · Earth · Water · Salt · Earth · Life · Salt · Sulphur", "Plant · Life · Anima · Plant · Life · Salt · Sulphur", "Self", SpellOutcome.Neither),
            E(104, SpellBook.Mind, SpellId.CloudForm, "The hanging veil given logos and eros and the veil again, stood, held. You become mist and fly. Pits and water cannot hold you.", "Cloud-form", "Air · Water · Fire · Sulphur · Air · Water · Sulphur · Earth · Air · Water · Salt · Sulphur", "Cloud · Animus · Anima · Cloud · Salt · Sulphur", "Self", SpellOutcome.Neither),
            E(105, SpellBook.Weather, SpellId.AcidRain, "The hanging veil forced through acid and sent down.", "Acid rain", "Cloud · Acid · Mercury", "", "Remote", SpellOutcome.Kill),
            E(106, SpellBook.Weather, SpellId.MetalRain, "The hanging veil given iron and sent down. Needles.", "Metal rain", "Cloud · Metal · Mercury", "", "Remote", SpellOutcome.Kill),
            E(107, SpellBook.Weather, SpellId.LavaRain, "The hanging veil given hungry earth and sent down.", "Lava rain", "Cloud · Lava · Mercury", "", "Remote", SpellOutcome.Kill),
            E(108, SpellBook.Weather, SpellId.EmberRain, "The hanging veil given hunger and sent down.", "Ember rain", "Cloud · Fire · Mercury", "", "Remote", SpellOutcome.Kill),
            E(109, SpellBook.Weather, SpellId.SparkRain, "The hanging veil given the seed and sent down.", "Spark rain", "Cloud · Spark · Mercury", "", "Remote", SpellOutcome.Kill),
            E(110, SpellBook.Weather, SpellId.OilRain, "The hanging veil given fuel and sent down.", "Oil rain", "Cloud · Oil · Mercury", "", "Remote", SpellOutcome.Neither),
            E(111, SpellBook.Weather, SpellId.AshRain, "The hanging veil given what hunger left and sent down.", "Ash rain", "Cloud · Ash · Mercury", "", "Remote", SpellOutcome.Neither),
            E(112, SpellBook.Weather, SpellId.PlantRain, "The hanging veil given a vegetable body and sent down. They root.", "Plant rain", "Cloud · Plant · Mercury", "", "Remote", SpellOutcome.Restrain),
            E(113, SpellBook.Grave, SpellId.DeathCloud, "The hanging veil is withheld, marked by the grave, given logos, and sent.", "Death-cloud", "Cloud · Dark · Death · Animus · Mercury", "", "Remote", SpellOutcome.Kill, "Free"),
            E(114, SpellBook.Weather, SpellId.AirWall, "Breath given a body, more breath, sent as a span. A wall of wind. They go.", "Air-wall", "Air · Salt · Air · Mercury", "", "Pillar", SpellOutcome.Restrain),
            E(115, SpellBook.End, SpellId.Glacier, "Hard water given logos and itself again, then sent. Ordinary fire cannot take it.", "Glacier", "Ice · Animus · Ice · Mercury", "Glacier · Mercury", "Shot", SpellOutcome.Kill),
            E(116, SpellBook.GrowHeal, SpellId.Cleanse, "Shown, given a body of yield, and sent into you. Poison and the lesser holds lift. Tricky work.", "Cleanse", "Light · Salt · Water · Mercury", "", "Self", SpellOutcome.Neither),
            E(117, SpellBook.Grave, SpellId.Turn, "The waking is given to the grave and a mind is sent. They walk as the dead. Know their formula.", "Turn", "Life · Death · Sulphur · Mercury", "", "Remote", SpellOutcome.Restrain, "Free"),
            E(118, SpellBook.Grave, SpellId.Animate, "Many graves are opened and opened to many. They rise together.", "Animate", "Salt · Water · Earth · Fire · Death · Anima · Mercury", "", "Grow", SpellOutcome.Restrain, "Free"),
            E(119, SpellBook.Grave, SpellId.DeathHost, "The four as a grave-body, opened to many, the mind holds. The host does not end until focus breaks.", "Death-host", "Salt · Water · Earth · Fire · Death · Anima · Sulphur", "", "Grow", SpellOutcome.Restrain, "Free"),
            E(120, SpellBook.End, SpellId.Exorcism, "Shown waking, sent. The dead cannot hold.", "Exorcism", "Light · Life · Mercury", "", "Remote", SpellOutcome.Kill),
            E(121, SpellBook.GrowHeal, SpellId.Wolfsbane, "A living plant given yield and sent. Wolfsbane. A patch grows from the mark. Yield walks it. Poison turns it. Poison already on a plant walks, the way yield walks green.", "Wolfsbane", "Water · Salt · Earth · Life · Water · Mercury", "Plant · Life · Water · Mercury", "Remote", SpellOutcome.Neither),
            E(122, SpellBook.GrowHeal, SpellId.GroveCure, "A living plant opened to many and sent. Poison lifts around the mark. Blighted green remembers itself.", "Grove-cure", "Water · Salt · Earth · Life · Water · Sulphur · Earth · Mercury", "Plant · Life · Anima · Mercury", "Remote", SpellOutcome.Neither),
            E(123, SpellBook.GrowHeal, SpellId.SunOrb, "Shown waking, given a body. A sun-orb. Poison lifts. The dead cannot hold. Blighted green remembers itself.", "Sun-orb", "Light · Life · Salt", "", "Pillar", SpellOutcome.Kill),
            E(124, SpellBook.GrowHeal, SpellId.Sanctuary, "Shown waking, opened to many, given a body. A sanctuary. Poison lifts. The dead cannot hold. Blighted green remembers itself.", "Sanctuary", "Light · Life · Water · Sulphur · Earth · Salt", "Light · Life · Anima · Salt", "Pillar", SpellOutcome.Kill),
            E(125, SpellBook.Grave, SpellId.Spore, "The grave of a plant given breath and sent. A spore. Foul breath that poisons what it crosses.", "Spore", "Water · Salt · Earth · Death · Air · Mercury", "Poison · Air · Mercury", "Shot", SpellOutcome.Kill, "Either"),
            E(126, SpellBook.Grave, SpellId.Hemlock, "A living plant, then the grave, sent. Hemlock. Living venom, stronger than the dead spray.", "Hemlock", "Water · Salt · Earth · Life · Death · Mercury", "Plant · Life · Death · Mercury", "Shot", SpellOutcome.Kill, "Either"),
            E(127, SpellBook.Grave, SpellId.Nightshade, "A living plant, then the grave, given a body. Nightshade. A living poison column. It weeps onto adjacent tiles until it is destroyed.", "Nightshade", "Water · Salt · Earth · Life · Death · Salt", "Plant · Life · Death · Salt", "Pillar", SpellOutcome.Kill, "Either"),
            E(128, SpellBook.Hold, SpellId.Briar, "A stood living plant sent. Briar. It holds them, and hunger can run it as a wick.", "Briar", "Water · Salt · Earth · Life · Salt · Mercury", "Plant · Life · Salt · Mercury", "Remote", SpellOutcome.Restrain),
            E(129, SpellBook.Cross, SpellId.Float, "Breath going, then stood on you. You hang. Pits will not take you. You barely walk. Wind, a vine, or a jet of yield moves you.", "Float", "Air · Mercury · Salt", "", "Self", SpellOutcome.Neither)
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

                if (entry.RecipeRunes.Count > SpellComposer.MaxSlots)
                {
                    broken.Add($"{entry.Number} {entry.Name}: recipe is {entry.RecipeRunes.Count} runes; the string holds {SpellComposer.MaxSlots}");
                }

                if (entry.ViaRunes.Count > SpellComposer.MaxSlots)
                {
                    broken.Add($"{entry.Number} {entry.Name}: via is {entry.ViaRunes.Count} runes; the string holds {SpellComposer.MaxSlots}");
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
            ValidatePlaybook(broken);

            return broken.Count == 0
                ? string.Empty
                : string.Join("; ", broken);
        }

        static void ValidatePlaybook(List<string> broken)
        {
            if (RuneCatalog.StringRole(RuneId.Fire) != "elemental"
                || RuneCatalog.StringRole(RuneId.Salt) != "catalyst"
                || RuneCatalog.StringRole(RuneId.Mercury) != "catalyst"
                || RuneCatalog.StringRole(RuneId.Sulphur) != "catalyst"
                || RuneCatalog.StringRole(RuneId.Animus) != "special"
                || RuneCatalog.StringRole(RuneId.Aether) != "special")
            {
                broken.Add("String roles must mark elemental, catalyst, and special runes");
            }

            var ledger = new CastLedger();
            var miss = Composition.FromSequence(new[] { RuneId.Fire });
            ledger.Record(miss, CastingStance.Charter, false, SpellId.None, hideBadRecipes: true);
            ledger.Record(miss, CastingStance.Charter, false, SpellId.None, hideBadRecipes: true);
            if (ledger.Recent.Count != 1 || ledger.Recent[0].Worked)
            {
                broken.Add("Hide bad recipes must keep only the last failed cast");
            }

            var recents = new CastLedger();
            var fireball = Composition.FromSequence(new[] { RuneId.Fire, RuneId.Mercury });
            var hop = Composition.FromSequence(new[] { RuneId.Air, RuneId.Salt, RuneId.Air });
            recents.Record(fireball, CastingStance.Charter, true, SpellId.Fireball);
            recents.Record(hop, CastingStance.Charter, true, SpellId.Hop);
            recents.Record(fireball, CastingStance.Free, true, SpellId.Fireball);
            if (recents.Recent.Count != 2
                || recents.Recent[0].Stance != CastingStance.Free
                || !WorkingNames.SameComposition(recents.Recent[0].Runes, fireball.Sequence)
                || !WorkingNames.SameComposition(recents.Recent[1].Runes, hop.Sequence))
            {
                broken.Add("Recent must show each writing once and move a recast to the top");
            }

            var book = new Grimoire();
            book.KeepWorking(CastingStance.Free, new[] { RuneId.Fire, RuneId.Mercury }, SpellId.None, "wild");
            if (book.KeptWorkings.Count != 1
                || book.KeptWorkings[0].Stance != CastingStance.Free
                || !WorkingNames.SameComposition(book.KeptWorkings[0].Runes, new[] { RuneId.Fire, RuneId.Mercury }))
            {
                broken.Add("Free workings must be keepable by the runes that were strung");
            }

            var unnamed = new Grimoire();
            if (!unnamed.TryAutoKeep(CastingStance.Charter, new[] { RuneId.Fire, RuneId.Mercury }, SpellId.Fireball)
                || unnamed.KeptWorkings.Count != 1
                || !string.IsNullOrEmpty(unnamed.KeptWorkings[0].GivenName)
                || unnamed.KeptWorkings[0].Label != WorkingNames.RunePhrase(new[] { RuneId.Fire, RuneId.Mercury })
                || unnamed.TryAutoKeep(CastingStance.Charter, new[] { RuneId.Fire, RuneId.Mercury }, SpellId.Fireball))
            {
                broken.Add("Add new spells must write an unnamed page once and leave it to be renamed");
            }

            if (!unnamed.RenameWorking(0, "Hunger sent")
                || unnamed.KeptWorkings[0].GivenName != "Hunger sent"
                || unnamed.Names.Call(new[] { RuneId.Fire, RuneId.Mercury }) != "Hunger sent")
            {
                broken.Add("A kept working must be renameable from the Grimoire");
            }

            if (!PrayerReveal.TryNamed("Fireball", out var prayed)
                || prayed.Spell != SpellId.Fireball
                || !PrayerReveal.TryRecipe("Fire · Mercury", out var fromChain)
                || fromChain.Spell != SpellId.Fireball
                || PrayerReveal.RolesOf(prayed.RecipeRunes).Count != 2
                || PrayerReveal.RolesOf(prayed.RecipeRunes)[0] != "elemental"
                || PrayerReveal.RolesOf(prayed.RecipeRunes)[1] != "catalyst")
            {
                broken.Add("Prayer must show a written spell and label elemental and catalyst marks");
            }

            if (!PrayerReveal.TryUnkept(unnamed, out var next)
                || WorkingNames.SameComposition(next.RecipeRunes, new[] { RuneId.Fire, RuneId.Mercury }))
            {
                broken.Add("An empty altar should offer a written spell that is not already in the book");
            }
        }

        static void ValidateFills(List<string> broken)
        {
            if (!TryGet(SpellId.Fireball, out var fireballEntry) ||
                !ChainBook.SameStory(fireballEntry.RecipeRunes, ChainBook.Parse("Fire · Mercury")))
            {
                return;
            }

            var salt = Composition.FromSequence(new[] { RuneId.Fire, RuneId.Salt });
            var firePillar = ChainBook.CollectExact(salt, SpellShape.None);
            if (firePillar.Count == 0 || firePillar[0].Spell != SpellId.FirePillar)
            {
                broken.Add("Fire · Salt should be Fire-pillar");
            }

            var flamePillar = Composition.FromSequence(new[] { RuneId.Fire, RuneId.Salt, RuneId.Earth });
            var flamePillarExact = ChainBook.CollectExact(flamePillar, SpellShape.None);
            if (flamePillarExact.Count == 0 || flamePillarExact[0].Spell != SpellId.FlamePillar)
            {
                broken.Add("Fire · Salt · Earth should stay Flame-pillar");
            }

            var lavaPillar = Composition.FromSequence(new[] { RuneId.Fire, RuneId.Earth, RuneId.Salt });
            var lavaPillarExact = ChainBook.CollectExact(lavaPillar, SpellShape.None);
            if (lavaPillarExact.Count == 0 || lavaPillarExact[0].Spell != SpellId.LavaPillar)
            {
                broken.Add("Fire · Earth · Salt should be Lava-pillar");
            }

            var lavaJoin = Composition.FromSequence(new[] { RuneId.Lava, RuneId.Salt });
            var lavaJoinExact = ChainBook.CollectExact(lavaJoin, SpellShape.None);
            if (lavaJoinExact.Count == 0 || lavaJoinExact[0].Spell != SpellId.LavaPillar)
            {
                broken.Add("Lava · Salt should be Lava-pillar");
            }

            var oldLava = Composition.FromSequence(new[] { RuneId.Fire, RuneId.Earth, RuneId.Salt, RuneId.Earth });
            if (ChainBook.CollectExact(oldLava, SpellShape.None).Count != 0)
            {
                broken.Add("Fire · Earth · Salt · Earth is no longer Lava-pillar");
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

            if (FocusLaw.Breaks(StatusId.Watershield, SpellId.Douse)
                || FocusLaw.Breaks(StatusId.Flameward, SpellId.Fireball)
                || FocusLaw.Breaks(StatusId.Sleeping, SpellId.Fireball)
                || FocusLaw.Breaks(StatusId.Charmed, SpellId.Fireball)
                || FocusLaw.Breaks(StatusId.Charmed, SpellId.Wall)
                || FocusLaw.Breaks(StatusId.Stoneskin, SpellId.Wall)
                || FocusLaw.Breaks(StatusId.Stoneskin, SpellId.Hop)
                || FocusLaw.Breaks(StatusId.Sleeping, SpellId.Hop))
            {
                broken.Add("A non-focus sentence must not drop a hold");
            }

            if (!FocusLaw.Breaks(StatusId.Sleeping, SpellId.Rage))
            {
                broken.Add("Rage must drop sleep — both send Mercury");
            }

            if (!FocusLaw.Breaks(StatusId.Charmed, SpellId.Command))
            {
                broken.Add("Command must drop charm — both send Mercury");
            }

            if (!FocusLaw.Breaks(StatusId.Watershield, SpellId.Lull))
            {
                broken.Add("Lull must drop a water ward — both use Water");
            }

            if (!FocusLaw.Breaks(StatusId.Flameward, SpellId.Rage))
            {
                broken.Add("Rage must drop a flame ward — both use Fire");
            }

            if (!FocusLaw.Breaks(StatusId.Stoneskin, SpellId.Flameward))
            {
                broken.Add("A later ward must drop stoneskin — both stand with Salt");
            }

            if (FocusLaw.Breaks(StatusId.Charmed, SpellId.Stoneskin)
                || FocusLaw.Breaks(StatusId.Stoneskin, SpellId.Charm)
                || FocusLaw.Breaks(StatusId.Sleeping, SpellId.Flameward))
            {
                broken.Add("Sulphur alone must not drop a hold — charm can stand with stoneskin");
            }

            if (!FocusLaw.Breaks(StatusId.Charmed, SpellId.Plantward))
            {
                broken.Add("Plant ward must drop charm — both write Life");
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

            if (SpellVerb.Of(SpellId.Blight).Status != StatusId.Poisoned
                || SpellVerb.Of(SpellId.Poison).Tiles != TileVerb.Poison
                || SpellVerb.Of(SpellId.Miasma).Tiles != TileVerb.Foul)
            {
                broken.Add("Blight and miasma must foul a cloud; Poison must slick a liquid");
            }

            if (!WorldWork.StopsOnWalls(SpellId.Fireball)
                || !WorldWork.StopsOnWalls(SpellId.IceSpear)
                || !WorldWork.StopsOnWalls(SpellId.HurledStone)
                || !WorldWork.StopsOnWalls(SpellId.DirtToss)
                || !WorldWork.StopsOnWalls(SpellId.WaterJet)
                || !WorldWork.StopsOnWalls(SpellId.Douse)
                || !WorldWork.StopsOnWalls(SpellId.Gust)
                || !WorldWork.StopsOnWalls(SpellId.Push)
                || !WorldWork.StopsOnWalls(SpellId.LightningBolt)
                || !WorldWork.StopsOnWalls(SpellId.Vine))
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
                || WorldWork.StopsOnWalls(SpellId.Wither))
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

            if (!TryGet(SpellId.Wither, out var wither)
                || !ChainBook.SameStory(wither.RecipeRunes, ChainBook.Parse("Water · Salt · Earth · Dark"))
                || !ChainBook.SameStory(wither.ViaRunes, ChainBook.Parse("Plant · Dark"))
                || wither.Shape != SpellShape.Spread)
            {
                broken.Add("Wither must be Water · Salt · Earth · Dark, via Plant · Dark, from the feet");
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
            VitalLaw.Audit(broken);
            StrikeLaw.Audit(broken);
            SpellGrammar.Audit(broken);
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

            var tree = Composition.FromSequence(new[] { RuneId.Plant, RuneId.Vita, RuneId.Salt });
            var treeExact = ChainBook.CollectExact(tree, SpellShape.None);
            if (treeExact.Count == 0 || treeExact[0].Spell != SpellId.Tree)
            {
                broken.Add("Plant · Life · Salt should be Tree");
            }

            var treeRoots = Composition.FromSequence(new[]
            {
                RuneId.Water, RuneId.Salt, RuneId.Earth, RuneId.Vita, RuneId.Salt
            });
            var treeFromRoots = ChainBook.CollectExact(treeRoots, SpellShape.None);
            if (treeFromRoots.Count == 0 || treeFromRoots[0].Spell != SpellId.Tree)
            {
                broken.Add("Water · Salt · Earth · Life · Salt should be Tree");
            }

            var oldForest = Composition.FromSequence(new[] { RuneId.Plant, RuneId.Vita, RuneId.Earth });
            if (ChainBook.CollectExact(oldForest, SpellShape.None).Count != 0)
            {
                broken.Add("Plant · Life · Earth is no longer Forest — Tree is Plant · Life · Salt");
            }

            var woodWall = Composition.FromSequence(new[]
            {
                RuneId.Plant, RuneId.Vita, RuneId.Salt, RuneId.Plant, RuneId.Vita
            });
            var woodWallExact = ChainBook.CollectExact(woodWall, SpellShape.None);
            if (woodWallExact.Count == 0 || woodWallExact[0].Spell != SpellId.WoodWall)
            {
                broken.Add("Plant · Life · Salt · Plant · Life should be Wood-wall");
            }

            var woodWallRoots = Composition.FromSequence(new[]
            {
                RuneId.Water, RuneId.Salt, RuneId.Earth, RuneId.Vita, RuneId.Salt,
                RuneId.Water, RuneId.Salt, RuneId.Earth, RuneId.Vita
            });
            var woodWallFromRoots = ChainBook.CollectExact(woodWallRoots, SpellShape.None);
            if (woodWallFromRoots.Count == 0 || woodWallFromRoots[0].Spell != SpellId.WoodWall)
            {
                broken.Add("Water · Salt · Earth · Life · Salt · Water · Salt · Earth · Life should be Wood-wall");
            }

            if (!TryGet(SpellId.Tree, out _) || !TryGet(SpellId.WoodWall, out _))
            {
                broken.Add("Tree and Wood-wall must be written in the developer book");
            }

            var forest = Composition.FromSequence(new[]
            {
                RuneId.Plant, RuneId.Vita, RuneId.Anima, RuneId.Plant, RuneId.Vita
            });
            var forestExact = ChainBook.CollectExact(forest, SpellShape.None);
            if (forestExact.Count == 0 || forestExact[0].Spell != SpellId.Forest)
            {
                broken.Add("Plant · Life · Anima · Plant · Life should be Forest");
            }

            var forestRoots = Composition.FromSequence(new[]
            {
                RuneId.Water, RuneId.Salt, RuneId.Earth, RuneId.Vita,
                RuneId.Water, RuneId.Sulphur, RuneId.Earth,
                RuneId.Water, RuneId.Salt, RuneId.Earth, RuneId.Vita
            });
            var forestFromRoots = ChainBook.CollectExact(forestRoots, SpellShape.None);
            if (forestFromRoots.Count == 0 || forestFromRoots[0].Spell != SpellId.Forest)
            {
                broken.Add("Water · Salt · Earth · Life · Water · Sulphur · Earth · Water · Salt · Earth · Life should be Forest");
            }

            if (!TryGet(SpellId.Forest, out _))
            {
                broken.Add("Forest must be written in the developer book");
            }

            var oilPuddle = Composition.FromSequence(new[] { RuneId.Oil, RuneId.Salt });
            var oilPuddleExact = ChainBook.CollectExact(oilPuddle, SpellShape.None);
            if (oilPuddleExact.Count == 0 || oilPuddleExact[0].Spell != SpellId.OilPuddle)
            {
                broken.Add("Oil · Salt should be Oil puddle");
            }

            var oilGeyser = Composition.FromSequence(new[] { RuneId.Oil, RuneId.Salt, RuneId.Mercury });
            var oilGeyserExact = ChainBook.CollectExact(oilGeyser, SpellShape.None);
            if (oilGeyserExact.Count == 0 || oilGeyserExact[0].Spell != SpellId.OilGeyser)
            {
                broken.Add("Oil · Salt · Mercury should be Oil geyser");
            }

            var oilSlick = Composition.FromSequence(new[] { RuneId.Oil, RuneId.Salt, RuneId.Oil });
            var oilSlickExact = ChainBook.CollectExact(oilSlick, SpellShape.None);
            if (oilSlickExact.Count == 0 || oilSlickExact[0].Spell != SpellId.OilSlick)
            {
                broken.Add("Oil · Salt · Oil should be Oil slick");
            }

            var oilPillar = Composition.FromSequence(new[] { RuneId.Oil, RuneId.Salt, RuneId.Earth });
            var oilPillarExact = ChainBook.CollectExact(oilPillar, SpellShape.None);
            if (oilPillarExact.Count == 0 || oilPillarExact[0].Spell != SpellId.OilPillar)
            {
                broken.Add("Oil · Salt · Earth should stay Oil-pillar");
            }

            if (!TryGet(SpellId.OilPuddle, out _) || !TryGet(SpellId.OilGeyser, out _) || !TryGet(SpellId.OilSlick, out _))
            {
                broken.Add("Oil puddle, Oil geyser, and Oil slick must be written in the developer book");
            }

            if (Entries.Length < 129)
            {
                broken.Add("The written book must keep every catalog spell, including wolfsbane, the light orbs, living venom, and Float");
            }

            var flight = Composition.FromSequence(new[]
            {
                RuneId.Air, RuneId.Animus, RuneId.Air, RuneId.Mercury, RuneId.Salt
            });
            var flightExact = ChainBook.CollectExact(flight, SpellShape.None);
            if (flightExact.Count == 0 || flightExact[0].Spell != SpellId.Flight)
            {
                broken.Add("Air · Animus · Air · Mercury · Salt should be Flight");
            }

            var hover = Composition.FromSequence(new[] { RuneId.Air, RuneId.Mercury, RuneId.Salt });
            var hoverExact = ChainBook.CollectExact(hover, SpellShape.None);
            if (hoverExact.Count == 0 || hoverExact[0].Spell != SpellId.Float)
            {
                broken.Add("Air · Mercury · Salt should be Float");
            }

            if (!TryGet(SpellId.TimeStop, out var timeStop) || timeStop.FreeOnly)
            {
                broken.Add("Time-stop is Charter — it no longer writes Death");
            }

            if (!ChainBook.TryBirth(RuneId.DarkCrystal, out var darkCrystalBirth)
                || darkCrystalBirth.Count != 3
                || darkCrystalBirth[0] != RuneId.Crystal
                || darkCrystalBirth[1] != RuneId.Umbra
                || darkCrystalBirth[2] != RuneId.Mors)
            {
                broken.Add("Dark-crystal must be Crystal · Dark · Death");
            }

            var grow = Composition.FromSequence(new[] { RuneId.Plant, RuneId.Vita, RuneId.Mercury });
            var growExact = ChainBook.CollectExact(grow, SpellShape.None);
            if (growExact.Count == 0 || growExact[0].Spell != SpellId.Grow)
            {
                broken.Add("Plant · Life · Mercury should be Grow");
            }

            var oldVineRise = Composition.FromSequence(new[] { RuneId.Plant, RuneId.Mercury, RuneId.Earth });
            if (ChainBook.CollectExact(oldVineRise, SpellShape.None).Count != 0)
            {
                broken.Add("Plant · Mercury · Earth is no longer Vine-rise — Grow is Plant · Life · Mercury");
            }

            var tainted = Composition.FromSequence(new[] { RuneId.Poison, RuneId.Salt, RuneId.Earth });
            var taintedExact = ChainBook.CollectExact(tainted, SpellShape.None);
            if (taintedExact.Count == 0 || taintedExact[0].Spell != SpellId.TaintedTree)
            {
                broken.Add("Poison · Salt · Earth should be Tainted-tree");
            }

            var plantWard = Composition.FromSequence(new[] { RuneId.Plant, RuneId.Vita, RuneId.Salt, RuneId.Sulphur });
            var plantWardExact = ChainBook.CollectExact(plantWard, SpellShape.None);
            if (plantWardExact.Count == 0 || plantWardExact[0].Spell != SpellId.Plantward)
            {
                broken.Add("Plant · Life · Salt · Sulphur should be Plant ward");
            }

            if (!ElementalLaw.WardsAgainst(Essence.Fire, Essence.Fire)
                || !ElementalLaw.WardsAgainst(Essence.Water, Essence.Water)
                || ElementalLaw.WardsAgainst(Essence.Water, Essence.Fire)
                || !ElementalLaw.WardsAgainst(Essence.Plant, Essence.Water)
                || !ElementalLaw.WardsAgainst(Essence.Plant, Essence.Earth)
                || !ElementalLaw.WardsAgainst(Essence.Plant, Essence.Plant))
            {
                broken.Add("A ward must turn its own element and the roots that constructed it, not the old opposite");
            }

            if (SpellVerb.Of(SpellId.Grow).Tiles != TileVerb.Grow
                || WorldWork.IsPlantGrowWork(SpellId.Grow)
                || SpellVerb.Of(SpellId.Wither).Tiles != TileVerb.Wither)
            {
                broken.Add("Grow is sprout at range; Wither withholds plant from the feet");
            }

            if (!TryGet(SpellId.Poison, out var poisonSpray)
                || poisonSpray.Name != "Poison spray"
                || !WorldWork.IsPoisonLiquid(SpellId.Poison)
                || !WorldPhysics.SweepsPath(SpellId.Poison, SpellShape.Shot))
            {
                broken.Add("Poison · Mercury must be a poison spray that streams along its path");
            }

            var wolfsbane = Composition.FromSequence(new[] { RuneId.Plant, RuneId.Vita, RuneId.Water, RuneId.Mercury });
            var wolfsbaneExact = ChainBook.CollectExact(wolfsbane, SpellShape.None);
            if (wolfsbaneExact.Count == 0 || wolfsbaneExact[0].Spell != SpellId.Wolfsbane)
            {
                broken.Add("Plant · Life · Water · Mercury should be Wolfsbane");
            }

            var groveCure = Composition.FromSequence(new[] { RuneId.Plant, RuneId.Vita, RuneId.Anima, RuneId.Mercury });
            var groveCureExact = ChainBook.CollectExact(groveCure, SpellShape.None);
            if (groveCureExact.Count == 0 || groveCureExact[0].Spell != SpellId.GroveCure)
            {
                broken.Add("Plant · Life · Anima · Mercury should be Grove-cure");
            }

            var sunOrb = Composition.FromSequence(new[] { RuneId.Lumen, RuneId.Vita, RuneId.Salt });
            var sunOrbExact = ChainBook.CollectExact(sunOrb, SpellShape.None);
            if (sunOrbExact.Count == 0 || sunOrbExact[0].Spell != SpellId.SunOrb)
            {
                broken.Add("Light · Life · Salt should be Sun-orb");
            }

            var sanctuary = Composition.FromSequence(new[] { RuneId.Lumen, RuneId.Vita, RuneId.Anima, RuneId.Salt });
            var sanctuaryExact = ChainBook.CollectExact(sanctuary, SpellShape.None);
            if (sanctuaryExact.Count == 0 || sanctuaryExact[0].Spell != SpellId.Sanctuary)
            {
                broken.Add("Light · Life · Anima · Salt should be Sanctuary");
            }

            var spore = Composition.FromSequence(new[] { RuneId.Poison, RuneId.Air, RuneId.Mercury });
            var sporeExact = ChainBook.CollectExact(spore, SpellShape.None);
            if (sporeExact.Count == 0 || sporeExact[0].Spell != SpellId.Spore)
            {
                broken.Add("Poison · Air · Mercury should be Spore");
            }

            var hemlock = Composition.FromSequence(new[] { RuneId.Plant, RuneId.Vita, RuneId.Mors, RuneId.Mercury });
            var hemlockExact = ChainBook.CollectExact(hemlock, SpellShape.None);
            if (hemlockExact.Count == 0 || hemlockExact[0].Spell != SpellId.Hemlock)
            {
                broken.Add("Plant · Life · Death · Mercury should be Hemlock");
            }

            var nightshade = Composition.FromSequence(new[] { RuneId.Plant, RuneId.Vita, RuneId.Mors, RuneId.Salt });
            var nightshadeExact = ChainBook.CollectExact(nightshade, SpellShape.None);
            if (nightshadeExact.Count == 0 || nightshadeExact[0].Spell != SpellId.Nightshade)
            {
                broken.Add("Plant · Life · Death · Salt should be Nightshade");
            }

            var briar = Composition.FromSequence(new[] { RuneId.Plant, RuneId.Vita, RuneId.Salt, RuneId.Mercury });
            var briarExact = ChainBook.CollectExact(briar, SpellShape.None);
            if (briarExact.Count == 0 || briarExact[0].Spell != SpellId.Briar)
            {
                broken.Add("Plant · Life · Salt · Mercury should be Briar");
            }

            if (SpellVerb.Of(SpellId.Wolfsbane).Tiles != TileVerb.Grow
                || SpellVerb.Of(SpellId.Wolfsbane).Radius != PlantLaw.GrowRadius
                || SpellVerb.Of(SpellId.SunOrb).Tiles != TileVerb.Restore
                || !StrikeLaw.Cleanses(SpellId.Wolfsbane)
                || !PlantLaw.PlantsNewBodies(SpellId.Wolfsbane)
                || !WorldWork.IsPoisonBreath(SpellId.Spore)
                || !WorldWork.IsPoisonLiquid(SpellId.Hemlock)
                || !WorldWork.IsPoisonWell(SpellId.Nightshade)
                || !WorldWork.IsVineWork(SpellId.Briar)
                || !WorldWork.IsLightWell(SpellId.SunOrb))
            {
                broken.Add("Wolfsbane is a sent living patch; spore is breath; hemlock is liquid; nightshade weeps; briar climbs; the orb stands");
            }

            var cloudForm = Composition.FromSequence(new[]
            {
                RuneId.Cloud, RuneId.Animus, RuneId.Anima, RuneId.Cloud, RuneId.Salt, RuneId.Sulphur
            });
            var cloudFormExact = ChainBook.CollectExact(cloudForm, SpellShape.None);
            if (cloudFormExact.Count == 0 || cloudFormExact[0].Spell != SpellId.CloudForm)
            {
                broken.Add("Cloud · Animus · Anima · Cloud · Salt · Sulphur should be Cloud-form");
            }

            FormLaw.Audit(broken);
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
