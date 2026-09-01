using System.Collections.Generic;
using UnityEngine;

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
    /// The clock only runs while the body still stands in matching
    /// fire or foul. Step off that cover and the condition lifts —
    /// you are no longer on fire, no longer poisoned. Sulphur work
    /// does not use this — it stays until focus breaks.
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
        /// Fuel clocks are one to five seconds. Wood burns better
        /// than plant. Spread is 5 − seconds, floored at 0.
        /// Ember (5s) stays put; nothing goes negative.
        /// </summary>
        public const float OilBurnSeconds = 1f;
        public const float TimberBurnSeconds = 2f;
        public const float PlantBurnSeconds = 3f;
        public const float GroveBurnSeconds = 4f;
        public const float EmberBurnSeconds = 5f;
        public const float SlowBurnSeconds = 5f;

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
        /// Meters do not linger. Off the matching fire or foul, the
        /// status drops instead of pausing.
        /// </summary>
        public static bool MeterEndsWithoutContact(StatusId id) =>
            IsMeter(id);

        /// <summary>
        /// Burning and poison only hold while the body still stands in
        /// that kind of walk or covering. Hunger needs fire floor,
        /// fire cover, or a live flame. Poison needs a poison slick
        /// underfoot, or a miasma cloud (the tile, a neighbour, or a
        /// hanging veil). Leave the tile — or lift the feet — and
        /// the condition resets.
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
                || tile.HasFireCover
                || tile.IsFireFloor
                || tile.Kindled
                || WorldWork.BurnsOccupants(tile);
        }

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
        /// Floor-Fire, hearth, ember, lava. Rest hunger in the walk.
        /// It does not burn out. Coverings and spells are what react.
        /// </summary>
        public static bool IsRestFire(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Fire:
                case MaterialId.Ember:
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
        /// How hard a standing fire runs from a body.
        /// 5 − seconds: oil 4, wood 3, plant 2, grove 1, ember 0.
        /// Flammability is a separate catch number.
        /// </summary>
        public static float FireRun(float burnSeconds)
        {
            if (burnSeconds <= 0f)
            {
                return 0f;
            }

            var run = EmberBurnSeconds - burnSeconds;
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
                || !MeterEndsWithoutContact(StatusId.Poisoned)
                || MeterEndsWithoutContact(StatusId.Frozen)
                || MeterEndsWithoutContact(StatusId.Stunned))
            {
                broken.Add("Burning and poison must reset once the body leaves that fire or foul");
            }

            if (IsFireContact(null) || IsPoisonLiquidContact(null))
            {
                broken.Add("Empty ground cannot feed a burn or poison meter");
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
                || !IsRestFire(MaterialId.Ember)
                || IsRestFire(MaterialId.Timber))
            {
                broken.Add("Burn and poison capacities must follow nature and matter");
            }

            if (OilBurnSeconds != 1f
                || TimberBurnSeconds != 2f
                || PlantBurnSeconds != 3f
                || GroveBurnSeconds != SlowBurnSeconds - 1f
                || EmberBurnSeconds > 5f
                || ItemBurnSeconds(MaterialId.Oil) != OilBurnSeconds
                || ItemBurnSeconds(MaterialId.Timber) != TimberBurnSeconds
                || ItemBurnSeconds(MaterialId.Plant) != PlantBurnSeconds
                || ItemBurnSeconds(MaterialId.Moss) != PlantBurnSeconds
                || FireRun(OilBurnSeconds) != EmberBurnSeconds - OilBurnSeconds
                || FireRun(TimberBurnSeconds) != EmberBurnSeconds - TimberBurnSeconds
                || FireRun(PlantBurnSeconds) != EmberBurnSeconds - PlantBurnSeconds
                || FireRun(GroveBurnSeconds) != EmberBurnSeconds - GroveBurnSeconds
                || FireRun(EmberBurnSeconds) != 0f)
            {
                broken.Add("Fuel clocks are 5 − seconds: oil 4, wood 3, plant 2, grove 1, ember 0");
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
