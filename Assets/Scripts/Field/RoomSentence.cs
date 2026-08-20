using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public enum WeaveKind
    {
        Material,
        Lock,
        String,
        Tear,
        Ambient
    }

    public readonly struct WeaveGlyph
    {
        public WeaveGlyph(RuneId rune, MaterialId material, WeaveKind kind)
            : this(rune, rune, material, kind, 0, 0, 1)
        {
        }

        public WeaveGlyph(
            RuneId shown,
            RuneId join,
            MaterialId material,
            WeaveKind kind,
            int groupId,
            int groupIndex,
            int groupSize)
        {
            Shown = shown;
            Rune = join;
            Material = material;
            Kind = kind;
            GroupId = groupId;
            GroupIndex = groupIndex;
            GroupSize = groupSize < 1 ? 1 : groupSize;
        }

        public RuneId Shown { get; }
        public RuneId Rune { get; }
        public MaterialId Material { get; }
        public WeaveKind Kind { get; }
        public int GroupId { get; }
        public int GroupIndex { get; }
        public int GroupSize { get; }
        public bool IsGroup => GroupId != 0 && GroupSize > 1;
        public bool IsTear => Kind == WeaveKind.Tear || (Shown == RuneId.None && Rune == RuneId.None);
    }

    /// <summary>
    /// Walks what is on screen as a weave. Joins stretch one column per
    /// ingredient and stay grouped. Odd rows travel right, even rows left.
    /// </summary>
    public static class RoomSentence
    {
        static int NextGroup = 1;

        public static List<WeaveGlyph> Read(
            WorldGrid grid,
            ISpellLock[] locks,
            RuneStringSource[] strings,
            Rect view)
        {
            var sequence = new List<WeaveGlyph>(64);
            if (grid == null || view.width <= 0f || view.height <= 0f)
            {
                return sequence;
            }

            var x0 = Mathf.FloorToInt(view.xMin);
            var x1 = Mathf.FloorToInt(view.xMax);
            var y0 = Mathf.FloorToInt(view.yMin);
            var y1 = Mathf.FloorToInt(view.yMax);
            var lastMaterial = MaterialId.None;
            var lastWasTear = false;
            var breathable = false;
            var spokenLocks = new HashSet<int>();
            var spokenStrings = new HashSet<int>();

            for (var row = 0; row <= y1 - y0; row++)
            {
                var y = y0 + row;
                var even = (row & 1) == 0;
                var width = x1 - x0 + 1;
                for (var step = 0; step < width; step++)
                {
                    var x = even ? x0 + step : x1 - step;
                    if (!FieldView.ContainsTile(view, x, y))
                    {
                        continue;
                    }

                    var tile = grid.Get(x, y);
                    if (tile != null && !tile.Def.TearsTapestry)
                    {
                        breathable = true;
                    }

                    AppendTile(sequence, tile, ref lastMaterial, ref lastWasTear);
                    AppendHere(sequence, locks, strings, x, y, spokenLocks, spokenStrings);
                }
            }

            if (breathable)
            {
                AddAmbientAir(sequence);
            }

            return sequence;
        }

        static void AddAmbientAir(List<WeaveGlyph> sequence)
        {
            for (var i = 0; i < sequence.Count; i++)
            {
                if (sequence[i].Shown == RuneId.Air || sequence[i].Rune == RuneId.Air)
                {
                    return;
                }
            }

            sequence.Insert(0, new WeaveGlyph(RuneId.Air, MaterialId.None, WeaveKind.Ambient));
        }

        static void AppendTile(
            List<WeaveGlyph> sequence,
            WorldTile tile,
            ref MaterialId lastMaterial,
            ref bool lastWasTear)
        {
            if (tile == null)
            {
                return;
            }

            if (tile.Def.TearsTapestry)
            {
                if (!lastWasTear)
                {
                    sequence.Add(new WeaveGlyph(RuneId.None, MaterialId.Void, WeaveKind.Tear));
                    lastWasTear = true;
                }

                lastMaterial = MaterialId.Void;
                return;
            }

            lastWasTear = false;
            var material = tile.Material;
            if (material == MaterialId.None || material == lastMaterial)
            {
                return;
            }

            lastMaterial = material;
            var def = MaterialCatalog.Of(material);
            if (def.Manifestation != RuneId.None && ChainBook.IsWrought(def.Manifestation))
            {
                AppendRune(sequence, def.Manifestation, material, WeaveKind.Material);
                return;
            }

            var signature = def.Signature;
            for (var i = 0; i < signature.Count; i++)
            {
                if (signature[i] != RuneId.None)
                {
                    AppendRune(sequence, signature[i], material, WeaveKind.Material);
                }
            }
        }

        static void AppendHere(
            List<WeaveGlyph> sequence,
            ISpellLock[] locks,
            RuneStringSource[] strings,
            int x,
            int y,
            HashSet<int> spokenLocks,
            HashSet<int> spokenStrings)
        {
            if (locks != null)
            {
                for (var i = 0; i < locks.Length; i++)
                {
                    if (locks[i] is not MonoBehaviour body || body == null)
                    {
                        continue;
                    }

                    if (locks[i] is not IRuneSource source || !source.IsEmitting)
                    {
                        continue;
                    }

                    var id = body.GetInstanceID();
                    if (spokenLocks.Contains(id) || !AtCell(source.WorldOrigin, x, y))
                    {
                        continue;
                    }

                    spokenLocks.Add(id);
                    AppendSource(sequence, source, MaterialId.None, WeaveKind.Lock);
                }
            }

            if (strings == null)
            {
                return;
            }

            for (var i = 0; i < strings.Length; i++)
            {
                var sentence = strings[i];
                if (sentence == null || !sentence.IsEmitting)
                {
                    continue;
                }

                var id = sentence.StringId;
                if (spokenStrings.Contains(id) || !AtCell(sentence.WorldOrigin, x, y))
                {
                    continue;
                }

                spokenStrings.Add(id);
                var runes = sentence.Sequence;
                for (var r = 0; r < runes.Length; r++)
                {
                    if (runes[r] != RuneId.None)
                    {
                        AppendRune(sequence, runes[r], MaterialId.None, WeaveKind.String);
                    }
                }
            }
        }

        static void AppendSource(
            List<WeaveGlyph> sequence,
            IRuneSource source,
            MaterialId material,
            WeaveKind kind)
        {
            var buffer = new List<RuneId>(6);
            source.Collect(buffer);
            for (var i = 0; i < buffer.Count; i++)
            {
                if (buffer[i] != RuneId.None)
                {
                    AppendRune(sequence, buffer[i], material, kind);
                }
            }
        }

        static void AppendRune(List<WeaveGlyph> sequence, RuneId rune, MaterialId material, WeaveKind kind)
        {
            if (ChainBook.TryBirth(rune, out var sources) && sources.Count > 0)
            {
                var id = NextGroup++;
                for (var i = 0; i < sources.Count; i++)
                {
                    sequence.Add(new WeaveGlyph(sources[i], rune, material, kind, id, i, sources.Count));
                }

                return;
            }

            sequence.Add(new WeaveGlyph(rune, material, kind));
        }

        static bool AtCell(Vector3 world, int x, int y)
        {
            return Mathf.FloorToInt(world.x) == x && Mathf.FloorToInt(world.y) == y;
        }
    }
}
