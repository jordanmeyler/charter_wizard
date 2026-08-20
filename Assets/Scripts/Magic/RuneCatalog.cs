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
        Lightning,
        Inferno,
        Plasma,
        Rain,
        Snow,
        Blizzard,
        Sandstorm,
        Obsidian,
        Metal,
        Crystal,
        Glacier,
        Acid,
        Vine,
        Forest,
        Blight,
        Ash,

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
                new RuneDef(RuneId.Storm, RuneFamily.Material, "Storm", "Sr", "Spark joined to Cloud. A charged thundercloud."),
                new RuneDef(RuneId.Ice, RuneFamily.Material, "Ice", "Ic", "Water given body and Cold. Fixed solid."),
                new RuneDef(RuneId.Stone, RuneFamily.Material, "Stone", "Sn", "Earth given body. Lasting mass."),
                new RuneDef(RuneId.Glass, RuneFamily.Material, "Glass", "Gl", "Sand melted by Fire. Brittle and clear."),
                new RuneDef(RuneId.Sand, RuneFamily.Material, "Sand", "Sd", "Mud dried by Air."),
                new RuneDef(RuneId.Plant, RuneFamily.Material, "Plant", "Pl", "Water, Earth, and Salt. Wet earth given a body."),
                new RuneDef(RuneId.Lightning, RuneFamily.Material, "Lightning", "Ln", "Spark driven into Air. Arcing charge."),
                new RuneDef(RuneId.Inferno, RuneFamily.Material, "Inferno", "In", "Fire given Sulphur. All-consuming."),
                new RuneDef(RuneId.Plasma, RuneFamily.Material, "Plasma", "Pm", "Inferno joined to Spark. The hot ceiling."),
                new RuneDef(RuneId.Rain, RuneFamily.Material, "Rain", "Rn", "Cloud given more Water. Douses and wets."),
                new RuneDef(RuneId.Snow, RuneFamily.Material, "Snow", "Sw", "Cloud joined to Ice. Soft cold."),
                new RuneDef(RuneId.Blizzard, RuneFamily.Material, "Blizzard", "Bz", "Gale driving Snow."),
                new RuneDef(RuneId.Sandstorm, RuneFamily.Material, "Sandstorm", "Ss", "Gale driving Dust."),
                new RuneDef(RuneId.Obsidian, RuneFamily.Material, "Obsidian", "Ob", "Lava quenched by Water."),
                new RuneDef(RuneId.Metal, RuneFamily.Material, "Metal", "Mt", "Lava smelted with Earth."),
                new RuneDef(RuneId.Crystal, RuneFamily.Material, "Crystal", "Cr", "Stone grown with Water."),
                new RuneDef(RuneId.Glacier, RuneFamily.Material, "Glacier", "Gc", "Ice given Stone. Permanent cold."),
                new RuneDef(RuneId.Acid, RuneFamily.Material, "Acid", "Ac", "Steam forced through Metal."),
                new RuneDef(RuneId.Vine, RuneFamily.Material, "Vine", "Vn", "Plant asked to climb and bind."),
                new RuneDef(RuneId.Forest, RuneFamily.Material, "Forest", "Fr", "Plant, Water, and Life. Dense cover."),
                new RuneDef(RuneId.Blight, RuneFamily.Material, "Blight", "Bl", "Plant given Death. Rot."),
                new RuneDef(RuneId.Ash, RuneFamily.Material, "Ash", "Ah", "What Fire leaves of Plant."),

                new RuneDef(RuneId.Salt, RuneFamily.Aspect, "Salt", "Sa", "Body / matter. Lasting, still. Not a formation — it asks for a standing shape."),
                new RuneDef(RuneId.Mercury, RuneFamily.Aspect, "Mercury", "Hg", "Motion / spirit. Flow and flight. Not a formation — it asks to travel."),
                new RuneDef(RuneId.Sulphur, RuneFamily.Aspect, "Sulphur", "Su", "Mind / soul-mood. Eruption, fear, sleep, command."),

                new RuneDef(RuneId.Aether, RuneFamily.Catalyst, "Aether", "Ae", "Prima materia. Inert alone. Union of Light and Dark aspects."),

                new RuneDef(RuneId.Vita, RuneFamily.Existential, "Life", "Vi", "Vita. The animating pole. Required for anything that grows."),
                new RuneDef(RuneId.Mors, RuneFamily.Existential, "Death", "Mo", "Mors. The stilling pole. Snuffs, stills, and ends."),
                new RuneDef(RuneId.Male, RuneFamily.Existential, "Male", "Ma", "Old name for projective polarity. Use Animus."),
                new RuneDef(RuneId.Female, RuneFamily.Existential, "Female", "Fe", "Old name for receptive polarity. Use Anima."),

                new RuneDef(RuneId.Hot, RuneFamily.PrimordialMundane, "Hot", "Ht", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Cold, RuneFamily.PrimordialMundane, "Cold", "Cd", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Wet, RuneFamily.PrimordialMundane, "Wet", "Wt", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Dry, RuneFamily.PrimordialMundane, "Dry", "Dr", "Mundane quality. Knowledge-gated; synthesizable."),

                new RuneDef(RuneId.Animus, RuneFamily.PrimordialDivine, "Animus", "As", "Projective soul. Drive, force, the going-out."),
                new RuneDef(RuneId.Anima, RuneFamily.PrimordialDivine, "Anima", "Aa", "Receptive soul. Draw, welcome, the taking-in."),
                new RuneDef(RuneId.Lumen, RuneFamily.PrimordialDivine, "Light", "Lu", "Lumen. Sol, projective light."),
                new RuneDef(RuneId.Umbra, RuneFamily.PrimordialDivine, "Dark", "Um", "Umbra. Luna, receptive dark.")
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

        /// <summary>
        /// A spell needs a non-elemental aspect. Elements and blends are never enough.
        /// Tria prima, Life/Death, Light/Dark, and Animus/Anima all count.
        /// Aether, qualities, and the old Male/Female labels do not.
        /// </summary>
        public static bool IsFormAspect(RuneId id)
        {
            switch (id)
            {
                case RuneId.Salt:
                case RuneId.Mercury:
                case RuneId.Sulphur:
                case RuneId.Vita:
                case RuneId.Mors:
                case RuneId.Animus:
                case RuneId.Anima:
                case RuneId.Lumen:
                case RuneId.Umbra:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsElemental(RuneId id) => IsMaterial(id);

        public static string NameOf(RuneId id) => id == RuneId.None ? "—" : Get(id).Name;

        public static string GlyphOf(RuneId id) => id == RuneId.None ? "?" : Get(id).Glyph;
    }
}
