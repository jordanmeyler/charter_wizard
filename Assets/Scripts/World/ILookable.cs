using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Something the info box can name when the pointer rests on it.
    /// Locks are found through the director; this is for items, marks,
    /// plaques, and the crystal.
    /// </summary>
    public interface ILookable
    {
        Vector3 WorldPosition { get; }
        float LookRadius { get; }
        bool CanLook { get; }
        string LookText { get; }
    }

    public static class Lookables
    {
        static readonly List<ILookable> Live = new();

        public static void Register(ILookable lookable)
        {
            if (lookable != null && !Live.Contains(lookable))
            {
                Live.Add(lookable);
            }
        }

        public static void Unregister(ILookable lookable)
        {
            if (lookable != null)
            {
                Live.Remove(lookable);
            }
        }

        public static ILookable Nearest(Vector3 world, float extra = 0.15f)
        {
            ILookable best = null;
            var bestDistance = float.MaxValue;
            for (var i = Live.Count - 1; i >= 0; i--)
            {
                var lookable = Live[i];
                if (lookable == null || lookable is Object vanished && vanished == null)
                {
                    Live.RemoveAt(i);
                    continue;
                }

                if (!lookable.CanLook)
                {
                    continue;
                }

                var distance = Vector2.Distance(world, lookable.WorldPosition);
                var reach = lookable.LookRadius + extra;
                if (distance <= reach && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = lookable;
                }
            }

            return best;
        }
    }
}
