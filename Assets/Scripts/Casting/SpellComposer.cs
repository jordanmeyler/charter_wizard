using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// A linear rune string. Materials fold left to right; the last aspect
    /// sets form. Eight slots is the present ceiling.
    /// </summary>
    public sealed class SpellComposer
    {
        public const int MaxSlots = 8;

        readonly List<RuneId> _slots = new();

        public IReadOnlyList<RuneId> Slots => _slots;
        public int Count => _slots.Count;
        public bool IsEmpty => _slots.Count == 0;
        public bool IsFull => _slots.Count >= MaxSlots;
        public CastingStance Stance { get; private set; } = CastingStance.Charter;

        public Composition Snapshot() => Composition.FromSequence(_slots);

        public void ToggleStance()
        {
            Stance = Stance == CastingStance.Charter ? CastingStance.Free : CastingStance.Charter;
        }

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

        public string SlotSummary()
        {
            if (IsEmpty)
            {
                return "empty string";
            }

            var parts = new string[_slots.Count];
            for (var i = 0; i < _slots.Count; i++)
            {
                parts[i] = RuneCatalog.NameOf(_slots[i]);
            }

            return string.Join(" · ", parts);
        }

        public string Describe()
        {
            var composition = Snapshot();
            if (!composition.TryFoldMaterials(out var material, out var blend) && composition.MaterialCount >= 2)
            {
                return $"{SlotSummary()} — those materials have no recorded join yet.";
            }

            var aspect = composition.Aspect;
            if (material == RuneId.None && aspect == RuneId.None)
            {
                return "The string is empty.";
            }

            if (material == RuneId.None)
            {
                return $"{SlotSummary()} — {RuneCatalog.NameOf(aspect)} waits on a material.";
            }

            if (!RuneCatalog.IsFormAspect(aspect))
            {
                var blendNote = blend.HasValue ? $" {blend.Value.Note}" : string.Empty;
                return $"{SlotSummary()} — {RuneCatalog.NameOf(material)} is a clause, waiting. Add what happens next.{blendNote}";
            }

            var forms = SpellFormations.Available(material, aspect);
            if (forms.Count == 0)
            {
                return Stance == CastingStance.Free
                    ? $"{SlotSummary()} — no natural form. Free will borrow a random spell of that type."
                    : $"{SlotSummary()} — those runes have no natural form. Charter will fizzle.";
            }

            var written = 0;
            foreach (var shape in forms)
            {
                if (SpellGrammar.TryGet(material, aspect, shape, out _))
                {
                    written++;
                }
            }

            var formNames = new string[forms.Count];
            for (var i = 0; i < forms.Count; i++)
            {
                formNames[i] = SpellFormations.NameOf(forms[i]);
            }

            if (written == 0)
            {
                return $"{SlotSummary()} — may take {string.Join(", ", formNames)}. No Charter form is written. Charter fizzles; Free borrows.";
            }

            return $"{SlotSummary()} — may take {string.Join(", ", formNames)}. Cast to choose how it aims.";
        }
    }
}
