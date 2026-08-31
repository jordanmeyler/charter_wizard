using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Tiles speak to their neighbors. Fire spreads, water grows plants,
    /// retardant matter puts hunger out, and charge runs through what conducts.
    /// </summary>
    public sealed class WorldSim : MonoBehaviour
    {
        WorldGrid _grid;
        float _tick;
        const float Step = 0.32f;

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
            StepFire();
            StepWet();
            StepCharge();
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

            for (var i = 0; i < burning.Count; i++)
            {
                var tile = burning[i];
                if (tile.Kindled && !tile.LiveFire && tile.Wet < 0.15f)
                {
                    tile.KeepKindled();
                    continue;
                }

                if (!tile.LiveFire)
                {
                    tile.Ignite(-0.12f, live: false);
                    continue;
                }

                var neighbors = _grid.Neighbors(tile.Coord);
                var quench = 0f;
                for (var n = 0; n < neighbors.Count; n++)
                {
                    var other = neighbors[n];
                    var flam = other.Flammability;
                    if (flam < 0f)
                    {
                        quench += -flam * 0.45f;
                    }
                    else if (other.Wet < 0.2f && !other.HasWaterCover && other.Fire < 0.15f)
                    {
                        var wick = tile.HasOil || tile.HasVine || other.HasVine || other.HasOil;
                        var catchable = flam > 0f || other.HasVine || other.HasOil;
                        if (catchable)
                        {
                            var run = wick ? 0.9f : 0.4f;
                            var fuel = flam > 0f ? flam : 1.2f;
                            other.Ignite(fuel * run);
                        }
                    }
                }

                if (tile.Wet > 0.15f)
                {
                    quench += 0.8f;
                }

                tile.Ignite(-0.12f - quench);
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

        void StepWet()
        {
            foreach (var tile in _grid.All)
            {
                if (tile == null)
                {
                    continue;
                }

                if (tile.Wet > 0.2f && tile.IsPlantish)
                {
                    tile.Grow(1);
                    _grid.SpreadPlant(tile);
                }

                tile.Dry(0.08f);
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
