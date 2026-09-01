using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// How far a vegetable sentence may grow. Low work stays local.
    /// Forest, opened by Anima, drinks every water still on the screen.
    /// </summary>
    public static class PlantLaw
    {
        public const int ScreenSpread = 256;

        public static int MaxSpread(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Douse:
                case SpellId.WaterJet:
                    return 0;
                case SpellId.Rain:
                case SpellId.Flood:
                case SpellId.StormCall:
                case SpellId.Sprout:
                case SpellId.Tree:
                    return 1;
                case SpellId.Monsoon:
                case SpellId.Swamp:
                case SpellId.CallGrowth:
                case SpellId.Grove:
                case SpellId.WoodWall:
                    return 2;
                case SpellId.Forest:
                    return ScreenSpread;
                default:
                    return 0;
            }
        }

        public static int GrowSteps(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Forest:
                case SpellId.WoodWall:
                case SpellId.Grove:
                    return 2;
                case SpellId.Sprout:
                case SpellId.CallGrowth:
                case SpellId.Tree:
                case SpellId.Rain:
                case SpellId.Flood:
                case SpellId.Monsoon:
                case SpellId.Swamp:
                case SpellId.StormCall:
                case SpellId.Douse:
                case SpellId.WaterJet:
                    return 1;
                default:
                    return 0;
            }
        }

        public static bool PlantsNewBodies(SpellId spell)
        {
            return spell == SpellId.Sprout
                || spell == SpellId.CallGrowth
                || spell == SpellId.Grove
                || spell == SpellId.Forest;
        }

        public static bool FillsVisibleWater(SpellId spell)
        {
            return spell == SpellId.Forest;
        }

        public static bool OnScreen(Vector3 world)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                return true;
            }

            var view = camera.WorldToViewportPoint(world);
            return view.z > 0f
                && view.x >= -0.02f && view.x <= 1.02f
                && view.y >= -0.02f && view.y <= 1.02f;
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (MaxSpread(SpellId.Douse) != 0
                || MaxSpread(SpellId.WaterJet) != 0
                || MaxSpread(SpellId.Sprout) != 1
                || MaxSpread(SpellId.Rain) != 1
                || MaxSpread(SpellId.CallGrowth) != 2
                || MaxSpread(SpellId.Monsoon) != 2
                || MaxSpread(SpellId.Forest) < ScreenSpread)
            {
                broken.Add("Low plant work stays local; Forest drinks the visible water");
            }

            if (MaxSpread(SpellId.Sprout) >= MaxSpread(SpellId.CallGrowth)
                || MaxSpread(SpellId.CallGrowth) >= MaxSpread(SpellId.Forest)
                || MaxSpread(SpellId.Rain) >= MaxSpread(SpellId.Monsoon))
            {
                broken.Add("Plant reach must climb: water-jet, sprout/rain, call-growth/monsoon, then Forest");
            }

            if (!FillsVisibleWater(SpellId.Forest) || FillsVisibleWater(SpellId.Sprout))
            {
                broken.Add("Only Forest may run plant cover to the edge of the screen");
            }

            if (PlantsNewBodies(SpellId.Douse) || !PlantsNewBodies(SpellId.Sprout))
            {
                broken.Add("Yield wets a standing plant; Sprout is what plants a new body");
            }
        }
    }
}
