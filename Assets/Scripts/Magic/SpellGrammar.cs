using System.Collections.Generic;

namespace RuneMagic
{
    public enum SpellId
    {
        None = 0,
        Fireball,
        FlameWall,
        Frenzy,
        WaterJet,
        IceWall,
        Lull,
        LightningBolt,
        LiveFloor,
        Jolt,
        HurledStone,
        StoneWall,
        Dread,
        Gale,
        StillAir,
        Daze,
        Scald,
        ScatterDust
    }

    public readonly struct SpellRecipe
    {
        public SpellRecipe(RuneId material, RuneId aspect, SpellId spell, string name, string effect)
        {
            Material = material;
            Aspect = aspect;
            Spell = spell;
            Name = name;
            Effect = effect;
        }

        public RuneId Material { get; }
        public RuneId Aspect { get; }
        public SpellId Spell { get; }
        public string Name { get; }
        public string Effect { get; }
        public (RuneId Material, RuneId Aspect) Key => (Material, Aspect);
    }

    /// <summary>
    /// Material (noun) × Aspect (verb) = spell. Orthogonal axes from the design reference.
    /// </summary>
    public static class SpellGrammar
    {
        static readonly Dictionary<(RuneId, RuneId), SpellRecipe> Recipes = new();

        static SpellGrammar()
        {
            Register(RuneId.Fire, RuneId.Mercury, SpellId.Fireball, "Fireball", "Motion of fire. A jet or bolt.");
            Register(RuneId.Fire, RuneId.Salt, SpellId.FlameWall, "Flame-wall", "Body of fire. Lasting terrain.");
            Register(RuneId.Fire, RuneId.Sulphur, SpellId.Frenzy, "Frenzy", "Mind of fire. Heat in the thoughts.");

            Register(RuneId.Water, RuneId.Mercury, SpellId.WaterJet, "Water-jet", "Motion of water. Wave or jet.");
            Register(RuneId.Water, RuneId.Salt, SpellId.IceWall, "Ice-wall", "Body of water. Solid, lasting.");
            Register(RuneId.Water, RuneId.Sulphur, SpellId.Lull, "Lull", "Mind of water. Sleep.");

            Register(RuneId.Spark, RuneId.Mercury, SpellId.LightningBolt, "Lightning bolt", "Motion of spark.");
            Register(RuneId.Spark, RuneId.Salt, SpellId.LiveFloor, "Live-floor", "Body of spark. Charged ground.");
            Register(RuneId.Spark, RuneId.Sulphur, SpellId.Jolt, "Jolt", "Mind of spark. Stun.");

            Register(RuneId.Earth, RuneId.Mercury, SpellId.HurledStone, "Hurled stone", "Motion of earth.");
            Register(RuneId.Earth, RuneId.Salt, SpellId.StoneWall, "Stone wall", "Body of earth.");
            Register(RuneId.Earth, RuneId.Sulphur, SpellId.Dread, "Dread", "Mind of earth. Weight and fear.");

            Register(RuneId.Air, RuneId.Mercury, SpellId.Gale, "Gale", "Motion of air. Provisional.");
            Register(RuneId.Air, RuneId.Salt, SpellId.StillAir, "Still-air", "Body of air. Provisional.");
            Register(RuneId.Air, RuneId.Sulphur, SpellId.Daze, "Daze", "Mind of air. Provisional.");

            Register(RuneId.Steam, RuneId.Mercury, SpellId.Scald, "Scald", "Violent Fire+Water in motion.");
            Register(RuneId.Dust, RuneId.Mercury, SpellId.ScatterDust, "Scatter-dust", "Violent Air+Earth in motion.");
        }

        static void Register(RuneId material, RuneId aspect, SpellId spell, string name, string effect)
        {
            Recipes[(material, aspect)] = new SpellRecipe(material, aspect, spell, name, effect);
        }

        public static bool TryGet(RuneId material, RuneId aspect, out SpellRecipe recipe)
        {
            return Recipes.TryGetValue((material, aspect), out recipe);
        }

        public static IEnumerable<SpellRecipe> All => Recipes.Values;

        public static string FormulaText(RuneId material, RuneId aspect)
        {
            return $"{RuneCatalog.NameOf(material)} × {RuneCatalog.NameOf(aspect)}";
        }

        public static string RecipeLine(SpellRecipe recipe)
        {
            if (MaterialTree.TryFindSources(recipe.Material, out var left, out var right))
            {
                return $"{RuneCatalog.NameOf(left)} + {RuneCatalog.NameOf(right)} → {FormulaText(recipe.Material, recipe.Aspect)}";
            }

            return FormulaText(recipe.Material, recipe.Aspect);
        }
    }
}
