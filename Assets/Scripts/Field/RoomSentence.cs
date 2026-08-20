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
        {
            Rune = rune;
            Material = material;
            Kind = kind;
        }

        public RuneId Rune { get; }
        public MaterialId Material { get; }
        public WeaveKind Kind { get; }
        public bool IsTear => Kind == WeaveKind.Tear || Rune == RuneId.None;
    }

    /// <summary>
    /// Walks what is on screen as a weave: even rows left to right, odd
    /// rows right to left. Off-screen tiles do not speak. Contiguous
    /// same-material runs collapse to one full signature. Breath (Air)
    /// is ambient wherever a floor or wall still holds a room. Locks and
    /// world-strings enter when the scan first reaches their tile.
    /// </summary>
    public static class RoomSentence
    {
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
                if (sequence[i].Rune == RuneId.Air)
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
            var signature = tile.Emission;
            for (var i = 0; i < signature.Count; i++)
            {
                if (signature[i] != RuneId.None)
                {
                    sequence.Add(new WeaveGlyph(signature[i], material, WeaveKind.Material));
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
                        sequence.Add(new WeaveGlyph(runes[r], MaterialId.None, WeaveKind.String));
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
                    sequence.Add(new WeaveGlyph(buffer[i], material, kind));
                }
            }
        }

        static bool AtCell(Vector3 world, int x, int y)
        {
            return Mathf.FloorToInt(world.x) == x && Mathf.FloorToInt(world.y) == y;
        }
    }
}
