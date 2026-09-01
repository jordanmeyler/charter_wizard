using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// How a condition lives on a body.
    /// Timed clocks lift. Focus holds until a mark is reused.
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
    /// Sulphur work does not use this — it stays until focus breaks.
    /// </summary>
    public static class VitalLaw
    {
        public const float AdeptBurnSeconds = 8f;
        public const float AdeptPoisonSeconds = 6f;
        public const float FleshBurnSeconds = 6f;
        public const float FleshPoisonSeconds = 6f;
        public const float IceBurnSeconds = 4f;
        public const float EarthBurnSeconds = 12f;
        /// <summary>
        /// Fuel clocks are one to five seconds. Oil is the short
        /// end; a slow body (grove, ember) is the long end and
        /// does not carry the flame. Wood finishes before plant —
        /// a green body lasts, even if it catches readily.
        /// </summary>
        public const float OilBurnSeconds = 1f;
        public const float TimberBurnSeconds = 2f;
        public const float PlantBurnSeconds = 3f;
        public const float GroveBurnSeconds = 4f;
        public const float EmberBurnSeconds = 5f;
        public const float SlowBurnSeconds = 4f;

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

        public static float ItemBurnSeconds(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Oil:
                    return OilBurnSeconds;
                case MaterialId.Plant:
                case MaterialId.Moss:
                    return PlantBurnSeconds;
                case MaterialId.Grove:
                    return GroveBurnSeconds;
                case MaterialId.Timber:
                    return TimberBurnSeconds;
                default:
                    return 0f;
            }
        }

        /// <summary>
        /// How hard a standing fire runs from a body. Faster fuel
        /// spreads; four seconds and slower stay put.
        /// </summary>
        public static float FireRun(float burnSeconds)
        {
            if (burnSeconds <= 0f || burnSeconds >= SlowBurnSeconds)
            {
                return 0f;
            }

            return SlowBurnSeconds - burnSeconds;
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

            if (BurnSeconds(CreatureNature.Flesh, true) != AdeptBurnSeconds
                || BurnSeconds(CreatureNature.Fire, false) > 0f
                || PoisonSeconds(CreatureNature.Earth, false) > 0f
                || !CanBurn(MaterialId.Timber)
                || CanBurn(MaterialId.Stone))
            {
                broken.Add("Burn and poison capacities must follow nature and matter");
            }

            if (OilBurnSeconds != 1f
                || TimberBurnSeconds != 2f
                || PlantBurnSeconds != 3f
                || GroveBurnSeconds < SlowBurnSeconds
                || EmberBurnSeconds > 5f
                || ItemBurnSeconds(MaterialId.Oil) != OilBurnSeconds
                || ItemBurnSeconds(MaterialId.Timber) != TimberBurnSeconds
                || ItemBurnSeconds(MaterialId.Plant) != PlantBurnSeconds
                || FireRun(OilBurnSeconds) <= FireRun(TimberBurnSeconds)
                || FireRun(GroveBurnSeconds) > 0f
                || FireRun(EmberBurnSeconds) > 0f)
            {
                broken.Add("Fuel clocks are 1–5s: oil, wood, plant; slow bodies do not spread");
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
