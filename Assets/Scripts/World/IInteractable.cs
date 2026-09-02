using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Something the adept can use in the world — pray, later read,
    /// pull a lever. Lookables only fill the info box; this is the
    /// Interact button. Drop <see cref="WorldInteract"/> on an empty
    /// object so tiles or child sprites can carry the look.
    /// </summary>
    public interface IInteractable
    {
        Vector3 WorldPosition { get; }
        float InteractRadius { get; }
        bool CanInteract { get; }
        string InteractVerb { get; }
        void Interact(SanctumDirector director);
    }

    public static class Interactables
    {
        static readonly List<IInteractable> Live = new();

        public static void Register(IInteractable interactable)
        {
            if (interactable != null && !Live.Contains(interactable))
            {
                Live.Add(interactable);
            }
        }

        public static void Unregister(IInteractable interactable)
        {
            if (interactable != null)
            {
                Live.Remove(interactable);
            }
        }

        public static IInteractable Nearest(Vector3 world, float extra = 0.05f)
        {
            IInteractable best = null;
            var bestDistance = float.MaxValue;
            for (var i = Live.Count - 1; i >= 0; i--)
            {
                var interactable = Live[i];
                if (interactable == null || interactable is Object vanished && vanished == null)
                {
                    Live.RemoveAt(i);
                    continue;
                }

                if (!interactable.CanInteract)
                {
                    continue;
                }

                var distance = Vector2.Distance(world, interactable.WorldPosition);
                var reach = interactable.InteractRadius + extra;
                if (distance <= reach && distance < bestDistance)
                {
                    bestDistance = distance;
                    best = interactable;
                }
            }

            return best;
        }
    }
}
