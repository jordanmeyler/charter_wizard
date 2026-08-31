using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// How a spark treats a body. Conductivity is a signed number,
    /// the same shape as flammability: positive runs, zero holds,
    /// negative breaks the path.
    /// </summary>
    public static class ChargeLaw
    {
        /// <summary>
        /// A neighbor must be this conductive before charge will
        /// step onto it. Neutral stone sits at zero and will not
        /// take a neighbor's spark.
        /// </summary>
        public const float SpreadMin = 0.2f;

        public static float Of(MaterialId material)
        {
            return MaterialCatalog.Of(material).Conductivity;
        }

        public static bool Conducts(float conductivity)
        {
            return conductivity >= SpreadMin;
        }

        public static bool Insulates(float conductivity)
        {
            return conductivity < 0f;
        }

        public static bool IsNeutral(float conductivity)
        {
            return conductivity >= 0f && conductivity < SpreadMin;
        }

        /// <summary>
        /// A bolt may land here. Neutral stone takes the spark and
        /// holds it. Wood and plants refuse it.
        /// </summary>
        public static bool AcceptsDirectCharge(float conductivity)
        {
            return conductivity >= 0f;
        }

        /// <summary>
        /// Charge only walks onto a body that conducts.
        /// </summary>
        public static bool AcceptsSpread(float conductivity)
        {
            return Conducts(conductivity);
        }

        /// <summary>
        /// An insulator on the tile wins — plants disrupt the flow.
        /// Otherwise the stronger conductor speaks.
        /// </summary>
        public static float Combine(float a, float b)
        {
            if (a < 0f || b < 0f)
            {
                return Mathf.Min(a, b);
            }

            return Mathf.Max(a, b);
        }

        public static float OfCover(TileCover cover)
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
                    return 0f;
            }
        }

        /// <summary>
        /// Standing yield on a tile is enough water for the spark
        /// to run, unless an insulator already breaks the path.
        /// </summary>
        public static float OfWetness(float wet)
        {
            if (wet <= 0.2f)
            {
                return 0f;
            }

            return Of(MaterialId.Water) * 0.55f;
        }

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
                || !Insulates(Of(MaterialId.Moss)))
            {
                broken.Add("Wood and plants must insulate — they disrupt the flow");
            }

            if (!IsNeutral(Of(MaterialId.Stone))
                || !IsNeutral(Of(MaterialId.Dirt))
                || !IsNeutral(Of(MaterialId.Sand))
                || !IsNeutral(Of(MaterialId.Ash)))
            {
                broken.Add("Stone, dirt, sand, and ash must be neutral — they hold a spark but do not pass it");
            }

            if (AcceptsSpread(Of(MaterialId.Stone))
                || AcceptsSpread(Of(MaterialId.Timber))
                || !AcceptsSpread(Of(MaterialId.Metal)))
            {
                broken.Add("Charge must spread onto metal, not onto stone or timber");
            }

            if (!AcceptsDirectCharge(Of(MaterialId.Stone))
                || !AcceptsDirectCharge(Of(MaterialId.Metal))
                || AcceptsDirectCharge(Of(MaterialId.Timber))
                || AcceptsDirectCharge(Of(MaterialId.Plant)))
            {
                broken.Add("A bolt may land on stone or metal; wood and plants refuse it");
            }

            if (Conducts(Combine(Of(MaterialId.Metal), Of(MaterialId.Plant)))
                || !Insulates(Combine(Of(MaterialId.Metal), Of(MaterialId.Timber)))
                || !Insulates(Combine(Of(MaterialId.Water), Of(MaterialId.Plant)))
                || !Conducts(Combine(Of(MaterialId.Stone), Of(MaterialId.Water)))
                || !Conducts(Combine(Of(MaterialId.Stone), OfWetness(1f))))
            {
                broken.Add("A plant or timber on metal must break the path; water on stone must run the spark");
            }

            if (!Conducts(OfCover(TileCover.Water))
                || !Insulates(OfCover(TileCover.Vine)))
            {
                broken.Add("Water cover must conduct; vine cover must insulate");
            }
        }
    }
}
