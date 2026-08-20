using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public enum WeaveKind
    {
        Material,
        Lock,
        String,
        Tear
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
    /// Walks a room as a weave: even rows left to right, odd rows right to
    /// left. Contiguous same-material runs collapse to one full signature
    /// so the sentence is the room, not eighty copies of ash. Locks and
    /// world-strings enter when the scan first reaches their tile.
    /// </summary>
    public static class RoomSentence
    {
        public static List<WeaveGlyph> Read(
            RoomInfo room,
            WorldGrid grid,
            ISpellLock[] locks,
            RuneStringSource[] strings)
        {
            var sequence = new List<WeaveGlyph>(64);
            if (room == null || grid == null)
            {
                return sequence;
            }

            var bounds = room.Bounds;
            var lastMaterial = MaterialId.None;
            var lastWasTear = false;
            var spokenLocks = new HashSet<int>();
            var spokenStrings = new HashSet<int>();

            for (var row = 0; row < bounds.height; row++)
            {
                var y = bounds.yMin + row;
                var even = (row & 1) == 0;
                for (var step = 0; step < bounds.width; step++)
                {
                    var x = even ? bounds.xMin + step : bounds.xMax - 1 - step;
                    var tile = grid.Get(x, y);
                    AppendTile(sequence, tile, ref lastMaterial, ref lastWasTear);
                    AppendHere(sequence, locks, strings, x, y, spokenLocks, spokenStrings);
                }
            }

            return sequence;
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
