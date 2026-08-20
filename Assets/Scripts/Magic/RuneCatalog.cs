namespace RuneMagic
{
    public enum RuneFamily
    {
        Material,
        Aspect,
        Catalyst,
        Existential,
        PrimordialMundane,
        PrimordialDivine
    }

    public enum RuneId
    {
        None = 0,

        Fire,
        Air,
        Earth,
        Water,
        Spark,
        Cloud,
        Mud,
        Lava,
        Steam,
        Dust,
        Storm,
        Ice,
        Stone,
        Glass,
        Sand,
        Plant,

        Salt,
        Mercury,
        Sulphur,

        Aether,

        Vita,
        Mors,
        Male,
        Female,

        Hot,
        Cold,
        Wet,
        Dry,

        Animus,
        Anima,
        Lumen,
        Umbra
    }

    public readonly struct RuneDef
    {
        public RuneDef(RuneId id, RuneFamily family, string name, string glyph, string meaning)
        {
            Id = id;
            Family = family;
            Name = name;
            Glyph = glyph;
            Meaning = meaning;
        }

        public RuneId Id { get; }
        public RuneFamily Family { get; }
        public string Name { get; }
        public string Glyph { get; }
        public string Meaning { get; }
    }

    /// <summary>
    /// All runes the design currently names. Perception is equal: names are not gated.
    /// Understanding (recipes, interpretations) lives in <see cref="Grimoire"/>.
    /// </summary>
    public static class RuneCatalog
    {
        static readonly System.Collections.Generic.Dictionary<RuneId, RuneDef> ById;

        static RuneCatalog()
        {
            var defs = new[]
            {
                new RuneDef(RuneId.Fire, RuneFamily.Material, "Fire", "F", "Hot and dry. The consuming element."),
                new RuneDef(RuneId.Air, RuneFamily.Material, "Air", "A", "Hot and wet. The mobile element."),
                new RuneDef(RuneId.Earth, RuneFamily.Material, "Earth", "E", "Cold and dry. The lasting element."),
                new RuneDef(RuneId.Water, RuneFamily.Material, "Water", "W", "Cold and wet. The yielding element."),
                new RuneDef(RuneId.Spark, RuneFamily.Material, "Spark", "Sp", "Fire joined to Air. A stable Hot blend."),
                new RuneDef(RuneId.Cloud, RuneFamily.Material, "Cloud", "Cl", "Air joined to Water. A stable Wet blend."),
                new RuneDef(RuneId.Mud, RuneFamily.Material, "Mud", "Md", "Water joined to Earth. A stable Cold blend."),
                new RuneDef(RuneId.Lava, RuneFamily.Material, "Lava", "Lv", "Fire joined to Earth. A stable Dry blend."),
                new RuneDef(RuneId.Steam, RuneFamily.Material, "Steam", "St", "Fire forced through Water. A violent blend."),
                new RuneDef(RuneId.Dust, RuneFamily.Material, "Dust", "Ds", "Air forced through Earth. A violent blend."),
                new RuneDef(RuneId.Storm, RuneFamily.Material, "Storm", "Sr", "Spark driven into Air or Water."),
                new RuneDef(RuneId.Ice, RuneFamily.Material, "Ice", "Ic", "Water drawn toward Cold."),
                new RuneDef(RuneId.Stone, RuneFamily.Material, "Stone", "Sn", "Lava cooled into permanence."),
                new RuneDef(RuneId.Glass, RuneFamily.Material, "Glass", "Gl", "Lava cooled into a brittle sheet."),
                new RuneDef(RuneId.Sand, RuneFamily.Material, "Sand", "Sd", "Mud dried by Air."),
                new RuneDef(RuneId.Plant, RuneFamily.Material, "Plant", "Pl", "Mud, Vita, and Aether. Soulless life."),

                new RuneDef(RuneId.Salt, RuneFamily.Aspect, "Salt", "Sa", "Body / matter. Solid, lasting, self-directed. Default: oneself."),
                new RuneDef(RuneId.Mercury, RuneFamily.Aspect, "Mercury", "Hg", "Motion / spirit. Projectiles, flow, weakening. Default: the enemy."),
                new RuneDef(RuneId.Sulphur, RuneFamily.Aspect, "Sulphur", "Su", "Mind / soul-mood. Fear, sleep, command. Default: something else."),

                new RuneDef(RuneId.Aether, RuneFamily.Catalyst, "Aether", "Ae", "Prima materia. Inert alone. Union of Light and Dark aspects."),

                new RuneDef(RuneId.Vita, RuneFamily.Existential, "Vita", "Vi", "Life. The animating pole."),
                new RuneDef(RuneId.Mors, RuneFamily.Existential, "Mors", "Mo", "Death. The stilling pole."),
                new RuneDef(RuneId.Male, RuneFamily.Existential, "Male", "Ma", "Projective polarity. Placeholder name."),
                new RuneDef(RuneId.Female, RuneFamily.Existential, "Female", "Fe", "Receptive polarity. Placeholder name."),

                new RuneDef(RuneId.Hot, RuneFamily.PrimordialMundane, "Hot", "Ht", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Cold, RuneFamily.PrimordialMundane, "Cold", "Cd", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Wet, RuneFamily.PrimordialMundane, "Wet", "Wt", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Dry, RuneFamily.PrimordialMundane, "Dry", "Dr", "Mundane quality. Knowledge-gated; synthesizable."),

                new RuneDef(RuneId.Animus, RuneFamily.PrimordialDivine, "Animus", "As", "Male soul. Divine primordial. Not craftable from the field."),
                new RuneDef(RuneId.Anima, RuneFamily.PrimordialDivine, "Anima", "Aa", "Female soul. Divine primordial. Not craftable from the field."),
                new RuneDef(RuneId.Lumen, RuneFamily.PrimordialDivine, "Lumen", "Lu", "Light. Divine primordial."),
                new RuneDef(RuneId.Umbra, RuneFamily.PrimordialDivine, "Umbra", "Um", "Dark. Divine primordial.")
            };

            ById = new System.Collections.Generic.Dictionary<RuneId, RuneDef>(defs.Length);
            foreach (var def in defs)
            {
                ById[def.Id] = def;
            }
        }

        public static RuneDef Get(RuneId id) => ById[id];

        public static bool IsMaterial(RuneId id) =>
            id != RuneId.None && ById.TryGetValue(id, out var def) && def.Family == RuneFamily.Material;

        public static bool IsAspect(RuneId id) =>
            id != RuneId.None && ById.TryGetValue(id, out var def) && def.Family == RuneFamily.Aspect;

        public static string NameOf(RuneId id) => id == RuneId.None ? "—" : Get(id).Name;

        public static string GlyphOf(RuneId id) => id == RuneId.None ? "?" : Get(id).Glyph;
    }
}
