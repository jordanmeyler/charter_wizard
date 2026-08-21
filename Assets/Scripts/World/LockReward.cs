using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Spawns a catalog item when a lock turns — stones sit behind
    /// a cage, or drop when a guardian falls.
    /// </summary>
    public static class LockReward
    {
        public static void Grant(Vector3 position, string itemId)
        {
            if (string.IsNullOrEmpty(itemId) || !CatalogBook.TryItem(itemId, out var item))
            {
                return;
            }

            var host = WorldItem.Spawn(position, item);
            var director = Object.FindFirstObjectByType<SanctumDirector>();
            if (director == null)
            {
                return;
            }

            var view = host != null ? host.GetComponent<WorldItem>() : null;
            view?.Bind(director.Grimoire, director.Log, director.Pack);
        }
    }
}
