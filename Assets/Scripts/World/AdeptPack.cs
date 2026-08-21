using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Stones and other keys the adept is holding. Doors gate on
    /// possession, never on the order the keys were found.
    /// </summary>
    public sealed class AdeptPack
    {
        readonly List<CatalogItem> _held = new();

        public IReadOnlyList<CatalogItem> Held => _held;

        public bool Has(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return false;
            }

            for (var i = 0; i < _held.Count; i++)
            {
                if (_held[i] != null && _held[i].id == id)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAll(IReadOnlyList<string> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                return true;
            }

            for (var i = 0; i < ids.Count; i++)
            {
                if (!Has(ids[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public bool Take(CatalogItem item)
        {
            if (item == null || string.IsNullOrEmpty(item.id) || Has(item.id))
            {
                return false;
            }

            _held.Add(item);
            return true;
        }

        public string Summary()
        {
            if (_held.Count == 0)
            {
                return "no stones";
            }

            var parts = new string[_held.Count];
            for (var i = 0; i < _held.Count; i++)
            {
                parts[i] = string.IsNullOrEmpty(_held[i].name) ? _held[i].id : _held[i].name;
            }

            return string.Join(" · ", parts);
        }
    }
}
