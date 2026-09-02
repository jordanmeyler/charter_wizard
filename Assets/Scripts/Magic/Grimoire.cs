using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Knowledge store. No XP, no levels. Recipes and interpretations are learned;
    /// rune identities in the field are perceived by everyone.
    /// Workings the adept Keep from Recent sit here as the player's book.
    /// </summary>
    public sealed class Grimoire
    {
        readonly HashSet<(RuneId Material, RuneId Aspect, SpellShape Shape)> _knownRecipes = new();
        readonly HashSet<string> _knownInterpretations = new();
        readonly HashSet<SpellId> _keptSpells = new();
        readonly List<KeptWorking> _keptWorkings = new();

        public WorkingNames Names { get; } = new();
        public IReadOnlyCollection<(RuneId Material, RuneId Aspect, SpellShape Shape)> KnownRecipes => _knownRecipes;
        public IReadOnlyList<KeptWorking> KeptWorkings => _keptWorkings;

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

        public bool Keeps(SpellId spell) =>
            spell != SpellId.None && _keptSpells.Contains(spell);

        public void Keep(SpellId spell)
        {
            if (spell != SpellId.None)
            {
                _keptSpells.Add(spell);
            }
        }

        public void KeepWorking(CastingStance stance, IReadOnlyList<RuneId> runes, SpellId spell, string givenName)
        {
            if (runes == null || runes.Count == 0)
            {
                return;
            }

            Keep(spell);
            var copy = new RuneId[runes.Count];
            for (var i = 0; i < runes.Count; i++)
            {
                copy[i] = runes[i];
            }

            var name = givenName ?? string.Empty;
            for (var i = 0; i < _keptWorkings.Count; i++)
            {
                if (WorkingNames.SameComposition(_keptWorkings[i].Runes, copy))
                {
                    _keptWorkings[i] = new KeptWorking(stance, copy, spell, name);
                    return;
                }
            }

            _keptWorkings.Add(new KeptWorking(stance, copy, spell, name));
        }

        public bool TryGetKept(int index, out KeptWorking working)
        {
            if (index < 0 || index >= _keptWorkings.Count)
            {
                working = default;
                return false;
            }

            working = _keptWorkings[index];
            return true;
        }

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

    /// <summary>
    /// A working the adept named and kept. The exact writing is the page —
    /// Spark is not Fire · Air.
    /// </summary>
    public readonly struct KeptWorking
    {
        public KeptWorking(CastingStance stance, RuneId[] runes, SpellId spell, string givenName)
        {
            Stance = stance;
            Runes = runes ?? System.Array.Empty<RuneId>();
            Spell = spell;
            GivenName = givenName ?? string.Empty;
        }

        public CastingStance Stance { get; }
        public RuneId[] Runes { get; }
        public SpellId Spell { get; }
        public string GivenName { get; }

        public string Label =>
            string.IsNullOrEmpty(GivenName) ? WorkingNames.RunePhrase(Runes) : GivenName;
    }
}
