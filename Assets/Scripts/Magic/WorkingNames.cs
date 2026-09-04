using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Player-facing names for workings. A spell is the exact rune
    /// string that was written — Spark is not Fire · Air, even when
    /// both join the same thing. Catalog titles (Fireball, Spark shot,
    /// Lightning bolt) stay in the written book. The world only says the runes, or a
    /// name the adept saved for that same composition.
    /// </summary>
    public sealed class WorkingNames
    {
        readonly Dictionary<string, string> _saved = new();

        public static string Key(IReadOnlyList<RuneId> runes)
        {
            if (runes == null || runes.Count == 0)
            {
                return string.Empty;
            }

            var parts = new string[runes.Count];
            for (var i = 0; i < runes.Count; i++)
            {
                parts[i] = ((int)runes[i]).ToString();
            }

            return string.Join(".", parts);
        }

        public static bool SameComposition(IReadOnlyList<RuneId> left, IReadOnlyList<RuneId> right)
        {
            if (left == right)
            {
                return true;
            }

            if (left == null || right == null || left.Count != right.Count)
            {
                return false;
            }

            for (var i = 0; i < left.Count; i++)
            {
                if (left[i] != right[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static string RunePhrase(IReadOnlyList<RuneId> runes)
        {
            if (runes == null || runes.Count == 0)
            {
                return "the working";
            }

            var parts = new string[runes.Count];
            for (var i = 0; i < runes.Count; i++)
            {
                parts[i] = RuneCatalog.NameOf(runes[i]);
            }

            return string.Join(" · ", parts);
        }

        public static string RunePhrase(Composition composition) =>
            RunePhrase(composition.Sequence);

        public void Remember(IReadOnlyList<RuneId> runes, string name)
        {
            var key = Key(runes);
            if (string.IsNullOrEmpty(key) || string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            _saved[key] = name.Trim();
        }

        public void Forget(IReadOnlyList<RuneId> runes)
        {
            var key = Key(runes);
            if (!string.IsNullOrEmpty(key))
            {
                _saved.Remove(key);
            }
        }

        public bool TryGet(IReadOnlyList<RuneId> runes, out string name)
        {
            var key = Key(runes);
            if (string.IsNullOrEmpty(key))
            {
                name = string.Empty;
                return false;
            }

            return _saved.TryGetValue(key, out name);
        }

        public string SavedOrEmpty(IReadOnlyList<RuneId> runes) =>
            TryGet(runes, out var name) ? name : string.Empty;

        public string Call(IReadOnlyList<RuneId> runes) =>
            TryGet(runes, out var name) ? name : RunePhrase(runes);

        public string Call(Composition composition) => Call(composition.Sequence);
    }
}
