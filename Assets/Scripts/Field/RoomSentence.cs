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
    /// unfolded. Off-screen tiles do not speak. The adept is mind, body,
    /// and soul. Air is ambient wherever the room can be breathed.
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
            var live = new HashSet<RuneId>();

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

                    AppendTile(sequence, tile, scatter, live, ref lastMaterial, ref lastWasTear);
                    AppendHere(sequence, scatter, locks, strings, extras, x, y, spokenLocks, spokenStrings);
                }
            }

            EnsureLiveRunes(scatter, live);
            ScatterComposing(sequence, scatter);

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

        static void CollectLive(WorldTile tile, HashSet<RuneId> live)
        {
            if (tile == null || live == null)
            {
                return;
            }

            if (tile.Element != RuneId.None)
            {
                live.Add(tile.Element);
            }

            var emission = tile.Emission;
            if (emission != null)
            {
                for (var i = 0; i < emission.Count; i++)
                {
                    if (emission[i] != RuneId.None)
                    {
                        live.Add(emission[i]);
                    }
                }
            }

            if (tile.Fire > 0.05f)
            {
                live.Add(RuneId.Fire);
            }

            if (tile.Wet > 0.2f)
            {
                live.Add(RuneId.Water);
            }

            if (tile.Charge > 0.2f)
            {
                live.Add(RuneId.Spark);
                live.Add(RuneId.Fire);
                live.Add(RuneId.Air);
            }

            if (tile.HasFog)
            {
                CoverCatalog.Speak(TileCover.Fog, live);
            }

            if (tile.HasMiasma)
            {
                CoverCatalog.Speak(TileCover.Miasma, live);
            }

            if (tile.IsPoisonWater)
            {
                CoverCatalog.Speak(TileCover.Poison, live);
            }

            CollectCover(tile, live);

            if (tile.Growth > 0 || tile.IsPlantish)
            {
                live.Add(RuneId.Plant);
                live.Add(RuneId.Vita);
            }

            switch (tile.Material)
            {
                case MaterialId.Ice:
                case MaterialId.Snow:
                    live.Add(RuneId.Ice);
                    live.Add(RuneId.Water);
                    live.Add(RuneId.Earth);
                    break;
                case MaterialId.Glacier:
                    live.Add(RuneId.Glacier);
                    live.Add(RuneId.Ice);
                    live.Add(RuneId.Animus);
                    live.Add(RuneId.Water);
                    live.Add(RuneId.Earth);
                    break;
                case MaterialId.Water:
                    live.Add(RuneId.Water);
                    break;
                case MaterialId.Hearth:
                case MaterialId.Ember:
                    live.Add(RuneId.Fire);
                    live.Add(RuneId.Flame);
                    break;
                case MaterialId.Lava:
                    live.Add(RuneId.Lava);
                    live.Add(RuneId.Fire);
                    live.Add(RuneId.Earth);
                    break;
                case MaterialId.Dirt:
                    live.Add(RuneId.Earth);
                    break;
                case MaterialId.Vein:
                case MaterialId.Metal:
                    live.Add(RuneId.Spark);
                    live.Add(RuneId.Fire);
                    live.Add(RuneId.Air);
                    break;
            }
        }

        /// <summary>
        /// A covering answers the current catalog, same as an inscription
        /// of that mark. Ice cover on stone still speaks Ice.
        /// </summary>
        static void CollectCover(WorldTile tile, HashSet<RuneId> live)
        {
            if (tile == null)
            {
                return;
            }

            CoverCatalog.Speak(tile.Cover, live);
            if (tile.Cover == TileCover.None && tile.CoverMaterial != MaterialId.None)
            {
                CoverCatalog.SpeakMaterial(tile.CoverMaterial, live);
            }
        }

        static void EnsureLiveRunes(List<WeaveGlyph> scatter, HashSet<RuneId> live)
        {
            if (scatter == null || live == null || live.Count == 0)
            {
                return;
            }

            foreach (var rune in live)
            {
                if (rune == RuneId.None)
                {
                    continue;
                }

                scatter.Add(new WeaveGlyph(rune, MaterialId.None, WeaveKind.Material));
                if (ChainBook.IsWrought(rune))
                {
                    CollectComposing(scatter, rune, MaterialId.None);
                }
            }
        }

        static void AppendTile(
            List<WeaveGlyph> sequence,
            WorldTile tile,
            List<WeaveGlyph> scatter,
            HashSet<RuneId> live,
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
            CollectLive(tile, live);
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
                    AppendRune(sequence, scatter, signature[i], material, WeaveKind.Material);
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

        static void ScatterComposing(List<WeaveGlyph> sequence, List<WeaveGlyph> extras)
        {
            if (sequence == null || extras == null || extras.Count == 0)
            {
                return;
            }

            extras.Sort(CompareGlyphs);
            var span = sequence.Count;
            for (var i = 0; i < extras.Count; i++)
            {
                var at = span <= 0
                    ? sequence.Count
                    : StableSlot(extras[i], i, span + 1);
                sequence.Insert(at, extras[i]);
                span++;
            }
        }

        static int CompareGlyphs(WeaveGlyph a, WeaveGlyph b)
        {
            var shown = a.Shown.CompareTo(b.Shown);
            if (shown != 0)
            {
                return shown;
            }

            var join = a.Rune.CompareTo(b.Rune);
            return join != 0 ? join : a.Material.CompareTo(b.Material);
        }

        static int StableSlot(WeaveGlyph glyph, int order, int modulus)
        {
            if (modulus <= 1)
            {
                return 0;
            }

            unchecked
            {
                var hash = (int)glyph.Shown * 73856093
                    ^ (int)glyph.Rune * 19349663
                    ^ (int)glyph.Material * 83492791
                    ^ order * 39916801;
                var wrapped = hash % modulus;
                return wrapped < 0 ? wrapped + modulus : wrapped;
            }
        }

        static void AppendHere(
            List<WeaveGlyph> sequence,
            List<WeaveGlyph> scatter,
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
                            EncounterLock.WithLife(creature.Formula),
                            WeaveKind.Lock,
                            markLiving: true);
                    }
                    else
                    {
                        AppendSource(sequence, scatter, source, MaterialId.None, WeaveKind.Lock);
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
                            AppendRune(sequence, scatter, runes[r], MaterialId.None, WeaveKind.String);
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
                AppendSource(sequence, scatter, extra, MaterialId.None, WeaveKind.String);
            }
        }

        static void AppendSource(
            List<WeaveGlyph> sequence,
            List<WeaveGlyph> scatter,
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
                    AppendRune(sequence, scatter, buffer[i], material, kind);
                }
            }
        }

        static void AppendCreature(
            List<WeaveGlyph> sequence,
            string title,
            RuneId wash,
            IReadOnlyList<RuneId> formula,
            WeaveKind kind,
            bool atHead = false,
            bool markLiving = false)
        {
            if (formula == null || formula.Count == 0)
            {
                return;
            }

            var written = new List<RuneId>(formula.Count + 1);
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

            if (markLiving && !living)
            {
                written.Add(RuneId.Vita);
                living = true;
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

        static void AppendRune(
            List<WeaveGlyph> sequence,
            List<WeaveGlyph> scatter,
            RuneId rune,
            MaterialId material,
            WeaveKind kind)
        {
            if (rune == RuneId.None)
            {
                return;
            }

            if (ChainBook.IsWrought(rune))
            {
                sequence.Add(new WeaveGlyph(rune, rune, material, kind, 0, 0, 1));
                CollectComposing(scatter, rune, material);
                return;
            }

            sequence.Add(new WeaveGlyph(rune, material, kind));
        }

        static bool AtCell(Vector3 world, int x, int y)
        {
            return Mathf.FloorToInt(world.x) == x && Mathf.FloorToInt(world.y) == y;
        }

        public static void Audit(System.Collections.Generic.List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (!ChainBook.IsWrought(RuneId.Spark)
                || !ChainBook.IsWrought(RuneId.Ice)
                || !ChainBook.IsWrought(RuneId.Lightning)
                || !ChainBook.IsWrought(RuneId.Plant))
            {
                broken.Add("Spark, Ice, Lightning, and Plant must stand as themselves in the weave");
            }
        }
    }
}
