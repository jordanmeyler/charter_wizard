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
        Umbra,

        Flame,
        Grove,
        Wind,
        Current,
        Ember,
        Shade,
        Thunder,
        Oil,
        Miasma,
        Poison
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
                new RuneDef(RuneId.Mud, RuneFamily.Material, "Mud", "Md", "Rest meeting yield. Earth · Water. Soft ground. Order is the sentence — Water · Earth is Ice."),
                new RuneDef(RuneId.Lava, RuneFamily.Material, "Lava", "Lv", "Hunger meeting rest. Fire · Earth. Earth that cannot stay earth."),
                new RuneDef(RuneId.Steam, RuneFamily.Material, "Steam", "St", "Hunger forced through yield. Fire · Water. Water that cannot stay water."),
                new RuneDef(RuneId.Dust, RuneFamily.Material, "Dust", "Ds", "Breath forced through rest. Air · Earth. The same grit as sand."),
                new RuneDef(RuneId.Ice, RuneFamily.Material, "Ice", "Ic", "Yield meeting rest. Water · Earth. Hard water. It will thaw."),
                new RuneDef(RuneId.Stone, RuneFamily.Material, "Stone", "Sn", "Rest given a body. Earth · Salt."),
                new RuneDef(RuneId.Glass, RuneFamily.Material, "Glass", "Gl", "Grains, a fire-body, asked to rest. Dust · Flame · Earth."),
                new RuneDef(RuneId.Plant, RuneFamily.Material, "Plant", "Pl", "Yield given a body, then rest. Water · Salt · Earth. Fiber, a seed. Not living until Life."),
                new RuneDef(RuneId.Lightning, RuneFamily.Material, "Lightning", "Ln", "The seed stretched through more breath. Spark · Air. A path, not a body."),
                new RuneDef(RuneId.Plasma, RuneFamily.Material, "Plasma", "Pm", "Witchfire joined to the bolt. Flame · Lightning. Eats ordinary matter. Obsidian and warded stone refuse it."),
                new RuneDef(RuneId.Obsidian, RuneFamily.Material, "Obsidian", "Ob", "Hungry earth quenched and given a body. Melt and Shatter will not take it. Lava · Salt · Water."),
                new RuneDef(RuneId.Metal, RuneFamily.Material, "Metal", "Mt", "Hungry earth given spark, then stilled. Lava · Spark · Earth. Conducts heat and the spark."),
                new RuneDef(RuneId.Crystal, RuneFamily.Material, "Crystal", "Cr", "Stone grown with Water."),
                new RuneDef(RuneId.Glacier, RuneFamily.Material, "Glacier", "Gc", "Ice given logos and its own perpetuity. Ice · Animus · Ice. Ordinary fire cannot take it."),
                new RuneDef(RuneId.Acid, RuneFamily.Material, "Acid", "Ac", "Steam forced through Metal."),
                new RuneDef(RuneId.Ash, RuneFamily.Material, "Ash", "Ah", "What hunger leaves of a vegetable body. Fire · Plant."),
                new RuneDef(RuneId.Oil, RuneFamily.Material, "Oil", "Ol", "A vegetable body pressed with hunger and rest. Plant · Fire · Earth. It holds flame."),
                new RuneDef(RuneId.Miasma, RuneFamily.Material, "Miasma", "Mi", "The hanging veil forced through acid. Cloud · Acid. Foul breath."),
                new RuneDef(RuneId.Poison, RuneFamily.Material, "Poison", "Po", "The vegetable body, then the grave. Plant · Death."),

                new RuneDef(RuneId.Salt, RuneFamily.Aspect, "Salt", "Sa", "Body. A standing manifestation — walls, pillars, and the flesh of a creature."),
                new RuneDef(RuneId.Mercury, RuneFamily.Aspect, "Mercury", "Hg", "Soul / going. A path opening. The adept’s soul is always in the field."),
                new RuneDef(RuneId.Sulphur, RuneFamily.Aspect, "Sulphur", "Su", "Mind. Also the wildcard — add it and the sentence becomes something else."),

                new RuneDef(RuneId.Aether, RuneFamily.Catalyst, "Aether", "Ae", "Prima materia. Inert alone. Union of Light and Dark aspects."),

                new RuneDef(RuneId.Vita, RuneFamily.Existential, "Life", "Vi", "Marks a living recipe. Shown as written — Life is not unfolded. Soulless creatures carry it; the adept’s soul is Mercury instead."),
                new RuneDef(RuneId.Mors, RuneFamily.Existential, "Death", "Mo", "Modifier. The grave. Reserved for Free and arcane work. Not in ordinary recipes."),
                new RuneDef(RuneId.Hot, RuneFamily.PrimordialMundane, "Hot", "Ht", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Cold, RuneFamily.PrimordialMundane, "Cold", "Cd", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Wet, RuneFamily.PrimordialMundane, "Wet", "Wt", "Mundane quality. Knowledge-gated; synthesizable."),
                new RuneDef(RuneId.Dry, RuneFamily.PrimordialMundane, "Dry", "Dr", "Mundane quality. Knowledge-gated; synthesizable."),

                new RuneDef(RuneId.Lumen, RuneFamily.PrimordialDivine, "Light", "Lu", "Shown. The veil is lifted."),
                new RuneDef(RuneId.Umbra, RuneFamily.PrimordialDivine, "Dark", "Um", "Withheld. The veil is drawn."),

                new RuneDef(RuneId.Flame, RuneFamily.Material, "Flame", "Fl", "Witchfire. Fire given logos and its own perpetuity. Fire · Animus · Fire. Stronger than hunger."),
                new RuneDef(RuneId.Animus, RuneFamily.Material, "Animus", "As", "Logos. Assertiveness, decisiveness, challenge. Hunger given mind and breath. Fire · Sulphur · Air. Gives a work a magical quality — Flame and Glacier are fire and ice asserted that way."),
                new RuneDef(RuneId.Anima, RuneFamily.Material, "Anima", "Aa", "Eros. Receptivity, empathy, intuition, emotional connection. Yield given mind and rest. Water · Sulphur · Earth. Opens a work to many, and can make it healing."),
                new RuneDef(RuneId.Current, RuneFamily.Material, "Current", "Cu", "Yield going. Water · Mercury."),
                new RuneDef(RuneId.Ember, RuneFamily.Material, "Ember", "Em", "Hunger after the grave takes its motion. Fire · Death."),
                new RuneDef(RuneId.Shade, RuneFamily.Material, "Shade", "Sh", "Withheld, given a body, marked by the grave. Dark · Death · Salt.")
            };

            ById = new System.Collections.Generic.Dictionary<RuneId, RuneDef>(defs.Length);
            foreach (var def in defs)
            {
                ById[def.Id] = def;
            }
        }

        public static RuneDef Get(RuneId id) => ById[id];

        public static System.Collections.Generic.IReadOnlyCollection<RuneDef> All => ById.Values;

        public static bool TryGet(RuneId id, out RuneDef def) => ById.TryGetValue(id, out def);

        public static bool IsMaterial(RuneId id) =>
            id != RuneId.None && ById.TryGetValue(id, out var def) && def.Family == RuneFamily.Material;

        public static bool IsAspect(RuneId id) =>
            id != RuneId.None && ById.TryGetValue(id, out var def) && def.Family == RuneFamily.Aspect;

        /// <summary>
        /// Operators, poles, and veils — the non-root concepts a compressed
        /// slice still treats as the last rune of a pair. Animus/Anima are
        /// wrought eros/logos. Aether and the quality runes do not count.
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

        public static bool IsBasic(RuneId id)
        {
            for (var i = 0; i < BasicRunes.Length; i++)
            {
                if (BasicRunes[i] == id)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// A mark that can sit on the Charter wall and be strung into
        /// a recipe — the eleven, or a wrought join (Spark, Ice, Plant).
        /// </summary>
        public static bool OffersOnWall(RuneId id)
        {
            return IsBasic(id) || ChainBook.IsWrought(id);
        }

        /// <summary>
        /// Player-facing role for a non-root rune in a join. Salt is Body;
        /// Mercury is Spirit. Empty for elemental / material runes.
        /// </summary>
        public static string OperatorRole(RuneId id)
        {
            switch (id)
            {
                case RuneId.Salt: return "Body";
                case RuneId.Mercury: return "Spirit";
                case RuneId.Sulphur: return "Mind";
                case RuneId.Vita: return "Life";
                case RuneId.Mors: return "Death";
                case RuneId.Lumen: return "Light";
                case RuneId.Umbra: return "Dark";
                case RuneId.Animus: return "Logos";
                case RuneId.Anima: return "Eros";
                case RuneId.Hot: return "Hot";
                case RuneId.Cold: return "Cold";
                case RuneId.Wet: return "Wet";
                case RuneId.Dry: return "Dry";
                default: return string.Empty;
            }
        }

        public static string NameOf(RuneId id)
        {
            if (id == RuneId.None)
            {
                return "—";
            }

            return TryGet(id, out var def) ? def.Name : id.ToString();
        }

        public static string GlyphOf(RuneId id)
        {
            if (id == RuneId.None)
            {
                return "?";
            }

            return TryGet(id, out var def) ? def.Glyph : "?";
        }

        public static bool TryParseName(string name, out RuneId id)
        {
            id = RuneId.None;
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            name = name.Trim();
            foreach (var def in ById.Values)
            {
                if (string.Equals(def.Name, name, System.StringComparison.OrdinalIgnoreCase))
                {
                    id = def.Id;
                    return true;
                }
            }

            switch (name.ToLowerInvariant())
            {
                case "life":
                case "vita":
                    id = RuneId.Vita;
                    return true;
                case "death":
                case "mors":
                    id = RuneId.Mors;
                    return true;
                case "light":
                case "lumen":
                    id = RuneId.Lumen;
                    return true;
                case "dark":
                case "umbra":
                    id = RuneId.Umbra;
                    return true;
                case "grove":
                case "forest":
                    id = RuneId.Plant;
                    return true;
                case "sand":
                    id = RuneId.Dust;
                    return true;
                case "male":
                case "logos":
                    id = RuneId.Animus;
                    return true;
                case "female":
                case "eros":
                    id = RuneId.Anima;
                    return true;
                case "blight":
                    id = RuneId.Poison;
                    return true;
            }

            return System.Enum.TryParse(name, true, out id) && id != RuneId.None && TryGet(id, out _);
        }

        /// <summary>
        /// Every named rune, roots first, then the rest of the book.
        /// Inscriptions can carry any of these — not only the eleven basics.
        /// </summary>
        public static RuneId[] PlaceableRunes()
        {
            var seen = new System.Collections.Generic.HashSet<RuneId>();
            var list = new System.Collections.Generic.List<RuneId>(ById.Count);
            for (var i = 0; i < PlaceableLead.Length; i++)
            {
                var rune = PlaceableLead[i];
                if (TryGet(rune, out _) && seen.Add(rune))
                {
                    list.Add(rune);
                }
            }

            var rest = new System.Collections.Generic.List<RuneId>();
            foreach (var def in ById.Values)
            {
                if (seen.Add(def.Id))
                {
                    rest.Add(def.Id);
                }
            }

            rest.Sort((a, b) => string.Compare(NameOf(a), NameOf(b), System.StringComparison.OrdinalIgnoreCase));
            list.AddRange(rest);
            return list.ToArray();
        }

        /// <summary>
        /// Developer Grimoire pages. Roots and operators stay first; every
        /// named mark that is not in those lines is appended so a new join
        /// cannot fall off the book.
        /// </summary>
        public static System.Collections.Generic.IReadOnlyList<LedgerGroup> LedgerGroups()
        {
            var used = new System.Collections.Generic.HashSet<RuneId>();
            var groups = new System.Collections.Generic.List<LedgerGroup>(6);
            AddLedgerGroup(groups, used, "Roots",
                RuneId.Fire, RuneId.Air, RuneId.Earth, RuneId.Water);
            AddLedgerGroup(groups, used, "Body, spirit, mind",
                RuneId.Salt, RuneId.Mercury, RuneId.Sulphur);
            AddLedgerGroup(groups, used, "Life, death, veils",
                RuneId.Vita, RuneId.Mors, RuneId.Lumen, RuneId.Umbra);

            var wrought = new System.Collections.Generic.List<RuneId>();
            var births = ChainBook.AllBirths;
            for (var i = 0; i < births.Count; i++)
            {
                var rune = births[i].Rune;
                if (TryGet(rune, out _) && used.Add(rune))
                {
                    wrought.Add(rune);
                }
            }

            wrought.Sort((a, b) => string.Compare(NameOf(a), NameOf(b), System.StringComparison.OrdinalIgnoreCase));
            if (wrought.Count > 0)
            {
                groups.Add(new LedgerGroup("Wrought joins", wrought));
            }

            AddLedgerGroup(groups, used, "Reserved / later",
                RuneId.Hot, RuneId.Cold, RuneId.Wet, RuneId.Dry);

            var leftover = new System.Collections.Generic.List<RuneId>();
            foreach (var def in ById.Values)
            {
                if (used.Add(def.Id))
                {
                    leftover.Add(def.Id);
                }
            }

            leftover.Sort((a, b) => string.Compare(NameOf(a), NameOf(b), System.StringComparison.OrdinalIgnoreCase));
            if (leftover.Count > 0)
            {
                groups.Add(new LedgerGroup("Named runes", leftover));
            }

            return groups;
        }

        static void AddLedgerGroup(
            System.Collections.Generic.List<LedgerGroup> groups,
            System.Collections.Generic.HashSet<RuneId> used,
            string title,
            params RuneId[] runes)
        {
            var listed = new System.Collections.Generic.List<RuneId>(runes.Length);
            for (var i = 0; i < runes.Length; i++)
            {
                var rune = runes[i];
                if (rune != RuneId.None && TryGet(rune, out _) && used.Add(rune))
                {
                    listed.Add(rune);
                }
            }

            if (listed.Count > 0)
            {
                groups.Add(new LedgerGroup(title, listed));
            }
        }

        public static void AuditLedger(System.Collections.Generic.List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            var seen = new System.Collections.Generic.HashSet<RuneId>();
            foreach (var group in LedgerGroups())
            {
                for (var i = 0; i < group.Runes.Count; i++)
                {
                    var rune = group.Runes[i];
                    if (!TryGet(rune, out var def))
                    {
                        broken.Add($"developer ledger {group.Title} lists unnamed {rune}");
                        continue;
                    }

                    if (!seen.Add(rune))
                    {
                        broken.Add($"{def.Name} is listed twice in the developer ledger");
                    }
                }
            }

            foreach (var def in ById.Values)
            {
                if (!seen.Contains(def.Id))
                {
                    broken.Add($"{def.Name} is named but missing from the developer ledger");
                }
            }

            if (!IsBasic(RuneId.Fire) || !OffersOnWall(RuneId.Spark)
                || !OffersOnWall(RuneId.Ice) || !OffersOnWall(RuneId.Lightning)
                || !OffersOnWall(RuneId.Plant) || OffersOnWall(RuneId.Vine)
                || ElementalJoins.Length < 8 || ElementalJoins[0] != RuneId.Spark)
            {
                broken.Add("Spark and other wrought elementals must sit on the Charter wall; Vine is a spell");
            }
        }

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

        /// <summary>
        /// Wrought elementals that sit on the Develop Charter wall so
        /// Spark, Ice, Plant and the rest can be strung like the eleven.
        /// Play draws them from the weave when the room is speaking them.
        /// </summary>
        public static readonly RuneId[] ElementalJoins =
        {
            RuneId.Spark, RuneId.Lightning, RuneId.Flame, RuneId.Ember,
            RuneId.Steam, RuneId.Cloud, RuneId.Ice, RuneId.Mud,
            RuneId.Lava, RuneId.Dust, RuneId.Stone, RuneId.Plant,
            RuneId.Ash, RuneId.Oil, RuneId.Metal
        };

        static readonly RuneId[] PlaceableLead =
        {
            RuneId.Fire, RuneId.Air, RuneId.Earth, RuneId.Water,
            RuneId.Salt, RuneId.Mercury, RuneId.Sulphur,
            RuneId.Vita, RuneId.Mors, RuneId.Lumen, RuneId.Umbra,
            RuneId.Spark, RuneId.Lightning, RuneId.Flame, RuneId.Animus, RuneId.Anima, RuneId.Ember,
            RuneId.Cloud, RuneId.Steam,
            RuneId.Ice, RuneId.Glacier,
            RuneId.Plant, RuneId.Ash, RuneId.Oil,
            RuneId.Dust, RuneId.Mud, RuneId.Stone,
            RuneId.Lava, RuneId.Metal, RuneId.Obsidian, RuneId.Glass, RuneId.Crystal,
            RuneId.Acid, RuneId.Miasma, RuneId.Poison, RuneId.Plasma,
            RuneId.Current, RuneId.Shade, RuneId.Aether
        };
    }

    public readonly struct LedgerGroup
    {
        public LedgerGroup(string title, System.Collections.Generic.IReadOnlyList<RuneId> runes)
        {
            Title = title ?? string.Empty;
            Runes = runes ?? System.Array.Empty<RuneId>();
        }

        public string Title { get; }
        public System.Collections.Generic.IReadOnlyList<RuneId> Runes { get; }
    }
}
