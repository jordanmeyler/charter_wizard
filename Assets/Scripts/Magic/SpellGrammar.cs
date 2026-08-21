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
        LightningStrike
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
            Register(RuneId.Flame, RuneId.Mercury, SpellShape.Remote, SpellId.Melt, "Melt", "Compressed. Catalog: Fire · Salt · Mercury. A stood fire-body into a thing.");
            Register(RuneId.Fire, RuneId.Salt, SpellShape.Pillar, SpellId.FlamePillar, "Flame-pillar", "A standing column of fire.");
            Register(RuneId.Fire, RuneId.Sulphur, SpellShape.Self, SpellId.Flameward, "Flame ward", "Compressed. Catalog: Fire · Salt · Sulphur. Hunger given a body, then the mind holds it on you.");
            Register(RuneId.Fire, RuneId.Sulphur, SpellShape.Spread, SpellId.Frenzy, "Frenzy", "Heat in the thoughts, from the feet out.");
            Register(RuneId.Fire, RuneId.Mors, SpellShape.Remote, SpellId.Snuff, "Snuff", "Death-work. Hunger marked by the grave, placed on a flame.");
            Register(RuneId.Fire, RuneId.Lumen, SpellShape.Shot, SpellId.SunLance, "Sun-lance", "Light riding fire.");
            Register(RuneId.Fire, RuneId.Animus, SpellShape.Shot, SpellId.Drive, "Drive", "Projective fire. It goes out and does not return.");
            Register(RuneId.Fire, RuneId.Umbra, SpellShape.Remote, SpellId.Smother, "Smother", "Dark laid over a flame.");

            Register(RuneId.Water, RuneId.Mercury, SpellShape.Shot, SpellId.WaterJet, "Water-jet", "Water thrown as a line.");
            Register(RuneId.Water, RuneId.Salt, SpellShape.Pillar, SpellId.IcePillar, "Ice-pillar", "Yield given a body and asked to rest. Hard water. No Death.");
            Register(RuneId.Ice, RuneId.Sulphur, SpellShape.Remote, SpellId.Freeze, "Freeze", "Hard water held as a condition. They freeze.");
            Register(RuneId.Snow, RuneId.Air, SpellShape.Remote, SpellId.Snowstorm, "Snowstorm", "The veil given ice’s story, then driven. They freeze.");
            Register(RuneId.Blizzard, RuneId.Mercury, SpellShape.Remote, SpellId.Snowstorm, "Snowstorm", "Wind driving Snow, sent.");
            Register(RuneId.Water, RuneId.Sulphur, SpellShape.Self, SpellId.Watershield, "Water ward", "Compressed. Catalog: Water · Salt · Sulphur. Yield given a body, then the mind holds it on you.");
            Register(RuneId.Water, RuneId.Sulphur, SpellShape.Remote, SpellId.Lull, "Lull", "Mind of water. Sleep, placed elsewhere.");
            Register(RuneId.Water, RuneId.Vita, SpellShape.Spread, SpellId.Spring, "Spring", "Life welling from the feet.");
            Register(RuneId.Water, RuneId.Umbra, SpellShape.Spread, SpellId.Fog, "Fog", "Dark water as cover around you.");
            Register(RuneId.Water, RuneId.Anima, SpellShape.Remote, SpellId.Draw, "Draw", "Receptive pull. It calls, it does not strike.");

            Register(RuneId.Spark, RuneId.Mercury, SpellShape.Shot, SpellId.LightningBolt, "Lightning bolt", "Compressed. Catalog: Fire · Air · Mercury. Hunger given breath and sent.");
            Register(RuneId.Lightning, RuneId.Mercury, SpellShape.Shot, SpellId.LightningBolt, "Lightning bolt", "Compressed. Catalog: Lightning · Mercury when the bolt already stands.");
            Register(RuneId.Spark, RuneId.Salt, SpellShape.Spread, SpellId.LiveFloor, "Live-floor", "Charged ground around the caster.");
            Register(RuneId.Spark, RuneId.Sulphur, SpellShape.Remote, SpellId.Jolt, "Jolt", "Stun placed at a point.");
            Register(RuneId.Spark, RuneId.Lumen, SpellShape.Shot, SpellId.BrilliantArc, "Brilliant-arc", "Spark with Light riding it.");
            Register(RuneId.Spark, RuneId.Mors, SpellShape.Shot, SpellId.Blackout, "Blackout", "Death-work. The seed marked by the grave.");

            Register(RuneId.Earth, RuneId.Mercury, SpellShape.Shot, SpellId.HurledStone, "Hurled stone", "Earth given motion.");
            Register(RuneId.Earth, RuneId.Salt, SpellShape.Pillar, SpellId.EarthPillar, "Earth-pillar", "Compressed. Catalog: Earth · Salt. Rest given a body.");
            Register(RuneId.Earth, RuneId.Sulphur, SpellShape.Self, SpellId.Stoneskin, "Stoneskin", "Compressed. Catalog: Earth · Salt · Sulphur. Rest given a body, then the mind holds it on you.");
            Register(RuneId.Stone, RuneId.Salt, SpellShape.Pillar, SpellId.EarthPillar, "Earth-pillar", "Compressed. Catalog: Earth · Salt. Stone already stood.");
            Register(RuneId.Earth, RuneId.Sulphur, SpellShape.Remote, SpellId.Dread, "Dread", "Weight and fear, placed elsewhere.");
            Register(RuneId.Earth, RuneId.Mors, SpellShape.Spread, SpellId.GraveDust, "Grave-dust", "Death-work. Rest marked by the grave.");
            Register(RuneId.Earth, RuneId.Umbra, SpellShape.Remote, SpellId.ShadowWell, "Shadow-well", "A dark hollow opened at a point.");

            Register(RuneId.Air, RuneId.Mercury, SpellShape.Shot, SpellId.Gust, "Gust", "Compressed. Catalog: Air · Mercury. Breath sent.");
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
            Register(RuneId.Plant, RuneId.Vita, SpellShape.Pillar, SpellId.VineRise, "Vine-rise", "Living plant standing as a column.");
            Register(RuneId.Plant, RuneId.Anima, SpellShape.Remote, SpellId.CallGrowth, "Call-growth", "Plant invited at a distance.");

            Register(RuneId.Lava, RuneId.Salt, SpellShape.Pillar, SpellId.LavaPillar, "Lava-pillar", "Compressed. Catalog: Fire · Earth · Salt · Earth. Hungry earth given a body and asked to rest.");
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
