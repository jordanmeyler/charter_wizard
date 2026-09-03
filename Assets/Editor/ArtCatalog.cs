#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Reads and writes item descriptions on art.json and matching prefabs.
    /// </summary>
    public static class ArtCatalog
    {
        public const string ArtPath = "Assets/Resources/Catalog/art.json";

        public sealed class Draft
        {
            public string Id;
            public string Name;
            public string Kind;
            public string Look;
            public string Note;
            public string PrefabPath;
        }

        public static List<Draft> Load()
        {
            CatalogBook.EnsureLoaded();
            var drafts = new List<Draft>();
            var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var pair in CatalogBook.AllItems)
            {
                var item = pair.Value;
                if (item == null || string.IsNullOrEmpty(item.id) || !AdeptPack.CanCarry(item))
                {
                    continue;
                }

                var prefab = FindPrefab(item.id);
                drafts.Add(new Draft
                {
                    Id = item.id,
                    Name = string.IsNullOrEmpty(item.name) ? item.id : item.name,
                    Kind = AdeptPack.KindLabel(item),
                    Look = First(PrefabString(prefab, "look"), item.look),
                    Note = First(PrefabString(prefab, "note"), item.note),
                    PrefabPath = prefab
                });
                seen.Add(item.id);
            }

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var item = LoadItem(path);
                if (item == null)
                {
                    continue;
                }

                var so = new SerializedObject(item);
                var id = so.FindProperty("catalogId").stringValue;
                if (string.IsNullOrEmpty(id) || seen.Contains(id))
                {
                    continue;
                }

                drafts.Add(new Draft
                {
                    Id = id,
                    Name = First(so.FindProperty("displayName").stringValue, id),
                    Kind = "Item",
                    Look = so.FindProperty("look").stringValue,
                    Note = so.FindProperty("note").stringValue,
                    PrefabPath = path
                });
                seen.Add(id);
            }

            drafts.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            return drafts;
        }

        public static bool Save(IReadOnlyList<Draft> drafts, out string error)
        {
            error = null;
            if (drafts == null || drafts.Count == 0)
            {
                return true;
            }

            var json = File.Exists(ArtPath) ? File.ReadAllText(ArtPath) : string.Empty;
            if (string.IsNullOrEmpty(json))
            {
                error = "Missing " + ArtPath;
                return false;
            }

            for (var i = 0; i < drafts.Count; i++)
            {
                var draft = drafts[i];
                if (draft == null || string.IsNullOrEmpty(draft.Id))
                {
                    continue;
                }

                if (TryFindItemObject(json, draft.Id, out var start, out var end))
                {
                    if (!TrySetStringField(ref json, start, ref end, "look", draft.Look ?? string.Empty)
                        || !TrySetStringField(ref json, start, ref end, "note", draft.Note ?? string.Empty))
                    {
                        error = "Could not write " + draft.Id + " into art.json.";
                        return false;
                    }
                }

                if (!string.IsNullOrEmpty(draft.PrefabPath))
                {
                    WritePrefab(draft);
                }
            }

            File.WriteAllText(ArtPath, json);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(ArtPath);
            AssetDatabase.Refresh();
            CatalogBook.ReloadItems();
            return true;
        }

        public static string FindPrefab(string catalogId)
        {
            if (string.IsNullOrEmpty(catalogId))
            {
                return null;
            }

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs" });
            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var item = LoadItem(path);
                if (item == null)
                {
                    continue;
                }

                var so = new SerializedObject(item);
                if (so.FindProperty("catalogId").stringValue == catalogId)
                {
                    return path;
                }
            }

            return null;
        }

        static WorldItem LoadItem(string path)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            return prefab != null ? prefab.GetComponent<WorldItem>() : null;
        }

        static string PrefabString(string path, string field)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            var item = LoadItem(path);
            if (item == null)
            {
                return null;
            }

            return new SerializedObject(item).FindProperty(field)?.stringValue;
        }

        static void WritePrefab(Draft draft)
        {
            var item = LoadItem(draft.PrefabPath);
            if (item == null)
            {
                return;
            }

            var so = new SerializedObject(item);
            so.FindProperty("look").stringValue = draft.Look ?? string.Empty;
            so.FindProperty("note").stringValue = draft.Note ?? string.Empty;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(item);
        }

        static bool TryFindItemObject(string json, string id, out int start, out int end)
        {
            start = -1;
            end = -1;
            if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(id))
            {
                return false;
            }

            var needle = "\"id\"";
            var from = 0;
            while (from < json.Length)
            {
                var field = IndexOfJsonField(json, needle, from);
                if (field < 0)
                {
                    return false;
                }

                if (!TryReadJsonStringAfter(json, field + needle.Length, out var value, out var after)
                    || value != id)
                {
                    from = field + needle.Length;
                    continue;
                }

                start = ObjectStart(json, field);
                end = ObjectEnd(json, start);
                return start >= 0 && end > start;
            }

            return false;
        }

        static bool TrySetStringField(ref string json, int start, ref int end, string field, string value)
        {
            var slice = json.Substring(start, end - start + 1);
            var key = "\"" + field + "\"";
            var at = IndexOfJsonField(slice, key, 0);
            var encoded = EscapeJson(value);
            string nextSlice;
            if (at >= 0)
            {
                var valueOpen = FindValueOpenQuote(slice, at + key.Length);
                if (valueOpen < 0)
                {
                    return false;
                }

                var valueClose = EndOfJsonString(slice, valueOpen);
                if (valueClose < 0)
                {
                    return false;
                }

                nextSlice = slice.Substring(0, valueOpen + 1) + encoded + slice.Substring(valueClose);
            }
            else
            {
                var insertAt = slice.LastIndexOf('}');
                if (insertAt < 0)
                {
                    return false;
                }

                var before = slice.Substring(0, insertAt).TrimEnd();
                if (HasFields(slice) && !before.EndsWith(","))
                {
                    before += ",";
                }

                nextSlice = before + "\n      \"" + field + "\": \"" + encoded + "\"\n    }";
            }

            json = json.Substring(0, start) + nextSlice + json.Substring(end + 1);
            end = start + nextSlice.Length - 1;
            return true;
        }

        static int FindValueOpenQuote(string json, int from)
        {
            for (var i = from; i < json.Length; i++)
            {
                var c = json[i];
                if (char.IsWhiteSpace(c) || c == ':')
                {
                    continue;
                }

                return c == '"' ? i : -1;
            }

            return -1;
        }

        static bool HasFields(string slice)
        {
            return slice.IndexOf(':') >= 0;
        }

        static int IndexOfJsonField(string json, string key, int from)
        {
            var i = from;
            while (i < json.Length)
            {
                var at = json.IndexOf(key, i, System.StringComparison.Ordinal);
                if (at < 0)
                {
                    return -1;
                }

                if (InJsonString(json, at))
                {
                    i = at + key.Length;
                    continue;
                }

                return at;
            }

            return -1;
        }

        static bool TryReadJsonStringAfter(string json, int from, out string value, out int after)
        {
            value = null;
            after = from;
            var open = FindValueOpenQuote(json, from);
            if (open < 0)
            {
                return false;
            }

            var close = EndOfJsonString(json, open);
            if (close < 0)
            {
                return false;
            }

            value = UnescapeJson(json.Substring(open + 1, close - open - 1));
            after = close + 1;
            return true;
        }

        static int EndOfJsonString(string json, int openQuote)
        {
            for (var i = openQuote + 1; i < json.Length; i++)
            {
                if (json[i] == '\\')
                {
                    i++;
                    continue;
                }

                if (json[i] == '"')
                {
                    return i;
                }
            }

            return -1;
        }

        static int ObjectStart(string json, int inside)
        {
            var depth = 0;
            for (var i = inside; i >= 0; i--)
            {
                if (InJsonString(json, i))
                {
                    continue;
                }

                if (json[i] == '}')
                {
                    depth++;
                }
                else if (json[i] == '{')
                {
                    if (depth == 0)
                    {
                        return i;
                    }

                    depth--;
                }
            }

            return -1;
        }

        static int ObjectEnd(string json, int start)
        {
            var depth = 0;
            for (var i = start; i < json.Length; i++)
            {
                if (InJsonString(json, i))
                {
                    continue;
                }

                if (json[i] == '{')
                {
                    depth++;
                }
                else if (json[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        static bool InJsonString(string json, int index)
        {
            var inString = false;
            var escape = false;
            for (var i = 0; i < index && i < json.Length; i++)
            {
                var c = json[i];
                if (escape)
                {
                    escape = false;
                    continue;
                }

                if (c == '\\' && inString)
                {
                    escape = true;
                    continue;
                }

                if (c == '"')
                {
                    inString = !inString;
                }
            }

            return inString;
        }

        static string EscapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var text = new StringBuilder(value.Length + 8);
            for (var i = 0; i < value.Length; i++)
            {
                var c = value[i];
                switch (c)
                {
                    case '\\':
                        text.Append("\\\\");
                        break;
                    case '"':
                        text.Append("\\\"");
                        break;
                    case '\n':
                        text.Append("\\n");
                        break;
                    case '\r':
                        break;
                    case '\t':
                        text.Append("\\t");
                        break;
                    default:
                        text.Append(c);
                        break;
                }
            }

            return text.ToString();
        }

        static string UnescapeJson(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value
                .Replace("\\n", "\n")
                .Replace("\\t", "\t")
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\");
        }

        static string First(params string[] values)
        {
            if (values == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < values.Length; i++)
            {
                if (!string.IsNullOrEmpty(values[i]))
                {
                    return values[i];
                }
            }

            return string.Empty;
        }
    }
}
#endif
