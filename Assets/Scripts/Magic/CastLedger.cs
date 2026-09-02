using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// The last twenty-five unique writings. Recasting the same
    /// combination moves it to the top. Charter and Free both keep
    /// the marks that were strung. Workings the adept Keep also live
    /// in the Grimoire, past this strip.
    /// </summary>
    public sealed class CastLedger
    {
        public const int Cap = 25;

        readonly List<CastAttempt> _entries = new();

        public IReadOnlyList<CastAttempt> Recent => _entries;

        public void Record(
            Composition composition,
            CastingStance stance,
            bool worked,
            SpellId spell,
            string givenName = "",
            bool saved = false,
            bool hideBadRecipes = false)
        {
            var source = composition.Sequence;
            var runes = source != null && source.Length > 0
                ? (RuneId[])source.Clone()
                : System.Array.Empty<RuneId>();
            if (hideBadRecipes && !worked)
            {
                for (var i = _entries.Count - 1; i >= 0; i--)
                {
                    if (!_entries[i].Worked)
                    {
                        _entries.RemoveAt(i);
                    }
                }
            }

            for (var i = 0; i < _entries.Count; i++)
            {
                if (!WorkingNames.SameComposition(_entries[i].Runes, runes))
                {
                    continue;
                }

                var old = _entries[i];
                if (string.IsNullOrWhiteSpace(givenName) && !string.IsNullOrWhiteSpace(old.GivenName))
                {
                    givenName = old.GivenName;
                }

                saved = saved || old.Saved;
                _entries.RemoveAt(i);
                break;
            }

            _entries.Insert(0, new CastAttempt(stance, runes, worked, spell, givenName ?? string.Empty, saved));
            while (_entries.Count > Cap)
            {
                _entries.RemoveAt(_entries.Count - 1);
            }
        }

        public bool TryKeep(int index, string givenName)
        {
            if (index < 0 || index >= _entries.Count)
            {
                return false;
            }

            var old = _entries[index];
            if (!old.Worked || old.Runes == null || old.Runes.Length == 0)
            {
                return false;
            }

            _entries[index] = new CastAttempt(
                old.Stance,
                old.Runes,
                old.Worked,
                old.Spell,
                string.IsNullOrWhiteSpace(givenName) ? old.GivenName : givenName.Trim(),
                saved: true);
            return true;
        }
    }

    public readonly struct CastAttempt
    {
        public CastAttempt(
            CastingStance stance,
            RuneId[] runes,
            bool worked,
            SpellId spell,
            string givenName = "",
            bool saved = false)
        {
            Stance = stance;
            Runes = runes ?? System.Array.Empty<RuneId>();
            Worked = worked;
            Spell = spell;
            GivenName = givenName ?? string.Empty;
            Saved = saved;
        }

        public CastingStance Stance { get; }
        public RuneId[] Runes { get; }
        public bool Worked { get; }
        public SpellId Spell { get; }
        public string GivenName { get; }
        public bool Saved { get; }
        public bool HideRunes => false;
    }
}
