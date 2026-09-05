using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// How a spark treats a body. Conduct 0–10 is the same shape
    /// as Hunger: zero refuses, the middle holds, high grades walk.
    /// Leftover conductivity stays on the catalog as a signed
    /// number (negative breaks a neighbor's clock harder).
    /// </summary>
    public static class ChargeLaw
    {
        /// <summary>
        /// A cell this live stuns who stands on it and turns a
        /// charge gate. Poor stone holds that long after a bolt
        /// or live-floor; wood never reaches it.
        /// </summary>
        public const float LiveMin = 0.2f;

        public static float LeftoverOf(MaterialId material)
        {
            return MaterialCatalog.Of(material).Conductivity;
        }

        public static float LeftoverOfCover(TileCover cover)
        {
            switch (cover)
            {
                case TileCover.Water:
                    return LeftoverOf(MaterialId.Water);
                case TileCover.Vine:
                    return LeftoverOf(MaterialId.Plant);
                case TileCover.Ice:
                    return LeftoverOf(MaterialId.Ice);
                case TileCover.Mud:
                    return LeftoverOf(MaterialId.Mud);
                case TileCover.Lightning:
                    return LeftoverOf(MaterialId.Vein);
                default:
                    return 0f;
            }
        }

        public static float LeftoverOfWetness(float wet)
        {
            if (wet <= 0.2f)
            {
                return 0f;
            }

            return LeftoverOf(MaterialId.Water) * 0.55f;
        }

        public static int Of(MaterialId material)
        {
            return VitalLaw.ConductOf(material);
        }

        public static bool Conducts(int conduct)
        {
            return VitalLaw.SpreadsCharge(conduct);
        }

        public static bool Conducts(float leftover)
        {
            return leftover >= SpreadLeftoverMin;
        }

        public static bool Insulates(int conduct)
        {
            return VitalLaw.Insulates(conduct);
        }

        public static bool Insulates(float leftover)
        {
            return leftover < 0f;
        }

        public static bool IsPoor(int conduct)
        {
            return VitalLaw.HoldsCharge(conduct) && !VitalLaw.SpreadsCharge(conduct);
        }

        /// <summary>
        /// A bolt or live-floor may land here. Stone holds it.
        /// Wood and plants refuse the cell — the volume still
        /// stuns who stands in it.
        /// </summary>
        public static bool AcceptsDirectCharge(int conduct)
        {
            return VitalLaw.HoldsCharge(conduct);
        }

        public static bool AcceptsDirectCharge(float leftover)
        {
            return leftover >= 0f;
        }

        /// <summary>
        /// Charge only walks onto a conductor (7+).
        /// </summary>
        public static bool AcceptsSpread(int conduct)
        {
            return VitalLaw.SpreadsCharge(conduct);
        }

        public static bool AcceptsSpread(float leftover)
        {
            return leftover >= SpreadLeftoverMin;
        }

        /// <summary>
        /// An insulator on the tile wins — plants disrupt the flow.
        /// Otherwise the stronger conductor speaks.
        /// </summary>
        public static int Combine(int a, int b)
        {
            if (a <= VitalLaw.ConductInsulator || b <= VitalLaw.ConductInsulator)
            {
                return VitalLaw.ConductInsulator;
            }

            return Mathf.Max(a, b);
        }

        public static float Combine(float a, float b)
        {
            if (a < 0f || b < 0f)
            {
                return Mathf.Min(a, b);
            }

            return Mathf.Max(a, b);
        }

        public static int OfCover(TileCover cover)
        {
            switch (cover)
            {
                case TileCover.Water:
                    return Of(MaterialId.Water);
                case TileCover.Vine:
                    return Of(MaterialId.Plant);
                case TileCover.Ice:
                    return Of(MaterialId.Ice);
                case TileCover.Mud:
                    return Of(MaterialId.Mud);
                case TileCover.Lightning:
                    return Of(MaterialId.Vein);
                default:
                    return VitalLaw.ConductPoor;
            }
        }

        /// <summary>
        /// Standing yield on a tile is enough water for the spark
        /// to run, unless an insulator already breaks the path.
        /// </summary>
        public static int OfWetness(float wet)
        {
            if (wet <= 0.2f)
            {
                return VitalLaw.ConductPoor;
            }

            return VitalLaw.ConductRain;
        }

        public static float ChargeHold(int conduct) =>
            VitalLaw.ChargeHold(conduct);

        /// <summary>
        /// How much of a full spark drains in one sim step so the
        /// cell stays live for <see cref="VitalLaw.ChargeHold"/>.
        /// </summary>
        public static float DrainPerStep(int conduct, float step)
        {
            var hold = ChargeHold(conduct);
            if (hold <= 0f)
            {
                return 1f;
            }

            return (1f - LiveMin) * step / hold;
        }

        const float SpreadLeftoverMin = 0.2f;

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (!Conducts(Of(MaterialId.Metal))
                || !Conducts(Of(MaterialId.Water))
                || !Conducts(Of(MaterialId.Vein))
                || !Conducts(Of(MaterialId.Aegis))
                || !Conducts(Of(MaterialId.Rain)))
            {
                broken.Add("Metal, water, vein, aegis, and rain must conduct the spark");
            }

            if (!Insulates(Of(MaterialId.Timber))
                || !Insulates(Of(MaterialId.Plant))
                || !Insulates(Of(MaterialId.Grove))
                || !Insulates(Of(MaterialId.Moss))
                || !Insulates(Of(MaterialId.Oil)))
            {
                broken.Add("Wood, plants, and oil must insulate — they refuse the spark");
            }

            if (!IsPoor(Of(MaterialId.Stone))
                || !IsPoor(Of(MaterialId.Dirt))
                || !IsPoor(Of(MaterialId.Sand))
                || !IsPoor(Of(MaterialId.Ash))
                || !IsPoor(Of(MaterialId.Ice)))
            {
                broken.Add("Stone, dirt, sand, ash, and ice must hold a spark but not pass it");
            }

            if (AcceptsSpread(Of(MaterialId.Stone))
                || AcceptsSpread(Of(MaterialId.Timber))
                || AcceptsSpread(Of(MaterialId.Damp))
                || !AcceptsSpread(Of(MaterialId.Metal)))
            {
                broken.Add("Charge must spread onto metal, not onto stone, timber, or damp rest");
            }

            if (!AcceptsDirectCharge(Of(MaterialId.Stone))
                || !AcceptsDirectCharge(Of(MaterialId.Metal))
                || AcceptsDirectCharge(Of(MaterialId.Timber))
                || AcceptsDirectCharge(Of(MaterialId.Plant)))
            {
                broken.Add("A bolt may land on stone or metal; wood and plants refuse the cell");
            }

            if (Conducts(Combine(Of(MaterialId.Metal), Of(MaterialId.Plant)))
                || !Insulates(Combine(Of(MaterialId.Metal), Of(MaterialId.Timber)))
                || !Insulates(Combine(Of(MaterialId.Water), Of(MaterialId.Plant)))
                || !Conducts(Combine(Of(MaterialId.Stone), Of(MaterialId.Water)))
                || !Conducts(Combine(Of(MaterialId.Stone), Of(MaterialId.Metal)))
                || !Conducts(Combine(Of(MaterialId.Stone), OfWetness(1f))))
            {
                broken.Add("A plant or timber on metal must break the path; water or a metal stamp on stone must run the spark");
            }

            if (!Conducts(OfCover(TileCover.Water))
                || !Insulates(OfCover(TileCover.Vine)))
            {
                broken.Add("Water cover must conduct; vine cover must insulate");
            }

            if (ChargeHold(Of(MaterialId.Stone)) != VitalLaw.ChargeHoldSeconds
                || ChargeHold(Of(MaterialId.Timber)) != 0f
                || ChargeHold(Of(MaterialId.Metal)) < ChargeHold(Of(MaterialId.Stone)) * 2f
                || Of(MaterialId.Stone) != VitalLaw.ConductPoor
                || Of(MaterialId.Metal) != VitalLaw.ConductMetal
                || Of(MaterialId.Water) != VitalLaw.ConductWater
                || Of(MaterialId.Timber) != VitalLaw.ConductInsulator)
            {
                broken.Add("Conduct 0–10: wood refuses, stone holds one second, metal holds and spreads");
            }

            if (SpellVerb.Of(SpellId.LiveFloor).Tiles != TileVerb.Charge
                || SpellVerb.Of(SpellId.LiveFloor).Status != StatusId.Stunned
                || SpellVerb.Of(SpellId.LavaFlood).Tiles != TileVerb.Ignite)
            {
                broken.Add("Live-floor must charge and stun; lava-flood must still ignite");
            }
        }
    }
}
