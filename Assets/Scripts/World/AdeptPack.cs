using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// What the adept is carrying: stones, charms, wards, mediums.
    /// Doors still gate on possession. I opens the pack to look.
    /// </summary>
    public sealed class AdeptPack
    {
        readonly List<CatalogItem> _held = new();

        public IReadOnlyList<CatalogItem> Held => _held;
        public int Count => _held.Count;
        public int SelectedIndex { get; private set; } = -1;
        public bool Empty => _held.Count == 0;

        public CatalogItem Selected =>
            SelectedIndex >= 0 && SelectedIndex < _held.Count ? _held[SelectedIndex] : null;

        public static bool CanCarry(CatalogItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.id))
            {
                return false;
            }

            switch ((item.kind ?? string.Empty).ToLowerInvariant())
            {
                case "mite":
                case "torch":
                case "rod":
                case "chasm":
                case "barrier":
                case "gate":
                case "plaque":
                case "prop":
                    return false;
                default:
                    return true;
            }
        }

        public static string KindLabel(CatalogItem item)
        {
            switch ((item != null ? item.kind : null)?.ToLowerInvariant())
            {
                case "key":
                    return "Key";
                case "artifact":
                    return "Artifact";
                case "stone":
                    return "Stone";
                case "charm":
                    return "Charm";
                case "ward":
                    return "Ward";
                case "medium":
                    return "Medium";
                case "relic":
                    return "Relic";
                default:
                    return "Item";
            }
        }

        public static string LookText(CatalogItem item)
        {
            if (item == null)
            {
                return "The pack is empty.";
            }

            return Sight.YouSee(Sight.OfItem(item));
        }

        public bool Has(string id)
        {
            return IndexOf(id) >= 0;
        }

        public bool HasAll(IReadOnlyList<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return true;
            }

            for (var i = 0; i < ids.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(ids[i]))
                {
                    continue;
                }

                if (!Has(ids[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Take(CatalogItem item)
        {
            if (!CanCarry(item) || Has(item.id))
            {
                return false;
            }

            _held.Add(item);
            SelectedIndex = _held.Count - 1;
            return true;
        }

        public bool Select(int index)
        {
            if (index < 0 || index >= _held.Count)
            {
                return false;
            }

            SelectedIndex = index;
            return true;
        }

        public bool Nudge(int delta)
        {
            if (_held.Count == 0)
            {
                return false;
            }

            var next = SelectedIndex < 0 ? 0 : SelectedIndex + delta;
            if (next < 0)
            {
                next = _held.Count - 1;
            }
            else if (next >= _held.Count)
            {
                next = 0;
            }

            return Select(next);
        }

        public string Summary()
        {
            if (_held.Count == 0)
            {
                return "empty";
            }

            var parts = new string[_held.Count];
            for (var i = 0; i < _held.Count; i++)
            {
                parts[i] = string.IsNullOrEmpty(_held[i].name) ? _held[i].id : _held[i].name;
            }

            return string.Join(" · ", parts);
        }

        public static void Audit(List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            var authored = AuthoringUtil.ResolveItem(
                "fire-stone",
                "Fire Stone",
                "stone-fire",
                null,
                false,
                null,
                null,
                null,
                "a custom hunger.");
            if (Sight.OfItem(authored) != "a custom hunger.")
            {
                broken.Add("An authored item Description must win over the catalog row");
            }

            if (LookText(authored) != Sight.YouSee("a custom hunger."))
            {
                broken.Add("The pack must show You see plus the item Description");
            }

            if (!CatalogBook.TryItem("fire-stone", out var fire) || string.IsNullOrEmpty(fire.look))
            {
                return;
            }

            var fallback = AuthoringUtil.ResolveItem(
                "fire-stone",
                "",
                "",
                null,
                false,
                null,
                null,
                "",
                "");
            if (Sight.OfItem(fallback) != fire.look)
            {
                broken.Add("An empty Description must use the catalog look");
            }

            var pack = new AdeptPack();
            pack.Take(fire);
            if (!pack.HasAll(new[] { "fire-stone", "", "  " }))
            {
                broken.Add("Empty required ids must not keep a lock shut");
            }
        }

        int IndexOf(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return -1;
            }

            for (var i = 0; i < _held.Count; i++)
            {
                if (_held[i] != null && _held[i].id == id)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
