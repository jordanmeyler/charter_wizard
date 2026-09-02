using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// How a condition lives on a body.
        /// Timed clocks lift. Focus holds until another focus
        /// sentence reuses a mark other than Sulphur.
    /// Meters run down; empty is death or ash.
    /// </summary>
    public enum StatusClock
    {
        Timed,
        Focus,
        Meter
    }

    /// <summary>
    /// Burning and poison share one law: a named integrity clock.
    /// The clock only runs while the body still stands in matching
    /// fire or foul. Step off that cover and the condition lifts —
    /// you are no longer on fire, no longer poisoned. Sulphur work
    /// does not use this — it stays until focus breaks.
    /// </summary>
    public static class VitalLaw
    {
        public const float AdeptBurnSeconds = 8f;
        public const float AdeptPoisonSeconds = 14f;
        public const float FleshBurnSeconds = 6f;
        public const float FleshPoisonSeconds = 14f;
        public const float IceBurnSeconds = 4f;
        public const float EarthBurnSeconds = 12f;
        /// <summary>
        /// Fuel clocks are one to five seconds, inverted: oil and
        /// wood last, plant and grove burn out sooner. Leftover is
        /// 5 − seconds, floored at 0. Spread uses Hunger, not this
        /// leftover. Ember hosts fire and is a path, not fuel.
        /// </summary>
        public const float OilBurnSeconds = 5f;
        public const float TimberBurnSeconds = 4f;
        public const float PlantBurnSeconds = 3f;
        public const float GroveBurnSeconds = 2f;
        public const float TinderBurnSeconds = 2f;
        public const float SlowBurnSeconds = 5f;
        /// <summary>
        /// Hunger stood without rest (Fire · Salt). No fuel, kindled
        /// hall, geyser, or rest-fire walk under it — the column
        /// goes out on this clock.
        /// </summary>
        public const float FirePillarSeconds = 3f;

        /// <summary>
        /// One 0–10 hunger grade. Catch and spread use this range.
        /// Burn seconds stay their own 1–5 clock.
        /// 0       Neutral — spell volume only. Stone, dirt, metal.
        /// 1–2     Tinder — dust / fire cover (2). 1 is open. Catch-only.
        /// 3–4     Soft — moss (3), grove (4). Catch-only.
        /// 5–6     Plant — living plant (6). Catches from a strong
        ///         source. Does not run. 5 is free for later fuel.
        /// 7–8     Timber — wood (8). A strong source: fire may walk
        ///         to equal-or-weaker fuel out to hunger − 6. 7 is
        ///         free for brush.
        /// 9–10    Oil / a kindled hall (10). Strong source. 9 is free
        ///         for later pitch / grease.
        /// </summary>
        public const int HungerNeutral = 0;
        public const int HungerEmber = 1;
        public const int HungerTinder = 2;
        public const int HungerMoss = 3;
        public const int HungerSoft = 4;
        public const int HungerPlant = 6;
        public const int HungerTimber = 8;
        public const int HungerOil = 10;
        public const int HungerMax = 10;
        public const int HungerSpreadMin = 7;

        /// <summary>
        /// How far a live source may walk, in Chebyshev tiles.
        /// Strong sources use the grade directly: reach = hunger − 6
        /// (timber 8 → 2, oil / hall 10 → 4). Weaker fuel only feeds
        /// a vine wick on the next tile.
        /// </summary>
        public static int CatchReach(int sourceHunger)
        {
            if (!IsStrongSource(sourceHunger))
            {
                return 1;
            }

            return sourceHunger - (HungerSpreadMin - 1);
        }

        /// <summary>
        /// One 0–10 quench grade. The wet counterpart of Hunger.
        /// Dry stone is 0 — it leaves a fire alone. Mud suppresses.
        /// Water puts the fire out.
        /// 0       Dry — stone, dirt, timber, oil. No neighbor effect.
        /// 1–2     Trace moisture — salt crust (1). Below suppress.
        /// 3–4     Mud / damp — mud (3), damp stone (4). Suppresses
        ///         neighbor fire: no spread, the clock runs down sooner.
        /// 5–6     Ice / snow / glacier. Melts, then wets. Suppresses.
        /// 7–8     Rain (7). Strong suppress. 8 is free for shallow water.
        /// 9–10    Water / flood (10). Puts fire out on the cell and
        ///         on adjacent fuel (oil and plant-on-water still ignore it).
        /// </summary>
        public const int QuenchDry = 0;
        public const int QuenchSalt = 1;
        public const int QuenchMud = 3;
        public const int QuenchDamp = 4;
        public const int QuenchIce = 5;
        public const int QuenchGlacier = 6;
        public const int QuenchRain = 7;
        public const int QuenchWater = 10;
        public const int QuenchMax = 10;
        public const int QuenchSuppressMin = 3;
        public const int QuenchSnuffMin = 9;
        /// <summary>
        /// Extra fire drain per neighbor quench grade each sim step.
        /// Four mud tiles (sum 12) smother a timber clock; dry stone
        /// adds nothing, so the same timber burns its full seconds.
        /// </summary>
        public const float QuenchDrainPerGrade = 0.06f;

        /// <summary>
        /// One 0–10 conduct grade. Hold and spread use this range,
        /// the same shape as Hunger.
        /// 0       Insulator — wood, plants, oil. Refuse the spark.
        ///         Break a neighbor's path.
        /// 1–3     Poor hold — stone, dirt, sand, ash, ice. A bolt
        ///         or live-floor still charges the cell for one
        ///         second. It will not walk.
        /// 4–6     Weak — salt, mud, damp, crystal, lava, acid.
        ///         Hold a breath longer. Still no neighbor walk.
        /// 7–10    Conductor — rain (7), vein / aegis (8), water (9),
        ///         metal (10). Hold and spread onto other conductors.
        /// </summary>
        public const int ConductInsulator = 0;
        public const int ConductPoor = 2;
        public const int ConductSalt = 4;
        public const int ConductDamp = 5;
        public const int ConductAcid = 6;
        public const int ConductRain = 7;
        public const int ConductVein = 8;
        public const int ConductWater = 9;
        public const int ConductMetal = 10;
        public const int ConductMax = 10;
        public const int ConductSpreadMin = 7;
        /// <summary>
        /// How long a poor floor (stone) keeps a spark after a
        /// bolt or live-floor. Occupants stay stunned while the
        /// cell is live, then one more second after they step off.
        /// </summary>
        public const float ChargeHoldSeconds = 1f;
        public const float ChargeContactSeconds = 1f;

        public static StatusClock ClockOf(StatusId id)
        {
            if (id == StatusId.Burning || id == StatusId.Poisoned)
            {
                return StatusClock.Meter;
            }

            return StatusSpec.Of(id).NeedsConcentration ? StatusClock.Focus : StatusClock.Timed;
        }

        public static bool IsMeter(StatusId id) =>
            ClockOf(id) == StatusClock.Meter;

        /// <summary>
        /// Hunger lifts off the fire. Poison does not — it stays
        /// until Light cleanses it, and the clock still runs.
        /// </summary>
        public static bool MeterEndsWithoutContact(StatusId id) =>
            id == StatusId.Burning;

        /// <summary>
        /// Burning and poison only hold while the body still stands in
        /// that kind of walk or covering. Hunger needs live fire, a
        /// kindled hall, ember, or a stood flame — a painted fire mark
        /// at rest is not enough. Poison needs a poison slick underfoot,
        /// or a miasma cloud (the tile, a neighbour, or a hanging
        /// veil). Leave the tile — or lift the feet — and the
        /// condition resets.
        /// </summary>
        public static bool ContactFeeds(StatusId id, WorldGrid grid, Vector3 world, bool airborne)
        {
            if (!IsMeter(id))
            {
                return true;
            }

            if (airborne)
            {
                return false;
            }

            var tile = grid != null ? grid.TileAtWorld(world) : null;
            if (id == StatusId.Burning)
            {
                return IsFireContact(tile);
            }

            if (id == StatusId.Poisoned)
            {
                return IsPoisonLiquidContact(tile) || WorldPhysics.MiasmaCloudAt(grid, world);
            }

            return false;
        }

        public static bool IsFireContact(WorldTile tile)
        {
            if (tile == null)
            {
                return false;
            }

            return tile.IsBurning
                || tile.LiveFire
                || tile.Kindled
                || tile.HasEmber
                || WorldWork.BurnsOccupants(tile);
        }

        public static bool IsChargeContact(WorldTile tile) =>
            tile != null && tile.IsCharged;

        public static bool IsPoisonLiquidContact(WorldTile tile) =>
            tile != null && tile.IsPoisonWater;

        public static float Seconds(StatusId id, CreatureNature nature, bool adept)
        {
            if (id == StatusId.Burning)
            {
                return BurnSeconds(nature, adept);
            }

            if (id == StatusId.Poisoned)
            {
                return PoisonSeconds(nature, adept);
            }

            return 0f;
        }

        public static float BurnSeconds(CreatureNature nature, bool adept)
        {
            if (adept)
            {
                return AdeptBurnSeconds;
            }

            switch (nature)
            {
                case CreatureNature.Fire:
                    return 0f;
                case CreatureNature.Ice:
                    return IceBurnSeconds;
                case CreatureNature.Earth:
                    return EarthBurnSeconds;
                default:
                    return FleshBurnSeconds;
            }
        }

        public static float PoisonSeconds(CreatureNature nature, bool adept)
        {
            if (nature == CreatureNature.Fire
                || nature == CreatureNature.Ice
                || nature == CreatureNature.Earth)
            {
                return 0f;
            }

            return adept ? AdeptPoisonSeconds : FleshPoisonSeconds;
        }

        public static bool CanBurn(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Timber:
                case MaterialId.Plant:
                case MaterialId.Grove:
                case MaterialId.Moss:
                case MaterialId.Oil:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Ember lets fire walk across and sit on the mark. Crossing
        /// it is not a leap — the path is still touching toward the
        /// source. Hunger stays 0; the tile underneath does not leftover.
        /// </summary>
        public static bool ConductsFire(MaterialId material) =>
            material == MaterialId.Ember;

        /// <summary>
        /// A cell that can catch from a potent source. Neutral walk
        /// and rest fire in the floor do not. A spell can still hit
        /// those cells.
        /// </summary>
        public static bool IsSpreadFuel(
            MaterialId walk,
            MaterialId detail = MaterialId.None,
            bool vine = false,
            bool oil = false)
        {
            if (IsRestFire(walk))
            {
                return false;
            }

            return HungerOf(walk) > HungerNeutral
                || HungerOf(detail) > HungerNeutral
                || vine
                || oil;
        }

        /// <summary>
        /// Catalog hunger for a material. Set it on
        /// <c>MaterialCatalog.Flag(..., hunger)</c> when you add a body.
        /// </summary>
        public static int HungerOf(MaterialId material)
        {
            if (material == MaterialId.None)
            {
                return HungerNeutral;
            }

            return Mathf.Clamp(MaterialCatalog.Of(material).Hunger, HungerNeutral, HungerMax);
        }

        /// <summary>
        /// Catalog quench for a material. Set it on
        /// <c>MaterialCatalog.Flag(..., hunger, quench)</c> when you
        /// add a wet body. Omit it and the body stays dry (0).
        /// </summary>
        public static int QuenchOf(MaterialId material)
        {
            if (material == MaterialId.None)
            {
                return QuenchDry;
            }

            return Mathf.Clamp(MaterialCatalog.Of(material).Quench, QuenchDry, QuenchMax);
        }

        /// <summary>
        /// Catalog conduct for a material. Set it on
        /// <c>MaterialCatalog.Flag(..., hunger, quench, conduct)</c>
        /// when you add a body. Omit it and the leftover
        /// conductivity number is read into a grade.
        /// </summary>
        public static int ConductOf(MaterialId material)
        {
            if (material == MaterialId.None)
            {
                return ConductPoor;
            }

            return Mathf.Clamp(MaterialCatalog.Of(material).Conduct, ConductInsulator, ConductMax);
        }

        public static bool Insulates(int conduct) =>
            conduct <= ConductInsulator;

        public static bool HoldsCharge(int conduct) =>
            conduct > ConductInsulator;

        public static bool SpreadsCharge(int conduct) =>
            conduct >= ConductSpreadMin;

        /// <summary>
        /// How long a full spark lasts on this grade. Poor stone
        /// is one second. Metal holds three. Insulators refuse.
        /// </summary>
        public static float ChargeHold(int conduct)
        {
            if (Insulates(conduct))
            {
                return 0f;
            }

            if (!SpreadsCharge(conduct))
            {
                return ChargeHoldSeconds;
            }

            return ChargeHoldSeconds + (conduct - (ConductSpreadMin - 1)) * 0.5f;
        }

        /// <summary>
        /// A strong source is Hunger 7+. Only those may walk fire to
        /// a neighbor, and only onto flammable grades at or below them.
        /// </summary>
        public static bool IsStrongSource(int hunger) =>
            hunger >= HungerSpreadMin;

        public static bool SpreadsFire(int hunger) =>
            IsStrongSource(hunger);

        public static bool SuppressesFire(int quench) =>
            quench >= QuenchSuppressMin;

        public static bool SnuffsFire(int quench) =>
            quench >= QuenchSnuffMin;

        public static bool BlocksCatch(int quench) =>
            quench >= QuenchSuppressMin;

        /// <summary>
        /// Suggested negative flam for a quench grade. Water 10 → −1.6.
        /// New wet bodies can use this so the leftover flam number
        /// stays in step with the 0–10 grade.
        /// </summary>
        public static float FlamFromQuench(int quench) =>
            quench <= QuenchDry ? 0f : -quench * 0.16f;

        /// <summary>
        /// Whether a live source may light a target this many tiles
        /// away (Chebyshev). A strong source (7+) may spread to any
        /// flammable grade at or below it, out to
        /// <see cref="CatchReach"/> (the hunger grade itself). The
        /// world also requires the target to touch fuel toward the
        /// source — fire does not leap a stone gap. Weaker fuel does
        /// not walk fire. A vine wick takes any adjacent live flame.
        /// Neutral never catches here.
        /// </summary>
        public static bool CanIgnite(int sourceHunger, int targetHunger, int chebyshev, bool vineWick)
        {
            if (targetHunger <= HungerNeutral || chebyshev <= 0)
            {
                return false;
            }

            if (vineWick && chebyshev == 1)
            {
                return true;
            }

            if (!IsStrongSource(sourceHunger) || chebyshev > CatchReach(sourceHunger))
            {
                return false;
            }

            return targetHunger <= sourceHunger;
        }

        /// <summary>
        /// Floor-Fire, hearth, lava. Rest hunger in the walk.
        /// It does not burn out. Coverings and spells are what react.
        /// Ember is a Fire-speaking mark that hosts fire, not rest fire.
        /// </summary>
        public static bool IsRestFire(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Fire:
                case MaterialId.Hearth:
                case MaterialId.Lava:
                    return true;
                default:
                    return false;
            }
        }

        public static float ItemBurnSeconds(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Oil:
                    return OilBurnSeconds;
                case MaterialId.Timber:
                    return TimberBurnSeconds;
                case MaterialId.Plant:
                case MaterialId.Moss:
                    return PlantBurnSeconds;
                case MaterialId.Grove:
                    return GroveBurnSeconds;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// Clock leftover: 5 − seconds. Oil 0, wood 1, plant 2,
        /// grove 3. Not what walks fire — Hunger does.
        /// Flammability is a separate catch number.
        /// </summary>
        public static float FireRun(float burnSeconds)
        {
            if (burnSeconds <= 0f)
            {
                return 0f;
            }

            var run = SlowBurnSeconds - burnSeconds;
            return run > 0f ? run : 0f;
        }

        public static string FatalNote(StatusId id, string who, bool adept)
        {
            if (id == StatusId.Burning)
            {
                return adept
                    ? "Hunger takes the body. Eight breaths without yield."
                    : string.IsNullOrEmpty(who)
                        ? "Hunger burns them to ash."
                        : $"{who} burns to ash.";
            }

            if (id == StatusId.Poisoned)
            {
                return adept
                    ? "The foul breath finishes its work."
                    : string.IsNullOrEmpty(who)
                        ? "They cannot hold the foul breath. They fall."
                        : $"{who} cannot hold the foul breath. They fall.";
            }

            return adept ? "You fall." : "They fall.";
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (ClockOf(StatusId.Burning) != StatusClock.Meter
                || ClockOf(StatusId.Poisoned) != StatusClock.Meter)
            {
                broken.Add("Burning and poison must be meters that run to death");
            }

            if (AdeptPoisonSeconds < 12f || FleshPoisonSeconds < 12f)
            {
                broken.Add("Poison must take longer than a short burn to finish its work");
            }

            if (ClockOf(StatusId.Charmed) != StatusClock.Focus
                || ClockOf(StatusId.Sleeping) != StatusClock.Focus
                || ClockOf(StatusId.Stoneskin) != StatusClock.Focus)
            {
                broken.Add("Sulphur work and wards must stay on focus, not a meter");
            }

            if (ClockOf(StatusId.Frozen) != StatusClock.Timed
                || ClockOf(StatusId.Stunned) != StatusClock.Timed
                || ClockOf(StatusId.Rooted) != StatusClock.Timed)
            {
                broken.Add("Frost, stun, and root must lift on a clock, not kill");
            }

            if (ContactFeeds(StatusId.Burning, null, default, true)
                || ContactFeeds(StatusId.Poisoned, null, default, true)
                || ContactFeeds(StatusId.Frozen, null, default, true) == false)
            {
                broken.Add("Meters lift when the feet leave the matching walk; timed clocks still lift");
            }

            if (!MeterEndsWithoutContact(StatusId.Burning)
                || MeterEndsWithoutContact(StatusId.Poisoned)
                || MeterEndsWithoutContact(StatusId.Frozen)
                || MeterEndsWithoutContact(StatusId.Stunned))
            {
                broken.Add("Burning lifts off the fire; poison stays until Light cleanses it");
            }

            if (IsFireContact(null) || IsPoisonLiquidContact(null) || IsChargeContact(null))
            {
                broken.Add("Empty ground cannot feed a burn, poison, or charge contact");
            }

            if (FireRun(0f) != 0f || FireRun(SlowBurnSeconds) != 0f)
            {
                broken.Add("Zero fuel and a full five-second clock leftover stay put — they do not run hunger");
            }

            if (SpellVerb.Of(SpellId.Poison).Tiles != TileVerb.Poison
                || SpellVerb.Of(SpellId.Miasma).Tiles != TileVerb.Foul
                || SpellVerb.Of(SpellId.Blight).Tiles != TileVerb.Foul)
            {
                broken.Add("Poison must slick a liquid; blight and miasma must foul a cloud");
            }

            if (WorldWork.IsPoisonVeil(SpellId.Poison)
                || !WorldWork.IsPoisonVeil(SpellId.Miasma)
                || !WorldWork.ClearsVeil(SpellId.Gust, VeilKind.Poison)
                || WorldWork.ClearsVeil(SpellId.Fireball, VeilKind.Poison)
                || WorldWork.ClearsVeil(SpellId.Douse, VeilKind.Poison))
            {
                broken.Add("Miasma is a cloud wind must take; poison is a liquid, not a veil");
            }

            if (BurnSeconds(CreatureNature.Flesh, true) != AdeptBurnSeconds
                || BurnSeconds(CreatureNature.Fire, false) > 0f
                || PoisonSeconds(CreatureNature.Earth, false) > 0f
                || !CanBurn(MaterialId.Timber)
                || CanBurn(MaterialId.Stone)
                || CanBurn(MaterialId.Fire)
                || CanBurn(MaterialId.Lava)
                || CanBurn(MaterialId.Ember)
                || !IsRestFire(MaterialId.Fire)
                || !IsRestFire(MaterialId.Lava)
                || !IsRestFire(MaterialId.Hearth)
                || IsRestFire(MaterialId.Ember)
                || IsRestFire(MaterialId.Timber))
            {
                broken.Add("Burn and poison capacities must follow nature and matter");
            }

            if (IsSpreadFuel(MaterialId.Stone)
                || IsSpreadFuel(MaterialId.Dirt)
                || IsSpreadFuel(MaterialId.Fire)
                || IsSpreadFuel(MaterialId.Ember)
                || ConductsFire(MaterialId.Stone)
                || ConductsFire(MaterialId.Dirt)
                || !ConductsFire(MaterialId.Ember)
                || !IsSpreadFuel(MaterialId.Timber)
                || !IsSpreadFuel(MaterialId.Oil)
                || !IsSpreadFuel(MaterialId.Plant)
                || !IsSpreadFuel(MaterialId.Stone, MaterialId.Timber)
                || !IsSpreadFuel(MaterialId.Dirt, MaterialId.None, true, false)
                || !IsSpreadFuel(MaterialId.Stone, MaterialId.None, false, true))
            {
                broken.Add("Neighbor fire only takes timber, oil, plant, or a wick — ember hosts fire but is not fuel");
            }

            if (HungerOf(MaterialId.Stone) != HungerNeutral
                || HungerOf(MaterialId.Ember) != HungerNeutral
                || MaterialCatalog.Of(MaterialId.Ember).BurnSeconds != 0f
                || MaterialCatalog.Of(MaterialId.Ember).Manifestation != RuneId.Fire
                || !MaterialCatalog.IsStampable(MaterialId.Ember)
                || HungerOf(MaterialId.Dust) != HungerTinder
                || HungerOf(MaterialId.Moss) != HungerMoss
                || HungerOf(MaterialId.Grove) != HungerSoft
                || HungerOf(MaterialId.Plant) != HungerPlant
                || HungerOf(MaterialId.Timber) != HungerTimber
                || HungerOf(MaterialId.Oil) != HungerOil
                || IsStrongSource(HungerPlant)
                || !IsStrongSource(HungerTimber)
                || CatchReach(HungerTimber) != 2
                || CatchReach(HungerOil) != 4
                || CatchReach(HungerPlant) != 1
                || CanIgnite(HungerPlant, HungerSoft, 2, false)
                || CanIgnite(HungerPlant, HungerPlant, 1, false)
                || !CanIgnite(HungerTimber, HungerTimber, 1, false)
                || !CanIgnite(HungerTimber, HungerPlant, 2, false)
                || !CanIgnite(HungerOil, HungerTimber, 1, false)
                || !CanIgnite(HungerOil, HungerOil, 1, false)
                || !CanIgnite(HungerOil, HungerPlant, 3, false)
                || CanIgnite(HungerTimber, HungerPlant, 3, false)
                || !CanIgnite(HungerTinder, HungerPlant, 1, true))
            {
                broken.Add("Hunger 0–10: a strong source (7+) walks fire to equal-or-weaker fuel out to its own reach; it does not leap a gap");
            }

            if (QuenchOf(MaterialId.Stone) != QuenchDry
                || QuenchOf(MaterialId.Dirt) != QuenchDry
                || QuenchOf(MaterialId.Timber) != QuenchDry
                || QuenchOf(MaterialId.Mud) != QuenchMud
                || QuenchOf(MaterialId.Damp) != QuenchDamp
                || QuenchOf(MaterialId.Ice) != QuenchIce
                || QuenchOf(MaterialId.Rain) != QuenchRain
                || QuenchOf(MaterialId.Water) != QuenchWater
                || SuppressesFire(QuenchDry)
                || !SuppressesFire(QuenchMud)
                || SnuffsFire(QuenchMud)
                || SnuffsFire(QuenchRain)
                || !SnuffsFire(QuenchWater)
                || BlocksCatch(QuenchSalt)
                || !BlocksCatch(QuenchMud)
                || FlamFromQuench(QuenchWater) > -1.55f)
            {
                broken.Add("Quench 0–10: dry stone leaves fire alone; mud suppresses; water puts it out");
            }

            if (ConductOf(MaterialId.Timber) != ConductInsulator
                || ConductOf(MaterialId.Plant) != ConductInsulator
                || ConductOf(MaterialId.Stone) != ConductPoor
                || ConductOf(MaterialId.Dirt) != ConductPoor
                || ConductOf(MaterialId.Damp) != ConductDamp
                || ConductOf(MaterialId.Rain) != ConductRain
                || ConductOf(MaterialId.Vein) != ConductVein
                || ConductOf(MaterialId.Water) != ConductWater
                || ConductOf(MaterialId.Metal) != ConductMetal
                || HoldsCharge(ConductInsulator)
                || !HoldsCharge(ConductPoor)
                || SpreadsCharge(ConductPoor)
                || SpreadsCharge(ConductDamp)
                || !SpreadsCharge(ConductRain)
                || ChargeHold(ConductInsulator) != 0f
                || ChargeHold(ConductPoor) != ChargeHoldSeconds
                || ChargeHold(ConductMetal) < ChargeHoldSeconds * 2f)
            {
                broken.Add("Conduct 0–10: wood refuses, stone holds one second, metal and water walk the spark");
            }

            if (OilBurnSeconds != SlowBurnSeconds
                || TimberBurnSeconds != SlowBurnSeconds - 1f
                || PlantBurnSeconds != 3f
                || GroveBurnSeconds != TinderBurnSeconds
                || TinderBurnSeconds != 2f
                || OilBurnSeconds <= TimberBurnSeconds
                || TimberBurnSeconds <= PlantBurnSeconds
                || PlantBurnSeconds <= GroveBurnSeconds
                || ItemBurnSeconds(MaterialId.Oil) != OilBurnSeconds
                || ItemBurnSeconds(MaterialId.Timber) != TimberBurnSeconds
                || ItemBurnSeconds(MaterialId.Plant) != PlantBurnSeconds
                || ItemBurnSeconds(MaterialId.Moss) != PlantBurnSeconds
                || ItemBurnSeconds(MaterialId.Grove) != GroveBurnSeconds
                || FireRun(OilBurnSeconds) != SlowBurnSeconds - OilBurnSeconds
                || FireRun(TimberBurnSeconds) != SlowBurnSeconds - TimberBurnSeconds
                || FireRun(PlantBurnSeconds) != SlowBurnSeconds - PlantBurnSeconds
                || FireRun(GroveBurnSeconds) != SlowBurnSeconds - GroveBurnSeconds
                || FireRun(SlowBurnSeconds) != 0f)
            {
                broken.Add("Fuel clocks last longer on oil and wood: oil 5, wood 4, plant 3, grove 2");
            }

            if (SpellCodex.TryGet(SpellId.Vine, out var vine) && vine.Shape != SpellShape.Shot)
            {
                broken.Add("Vine must be the vegetable body sent — a shot from the adept");
            }

            if (!WorldWork.StopsOnWalls(SpellId.Vine)
                || !WorldWork.IsVineWork(SpellId.Vine)
                || !WorldPhysics.SweepsPath(SpellId.Vine, SpellShape.Shot))
            {
                broken.Add("Vine must fly a line, stop on a wall, and leave a wick");
            }

            if (SpellVerb.Of(SpellId.Vine).Tiles != TileVerb.Vine
                || SpellVerb.Of(SpellId.Vine).Status != StatusId.Rooted)
            {
                broken.Add("Vine must lay a climbing body and hold what it crosses");
            }
        }
    }
}
