using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Knowledge store. No XP, no levels. Recipes and interpretations are learned;
    /// rune identities in the field are perceived by everyone.
    /// </summary>
    public sealed class Grimoire
    {
        readonly HashSet<(RuneId Material, RuneId Aspect, SpellShape Shape)> _knownRecipes = new();
        readonly HashSet<string> _knownInterpretations = new();

        public IReadOnlyCollection<(RuneId Material, RuneId Aspect, SpellShape Shape)> KnownRecipes => _knownRecipes;

        public bool KnowsRecipe(RuneId material, RuneId aspect, SpellShape shape) =>
            _knownRecipes.Contains((material, aspect, shape));

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

        public void LearnRecipe(RuneId material, RuneId aspect, SpellShape shape)
        {
            _knownRecipes.Add((material, aspect, shape));
        }

        public void LearnInterpretation(string id)
        {
            _knownInterpretations.Add(id);
        }

        public bool KnowsInterpretation(string id) => _knownInterpretations.Contains(id);

        public string DescribeSpell(RuneId material, RuneId aspect, SpellShape shape)
        {
            if (!SpellGrammar.TryGet(material, aspect, shape, out var recipe))
            {
                return $"{SpellGrammar.FormulaText(material, aspect, shape)} has no recorded Charter form.";
            }

            return KnowsRecipe(material, aspect, shape)
                ? $"{recipe.Name} — {recipe.Effect}"
                : $"{SpellGrammar.FormulaText(material, aspect, shape)} — unlearned. Borrow it or compose it to write it down.";
        }
    }
}
