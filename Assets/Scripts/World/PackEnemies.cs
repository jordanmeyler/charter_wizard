using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Rogue Adventure enemies you can drop in the scene.
    /// GameObject → Rune Magic → Enemies.
    /// </summary>
    public static class PackEnemies
    {
        public sealed class Spec
        {
            public string Id;
            public string Name;
            public string SpriteId;
            public string[] Formula;
            public string Attack;
            public bool Blocking;
            public bool Ensouled;
        }

        public static readonly Spec[] All =
        {
            new() { Id = "shade", Name = "Shade", SpriteId = "enemy-001", Formula = new[] { "Fire", "Salt", "Life" }, Attack = "none" },
            new() { Id = "squire", Name = "Squire", SpriteId = "enemy-002", Formula = new[] { "Earth", "Salt", "Life" }, Attack = "golem", Blocking = true },
            new() { Id = "crawler", Name = "Crawler", SpriteId = "enemy-003", Formula = new[] { "Water", "Salt", "Life" } },
            new() { Id = "wisp", Name = "Wisp", SpriteId = "enemy-004", Formula = new[] { "Air", "Salt", "Life" } },
            new() { Id = "brute", Name = "Brute", SpriteId = "enemy-005", Formula = new[] { "Earth", "Fire", "Life" }, Attack = "golem", Blocking = true },
            new() { Id = "imp", Name = "Imp", SpriteId = "enemy-006", Formula = new[] { "Fire", "Mercury", "Life" } },
            new() { Id = "skeleton", Name = "Skeleton", SpriteId = "enemy-007", Formula = new[] { "Earth", "Air", "Life" }, Blocking = true },
            new() { Id = "cultist", Name = "Cultist", SpriteId = "enemy-008", Formula = new[] { "Fire", "Sulphur", "Life" }, Attack = "wizard", Ensouled = true },
            new() { Id = "bat", Name = "Bat", SpriteId = "enemy-009", Formula = new[] { "Air", "Mercury", "Life" } },
            new() { Id = "serpent", Name = "Serpent", SpriteId = "enemy-010", Formula = new[] { "Water", "Earth", "Life" } },
            new() { Id = "golem", Name = "Golem", SpriteId = "enemy-011", Formula = new[] { "Earth", "Salt", "Fire" }, Attack = "golem", Blocking = true },
            new() { Id = "warden", Name = "Warden", SpriteId = "enemy-012", Formula = new[] { "Fire", "Sulphur", "Mercury" }, Attack = "wizard", Ensouled = true }
        };

        public static EncounterLock Spawn(Spec spec, Vector3 world)
        {
            if (spec == null)
            {
                spec = All[0];
            }

            var host = new GameObject(spec.Name);
            host.transform.position = world;
            var encounter = host.AddComponent<EncounterLock>();
            encounter.ApplyPack(spec);
            if (host.GetComponent<SpriteRenderer>() == null)
            {
                host.AddComponent<SpriteRenderer>();
            }

            return encounter;
        }
    }
}
