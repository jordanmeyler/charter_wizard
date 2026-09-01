using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Tiles speak to their neighbors after a spell starts the work.
    /// Fire spreads by burn rate, retardant matter puts hunger out,
    /// and charge runs through what conducts. Wood and plants break
    /// that path. Plants do not grow on their own. Cover on water
    /// stays put, like ice, unless a spell-watered land plant or
    /// Forest asked for more.
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
                    else if (other.Fire < 0.15f && CanCatch(other) && run > 0.05f)
                    {
                        var catchable = flam > 0f || other.HasVine || other.HasOil || other.HasFireCover;
                        if (catchable)
                        {
                            var fuel = flam > 0f ? flam : 1.2f;
                            other.Ignite(fuel * run);
                        }
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
                    var plantFuel = tile.IsPlantish || tile.HasPlantishDetail;
                    var vineFuel = tile.HasVine;
                    var oilFuel = tile.HasOil && !tile.IsGeyser;
                    var wallFuel = tile.Kind == TileKind.Wall && VitalLaw.CanBurn(tile.Material);
                    var floorFuel = (tile.Kind == TileKind.Floor || tile.Kind == TileKind.Bridge)
                        && (VitalLaw.CanBurn(tile.Material) || plantFuel || vineFuel || oilFuel);
                    tile.Ignite(-consume - quench);
                    if (quench < 0.4f && tile.Fire <= 0.08f
                        && (floorFuel || wallFuel || plantFuel || vineFuel || oilFuel))
                    {
                        tile.BurnOut();
                    }
                }
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

                    if (other.Fire < 0.35f)
                    {
                        other.Ignite(1.15f);
                    }

                    queue.Enqueue(other);
                }
            }
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
