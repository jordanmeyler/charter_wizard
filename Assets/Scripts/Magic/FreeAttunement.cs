using System.Collections.Generic;
using System.Text;

namespace RuneMagic
{
    /// <summary>
    /// Free grows by use. A type or a named spell you keep throwing
    /// weighs later clashes and makes that work larger.
    /// FillBudget is how many missing runes Free may supply (1 now).
    /// </summary>
    public sealed class FreeAttunement
    {
        public const int DefaultFillBudget = 1;

        readonly Dictionary<SpellId, float> _spells = new();
        readonly Dictionary<RuneId, float> _types = new();

        public int FillBudget { get; set; } = DefaultFillBudget;

        public float SpellUses(SpellId spell) =>
            spell != SpellId.None && _spells.TryGetValue(spell, out var uses) ? uses : 0f;

        public float TypeUses(RuneId rune) =>
            rune != RuneId.None && _types.TryGetValue(rune, out var uses) ? uses : 0f;

        public void Record(CodexEntry entry)
        {
            if (entry.Spell == SpellId.None)
            {
                return;
            }

            _spells.TryGetValue(entry.Spell, out var spellUses);
            _spells[entry.Spell] = spellUses + 1f;

            foreach (var type in TypesOf(entry))
            {
                _types.TryGetValue(type, out var typeUses);
                _types[type] = typeUses + 1f;
            }
        }

        public float Weight(CodexEntry entry)
        {
            var weight = 1f + SpellUses(entry.Spell);
            foreach (var type in TypesOf(entry))
            {
                weight += TypeUses(type) * 0.5f;
            }

            return weight;
        }

        public float Potency(CodexEntry entry)
        {
            var bonus = SpellUses(entry.Spell) * 0.06f;
            foreach (var type in TypesOf(entry))
            {
                bonus += TypeUses(type) * 0.03f;
            }

            if (bonus > 0.8f)
            {
                bonus = 0.8f;
            }

            return 1f + bonus;
        }

        public string Notes()
        {
            if (_spells.Count == 0 && _types.Count == 0)
            {
                return "Free attunement: unused. Use a type and it starts to lean that way.";
            }

            var text = new StringBuilder();
            text.Append("Free attunement");
            var type = Top(_types);
            if (type != RuneId.None)
            {
                text.Append(" · ").Append(RuneCatalog.NameOf(type)).Append(" ").Append(TypeUses(type).ToString("0"));
            }

            var spell = TopSpell();
            if (spell != SpellId.None && SpellCodex.TryGet(spell, out var entry))
            {
                text.Append(" · ").Append(entry.Name).Append(" ").Append(SpellUses(spell).ToString("0"));
            }

            text.Append(" · fill budget ").Append(FillBudget);
            return text.ToString();
        }

        static IEnumerable<RuneId> TypesOf(CodexEntry entry)
        {
            var seen = new HashSet<RuneId>();
            foreach (var rune in entry.RecipeRunes)
            {
                var type = TypeOf(rune);
                if (type != RuneId.None && seen.Add(type))
                {
                    yield return type;
                }
            }
        }

        static RuneId TypeOf(RuneId rune)
        {
            switch (rune)
            {
                case RuneId.Fire:
                case RuneId.Flame:
                case RuneId.Ember:
                case RuneId.Inferno:
                    return RuneId.Fire;
                case RuneId.Air:
                case RuneId.Wind:
                    return RuneId.Air;
                case RuneId.Earth:
                case RuneId.Stone:
                case RuneId.Mud:
                    return RuneId.Earth;
                case RuneId.Water:
                case RuneId.Ice:
                case RuneId.Current:
                    return RuneId.Water;
                case RuneId.Spark:
                case RuneId.Lightning:
                case RuneId.Plasma:
                    return RuneId.Spark;
                case RuneId.Vita:
                case RuneId.Plant:
                case RuneId.Vine:
                    return RuneId.Vita;
                case RuneId.Mors:
                case RuneId.Shade:
                case RuneId.Poison:
                    return RuneId.Mors;
                case RuneId.Oil:
                    return RuneId.Fire;
                case RuneId.Miasma:
                    return RuneId.Mors;
                case RuneId.Sulphur:
                    return RuneId.Sulphur;
                default:
                    return RuneId.None;
            }
        }

        static RuneId Top(Dictionary<RuneId, float> table)
        {
            var best = RuneId.None;
            var score = 0f;
            foreach (var pair in table)
            {
                if (pair.Value > score)
                {
                    score = pair.Value;
                    best = pair.Key;
                }
            }

            return best;
        }

        SpellId TopSpell()
        {
            var best = SpellId.None;
            var score = 0f;
            foreach (var pair in _spells)
            {
                if (pair.Value > score)
                {
                    score = pair.Value;
                    best = pair.Key;
                }
            }

            return best;
        }
    }
}
