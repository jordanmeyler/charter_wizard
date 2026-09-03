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

        public static CombatKind KindOf(string attack)
        {
            switch ((attack ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "golem":
                case "melee":
                    return CombatKind.Golem;
                case "wizard":
                    return CombatKind.Wizard;
                case "archer":
                case "ranged":
                    return CombatKind.Archer;
                default:
                    return CombatKind.None;
            }
        }

        public static string AttackName(CombatKind kind)
        {
            switch (kind)
            {
                case CombatKind.Golem:
                    return "golem";
                case CombatKind.Wizard:
                    return "wizard";
                case CombatKind.Archer:
                    return "archer";
                default:
                    return string.Empty;
            }
        }

        public static string SheetPath(string spriteId, char variant)
        {
            var id = (spriteId ?? string.Empty).Trim().ToLowerInvariant();
            if (id.Length != 9 || !id.StartsWith("enemy-") || !char.IsDigit(id[6]))
            {
                return null;
            }

            var letter = char.ToUpperInvariant(variant);
            if (letter < 'A' || letter > 'D')
            {
                letter = 'A';
            }

            return "Assets/ElvGames/Rogue Adventure/Enemies/Enemy_" + id.Substring(6) + "_" + letter + ".png";
        }

        public static int FrameIndex(string spriteName)
        {
            if (string.IsNullOrEmpty(spriteName))
            {
                return -1;
            }

            var under = spriteName.LastIndexOf('_');
            if (under < 0 || under == spriteName.Length - 1)
            {
                return -1;
            }

            return int.TryParse(spriteName.Substring(under + 1), out var index) ? index : -1;
        }

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
