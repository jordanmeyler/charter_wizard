using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public enum CombatRange
    {
        Close = 0,
        Mid,
        Long
    }

    public enum CombatMode
    {
        Auto = 0,
        Hunt,
        Guard,
        Skirmish,
        Caster,
        Wander
    }

    public enum CombatStrike
    {
        None = 0,
        Slam,
        Shot,
        Pillar
    }

    public enum AuthoredNature
    {
        Auto = 0,
        Flesh,
        Fire,
        Ice,
        Earth,
        Mind,
        Plant,
        Undead
    }

    public enum GambitWhen
    {
        Always = 0,
        PlayerCasts,
        PlayerRaisesWall,
        InCloseRange,
        InMidRange,
        InLongRange,
        AllyNearby,
        SelfHasStatus,
        TargetHasStatus
    }

    /// <summary>
    /// One strike an enemy can throw. Close slams, mid and long write
    /// a sentence. Picking a spell fills the runes from the book.
    /// </summary>
    [System.Serializable]
    public sealed class CombatSlot
    {
        public string Name;
        public CombatRange Range;
        public CombatStrike Strike;
        public SpellId Spell;
        public string[] Recipe;
        public float CastSeconds;
    }

    /// <summary>
    /// FF12-style if/then. First match wins. Player-cast triggers
    /// arrive through <see cref="CombatActor.NoticePlayerSpell"/>.
    /// </summary>
    [System.Serializable]
    public sealed class CombatGambit
    {
        public string Name;
        public GambitWhen When;
        public SpellId WhenSpell;
        public StatusId WhenStatus;
        public CombatStrike ThenStrike;
        public SpellId ThenSpell;
        public string[] ThenRecipe;
        public bool Once;
    }

    [System.Serializable]
    public sealed class StrikeAffinity
    {
        public StrikeKind Kind = StrikeKind.Fire;
        public int Affinity = 1;
    }

    [System.Serializable]
    public sealed class StatusAffinity
    {
        public StatusId Status = StatusId.Burning;
        public int Affinity = 1;
    }

    /// <summary>
    /// Runtime mind for a lock: mode, range bands, slots, and gambits.
    /// </summary>
    public sealed class CombatPlan
    {
        public CombatKind Kind;
        public CombatMode Mode;
        public float CloseRange;
        public float MidRange;
        public float LongRange;
        public CombatSlot[] Slots;
        public CombatGambit[] Gambits;
        public float CastSeconds;
    }

    public readonly struct CombatPage
    {
        public CombatPage(string name, SpellId spell, CombatRange range, CombatStrike strike, float castSeconds)
        {
            Name = name;
            Spell = spell;
            Range = range;
            Strike = strike;
            CastSeconds = castSeconds;
        }

        public string Name { get; }
        public SpellId Spell { get; }
        public CombatRange Range { get; }
        public CombatStrike Strike { get; }
        public float CastSeconds { get; }
    }

    /// <summary>
    /// Enemy attacks the Inspector can pick. A spell writes its runes
    /// so the author does not have to.
    /// </summary>
    public static class CombatBook
    {
        public const float DefaultClose = 1.25f;
        public const float DefaultMid = 4.5f;
        public const float DefaultLong = 8.2f;

        public static readonly CombatPage[] Pages =
        {
            new("None (write runes)", SpellId.None, CombatRange.Close, CombatStrike.None, 0f),
            new("Slam", SpellId.None, CombatRange.Close, CombatStrike.Slam, 0.85f),
            new("Fireball", SpellId.Fireball, CombatRange.Mid, CombatStrike.Shot, 2f),
            new("Wood arrow", SpellId.WoodArrow, CombatRange.Long, CombatStrike.Shot, 1.15f),
            new("Hurled stone", SpellId.HurledStone, CombatRange.Mid, CombatStrike.Shot, 1.3f),
            new("Ice-spear", SpellId.IceSpear, CombatRange.Long, CombatStrike.Shot, 1.6f),
            new("Spark shot", SpellId.SparkShot, CombatRange.Mid, CombatStrike.Shot, 1.5f),
            new("Lightning bolt", SpellId.LightningBolt, CombatRange.Long, CombatStrike.Shot, 1.8f),
            new("Water-jet", SpellId.WaterJet, CombatRange.Mid, CombatStrike.Shot, 1.4f),
            new("Scald", SpellId.Scald, CombatRange.Mid, CombatStrike.Shot, 1.5f),
            new("Vine", SpellId.Vine, CombatRange.Mid, CombatStrike.Shot, 1.5f),
            new("Gust", SpellId.Gust, CombatRange.Mid, CombatStrike.Shot, 1.2f),
            new("Poison", SpellId.Poison, CombatRange.Mid, CombatStrike.Shot, 1.6f),
            new("Witchfire", SpellId.Witchfire, CombatRange.Long, CombatStrike.Shot, 2.2f),
            new("Flame-pillar", SpellId.FlamePillar, CombatRange.Mid, CombatStrike.Pillar, 2f),
            new("Fire-pillar", SpellId.FirePillar, CombatRange.Mid, CombatStrike.Pillar, 1.8f),
            new("Ice-pillar", SpellId.IcePillar, CombatRange.Mid, CombatStrike.Pillar, 2f),
            new("Earth-pillar", SpellId.EarthPillar, CombatRange.Close, CombatStrike.Pillar, 1.6f),
            new("Lava-pillar", SpellId.LavaPillar, CombatRange.Mid, CombatStrike.Pillar, 2.2f),
            new("Water-pillar", SpellId.WaterPillar, CombatRange.Mid, CombatStrike.Pillar, 1.8f)
        };

        public static readonly StrikeKind[] TunableStrikes =
        {
            StrikeKind.Fire, StrikeKind.Water, StrikeKind.Earth, StrikeKind.Air,
            StrikeKind.Physical, StrikeKind.Spark, StrikeKind.Ice, StrikeKind.Plant,
            StrikeKind.Poison, StrikeKind.Lava, StrikeKind.Witchfire, StrikeKind.Plasma,
            StrikeKind.Metal, StrikeKind.Light, StrikeKind.Dark, StrikeKind.Death, StrikeKind.Life
        };

        public static readonly StatusId[] TunableStatuses =
        {
            StatusId.Burning, StatusId.Frozen, StatusId.Soaked, StatusId.Stunned,
            StatusId.Sleeping, StatusId.Rooted, StatusId.Frightened, StatusId.Raging,
            StatusId.Charmed, StatusId.Confused, StatusId.Poisoned
        };

        public static CombatSlot SlamSlot()
        {
            return new CombatSlot
            {
                Name = "Slam",
                Range = CombatRange.Close,
                Strike = CombatStrike.Slam,
                Spell = SpellId.None,
                Recipe = System.Array.Empty<string>(),
                CastSeconds = 0.85f
            };
        }

        public static CombatSlot SlotFromPage(CombatPage page)
        {
            return new CombatSlot
            {
                Name = page.Name,
                Range = page.Range,
                Strike = page.Strike,
                Spell = page.Spell,
                Recipe = RecipeNames(page.Spell),
                CastSeconds = page.CastSeconds
            };
        }

        public static CombatGambit WallToFlamePillar()
        {
            return new CombatGambit
            {
                Name = "Wall → flame-pillar",
                When = GambitWhen.PlayerRaisesWall,
                WhenSpell = SpellId.Wall,
                ThenStrike = CombatStrike.Pillar,
                ThenSpell = SpellId.FlamePillar,
                ThenRecipe = RecipeNames(SpellId.FlamePillar),
                Once = false
            };
        }

        public static CombatSlot SlotFromGambit(CombatGambit gambit)
        {
            if (gambit == null)
            {
                return null;
            }

            var strike = gambit.ThenStrike;
            if (strike == CombatStrike.None)
            {
                strike = StrikeOf(gambit.ThenSpell);
            }

            var recipe = gambit.ThenRecipe;
            if ((recipe == null || recipe.Length == 0) && gambit.ThenSpell != SpellId.None)
            {
                recipe = RecipeNames(gambit.ThenSpell);
            }

            return new CombatSlot
            {
                Name = string.IsNullOrEmpty(gambit.Name) ? NameOf(gambit.ThenSpell, strike) : gambit.Name,
                Range = DefaultRange(gambit.ThenSpell, strike),
                Strike = strike,
                Spell = gambit.ThenSpell,
                Recipe = recipe ?? System.Array.Empty<string>(),
                CastSeconds = DefaultCastSeconds(gambit.ThenSpell, strike)
            };
        }

        public static string NameOf(SpellId spell, CombatStrike strike)
        {
            if (strike == CombatStrike.Slam)
            {
                return "Slam";
            }

            if (spell != SpellId.None && TryPage(spell, out var page))
            {
                return page.Name;
            }

            if (spell != SpellId.None)
            {
                return SpellRegistry.NameOf(spell);
            }

            return strike == CombatStrike.None ? "None" : strike.ToString();
        }

        public static bool TryPage(SpellId spell, out CombatPage page)
        {
            for (var i = 0; i < Pages.Length; i++)
            {
                if (Pages[i].Spell == spell && (spell != SpellId.None || Pages[i].Strike == CombatStrike.None))
                {
                    page = Pages[i];
                    return true;
                }
            }

            page = default;
            return false;
        }

        public static CombatPage PageOf(SpellId spell, CombatStrike strike)
        {
            if (strike == CombatStrike.Slam)
            {
                return Pages[1];
            }

            for (var i = 0; i < Pages.Length; i++)
            {
                if (Pages[i].Spell == spell && Pages[i].Strike != CombatStrike.None)
                {
                    return Pages[i];
                }
            }

            return Pages[0];
        }

        public static RuneId[] RecipeOf(SpellId spell)
        {
            return DeathCause.RecipeOf(spell);
        }

        public static string[] RecipeNames(SpellId spell)
        {
            return NamesOf(RecipeOf(spell));
        }

        public static string[] NamesOf(IReadOnlyList<RuneId> runes)
        {
            if (runes == null || runes.Count == 0)
            {
                return System.Array.Empty<string>();
            }

            var names = new string[runes.Count];
            for (var i = 0; i < runes.Count; i++)
            {
                names[i] = RuneCatalog.NameOf(runes[i]);
            }

            return names;
        }

        public static RuneId[] ParseRecipe(string[] names)
        {
            return AuthoringUtil.ParseRunes(names);
        }

        public static CombatStrike StrikeOf(SpellId spell)
        {
            if (spell == SpellId.None)
            {
                return CombatStrike.None;
            }

            for (var i = 0; i < Pages.Length; i++)
            {
                if (Pages[i].Spell == spell && Pages[i].Strike != CombatStrike.None)
                {
                    return Pages[i].Strike;
                }
            }

            if (SpellCodex.TryGet(spell, out var entry))
            {
                if (entry.Shape == SpellShape.Pillar)
                {
                    return CombatStrike.Pillar;
                }

                if (entry.Shape == SpellShape.Shot)
                {
                    return CombatStrike.Shot;
                }
            }

            return CombatStrike.Shot;
        }

        public static CombatRange DefaultRange(SpellId spell, CombatStrike strike)
        {
            if (strike == CombatStrike.Slam)
            {
                return CombatRange.Close;
            }

            for (var i = 0; i < Pages.Length; i++)
            {
                if (Pages[i].Spell == spell && Pages[i].Strike != CombatStrike.None)
                {
                    return Pages[i].Range;
                }
            }

            return strike == CombatStrike.Pillar ? CombatRange.Mid : CombatRange.Long;
        }

        public static float DefaultCastSeconds(SpellId spell, CombatStrike strike)
        {
            if (strike == CombatStrike.Slam)
            {
                return 0.85f;
            }

            for (var i = 0; i < Pages.Length; i++)
            {
                if (Pages[i].Spell == spell && Pages[i].CastSeconds > 0f)
                {
                    return Pages[i].CastSeconds;
                }
            }

            return strike == CombatStrike.Pillar ? 2f : 1.6f;
        }

        public static void FillFromSpell(CombatSlot slot)
        {
            if (slot == null || slot.Spell == SpellId.None)
            {
                return;
            }

            var page = PageOf(slot.Spell, slot.Strike);
            slot.Recipe = RecipeNames(slot.Spell);
            if (slot.Strike == CombatStrike.None)
            {
                slot.Strike = page.Strike != CombatStrike.None ? page.Strike : StrikeOf(slot.Spell);
            }

            if (string.IsNullOrEmpty(slot.Name) || slot.Name == "None (write runes)")
            {
                slot.Name = page.Name;
            }

            if (slot.CastSeconds <= 0f)
            {
                slot.CastSeconds = page.CastSeconds > 0f ? page.CastSeconds : DefaultCastSeconds(slot.Spell, slot.Strike);
            }
        }

        public static void FillEmptyRecipes(CombatSlot[] slots)
        {
            if (slots == null)
            {
                return;
            }

            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null || slot.Spell == SpellId.None)
                {
                    continue;
                }

                if (slot.Recipe == null || slot.Recipe.Length == 0)
                {
                    FillFromSpell(slot);
                }
            }
        }

        public static void FillFromSpell(CombatGambit gambit)
        {
            if (gambit == null || gambit.ThenSpell == SpellId.None)
            {
                return;
            }

            if (gambit.ThenRecipe == null || gambit.ThenRecipe.Length == 0)
            {
                gambit.ThenRecipe = RecipeNames(gambit.ThenSpell);
            }

            if (gambit.ThenStrike == CombatStrike.None)
            {
                gambit.ThenStrike = StrikeOf(gambit.ThenSpell);
            }

            if (string.IsNullOrEmpty(gambit.Name))
            {
                gambit.Name = NameOf(gambit.ThenSpell, gambit.ThenStrike);
            }
        }

        public static ProjectileKind ShotKind(SpellId spell, IReadOnlyList<RuneId> recipe)
        {
            if (spell == SpellId.WoodArrow || spell == SpellId.Vine || WritesWood(recipe))
            {
                return ProjectileKind.Wood;
            }

            if (spell == SpellId.HurledStone || spell == SpellId.IceSpear || WritesEarth(recipe))
            {
                return ProjectileKind.Arrow;
            }

            return ProjectileKind.Fireball;
        }

        public static bool WritesWood(IReadOnlyList<RuneId> recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            var plant = false;
            var mercury = false;
            var life = false;
            var death = false;
            var fire = false;
            for (var i = 0; i < recipe.Count; i++)
            {
                switch (recipe[i])
                {
                    case RuneId.Plant:
                        plant = true;
                        break;
                    case RuneId.Mercury:
                        mercury = true;
                        break;
                    case RuneId.Vita:
                        life = true;
                        break;
                    case RuneId.Mors:
                        death = true;
                        break;
                    case RuneId.Fire:
                        fire = true;
                        break;
                }
            }

            return plant && mercury && !life && !death && !fire;
        }

        static bool WritesEarth(IReadOnlyList<RuneId> recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            var earth = false;
            var fire = false;
            var plant = false;
            for (var i = 0; i < recipe.Count; i++)
            {
                if (recipe[i] == RuneId.Earth || recipe[i] == RuneId.Stone)
                {
                    earth = true;
                }

                if (recipe[i] == RuneId.Fire)
                {
                    fire = true;
                }

                if (recipe[i] == RuneId.Plant)
                {
                    plant = true;
                }
            }

            return earth && !fire && !plant;
        }

        public static bool WritesFire(IReadOnlyList<RuneId> recipe)
        {
            if (recipe == null)
            {
                return false;
            }

            var fire = false;
            for (var i = 0; i < recipe.Count; i++)
            {
                if (recipe[i] == RuneId.Spark)
                {
                    return false;
                }

                if (recipe[i] == RuneId.Fire || recipe[i] == RuneId.Flame)
                {
                    fire = true;
                }
            }

            return fire;
        }

        public static RuneId ElementOf(SpellId spell, IReadOnlyList<RuneId> recipe)
        {
            if (recipe != null)
            {
                for (var i = 0; i < recipe.Count; i++)
                {
                    switch (recipe[i])
                    {
                        case RuneId.Fire:
                        case RuneId.Flame:
                        case RuneId.Water:
                        case RuneId.Earth:
                        case RuneId.Air:
                        case RuneId.Ice:
                        case RuneId.Plant:
                        case RuneId.Spark:
                        case RuneId.Lightning:
                        case RuneId.Lava:
                        case RuneId.Poison:
                            return recipe[i];
                    }
                }
            }

            var essence = ElementalLaw.Of(spell);
            switch (essence)
            {
                case Essence.Water:
                    return RuneId.Water;
                case Essence.Earth:
                    return RuneId.Earth;
                case Essence.Air:
                    return RuneId.Air;
                case Essence.Plant:
                    return RuneId.Plant;
                case Essence.Poison:
                    return RuneId.Poison;
                default:
                    return RuneId.Fire;
            }
        }

        public static CombatRange BandOf(float distance, float close, float mid, float longRange)
        {
            if (distance <= close + 0.15f)
            {
                return CombatRange.Close;
            }

            if (distance <= mid)
            {
                return CombatRange.Mid;
            }

            return CombatRange.Long;
        }

        public static float MaxOf(CombatRange range, float close, float mid, float longRange)
        {
            switch (range)
            {
                case CombatRange.Close:
                    return close;
                case CombatRange.Mid:
                    return mid;
                default:
                    return longRange;
            }
        }

        public static void ResolveBands(float close, float mid, float longRange, out float closeOut, out float midOut, out float longOut)
        {
            closeOut = close > 0.05f ? close : DefaultClose;
            midOut = mid > 0.05f ? mid : DefaultMid;
            longOut = longRange > 0.05f ? longRange : DefaultLong;
            if (midOut <= closeOut)
            {
                midOut = closeOut + 2.4f;
            }

            if (longOut <= midOut)
            {
                longOut = midOut + 3.2f;
            }
        }

        public static CombatMode ModeOf(CombatMode authored, CombatKind kind)
        {
            if (authored != CombatMode.Auto)
            {
                return authored;
            }

            switch (kind)
            {
                case CombatKind.Golem:
                    return CombatMode.Guard;
                case CombatKind.Wizard:
                case CombatKind.Archer:
                    return CombatMode.Caster;
                default:
                    return CombatMode.Wander;
            }
        }

        public static CombatKind KindFromSlots(CombatKind authored, CombatSlot[] slots)
        {
            if (authored != CombatKind.None)
            {
                return authored;
            }

            if (slots == null)
            {
                return CombatKind.None;
            }

            var slam = false;
            var wood = false;
            var ranged = false;
            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                if (slot.Strike == CombatStrike.Slam)
                {
                    slam = true;
                }

                if (slot.Strike == CombatStrike.Shot || slot.Strike == CombatStrike.Pillar)
                {
                    ranged = true;
                    if (slot.Spell == SpellId.WoodArrow || WritesWood(ParseRecipe(slot.Recipe)))
                    {
                        wood = true;
                    }
                }
            }

            if (ranged)
            {
                return wood && !slam ? CombatKind.Archer : CombatKind.Wizard;
            }

            return slam ? CombatKind.Golem : CombatKind.None;
        }

        public static CombatSlot[] SlotsFromKind(CombatKind kind, RuneId[] written, float castSeconds)
        {
            switch (kind)
            {
                case CombatKind.Golem:
                    var slam = SlamSlot();
                    if (castSeconds > 0.35f && castSeconds <= 2.01f)
                    {
                        slam.CastSeconds = 0.85f;
                    }
                    else if (castSeconds > 2.01f)
                    {
                        slam.CastSeconds = castSeconds;
                    }

                    return new[] { slam };
                case CombatKind.Wizard:
                    return new[]
                    {
                        ShotSlot(SpellId.Fireball, written, castSeconds > 0.35f ? castSeconds : 2f, CombatRange.Long)
                    };
                case CombatKind.Archer:
                    var seconds = castSeconds <= 2.01f ? 1.15f : castSeconds;
                    return new[]
                    {
                        ShotSlot(SpellId.WoodArrow, written, seconds, CombatRange.Long)
                    };
                default:
                    return System.Array.Empty<CombatSlot>();
            }
        }

        static CombatSlot ShotSlot(SpellId fallback, RuneId[] written, float castSeconds, CombatRange range)
        {
            var spell = fallback;
            var recipe = written != null && written.Length > 0 ? NamesOf(written) : RecipeNames(fallback);
            if (written != null && written.Length > 0)
            {
                if (WritesWood(written))
                {
                    spell = SpellId.WoodArrow;
                }
                else
                {
                    var exact = ChainBook.CollectExact(Composition.FromSequence(written), SpellShape.None);
                    if (exact.Count > 0)
                    {
                        spell = exact[0].Spell;
                    }
                    else if (WritesFire(written))
                    {
                        spell = SpellId.Fireball;
                    }
                }
            }

            return new CombatSlot
            {
                Name = NameOf(spell, CombatStrike.Shot),
                Range = range,
                Strike = CombatStrike.Shot,
                Spell = spell,
                Recipe = recipe,
                CastSeconds = castSeconds
            };
        }

        public static CombatPlan PlanFrom(
            CombatKind kind,
            CombatMode mode,
            float close,
            float mid,
            float longRange,
            CombatSlot[] slots,
            CombatGambit[] gambits,
            float castSeconds,
            RuneId[] legacyRecipe)
        {
            ResolveBands(close, mid, longRange, out var closeOut, out var midOut, out var longOut);
            var resolvedKind = KindFromSlots(kind, slots);
            var resolvedSlots = HasStrike(slots)
                ? CopySlots(slots)
                : SlotsFromKind(resolvedKind == CombatKind.None ? kind : resolvedKind, legacyRecipe, castSeconds);
            FillEmptyRecipes(resolvedSlots);
            FillEmptyGambits(gambits);
            var seconds = castSeconds > 0.35f ? castSeconds : 2f;
            if (resolvedKind == CombatKind.Golem && castSeconds <= 2.01f)
            {
                seconds = 0.85f;
            }

            if (resolvedKind == CombatKind.Archer && castSeconds <= 2.01f)
            {
                seconds = 1.15f;
            }

            return new CombatPlan
            {
                Kind = resolvedKind,
                Mode = ModeOf(mode, resolvedKind),
                CloseRange = closeOut,
                MidRange = midOut,
                LongRange = longOut,
                Slots = resolvedSlots,
                Gambits = gambits ?? System.Array.Empty<CombatGambit>(),
                CastSeconds = seconds
            };
        }

        public static CombatPlan PlanFromLegacy(CombatKind kind, float castSeconds, RuneId[] recipe)
        {
            return PlanFrom(kind, CombatMode.Auto, 0f, 0f, 0f, null, null, castSeconds, recipe);
        }

        static bool HasStrike(CombatSlot[] slots)
        {
            if (slots == null)
            {
                return false;
            }

            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && slots[i].Strike != CombatStrike.None)
                {
                    return true;
                }

                if (slots[i] != null && slots[i].Spell != SpellId.None)
                {
                    return true;
                }
            }

            return false;
        }

        static CombatSlot[] CopySlots(CombatSlot[] slots)
        {
            var copy = new CombatSlot[slots.Length];
            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                copy[i] = new CombatSlot
                {
                    Name = slot.Name,
                    Range = slot.Range,
                    Strike = slot.Strike == CombatStrike.None && slot.Spell != SpellId.None
                        ? StrikeOf(slot.Spell)
                        : slot.Strike,
                    Spell = slot.Spell,
                    Recipe = slot.Recipe,
                    CastSeconds = slot.CastSeconds
                };
            }

            return copy;
        }

        static void FillEmptyGambits(CombatGambit[] gambits)
        {
            if (gambits == null)
            {
                return;
            }

            for (var i = 0; i < gambits.Length; i++)
            {
                FillFromSpell(gambits[i]);
            }
        }

        public static CreatureNature NatureOf(AuthoredNature authored, string formulaId, bool ensouled)
        {
            switch (authored)
            {
                case AuthoredNature.Flesh:
                    return CreatureNature.Flesh;
                case AuthoredNature.Fire:
                    return CreatureNature.Fire;
                case AuthoredNature.Ice:
                    return CreatureNature.Ice;
                case AuthoredNature.Earth:
                    return CreatureNature.Earth;
                case AuthoredNature.Mind:
                    return CreatureNature.Mind;
                case AuthoredNature.Plant:
                    return CreatureNature.Plant;
                case AuthoredNature.Undead:
                    return CreatureNature.Undead;
                default:
                    return NatureFromId(formulaId, ensouled);
            }
        }

        public static CreatureNature NatureFromId(string formulaId, bool ensouled)
        {
            switch ((formulaId ?? string.Empty).ToLowerInvariant())
            {
                case "fire-golem":
                case "ash-mite":
                    return CreatureNature.Fire;
                case "ice-thing":
                    return CreatureNature.Ice;
                case "golem":
                case "stone-man":
                    return CreatureNature.Earth;
                case "warden":
                case "spirit-warden":
                    return ensouled ? CreatureNature.Mind : CreatureNature.Flesh;
                default:
                    return ensouled ? CreatureNature.Mind : CreatureNature.Flesh;
            }
        }

        public static AuthoredNature AuthoredOf(CreatureNature nature)
        {
            switch (nature)
            {
                case CreatureNature.Fire:
                    return AuthoredNature.Fire;
                case CreatureNature.Ice:
                    return AuthoredNature.Ice;
                case CreatureNature.Earth:
                    return AuthoredNature.Earth;
                case CreatureNature.Mind:
                    return AuthoredNature.Mind;
                case CreatureNature.Plant:
                    return AuthoredNature.Plant;
                case CreatureNature.Undead:
                    return AuthoredNature.Undead;
                default:
                    return AuthoredNature.Flesh;
            }
        }

        public static float SecondsOf(CombatSlot slot, float fallback)
        {
            if (slot != null && slot.CastSeconds > 0.35f)
            {
                return slot.CastSeconds;
            }

            if (slot != null)
            {
                return DefaultCastSeconds(slot.Spell, slot.Strike);
            }

            return Mathf.Max(0.35f, fallback);
        }

        public static bool IsRanged(CombatStrike strike)
        {
            return strike == CombatStrike.Shot || strike == CombatStrike.Pillar;
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            var golem = SlotsFromKind(CombatKind.Golem, null, 2f);
            if (golem.Length != 1 || golem[0].Strike != CombatStrike.Slam || golem[0].Range != CombatRange.Close)
            {
                broken.Add("A golem's default slot is a close slam");
            }

            var wizard = SlotsFromKind(CombatKind.Wizard, null, 2f);
            if (wizard.Length != 1
                || wizard[0].Strike != CombatStrike.Shot
                || wizard[0].Spell != SpellId.Fireball
                || !WritesFire(ParseRecipe(wizard[0].Recipe)))
            {
                broken.Add("A wizard's default slot is a long fireball with Fire · Mercury");
            }

            var spark = SlotsFromKind(CombatKind.Wizard, new[] { RuneId.Spark, RuneId.Mercury }, 2f);
            if (spark[0].Recipe == null || spark[0].Recipe.Length < 2 || spark[0].Recipe[0] != "Spark"
                || spark[0].Spell != SpellId.SparkShot)
            {
                broken.Add("A written wizard recipe must keep the author's marks");
            }

            var archer = SlotsFromKind(CombatKind.Archer, null, 2f);
            if (archer[0].Spell != SpellId.WoodArrow || archer[0].Range != CombatRange.Long)
            {
                broken.Add("An archer's default slot is a long wood arrow");
            }

            var fireball = RecipeOf(SpellId.Fireball);
            if (fireball.Length != 2 || fireball[0] != RuneId.Fire || fireball[1] != RuneId.Mercury)
            {
                broken.Add("Picking Fireball must write Fire · Mercury");
            }

            var pillar = RecipeOf(SpellId.FlamePillar);
            if (pillar.Length != 3 || pillar[0] != RuneId.Fire || pillar[1] != RuneId.Salt || pillar[2] != RuneId.Earth)
            {
                broken.Add("Picking Flame-pillar must write Fire · Salt · Earth");
            }

            if (ModeOf(CombatMode.Auto, CombatKind.Golem) != CombatMode.Guard
                || ModeOf(CombatMode.Auto, CombatKind.Wizard) != CombatMode.Caster
                || ModeOf(CombatMode.Hunt, CombatKind.Golem) != CombatMode.Hunt)
            {
                broken.Add("Auto mode is Guard for a golem and Caster for a wizard; Hunt stays Hunt");
            }

            ResolveBands(0f, 0f, 0f, out var close, out var mid, out var longRange);
            if (BandOf(0.8f, close, mid, longRange) != CombatRange.Close
                || BandOf(3f, close, mid, longRange) != CombatRange.Mid
                || BandOf(7f, close, mid, longRange) != CombatRange.Long
                || MaxOf(CombatRange.Mid, close, mid, longRange) != mid)
            {
                broken.Add("Close / mid / long bands must split slam reach from a standing shot");
            }

            if (ShotKind(SpellId.WoodArrow, null) != ProjectileKind.Wood
                || ShotKind(SpellId.HurledStone, null) != ProjectileKind.Arrow
                || ShotKind(SpellId.Fireball, null) != ProjectileKind.Fireball)
            {
                broken.Add("Shot kind follows the spell: wood, stone, hunger");
            }

            if (NatureOf(AuthoredNature.Auto, "golem", false) != CreatureNature.Earth
                || NatureOf(AuthoredNature.Fire, "golem", false) != CreatureNature.Fire
                || NatureOf(AuthoredNature.Auto, "warden", true) != CreatureNature.Mind)
            {
                broken.Add("Nature Auto reads the id; an authored nature wins");
            }

            var slot = new CombatSlot { Spell = SpellId.FlamePillar };
            FillFromSpell(slot);
            if (slot.Strike != CombatStrike.Pillar
                || slot.Recipe == null
                || slot.Recipe.Length != 3
                || slot.Recipe[1] != "Salt")
            {
                broken.Add("Selecting a spell must fill its runes and strike");
            }

            var gambit = WallToFlamePillar();
            var fromGambit = SlotFromGambit(gambit);
            if (fromGambit == null
                || fromGambit.Strike != CombatStrike.Pillar
                || fromGambit.Spell != SpellId.FlamePillar
                || !WritesFire(ParseRecipe(fromGambit.Recipe)))
            {
                broken.Add("A wall gambit must write a flame-pillar");
            }

            var mixed = PlanFrom(
                CombatKind.Wizard,
                CombatMode.Auto,
                0f, 0f, 0f,
                null,
                new[] { gambit },
                2f,
                null);
            if (mixed.Mode != CombatMode.Caster
                || mixed.Slots.Length != 1
                || mixed.Gambits.Length != 1
                || mixed.Gambits[0].ThenSpell != SpellId.FlamePillar)
            {
                broken.Add("A wizard plan keeps the fireball slot and the wall gambit");
            }

            if (KindFromSlots(CombatKind.None, new[] { SlamSlot() }) != CombatKind.Golem
                || KindFromSlots(CombatKind.None, wizard) != CombatKind.Wizard)
            {
                broken.Add("Kind follows the slots when Attack is empty");
            }

            var stone = AffinityProfile.Of(CreatureNature.Earth);
            var stubborn = stone.WithOverrides(
                null,
                null,
                new[] { new StrikeAffinity { Kind = StrikeKind.Fire, Affinity = 0 } },
                new[] { new StatusAffinity { Status = StatusId.Charmed, Affinity = 0 } });
            if (stubborn.Defense != 4
                || stubborn.Strike(StrikeKind.Fire) != 0
                || stubborn.Strike(StrikeKind.Lava) != 2
                || stubborn.Status(StatusId.Charmed) != 0
                || StrikeLaw.Kills(SpellId.Fireball, stubborn)
                || !StrikeLaw.Kills(SpellId.LightningStrike, stubborn))
            {
                broken.Add("Inspector affinities must override a nature without wiping the rest of the row");
            }

            var tank = stone.WithOverrides(6, null, null, null);
            if (tank.Defense != 6
                || StrikeLaw.Kills(SpellId.LightningStrike, tank)
                || !StrikeLaw.Kills(SpellId.Witchfire, tank))
            {
                broken.Add("Inspector defense must stand above a bolt and still fall to witchfire");
            }
        }
    }
}
