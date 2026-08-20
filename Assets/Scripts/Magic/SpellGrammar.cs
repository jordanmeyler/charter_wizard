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
        CallGrowth
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
    /// Material × Aspect × Formation = a written spell. Sparse on purpose.
    /// A sensible-looking combo that is not written here fizzles under Charter.
    /// </summary>
    public static class SpellGrammar
    {
        static readonly Dictionary<(RuneId, RuneId, SpellShape), SpellRecipe> Recipes = new();
        static readonly List<SpellRecipe> Ordered = new();

        static SpellGrammar()
        {
            Register(RuneId.Fire, RuneId.Mercury, SpellShape.Shot, SpellId.Fireball, "Fireball", "Fire thrown along a line.");
            Register(RuneId.Fire, RuneId.Salt, SpellShape.Pillar, SpellId.FlamePillar, "Flame-pillar", "A standing column of fire.");
            Register(RuneId.Fire, RuneId.Sulphur, SpellShape.Spread, SpellId.Frenzy, "Frenzy", "Heat in the thoughts, from the feet out.");
            Register(RuneId.Fire, RuneId.Mors, SpellShape.Remote, SpellId.Snuff, "Snuff", "Death of a flame, placed at a point.");
            Register(RuneId.Fire, RuneId.Lumen, SpellShape.Shot, SpellId.SunLance, "Sun-lance", "Light riding fire.");
            Register(RuneId.Fire, RuneId.Animus, SpellShape.Shot, SpellId.Drive, "Drive", "Projective fire. It goes out and does not return.");
            Register(RuneId.Fire, RuneId.Umbra, SpellShape.Remote, SpellId.Smother, "Smother", "Dark laid over a flame.");

            Register(RuneId.Water, RuneId.Mercury, SpellShape.Shot, SpellId.WaterJet, "Water-jet", "Water thrown as a line.");
            Register(RuneId.Water, RuneId.Mors, SpellShape.Pillar, SpellId.IcePillar, "Ice-pillar", "Water given body, then stilled. Standing ice.");
            Register(RuneId.Water, RuneId.Sulphur, SpellShape.Remote, SpellId.Lull, "Lull", "Mind of water. Sleep, placed elsewhere.");
            Register(RuneId.Water, RuneId.Vita, SpellShape.Spread, SpellId.Spring, "Spring", "Life welling from the feet.");
            Register(RuneId.Water, RuneId.Umbra, SpellShape.Spread, SpellId.Fog, "Fog", "Dark water as cover around you.");
            Register(RuneId.Water, RuneId.Anima, SpellShape.Remote, SpellId.Draw, "Draw", "Receptive pull. It calls, it does not strike.");

            Register(RuneId.Spark, RuneId.Mercury, SpellShape.Shot, SpellId.LightningBolt, "Lightning bolt", "Spark thrown as a line.");
            Register(RuneId.Spark, RuneId.Salt, SpellShape.Spread, SpellId.LiveFloor, "Live-floor", "Charged ground around the caster.");
            Register(RuneId.Spark, RuneId.Sulphur, SpellShape.Remote, SpellId.Jolt, "Jolt", "Stun placed at a point.");
            Register(RuneId.Spark, RuneId.Lumen, SpellShape.Shot, SpellId.BrilliantArc, "Brilliant-arc", "Spark with Light riding it.");
            Register(RuneId.Spark, RuneId.Mors, SpellShape.Shot, SpellId.Blackout, "Blackout", "Death of a spark, thrown.");

            Register(RuneId.Earth, RuneId.Mercury, SpellShape.Shot, SpellId.HurledStone, "Hurled stone", "Earth given motion.");
            Register(RuneId.Earth, RuneId.Salt, SpellShape.Pillar, SpellId.StonePillar, "Stone pillar", "Earth standing.");
            Register(RuneId.Earth, RuneId.Salt, SpellShape.Remote, SpellId.RaisedEarth, "Raised earth", "A body of earth called up away from you.");
            Register(RuneId.Earth, RuneId.Sulphur, SpellShape.Remote, SpellId.Dread, "Dread", "Weight and fear, placed elsewhere.");
            Register(RuneId.Earth, RuneId.Vita, SpellShape.Pillar, SpellId.Menhir, "Menhir", "Earth asked to live as a standing stone.");
            Register(RuneId.Earth, RuneId.Mors, SpellShape.Spread, SpellId.GraveDust, "Grave-dust", "Death of earth, from the feet out.");
            Register(RuneId.Earth, RuneId.Umbra, SpellShape.Remote, SpellId.ShadowWell, "Shadow-well", "A dark hollow opened at a point.");

            Register(RuneId.Air, RuneId.Mercury, SpellShape.Shot, SpellId.Gale, "Gale", "Air thrown as a line.");
            Register(RuneId.Air, RuneId.Sulphur, SpellShape.Spread, SpellId.Daze, "Daze", "Mind of air around you.");
            Register(RuneId.Air, RuneId.Lumen, SpellShape.Spread, SpellId.DayWake, "Day-wake", "Light blooming from the feet.");
            Register(RuneId.Air, RuneId.Umbra, SpellShape.Spread, SpellId.Gloom, "Gloom", "Dark air around you.");

            Register(RuneId.Steam, RuneId.Mercury, SpellShape.Shot, SpellId.Scald, "Scald", "Violent Fire+Water in motion.");
            Register(RuneId.Dust, RuneId.Mercury, SpellShape.Shot, SpellId.ScatterDust, "Scatter-dust", "Violent Air+Earth in motion.");
            Register(RuneId.Mud, RuneId.Vita, SpellShape.Spread, SpellId.Sprout, "Sprout", "Mud asked to live, from the feet.");
            Register(RuneId.Plant, RuneId.Vita, SpellShape.Pillar, SpellId.VineRise, "Vine-rise", "Living plant standing as a column.");
            Register(RuneId.Plant, RuneId.Anima, SpellShape.Remote, SpellId.CallGrowth, "Call-growth", "Plant invited at a distance.");
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
