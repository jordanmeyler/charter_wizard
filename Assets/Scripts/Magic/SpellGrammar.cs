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
        OilSlick
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
            Register(RuneId.Fire, RuneId.Salt, SpellShape.Pillar, SpellId.FlamePillar, "Flame-pillar", "A standing column of fire.");
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
            Register(RuneId.Air, RuneId.Mercury, SpellShape.Self, SpellId.Flight, "Flight", "Compressed. Catalog: Air · Mercury · Salt. You fly.");
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
            Register(RuneId.Plant, RuneId.Mercury, SpellShape.Pillar, SpellId.VineRise, "Vine-rise", "The sent plant asked to stand.");
            Register(RuneId.Plant, RuneId.Salt, SpellShape.Pillar, SpellId.Tree, "Tree", "Compressed. Catalog: Plant · Life · Salt. A living vegetable body given a standing body.");
            Register(RuneId.Plant, RuneId.Vita, SpellShape.Spread, SpellId.Sprout, "Sprout", "The vegetable body marked living, from the feet.");
            Register(RuneId.Plant, RuneId.Umbra, SpellShape.Remote, SpellId.Grotto, "Grotto", "Compressed. Catalog: Plant · Dark. The vegetable body withheld. A damp cave opens. Not a rune.");
            Register(RuneId.Oil, RuneId.Mercury, SpellShape.Shot, SpellId.OilShot, "Oil shot", "Compressed. Catalog: Oil · Mercury. Fuel sent. Fire grows.");
            Register(RuneId.Oil, RuneId.Salt, SpellShape.Remote, SpellId.OilPuddle, "Oil puddle", "Compressed. Catalog: Oil · Salt. Fuel given a standing body. A puddle.");
            Register(RuneId.Oil, RuneId.Mercury, SpellShape.Remote, SpellId.OilGeyser, "Oil geyser", "Compressed. Catalog: Oil · Salt · Mercury. A stood fountain. Hunger that finds it will not leave.");
            Register(RuneId.Oil, RuneId.Salt, SpellShape.Spread, SpellId.OilSlick, "Oil slick", "Compressed. Catalog: Oil · Salt · Oil. Fuel given a body, then more fuel. It runs outward.");
            Register(RuneId.Oil, RuneId.Salt, SpellShape.Pillar, SpellId.OilPillar, "Oil-pillar", "Compressed. Catalog: Oil · Salt · Earth. A stood wick. A later fire sentence would make it a bomb.");
            Register(RuneId.Poison, RuneId.Mercury, SpellShape.Shot, SpellId.Poison, "Poison", "Compressed. Catalog: Plant · Death · Mercury. The grave of a plant, sent.");
            Register(RuneId.Miasma, RuneId.Salt, SpellShape.Spread, SpellId.Miasma, "Miasma", "Compressed. Catalog: Cloud · Acid. Foul breath given a body.");
            Register(RuneId.Plasma, RuneId.Mercury, SpellShape.Shot, SpellId.Plasma, "Plasma", "Witchfire joined to the bolt and sent. Ordinary matter ends.");
            Register(RuneId.Obsidian, RuneId.Salt, SpellShape.Pillar, SpellId.ObsidianWall, "Obsidian-wall", "Compressed. Catalog: Obsidian · Salt · Obsidian. Lava · Salt · Water · Salt · Lava · Salt · Water.");
            Register(RuneId.Plant, RuneId.Anima, SpellShape.Remote, SpellId.CallGrowth, "Call-growth", "Plant invited at a distance. Two wet steps.");
            Register(RuneId.Vita, RuneId.Mercury, SpellShape.Remote, SpellId.Charm, "Charm", "Compressed. Catalog: Life · Sulphur · Mercury. A living mind is reached. They fetch, and they fight what you mark.");

            Register(RuneId.Lava, RuneId.Salt, SpellShape.Pillar, SpellId.LavaPillar, "Lava-pillar", "Compressed. Catalog: Fire · Earth · Salt · Earth. Hungry earth given a body and asked to rest.");
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
    }
}
