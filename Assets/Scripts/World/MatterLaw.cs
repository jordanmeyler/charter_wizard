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
            if (spell == SpellId.None)
            {
                return Heat.None;
            }

            return SpellCodex.TryGet(spell, out var entry)
                ? HeatOf(entry.RecipeRunes)
                : Heat.None;
        }

        public static Heat HeatOf(IReadOnlyList<RuneId> recipe)
        {
            if (recipe == null || recipe.Count == 0)
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
                default:
                    return MaterialId.None;
            }
        }

        public static bool CanMelt(MaterialId material, Heat heat)
        {
            var need = MeltHeat(material);
            return need != Heat.None && heat >= need;
        }

        public static bool Melts(SpellId spell, MaterialId material) =>
            CanMelt(material, HeatOf(spell));

        public static MaterialId MatterOf(IReadOnlyList<RuneId> formula)
        {
            if (formula == null || formula.Count == 0)
            {
                return MaterialId.None;
            }

            var hasWater = false;
            var hasSalt = false;
            var hasEarth = false;
            var hasStone = false;
            for (var i = 0; i < formula.Count; i++)
            {
                var rune = formula[i];
                if (rune == RuneId.Glacier)
                {
                    return MaterialId.Glacier;
                }

                if (rune == RuneId.Glass)
                {
                    return MaterialId.Glass;
                }

                if (rune == RuneId.Ice || rune == RuneId.Snow)
                {
                    return rune == RuneId.Snow ? MaterialId.Snow : MaterialId.Ice;
                }

                hasWater |= rune == RuneId.Water;
                hasSalt |= rune == RuneId.Salt;
                hasEarth |= rune == RuneId.Earth;
                hasStone |= rune == RuneId.Stone;
            }

            if (hasWater && hasSalt && hasEarth)
            {
                return hasStone ? MaterialId.Glacier : MaterialId.Ice;
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
                default:
                    return false;
            }
        }

        public static string MeltNote(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Glacier:
                    return "Witchfire finds the stone-ice. What ordinary hunger could not take, yields.";
                case MaterialId.Glass:
                    return "Witchfire remembers the grains. The glass forgets it was liquid.";
                case MaterialId.Snow:
                    return "Hunger finds the snow. It remembers yield.";
                default:
                    return "Hunger finds the ice. It remembers yield.";
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
                || HeatOf(SpellId.FlamePillar) != Heat.Fire)
            {
                broken.Add("Ordinary fire-work must carry Fire heat");
            }

            if (HeatOf(SpellId.Witchfire) != Heat.Flame)
            {
                broken.Add("Witchfire must carry Flame heat");
            }

            if (HeatOf(SpellId.Rage) != Heat.None
                || HeatOf(SpellId.Frenzy) != Heat.None
                || HeatOf(SpellId.Gust) != Heat.None)
            {
                broken.Add("Mind-fire and breath must not count as heat");
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
                || flame[1] != RuneId.Sulphur
                || flame[2] != RuneId.Fire)
            {
                broken.Add("Flame must be born Fire · Sulphur · Fire");
            }

            if (!SpellCodex.TryGet(SpellId.Witchfire, out var witch)
                || !ChainBook.SameStory(witch.RecipeRunes, ChainBook.Parse("Fire · Sulphur · Fire · Mercury"))
                || !ChainBook.SameStory(witch.ViaRunes, ChainBook.Parse("Flame · Mercury")))
            {
                broken.Add("Witchfire must be Fire · Sulphur · Fire · Mercury, via Flame · Mercury");
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
        }

        static bool IsWitchfireChain(IReadOnlyList<RuneId> recipe)
        {
            for (var i = 0; i + 2 < recipe.Count; i++)
            {
                if (recipe[i] == RuneId.Fire
                    && recipe[i + 1] == RuneId.Sulphur
                    && recipe[i + 2] == RuneId.Fire)
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
