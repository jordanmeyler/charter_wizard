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
            int groupSize,
            string title = null,
            bool living = false)
        {
            Shown = shown;
            Rune = join;
            Material = material;
            Kind = kind;
            GroupId = groupId;
            GroupIndex = groupIndex;
            GroupSize = groupSize < 1 ? 1 : groupSize;
            Title = title ?? string.Empty;
            Living = living;
        }

        public RuneId Shown { get; }
        public RuneId Rune { get; }
        public MaterialId Material { get; }
        public WeaveKind Kind { get; }
        public int GroupId { get; }
        public int GroupIndex { get; }
        public int GroupSize { get; }
        public string Title { get; }
        public bool Living { get; }
        public bool IsGroup => GroupId != 0 && GroupSize > 1;
        public bool IsTear => Kind == WeaveKind.Tear || (Shown == RuneId.None && Rune == RuneId.None);
        public string GroupTitle => !string.IsNullOrEmpty(Title) ? Title : string.Empty;
    }

    /// <summary>
    /// Walks what is on screen as a weave. A wrought join that already
    /// stands (Spark, Plant, Ice) appears as itself so it can be drawn.
    /// The basics that compose it are strewn through the grid. Creature
    /// recipes stay as written — Life marks a living formula and is not
    /// unfolded. The adept is mind, body, and soul.
    /// </summary>
    public static class RoomSentence
    {
        static int NextGroup = 1;

        public static List<WeaveGlyph> Read(
            WorldGrid grid,
            ISpellLock[] locks,
            RuneStringSource[] strings,
            Rect view,
            IRuneSource[] extras = null)
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
            var scatter = new List<WeaveGlyph>(16);

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

                    AppendTile(sequence, tile, scatter, ref lastMaterial, ref lastWasTear);
                    AppendHere(sequence, locks, strings, extras, x, y, spokenLocks, spokenStrings);
                }
            }

            ScatterComposing(sequence, scatter, FieldView.Key(view));

            if (breathable)
            {
                AddAmbient(sequence, RuneId.Air);
            }

            // You are always in the field: mind, body, and soul.
            AppendCreature(
                sequence,
                AdeptAvatar.DisplayTitle,
                AdeptAvatar.Wash,
                AdeptAvatar.Formula,
                WeaveKind.Ambient,
                atHead: true);

            return sequence;
        }

        static void AddAmbient(List<WeaveGlyph> sequence, RuneId rune)
        {
            for (var i = 0; i < sequence.Count; i++)
            {
                if (sequence[i].Shown == rune || sequence[i].Rune == rune)
                {
                    return;
                }
            }

            sequence.Insert(0, new WeaveGlyph(rune, MaterialId.None, WeaveKind.Ambient));
        }

        static void AppendTile(
            List<WeaveGlyph> sequence,
            WorldTile tile,
            List<WeaveGlyph> scatter,
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
                sequence.Add(new WeaveGlyph(
                    def.Manifestation, def.Manifestation, material, WeaveKind.Material, 0, 0, 1));
                CollectComposing(scatter, def.Manifestation, material);
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

        static void CollectComposing(List<WeaveGlyph> scatter, RuneId wrought, MaterialId material)
        {
            if (scatter == null)
            {
                return;
            }

            var recipe = new List<RuneId>(8);
            ChainBook.ExpandRecipe(wrought, recipe);
            for (var i = 0; i < recipe.Count; i++)
            {
                if (recipe[i] != RuneId.None && recipe[i] != wrought)
                {
                    scatter.Add(new WeaveGlyph(recipe[i], material, WeaveKind.Material));
                }
            }
        }

        static void ScatterComposing(List<WeaveGlyph> sequence, List<WeaveGlyph> extras, int seed)
        {
            if (sequence == null || extras == null || extras.Count == 0)
            {
                return;
            }

            var rng = new System.Random(seed == int.MinValue ? 1 : seed);
            for (var i = 0; i < extras.Count; i++)
            {
                var at = rng.Next(0, sequence.Count + 1);
                sequence.Insert(at, extras[i]);
            }
        }

        static void AppendHere(
            List<WeaveGlyph> sequence,
            ISpellLock[] locks,
            RuneStringSource[] strings,
            IRuneSource[] extras,
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
                    if (locks[i] is EncounterLock creature && creature.Formula != null && creature.Formula.Length > 0)
                    {
                        AppendCreature(
                            sequence,
                            creature.DisplayName,
                            CreatureWash(creature),
                            creature.Formula,
                            WeaveKind.Lock);
                    }
                    else
                    {
                        AppendSource(sequence, source, MaterialId.None, WeaveKind.Lock);
                    }
                }
            }

            if (strings != null)
            {
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

            if (extras == null)
            {
                return;
            }

            for (var i = 0; i < extras.Length; i++)
            {
                var extra = extras[i];
                if (extra == null || !extra.IsEmitting || extra is not MonoBehaviour body || body == null)
                {
                    continue;
                }

                var id = body.GetInstanceID();
                if (spokenStrings.Contains(id) || !AtCell(extra.WorldOrigin, x, y))
                {
                    continue;
                }

                spokenStrings.Add(id);
                AppendSource(sequence, extra, MaterialId.None, WeaveKind.String);
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

        static void AppendCreature(
            List<WeaveGlyph> sequence,
            string title,
            RuneId wash,
            IReadOnlyList<RuneId> formula,
            WeaveKind kind,
            bool atHead = false)
        {
            if (formula == null || formula.Count == 0)
            {
                return;
            }

            var written = new List<RuneId>(formula.Count);
            var living = false;
            for (var i = 0; i < formula.Count; i++)
            {
                if (formula[i] == RuneId.None)
                {
                    continue;
                }

                written.Add(formula[i]);
                if (formula[i] == RuneId.Vita)
                {
                    living = true;
                }
            }

            if (written.Count == 0)
            {
                return;
            }

            // Living recipes stay as written. Life is a mark, not a join to unfold.
            var glyphs = new List<WeaveGlyph>(written.Count);
            if (written.Count == 1)
            {
                glyphs.Add(new WeaveGlyph(written[0], wash, MaterialId.None, kind, 0, 0, 1, title, living));
            }
            else
            {
                var id = NextGroup++;
                for (var i = 0; i < written.Count; i++)
                {
                    glyphs.Add(new WeaveGlyph(
                        written[i], wash, MaterialId.None, kind, id, i, written.Count, title, living));
                }
            }

            if (atHead)
            {
                sequence.InsertRange(0, glyphs);
            }
            else
            {
                sequence.AddRange(glyphs);
            }
        }

        static RuneId CreatureWash(EncounterLock creature)
        {
            if (creature.Formula != null)
            {
                for (var i = 0; i < creature.Formula.Length; i++)
                {
                    if (creature.Formula[i] == RuneId.Vita)
                    {
                        return RuneId.Vita;
                    }
                }
            }

            return creature.Ensouled ? RuneId.Mercury : (creature.Formula != null && creature.Formula.Length > 0
                ? creature.Formula[0]
                : RuneId.Salt);
        }

        static void AppendRune(List<WeaveGlyph> sequence, RuneId rune, MaterialId material, WeaveKind kind)
        {
            var recipe = new List<RuneId>(8);
            ChainBook.ExpandRecipe(rune, recipe);
            if (recipe.Count > 1)
            {
                var id = NextGroup++;
                for (var i = 0; i < recipe.Count; i++)
                {
                    sequence.Add(new WeaveGlyph(recipe[i], rune, material, kind, id, i, recipe.Count));
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
