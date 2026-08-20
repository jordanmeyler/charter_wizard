using System;

namespace RuneMagic
{
    public enum CastingStance
    {
        Charter,
        Free
    }

    public readonly struct Composition
    {
        public Composition(RuneId materialA, RuneId materialB, RuneId aspect)
        {
            MaterialA = materialA;
            MaterialB = materialB;
            Aspect = aspect;
        }

        public RuneId MaterialA { get; }
        public RuneId MaterialB { get; }
        public RuneId Aspect { get; }

        public bool HasBlank => MaterialA == RuneId.None || Aspect == RuneId.None;
        public bool HasSecondMaterial => MaterialB != RuneId.None;
    }

    public readonly struct PreparedSpell
    {
        public PreparedSpell(RuneId material, RuneId aspect, SpellId spell, BlendKind? blend, string note)
        {
            Material = material;
            Aspect = aspect;
            Spell = spell;
            Blend = blend;
            Note = note;
        }

        public RuneId Material { get; }
        public RuneId Aspect { get; }
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
            float taintDelta,
            SpellId spell,
            string log)
        {
            Resolved = resolved;
            RevealedRecipe = revealedRecipe;
            Backfired = backfired;
            TaintDelta = taintDelta;
            Spell = spell;
            Log = log;
        }

        public bool Resolved { get; }
        public bool RevealedRecipe { get; }
        public bool Backfired { get; }
        public float TaintDelta { get; }
        public SpellId Spell { get; }
        public string Log { get; }
    }

    /// <summary>
    /// Charter is coherent and reliable. Free is a risk dial and a teacher.
    /// Free is never the required key; it is only a tempting shortcut.
    /// </summary>
    public sealed class CastResolver
    {
        readonly Random _random;

        public CastResolver(int seed = 17)
        {
            _random = new Random(seed);
        }

        public bool TryPrepare(Composition composition, out PreparedSpell prepared)
        {
            var material = composition.MaterialA;
            var note = string.Empty;
            BlendKind? blendKind = null;

            if (composition.HasSecondMaterial)
            {
                if (!MaterialTree.TryBlend(composition.MaterialA, composition.MaterialB, out var blend))
                {
                    prepared = default;
                    return false;
                }

                material = blend.Result;
                blendKind = blend.Kind;
                note = blend.Note;
            }

            if (material == RuneId.None || composition.Aspect == RuneId.None)
            {
                prepared = new PreparedSpell(material, composition.Aspect, SpellId.None, blendKind, note);
                return true;
            }

            SpellGrammar.TryGet(material, composition.Aspect, out var recipe);
            prepared = new PreparedSpell(material, composition.Aspect, recipe.Spell, blendKind, note);
            return true;
        }

        public CastOutcome Resolve(
            Composition composition,
            CastingStance stance,
            SpellId[] acceptedKeys,
            Grimoire grimoire)
        {
            if (stance == CastingStance.Charter && composition.HasBlank)
            {
                return new CastOutcome(false, false, false, 0f, SpellId.None,
                    "Charter refuses a blank. Specify a material and an aspect, or shift to Free.");
            }

            if (!TryPrepare(composition, out var prepared))
            {
                return new CastOutcome(false, false, false, 0f, SpellId.None,
                    MaterialTree.DescribePair(composition.MaterialA, composition.MaterialB));
            }

            if (stance == CastingStance.Charter)
            {
                return ResolveCharter(prepared, acceptedKeys, grimoire);
            }

            return ResolveFree(composition, prepared, acceptedKeys, grimoire);
        }

        CastOutcome ResolveCharter(PreparedSpell prepared, SpellId[] acceptedKeys, Grimoire grimoire)
        {
            if (!prepared.IsFormed)
            {
                return new CastOutcome(false, false, false, 0f, SpellId.None,
                    $"{SpellGrammar.FormulaText(prepared.Material, prepared.Aspect)} is coherent as far as it goes, but no Charter form is written yet.");
            }

            grimoire.LearnRecipe(prepared.Material, prepared.Aspect);
            var name = SpellGrammar.TryGet(prepared.Material, prepared.Aspect, out var recipe)
                ? recipe.Name
                : SpellGrammar.FormulaText(prepared.Material, prepared.Aspect);

            if (IsKey(prepared.Spell, acceptedKeys))
            {
                return new CastOutcome(true, true, false, 0f, prepared.Spell,
                    $"{name} turns the lock. The encounter is resolved.");
            }

            return new CastOutcome(false, true, false, 0f, prepared.Spell,
                $"{name} holds together — Charter overpowers, it does not dispel — but this lock does not accept that key.");
        }

        CastOutcome ResolveFree(Composition composition, PreparedSpell prepared, SpellId[] acceptedKeys, Grimoire grimoire)
        {
            var filled = prepared;
            var fillNote = string.Empty;

            if (composition.HasBlank)
            {
                filled = AutoFill(composition);
                fillNote = "Blanks flood from the field. ";
            }

            if (filled.IsFormed)
            {
                grimoire.LearnRecipe(filled.Material, filled.Aspect);
            }

            var reliability = 0.7f;
            if (composition.HasBlank)
            {
                reliability -= 0.3f;
            }

            if (filled.Blend == BlendKind.Violent)
            {
                reliability -= 0.15f;
            }

            var roll = _random.NextDouble();
            var isKey = filled.IsFormed && IsKey(filled.Spell, acceptedKeys);
            var name = filled.IsFormed && SpellGrammar.TryGet(filled.Material, filled.Aspect, out var recipe)
                ? recipe.Name
                : "an unshaped surge";

            if (isKey && roll <= reliability)
            {
                return new CastOutcome(true, filled.IsFormed, false, 0.08f, filled.Spell,
                    fillNote + $"{name} tears the lock open. Free is a shortcut, not the required key. Hubris gathers.");
            }

            if (roll < 0.28)
            {
                return new CastOutcome(false, filled.IsFormed, true, 0.18f, filled.Spell,
                    fillNote + $"{name} backfires. Target inverts, or the surge folds inward. Taint remains.");
            }

            return new CastOutcome(false, filled.IsFormed, false, 0.1f, filled.Spell,
                fillNote + $"{name} sputters. Magnitude without coherence. The lock holds.");
        }

        PreparedSpell AutoFill(Composition composition)
        {
            var material = composition.MaterialA;
            BlendKind? blend = null;

            if (material == RuneId.None)
            {
                var primaries = new[] { RuneId.Fire, RuneId.Air, RuneId.Earth, RuneId.Water };
                material = primaries[_random.Next(primaries.Length)];
            }

            if (composition.HasSecondMaterial && MaterialTree.TryBlend(material, composition.MaterialB, out var blended))
            {
                material = blended.Result;
                blend = blended.Kind;
            }

            var aspect = composition.Aspect;
            if (aspect == RuneId.None)
            {
                var aspects = new[] { RuneId.Salt, RuneId.Mercury, RuneId.Sulphur };
                aspect = aspects[_random.Next(aspects.Length)];
            }

            SpellGrammar.TryGet(material, aspect, out var recipe);
            return new PreparedSpell(material, aspect, recipe.Spell, blend, "Free-fill");
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
