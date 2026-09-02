using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// A form rewrites your nature as an element. The sentence is
    /// the element, the stance that shares its roots, the element
    /// again, then Salt · Sulphur — the same hold as a ward.
    /// Animus is Fire · Sulphur · Air, so it stands with Fire and Air.
    /// Anima is Water · Sulphur · Earth, so it stands with Water and Earth.
    /// A join that carries both sides asks for both stances, in the
    /// order those roots first appear. Plant also keeps Life, because
    /// a vegetable body is not living until Life.
    /// </summary>
    public static class FormLaw
    {
        public static RuneId StanceOf(RuneId root)
        {
            switch (root)
            {
                case RuneId.Fire:
                case RuneId.Air:
                    return RuneId.Animus;
                case RuneId.Water:
                case RuneId.Earth:
                    return RuneId.Anima;
                default:
                    return RuneId.None;
            }
        }

        public static List<RuneId> StancesOf(RuneId element)
        {
            var stances = new List<RuneId>(2);
            var roots = new List<RuneId>(4);
            CollectRoots(element, roots, new HashSet<RuneId>());
            for (var i = 0; i < roots.Count; i++)
            {
                var stance = StanceOf(roots[i]);
                if (stance != RuneId.None && !stances.Contains(stance))
                {
                    stances.Add(stance);
                }
            }

            return stances;
        }

        public static List<RuneId> BodyOf(RuneId element)
        {
            var body = new List<RuneId>(2) { element };
            if (element == RuneId.Plant)
            {
                body.Add(RuneId.Vita);
            }

            return body;
        }

        public static List<RuneId> RecipeOf(RuneId element)
        {
            var recipe = new List<RuneId>(8);
            var body = BodyOf(element);
            recipe.AddRange(body);
            recipe.AddRange(StancesOf(element));
            recipe.AddRange(body);
            recipe.Add(RuneId.Salt);
            recipe.Add(RuneId.Sulphur);
            return recipe;
        }

        static void CollectRoots(RuneId rune, List<RuneId> roots, HashSet<RuneId> seen)
        {
            if (rune == RuneId.None || !seen.Add(rune))
            {
                return;
            }

            if (StanceOf(rune) != RuneId.None)
            {
                roots.Add(rune);
                return;
            }

            if (IsOperator(rune) || !ChainBook.TryBirth(rune, out var sources))
            {
                return;
            }

            for (var i = 0; i < sources.Count; i++)
            {
                CollectRoots(sources[i], roots, seen);
            }
        }

        static bool IsOperator(RuneId rune)
        {
            switch (rune)
            {
                case RuneId.Salt:
                case RuneId.Mercury:
                case RuneId.Sulphur:
                case RuneId.Vita:
                case RuneId.Mors:
                case RuneId.Lumen:
                case RuneId.Umbra:
                case RuneId.Anima:
                case RuneId.Animus:
                    return true;
                default:
                    return false;
            }
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (StanceOf(RuneId.Fire) != RuneId.Animus
                || StanceOf(RuneId.Air) != RuneId.Animus
                || StanceOf(RuneId.Water) != RuneId.Anima
                || StanceOf(RuneId.Earth) != RuneId.Anima
                || StanceOf(RuneId.Plant) != RuneId.None)
            {
                broken.Add("Animus shares Fire and Air; Anima shares Water and Earth");
            }

            if (!OneStance(RuneId.Fire, RuneId.Animus)
                || !OneStance(RuneId.Air, RuneId.Animus)
                || !OneStance(RuneId.Water, RuneId.Anima)
                || !OneStance(RuneId.Earth, RuneId.Anima)
                || !OneStance(RuneId.Plant, RuneId.Anima)
                || !OneStance(RuneId.Ice, RuneId.Anima)
                || !OneStance(RuneId.Spark, RuneId.Animus)
                || !OneStance(RuneId.Mud, RuneId.Anima))
            {
                broken.Add("A root form uses only the stance that shares its symbol");
            }

            if (!BothStances(RuneId.Lava, RuneId.Animus, RuneId.Anima)
                || !BothStances(RuneId.Steam, RuneId.Animus, RuneId.Anima)
                || !BothStances(RuneId.Cloud, RuneId.Animus, RuneId.Anima)
                || !BothStances(RuneId.Dust, RuneId.Animus, RuneId.Anima)
                || !BothStances(RuneId.Oil, RuneId.Anima, RuneId.Animus))
            {
                broken.Add("A mixed join must ask for both Anima and Animus, in root order");
            }

            if (!Matches(SpellId.FlameForm, RuneId.Fire)
                || !Matches(SpellId.TideForm, RuneId.Water)
                || !Matches(SpellId.StoneForm, RuneId.Earth)
                || !Matches(SpellId.GaleForm, RuneId.Air)
                || !Matches(SpellId.GroveForm, RuneId.Plant)
                || !Matches(SpellId.CloudForm, RuneId.Cloud))
            {
                broken.Add("Each written form must be Element · matching stance · Element · Salt · Sulphur");
            }
        }

        static bool OneStance(RuneId element, RuneId stance)
        {
            var stances = StancesOf(element);
            return stances.Count == 1 && stances[0] == stance;
        }

        static bool BothStances(RuneId element, RuneId first, RuneId second)
        {
            var stances = StancesOf(element);
            return stances.Count == 2 && stances[0] == first && stances[1] == second;
        }

        static bool Matches(SpellId spell, RuneId element)
        {
            return SpellCodex.TryGet(spell, out var entry)
                && ChainBook.SameStory(entry.RecipeRunes, RecipeOf(element));
        }
    }
}
