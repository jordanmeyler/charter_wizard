using System;
using System.Collections.Generic;

namespace RuneMagic
{
    public enum CastingStance
    {
        Charter,
        Free
    }

    public readonly struct Composition
    {
        static readonly RuneId[] EmptySequence = Array.Empty<RuneId>();

        public Composition(RuneId materialA, RuneId materialB, RuneId aspect)
            : this(EmptySequence, materialA, materialB, aspect)
        {
        }

        Composition(RuneId[] sequence, RuneId materialA, RuneId materialB, RuneId aspect)
        {
            Sequence = sequence ?? EmptySequence;
            MaterialA = materialA;
            MaterialB = materialB;
            Aspect = aspect;
        }

        public RuneId[] Sequence { get; }
        public RuneId MaterialA { get; }
        public RuneId MaterialB { get; }
        public RuneId Aspect { get; }

        public bool HasBlank
        {
            get
            {
                if (Sequence != null && Sequence.Length > 0)
                {
                    for (var i = 0; i < Sequence.Length; i++)
                    {
                        if (Sequence[i] == RuneId.None)
                        {
                            return true;
                        }
                    }

                    return false;
                }

                return MaterialA == RuneId.None || !RuneCatalog.IsFormAspect(Aspect);
            }
        }
        public bool HasSecondMaterial => MaterialCount >= 2;
        public int MaterialCount
        {
            get
            {
                var count = 0;
                foreach (var rune in Materials())
                {
                    count++;
                }

                return count;
            }
        }

        public static Composition FromSequence(IReadOnlyList<RuneId> slots)
        {
            if (slots == null || slots.Count == 0)
            {
                return new Composition(RuneId.None, RuneId.None, RuneId.None);
            }

            var copy = new RuneId[slots.Count];
            var materialA = RuneId.None;
            var materialB = RuneId.None;
            var aspect = RuneId.None;
            var materials = 0;
            for (var i = 0; i < slots.Count; i++)
            {
                var rune = slots[i];
                copy[i] = rune;
                if (RuneCatalog.IsFormAspect(rune))
                {
                    aspect = rune;
                    continue;
                }

                if (!RuneCatalog.IsMaterial(rune))
                {
                    continue;
                }

                if (materials == 0)
                {
                    materialA = rune;
                }
                else if (materials == 1)
                {
                    materialB = rune;
                }

                materials++;
            }

            return new Composition(copy, materialA, materialB, aspect);
        }

        public IEnumerable<RuneId> Materials()
        {
            if (Sequence != null && Sequence.Length > 0)
            {
                for (var i = 0; i < Sequence.Length; i++)
                {
                    if (RuneCatalog.IsMaterial(Sequence[i]))
                    {
                        yield return Sequence[i];
                    }
                }

                yield break;
            }

            if (MaterialA != RuneId.None)
            {
                yield return MaterialA;
            }

            if (MaterialB != RuneId.None)
            {
                yield return MaterialB;
            }
        }

        public bool TryFoldMaterials(out RuneId material, out BlendResult? blend)
        {
            var gathered = new List<RuneId>();
            foreach (var rune in Materials())
            {
                gathered.Add(rune);
            }

            if (gathered.Count == 0)
            {
                material = RuneId.None;
                blend = null;
                return true;
            }

            if (gathered.Count == 1)
            {
                material = gathered[0];
                blend = null;
                return true;
            }

            if (!MaterialTree.TryFold(gathered, out material, out blend))
            {
                material = RuneId.None;
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// One held Charter sentence. Free cannot be stored — it is wild.
    /// An item may later write a Free-held form; CastHeld honours Stance.
    /// </summary>
    public readonly struct StoredSpell
    {
        public StoredSpell(Composition composition, CastingStance stance, string name)
        {
            Occupied = true;
            Composition = composition;
            Stance = stance;
            Name = name;
        }

        public bool Occupied { get; }
        public Composition Composition { get; }
        public CastingStance Stance { get; }
        public string Name { get; }

        public static StoredSpell Empty => default;
    }

    public readonly struct PreparedSpell
    {
        public PreparedSpell(RuneId material, RuneId aspect, SpellShape shape, SpellId spell, BlendKind? blend, string note, string name = "", bool freeOnly = false)
        {
            Material = material;
            Aspect = aspect;
            Shape = shape;
            Spell = spell;
            Blend = blend;
            Note = note;
            Name = name;
            FreeOnly = freeOnly;
        }

        public RuneId Material { get; }
        public RuneId Aspect { get; }
        public SpellShape Shape { get; }
        public SpellId Spell { get; }
        public BlendKind? Blend { get; }
        public string Note { get; }
        public string Name { get; }
        public bool FreeOnly { get; }
        public bool IsFormed => Spell != SpellId.None;
    }

    public readonly struct CastOutcome
    {
        public CastOutcome(
            bool resolved,
            bool revealedRecipe,
            bool backfired,
            bool fizzled,
            float taintDelta,
            SpellId spell,
            SpellShape shape,
            RuneId material,
            RuneId aspect,
            string log,
            float potency = 1f,
            int fillsUsed = 0)
        {
            Resolved = resolved;
            RevealedRecipe = revealedRecipe;
            Backfired = backfired;
            Fizzled = fizzled;
            TaintDelta = taintDelta;
            Spell = spell;
            Shape = shape;
            Material = material;
            Aspect = aspect;
            Log = log;
            Potency = potency <= 0f ? 1f : potency;
            FillsUsed = fillsUsed;
        }

        public bool Resolved { get; }
        public bool RevealedRecipe { get; }
        public bool Backfired { get; }
        public bool Fizzled { get; }
        public float TaintDelta { get; }
        public SpellId Spell { get; }
        public SpellShape Shape { get; }
        public RuneId Material { get; }
        public RuneId Aspect { get; }
        public string Log { get; }
        public float Potency { get; }
        public int FillsUsed { get; }
    }

    /// <summary>
    /// Charter is coherent: an unwritten or scrambled chain fizzles.
    /// Free may unscramble the same runes into a written sentence, or
    /// fill up to FillBudget missing runes (1 now). Attunement weighs
    /// clashes and grows the work.
    /// </summary>
    public sealed class CastResolver
    {
        readonly Random _random;

        public CastResolver(int seed = 17)
        {
            _random = new Random(seed);
        }

        public bool TryPrepare(Composition composition, SpellShape shape, out PreparedSpell prepared)
        {
            composition.TryFoldMaterials(out var material, out var blend);
            var blendKind = blend?.Kind;
            var note = blend?.Note ?? string.Empty;
            var aspect = composition.Aspect;

            if (ChainBook.TryMatch(composition, shape, out var written))
            {
                if (material == RuneId.None)
                {
                    material = FirstMaterial(written);
                }

                prepared = new PreparedSpell(material, aspect, written.Shape, written.Spell, blendKind, note, written.Name, written.FreeOnly);
                return true;
            }

            if (material == RuneId.None && composition.Sequence != null && composition.Sequence.Length >= 2)
            {
                prepared = default;
                return false;
            }

            if (material == RuneId.None || !RuneCatalog.IsFormAspect(aspect))
            {
                prepared = new PreparedSpell(material, aspect, shape, SpellId.None, blendKind, note);
                return true;
            }

            if (shape != SpellShape.None &&
                SpellGrammar.TryGet(material, aspect, shape, out var recipe))
            {
                prepared = new PreparedSpell(material, aspect, recipe.Shape, recipe.Spell, blendKind, note, recipe.Name);
                return true;
            }

            prepared = new PreparedSpell(material, aspect, shape, SpellId.None, blendKind, note);
            return true;
        }

        public bool TryChooseFree(Composition composition, FreeAttunement attunement, out CodexEntry pick)
        {
            attunement = attunement ?? new FreeAttunement();
            var pool = ChainBook.CollectForFree(composition, SpellShape.None, attunement.FillBudget);
            if (pool.Count == 0)
            {
                pick = default;
                return false;
            }

            pick = PickWeighted(pool, attunement);
            return true;
        }

        public string PreviewName(Composition composition, SpellShape shape = SpellShape.None)
        {
            var chain = ChainBook.Preview(composition, shape);
            if (!string.IsNullOrEmpty(chain))
            {
                return chain;
            }

            if (!composition.TryFoldMaterials(out var material, out _))
            {
                return composition.Sequence != null && composition.Sequence.Length > 0
                    ? "unjoined string"
                    : "empty string";
            }

            if (material == RuneId.None && !RuneCatalog.IsFormAspect(composition.Aspect))
            {
                return "empty string";
            }

            if (material == RuneId.None)
            {
                return $"{RuneCatalog.NameOf(composition.Aspect)} waits on a material";
            }

            if (!RuneCatalog.IsFormAspect(composition.Aspect))
            {
                return $"{RuneCatalog.NameOf(material)} is a clause, waiting";
            }

            if (shape != SpellShape.None &&
                SpellGrammar.TryGet(material, composition.Aspect, shape, out var named))
            {
                return named.Name;
            }

            return SpellGrammar.FormulaText(material, composition.Aspect, shape);
        }

        static RuneId FirstMaterial(CodexEntry entry)
        {
            for (var i = 0; i < entry.RecipeRunes.Count; i++)
            {
                if (RuneCatalog.IsMaterial(entry.RecipeRunes[i]))
                {
                    return entry.RecipeRunes[i];
                }
            }

            return RuneId.None;
        }

        public CastOutcome Resolve(
            Composition composition,
            CastingStance stance,
            SpellShape shape,
            SpellId[] acceptedKeys,
            Grimoire grimoire,
            FreeAttunement attunement = null,
            CodexEntry lockedFree = default)
        {
            if (composition.Sequence == null || composition.Sequence.Length == 0)
            {
                return Fail(false, false, 0f, SpellId.None, shape, RuneId.None, composition.Aspect,
                    stance == CastingStance.Free
                        ? "Free has nothing to complete. String at least one rune."
                        : "Charter refuses a blank. String a chain, or cast Free.");
            }

            if (stance == CastingStance.Free)
            {
                return ResolveFree(composition, shape, acceptedKeys, grimoire, attunement, lockedFree);
            }

            if (!TryPrepare(composition, shape, out var prepared))
            {
                return Fail(false, false, 0f, SpellId.None, shape, RuneId.None, composition.Aspect,
                    "Charter does not know that sentence. The string fizzles.");
            }

            return ResolveCharter(prepared, shape, acceptedKeys, grimoire);
        }

        CastOutcome ResolveCharter(PreparedSpell prepared, SpellShape shape, SpellId[] acceptedKeys, Grimoire grimoire)
        {
            if (!prepared.IsFormed)
            {
                var reason = !SpellFormations.MakesSense(prepared.Material, prepared.Aspect, shape)
                    ? $"{SpellGrammar.FormulaText(prepared.Material, prepared.Aspect, shape)} has no natural form. The string fizzles."
                    : $"{SpellGrammar.FormulaText(prepared.Material, prepared.Aspect, shape)} looks as if it should work. It does not. No Charter form is written. The string fizzles.";
                return Fail(false, true, 0f, SpellId.None, shape, prepared.Material, prepared.Aspect, reason);
            }

            if (prepared.FreeOnly)
            {
                return Fail(false, true, 0f, SpellId.None, shape, prepared.Material, prepared.Aspect,
                    $"{prepared.Name} is Death-work the Charter will not write. Cast it Free.");
            }

            grimoire.LearnRecipe(prepared.Material, prepared.Aspect, prepared.Shape);
            var name = !string.IsNullOrEmpty(prepared.Name)
                ? prepared.Name
                : SpellGrammar.TryGet(prepared.Material, prepared.Aspect, prepared.Shape, out var recipe)
                    ? recipe.Name
                    : SpellGrammar.FormulaText(prepared.Material, prepared.Aspect, prepared.Shape);

            if (IsKey(prepared.Spell, acceptedKeys))
            {
                return new CastOutcome(true, true, false, false, 0f, prepared.Spell, prepared.Shape,
                    prepared.Material, prepared.Aspect,
                    $"{name} turns the lock. The encounter is resolved.");
            }

            return new CastOutcome(false, true, false, false, 0f, prepared.Spell, prepared.Shape,
                prepared.Material, prepared.Aspect,
                $"{name} holds together — Charter overpowers, it does not dispel — but this lock does not accept that key.");
        }

        CastOutcome ResolveFree(
            Composition composition,
            SpellShape shape,
            SpellId[] acceptedKeys,
            Grimoire grimoire,
            FreeAttunement attunement,
            CodexEntry lockedFree)
        {
            attunement = attunement ?? new FreeAttunement();
            var pool = ChainBook.CollectForFree(composition, SpellShape.None, attunement.FillBudget);
            if (pool.Count == 0)
            {
                return Fail(false, true, 0.06f, SpellId.None, shape, composition.MaterialA, composition.Aspect,
                    $"Free reaches and finds no written chain it can unscramble or complete with {FillWords(attunement.FillBudget)}. The surge folds inward.");
            }

            var pick = lockedFree.Spell != SpellId.None ? lockedFree : PickWeighted(pool, attunement);
            attunement.Record(pick);
            grimoire.LearnRecipe(FirstMaterial(pick), LastAspect(pick), pick.Shape);

            var fills = ChainBook.FillsNeeded(composition, pick);
            if (fills < 0)
            {
                fills = 0;
            }

            var clash = pool.Count > 1
                ? $" {pool.Count} chains fit; attunement drew {pick.Name}."
                : string.Empty;
            var scrambled = ChainBook.IsScrambled(composition, pick);
            var fillNote = scrambled
                ? $"Free unscrambles the runes and the chain becomes {pick.Name}.{clash} "
                : fills == 0
                    ? $"Free takes the finished sentence.{clash} "
                    : fills == 1
                        ? $"Free fills a rune and the chain becomes {pick.Name}.{clash} "
                        : $"Free fills {fills} runes and the chain becomes {pick.Name}.{clash} ";

            var material = FirstMaterial(pick);
            var potency = attunement.Potency(pick);
            var isKey = IsKey(pick.Spell, acceptedKeys);

            if (isKey)
            {
                return new CastOutcome(true, true, false, false, 0.08f, pick.Spell, pick.Shape,
                    material, LastAspect(pick),
                    fillNote + $"{pick.Name} tears the lock open. Free is a shortcut, not the required key.",
                    potency, fills);
            }

            return new CastOutcome(false, true, false, false, 0.06f, pick.Spell, pick.Shape,
                material, LastAspect(pick),
                fillNote + $"{pick.Name} lands, wild and larger, but this lock does not accept that key.",
                potency, fills);
        }

        CodexEntry PickWeighted(List<CodexEntry> pool, FreeAttunement attunement)
        {
            var total = 0f;
            var weights = new float[pool.Count];
            for (var i = 0; i < pool.Count; i++)
            {
                weights[i] = attunement.Weight(pool[i]);
                total += weights[i];
            }

            if (total <= 0f)
            {
                return pool[_random.Next(pool.Count)];
            }

            var roll = (float)_random.NextDouble() * total;
            var walk = 0f;
            for (var i = 0; i < pool.Count; i++)
            {
                walk += weights[i];
                if (roll <= walk)
                {
                    return pool[i];
                }
            }

            return pool[pool.Count - 1];
        }

        public static string FillWords(int fillBudget)
        {
            return fillBudget == 1 ? "one rune" : fillBudget + " runes";
        }

        static RuneId LastAspect(CodexEntry entry)
        {
            for (var i = entry.RecipeRunes.Count - 1; i >= 0; i--)
            {
                if (RuneCatalog.IsFormAspect(entry.RecipeRunes[i]))
                {
                    return entry.RecipeRunes[i];
                }
            }

            return RuneId.None;
        }

        static CastOutcome Fail(
            bool revealed,
            bool fizzled,
            float taint,
            SpellId spell,
            SpellShape shape,
            RuneId material,
            RuneId aspect,
            string log)
        {
            return new CastOutcome(false, revealed, false, fizzled, taint, spell, shape, material, aspect, log);
        }

        static string DescribeUnjoined(Composition composition)
        {
            var names = new List<string>();
            foreach (var rune in composition.Materials())
            {
                names.Add(RuneCatalog.NameOf(rune));
            }

            if (names.Count < 2)
            {
                return "Those runes do not form a material.";
            }

            return string.Join(" + ", names) + " have no recorded join yet.";
        }

        static bool IsKey(SpellId spell, SpellId[] acceptedKeys)
        {
            if (spell == SpellId.None || acceptedKeys == null)
            {
                return false;
            }

            for (var i = 0; i < acceptedKeys.Length; i++)
            {
                if (acceptedKeys[i] == spell)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
