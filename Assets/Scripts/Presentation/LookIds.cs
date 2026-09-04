using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Ids Play asks for when a spell stands a body. Assign a Look or
    /// Sprite Sheet with that id — Floor / Wall stamps never use these.
    /// </summary>
    public static class LookIds
    {
        public static string Of(MaterialId material)
        {
            return material == MaterialId.None
                ? string.Empty
                : material.ToString().ToLowerInvariant();
        }

        public static string[] Wall(MaterialId material)
        {
            var ids = new List<string>(4);
            var key = Of(material);
            if (!string.IsNullOrEmpty(key))
            {
                Add(ids, "wall-" + key);
            }

            switch (material)
            {
                case MaterialId.Ice:
                case MaterialId.Snow:
                case MaterialId.Glacier:
                    Add(ids, "wall-ice");
                    break;
                case MaterialId.Plant:
                case MaterialId.Grove:
                case MaterialId.Moss:
                    Add(ids, "wall-plant");
                    Add(ids, "wall-grove");
                    Add(ids, "wall-moss");
                    break;
                case MaterialId.Timber:
                    Add(ids, "wall-timber");
                    Add(ids, "wall-wood");
                    return ids.ToArray();
            }

            Add(ids, "wall");
            return ids.ToArray();
        }

        public static string[] Floor(MaterialId material)
        {
            var ids = new List<string>(4);
            var key = Of(material);
            if (!string.IsNullOrEmpty(key))
            {
                Add(ids, "floor-" + key);
            }

            switch (material)
            {
                case MaterialId.Dirt:
                case MaterialId.Sand:
                case MaterialId.Dust:
                    Add(ids, "floor-dirt");
                    break;
                case MaterialId.Mud:
                    Add(ids, "floor-mud");
                    break;
                case MaterialId.Ash:
                    Add(ids, "floor-ash");
                    break;
                case MaterialId.Water:
                case MaterialId.Rain:
                    Add(ids, "floor-water");
                    break;
                case MaterialId.Stone:
                    Add(ids, "floor-stone");
                    break;
            }

            return ids.ToArray();
        }

        public static string[] Bridge(MaterialId material)
        {
            var ids = new List<string>(3);
            var key = Of(material);
            if (!string.IsNullOrEmpty(key))
            {
                Add(ids, "bridge-" + key);
            }

            Add(ids, "bridge");
            return ids.ToArray();
        }

        public static string[] Column(MaterialId material)
        {
            var ids = new List<string>(4);
            var key = Of(material);
            if (!string.IsNullOrEmpty(key))
            {
                Add(ids, "pillar-" + key);
            }

            switch (material)
            {
                case MaterialId.Ice:
                case MaterialId.Snow:
                case MaterialId.Glacier:
                    Add(ids, "pillar-ice");
                    Add(ids, "wall-ice");
                    Add(ids, "cover-ice");
                    break;
                case MaterialId.Timber:
                    Add(ids, "pillar-timber");
                    Add(ids, "pillar-wood");
                    return ids.ToArray();
                case MaterialId.Fire:
                    Add(ids, "pillar-fire");
                    Add(ids, "fire-pillar");
                    break;
                case MaterialId.Hearth:
                    Add(ids, "pillar-hearth");
                    Add(ids, "flame-pillar");
                    Add(ids, "pillar-flame");
                    break;
                case MaterialId.Ember:
                    Add(ids, "pillar-ember");
                    Add(ids, "pillar-fire");
                    break;
                case MaterialId.Lava:
                    Add(ids, "pillar-lava");
                    Add(ids, "lava-pillar");
                    break;
            }

            Add(ids, "pillar");
            return ids.ToArray();
        }

        public static string[] Door(bool open, bool leaf)
        {
            if (!leaf)
            {
                return open ? new[] { "arch", "door-open" } : new[] { "arch-shut", "door" };
            }

            return open ? new[] { "door-open", "arch" } : new[] { "door", "arch-shut" };
        }

        public static string[] Pit() => new[] { "pit" };

        /// <summary>
        /// Shot / pillar body ids for a spell. Kebab names first
        /// (<c>fire-pillar</c>), then the compact enum, then family.
        /// </summary>
        public static string[] SpellBody(SpellId spell, string family)
        {
            var ids = new List<string>(8);
            if (spell != SpellId.None)
            {
                var kebab = Kebab(spell.ToString());
                var compact = spell.ToString().ToLowerInvariant();
                Add(ids, kebab + "-shot");
                Add(ids, kebab);
                if (spell == SpellId.WoodArrow)
                {
                    Add(ids, "arrow-shot");
                    Add(ids, "wood-arrow-shot");
                }
                if (!string.Equals(compact, kebab, System.StringComparison.Ordinal))
                {
                    Add(ids, compact + "-shot");
                    Add(ids, compact);
                }
            }

            if (!string.IsNullOrWhiteSpace(family))
            {
                var key = family.Trim().ToLowerInvariant();
                Add(ids, key + "-shot");
                Add(ids, "fx-" + key);
            }

            return ids.ToArray();
        }

        public static string Kebab(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var built = new System.Text.StringBuilder(name.Length + 4);
            for (var i = 0; i < name.Length; i++)
            {
                var c = name[i];
                if (i > 0 && char.IsUpper(c))
                {
                    built.Append('-');
                }

                built.Append(char.ToLowerInvariant(c));
            }

            return built.ToString();
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (!Contains(Wall(MaterialId.Ice), "wall-ice")
                || !Contains(Wall(MaterialId.Stone), "wall")
                || Contains(Wall(MaterialId.Timber), "wall")
                || !Contains(Wall(MaterialId.Timber), "wall-timber"))
            {
                broken.Add("Conjured walls must ask wall-{material}; timber must not fall through to stone wall");
            }

            if (!Contains(Bridge(MaterialId.Stone), "bridge")
                || !Contains(Bridge(MaterialId.Ice), "bridge-ice")
                || !Contains(Column(MaterialId.Ice), "pillar-ice")
                || !Contains(Column(MaterialId.Ice), "wall-ice")
                || !Contains(Column(MaterialId.Ice), "cover-ice")
                || Contains(Column(MaterialId.Ice), "ice-fountain")
                || !Contains(Column(MaterialId.Fire), "fire-pillar")
                || !Contains(Column(MaterialId.Hearth), "flame-pillar")
                || !Contains(Column(MaterialId.Lava), "lava-pillar")
                || !Contains(Floor(MaterialId.Dirt), "floor-dirt"))
            {
                broken.Add("Bridge, pillar, and leftover dirt must share one id list so a Look can replace them");
            }

            var fireBody = SpellBody(SpellId.FirePillar, "fire");
            if (!Contains(fireBody, "fire-pillar")
                || !Contains(fireBody, "fire-pillar-shot")
                || !Contains(SpellBody(SpellId.LavaPillar, "lava"), "lava-pillar")
                || !Contains(SpellBody(SpellId.FlamePillar, "fire"), "flame-pillar"))
            {
                broken.Add("New pillar spells must resolve Looks as fire-pillar / lava-pillar / flame-pillar");
            }
        }

        static void Add(List<string> ids, string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return;
            }

            for (var i = 0; i < ids.Count; i++)
            {
                if (string.Equals(ids[i], id, System.StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            ids.Add(id);
        }

        static bool Contains(string[] ids, string id)
        {
            if (ids == null)
            {
                return false;
            }

            for (var i = 0; i < ids.Length; i++)
            {
                if (string.Equals(ids[i], id, System.StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
