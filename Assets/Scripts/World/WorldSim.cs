using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Tiles speak to their neighbors after a spell starts the work.
    /// Fire follows a 0–10 Hunger grade. A strong source (7+)
    /// may spread to equal-or-weaker fuel out to its own reach
    /// (hunger − 6) if that cell touches fuel toward the source.
    /// Fire does not leap a stone gap. Ember is a path, not a gap:
    /// hunger may sit on it and walk across it. The tile stays embered.
    /// Weaker fuel does not walk.
    /// Quench is the wet counterpart (0–10): dry stone leaves a
    /// fire alone, mud suppresses it, water puts it out. A tile
    /// already alight does not recatch. Fire cover stays and, at
    /// rest, lights adjacent covers. A burning plant covering
    /// wicks adjacent wood and oil. Floors and walls stay at rest
    /// until that covering or a spell starts hunger. Charge uses a
    /// 0–10 Conduct grade. Wood refuses.
    /// Stone holds a spark for a second. Metal and water walk it.
    /// Plants do not grow on their own.
    /// Cover on water stays put, like ice, unless a spell-watered
    /// land plant or Forest asked for more.
    /// </summary>
    public sealed class WorldSim : MonoBehaviour
    {
        WorldGrid _grid;
        float _tick;
        public const float Step = 0.32f;
        public const float DryFireRun = 3f;
        public const float VineFireRun = 2f;
        public const float OilFireRun = 4f;
        readonly List<OilWave> _slicks = new();

        struct OilWave
        {
            public Vector2Int Origin;
            public int NextRing;
            public int MaxRadius;
        }

        public void Bind(WorldGrid grid)
        {
            _grid = grid;
        }

        public static WorldSim Ensure(WorldGrid grid)
        {
            if (grid == null)
            {
                return null;
            }

            var sim = grid.GetComponent<WorldSim>();
            if (sim == null)
            {
                sim = grid.gameObject.AddComponent<WorldSim>();
            }

            sim.Bind(grid);
            return sim;
        }

        void Update()
        {
            if (_grid == null || AdeptAvatar.WorldHeld)
            {
                return;
            }

            _tick += Time.deltaTime;
            if (_tick < Step)
            {
                return;
            }

            _tick = 0f;
            StepOilWaves();
            StepFire();
            StepHungerPillars();
            StepPoisonWells();
            StepLightWells();
            StepWet();
            StepCharge();
        }

        public void BeginSlick(Vector2Int origin, int maxRadius)
        {
            _slicks.Add(new OilWave
            {
                Origin = origin,
                NextRing = 1,
                MaxRadius = Mathf.Max(1, maxRadius)
            });
        }

        void StepOilWaves()
        {
            for (var i = _slicks.Count - 1; i >= 0; i--)
            {
                var wave = _slicks[i];
                var cells = WorldWork.Disk(wave.Origin, wave.NextRing);
                WorldWork.SlickOil(_grid, cells);
                wave.NextRing++;
                if (wave.NextRing > wave.MaxRadius)
                {
                    _slicks.RemoveAt(i);
                }
                else
                {
                    _slicks[i] = wave;
                }
            }
        }

        void StepPoisonWells()
        {
            var wells = new List<Vector2Int>();
            foreach (var tile in _grid.All)
            {
                if (tile != null && tile.IsPoisonWell)
                {
                    wells.Add(tile.Coord);
                }
            }

            for (var i = 0; i < wells.Count; i++)
            {
                var around = WorldWork.Disk(wells[i], 1);
                for (var n = 0; n < around.Count; n++)
                {
                    if (around[n] == wells[i])
                    {
                        continue;
                    }

                    _grid.Get(around[n])?.SlickPoison();
                }
            }
        }

        void StepLightWells()
        {
            var wells = new List<(Vector2Int Coord, int Radius, Vector3 World)>();
            foreach (var tile in _grid.All)
            {
                if (tile != null && tile.IsLightWell)
                {
                    wells.Add((tile.Coord, tile.LightWellRadius > 0 ? tile.LightWellRadius : 2, tile.WorldOrigin));
                }
            }

            if (wells.Count == 0)
            {
                return;
            }

            var hosts = FindObjectsByType<StatusHost>(FindObjectsSortMode.None);
            for (var i = 0; i < wells.Count; i++)
            {
                var well = wells[i];
                var around = WorldWork.Disk(well.Coord, well.Radius);
                for (var n = 0; n < around.Count; n++)
                {
                    _grid.Get(around[n])?.RestoreNature();
                }

                var reach = well.Radius + 0.6f;
                for (var h = 0; h < hosts.Length; h++)
                {
                    var host = hosts[h];
                    if (host == null)
                    {
                        continue;
                    }

                    if (Vector2.Distance(host.transform.position, well.World) <= reach)
                    {
                        host.Cleanse();
                    }
                }
            }
        }

        void StepHungerPillars()
        {
            foreach (var tile in _grid.All)
            {
                if (tile != null && tile.IsHungerPillar)
                {
                    tile.TickHungerLife(Step);
                }
            }
        }

        void StepFire()
        {
            var burning = new List<WorldTile>();
            foreach (var tile in _grid.All)
            {
                if (tile != null && (tile.Fire > 0.05f || tile.ProvidesRestFlame))
                {
                    burning.Add(tile);
                }
            }

            var flashed = new HashSet<Vector2Int>();
            for (var i = 0; i < burning.Count; i++)
            {
                var tile = burning[i];
                if (tile.HasOil && tile.Fire > 0.12f)
                {
                    FlashOilFire(tile, flashed);
                }
            }

            for (var i = 0; i < burning.Count; i++)
            {
                var tile = burning[i];
                if (tile.IsFireFloor || tile.HasEmber || tile.HasFireCover)
                {
                    StepRestFire(tile);
                    continue;
                }

                if (tile.Kindled && !tile.LiveFire && tile.Wet < 0.15f && !tile.IsGeyser)
                {
                    tile.KeepKindled();
                    continue;
                }

                if (!tile.LiveFire && !tile.IsGeyser)
                {
                    tile.Ignite(-0.12f, live: false);
                    continue;
                }

                var pressure = QuenchPressure(tile);
                if (pressure.Snuff)
                {
                    tile.Snuff();
                    continue;
                }

                if (!pressure.Suppress)
                {
                    SpreadFrom(tile);
                }

                if (tile.Kindled && tile.Wet < 0.15f && !tile.LiveFire)
                {
                    tile.KeepKindled();
                }
                else
                {
                    var seconds = tile.BurnSeconds;
                    var consume = seconds > 0.05f ? Step / seconds : 0.12f;
                    tile.Ignite(-consume - pressure.Drain);
                    if (tile.Fire <= 0.08f && tile.HoldsBurnFuel)
                    {
                        tile.BurnOut();
                    }
                }
            }
        }

        // Rest fire (Floor-Fire, lava, a hearth), ember, and fire
        // cover stay without a spell. The room is at rest: they
        // light adjacent covers (vine / plant). Floors, walls,
        // oil, and details stay dark until that covering wicks
        // into them, or a spell starts hunger.
        // Ember and fire cover stay. When the overlay is gone rest
        // fire goes dark again — unless the hall is kindled.
        void StepRestFire(WorldTile tile)
        {
            var pressure = QuenchPressure(tile);
            if (pressure.Snuff)
            {
                tile.Snuff();
                return;
            }

            if (!pressure.Suppress)
            {
                CatchRestFuel(tile);
            }

            if (tile.LiveFire && !pressure.Suppress)
            {
                SpreadFrom(tile);
            }

            if ((tile.HasOverlayFuel || tile.HasCatchableFuel) && (tile.LiveFire || tile.Kindled))
            {
                tile.TickOverlayFuel(Step);
                return;
            }

            if (tile.LiveFire)
            {
                tile.EndSpellFire();
                return;
            }

            if (tile.Kindled)
            {
                tile.KeepKindled();
            }
        }

        /// <summary>
        /// A rest flame lights a covering on its own cell, and
        /// adjacent plant / vine covers. Floors, walls, oil, and
        /// details stay at rest until that covering wicks into them.
        /// A spell that starts hunger can still run into those walks.
        /// </summary>
        void CatchRestFuel(WorldTile tile)
        {
            if (tile == null)
            {
                return;
            }

            if (tile.HasRestCatchFuel && !tile.LiveFire)
            {
                tile.Ignite(0.55f, live: true, coverOnly: !tile.HasWalkFuel);
            }
            else if (tile.HasCatchableFuel && !tile.LiveFire)
            {
                tile.Ignite(0.55f);
            }

            var neighbors = _grid.Neighbors(tile.Coord);
            for (var n = 0; n < neighbors.Count; n++)
            {
                var other = neighbors[n];
                if (!AcceptsFireSpread(other) || !other.HasRestCatchFuel)
                {
                    continue;
                }

                var fuel = other.Flammability > 0f ? other.Flammability : 0.85f;
                other.Ignite(fuel, live: true, coverOnly: !other.HasWalkFuel);
            }
        }

        void FlashOilFire(WorldTile start, HashSet<Vector2Int> flashed)
        {
            if (start == null || !flashed.Add(start.Coord))
            {
                return;
            }

            var queue = new Queue<WorldTile>();
            queue.Enqueue(start);
            while (queue.Count > 0)
            {
                var tile = queue.Dequeue();
                var neighbors = _grid.Neighbors(tile.Coord);
                for (var n = 0; n < neighbors.Count; n++)
                {
                    var other = neighbors[n];
                    if (!other.HasOil && !other.ConductsFire)
                    {
                        continue;
                    }

                    if (!flashed.Add(other.Coord))
                    {
                        continue;
                    }

                    if (other.HasOil && !other.LiveFire)
                    {
                        other.Ignite(1.15f);
                    }

                    queue.Enqueue(other);
                }
            }
        }

        struct QuenchHit
        {
            public bool Snuff;
            public bool Suppress;
            public float Drain;
        }

        /// <summary>
        /// Dry stone is quench 0 — no drain, the fire keeps its full
        /// clock and may spread. Mud (3+) smothers: no spread, extra
        /// drain so the flame dies sooner. Water (9+) puts it out.
        /// Oil and plant-on-water ignore yield.
        /// </summary>
        QuenchHit QuenchPressure(WorldTile tile)
        {
            var hit = new QuenchHit();
            if (tile == null || tile.FireIgnoresWater)
            {
                return hit;
            }

            var own = tile.Quench;
            var neighborMax = 0;
            var neighborSum = 0;
            var neighbors = _grid.Neighbors(tile.Coord);
            for (var n = 0; n < neighbors.Count; n++)
            {
                var grade = neighbors[n].Quench;
                if (grade <= VitalLaw.QuenchDry)
                {
                    continue;
                }

                if (grade > neighborMax)
                {
                    neighborMax = grade;
                }

                neighborSum += grade;
            }

            if (VitalLaw.SnuffsFire(own) || VitalLaw.SnuffsFire(neighborMax))
            {
                hit.Snuff = true;
                hit.Suppress = true;
                return hit;
            }

            if (VitalLaw.SuppressesFire(own) || VitalLaw.SuppressesFire(neighborMax))
            {
                hit.Suppress = true;
                hit.Drain = neighborSum * VitalLaw.QuenchDrainPerGrade;
                if (VitalLaw.SuppressesFire(own))
                {
                    hit.Drain += own * VitalLaw.QuenchDrainPerGrade;
                }
            }

            return hit;
        }

        void SpreadFrom(WorldTile tile)
        {
            var potency = tile.FirePotency;
            if (potency <= VitalLaw.HungerNeutral)
            {
                return;
            }

            var coverWick = tile.HasPlantCover || tile.CoverOnlyBurn;
            _grid.ForEachInChebyshev(tile.Coord, VitalLaw.CatchReach(potency), (other, dist) =>
            {
                if (!AcceptsFireSpread(other))
                {
                    return;
                }

                if (!other.ConductsFire
                    && !VitalLaw.CanIgnite(potency, other.Hunger, dist, other.HasPlantCover, coverWick))
                {
                    return;
                }

                if (!TouchesFuelToward(tile, other))
                {
                    return;
                }

                var fuel = other.Flammability > 0f ? other.Flammability : 1.2f;
                var coverOnly = tile.CoverOnlyBurn && other.HasRestCatchFuel && !other.HasWalkFuel;
                other.Ignite(fuel, live: true, coverOnly: coverOnly);
            });
        }

        /// <summary>
        /// The target must sit next to fuel that leads back to the
        /// source — the burning cell, a flammable tile closer to it,
        /// or ember (a Fire path that may also hold the flame).
        /// Isolated fuel two tiles away across stone stays dark.
        /// Crossing ember is not a leap.
        /// </summary>
        bool TouchesFuelToward(WorldTile source, WorldTile target)
        {
            if (source == null || target == null || _grid == null)
            {
                return false;
            }

            var reach = Chebyshev(source.Coord, target.Coord);
            var found = false;
            _grid.ForEachInChebyshev(target.Coord, 1, (other, _) =>
            {
                if (found || !IsFuelTouch(other))
                {
                    return;
                }

                if (other.Coord == source.Coord || Chebyshev(other.Coord, source.Coord) < reach)
                {
                    found = true;
                }
            });

            return found;
        }

        static bool IsFuelTouch(WorldTile tile) =>
            tile != null && (tile.LiveFire || tile.Kindled || tile.IsSpreadFuel || tile.ConductsFire);

        static int Chebyshev(Vector2Int a, Vector2Int b) =>
            Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

        /// <summary>
        /// Hunger runs once through fuel. A tile already alight, or
        /// already ash, does not catch again. Neutral stone and dirt
        /// only light when a spell hits them. Ember is not fuel —
        /// it still accepts a flame from another source so hunger
        /// can sit on it and keep walking. Weaker fuel still needs
        /// a strong source (7+) within that source's reach, an
        /// equal-or-weaker grade, and fuel toward the flame.
        /// </summary>
        public static bool AcceptsFireSpread(WorldTile other)
        {
            if (other == null || other.LiveFire || other.HasAshCover || other.Kindled)
            {
                return false;
            }

            if (other.Fire >= 0.15f)
            {
                return false;
            }

            if (!CanCatch(other))
            {
                return false;
            }

            return other.IsSpreadFuel || other.ConductsFire;
        }

        static bool CanCatch(WorldTile other)
        {
            if (other.FireIgnoresWater)
            {
                return true;
            }

            return !VitalLaw.BlocksCatch(other.Quench);
        }

        void StepWet()
        {
            foreach (var tile in _grid.All)
            {
                tile?.Dry(0.08f);
            }
        }

        void StepCharge()
        {
            var charged = new List<WorldTile>();
            foreach (var tile in _grid.All)
            {
                if (tile != null && tile.Charge > ChargeLaw.LiveMin)
                {
                    charged.Add(tile);
                }
            }

            for (var i = 0; i < charged.Count; i++)
            {
                var tile = charged[i];
                if (tile.Insulates)
                {
                    tile.ChargeAt(-1f);
                    continue;
                }

                var neighbors = _grid.Neighbors(tile.Coord);
                var quench = 0f;
                for (var n = 0; n < neighbors.Count; n++)
                {
                    var other = neighbors[n];
                    if (other.Insulates)
                    {
                        quench += -other.Conductivity * 0.35f;
                        continue;
                    }

                    if (ChargeLaw.AcceptsSpread(tile.Conduct)
                        && ChargeLaw.AcceptsSpread(other.Conduct)
                        && other.Charge < tile.Charge * 0.7f)
                    {
                        other.ChargeAt(tile.Charge * 0.55f * (other.Conduct / (float)VitalLaw.ConductMax));
                    }
                }

                tile.ChargeAt(-ChargeLaw.DrainPerStep(tile.Conduct, Step) - quench);
            }
        }
    }
}
