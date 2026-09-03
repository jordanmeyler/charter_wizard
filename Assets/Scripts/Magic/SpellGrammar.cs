using System.Collections.Generic;

namespace RuneMagic
{
    public enum SpellId
    {
        None = 0,
        Fireball,
        FlamePillar,
        Frenzy,
        Snuff,
        SunLance,
        Drive,
        Smother,
        WaterJet,
        IcePillar,
        Lull,
        Spring,
        Fog,
        Draw,
        LightningBolt,
        LiveFloor,
        Jolt,
        BrilliantArc,
        Blackout,
        HurledStone,
        StonePillar,
        RaisedEarth,
        Dread,
        Menhir,
        GraveDust,
        ShadowWell,
        Gale,
        Daze,
        DayWake,
        Gloom,
        Scald,
        ScatterDust,
        Sprout,
        VineRise,
        Grow = VineRise,
        CallGrowth,
        Melt,
        Ignite,
        ChainLightning,
        Thunderclap,
        StormCall,
        Rain,
        Flood,
        IceSpear,
        Snowfall,
        Thaw,
        Wall,
        Pit,
        Bridge,
        Quagmire,
        LavaFlood,
        ObsidianPath,
        Vine,
        Mend,
        Hop,
        Flight,
        Rage,
        Terror,
        Veil,
        CallBeast,
        Blight,
        Shade,
        Unmake,
        GraveSleep,
        CorpseCall,
        GraveIce,
        LastBreath,
        TimeStop,
        Douse,
        Command,
        Gust,
        EarthPillar,
        Stoneskin,
        Watershield,
        Flameward,
        Windward,
        LavaPillar,
        Shatter,
        Confuse,
        Freeze,
        Snowstorm,
        IceWall,
        Push,
        LightningStrike,
        Charm,
        Swamp,
        Witchfire,
        Grove,
        Grotto,
        Wither = Grotto,
        Thunder,
        Darkness,
        Blizzard,
        Sandstorm,
        WaterPillar,
        OilShot,
        OilPillar,
        Poison,
        Miasma,
        Plasma,
        Forest,
        Monsoon,
        DirtToss,
        MetalPillar,
        MetalWall,
        ObsidianWall,
        Balm,
        Chorus,
        Tree,
        WoodWall,
        OilPuddle,
        OilGeyser,
        OilSlick,
        FirePillar,
        TaintedTree,
        Plantward,
        FlameForm,
        TideForm,
        StoneForm,
        GaleForm,
        GroveForm,
        CloudForm,
        AcidRain,
        MetalRain,
        LavaRain,
        EmberRain,
        SparkRain,
        OilRain,
        AshRain,
        PlantRain,
        DeathCloud,
        AirWall,
        Glacier,
        Cleanse,
        Turn,
        Animate,
        DeathHost,
        Exorcism,
        Wolfsbane,
        GroveCure,
        SunOrb,
        Sanctuary,
        Spore,
        Hemlock,
        Nightshade,
        Briar,
        Float,
        Blink,
        Teleport,
        // Aliases of earlier ids. Keep these last so they do not
        // reset the next unique values.
        DarkCrystal = GraveIce
    }

    public readonly struct SpellRecipe
    {
        public SpellRecipe(RuneId material, RuneId aspect, SpellShape shape, SpellId spell, string name, string effect)
        {
            Material = material;
            Aspect = aspect;
            Shape = shape;
            Spell = spell;
            Name = name;
            Effect = effect;
        }

        public RuneId Material { get; }
        public RuneId Aspect { get; }
        public SpellShape Shape { get; }
        public SpellId Spell { get; }
        public string Name { get; }
        public string Effect { get; }
        public (RuneId Material, RuneId Aspect, SpellShape Shape) Key => (Material, Aspect, Shape);
    }

    /// <summary>
    /// Compressed sanctum slice: folded material × last operator × formation.
    /// The real language is the story-chains in SPELLS.md / SpellCodex.
    /// A sensible-looking combo that is not written here fizzles under Charter.
    /// </summary>
    public static class SpellGrammar
    {
        static readonly Dictionary<(RuneId, RuneId, SpellShape), SpellRecipe> Recipes = new();
        static readonly List<SpellRecipe> Ordered = new();

        static SpellGrammar()
        {
            Register(RuneId.Fire, RuneId.Mercury, SpellShape.Shot, SpellId.Fireball, "Fireball", "Compressed. Catalog: Fire · Mercury. Hunger sent.");
            Register(RuneId.Flame, RuneId.Mercury, SpellShape.Remote, SpellId.Witchfire, "Witchfire", "Compressed. Catalog: Fire · Animus · Fire · Mercury. Fire given logos and its own perpetuity, then sent.");
            Register(RuneId.Fire, RuneId.Salt, SpellShape.Pillar, SpellId.FirePillar, "Fire-pillar", "Compressed. Catalog: Fire · Salt. Hunger given a standing body. Without a source it goes out in a few seconds.");
            Register(RuneId.Fire, RuneId.Sulphur, SpellShape.Self, SpellId.Flameward, "Flame ward", "Compressed. Catalog: Fire · Salt · Sulphur. Hunger given a body, then the mind holds it on you.");
            Register(RuneId.Fire, RuneId.Sulphur, SpellShape.Spread, SpellId.Frenzy, "Frenzy", "Heat in the thoughts, from the feet out.");
            Register(RuneId.Fire, RuneId.Mors, SpellShape.Remote, SpellId.Snuff, "Snuff", "Death-work. Hunger marked by the grave, placed on a flame.");
            Register(RuneId.Fire, RuneId.Lumen, SpellShape.Shot, SpellId.SunLance, "Sun-lance", "Light riding fire.");
            Register(RuneId.Fire, RuneId.Animus, SpellShape.Shot, SpellId.Drive, "Drive", "Compressed. Catalog: Animus · Mercury. Hunger given mind and breath, then sent.");
            Register(RuneId.Animus, RuneId.Mercury, SpellShape.Shot, SpellId.Drive, "Drive", "Compressed. Catalog: Fire · Sulphur · Air · Mercury. Logos sent.");
            Register(RuneId.Fire, RuneId.Umbra, SpellShape.Remote, SpellId.Smother, "Smother", "Dark laid over a flame.");

            Register(RuneId.Water, RuneId.Mercury, SpellShape.Shot, SpellId.WaterJet, "Water-jet", "Water thrown as a line.");
            Register(RuneId.Water, RuneId.Salt, SpellShape.Remote, SpellId.Monsoon, "Monsoon", "Compressed. Catalog: Water · Salt · Mercury. A remote flood. Yield given a body and sent.");
            Register(RuneId.Water, RuneId.Salt, SpellShape.Pillar, SpellId.WaterPillar, "Water-pillar", "Compressed. Catalog: Water · Earth · Salt. Yield and rest given a standing body.");
            Register(RuneId.Ice, RuneId.Salt, SpellShape.Pillar, SpellId.WaterPillar, "Water-pillar", "Compressed. Catalog: Ice · Salt. Hard water given a standing body.");
            Register(RuneId.Ice, RuneId.Earth, SpellShape.Pillar, SpellId.IcePillar, "Ice-pillar", "Hard water asked to rest as a column.");
            Register(RuneId.Ice, RuneId.Sulphur, SpellShape.Remote, SpellId.Freeze, "Freeze", "Hard water held as a condition. They freeze.");
            Register(RuneId.Ice, RuneId.Mercury, SpellShape.Shot, SpellId.IceSpear, "Ice-spear", "Hard water sent.");
            Register(RuneId.Cloud, RuneId.Earth, SpellShape.Spread, SpellId.Fog, "Fog", "Compressed. Catalog: Cloud · Earth. The hanging veil drawn to the ground.");
            Register(RuneId.Cloud, RuneId.Umbra, SpellShape.Remote, SpellId.Darkness, "Darkness", "Compressed. Catalog: Cloud · Dark. Nothing in the vicinity can see.");
            Register(RuneId.Lightning, RuneId.Earth, SpellShape.Remote, SpellId.Thunder, "Thunder", "The arc meeting rest.");
            Register(RuneId.Water, RuneId.Sulphur, SpellShape.Self, SpellId.Watershield, "Water ward", "Compressed. Catalog: Water · Salt · Sulphur. Yield given a body, then the mind holds it on you.");
            Register(RuneId.Water, RuneId.Sulphur, SpellShape.Remote, SpellId.Lull, "Lull", "Mind of water. Sleep, placed elsewhere.");
            Register(RuneId.Water, RuneId.Vita, SpellShape.Spread, SpellId.Spring, "Spring", "Life welling from the feet.");
            Register(RuneId.Water, RuneId.Umbra, SpellShape.Spread, SpellId.Darkness, "Darkness", "Yield withheld. The room cannot see.");
            Register(RuneId.Water, RuneId.Anima, SpellShape.Remote, SpellId.Draw, "Draw", "Receptive pull. It calls, it does not strike.");
            Register(RuneId.Anima, RuneId.Mercury, SpellShape.Spread, SpellId.Balm, "Balm", "Compressed. Catalog: Anima · Mercury. Care sent. It heals.");
            Register(RuneId.Anima, RuneId.Salt, SpellShape.Spread, SpellId.Chorus, "Chorus", "Compressed. Catalog: Anima · Salt. Care given a body around the feet. The work opens to many.");

            Register(RuneId.Spark, RuneId.Mercury, SpellShape.Shot, SpellId.LightningBolt, "Lightning bolt", "Compressed. Catalog: Fire · Air · Mercury. Hunger given breath and sent.");
            Register(RuneId.Spark, RuneId.Salt, SpellShape.Self, SpellId.Blink, "Blink", "Compressed. Catalog: Air · Salt · Air · Spark. The hop given the seed. A short jump. Walls will not stop you.");
            Register(RuneId.Spark, RuneId.Lumen, SpellShape.Self, SpellId.Teleport, "Teleport", "Compressed. Catalog: Air · Salt · Air · Spark · Light. The spark-leap shown. Anywhere you can see.");
            Register(RuneId.Lightning, RuneId.Mercury, SpellShape.Shot, SpellId.LightningBolt, "Lightning bolt", "Compressed. Catalog: Lightning · Mercury when the bolt already stands.");
            Register(RuneId.Spark, RuneId.Salt, SpellShape.Spread, SpellId.LiveFloor, "Live-floor", "Charged ground around the caster.");
            Register(RuneId.Spark, RuneId.Sulphur, SpellShape.Remote, SpellId.Jolt, "Jolt", "Stun placed at a point.");
            Register(RuneId.Spark, RuneId.Lumen, SpellShape.Shot, SpellId.BrilliantArc, "Brilliant-arc", "Spark with Light riding it.");
            Register(RuneId.Spark, RuneId.Mors, SpellShape.Shot, SpellId.Blackout, "Blackout", "Death-work. The seed marked by the grave.");

            Register(RuneId.Earth, RuneId.Mercury, SpellShape.Shot, SpellId.DirtToss, "Dirt toss", "Compressed. Catalog: Earth · Mercury. Rest sent as loose dirt. It smothers ground-fire and leaves Earth speaking.");
            Register(RuneId.Stone, RuneId.Mercury, SpellShape.Shot, SpellId.HurledStone, "Hurled stone", "Compressed. Catalog: Earth · Salt · Mercury. Rest given a body and sent.");
            Register(RuneId.Earth, RuneId.Salt, SpellShape.Pillar, SpellId.EarthPillar, "Earth-pillar", "Compressed. Catalog: Earth · Salt. Rest given a body.");
            Register(RuneId.Earth, RuneId.Sulphur, SpellShape.Self, SpellId.Stoneskin, "Stoneskin", "Compressed. Catalog: Earth · Salt · Sulphur. Rest given a body, then the mind holds it on you.");
            Register(RuneId.Stone, RuneId.Salt, SpellShape.Pillar, SpellId.EarthPillar, "Earth-pillar", "Compressed. Catalog: Earth · Salt. Stone already stood.");
            Register(RuneId.Earth, RuneId.Sulphur, SpellShape.Remote, SpellId.Dread, "Dread", "Weight and fear, placed elsewhere.");
            Register(RuneId.Earth, RuneId.Mors, SpellShape.Spread, SpellId.GraveDust, "Grave-dust", "Death-work. Rest marked by the grave.");
            Register(RuneId.Earth, RuneId.Umbra, SpellShape.Remote, SpellId.ShadowWell, "Shadow-well", "A dark hollow opened at a point.");

            Register(RuneId.Air, RuneId.Mercury, SpellShape.Shot, SpellId.Gust, "Wind", "Compressed. Catalog: Air · Mercury. Breath sent. Wind.");
            Register(RuneId.Air, RuneId.Salt, SpellShape.Shot, SpellId.Push, "Push", "Compressed. Catalog: Air · Salt · Mercury. Breath given a body and sent. Wind that pushes.");
            Register(RuneId.Lightning, RuneId.Mercury, SpellShape.Remote, SpellId.LightningStrike, "Lightning strike", "Compressed. Catalog: Fire · Air · Salt · Air · Mercury. A spark given form from the air, falling from the sky.");
            Register(RuneId.Air, RuneId.Sulphur, SpellShape.Self, SpellId.Windward, "Wind ward", "Compressed. Catalog: Air · Salt · Sulphur. Breath given a body, then the mind holds it on you.");
            Register(RuneId.Air, RuneId.Salt, SpellShape.Self, SpellId.Hop, "Hop", "Compressed. Catalog: Air · Salt · Air. A leap.");
            Register(RuneId.Animus, RuneId.Mercury, SpellShape.Self, SpellId.Flight, "Flight", "Compressed. Catalog: Air · Salt · Air · Animus · Mercury. The hop given logos, then going.");
            Register(RuneId.Air, RuneId.Mercury, SpellShape.Self, SpellId.Float, "Float", "Compressed. Catalog: Air · Mercury · Salt. Breath going, then stood on you. You hang. Wind and vine move you.");
            Register(RuneId.Air, RuneId.Sulphur, SpellShape.Spread, SpellId.Daze, "Daze", "Mind of air around you.");
            Register(RuneId.Air, RuneId.Sulphur, SpellShape.Remote, SpellId.Confuse, "Confuse", "Mind of air, placed elsewhere. They lose the thread.");
            Register(RuneId.Air, RuneId.Lumen, SpellShape.Spread, SpellId.DayWake, "Day-wake", "Light blooming from the feet.");
            Register(RuneId.Air, RuneId.Umbra, SpellShape.Spread, SpellId.Gloom, "Gloom", "Dark air around you.");

            Register(RuneId.Steam, RuneId.Mercury, SpellShape.Shot, SpellId.Scald, "Scald", "Violent Fire+Water in motion.");
            Register(RuneId.Dust, RuneId.Mercury, SpellShape.Shot, SpellId.ScatterDust, "Scatter-dust", "Violent Air+Earth in motion.");
            Register(RuneId.Mud, RuneId.Vita, SpellShape.Spread, SpellId.Sprout, "Sprout", "Mud asked to live, from the feet.");
            Register(RuneId.Mud, RuneId.Mercury, SpellShape.Spread, SpellId.Swamp, "Swamp", "Compressed. Catalog: Earth · Water · Mercury · Salt. A watery swamp from the feet.");
            Register(RuneId.Mud, RuneId.Salt, SpellShape.Spread, SpellId.Quagmire, "Quagmire", "Compressed. Catalog: Earth · Water · Salt. Soft ground given a body.");
            Register(RuneId.Plant, RuneId.Mercury, SpellShape.Shot, SpellId.Vine, "Vine", "Compressed. Catalog: Plant · Mercury. The vegetable body sent. A climbing line, and a wick. A spell, not a rune.");
            Register(RuneId.Plant, RuneId.Vita, SpellShape.Remote, SpellId.Grow, "Grow", "Compressed. Catalog: Plant · Life · Mercury. The living plant sent. Sprout at a distance.");
            Register(RuneId.Plant, RuneId.Water, SpellShape.Remote, SpellId.Wolfsbane, "Wolfsbane", "Compressed. Catalog: Plant · Life · Water · Mercury. A living plant given yield and sent. A patch. Yield walks it. Poison turns it.");
            Register(RuneId.Plant, RuneId.Anima, SpellShape.Remote, SpellId.GroveCure, "Grove-cure", "Compressed. Catalog: Plant · Life · Anima · Mercury. A living plant opened to many and sent. Poison lifts.");
            Register(RuneId.Plant, RuneId.Salt, SpellShape.Remote, SpellId.Briar, "Briar", "Compressed. Catalog: Plant · Life · Salt · Mercury. A stood living plant sent. It holds them.");
            Register(RuneId.Plant, RuneId.Mors, SpellShape.Shot, SpellId.Hemlock, "Hemlock", "Compressed. Catalog: Plant · Life · Death · Mercury. A living plant, then the grave, sent.");
            Register(RuneId.Plant, RuneId.Mors, SpellShape.Pillar, SpellId.Nightshade, "Nightshade", "Compressed. Catalog: Plant · Life · Death · Salt. A living poison column. It weeps.");
            Register(RuneId.Plant, RuneId.Salt, SpellShape.Pillar, SpellId.Tree, "Tree", "Compressed. Catalog: Plant · Life · Salt. A living vegetable body given a standing body.");
            Register(RuneId.Plant, RuneId.Vita, SpellShape.Spread, SpellId.Sprout, "Sprout", "The vegetable body marked living, from the feet.");
            Register(RuneId.Plant, RuneId.Umbra, SpellShape.Spread, SpellId.Wither, "Wither", "Compressed. Catalog: Plant · Dark. The vegetable body withheld. Plants around the feet die. The remains speak Death.");
            Register(RuneId.Plant, RuneId.Sulphur, SpellShape.Self, SpellId.Plantward, "Plant ward", "Compressed. Catalog: Plant · Life · Salt · Sulphur. A living plant stood, then the mind holds it. Green springs as you walk.");
            Register(RuneId.Oil, RuneId.Mercury, SpellShape.Shot, SpellId.OilShot, "Oil shot", "Compressed. Catalog: Oil · Mercury. Fuel sent. Fire grows.");
            Register(RuneId.Oil, RuneId.Salt, SpellShape.Remote, SpellId.OilPuddle, "Oil puddle", "Compressed. Catalog: Oil · Salt. Fuel given a standing body. A puddle.");
            Register(RuneId.Oil, RuneId.Mercury, SpellShape.Remote, SpellId.OilGeyser, "Oil geyser", "Compressed. Catalog: Oil · Salt · Mercury. A stood fountain. Hunger that finds it will not leave.");
            Register(RuneId.Oil, RuneId.Salt, SpellShape.Spread, SpellId.OilSlick, "Oil slick", "Compressed. Catalog: Oil · Salt · Oil. Fuel given a body, then more fuel. It runs outward.");
            Register(RuneId.Oil, RuneId.Salt, SpellShape.Pillar, SpellId.OilPillar, "Oil-pillar", "Compressed. Catalog: Oil · Salt · Earth. A stood wick. A later fire sentence would make it a bomb.");
            Register(RuneId.Poison, RuneId.Mercury, SpellShape.Shot, SpellId.Poison, "Poison spray", "Compressed. Catalog: Poison · Mercury. A stream of the grave of a plant. It poisons what it crosses.");
            Register(RuneId.Poison, RuneId.Air, SpellShape.Shot, SpellId.Spore, "Spore", "Compressed. Catalog: Poison · Air · Mercury. The grave of a plant given breath and sent.");
            Register(RuneId.Poison, RuneId.Salt, SpellShape.Pillar, SpellId.TaintedTree, "Tainted-tree", "Compressed. Catalog: Poison · Salt · Earth. A poison column. It weeps onto adjacent tiles until it is destroyed.");
            Register(RuneId.Flame, RuneId.Sulphur, SpellShape.Self, SpellId.FlameForm, "Flame-form", "Compressed. Catalog: Flame · Salt · Sulphur. You become hunger's body. Hunger cannot take you.");
            Register(RuneId.Earth, RuneId.Anima, SpellShape.Self, SpellId.StoneForm, "Stone-form", "Compressed. Catalog: Earth · Anima · Earth · Salt · Sulphur. Rest given eros and itself again. You become rest.");
            Register(RuneId.Air, RuneId.Animus, SpellShape.Self, SpellId.GaleForm, "Gale-form", "Compressed. Catalog: Air · Animus · Air · Salt · Sulphur. You become invisible. Enemies lose your trail.");
            Register(RuneId.Cloud, RuneId.Animus, SpellShape.Self, SpellId.CloudForm, "Cloud-form", "Compressed. Catalog: Cloud · Animus · Anima · Cloud · Salt · Sulphur. You become mist and fly.");
            Register(RuneId.Cloud, RuneId.Anima, SpellShape.Self, SpellId.CloudForm, "Cloud-form", "Compressed. Catalog: Cloud · Animus · Anima · Cloud · Salt · Sulphur. You become mist and fly.");
            Register(RuneId.Plant, RuneId.Anima, SpellShape.Self, SpellId.GroveForm, "Grove-form", "Compressed. Catalog: Plant · Life · Anima · Plant · Life · Salt · Sulphur. You become the living plant. Green springs as you walk.");
            Register(RuneId.Miasma, RuneId.Salt, SpellShape.Spread, SpellId.Miasma, "Miasma", "Compressed. Catalog: Cloud · Acid. Foul breath given a body.");
            Register(RuneId.Plasma, RuneId.Mercury, SpellShape.Shot, SpellId.Plasma, "Plasma", "Witchfire joined to the bolt and sent. Ordinary matter ends.");
            Register(RuneId.Obsidian, RuneId.Salt, SpellShape.Pillar, SpellId.ObsidianWall, "Obsidian-wall", "Compressed. Catalog: Obsidian · Salt · Obsidian. Lava · Salt · Water · Salt · Lava · Salt · Water.");
            Register(RuneId.Water, RuneId.Anima, SpellShape.Self, SpellId.TideForm, "Tide-form", "Compressed. Catalog: Water · Anima · Water · Salt · Sulphur. Yield given eros and itself again. You become yield.");
            Register(RuneId.Vita, RuneId.Mercury, SpellShape.Remote, SpellId.Charm, "Charm", "Compressed. Catalog: Life · Sulphur · Mercury. A living mind is reached. They fetch, and they fight what you mark.");
            Register(RuneId.Cloud, RuneId.Acid, SpellShape.Remote, SpellId.AcidRain, "Acid rain", "Compressed. Catalog: Cloud · Acid · Mercury.");
            Register(RuneId.Cloud, RuneId.Metal, SpellShape.Remote, SpellId.MetalRain, "Metal rain", "Compressed. Catalog: Cloud · Metal · Mercury.");
            Register(RuneId.Cloud, RuneId.Lava, SpellShape.Remote, SpellId.LavaRain, "Lava rain", "Compressed. Catalog: Cloud · Lava · Mercury.");
            Register(RuneId.Cloud, RuneId.Fire, SpellShape.Remote, SpellId.EmberRain, "Ember rain", "Compressed. Catalog: Cloud · Fire · Mercury.");
            Register(RuneId.Cloud, RuneId.Spark, SpellShape.Remote, SpellId.SparkRain, "Spark rain", "Compressed. Catalog: Cloud · Spark · Mercury.");
            Register(RuneId.Cloud, RuneId.Oil, SpellShape.Remote, SpellId.OilRain, "Oil rain", "Compressed. Catalog: Cloud · Oil · Mercury.");
            Register(RuneId.Cloud, RuneId.Ash, SpellShape.Remote, SpellId.AshRain, "Ash rain", "Compressed. Catalog: Cloud · Ash · Mercury.");
            Register(RuneId.Cloud, RuneId.Plant, SpellShape.Remote, SpellId.PlantRain, "Plant rain", "Compressed. Catalog: Cloud · Plant · Mercury.");
            Register(RuneId.Cloud, RuneId.Mors, SpellShape.Remote, SpellId.DeathCloud, "Death-cloud", "Compressed. Catalog: Cloud · Dark · Death · Animus · Mercury.");
            Register(RuneId.Air, RuneId.Salt, SpellShape.Pillar, SpellId.AirWall, "Air-wall", "Compressed. Catalog: Air · Salt · Air · Mercury. A wall of air. They blow toward the far end.");
            Register(RuneId.Glacier, RuneId.Mercury, SpellShape.Shot, SpellId.Glacier, "Glacier", "Compressed. Catalog: Ice · Animus · Ice · Mercury.");
            Register(RuneId.Lumen, RuneId.Mercury, SpellShape.Self, SpellId.Cleanse, "Cleanse", "Compressed. Catalog: Light · Salt · Water · Mercury.");
            Register(RuneId.Lumen, RuneId.Vita, SpellShape.Pillar, SpellId.SunOrb, "Sun-orb", "Compressed. Catalog: Light · Life · Salt. Shown waking, given a body.");
            Register(RuneId.Lumen, RuneId.Anima, SpellShape.Pillar, SpellId.Sanctuary, "Sanctuary", "Compressed. Catalog: Light · Life · Anima · Salt. Shown waking, opened to many, given a body.");
            Register(RuneId.Vita, RuneId.Mors, SpellShape.Remote, SpellId.Turn, "Turn", "Compressed. Catalog: Life · Death · Sulphur · Mercury.");
            Register(RuneId.Mors, RuneId.Anima, SpellShape.Spread, SpellId.Animate, "Animate", "Compressed. Catalog: the four · Death · Anima · Mercury.");
            Register(RuneId.Mors, RuneId.Anima, SpellShape.Self, SpellId.DeathHost, "Death-host", "Compressed. Catalog: the four · Death · Anima · Sulphur.");
            Register(RuneId.Lumen, RuneId.Vita, SpellShape.Remote, SpellId.Exorcism, "Exorcism", "Compressed. Catalog: Light · Life · Mercury.");
            Register(RuneId.DarkCrystal, RuneId.Mors, SpellShape.Remote, SpellId.DarkCrystal, "Dark-crystal", "Compressed. Catalog: Crystal · Dark · Death.");

            Register(RuneId.Lava, RuneId.Salt, SpellShape.Pillar, SpellId.LavaPillar, "Lava-pillar", "Compressed. Catalog: Fire · Earth · Salt. Hungry earth given a standing body.");
            Register(RuneId.Metal, RuneId.Earth, SpellShape.Pillar, SpellId.MetalPillar, "Metal-pillar", "Compressed. Catalog: Metal · Salt · Earth. Lava · Spark · Earth asked to stand. It hangs without a far bank.");
            Register(RuneId.Stone, RuneId.Mercury, SpellShape.Remote, SpellId.Shatter, "Shatter", "Compressed. Catalog: Earth · Salt · Earth · Air · Mercury. A stood wall given breath and sent. Matter comes apart.");
        }

        static void Register(RuneId material, RuneId aspect, SpellShape shape, SpellId spell, string name, string effect)
        {
            var recipe = new SpellRecipe(material, aspect, shape, spell, name, effect);
            Recipes[(material, aspect, shape)] = recipe;
            Ordered.Add(recipe);
        }

        public static bool TryGet(RuneId material, RuneId aspect, SpellShape shape, out SpellRecipe recipe)
        {
            return Recipes.TryGetValue((material, aspect, shape), out recipe);
        }

        public static bool TryGetBySpell(SpellId spell, out SpellRecipe recipe)
        {
            foreach (var entry in Ordered)
            {
                if (entry.Spell == spell)
                {
                    recipe = entry;
                    return true;
                }
            }

            recipe = default;
            return false;
        }

        public static IEnumerable<SpellRecipe> All => Ordered;

        public static IEnumerable<SpellRecipe> OfType(RuneId material, RuneId aspect)
        {
            foreach (var recipe in Ordered)
            {
                var materialOk = material == RuneId.None || recipe.Material == material;
                var aspectOk = aspect == RuneId.None || recipe.Aspect == aspect;
                if (materialOk && aspectOk)
                {
                    yield return recipe;
                }
            }
        }

        public static string FormulaText(RuneId material, RuneId aspect, SpellShape shape = SpellShape.None)
        {
            var pair = $"{RuneCatalog.NameOf(material)} × {RuneCatalog.NameOf(aspect)}";
            return shape == SpellShape.None ? pair : $"{pair} · {SpellFormations.NameOf(shape)}";
        }

        public static string RecipeLine(SpellRecipe recipe)
        {
            if (MaterialTree.TryFindSources(recipe.Material, out var left, out var right))
            {
                return $"{RuneCatalog.NameOf(left)} + {RuneCatalog.NameOf(right)} → {FormulaText(recipe.Material, recipe.Aspect, recipe.Shape)}";
            }

            return FormulaText(recipe.Material, recipe.Aspect, recipe.Shape);
        }

        /// <summary>
        /// Grow / VineRise and the other leftover names may share a
        /// number. Two different spells must not.
        /// </summary>
        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            var allowed = new HashSet<string>
            {
                "Grow=VineRise", "VineRise=Grow",
                "Wither=Grotto", "Grotto=Wither",
                "DarkCrystal=GraveIce", "GraveIce=DarkCrystal"
            };

            var byValue = new Dictionary<int, string>();
            foreach (var name in System.Enum.GetNames(typeof(SpellId)))
            {
                var n = (int)System.Enum.Parse(typeof(SpellId), name);
                if (byValue.TryGetValue(n, out var other))
                {
                    if (!allowed.Contains(name + "=" + other))
                    {
                        broken.Add(other + " and " + name + " must not share a SpellId number");
                    }
                }
                else
                {
                    byValue[n] = name;
                }
            }
        }
    }
}
