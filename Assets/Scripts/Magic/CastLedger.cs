using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// The last twenty-five attempted casts. Charter keeps the marks.
    /// Free blocks them — wild work is not written down.
    /// Workings the adept Keep also live in the Grimoire, past this strip.
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
            bool saved = false)
        {
            var source = composition.Sequence;
            var runes = source != null && source.Length > 0
                ? (RuneId[])source.Clone()
                : System.Array.Empty<RuneId>();
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
            if (!old.Worked || old.Spell == SpellId.None)
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
        public bool HideRunes => Stance == CastingStance.Free;
    }
}
