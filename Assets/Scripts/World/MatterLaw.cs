using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// How hot a finished sentence is, and what that heat can take.
    /// Heat lives on the recipe, not a spell-name list. Ice always
    /// yields to fire. Witchfire (Flame) takes what ordinary hunger cannot.
    /// </summary>
    public enum Heat
    {
        None = 0,
        Fire = 1,
        Flame = 2,
        Inferno = 3
    }

    public static class MatterLaw
    {
        public static Heat HeatOf(SpellId spell)
        {
            if (spell == SpellId.None || spell == SpellId.LiveFloor)
            {
                return Heat.None;
            }

            if (!SpellCodex.TryGet(spell, out var entry))
            {
                return Heat.None;
            }

            // Unfolded Oil recipes still contain Fire. The via mark is
            // the fuel rune; without it, standing a wick would be hunger.
            if (entry.ViaRunes.Count == 0)
            {
                return HeatOf(entry.RecipeRunes);
            }

            var marks = new List<RuneId>(entry.RecipeRunes.Count + entry.ViaRunes.Count);
            for (var i = 0; i < entry.RecipeRunes.Count; i++)
            {
                marks.Add(entry.RecipeRunes[i]);
            }

            for (var i = 0; i < entry.ViaRunes.Count; i++)
            {
                marks.Add(entry.ViaRunes[i]);
            }

            return HeatOf(marks);
        }

        public static Heat HeatOf(IReadOnlyList<RuneId> recipe)
        {
            if (recipe == null || recipe.Count == 0)
            {
                return Heat.None;
            }

            if (Has(recipe, RuneId.Oil)
                && !Has(recipe, RuneId.Plasma)
                && !Has(recipe, RuneId.Flame)
                && !Has(recipe, RuneId.Inferno)
                && !Has(recipe, RuneId.Explosion))
            {
                return Heat.None;
            }

            if (Has(recipe, RuneId.Inferno) || Has(recipe, RuneId.Plasma))
            {
                return Heat.Inferno;
            }

            if (Has(recipe, RuneId.Flame) || IsWitchfireChain(recipe))
            {
                return Heat.Flame;
            }

            if (Has(recipe, RuneId.Explosion))
            {
                return Heat.Fire;
            }

            // Animus is logos, not hunger. Ice given will stays cold even
            // when the Animus birth unfolds through Fire.
            if (IsLogosIce(recipe))
            {
                return Heat.None;
            }

            if (!Has(recipe, RuneId.Fire) && !Has(recipe, RuneId.Lava) && !Has(recipe, RuneId.Ember))
            {
                return Heat.None;
            }

            return IsMindFire(recipe) ? Heat.None : Heat.Fire;
        }

        public static Heat MeltHeat(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Ice:
                case MaterialId.Snow:
                    return Heat.Fire;
                case MaterialId.Glacier:
                case MaterialId.Glass:
                    return Heat.Flame;
                default:
                    return Heat.None;
            }
        }

        public static MaterialId MeltsTo(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Ice:
                case MaterialId.Snow:
                case MaterialId.Glacier:
                    return MaterialId.Damp;
                case MaterialId.Glass:
                    return MaterialId.Sand;
                case MaterialId.Stone:
                case MaterialId.SaltCrust:
                case MaterialId.Scoured:
                case MaterialId.Damp:
                case MaterialId.Metal:
                    return MaterialId.Scoured;
                default:
                    return MaterialId.None;
            }
        }

        public static bool CanMelt(MaterialId material, Heat heat)
        {
            if (ResistsMagic(material))
            {
                return false;
            }

            var need = MeltHeat(material);
            return need != Heat.None && heat >= need;
        }

        /// <summary>
        /// Melt is a lasting fire-body sent into a thing — Fire · Salt ·
        /// Earth · Mercury. It bores stone and steel, including room
        /// masonry. Fire · Salt · Mercury is still a fire-pillar.
        /// Fireball still only thaws ice.
        /// </summary>
        public static bool IsMeltWork(SpellId spell) =>
            spell == SpellId.Melt;

        public static bool IsBoreable(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Stone:
                case MaterialId.SaltCrust:
                case MaterialId.Scoured:
                case MaterialId.Damp:
                case MaterialId.Metal:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Quenched lava. It will not take Melt, Shatter, hunger's thaw,
        /// or water eating rest. Use it when a wall must stay a wall.
        /// </summary>
        public static bool ResistsMagic(MaterialId material) =>
            material == MaterialId.Obsidian
            || material == MaterialId.Wardstone
            || material == MaterialId.Aegis;

        public static bool IsPlasmaWork(SpellId spell) =>
            spell == SpellId.Plasma;

        /// <summary>
        /// Unmake and Atomic take warded glass. Plasma still refuses it.
        /// </summary>
        public static bool BreaksWard(SpellId spell) =>
            spell == SpellId.Unmake || spell == SpellId.Atomic;

        public static bool IsAnnihilable(MaterialId material)
        {
            if (material == MaterialId.None || material == MaterialId.Void || ResistsMagic(material))
            {
                return false;
            }

            return true;
        }

        public static bool Melts(SpellId spell, MaterialId material)
        {
            if (ResistsMagic(material))
            {
                return false;
            }

            if (CanMelt(material, HeatOf(spell)))
            {
                return true;
            }

            return IsMeltWork(spell) && IsBoreable(material);
        }

        public static MaterialId MatterOf(IReadOnlyList<RuneId> formula)
        {
            if (formula == null || formula.Count == 0)
            {
                return MaterialId.None;
            }

            var hasWater = false;
            var hasSalt = false;
            var hasEarth = false;
            var hasIce = false;
            var hasVita = false;
            var hasPlant = false;
            for (var i = 0; i < formula.Count; i++)
            {
                var rune = formula[i];
                if (rune == RuneId.Obsidian)
                {
                    return MaterialId.Obsidian;
                }

                if (rune == RuneId.Glacier)
                {
                    return MaterialId.Glacier;
                }

                if (rune == RuneId.Glass)
                {
                    return MaterialId.Glass;
                }

                if (rune == RuneId.Metal)
                {
                    return MaterialId.Metal;
                }

                if (rune == RuneId.Stone)
                {
                    return MaterialId.Stone;
                }

                if (rune == RuneId.Ice)
                {
                    hasIce = true;
                }

                if (rune == RuneId.Mud)
                {
                    return MaterialId.Mud;
                }

                if (rune == RuneId.Plant)
                {
                    hasPlant = true;
                }

                hasWater |= rune == RuneId.Water;
                hasSalt |= rune == RuneId.Salt;
                hasEarth |= rune == RuneId.Earth;
                hasVita |= rune == RuneId.Vita;
            }

            if (IsLogosIce(formula) && !hasSalt && !hasPlant)
            {
                return MaterialId.Glacier;
            }

            if (hasIce)
            {
                return MaterialId.Ice;
            }

            if (hasWater && hasEarth && !hasSalt && !hasPlant)
            {
                for (var i = 0; i < formula.Count; i++)
                {
                    if (formula[i] == RuneId.Earth)
                    {
                        return MaterialId.Mud;
                    }

                    if (formula[i] == RuneId.Water)
                    {
                        return MaterialId.Ice;
                    }
                }

                return MaterialId.Ice;
            }

            if (hasEarth && hasSalt)
            {
                return MaterialId.Stone;
            }

            return MaterialId.None;
        }

        public static bool TryParse(string matter, out MaterialId material)
        {
            material = MaterialId.None;
            if (string.IsNullOrEmpty(matter))
            {
                return false;
            }

            switch (matter.Trim().ToLowerInvariant())
            {
                case "ice":
                    material = MaterialId.Ice;
                    return true;
                case "snow":
                    material = MaterialId.Snow;
                    return true;
                case "glacier":
                    material = MaterialId.Glacier;
                    return true;
                case "glass":
                    material = MaterialId.Glass;
                    return true;
                case "stone":
                case "rock":
                    material = MaterialId.Stone;
                    return true;
                case "metal":
                case "steel":
                case "iron":
                    material = MaterialId.Metal;
                    return true;
                case "water":
                    material = MaterialId.Water;
                    return true;
                case "wood":
                case "timber":
                    material = MaterialId.Timber;
                    return true;
                case "plant":
                case "vine":
                    material = MaterialId.Plant;
                    return true;
                case "oil":
                    material = MaterialId.Oil;
                    return true;
                case "obsidian":
                    material = MaterialId.Obsidian;
                    return true;
                default:
                    return System.Enum.TryParse(matter.Trim(), true, out material)
                        && material != MaterialId.None;
            }
        }

        public static string MeltNote(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Glacier:
                    return "Witchfire finds ice given logos. What ordinary hunger could not take, yields.";
                case MaterialId.Glass:
                    return "Witchfire remembers the grains. The glass forgets it was liquid.";
                case MaterialId.Snow:
                    return "Hunger finds the snow. It remembers yield.";
                case MaterialId.Metal:
                    return "The stood fire-body finds the iron. Steel remembers it was liquid.";
                case MaterialId.Stone:
                case MaterialId.SaltCrust:
                case MaterialId.Scoured:
                case MaterialId.Damp:
                    return "The stood fire-body finds the stone. Rest remembers it was liquid.";
                default:
                    return "Hunger finds the ice. Water stands on the floor that was under it.";
            }
        }

        public static string ResistNote(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Obsidian:
                    return "The black glass will not take the work. Obsidian was already quenched.";
                case MaterialId.Wardstone:
                    return "The mind-bound stone will not take the work.";
                case MaterialId.Aegis:
                    return "The shown steel will not take the work.";
                default:
                    return "This will not take the work.";
            }
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (HeatOf(SpellId.Fireball) != Heat.Fire
                || HeatOf(SpellId.Melt) != Heat.Fire
                || HeatOf(SpellId.Thaw) != Heat.Fire
                || HeatOf(SpellId.Ignite) != Heat.Fire
                || HeatOf(SpellId.FlamePillar) != Heat.Fire
                || HeatOf(SpellId.FirePillar) != Heat.Fire
                || HeatOf(SpellId.LavaPillar) != Heat.Fire)
            {
                broken.Add("Ordinary fire-work must carry Fire heat");
            }

            if (HeatOf(SpellId.Witchfire) != Heat.Flame)
            {
                broken.Add("Witchfire must carry Flame heat");
            }

            if (HeatOf(SpellId.Plasma) != Heat.Inferno)
            {
                broken.Add("Plasma must carry Inferno heat");
            }

            if (HeatOf(SpellId.Explosion) != Heat.Fire
                || HeatOf(SpellId.Atomic) != Heat.Inferno)
            {
                broken.Add("Explosion is hunger on fuel; Atomic is that blast joined to plasma");
            }

            if (!BreaksWard(SpellId.Unmake)
                || !BreaksWard(SpellId.Atomic)
                || BreaksWard(SpellId.Plasma)
                || BreaksWard(SpellId.Explosion)
                || BreaksWard(SpellId.Melt))
            {
                broken.Add("Unmake and Atomic take warded stone; Plasma, Explosion, and Melt still refuse it");
            }

            if (HeatOf(SpellId.Rage) != Heat.None
                || HeatOf(SpellId.Frenzy) != Heat.None
                || HeatOf(SpellId.Gust) != Heat.None
                || HeatOf(SpellId.OilShot) != Heat.None
                || HeatOf(SpellId.OilPillar) != Heat.None
                || HeatOf(SpellId.OilPuddle) != Heat.None
                || HeatOf(SpellId.OilGeyser) != Heat.None
                || HeatOf(SpellId.OilSlick) != Heat.None
                || HeatOf(SpellId.LiveFloor) != Heat.None)
            {
                broken.Add("Mind-fire, breath, oil fuel, and live-floor must not count as heat");
            }

            if (!Melts(SpellId.Fireball, MaterialId.Ice)
                || !Melts(SpellId.Thaw, MaterialId.Ice)
                || !Melts(SpellId.Melt, MaterialId.Snow)
                || Melts(SpellId.Fireball, MaterialId.Glacier)
                || Melts(SpellId.Fireball, MaterialId.Glass)
                || Melts(SpellId.Rage, MaterialId.Ice)
                || Melts(SpellId.Gust, MaterialId.Ice))
            {
                broken.Add("Ordinary fire must melt ice and snow, and must not melt glacier or glass");
            }

            if (!Melts(SpellId.Witchfire, MaterialId.Ice)
                || !Melts(SpellId.Witchfire, MaterialId.Glacier)
                || !Melts(SpellId.Witchfire, MaterialId.Glass))
            {
                broken.Add("Witchfire must melt ice, glacier, and glass");
            }

            if (!Melts(SpellId.Melt, MaterialId.Stone)
                || !Melts(SpellId.Melt, MaterialId.Metal)
                || Melts(SpellId.Fireball, MaterialId.Stone)
                || Melts(SpellId.Witchfire, MaterialId.Stone)
                || Melts(SpellId.Melt, MaterialId.Obsidian)
                || Melts(SpellId.Fireball, MaterialId.Obsidian)
                || !ResistsMagic(MaterialId.Obsidian)
                || !ResistsMagic(MaterialId.Wardstone)
                || !ResistsMagic(MaterialId.Aegis)
                || ResistsMagic(MaterialId.Stone))
            {
                broken.Add("Melt must bore stone and steel; fireball must not; obsidian and warded matter must refuse the work");
            }

            if (!IsAnnihilable(MaterialId.Stone)
                || !IsAnnihilable(MaterialId.Ice)
                || IsAnnihilable(MaterialId.Obsidian)
                || IsAnnihilable(MaterialId.Wardstone)
                || IsAnnihilable(MaterialId.Aegis))
            {
                broken.Add("Plasma must eat ordinary matter and spare obsidian, wardstone, and aegis");
            }

            if (!WorldWork.IsFireWork(SpellId.Fireball)
                || !WorldWork.IsFireWork(SpellId.Witchfire)
                || !WorldWork.IsFireWork(SpellId.Thaw)
                || WorldWork.IsFireWork(SpellId.Rage)
                || WorldWork.IsFireWork(SpellId.Gust))
            {
                broken.Add("IsFireWork must follow heat, not a spell-name list");
            }

            if (!ChainBook.TryBirth(RuneId.Flame, out var flame)
                || flame.Count != 3
                || flame[0] != RuneId.Fire
                || flame[1] != RuneId.Animus
                || flame[2] != RuneId.Fire)
            {
                broken.Add("Flame must be born Fire · Animus · Fire");
            }

            if (!ChainBook.TryBirth(RuneId.Glacier, out var glacier)
                || glacier.Count != 3
                || glacier[0] != RuneId.Ice
                || glacier[1] != RuneId.Animus
                || glacier[2] != RuneId.Ice)
            {
                broken.Add("Glacier must be born Ice · Animus · Ice");
            }

            if (HeatOf(ChainBook.Parse("Ice · Animus · Ice")) != Heat.None
                || HeatOf(ChainBook.Parse("Water · Earth · Fire · Sulphur · Air · Water · Earth")) != Heat.None)
            {
                broken.Add("Ice given logos must not count as heat");
            }

            if (HeatOf(ChainBook.Parse("Fire · Animus · Fire")) != Heat.Flame)
            {
                broken.Add("Fire given logos and its own perpetuity must carry Flame heat");
            }

            if (MatterOf(ChainBook.Parse("Ice · Animus · Ice")) != MaterialId.Glacier
                || MatterOf(ChainBook.Parse("Ice")) != MaterialId.Ice
                || MatterOf(ChainBook.Parse("Ice · Stone")) == MaterialId.Glacier)
            {
                broken.Add("Glacier is Ice · Animus · Ice, not Ice · Stone");
            }

            if (!ChainBook.TryBirth(RuneId.Plasma, out var plasma)
                || plasma.Count != 2
                || plasma[0] != RuneId.Flame
                || plasma[1] != RuneId.Lightning)
            {
                broken.Add("Plasma must be born Flame · Lightning");
            }

            if (!ChainBook.TryBirth(RuneId.Anima, out var anima)
                || anima.Count != 3
                || anima[0] != RuneId.Water
                || anima[1] != RuneId.Sulphur
                || anima[2] != RuneId.Earth)
            {
                broken.Add("Anima must be born Water · Sulphur · Earth");
            }

            if (!ChainBook.TryBirth(RuneId.Animus, out var animus)
                || animus.Count != 3
                || animus[0] != RuneId.Fire
                || animus[1] != RuneId.Sulphur
                || animus[2] != RuneId.Air)
            {
                broken.Add("Animus must be born Fire · Sulphur · Air");
            }

            if (HeatOf(SpellId.Balm) != Heat.None || HeatOf(SpellId.Chorus) != Heat.None)
            {
                broken.Add("Anima-work must not count as heat");
            }

            if (HeatOf(SpellId.Drive) != Heat.Fire)
            {
                broken.Add("Drive must carry Fire heat");
            }

            if (!SpellCodex.TryGet(SpellId.Witchfire, out var witch)
                || !ChainBook.SameStory(witch.RecipeRunes, ChainBook.Parse("Fire · Animus · Fire · Mercury"))
                || !ChainBook.SameStory(witch.ViaRunes, ChainBook.Parse("Flame · Mercury")))
            {
                broken.Add("Witchfire must be Fire · Animus · Fire · Mercury, via Flame · Mercury");
            }

            if (SpellCodex.TryGet(SpellId.FlamePillar, out var pillar)
                && pillar.ViaRunes.Count > 0)
            {
                broken.Add("Flame-pillar must not take the Flame via — that join is witchfire now");
            }

            if (SpellCodex.TryGet(SpellId.Melt, out var melt)
                && melt.ViaRunes.Count > 0)
            {
                broken.Add("Melt must not take the Flame via — Flame · Mercury is witchfire");
            }

            if (SpellCodex.TryGet(SpellId.Melt, out melt)
                && !ChainBook.SameStory(melt.RecipeRunes, ChainBook.Parse("Fire · Salt · Earth · Mercury")))
            {
                broken.Add("Melt must be Fire · Salt · Earth · Mercury — a lasting fire-body sent into stone");
            }

            if (SpellCodex.TryGet(SpellId.FirePillar, out var firePillar)
                && !ChainBook.SameStory(firePillar.ViaRunes, ChainBook.Parse("Fire · Salt · Mercury")))
            {
                broken.Add("Fire · Salt · Mercury must stay Fire-pillar");
            }

            if (!TryParse("wood", out var wood) || wood != MaterialId.Timber
                || !TryParse("water", out var water) || water != MaterialId.Water
                || !TryParse("plant", out var plant) || plant != MaterialId.Plant
                || !TryParse("vine", out var vine) || vine != MaterialId.Plant)
            {
                broken.Add("Wood, water, plant, and vine matter names must parse");
            }
        }

        static bool IsWitchfireChain(IReadOnlyList<RuneId> recipe)
        {
            for (var i = 0; i + 2 < recipe.Count; i++)
            {
                if (recipe[i] == RuneId.Fire
                    && recipe[i + 1] == RuneId.Animus
                    && recipe[i + 2] == RuneId.Fire)
                {
                    return true;
                }
            }

            for (var i = 0; i + 4 < recipe.Count; i++)
            {
                if (recipe[i] == RuneId.Fire
                    && recipe[i + 1] == RuneId.Fire
                    && recipe[i + 2] == RuneId.Sulphur
                    && recipe[i + 3] == RuneId.Air
                    && recipe[i + 4] == RuneId.Fire)
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsLogosIce(IReadOnlyList<RuneId> recipe)
        {
            if (Has(recipe, RuneId.Glacier))
            {
                return true;
            }

            if (Has(recipe, RuneId.Ice) && Has(recipe, RuneId.Animus))
            {
                return true;
            }

            for (var i = 0; i + 6 < recipe.Count; i++)
            {
                if (recipe[i] == RuneId.Water
                    && recipe[i + 1] == RuneId.Earth
                    && recipe[i + 2] == RuneId.Fire
                    && recipe[i + 3] == RuneId.Sulphur
                    && recipe[i + 4] == RuneId.Air
                    && recipe[i + 5] == RuneId.Water
                    && recipe[i + 6] == RuneId.Earth)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Fire of the mind: Sulphur turns hunger toward a thought.
        /// Fire · Mercury alone is a fireball — that still heats the floor.
        /// </summary>
        static bool IsMindFire(IReadOnlyList<RuneId> recipe)
        {
            var sulphur = false;
            var mercury = false;
            var body = false;
            for (var i = 0; i < recipe.Count; i++)
            {
                switch (recipe[i])
                {
                    case RuneId.Sulphur:
                        sulphur = true;
                        break;
                    case RuneId.Mercury:
                        mercury = true;
                        break;
                    case RuneId.Salt:
                    case RuneId.Earth:
                    case RuneId.Water:
                    case RuneId.Lumen:
                    case RuneId.Air:
                    case RuneId.Flame:
                    case RuneId.Lava:
                    case RuneId.Ember:
                    case RuneId.Ice:
                    case RuneId.Steam:
                    case RuneId.Inferno:
                        body = true;
                        break;
                }
            }

            return sulphur && mercury && !body;
        }

        static bool Has(IReadOnlyList<RuneId> recipe, RuneId rune)
        {
            for (var i = 0; i < recipe.Count; i++)
            {
                if (recipe[i] == rune)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
