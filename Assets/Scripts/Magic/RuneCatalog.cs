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
                new RuneDef(RuneId.Fire, RuneFamily.Material, "Fire", "F", "Hunger. The will to consume so it can continue."),
                new RuneDef(RuneId.Air, RuneFamily.Material, "Air", "A", "Breath. The between. That which has no weight and will not stay."),
                new RuneDef(RuneId.Earth, RuneFamily.Material, "Earth", "E", "Rest. Patience. That which remains when everything else has left."),
                new RuneDef(RuneId.Water, RuneFamily.Material, "Water", "W", "Yield. Mercy. That which becomes what holds it."),
                new RuneDef(RuneId.Spark, RuneFamily.Material, "Spark", "Sp", "Hunger given breath. Fire · Air. A seed of charge."),
                new RuneDef(RuneId.Cloud, RuneFamily.Material, "Cloud", "Cl", "Breath holding yield. Air · Water. A hanging veil."),
                new RuneDef(RuneId.Mud, RuneFamily.Material, "Mud", "Md", "Yield meeting rest. Water · Earth. Soft ground."),
                new RuneDef(RuneId.Lava, RuneFamily.Material, "Lava", "Lv", "Hunger meeting rest. Fire · Earth. Earth that cannot stay earth."),
                new RuneDef(RuneId.Steam, RuneFamily.Material, "Steam", "St", "Hunger forced through yield. Fire · Water. Water that cannot stay water."),
                new RuneDef(RuneId.Dust, RuneFamily.Material, "Dust", "Ds", "Breath forced through rest. Air · Earth. Rest that has lost its weight."),
                new RuneDef(RuneId.Storm, RuneFamily.Material, "Storm", "Sr", "The seed inside the hanging veil. Spark · Cloud."),
                new RuneDef(RuneId.Ice, RuneFamily.Material, "Ice", "Ic", "Yield given a body, then stilled. Water · Salt · Death."),
                new RuneDef(RuneId.Stone, RuneFamily.Material, "Stone", "Sn", "Rest given a body, then stilled. Earth · Salt · Death."),
                new RuneDef(RuneId.Glass, RuneFamily.Material, "Glass", "Gl", "Grains, a fire-body, then still. Sand · Flame · Death."),
                new RuneDef(RuneId.Sand, RuneFamily.Material, "Sand", "Sd", "Grit given a body. Dust · Salt. Also mud given breath until it dries."),
                new RuneDef(RuneId.Plant, RuneFamily.Material, "Plant", "Pl", "Yield and rest given a vegetable body. Water · Earth · Salt. Not living until Life."),
                new RuneDef(RuneId.Lightning, RuneFamily.Material, "Lightning", "Ln", "The seed stretched through more breath. Spark · Air. A path, not a body."),
                new RuneDef(RuneId.Inferno, RuneFamily.Material, "Inferno", "In", "A fire-body taught to travel and keep eating."),
                new RuneDef(RuneId.Plasma, RuneFamily.Material, "Plasma", "Pm", "Inferno joined to Spark. Reserved."),
                new RuneDef(RuneId.Rain, RuneFamily.Material, "Rain", "Rn", "The hanging veil drawn down. Cloud · Earth."),
                new RuneDef(RuneId.Snow, RuneFamily.Material, "Snow", "Sw", "The hanging veil given ice’s story. Cloud · Ice."),
                new RuneDef(RuneId.Blizzard, RuneFamily.Material, "Blizzard", "Bz", "Wind driving Snow."),
                new RuneDef(RuneId.Sandstorm, RuneFamily.Material, "Sandstorm", "Ss", "Wind driving Dust."),
                new RuneDef(RuneId.Obsidian, RuneFamily.Material, "Obsidian", "Ob", "Hungry earth quenched and given a body. Lava · Water · Salt."),
                new RuneDef(RuneId.Metal, RuneFamily.Material, "Metal", "Mt", "Hungry earth given more rest, then stilled."),
                new RuneDef(RuneId.Crystal, RuneFamily.Material, "Crystal", "Cr", "Stone grown with Water."),
                new RuneDef(RuneId.Glacier, RuneFamily.Material, "Glacier", "Gc", "Ice given Stone. Still water that will not thaw easily."),
                new RuneDef(RuneId.Acid, RuneFamily.Material, "Acid", "Ac", "Steam forced through Metal."),
                new RuneDef(RuneId.Vine, RuneFamily.Material, "Vine", "Vn", "Waking plant sent. Grove · Mercury."),
                new RuneDef(RuneId.Forest, RuneFamily.Material, "Forest", "Fr", "Waking plant asked to rest as a mass."),
                new RuneDef(RuneId.Blight, RuneFamily.Material, "Blight", "Bl", "Waking plant, then stilled. Grove · Death."),
                new RuneDef(RuneId.Ash, RuneFamily.Material, "Ash", "Ah", "A fire-body after the motion left. Flame · Death."),

                new RuneDef(RuneId.Salt, RuneFamily.Aspect, "Salt", "Sa", "A body. “This, here, is a thing.” Without it the work is weather or a quality."),
                new RuneDef(RuneId.Mercury, RuneFamily.Aspect, "Mercury", "Hg", "Going. A path opening. Through space if breath is in the chain; into a thing if not."),
                new RuneDef(RuneId.Sulphur, RuneFamily.Aspect, "Sulphur", "Su", "Passion. A mind that can be reached."),

                new RuneDef(RuneId.Aether, RuneFamily.Catalyst, "Aether", "Ae", "Prima materia. Inert alone. Union of Light and Dark aspects."),

                new RuneDef(RuneId.Vita, RuneFamily.Existential, "Life", "Vi", "Waking. The thing keeps being itself. Only if the subject or product is living."),
                new RuneDef(RuneId.Mors, RuneFamily.Existential, "Death", "Mo", "Still. The motion leaves the thing. Not moral."),
                new RuneDef(RuneId.Male, RuneFamily.Existential, "Male", "Ma", "Old name for projective polarity. Use Animus."),
                new RuneDef(RuneId.Female, RuneFamily.Existential, "Female", "Fe", "Old name for receptive polarity. Use Anima."),

                new RuneDef(RuneId.Hot, RuneFamily.PrimordialMundane, "Hot", "Ht", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Cold, RuneFamily.PrimordialMundane, "Cold", "Cd", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Wet, RuneFamily.PrimordialMundane, "Wet", "Wt", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Dry, RuneFamily.PrimordialMundane, "Dry", "Dr", "Mundane quality. Knowledge-gated; synthesizable."),

                new RuneDef(RuneId.Animus, RuneFamily.PrimordialDivine, "Animus", "As", "Projective soul. Drive, force, the going-out."),
                new RuneDef(RuneId.Anima, RuneFamily.PrimordialDivine, "Anima", "Aa", "Receptive soul. Draw, welcome, the taking-in."),
                new RuneDef(RuneId.Lumen, RuneFamily.PrimordialDivine, "Light", "Lu", "Shown. The veil is lifted."),
                new RuneDef(RuneId.Umbra, RuneFamily.PrimordialDivine, "Dark", "Um", "Withheld. The veil is drawn.")
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
        /// Operators, poles, and veils — the non-root concepts a compressed
        /// slice still treats as the last rune of a pair. Animus/Anima remain
        /// for reserved recipes. Aether, qualities, and Male/Female do not count.
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

        /// <summary>
        /// The eleven writeable concepts. Primordial runes are not in this list.
        /// </summary>
        public static readonly RuneId[] BasicRunes =
        {
            RuneId.Fire, RuneId.Air, RuneId.Earth, RuneId.Water,
            RuneId.Salt, RuneId.Mercury, RuneId.Sulphur,
            RuneId.Vita, RuneId.Mors,
            RuneId.Lumen, RuneId.Umbra
        };
    }
}
