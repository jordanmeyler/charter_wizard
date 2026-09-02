using UnityEngine;

namespace RuneMagic
{
    public enum SpellTarget
    {
        Single,
        Area,
        Self
    }

    public enum TileVerb
    {
        None,
        Ignite,
        Douse,
        Wet,
        Grow,
        Charge,
        Freeze,
        Cloak,
        Foul,
        Vent,
        Dirt,
        Slick,
        Vine,
        /// <summary>
        /// Liquid poison on the walk. Contact only; yield washes it.
        /// Foul is the airborne miasma cloud.
        /// </summary>
        Poison,
        /// <summary>
        /// Withhold a vegetable body. Plants die. Remains speak Death.
        /// </summary>
        Wither,
        /// <summary>
        /// Shown or living plant-work remembers blighted green.
        /// Wither, poison slick, and foul breath lift.
        /// </summary>
        Restore
    }

    /// <summary>
    /// How a finished spell lands: who it touches, what condition it leaves,
    /// and which tile reaction it starts.
    /// </summary>
    public readonly struct SpellVerb
    {
        public SpellVerb(SpellTarget target, float radius, StatusId status, float statusSeconds, TileVerb tiles)
        {
            Target = target;
            Radius = radius;
            Status = status;
            StatusSeconds = statusSeconds;
            Tiles = tiles;
        }

        public SpellTarget Target { get; }
        public float Radius { get; }
        public StatusId Status { get; }
        public float StatusSeconds { get; }
        public TileVerb Tiles { get; }

        public static SpellVerb Of(SpellId spell, SpellShape shape = SpellShape.None)
        {
            var verb = FromSpell(spell);
            if (verb.Target != SpellTarget.Single || verb.Radius > 0f || verb.Status != StatusId.None)
            {
                return verb;
            }

            if (shape == SpellShape.Spread)
            {
                return new SpellVerb(SpellTarget.Area, 2.4f, verb.Status, verb.StatusSeconds, verb.Tiles);
            }

            if (shape == SpellShape.Self)
            {
                return new SpellVerb(SpellTarget.Self, 0f, verb.Status, verb.StatusSeconds, verb.Tiles);
            }

            return verb;
        }

        static SpellVerb FromSpell(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Fireball:
                case SpellId.SunLance:
                case SpellId.Drive:
                case SpellId.Scald:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Burning, 4.5f, TileVerb.Ignite);
                case SpellId.FlamePillar:
                case SpellId.FirePillar:
                case SpellId.LavaPillar:
                case SpellId.Ignite:
                case SpellId.Melt:
                case SpellId.Witchfire:
                    return new SpellVerb(SpellTarget.Single, 1.1f, StatusId.Burning, 5f, TileVerb.Ignite);
                case SpellId.LiveFloor:
                    return new SpellVerb(SpellTarget.Area, 2.6f, StatusId.Stunned, 1.2f, TileVerb.Charge);
                case SpellId.LavaFlood:
                    return new SpellVerb(SpellTarget.Area, 2.6f, StatusId.Burning, 4f, TileVerb.Ignite);
                case SpellId.LightningBolt:
                case SpellId.BrilliantArc:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Stunned, 2.2f, TileVerb.Charge);
                case SpellId.LightningStrike:
                    return new SpellVerb(SpellTarget.Single, 1.15f, StatusId.Stunned, 2.6f, TileVerb.Charge);
                case SpellId.Push:
                    return new SpellVerb(SpellTarget.Single, 1.2f, StatusId.None, 0f, TileVerb.Vent);
                case SpellId.ChainLightning:
                    return new SpellVerb(SpellTarget.Area, 3.2f, StatusId.Stunned, 2.8f, TileVerb.Charge);
                case SpellId.Jolt:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Stunned, 3.2f, TileVerb.Charge);
                case SpellId.Thunderclap:
                    return new SpellVerb(SpellTarget.Area, 2.6f, StatusId.Stunned, 2.6f, TileVerb.Charge);
                case SpellId.IceSpear:
                case SpellId.IcePillar:
                case SpellId.IceWall:
                    return new SpellVerb(SpellTarget.Single, 1.1f, StatusId.None, 0f, TileVerb.Freeze);
                case SpellId.Freeze:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Frozen, 5f, TileVerb.Freeze);
                case SpellId.Snowfall:
                case SpellId.GraveIce:
                    return new SpellVerb(SpellTarget.Area, 2.4f, StatusId.Frozen, 3.8f, TileVerb.Freeze);
                case SpellId.Snowstorm:
                case SpellId.Blizzard:
                    return new SpellVerb(SpellTarget.Area, 3.2f, StatusId.Frozen, 5f, TileVerb.Freeze);
                case SpellId.DirtToss:
                    return new SpellVerb(SpellTarget.Single, 2.2f, StatusId.None, 0f, TileVerb.Dirt);
                case SpellId.Thunder:
                    return new SpellVerb(SpellTarget.Area, 2.2f, StatusId.Stunned, 1.6f, TileVerb.Charge);
                case SpellId.Douse:
                case SpellId.WaterJet:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Soaked, 5f, TileVerb.Wet);
                case SpellId.Rain:
                case SpellId.Flood:
                case SpellId.Monsoon:
                case SpellId.Swamp:
                    return new SpellVerb(SpellTarget.Area, 2.8f, StatusId.Soaked, 6f, TileVerb.Wet);
                case SpellId.Sprout:
                case SpellId.Grove:
                    return new SpellVerb(SpellTarget.Area, PlantLaw.GrowRadius, StatusId.None, 0f, TileVerb.Grow);
                case SpellId.Grow:
                case SpellId.CallGrowth:
                    return new SpellVerb(SpellTarget.Area, PlantLaw.GrowRadius, StatusId.None, 0f, TileVerb.Grow);
                case SpellId.Wither:
                    return new SpellVerb(SpellTarget.Area, PlantLaw.GrowRadius, StatusId.None, 0f, TileVerb.Wither);
                case SpellId.Forest:
                    return new SpellVerb(SpellTarget.Area, 1.2f, StatusId.None, 0f, TileVerb.Grow);
                case SpellId.Balm:
                case SpellId.Chorus:
                    return new SpellVerb(SpellTarget.Area, 2.2f, StatusId.None, 0f, TileVerb.None);
                case SpellId.Quagmire:
                    return new SpellVerb(SpellTarget.Area, 2.2f, StatusId.Rooted, 4f, TileVerb.Wet);
                case SpellId.Vine:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Rooted, 4f, TileVerb.Vine);
                case SpellId.Lull:
                case SpellId.GraveSleep:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Sleeping, 6f, TileVerb.None);
                case SpellId.Rage:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Raging, 8f, TileVerb.None);
                case SpellId.Frenzy:
                    return new SpellVerb(SpellTarget.Area, 2.4f, StatusId.Raging, 6f, TileVerb.None);
                case SpellId.Command:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Charmed, 10f, TileVerb.None);
                case SpellId.Charm:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Charmed, 12f, TileVerb.None);
                case SpellId.Daze:
                    return new SpellVerb(SpellTarget.Area, 2.4f, StatusId.Confused, 6f, TileVerb.None);
                case SpellId.Confuse:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Confused, 6f, TileVerb.None);
                case SpellId.Terror:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Frightened, 6f, TileVerb.None);
                case SpellId.Dread:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Frightened, 5f, TileVerb.None);
                case SpellId.Stoneskin:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.Stoneskin, 14f, TileVerb.None);
                case SpellId.Watershield:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.Watershield, 14f, TileVerb.None);
                case SpellId.Flameward:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.Flameward, 14f, TileVerb.None);
                case SpellId.Windward:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.Windward, 14f, TileVerb.None);
                case SpellId.Plantward:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.Plantward, 14f, TileVerb.Grow);
                case SpellId.FlameForm:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.FlameForm, 14f, TileVerb.None);
                case SpellId.TideForm:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.TideForm, 14f, TileVerb.None);
                case SpellId.StoneForm:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.StoneForm, 14f, TileVerb.None);
                case SpellId.GaleForm:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.GaleForm, 14f, TileVerb.None);
                case SpellId.GroveForm:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.GroveForm, 14f, TileVerb.Grow);
                case SpellId.CloudForm:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.CloudForm, 14f, TileVerb.None);
                case SpellId.Veil:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.Veiled, 8f, TileVerb.None);
                case SpellId.StormCall:
                    return new SpellVerb(SpellTarget.Area, 3f, StatusId.Soaked, 4f, TileVerb.Wet);
                case SpellId.Gust:
                case SpellId.Gale:
                    return new SpellVerb(SpellTarget.Single, 1.2f, StatusId.None, 0f, TileVerb.Vent);
                case SpellId.Fog:
                case SpellId.Gloom:
                    return new SpellVerb(SpellTarget.Area, 2.8f, StatusId.None, 0f, TileVerb.Cloak);
                case SpellId.TimeStop:
                    return new SpellVerb(SpellTarget.Area, 3.2f, StatusId.Stunned, 2f, TileVerb.None);
                case SpellId.AcidRain:
                    return new SpellVerb(SpellTarget.Area, 3f, StatusId.Poisoned, StatusSpec.PoisonKillSeconds, TileVerb.Poison);
                case SpellId.MetalRain:
                    return new SpellVerb(SpellTarget.Area, 3f, StatusId.None, 0f, TileVerb.None);
                case SpellId.LavaRain:
                    return new SpellVerb(SpellTarget.Area, 3f, StatusId.Burning, 5f, TileVerb.Ignite);
                case SpellId.EmberRain:
                    return new SpellVerb(SpellTarget.Area, 3f, StatusId.Burning, 4f, TileVerb.Ignite);
                case SpellId.SparkRain:
                    return new SpellVerb(SpellTarget.Area, 3f, StatusId.Stunned, 2f, TileVerb.Charge);
                case SpellId.OilRain:
                    return new SpellVerb(SpellTarget.Area, 3f, StatusId.None, 0f, TileVerb.Slick);
                case SpellId.AshRain:
                    return new SpellVerb(SpellTarget.Area, 3f, StatusId.None, 0f, TileVerb.Cloak);
                case SpellId.PlantRain:
                    return new SpellVerb(SpellTarget.Area, 3f, StatusId.Rooted, 4f, TileVerb.Grow);
                case SpellId.DeathCloud:
                    return new SpellVerb(SpellTarget.Area, 3.2f, StatusId.Poisoned, StatusSpec.PoisonKillSeconds, TileVerb.Foul);
                case SpellId.AirWall:
                    return new SpellVerb(SpellTarget.Area, 1.4f, StatusId.None, 0f, TileVerb.Vent);
                case SpellId.Glacier:
                    return new SpellVerb(SpellTarget.Single, 1.1f, StatusId.Frozen, 5f, TileVerb.Freeze);
                case SpellId.Cleanse:
                    return new SpellVerb(SpellTarget.Self, 1.6f, StatusId.None, 0f, TileVerb.Restore);
                case SpellId.Wolfsbane:
                    return new SpellVerb(SpellTarget.Area, PlantLaw.GrowRadius, StatusId.None, 0f, TileVerb.Grow);
                case SpellId.GroveCure:
                    return new SpellVerb(SpellTarget.Area, 2.8f, StatusId.None, 0f, TileVerb.Restore);
                case SpellId.SunOrb:
                    return new SpellVerb(SpellTarget.Area, 2.4f, StatusId.None, 0f, TileVerb.Restore);
                case SpellId.Sanctuary:
                    return new SpellVerb(SpellTarget.Area, 3.2f, StatusId.None, 0f, TileVerb.Restore);
                case SpellId.Spore:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Poisoned, StatusSpec.PoisonKillSeconds, TileVerb.Foul);
                case SpellId.Hemlock:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Poisoned, StatusSpec.PoisonKillSeconds, TileVerb.Poison);
                case SpellId.Nightshade:
                    return new SpellVerb(SpellTarget.Single, 1.1f, StatusId.Poisoned, StatusSpec.PoisonKillSeconds, TileVerb.None);
                case SpellId.Briar:
                    return new SpellVerb(SpellTarget.Single, 1.1f, StatusId.Rooted, 5f, TileVerb.Vine);
                case SpellId.Turn:
                case SpellId.CorpseCall:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Charmed, 12f, TileVerb.None);
                case SpellId.Animate:
                case SpellId.DeathHost:
                    return new SpellVerb(SpellTarget.Area, 3.2f, StatusId.Charmed, 12f, TileVerb.None);
                case SpellId.Exorcism:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.None, 0f, TileVerb.None);
                case SpellId.Blight:
                case SpellId.Miasma:
                    return new SpellVerb(SpellTarget.Area, 2.4f, StatusId.Poisoned, StatusSpec.PoisonKillSeconds, TileVerb.Foul);
                case SpellId.Poison:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.Poisoned, StatusSpec.PoisonKillSeconds, TileVerb.Poison);
                case SpellId.OilShot:
                    return new SpellVerb(SpellTarget.Single, 1.1f, StatusId.None, 0f, TileVerb.Slick);
                case SpellId.OilPuddle:
                    return new SpellVerb(SpellTarget.Area, 1.35f, StatusId.None, 0f, TileVerb.Slick);
                case SpellId.OilGeyser:
                    return new SpellVerb(SpellTarget.Single, 1.1f, StatusId.None, 0f, TileVerb.Slick);
                case SpellId.OilSlick:
                    return new SpellVerb(SpellTarget.Area, 4.2f, StatusId.None, 0f, TileVerb.None);
                case SpellId.OilPillar:
                    return new SpellVerb(SpellTarget.Single, 1.1f, StatusId.None, 0f, TileVerb.None);
                case SpellId.TaintedTree:
                    return new SpellVerb(SpellTarget.Single, 1.1f, StatusId.Poisoned, StatusSpec.PoisonKillSeconds, TileVerb.None);
                case SpellId.Plasma:
                    return new SpellVerb(SpellTarget.Single, 1.2f, StatusId.Burning, 3f, TileVerb.Ignite);
                case SpellId.Tree:
                case SpellId.WoodWall:
                    return new SpellVerb(SpellTarget.Single, 1.1f, StatusId.None, 0f, TileVerb.Grow);
                case SpellId.Darkness:
                    return new SpellVerb(SpellTarget.Area, 3f, StatusId.None, 0f, TileVerb.Cloak);
                case SpellId.WaterPillar:
                    return new SpellVerb(SpellTarget.Single, 1.1f, StatusId.Soaked, 4f, TileVerb.Wet);
                case SpellId.Sandstorm:
                    return new SpellVerb(SpellTarget.Area, 2.8f, StatusId.None, 0f, TileVerb.Vent);
                case SpellId.GraveDust:
                    return new SpellVerb(SpellTarget.Area, 2.4f, StatusId.None, 0f, TileVerb.Foul);
                case SpellId.Hop:
                case SpellId.Flight:
                    return new SpellVerb(SpellTarget.Self, 0f, StatusId.None, 0f, TileVerb.None);
                default:
                    return new SpellVerb(SpellTarget.Single, 0f, StatusId.None, 0f, TileVerb.None);
            }
        }

        public static bool HoldsMind(SpellId spell)
        {
            return StatusSpec.IsMindAilment(Of(spell).Status);
        }

        public static bool IsArea(SpellId spell, SpellShape shape)
        {
            var verb = Of(spell, shape);
            return verb.Target == SpellTarget.Area || shape == SpellShape.Spread;
        }

        public static float RadiusOf(SpellId spell, SpellShape shape, float potency = 1f)
        {
            var verb = Of(spell, shape);
            var radius = verb.Radius;
            if (radius <= 0f)
            {
                radius = shape == SpellShape.Spread ? SpellFormations.Spread.LockRadius : 1.35f;
            }

            return radius * (potency <= 0f ? 1f : potency);
        }
    }
}
