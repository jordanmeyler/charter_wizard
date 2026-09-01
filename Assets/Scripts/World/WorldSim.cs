using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Tiles speak to their neighbors after a spell starts the work.
    /// Fire spreads by burn rate onto fuel that has not already
    /// caught. A tile already alight does not recatch — that loop
    /// is what made a grove burn forever. Ember cover stays put and
    /// then ashes. Retardant matter puts hunger out, and charge
    /// runs through what conducts. Wood and plants break that path.
    /// Plants do not grow on their own. Cover on water stays put,
    /// like ice, unless a spell-watered land plant or Forest asked
    /// for more.
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

                var neighbors = _grid.Neighbors(tile.Coord);
                var quench = 0f;
                var run = tile.BurnRate;
                for (var n = 0; n < neighbors.Count; n++)
                {
                    var other = neighbors[n];
                    var flam = other.Flammability;
                    if (flam < 0f && !tile.FireIgnoresWater)
                    {
                        quench += -flam * 0.45f;
                    }
                    else if (AcceptsFireSpread(other) && run > 0.05f)
                    {
                        var fuel = flam > 0f ? flam : 1.2f;
                        other.Ignite(fuel * run);
                    }
                }

                if (tile.Wet > 0.15f && !tile.FireIgnoresWater)
                {
                    quench += 0.8f;
                }

                if (tile.Kindled && tile.Wet < 0.15f && !tile.LiveFire)
                {
                    tile.KeepKindled();
                }
                else
                {
                    var seconds = tile.BurnSeconds;
                    var consume = seconds > 0.05f ? Step / seconds : 0.12f;
                    tile.Ignite(-consume - quench);
                    if (quench < 0.4f && tile.Fire <= 0.08f && tile.HoldsBurnFuel)
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
            if (tile.LiveFire)
            {
                var neighbors = _grid.Neighbors(tile.Coord);
                var run = tile.BurnRate > 0.05f ? tile.BurnRate : 1f;
                for (var n = 0; n < neighbors.Count; n++)
                {
                    var other = neighbors[n];
                    var flam = other.Flammability;
                    if (flam < 0f && !tile.FireIgnoresWater)
                    {
                        continue;
                    }

                    if (AcceptsFireSpread(other) && run > 0.05f)
                    {
                        var fuel = flam > 0f ? flam : 1.2f;
                        other.Ignite(fuel * run);
                    }
                }
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

        /// <summary>
        /// Hunger runs once through a body. A tile already alight, or
        /// already ash, does not catch again — that recatch is what
        /// made a grove burn forever. Ember cover can catch, then it
        /// stays put and wears off.
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

            return other.Flammability > 0f || other.HasVine || other.HasOil || other.HasFireCover;
        }

        static bool CanCatch(WorldTile other)
        {
            if (other.FireIgnoresWater)
            {
                return true;
            }

            return other.Wet < 0.2f && !other.HasWaterCover && !other.IsDeepWater;
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
