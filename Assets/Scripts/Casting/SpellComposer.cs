using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// A linear rune string. Materials fold left to right; the last aspect
    /// sets form. Eight slots is the present ceiling.
    /// Charter Cast and Free Cast are separate actions, not a stance toggle.
    /// </summary>
    public sealed class SpellComposer
    {
        public const int MaxSlots = 8;

        readonly List<RuneId> _slots = new();

        public IReadOnlyList<RuneId> Slots => _slots;
        public int Count => _slots.Count;
        public bool IsEmpty => _slots.Count == 0;
        public bool IsFull => _slots.Count >= MaxSlots;

        public Composition Snapshot() => Composition.FromSequence(_slots);

        public bool TryAdd(RuneId rune, out string note)
        {
            if (rune == RuneId.None)
            {
                note = "The field offered nothing there.";
                return false;
            }

            if (IsFull)
            {
                note = "The string is full. Eight runes is the present ceiling.";
                return false;
            }

            _slots.Add(rune);
            note = Describe();
            return true;
        }

        public bool TryRemoveAt(int index, out string note)
        {
            if (index < 0 || index >= _slots.Count)
            {
                note = "That slot is empty.";
                return false;
            }

            _slots.RemoveRange(index, _slots.Count - index);
            note = IsEmpty ? "The string is released back into the field." : Describe();
            return true;
        }

        public void Clear()
        {
            _slots.Clear();
        }

        public void Load(IReadOnlyList<RuneId> runes)
        {
            _slots.Clear();
            if (runes == null)
            {
                return;
            }

            for (var i = 0; i < runes.Count && _slots.Count < MaxSlots; i++)
            {
                if (runes[i] != RuneId.None)
                {
                    _slots.Add(runes[i]);
                }
            }
        }

        public string SlotSummary()
        {
            if (IsEmpty)
            {
                return "empty string";
            }

            var parts = new string[_slots.Count];
            for (var i = 0; i < _slots.Count; i++)
            {
                parts[i] = GlyphView.IsDevelop
                    ? RuneCatalog.NameOf(_slots[i])
                    : "·";
            }

            return GlyphView.IsDevelop
                ? string.Join(" · ", parts)
                : $"{_slots.Count} mark{(_slots.Count == 1 ? "" : "s")}";
        }

        public string Describe()
        {
            if (GlyphView.IsPlay)
            {
                return DescribePlay();
            }

            var composition = Snapshot();
            var preview = ChainBook.Preview(composition);
            if (!string.IsNullOrEmpty(preview))
            {
                return $"Charter: {SlotSummary()} — {preview}";
            }

            if (!composition.TryFoldMaterials(out var material, out var blend) && composition.MaterialCount >= 2)
            {
                return $"Charter: {SlotSummary()} — those materials have no recorded join. The string will fizzle.";
            }

            var aspect = composition.Aspect;
            if (material == RuneId.None && aspect == RuneId.None)
            {
                return "The string is empty.";
            }

            if (material == RuneId.None)
            {
                return $"Charter: {SlotSummary()} — {RuneCatalog.NameOf(aspect)} waits on a material. Charter will fizzle.";
            }

            if (!RuneCatalog.IsFormAspect(aspect))
            {
                var blendNote = blend.HasValue ? $" {blend.Value.Note}" : string.Empty;
                return $"Charter: {SlotSummary()} — {RuneCatalog.NameOf(material)} is a clause, waiting. The form is in the sentence, not a later choice.{blendNote}";
            }

            return $"Charter: {SlotSummary()} — that sentence is not written. Charter will fizzle.";
        }

        string DescribePlay()
        {
            if (IsEmpty)
            {
                return "The string is empty.";
            }

            var composition = Snapshot();
            if (!string.IsNullOrEmpty(ChainBook.Preview(composition)))
            {
                return "The sentence holds.";
            }

            if (!composition.TryFoldMaterials(out _, out _) && composition.MaterialCount >= 2)
            {
                return "The sentence does not hold.";
            }

            return "The sentence is unfinished.";
        }

        public string DescribeFree(FreeAttunement attunement)
        {
            if (GlyphView.IsPlay)
            {
                if (IsEmpty)
                {
                    return "Free needs at least one mark.";
                }

                attunement = attunement ?? new FreeAttunement();
                return string.IsNullOrEmpty(ChainBook.PreviewFree(Snapshot(), attunement.FillBudget))
                    ? "Free finds no way through."
                    : "Free will try to complete this.";
            }

            if (IsEmpty)
            {
                return "Free: string at least one rune. Wild work cannot be stored.";
            }

            attunement = attunement ?? new FreeAttunement();
            var preview = ChainBook.PreviewFree(Snapshot(), attunement.FillBudget);
            if (string.IsNullOrEmpty(preview))
            {
                return $"Free: no written chain it can unscramble or complete with {CastResolver.FillWords(attunement.FillBudget)}.";
            }

            return $"Free: {preview}. Attunement weighs clashes. Order can be unscrambled. Cannot be stored.";
        }
    }
}
