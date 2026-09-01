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

        public WeaveGlyph(RuneId rune, MaterialId material, WeaveKind kind, string origin)
            : this(rune, rune, material, kind, 0, 0, 1, null, false, origin)
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
            bool living = false,
            string origin = null)
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
            Origin = origin ?? string.Empty;
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
        public string Origin { get; }
        public bool IsGroup => GroupId != 0 && GroupSize > 1;
        public bool IsTear => Kind == WeaveKind.Tear || (Shown == RuneId.None && Rune == RuneId.None);
        public string GroupTitle => !string.IsNullOrEmpty(Title) ? Title : string.Empty;
    }

    /// <summary>
    /// The weave is what the camera can see. A rune is available when
    /// something on screen speaks it; that mark is valid to string.
    /// Generation puts several of each available rune in the grid so
    /// a lone tile stays clickable, then extra copies from how often
    /// that material appears — uncommon marks take a larger share.
    /// Creature recipes stay as written. You are mind, body, and soul
    /// when you are in view.
    /// </summary>
    public static class RoomSentence
    {
        public const int GridCells = RuneTapestry.Rows * RuneTapestry.DefaultCols;
        /// <summary>
        /// A spoken rune must show up this often. One tile used to
        /// yield one glyph, which the clipped belt could hide.
        /// </summary>
        public const int MinCopiesPerRune = 3;
        public const string PitOrigin = "the pit";
        public const string AirOrigin = "the air";

        static int NextGroup = 1;

        public static List<WeaveGlyph> Read(
            WorldGrid grid,
            ISpellLock[] locks,
            RuneStringSource[] strings,
            Rect view,
            IRuneSource[] extras = null)
        {
            var tally = new Tally();
            if (grid != null && view.width > 0f && view.height > 0f)
            {
                ScanView(tally, grid, locks, strings, extras, view);
            }

            if (tally.Breathable)
            {
                tally.Add(
                    RuneId.Air,
                    new WeaveGlyph(RuneId.Air, MaterialId.None, WeaveKind.Ambient, AirOrigin));
            }

            var sequence = Compose(tally, FieldView.Key(view), GridCells);
            AppendCreature(
                sequence,
                AdeptAvatar.DisplayTitle,
                AdeptAvatar.Wash,
                AdeptAvatar.Formula,
                WeaveKind.Ambient,
                atHead: true);
            return sequence;
        }

        /// <summary>
        /// Hover text: where this mark is from. Only what the
        /// camera can see speaks. Dark is the pit. Air is breath.
        /// </summary>
        public static string OriginOf(WeaveGlyph glyph)
        {
            if (!string.IsNullOrEmpty(glyph.Origin))
            {
                return glyph.Origin;
            }

            if (!string.IsNullOrEmpty(glyph.Title))
            {
                return glyph.Title == AdeptAvatar.DisplayTitle ? "you" : glyph.Title;
            }

            if (glyph.IsTear || glyph.Material == MaterialId.Void)
            {
                return PitOrigin;
            }

            if (glyph.Kind == WeaveKind.Ambient)
            {
                return glyph.Shown == RuneId.Air || glyph.Rune == RuneId.Air
                    ? AirOrigin
                    : "the room";
            }

            if (glyph.Material != MaterialId.None)
            {
                return MaterialCatalog.Of(glyph.Material).Name;
            }

            if (glyph.Kind == WeaveKind.String)
            {
                return "an inscription";
            }

            if (glyph.Kind == WeaveKind.Lock)
            {
                return "a lock";
            }

            return "the room";
        }

        /// <summary>
        /// Build a weave grid from camera tallies. Every available
        /// rune appears often enough to click; leftover cells follow
        /// material frequency with uncommon marks lifted. Used by
        /// play and by the audit.
        /// </summary>
        public static List<WeaveGlyph> Compose(Tally tally, int seed, int cells)
        {
            var sequence = new List<WeaveGlyph>(Mathf.Max(16, cells));
            if (tally == null)
            {
                return sequence;
            }

            for (var i = 0; i < tally.Groups.Count; i++)
            {
                var group = tally.Groups[i];
                if (group != null && group.Count > 0)
                {
                    sequence.AddRange(group);
                }
            }

            var already = new HashSet<RuneId>();
            for (var i = 0; i < sequence.Count; i++)
            {
                already.Add(sequence[i].Shown);
            }

            var marks = new List<WeaveGlyph>(cells);
            foreach (var pair in tally.Count)
            {
                if (pair.Key == RuneId.None || already.Contains(pair.Key))
                {
                    continue;
                }

                marks.Add(tally.GlyphOf(pair.Key));
                already.Add(pair.Key);
            }

            if (tally.Pits > 0)
            {
                marks.Add(new WeaveGlyph(RuneId.None, MaterialId.Void, WeaveKind.Tear, PitOrigin));
            }

            EnsureMinimumCopies(marks, tally, sequence);
            var used = sequence.Count + marks.Count;
            var extras = Mathf.Max(0, cells - used);
            FillByFrequency(marks, tally, extras, seed);
            Shuffle(marks, seed == int.MinValue ? 1 : seed);
            sequence.AddRange(marks);
            return sequence;
        }

        public sealed class Tally
        {
            public readonly Dictionary<RuneId, int> Count = new();
            public readonly Dictionary<RuneId, WeaveGlyph> Template = new();
            public readonly List<List<WeaveGlyph>> Groups = new();
            public int Pits;
            public bool Breathable;

            public void Add(RuneId rune, WeaveGlyph glyph, int n = 1)
            {
                if (rune == RuneId.None || n <= 0)
                {
                    return;
                }

                if (!Count.TryGetValue(rune, out var have))
                {
                    have = 0;
                }

                Count[rune] = have + n;
                if (!Template.ContainsKey(rune))
                {
                    Template[rune] = glyph;
                }
            }

            public WeaveGlyph GlyphOf(RuneId rune)
            {
                return Template.TryGetValue(rune, out var glyph)
                    ? glyph
                    : new WeaveGlyph(rune, MaterialId.None, WeaveKind.Material);
            }
        }

        static void EnsureMinimumCopies(List<WeaveGlyph> marks, Tally tally, List<WeaveGlyph> groups)
        {
            if (marks == null || tally == null || tally.Count.Count == 0)
            {
                return;
            }

            var have = new Dictionary<RuneId, int>();
            CountShownInto(have, groups);
            CountShownInto(have, marks);

            var rarest = new List<KeyValuePair<RuneId, int>>(tally.Count.Count);
            foreach (var pair in tally.Count)
            {
                if (pair.Key != RuneId.None && pair.Value > 0)
                {
                    rarest.Add(pair);
                }
            }

            rarest.Sort((a, b) => a.Value.CompareTo(b.Value));
            for (var i = 0; i < rarest.Count; i++)
            {
                var rune = rarest[i].Key;
                have.TryGetValue(rune, out var n);
                while (n < MinCopiesPerRune)
                {
                    marks.Add(tally.GlyphOf(rune));
                    n++;
                }
            }
        }

        static void CountShownInto(Dictionary<RuneId, int> dest, List<WeaveGlyph> glyphs)
        {
            if (dest == null || glyphs == null)
            {
                return;
            }

            for (var i = 0; i < glyphs.Count; i++)
            {
                var rune = glyphs[i].Shown;
                if (rune == RuneId.None)
                {
                    continue;
                }

                dest.TryGetValue(rune, out var n);
                dest[rune] = n + 1;
            }
        }

        /// <summary>
        /// Soften raw tile counts so a puddle still takes a real
        /// share of leftover cells. A floor of stone still speaks
        /// more. 1 → 4, 4 → 8, 16 → 16, 36 → 24.
        /// </summary>
        static int SpeakWeight(int count)
        {
            return Mathf.Max(1, Mathf.RoundToInt(4f * Mathf.Sqrt(Mathf.Max(1, count))));
        }

        static void FillByFrequency(List<WeaveGlyph> marks, Tally tally, int extras, int seed)
        {
            if (marks == null || tally == null || extras <= 0 || tally.Count.Count == 0)
            {
                return;
            }

            var runes = new List<RuneId>(tally.Count.Count);
            var weights = new List<int>(tally.Count.Count);
            var total = 0;
            foreach (var pair in tally.Count)
            {
                if (pair.Key == RuneId.None || pair.Value <= 0)
                {
                    continue;
                }

                var weight = SpeakWeight(pair.Value);
                runes.Add(pair.Key);
                weights.Add(weight);
                total += weight;
            }

            if (total <= 0 || runes.Count == 0)
            {
                return;
            }

            var rng = new System.Random(seed == int.MinValue ? 2 : seed ^ 0x5bd1e995);
            for (var i = 0; i < extras; i++)
            {
                var pick = rng.Next(0, total);
                var walk = 0;
                var rune = runes[0];
                for (var r = 0; r < runes.Count; r++)
                {
                    walk += weights[r];
                    if (pick < walk)
                    {
                        rune = runes[r];
                        break;
                    }
                }

                marks.Add(tally.GlyphOf(rune));
            }
        }

        static void Shuffle(List<WeaveGlyph> marks, int seed)
        {
            if (marks == null || marks.Count < 2)
            {
                return;
            }

            var rng = new System.Random(seed);
            for (var i = marks.Count - 1; i > 0; i--)
            {
                var j = rng.Next(0, i + 1);
                var swap = marks[i];
                marks[i] = marks[j];
                marks[j] = swap;
            }
        }

        static void ScanView(
            Tally tally,
            WorldGrid grid,
            ISpellLock[] locks,
            RuneStringSource[] strings,
            IRuneSource[] extras,
            Rect view)
        {
            var x0 = Mathf.FloorToInt(view.xMin);
            var x1 = Mathf.FloorToInt(view.xMax);
            var y0 = Mathf.FloorToInt(view.yMin);
            var y1 = Mathf.FloorToInt(view.yMax);
            var spoken = new HashSet<RuneId>();
            var seenActors = new HashSet<int>();

            for (var y = y0; y <= y1; y++)
            {
                for (var x = x0; x <= x1; x++)
                {
                    if (!FieldView.ContainsTile(view, x, y))
                    {
                        continue;
                    }

                    CountTile(tally, grid.Get(x, y), spoken);
                }
            }

            CountActors(tally, locks, strings, extras, view, seenActors);
        }

        static void CountTile(Tally tally, WorldTile tile, HashSet<RuneId> spoken)
        {
            if (tally == null || tile == null)
            {
                return;
            }

            if (tile.Def.TearsTapestry)
            {
                tally.Pits++;
                tally.Add(
                    RuneId.Umbra,
                    new WeaveGlyph(RuneId.Umbra, MaterialId.Void, WeaveKind.Material, PitOrigin));
                return;
            }

            tally.Breathable = true;
            spoken.Clear();
            CollectSpoken(tile, spoken);
            var material = tile.Material;
            var origin = material != MaterialId.None
                ? MaterialCatalog.Of(material).Name
                : "the room";
            foreach (var rune in spoken)
            {
                tally.Add(rune, new WeaveGlyph(rune, material, WeaveKind.Material, origin));
                ExpandComposing(tally, rune, material, WeaveKind.Material, origin);
            }
        }

        static void CollectSpoken(WorldTile tile, HashSet<RuneId> dest)
        {
            CollectLive(tile, dest);
            if (tile == null || dest == null)
            {
                return;
            }

            var material = tile.Material;
            if (material == MaterialId.None)
            {
                return;
            }

            var manifestation = MaterialCatalog.Of(material).Manifestation;
            if (manifestation != RuneId.None)
            {
                dest.Add(manifestation);
            }
        }

        static void CountActors(
            Tally tally,
            ISpellLock[] locks,
            RuneStringSource[] strings,
            IRuneSource[] extras,
            Rect view,
            HashSet<int> seen)
        {
            if (locks != null)
            {
                for (var i = 0; i < locks.Length; i++)
                {
                    if (locks[i] is not MonoBehaviour body || body == null)
                    {
                        continue;
                    }

                    if (locks[i] is not IRuneSource source || !source.IsEmitting || !VisibleIn(source, view))
                    {
                        continue;
                    }

                    var id = body.GetInstanceID();
                    if (!seen.Add(id))
                    {
                        continue;
                    }

                    if (locks[i] is EncounterLock creature && creature.Formula != null && creature.Formula.Length > 0)
                    {
                        var group = new List<WeaveGlyph>(8);
                        AppendCreature(
                            group,
                            creature.DisplayName,
                            CreatureWash(creature),
                            EncounterLock.WithLife(creature.Formula),
                            WeaveKind.Lock,
                            markLiving: true);
                        if (group.Count > 0)
                        {
                            tally.Groups.Add(group);
                        }
                    }
                    else
                    {
                        CountSource(tally, source, WeaveKind.Lock, "a lock");
                    }
                }
            }

            if (strings != null)
            {
                for (var i = 0; i < strings.Length; i++)
                {
                    var sentence = strings[i];
                    if (sentence == null || !sentence.IsEmitting || !VisibleIn(sentence, view))
                    {
                        continue;
                    }

                    if (!seen.Add(sentence.StringId))
                    {
                        continue;
                    }

                    var runes = sentence.Sequence;
                    for (var r = 0; r < runes.Length; r++)
                    {
                        if (runes[r] != RuneId.None)
                        {
                            tally.Add(
                                runes[r],
                                new WeaveGlyph(runes[r], MaterialId.None, WeaveKind.String, "an inscription"));
                            ExpandComposing(
                                tally,
                                runes[r],
                                MaterialId.None,
                                WeaveKind.String,
                                "an inscription");
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

                if (!VisibleIn(extra, view) || !seen.Add(body.GetInstanceID()))
                {
                    continue;
                }

                CountSource(tally, extra, WeaveKind.String, "an inscription");
            }
        }

        static void CountSource(Tally tally, IRuneSource source, WeaveKind kind, string origin)
        {
            if (tally == null || source == null)
            {
                return;
            }

            var buffer = new List<RuneId>(6);
            source.Collect(buffer);
            for (var i = 0; i < buffer.Count; i++)
            {
                if (buffer[i] != RuneId.None)
                {
                    tally.Add(buffer[i], new WeaveGlyph(buffer[i], MaterialId.None, kind, origin));
                    ExpandComposing(tally, buffer[i], MaterialId.None, kind, origin);
                }
            }
        }

        /// <summary>
        /// A wrought join that already stands (Spark, Plant, Ice) is
        /// itself in the weave. The basics that compose it are still
        /// there, strewn by frequency — not glued to the join.
        /// </summary>
        static void ExpandComposing(
            Tally tally,
            RuneId wrought,
            MaterialId material,
            WeaveKind kind,
            string origin)
        {
            if (tally == null || wrought == RuneId.None || !ChainBook.IsWrought(wrought))
            {
                return;
            }

            var recipe = new List<RuneId>(8);
            ChainBook.ExpandRecipe(wrought, recipe);
            for (var i = 0; i < recipe.Count; i++)
            {
                if (recipe[i] != RuneId.None && recipe[i] != wrought)
                {
                    tally.Add(recipe[i], new WeaveGlyph(recipe[i], material, kind, origin));
                }
            }
        }

        static bool VisibleIn(IRuneSource source, Rect view)
        {
            return source != null && FieldView.ContainsWorld(view, source.WorldOrigin);
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
                    live.Add(RuneId.Fire);
                    live.Add(RuneId.Flame);
                    break;
                case MaterialId.Ember:
                    live.Add(RuneId.Fire);
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

            var pitDark = new WeaveGlyph(RuneId.Umbra, MaterialId.Void, WeaveKind.Material, PitOrigin);
            if (OriginOf(pitDark) != PitOrigin)
            {
                broken.Add("Dark must read as from the pit");
            }

            var tear = new WeaveGlyph(RuneId.None, MaterialId.Void, WeaveKind.Tear);
            if (OriginOf(tear) != PitOrigin)
            {
                broken.Add("A tear must read as from the pit");
            }

            var tally = new Tally();
            var stone = new WeaveGlyph(RuneId.Earth, MaterialId.Stone, WeaveKind.Material, "stone");
            tally.Add(RuneId.Earth, stone, 20);
            tally.Add(RuneId.Salt, new WeaveGlyph(RuneId.Salt, MaterialId.Stone, WeaveKind.Material, "stone"), 20);
            tally.Add(RuneId.Stone, new WeaveGlyph(RuneId.Stone, MaterialId.Stone, WeaveKind.Material, "stone"), 20);
            tally.Add(RuneId.Water, new WeaveGlyph(RuneId.Water, MaterialId.Water, WeaveKind.Material, "water"), 2);
            var grid = Compose(tally, 7, GridCells);
            if (CountShown(grid, RuneId.Lumen) > 0 || CountShown(grid, RuneId.Umbra) > 0)
            {
                broken.Add("The weave must not invent Light or Dark the camera did not speak");
            }

            if (CountShown(grid, RuneId.Earth) < MinCopiesPerRune
                || CountShown(grid, RuneId.Salt) < MinCopiesPerRune
                || CountShown(grid, RuneId.Stone) < MinCopiesPerRune
                || CountShown(grid, RuneId.Water) < MinCopiesPerRune)
            {
                broken.Add("Each available rune must appear often enough to click in the generated grid");
            }

            if (CountShown(grid, RuneId.Earth) <= CountShown(grid, RuneId.Water))
            {
                broken.Add("A frequent material must speak more often than a rare one");
            }

            var lone = new Tally();
            lone.Add(RuneId.Earth, stone, 30);
            lone.Add(RuneId.Salt, new WeaveGlyph(RuneId.Salt, MaterialId.Stone, WeaveKind.Material, "stone"), 30);
            lone.Add(RuneId.Stone, new WeaveGlyph(RuneId.Stone, MaterialId.Stone, WeaveKind.Material, "stone"), 30);
            lone.Add(RuneId.Fire, new WeaveGlyph(RuneId.Fire, MaterialId.Hearth, WeaveKind.Material, "hearth"), 1);
            var loneGrid = Compose(lone, 11, GridCells);
            if (CountShown(loneGrid, RuneId.Fire) < MinCopiesPerRune)
            {
                broken.Add("A single on-screen tile must speak often enough to cast");
            }

            if (CountShown(loneGrid, RuneId.Earth) <= CountShown(loneGrid, RuneId.Fire))
            {
                broken.Add("A frequent material must still speak more often than a single tile");
            }

            var pits = new Tally();
            pits.Pits = 6;
            pits.Add(RuneId.Umbra, pitDark, 6);
            var pitGrid = Compose(pits, 3, GridCells);
            if (CountShown(pitGrid, RuneId.Umbra) < 1 || CountShown(pitGrid, RuneId.Lumen) > 0)
            {
                broken.Add("Pits must speak Dark and must not invent Light");
            }

            var spark = new Tally();
            spark.Add(RuneId.Spark, new WeaveGlyph(RuneId.Spark, MaterialId.Metal, WeaveKind.Material, "metal"));
            ExpandComposing(spark, RuneId.Spark, MaterialId.Metal, WeaveKind.Material, "metal");
            var sparkGrid = Compose(spark, 5, GridCells);
            if (CountShown(sparkGrid, RuneId.Spark) < MinCopiesPerRune
                || CountShown(sparkGrid, RuneId.Fire) < MinCopiesPerRune
                || CountShown(sparkGrid, RuneId.Air) < MinCopiesPerRune
                || CountShown(sparkGrid, RuneId.Lumen) > 0)
            {
                broken.Add("A stood Spark must stand as itself with Fire and Air, and must not invent Light");
            }
        }

        static int CountShown(List<WeaveGlyph> sequence, RuneId rune)
        {
            var n = 0;
            if (sequence == null)
            {
                return 0;
            }

            for (var i = 0; i < sequence.Count; i++)
            {
                if (sequence[i].Shown == rune)
                {
                    n++;
                }
            }

            return n;
        }
    }
}
