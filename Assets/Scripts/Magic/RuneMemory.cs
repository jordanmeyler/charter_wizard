using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Marks the player has chosen to keep on the Charter wall.
    /// The wall does not fill itself — a mark stays only after it is kept.
    /// For now the player just selects it. Later a rune's depth will ask
    /// for a variety of casts and other conditions before it will stay.
    /// </summary>
    public sealed class RuneMemory
    {
        readonly HashSet<RuneId> _kept = new();
        readonly List<RuneId> _order = new();

        public IReadOnlyList<RuneId> Kept => _order;
        public int Count => _order.Count;
        public bool Empty => _order.Count == 0;

        public bool Knows(RuneId rune) =>
            rune != RuneId.None && _kept.Contains(rune);

        /// <summary>
        /// How hard a mark is to keep. Unused for now — select is enough.
        /// Later: casts of several kinds, and other conditions, scale with this.
        /// </summary>
        public static int Depth(RuneId rune)
        {
            if (rune == RuneId.None)
            {
                return 0;
            }

            return ChainBook.IsWrought(rune) ? 2 : 1;
        }

        public bool TryKeep(RuneId rune, out string note)
        {
            if (rune == RuneId.None)
            {
                note = "There is no mark there to keep.";
                return false;
            }

            if (!_kept.Add(rune))
            {
                note = GlyphView.Speak(
                    $"{RuneCatalog.NameOf(rune)} is already on the wall.",
                    "That mark is already kept.");
                return false;
            }

            _order.Add(rune);
            note = GlyphView.Speak(
                $"You remember {RuneCatalog.NameOf(rune)}. It stays on the wall.",
                "You keep the mark. It stays on the wall.");
            return true;
        }

        public bool TryForget(RuneId rune, out string note)
        {
            if (!_kept.Remove(rune))
            {
                note = GlyphView.Speak(
                    $"{RuneCatalog.NameOf(rune)} was not on the wall.",
                    "That mark was not kept.");
                return false;
            }

            _order.Remove(rune);
            note = GlyphView.Speak(
                $"{RuneCatalog.NameOf(rune)} leaves the wall.",
                "The mark leaves the wall.");
            return true;
        }

        /// <summary>
        /// Play wall: only kept marks, basics first, then any other kept join.
        /// Develop still uses this list when the wall is in remember-mode;
        /// the director shows the eleven instead.
        /// </summary>
        public IReadOnlyList<RuneId> Wall(IReadOnlyList<RuneId> preferredOrder)
        {
            var list = new List<RuneId>(_order.Count);
            if (preferredOrder != null)
            {
                for (var i = 0; i < preferredOrder.Count; i++)
                {
                    if (Knows(preferredOrder[i]))
                    {
                        list.Add(preferredOrder[i]);
                    }
                }
            }

            for (var i = 0; i < _order.Count; i++)
            {
                if (!list.Contains(_order[i]))
                {
                    list.Add(_order[i]);
                }
            }

            return list;
        }
    }
}
