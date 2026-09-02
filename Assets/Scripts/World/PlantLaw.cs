using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// How a vegetable sentence grows. Grow from the feet lays
    /// plant cover in a disk, the way ice covers water — it does
    /// not walk the pool. Yield on a living plant walks one ring
    /// of green. Poison on a living plant turns it; more poison
    /// walks venom the same way. Forest, opened by Anima, drinks
    /// every water still on the screen.
    /// </summary>
    public static class PlantLaw
    {
        public const int ScreenSpread = 256;
        public const int GrowRadius = 3;

        public static int MaxSpread(SpellId spell)
        {
            switch (spell)
            {
                case SpellId.Rain:
                case SpellId.Flood:
                case SpellId.StormCall:
                case SpellId.Douse:
                case SpellId.WaterJet:
                case SpellId.Wolfsbane:
                    return 1;
                case SpellId.Monsoon:
                case SpellId.Swamp:
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
                case SpellId.Grow:
                case SpellId.CallGrowth:
                case SpellId.Tree:
                case SpellId.Rain:
                case SpellId.Flood:
                case SpellId.Monsoon:
                case SpellId.Swamp:
                case SpellId.StormCall:
                case SpellId.Douse:
                case SpellId.WaterJet:
                case SpellId.Wolfsbane:
                    return 1;
                default:
                    return 0;
            }
        }

        public static int PoisonSpread(SpellId spell)
        {
            if (WorldWork.IsPoisonLiquid(spell)
                || WorldWork.IsPoisonVeil(spell)
                || WorldWork.IsPoisonWell(spell)
                || WorldWork.IsPoisonBreath(spell))
            {
                return 1;
            }

            return 0;
        }

        public static bool PlantsNewBodies(SpellId spell)
        {
            return spell == SpellId.Sprout
                || spell == SpellId.Grow
                || spell == SpellId.CallGrowth
                || spell == SpellId.Grove
                || spell == SpellId.Forest
                || spell == SpellId.Plantward
                || spell == SpellId.GroveForm
                || spell == SpellId.Wolfsbane;
        }

        public static bool PlacesCoverFromCaster(SpellId spell)
        {
            return spell == SpellId.Sprout || spell == SpellId.Grove;
        }

        public static bool FillsVisibleWater(SpellId spell)
        {
            return spell == SpellId.Forest;
        }

        public static bool GrowsFromWater(SpellId spell)
        {
            return FillsVisibleWater(spell);
        }

        public static bool CanGrowFrom(WorldTile tile, SpellId spell)
        {
            if (tile == null)
            {
                return false;
            }

            if (GrowsFromWater(spell))
            {
                return true;
            }

            if (tile.IsOverWater || tile.IsDeepWater || tile.HasWaterCover)
            {
                return false;
            }

            return tile.IsPlantish || tile.HasPlantCover || tile.HasPlantishDetail;
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

            if (GrowRadius != 3)
            {
                broken.Add("Grow from the feet lays plant cover three tiles out");
            }

            if (MaxSpread(SpellId.Douse) != 1
                || MaxSpread(SpellId.WaterJet) != 1
                || MaxSpread(SpellId.Wolfsbane) != 1
                || MaxSpread(SpellId.Sprout) != 0
                || MaxSpread(SpellId.Grow) != 0
                || MaxSpread(SpellId.CallGrowth) != 0
                || MaxSpread(SpellId.Rain) != 1
                || MaxSpread(SpellId.Monsoon) != 2
                || MaxSpread(SpellId.Forest) < ScreenSpread)
            {
                broken.Add("Yield walks a living plant one ring; Wolfsbane is a sent patch; Forest drinks the visible water");
            }

            if (PoisonSpread(SpellId.Hemlock) != 1
                || PoisonSpread(SpellId.Poison) != 1
                || PoisonSpread(SpellId.Douse) != 0)
            {
                broken.Add("Poison on a poisoned plant walks one ring, the way yield walks green");
            }

            if (MaxSpread(SpellId.Rain) >= MaxSpread(SpellId.Monsoon)
                || MaxSpread(SpellId.Monsoon) >= MaxSpread(SpellId.Forest))
            {
                broken.Add("Watered reach must climb: rain, monsoon, then Forest");
            }

            if (!FillsVisibleWater(SpellId.Forest)
                || FillsVisibleWater(SpellId.Sprout)
                || GrowsFromWater(SpellId.Sprout)
                || GrowsFromWater(SpellId.Rain)
                || !GrowsFromWater(SpellId.Forest))
            {
                broken.Add("Only Forest may grow plant cover from water tiles");
            }

            if (!PlacesCoverFromCaster(SpellId.Sprout)
                || PlacesCoverFromCaster(SpellId.Forest)
                || PlacesCoverFromCaster(SpellId.Grow))
            {
                broken.Add("Sprout grows a cover disk from the caster; Grow and Forest are sent");
            }

            if (PlantsNewBodies(SpellId.Douse) || !PlantsNewBodies(SpellId.Sprout) || !PlantsNewBodies(SpellId.Grow))
            {
                broken.Add("Yield wets a standing plant; Sprout and Grow plant a new body");
            }
        }
    }
}
