namespace RuneMagic
{
    /// <summary>
    /// Slots drawn from the field: one or two materials, then an aspect.
    /// Two materials blend before the aspect is applied.
    /// </summary>
    public sealed class SpellComposer
    {
        public RuneId MaterialA { get; private set; }
        public RuneId MaterialB { get; private set; }
        public RuneId Aspect { get; private set; }
        public CastingStance Stance { get; private set; } = CastingStance.Charter;

        public Composition Snapshot() => new(MaterialA, MaterialB, Aspect);

        public void ToggleStance()
        {
            Stance = Stance == CastingStance.Charter ? CastingStance.Free : CastingStance.Charter;
        }

        public bool TryAdd(RuneId rune, out string note)
        {
            if (RuneCatalog.IsAspect(rune))
            {
                Aspect = rune;
                note = $"Aspect set to {RuneCatalog.NameOf(rune)}.";
                return true;
            }

            if (!RuneCatalog.IsMaterial(rune))
            {
                note = $"{RuneCatalog.NameOf(rune)} is not a material or aspect. The starting field does not offer it.";
                return false;
            }

            if (MaterialA == RuneId.None)
            {
                MaterialA = rune;
                note = $"Material set to {RuneCatalog.NameOf(rune)}.";
                return true;
            }

            if (MaterialB == RuneId.None && rune != MaterialA)
            {
                MaterialB = rune;
                note = MaterialTree.TryBlend(MaterialA, MaterialB, out var blend)
                    ? blend.Note
                    : $"{RuneCatalog.NameOf(MaterialA)} and {RuneCatalog.NameOf(MaterialB)} have no recorded join yet.";
                return true;
            }

            MaterialA = rune;
            MaterialB = RuneId.None;
            note = $"Material replaced with {RuneCatalog.NameOf(rune)}.";
            return true;
        }

        public void Clear()
        {
            MaterialA = RuneId.None;
            MaterialB = RuneId.None;
            Aspect = RuneId.None;
        }

        public string SlotSummary()
        {
            if (MaterialB != RuneId.None)
            {
                var blend = MaterialTree.TryBlend(MaterialA, MaterialB, out var result)
                    ? RuneCatalog.NameOf(result.Result)
                    : "unjoined";
                return $"{RuneCatalog.NameOf(MaterialA)} + {RuneCatalog.NameOf(MaterialB)} → {blend} × {RuneCatalog.NameOf(Aspect)}";
            }

            return $"{RuneCatalog.NameOf(MaterialA)} × {RuneCatalog.NameOf(Aspect)}";
        }
    }
}
