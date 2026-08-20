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

        public bool HasBlank => MaterialA == RuneId.None || !RuneCatalog.IsFormAspect(Aspect);
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
        public PreparedSpell(RuneId material, RuneId aspect, SpellShape shape, SpellId spell, BlendKind? blend, string note)
        {
            Material = material;
            Aspect = aspect;
            Shape = shape;
            Spell = spell;
            Blend = blend;
            Note = note;
        }

        public RuneId Material { get; }
        public RuneId Aspect { get; }
        public SpellShape Shape { get; }
        public SpellId Spell { get; }
        public BlendKind? Blend { get; }
        public string Note { get; }
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
            string log)
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
    }

    /// <summary>
    /// Charter is coherent and reliable. An unwritten combo fizzles.
    /// Free never invents a new form — it borrows a random written spell of that type.
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
            if (!composition.TryFoldMaterials(out var material, out var blend))
            {
                prepared = default;
                return false;
            }

            var blendKind = blend?.Kind;
            var note = blend?.Note ?? string.Empty;
            var aspect = composition.Aspect;

            if (material == RuneId.None || !RuneCatalog.IsFormAspect(aspect) || shape == SpellShape.None)
            {
                prepared = new PreparedSpell(material, aspect, shape, SpellId.None, blendKind, note);
                return true;
            }

            SpellGrammar.TryGet(material, aspect, shape, out var recipe);
            prepared = new PreparedSpell(material, aspect, shape, recipe.Spell, blendKind, note);
            return true;
        }

        public string PreviewName(Composition composition, SpellShape shape = SpellShape.None)
        {
            if (!composition.TryFoldMaterials(out var material, out _))
            {
                return "unjoined string";
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
                return $"{RuneCatalog.NameOf(material)} waits on a non-elemental aspect";
            }

            if (shape != SpellShape.None &&
                SpellGrammar.TryGet(material, composition.Aspect, shape, out var named))
            {
                return named.Name;
            }

            return SpellGrammar.FormulaText(material, composition.Aspect, shape);
        }

        public CastOutcome Resolve(
            Composition composition,
            CastingStance stance,
            SpellShape shape,
            SpellId[] acceptedKeys,
            Grimoire grimoire)
        {
            if (stance == CastingStance.Charter && composition.HasBlank)
            {
                return Fail(false, false, 0f, SpellId.None, shape, RuneId.None, composition.Aspect,
                    "Charter refuses a blank. An element is not a spell. String a non-elemental aspect, or shift to Free.");
            }

            if (!TryPrepare(composition, shape, out var prepared))
            {
                return Fail(false, false, 0f, SpellId.None, shape, RuneId.None, composition.Aspect,
                    DescribeUnjoined(composition));
            }

            if (stance == CastingStance.Charter)
            {
                return ResolveCharter(prepared, shape, acceptedKeys, grimoire);
            }

            return ResolveFree(composition, prepared, shape, acceptedKeys, grimoire);
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

            grimoire.LearnRecipe(prepared.Material, prepared.Aspect, prepared.Shape);
            var name = SpellGrammar.TryGet(prepared.Material, prepared.Aspect, prepared.Shape, out var recipe)
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

        CastOutcome ResolveFree(Composition composition, PreparedSpell prepared, SpellShape shape, SpellId[] acceptedKeys, Grimoire grimoire)
        {
            var filled = prepared;
            var fillNote = string.Empty;

            if (!filled.IsFormed)
            {
                if (!TryBorrow(composition, prepared, shape, out filled))
                {
                    return Fail(false, false, 0.12f, SpellId.None, shape, prepared.Material, prepared.Aspect,
                        "Free reaches for a spell of that type and finds none written. The surge folds inward.");
                }

                fillNote = composition.HasBlank
                    ? "Blanks flood from the field. Free borrows a written form of that type. "
                    : "No Charter form is written. Free borrows a random spell of that type. ";
            }

            grimoire.LearnRecipe(filled.Material, filled.Aspect, filled.Shape);

            var reliability = 0.7f;
            if (composition.HasBlank || !prepared.IsFormed)
            {
                reliability -= 0.3f;
            }

            if (filled.Blend == BlendKind.Violent)
            {
                reliability -= 0.15f;
            }

            var roll = _random.NextDouble();
            var isKey = filled.IsFormed && IsKey(filled.Spell, acceptedKeys);
            var name = filled.IsFormed && SpellGrammar.TryGet(filled.Material, filled.Aspect, filled.Shape, out var recipe)
                ? recipe.Name
                : "an unshaped surge";

            if (isKey && roll <= reliability)
            {
                return new CastOutcome(true, true, false, false, 0.08f, filled.Spell, filled.Shape,
                    filled.Material, filled.Aspect,
                    fillNote + $"{name} tears the lock open. Free is a shortcut, not the required key. Hubris gathers.");
            }

            if (roll < 0.28)
            {
                return new CastOutcome(false, true, true, false, 0.18f, filled.Spell, filled.Shape,
                    filled.Material, filled.Aspect,
                    fillNote + $"{name} backfires. Target inverts, or the surge folds inward. Taint remains.");
            }

            return new CastOutcome(false, true, false, false, 0.1f, filled.Spell, filled.Shape,
                filled.Material, filled.Aspect,
                fillNote + $"{name} sputters. Magnitude without coherence. The lock holds.");
        }

        bool TryBorrow(Composition composition, PreparedSpell prepared, SpellShape shape, out PreparedSpell borrowed)
        {
            var material = prepared.Material;
            var aspect = RuneCatalog.IsFormAspect(prepared.Aspect) ? prepared.Aspect : RuneId.None;

            if (material == RuneId.None && composition.MaterialA != RuneId.None)
            {
                material = composition.MaterialA;
            }

            var pool = new List<SpellRecipe>();
            Collect(pool, material, aspect, shape);
            if (pool.Count == 0 && shape != SpellShape.None)
            {
                Collect(pool, material, aspect, SpellShape.None);
            }

            if (pool.Count == 0 && aspect != RuneId.None)
            {
                Collect(pool, material, RuneId.None, SpellShape.None);
            }

            if (pool.Count == 0 && material != RuneId.None)
            {
                Collect(pool, material, RuneId.None, SpellShape.None);
            }

            if (pool.Count == 0)
            {
                foreach (var recipe in SpellGrammar.All)
                {
                    pool.Add(recipe);
                }
            }

            if (pool.Count == 0)
            {
                borrowed = default;
                return false;
            }

            var pick = pool[_random.Next(pool.Count)];
            borrowed = new PreparedSpell(pick.Material, pick.Aspect, pick.Shape, pick.Spell, prepared.Blend, "Free-borrow");
            return true;
        }

        static void Collect(List<SpellRecipe> pool, RuneId material, RuneId aspect, SpellShape shape)
        {
            foreach (var recipe in SpellGrammar.OfType(material, aspect))
            {
                if (shape == SpellShape.None || recipe.Shape == shape)
                {
                    pool.Add(recipe);
                }
            }
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
