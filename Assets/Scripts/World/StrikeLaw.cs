using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// How a finished sentence strikes a body. Fire, lava, and
    /// witchfire are different columns. Mind ailments are too —
    /// charm is not rage.
    /// </summary>
    public enum StrikeKind
    {
        None = 0,
        Fire,
        Water,
        Earth,
        Air,
        Physical,
        Spark,
        Ice,
        Plant,
        Poison,
        Lava,
        Witchfire,
        Plasma,
        Metal,
        Acid,
        Light,
        Dark,
        Death,
        Life,
        Chaos
    }

    /// <summary>
    /// Binary kill: power × affinity must beat defense.
    /// Affinity is 0–5. Defense is 0–10. Power is 0–10.
    /// Witchfire always reads as 1. Chaos (Unmake) has no element
    /// and always reads as 1. Power 0 never kills. Air is 1–2
    /// unless a later tempest is written.
    /// </summary>
    public static class StrikeLaw
    {
        public const int AffinityImmune = 0;
        public const int AffinityNormal = 1;
        public const int AffinityMax = 5;
        public const int DefenseMin = 0;
        public const int DefenseMax = 10;
        public const int PowerMax = 10;
        public const int WitchfirePower = 7;
        public const int GlacierPower = 6;
        public const int UnmakePower = 10;

        public readonly struct Strike
        {
            public Strike(int power, StrikeKind kind, int push = 0)
            {
                Power = MathfClamp(power, 0, PowerMax);
                Kind = kind;
                Push = MathfClamp(push, 0, 6);
            }

            public int Power { get; }
            public StrikeKind Kind { get; }
            public int Push { get; }
            public bool CanKill => Power > 0 && Kind != StrikeKind.None;
        }

        static int MathfClamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }

        public static Strike Of(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Fireball: return new Strike(3, StrikeKind.Fire);
                case SpellId.FirePillar: return new Strike(3, StrikeKind.Fire);
                case SpellId.FlamePillar: return new Strike(4, StrikeKind.Fire);
                case SpellId.SunLance: return new Strike(5, StrikeKind.Light);
                case SpellId.Scald: return new Strike(3, StrikeKind.Fire);
                case SpellId.Ignite: return new Strike(2, StrikeKind.Fire);
                case SpellId.Drive: return new Strike(5, StrikeKind.Fire);
                case SpellId.Witchfire: return new Strike(WitchfirePower, StrikeKind.Witchfire);
                case SpellId.Plasma: return new Strike(8, StrikeKind.Plasma);
                case SpellId.LavaPillar: return new Strike(4, StrikeKind.Lava);
                case SpellId.LavaFlood: return new Strike(4, StrikeKind.Lava);
                case SpellId.LavaRain: return new Strike(4, StrikeKind.Lava);
                case SpellId.EmberRain: return new Strike(3, StrikeKind.Fire);
                case SpellId.SparkShot: return new Strike(3, StrikeKind.Spark);
                case SpellId.LightningBolt: return new Strike(4, StrikeKind.Spark);
                case SpellId.LightningStrike: return new Strike(5, StrikeKind.Spark);
                case SpellId.ChainLightning: return new Strike(4, StrikeKind.Spark);
                case SpellId.BrilliantArc: return new Strike(4, StrikeKind.Spark);
                case SpellId.LiveFloor: return new Strike(2, StrikeKind.Spark);
                case SpellId.StormCall: return new Strike(5, StrikeKind.Spark);
                case SpellId.SparkRain: return new Strike(3, StrikeKind.Spark);
                case SpellId.HurledStone: return new Strike(3, StrikeKind.Earth);
                case SpellId.WoodArrow: return new Strike(3, StrikeKind.Plant);
                case SpellId.MetalRain: return new Strike(3, StrikeKind.Metal);
                case SpellId.MetalPillar: return new Strike(3, StrikeKind.Metal);
                case SpellId.Douse: return new Strike(2, StrikeKind.Water);
                case SpellId.WaterJet: return new Strike(2, StrikeKind.Water);
                case SpellId.Rain: return new Strike(2, StrikeKind.Water);
                case SpellId.Flood: return new Strike(2, StrikeKind.Water);
                case SpellId.Monsoon: return new Strike(2, StrikeKind.Water);
                case SpellId.AcidRain: return new Strike(3, StrikeKind.Acid);
                case SpellId.Poison: return new Strike(2, StrikeKind.Poison);
                case SpellId.Hemlock: return new Strike(3, StrikeKind.Poison);
                case SpellId.Spore: return new Strike(2, StrikeKind.Poison);
                case SpellId.Blight: return new Strike(3, StrikeKind.Poison);
                case SpellId.Miasma: return new Strike(2, StrikeKind.Poison);
                case SpellId.TaintedTree: return new Strike(2, StrikeKind.Poison);
                case SpellId.Nightshade: return new Strike(3, StrikeKind.Poison);
                case SpellId.Glacier: return new Strike(GlacierPower, StrikeKind.Ice);
                case SpellId.IceSpear: return new Strike(1, StrikeKind.Ice);
                case SpellId.Wither: return new Strike(3, StrikeKind.Plant);
                case SpellId.Vine: return new Strike(1, StrikeKind.Plant);
                case SpellId.Briar: return new Strike(2, StrikeKind.Plant);
                case SpellId.DeathCloud: return new Strike(6, StrikeKind.Dark);
                case SpellId.LastBreath: return new Strike(8, StrikeKind.Death);
                case SpellId.GraveDust: return new Strike(4, StrikeKind.Death);
                case SpellId.Unmake: return new Strike(UnmakePower, StrikeKind.Chaos);
                case SpellId.Exorcism: return new Strike(4, StrikeKind.Light);
                case SpellId.SunOrb: return new Strike(3, StrikeKind.Light);
                case SpellId.Sanctuary: return new Strike(4, StrikeKind.Light);
                case SpellId.Gust: return new Strike(1, StrikeKind.Air, 2);
                case SpellId.Gale: return new Strike(2, StrikeKind.Air, 3);
                case SpellId.Push: return new Strike(2, StrikeKind.Air, 3);
                case SpellId.AirWall: return new Strike(2, StrikeKind.Air, 3);
                case SpellId.Sandstorm: return new Strike(2, StrikeKind.Air, 2);
                default: return new Strike(0, StrikeKind.None);
            }
        }

        public static int AffinityOf(AffinityProfile profile, StrikeKind kind)
        {
            if (kind == StrikeKind.None || kind == StrikeKind.Chaos)
            {
                return kind == StrikeKind.Chaos ? AffinityNormal : AffinityImmune;
            }

            if (kind == StrikeKind.Witchfire)
            {
                return AffinityNormal;
            }

            return profile.Strike(kind);
        }

        public static int Effective(SpellId spell, AffinityProfile profile)
        {
            var strike = Of(spell);
            if (strike.Power <= 0)
            {
                return 0;
            }

            return strike.Power * AffinityOf(profile, strike.Kind);
        }

        /// <summary>
        /// Strict greater-than so a 4-power bolt does not drop a
        /// defense-4 stone golem. Power 1 cannot beat defense 9–10
        /// even at affinity 5.
        /// </summary>
        public static bool Kills(SpellId spell, AffinityProfile profile)
        {
            if (PurgesUndead(spell) && !profile.Undead)
            {
                return false;
            }

            return Effective(spell, profile) > profile.Defense;
        }

        public static bool Kills(SpellId spell, StatusHost host)
        {
            return host != null && Kills(spell, host.Profile);
        }

        public static bool CanPush(SpellId spell, StatusHost host)
        {
            var strike = Of(spell);
            if (strike.Push <= 0)
            {
                return false;
            }

            if (host == null)
            {
                return true;
            }

            if (host.Has(StatusId.Windward) || host.Has(StatusId.GaleForm) || host.Has(StatusId.CloudForm))
            {
                return false;
            }

            return PushTiles(spell, host) > 0;
        }

        public static int PushTiles(SpellId spell, StatusHost host)
        {
            var strike = Of(spell);
            if (strike.Push <= 0)
            {
                return 0;
            }

            if (host != null && (host.Has(StatusId.Windward) || host.Has(StatusId.GaleForm) || host.Has(StatusId.CloudForm)))
            {
                return 0;
            }

            var resist = host != null ? host.Profile.PushResist : 0;
            var tiles = strike.Push - resist;
            return tiles > 0 ? tiles : 0;
        }

        public static bool IgnoresWard(SpellId spell, Essence ward)
        {
            var kind = Of(spell).Kind;
            if (kind == StrikeKind.Chaos || kind == StrikeKind.Witchfire || kind == StrikeKind.Plasma)
            {
                return true;
            }

            return false;
        }

        public static bool RaisesDead(SpellId spell) =>
            spell == SpellId.Turn
            || spell == SpellId.CorpseCall
            || spell == SpellId.Animate
            || spell == SpellId.DeathHost;

        public static bool PurgesUndead(SpellId spell) =>
            spell == SpellId.Exorcism
            || spell == SpellId.SunOrb
            || spell == SpellId.Sanctuary;

        public static bool Cleanses(SpellId spell) =>
            spell == SpellId.Cleanse
            || spell == SpellId.Wolfsbane
            || spell == SpellId.GroveCure
            || spell == SpellId.SunOrb
            || spell == SpellId.Sanctuary;

        public static bool HealsNature(SpellId spell) =>
            Cleanses(spell);

        public static Essence EssenceOf(StrikeKind kind)
        {
            switch (kind)
            {
                case StrikeKind.Fire: return Essence.Fire;
                case StrikeKind.Water: return Essence.Water;
                case StrikeKind.Earth: return Essence.Earth;
                case StrikeKind.Air: return Essence.Air;
                case StrikeKind.Physical: return Essence.Physical;
                case StrikeKind.Plant: return Essence.Plant;
                case StrikeKind.Poison: return Essence.Poison;
                case StrikeKind.Spark: return Essence.Air;
                case StrikeKind.Ice: return Essence.Water;
                case StrikeKind.Lava: return Essence.Fire;
                case StrikeKind.Witchfire: return Essence.Fire;
                case StrikeKind.Plasma: return Essence.Fire;
                case StrikeKind.Metal: return Essence.Earth;
                case StrikeKind.Acid: return Essence.Poison;
                case StrikeKind.Light: return Essence.None;
                case StrikeKind.Dark: return Essence.Mind;
                case StrikeKind.Death: return Essence.None;
                case StrikeKind.Life: return Essence.None;
                case StrikeKind.Chaos: return Essence.None;
                default: return Essence.None;
            }
        }

        public static int StatusAffinity(AffinityProfile profile, StatusId id)
        {
            return profile.Status(id);
        }

        public static string AffinityWord(int value)
        {
            switch (MathfClamp(value, AffinityImmune, AffinityMax))
            {
                case 0:
                    return "immune";
                case 1:
                    return "normal";
                case 2:
                    return "weak";
                case 3:
                    return "frail";
                case 4:
                    return "brittle";
                default:
                    return "ruin-weak";
            }
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (Of(SpellId.Fireball).Power != 3 || Of(SpellId.Fireball).Kind != StrikeKind.Fire)
            {
                broken.Add("Fireball must be power 3 Fire");
            }

            if (Of(SpellId.SparkShot).Power != 3
                || Of(SpellId.LightningBolt).Power != 4
                || Of(SpellId.LightningStrike).Power != 5)
            {
                broken.Add("Spark shot is 3; lightning bolt is 4; lightning strike is 5");
            }

            if (Of(SpellId.Witchfire).Power != WitchfirePower || Of(SpellId.Glacier).Power != GlacierPower)
            {
                broken.Add("Witchfire is 7; Glacier is 6");
            }

            if (Of(SpellId.Unmake).Power != UnmakePower || Of(SpellId.Unmake).Kind != StrikeKind.Chaos)
            {
                broken.Add("Unmake is the power-10 chaos sentence");
            }

            if (Of(SpellId.EarthPillar).Power != 0 || Of(SpellId.Douse).Power != 2)
            {
                broken.Add("Earth-pillar is power 0; Douse is water force 2");
            }

            var stone = AffinityProfile.Of(CreatureNature.Earth);
            if (stone.Defense != 4
                || Kills(SpellId.Fireball, stone)
                || Kills(SpellId.SparkShot, stone)
                || Kills(SpellId.LightningBolt, stone)
                || !Kills(SpellId.LightningStrike, stone)
                || !Kills(SpellId.Witchfire, stone)
                || !Kills(SpellId.LavaPillar, stone)
                || !Kills(SpellId.MetalRain, stone))
            {
                broken.Add("A stone golem takes a fireball, a spark shot, and a bolt; strike, witchfire, lava, and metal drop it");
            }

            if (stone.Status(StatusId.Sleeping) <= 0
                || stone.Status(StatusId.Frightened) <= 0
                || stone.Status(StatusId.Raging) <= 0)
            {
                broken.Add("Earth bodies take Lull, Terror, and Rage — the Silent Court walks over a sleeping stone man");
            }

            if (AffinityWord(0) != "immune"
                || AffinityWord(1) != "normal"
                || AffinityWord(5) != "ruin-weak"
                || StatusAffinity(stone, StatusId.Sleeping) <= 0)
            {
                broken.Add("Affinity words are immune / normal / ruin-weak, and earth still takes Lull");
            }

            if (AffinityOf(stone, StrikeKind.Witchfire) != AffinityNormal)
            {
                broken.Add("Nothing resists witchfire — it always reads as 1");
            }

            var flesh = AffinityProfile.Of(CreatureNature.Flesh);
            if (flesh.Defense != 2
                || !Kills(SpellId.Fireball, flesh)
                || Kills(SpellId.Douse, flesh)
                || Kills(SpellId.DirtToss, flesh))
            {
                broken.Add("Flesh dies to a fireball and not to douse or dirt");
            }

            var fire = AffinityProfile.Of(CreatureNature.Fire);
            if (!Kills(SpellId.Douse, fire) || Kills(SpellId.Fireball, fire) || !Kills(SpellId.Witchfire, fire))
            {
                broken.Add("A fire body dies to water and witchfire, not to a fireball");
            }

            var zombie = flesh.AsZombie();
            if (zombie.Strike(StrikeKind.Fire) != 2
                || zombie.Strike(StrikeKind.Light) != AffinityMax
                || zombie.Strike(StrikeKind.Life) != AffinityMax)
            {
                broken.Add("A zombie doubles its fire weakness and is ruin-weak to Light and Life");
            }

            // These are compile-time constants on purpose — they fail
            // if someone later widens affinity or splits the 0–10 scale.
#pragma warning disable CS0162
            if (1 * AffinityMax > 9)
            {
                broken.Add("Power 1 must not beat a top defense even at affinity 5");
            }

            if (DefenseMin != 0 || DefenseMax != PowerMax)
            {
                broken.Add("Defense and power must share the 0–10 scale");
            }
#pragma warning restore CS0162

            if (Of(SpellId.Gust).Power != 1
                || Of(SpellId.Gale).Power != 2
                || Of(SpellId.Push).Power != 2
                || Of(SpellId.AirWall).Power != 2
                || Of(SpellId.Sandstorm).Power != 2
                || Of(SpellId.Gust).Kind != StrikeKind.Air)
            {
                broken.Add("Ordinary air is force 1–2; it still pushes");
            }

            if (Kills(SpellId.Gust, flesh)
                || Kills(SpellId.Gale, flesh)
                || Kills(SpellId.Push, flesh)
                || Kills(SpellId.Gust, stone))
            {
                broken.Add("Ordinary air must not drop flesh or stone; a later tempest can");
            }

            if (Kills(SpellId.Douse, AffinityProfile.Of(CreatureNature.Earth))
                || Effective(SpellId.Fireball, stone) > stone.Defense)
            {
                broken.Add("Power times affinity must be strictly greater than defense");
            }

            if (!Cleanses(SpellId.Wolfsbane)
                || !Cleanses(SpellId.GroveCure)
                || !Cleanses(SpellId.SunOrb)
                || !Cleanses(SpellId.Sanctuary)
                || !PurgesUndead(SpellId.SunOrb)
                || Kills(SpellId.SunOrb, flesh)
                || !Kills(SpellId.SunOrb, zombie)
                || !Kills(SpellId.Sanctuary, zombie))
            {
                broken.Add("Wolfsbane and the light orbs cleanse; the orbs kill only the dead");
            }

            if (Of(SpellId.Hemlock).Power != 3
                || Of(SpellId.Nightshade).Power != 3
                || Of(SpellId.Spore).Power != 2
                || Of(SpellId.Briar).Power != 2
                || Of(SpellId.Briar).Kind != StrikeKind.Plant)
            {
                broken.Add("Living venom is stronger than the dead spray; briar is plant force 2");
            }

            if (Of(SpellId.WoodArrow).Power != 3
                || Of(SpellId.WoodArrow).Kind != StrikeKind.Plant
                || !Kills(SpellId.WoodArrow, flesh)
                || Kills(SpellId.WoodArrow, stone)
                || Kills(SpellId.WoodArrow, AffinityProfile.Of(CreatureNature.Plant)))
            {
                broken.Add("Wood arrow is the plant twin of hurled stone — power 3, drops flesh, not a golem or a plant");
            }
        }
    }

    /// <summary>
    /// One row per body. Strike columns are 0–5. Mind ailments
    /// are their own columns so a golem can take charm and refuse rage.
    /// </summary>
    public readonly struct AffinityProfile
    {
        public const int Columns = 48;

        readonly int[] _strike;
        readonly int[] _status;

        AffinityProfile(CreatureNature nature, int defense, int pushResist, int[] strike, int[] status, bool undead)
        {
            Nature = nature;
            Defense = defense;
            PushResist = pushResist;
            Undead = undead;
            _strike = strike;
            _status = status;
        }

        public CreatureNature Nature { get; }
        public int Defense { get; }
        public int PushResist { get; }
        public bool Undead { get; }

        public int Strike(StrikeKind kind)
        {
            if (kind == StrikeKind.None || _strike == null)
            {
                return StrikeLaw.AffinityImmune;
            }

            var i = (int)kind;
            return i >= 0 && i < _strike.Length ? _strike[i] : StrikeLaw.AffinityNormal;
        }

        public int Status(StatusId id)
        {
            if (id == StatusId.None || _status == null)
            {
                return StrikeLaw.AffinityNormal;
            }

            var i = (int)id;
            return i >= 0 && i < _status.Length ? _status[i] : StrikeLaw.AffinityNormal;
        }

        public AffinityProfile AsZombie()
        {
            var strike = Copy(_strike);
            var fire = Strike(StrikeKind.Fire);
            Set(strike, StrikeKind.Fire, fire <= 0 ? 1 : System.Math.Min(StrikeLaw.AffinityMax, fire * 2));
            Set(strike, StrikeKind.Light, StrikeLaw.AffinityMax);
            Set(strike, StrikeKind.Life, StrikeLaw.AffinityMax);
            return new AffinityProfile(CreatureNature.Undead, Defense, PushResist, strike, Copy(_status), true);
        }

        /// <summary>
        /// Inspector overrides sit on top of a nature row. Empty lists
        /// leave that column as the nature wrote it.
        /// </summary>
        public AffinityProfile WithOverrides(
            int? defense,
            int? pushResist,
            StrikeAffinity[] strikes,
            StatusAffinity[] statuses)
        {
            var nextDefense = defense.HasValue
                ? Clamp(defense.Value, StrikeLaw.DefenseMin, StrikeLaw.DefenseMax)
                : Defense;
            var nextPush = pushResist.HasValue
                ? Clamp(pushResist.Value, 0, 6)
                : PushResist;
            var strike = Copy(_strike);
            var status = Copy(_status);
            if (strikes != null)
            {
                for (var i = 0; i < strikes.Length; i++)
                {
                    if (strikes[i] == null || strikes[i].Kind == StrikeKind.None)
                    {
                        continue;
                    }

                    Set(strike, strikes[i].Kind, Clamp(strikes[i].Affinity, StrikeLaw.AffinityImmune, StrikeLaw.AffinityMax));
                }
            }

            if (statuses != null)
            {
                for (var i = 0; i < statuses.Length; i++)
                {
                    if (statuses[i] == null || statuses[i].Status == StatusId.None)
                    {
                        continue;
                    }

                    Set(status, statuses[i].Status, Clamp(statuses[i].Affinity, StrikeLaw.AffinityImmune, StrikeLaw.AffinityMax));
                }
            }

            return new AffinityProfile(Nature, nextDefense, nextPush, strike, status, Undead);
        }

        public static AffinityProfile Of(CreatureNature nature)
        {
            switch (nature)
            {
                case CreatureNature.Fire:
                    return Fire();
                case CreatureNature.Ice:
                    return Ice();
                case CreatureNature.Earth:
                    return Stone();
                case CreatureNature.Mind:
                    return Mind();
                case CreatureNature.Plant:
                    return Plant();
                case CreatureNature.Undead:
                    return Of(CreatureNature.Flesh).AsZombie();
                default:
                    return Flesh();
            }
        }

        static AffinityProfile Flesh()
        {
            return Build(CreatureNature.Flesh, 2, 1, s =>
            {
                Fill(s, 1);
            }, m =>
            {
                Fill(m, 1);
            });
        }

        static AffinityProfile Fire()
        {
            return Build(CreatureNature.Fire, 3, 1, s =>
            {
                Fill(s, 1);
                Set(s, StrikeKind.Fire, 0);
                Set(s, StrikeKind.Lava, 0);
                Set(s, StrikeKind.Water, 3);
                Set(s, StrikeKind.Ice, 2);
                Set(s, StrikeKind.Poison, 0);
                Set(s, StrikeKind.Earth, 2);
            }, m =>
            {
                Fill(m, 1);
                Set(m, StatusId.Burning, 0);
                Set(m, StatusId.Soaked, 2);
                Set(m, StatusId.Frozen, 2);
                Set(m, StatusId.Poisoned, 0);
            });
        }

        static AffinityProfile Ice()
        {
            return Build(CreatureNature.Ice, 3, 1, s =>
            {
                Fill(s, 1);
                Set(s, StrikeKind.Ice, 0);
                Set(s, StrikeKind.Fire, 3);
                Set(s, StrikeKind.Lava, 3);
                Set(s, StrikeKind.Poison, 0);
            }, m =>
            {
                Fill(m, 1);
                Set(m, StatusId.Frozen, 0);
                Set(m, StatusId.Burning, 2);
                Set(m, StatusId.Poisoned, 0);
            });
        }

        static AffinityProfile Stone()
        {
            return Build(CreatureNature.Earth, 4, 3, s =>
            {
                Fill(s, 1);
                Set(s, StrikeKind.Earth, 0);
                Set(s, StrikeKind.Physical, 0);
                Set(s, StrikeKind.Poison, 0);
                Set(s, StrikeKind.Life, 0);
                Set(s, StrikeKind.Lava, 2);
                Set(s, StrikeKind.Metal, 2);
                Set(s, StrikeKind.Plasma, 1);
            }, m =>
            {
                Fill(m, 1);
                Set(m, StatusId.Burning, 1);
                Set(m, StatusId.Frozen, 1);
                Set(m, StatusId.Soaked, 1);
                Set(m, StatusId.Poisoned, 0);
                Set(m, StatusId.Charmed, 2);
                Set(m, StatusId.Raging, 1);
                Set(m, StatusId.Sleeping, 1);
                Set(m, StatusId.Frightened, 1);
                Set(m, StatusId.Confused, 1);
                Set(m, StatusId.Stunned, 1);
            });
        }

        static AffinityProfile Mind()
        {
            return Build(CreatureNature.Mind, 3, 1, s =>
            {
                Fill(s, 1);
                Set(s, StrikeKind.Spark, 2);
            }, m =>
            {
                Fill(m, 1);
                Set(m, StatusId.Charmed, 2);
                Set(m, StatusId.Sleeping, 2);
                Set(m, StatusId.Raging, 0);
                Set(m, StatusId.Frightened, 0);
                Set(m, StatusId.Confused, 1);
                Set(m, StatusId.Stunned, 2);
            });
        }

        static AffinityProfile Plant()
        {
            return Build(CreatureNature.Plant, 3, 2, s =>
            {
                Fill(s, 1);
                Set(s, StrikeKind.Plant, 0);
                Set(s, StrikeKind.Fire, 3);
                Set(s, StrikeKind.Lava, 3);
                Set(s, StrikeKind.Spark, 0);
                Set(s, StrikeKind.Poison, 2);
            }, m =>
            {
                Fill(m, 1);
                Set(m, StatusId.Rooted, 0);
                Set(m, StatusId.Burning, 2);
                Set(m, StatusId.Poisoned, 2);
                Set(m, StatusId.Charmed, 1);
            });
        }

        static AffinityProfile Build(
            CreatureNature nature,
            int defense,
            int push,
            System.Action<int[]> strike,
            System.Action<int[]> status)
        {
            var s = new int[Columns];
            var m = new int[Columns];
            Fill(s, StrikeLaw.AffinityNormal);
            Fill(m, StrikeLaw.AffinityNormal);
            strike?.Invoke(s);
            status?.Invoke(m);
            return new AffinityProfile(nature, defense, push, s, m, nature == CreatureNature.Undead);
        }

        static void Fill(int[] dest, int value)
        {
            for (var i = 0; i < dest.Length; i++)
            {
                dest[i] = value;
            }
        }

        static void Set(int[] dest, StrikeKind kind, int value)
        {
            var i = (int)kind;
            if (i >= 0 && i < dest.Length)
            {
                dest[i] = value;
            }
        }

        static void Set(int[] dest, StatusId id, int value)
        {
            var i = (int)id;
            if (i >= 0 && i < dest.Length)
            {
                dest[i] = value;
            }
        }

        static int[] Copy(int[] source)
        {
            var dest = new int[Columns];
            if (source != null)
            {
                System.Array.Copy(source, dest, System.Math.Min(source.Length, dest.Length));
            }

            return dest;
        }

        static int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }

            return value > max ? max : value;
        }
    }
}
