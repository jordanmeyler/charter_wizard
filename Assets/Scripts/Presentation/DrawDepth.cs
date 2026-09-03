using UnityEngine;
using UnityEngine.Rendering;

namespace RuneMagic
{
    /// <summary>
    /// Shared draw bands. Actors live on their own sorting layer so a
    /// pack of enemies can Y-sort without colliding with tiles or FX.
    /// Lower world Y (south) draws in front.
    /// </summary>
    public static class DrawDepth
    {
        public const string ActorsLayer = "Actors";
        public const string FxLayer = "FX";

        /// <summary>Order steps per world unit. One tile is 64 steps.</summary>
        public const int ActorScale = 64;

        /// <summary>When two bodies share a cell, the adept stays readable.</summary>
        public const int AdeptBias = 1;

        public const int Body = 12;
        public const int Glow = 2;
        public const int Name = 13;
        public const int Chip = 13;
        public const int CastChip = 14;

        public static int ActorOrder(float y, int bias = 0)
        {
            return Mathf.RoundToInt(-y * ActorScale) + bias;
        }

        public static int CompareSouthInFront(float aY, float bY)
        {
            return ActorOrder(aY).CompareTo(ActorOrder(bY));
        }

        public static void ApplyActor(SortingGroup group, float y, int bias = 0)
        {
            if (group == null)
            {
                return;
            }

            TryLayer(group, ActorsLayer);
            group.sortingOrder = ActorOrder(y, bias);
        }

        public static void ApplyFx(Renderer renderer, int order)
        {
            if (renderer == null)
            {
                return;
            }

            TryLayer(renderer, FxLayer);
            renderer.sortingOrder = order;
        }

        public static bool TryLayer(Renderer renderer, string layer)
        {
            if (renderer == null || !LayerExists(layer))
            {
                return false;
            }

            renderer.sortingLayerName = layer;
            return true;
        }

        public static bool TryLayer(SortingGroup group, string layer)
        {
            if (group == null || !LayerExists(layer))
            {
                return false;
            }

            group.sortingLayerName = layer;
            return true;
        }

        public static bool LayerExists(string layer)
        {
            if (string.IsNullOrEmpty(layer))
            {
                return false;
            }

            var layers = SortingLayer.layers;
            for (var i = 0; i < layers.Length; i++)
            {
                if (layers[i].name == layer)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

#if UNITY_EDITOR
namespace RuneMagic
{
    [UnityEditor.InitializeOnLoad]
    static class DrawDepthHealth
    {
        static DrawDepthHealth()
        {
            if (DrawDepth.ActorOrder(0f) <= DrawDepth.ActorOrder(1f))
            {
                Debug.LogError("DrawDepth: a southern body must sort in front of a northern one.");
            }

            if (DrawDepth.CompareSouthInFront(0f, 1f) <= 0)
            {
                Debug.LogError("DrawDepth: CompareSouthInFront must put y=0 in front of y=1.");
            }

            if (DrawDepth.ActorOrder(3f, DrawDepth.AdeptBias) != DrawDepth.ActorOrder(3f) + DrawDepth.AdeptBias)
            {
                Debug.LogError("DrawDepth: adept bias must shift order without changing Y.");
            }
        }
    }
}
#endif
