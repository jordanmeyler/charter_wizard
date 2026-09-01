using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Tiles speak to their neighbors. Fire spreads by burn rate, water grows plants,
    /// retardant matter puts hunger out, and charge runs through what conducts.
    /// </summary>
    public sealed class WorldSim : MonoBehaviour
    {
        WorldGrid _grid;
        float _tick;
        const float Step = 0.32f;
        public const float DryFireRun = 0.4f;
        public const float VineFireRun = 0.9f;
        public const float OilFireRun = 2.4f;
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
                        var catchable = flam > 0f || other.HasVine || other.HasOil;
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

                if (tile.Kindled && tile.Wet < 0.15f)
                {
                    tile.KeepKindled();
                }
                else
                {
                    var consume = 0.12f;
                    if (tile.BurnRate > 0.05f)
                    {
                        consume *= tile.BurnRate / DryFireRun;
                    }

                    tile.Ignite(-consume - quench);
                }

                if (tile.Fire > 0.55f && (tile.IsPlantish || tile.HasPlantishDetail))
                {
                    tile.BurnDown();
                }

                if (tile.Fire > 0.55f && tile.HasVine)
                {
                    tile.BurnVine();
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
            var spreading = new List<WorldTile>();
            foreach (var tile in _grid.All)
            {
                if (tile == null)
                {
                    continue;
                }

                var plant = tile.IsPlantish || tile.HasPlantCover || tile.HasPlantishDetail;
                var watered = tile.Wet > 0.2f || _grid.TouchesOpenWater(tile);
                if (plant && watered)
                {
                    if (tile.IsPlantish)
                    {
                        tile.Grow(1);
                    }

                    spreading.Add(tile);
                }

                tile.Dry(0.08f);
            }

            for (var i = 0; i < spreading.Count; i++)
            {
                _grid.SpreadPlant(spreading[i]);
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
                var neighbors = _grid.Neighbors(tile.Coord);
                for (var n = 0; n < neighbors.Count; n++)
                {
                    var other = neighbors[n];
                    if (other.Conductivity > 0.15f && other.Charge < tile.Charge * 0.7f)
                    {
                        other.ChargeAt(tile.Charge * 0.55f * other.Conductivity);
                    }
                }

                tile.ChargeAt(-0.22f);
            }
        }
    }
}
