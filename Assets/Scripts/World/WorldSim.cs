using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Tiles speak to their neighbors after a spell starts the work.
    /// Fire follows a 0–10 Hunger grade. A strong source (7+)
    /// may spread to equal-or-weaker fuel out to its own reach
    /// (hunger − 6) if that cell touches fuel toward the source.
    /// Fire does not leap a stone gap. Weaker fuel does not walk.
    /// Quench is the wet counterpart (0–10): dry stone leaves a
    /// fire alone, mud suppresses it, water puts it out. A tile
    /// already alight does not recatch. Ember cover stays put and
    /// then ashes. Charge runs through what conducts. Wood and
    /// plants break that path. Plants do not grow on their own.
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

        void StepFire()
        {
            var burning = new List<WorldTile>();
            foreach (var tile in _grid.All)
            {
                if (tile != null && tile.Fire > 0.05f)
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
                if (tile.IsFireFloor)
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

        // Rest fire (Floor-Fire, lava, a hearth) is inert walk. A spell
        // lights it; then it burns overlays and can jump to flammable
        // neighbors. When the overlay is gone it goes dark again —
        // unless the hall is kindled, in which case it stays hungry.
        void StepRestFire(WorldTile tile)
        {
            var pressure = QuenchPressure(tile);
            if (pressure.Snuff)
            {
                tile.Snuff();
                return;
            }

            if (tile.LiveFire && !pressure.Suppress)
            {
                SpreadFrom(tile);
            }

            if (tile.HasOverlayFuel && (tile.LiveFire || tile.Kindled))
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
                    if (!other.HasOil)
                    {
                        continue;
                    }

                    if (!flashed.Add(other.Coord))
                    {
                        continue;
                    }

                    if (!other.LiveFire)
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

            _grid.ForEachInChebyshev(tile.Coord, VitalLaw.CatchReach(potency), (other, dist) =>
            {
                if (!AcceptsFireSpread(other))
                {
                    return;
                }

                if (!VitalLaw.CanIgnite(potency, other.Hunger, dist, other.HasVine))
                {
                    return;
                }

                if (!TouchesFuelToward(tile, other))
                {
                    return;
                }

                var fuel = other.Flammability > 0f ? other.Flammability : 1.2f;
                other.Ignite(fuel);
            });
        }

        /// <summary>
        /// The target must sit next to fuel that leads back to the
        /// source — the burning cell, or a flammable tile closer to
        /// it. Isolated fuel two tiles away across stone stays dark.
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
            tile != null && (tile.LiveFire || tile.Kindled || tile.IsSpreadFuel);

        static int Chebyshev(Vector2Int a, Vector2Int b) =>
            Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

        /// <summary>
        /// Hunger runs once through fuel. A tile already alight, or
        /// already ash, does not catch again. Neutral stone and dirt
        /// only light when a spell hits them. Weaker fuel still needs
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

            if (!CanCatch(other) || !other.IsSpreadFuel)
            {
                return false;
            }

            return true;
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
                if (tile != null && tile.Charge > 0.2f)
                {
                    charged.Add(tile);
                }
            }

            for (var i = 0; i < charged.Count; i++)
            {
                var tile = charged[i];
                if (tile.Insulates)
                {
                    tile.ChargeAt(-0.55f);
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

                    if (ChargeLaw.AcceptsSpread(other.Conductivity)
                        && other.Charge < tile.Charge * 0.7f)
                    {
                        other.ChargeAt(tile.Charge * 0.55f * other.Conductivity);
                    }
                }

                tile.ChargeAt(-0.22f - quench);
            }
        }
    }
}
