using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Knowledge store. No XP, no levels. Recipes and interpretations are learned;
    /// rune identities in the field are perceived by everyone.
    /// </summary>
    public sealed class Grimoire
    {
        readonly HashSet<(RuneId Material, RuneId Aspect)> _knownRecipes = new();
        readonly HashSet<string> _knownInterpretations = new();

        public IReadOnlyCollection<(RuneId Material, RuneId Aspect)> KnownRecipes => _knownRecipes;

        public bool KnowsRecipe(RuneId material, RuneId aspect) =>
            _knownRecipes.Contains((material, aspect));

        public bool KnowsRecipe(SpellId spell)
        {
            foreach (var recipe in SpellGrammar.All)
            {
                if (recipe.Spell == spell && _knownRecipes.Contains(recipe.Key))
                {
                    return true;
                }
            }

            return false;
        }

        public void LearnRecipe(RuneId material, RuneId aspect)
        {
            _knownRecipes.Add((material, aspect));
        }

        public void LearnInterpretation(string id)
        {
            _knownInterpretations.Add(id);
        }

        public bool KnowsInterpretation(string id) => _knownInterpretations.Contains(id);

        public string DescribeSpell(RuneId material, RuneId aspect)
        {
            if (!SpellGrammar.TryGet(material, aspect, out var recipe))
            {
                return $"{SpellGrammar.FormulaText(material, aspect)} has no recorded Charter form.";
            }

            return KnowsRecipe(material, aspect)
                ? $"{recipe.Name} — {recipe.Effect}"
                : $"{SpellGrammar.FormulaText(material, aspect)} — unlearned. Borrow it or compose it to write it down.";
        }
    }
}
