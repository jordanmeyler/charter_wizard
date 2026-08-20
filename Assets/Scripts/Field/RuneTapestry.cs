using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// The living field. Tiles, locks, and later world-strings speak runes
    /// into a layer that drifts and weaves. The player reads it, then draws
    /// from it. Glyphs are not glued to their tiles.
    /// </summary>
    public sealed class RuneTapestry : MonoBehaviour
    {
        public const float PerceptionRadius = 8.2f;
        public const float PickRadius = 0.55f;

        const float SampleInterval = 0.22f;
        const float CellSize = 3f;
        const int MaxStrands = 72;
        const int MaxPerRune = 9;

        SanctumDirector _director;
        WorldGrid _grid;
        ISpellLock[] _locks;
        readonly List<TapestryStrand> _strands = new();
        readonly List<IRuneSource> _sources = new();
        readonly List<RuneId> _emission = new();
        readonly Dictionary<RuneId, float> _weights = new();
        readonly List<Well> _wells = new();
        readonly List<Vector3> _tears = new();
        readonly List<(RuneId Rune, float Weight)> _reading = new();
        readonly List<LineRenderer> _stringLines = new();
        float _sampleIn;
        string _readingText = string.Empty;

        struct Well
        {
            public RuneId Rune;
            public Vector3 Position;
            public bool FromString;
            public int StringId;
            public int StringIndex;
        }

        public string Reading => _readingText;

        public void Bind(SanctumDirector director, SanctumBuild build)
        {
            _director = director;
            _grid = build != null ? build.Grid : null;
            _locks = build != null ? build.Locks : null;
            _sampleIn = 0f;
        }

        public static List<RuneId> Perceive(Vector3 origin, WorldGrid grid, ISpellLock[] locks)
        {
            var seen = new List<RuneId>();
            foreach (var rune in RuneCatalog.BasicRunes)
            {
                AddUnique(seen, rune);
            }

            if (grid != null)
            {
                foreach (var tile in grid.All)
                {
                    if (tile == null || !tile.IsEmitting)
                    {
                        continue;
                    }

                    if (Vector2.Distance(origin, tile.WorldOrigin) > PerceptionRadius)
                    {
                        continue;
                    }

                    var emission = tile.Emission;
                    for (var i = 0; i < emission.Count; i++)
                    {
                        AddUnique(seen, emission[i]);
                    }
                }
            }

            if (locks != null)
            {
                foreach (var encounter in locks)
                {
                    if (encounter is not IRuneSource source || !source.IsEmitting)
                    {
                        continue;
                    }

                    if (Vector2.Distance(origin, source.WorldOrigin) > PerceptionRadius)
                    {
                        continue;
                    }

                    var buffer = new List<RuneId>(4);
                    source.Collect(buffer);
                    for (var i = 0; i < buffer.Count; i++)
                    {
                        AddUnique(seen, buffer[i]);
                    }
                }
            }

            var strings = Object.FindObjectsByType<RuneStringSource>(FindObjectsSortMode.None);
            foreach (var sentence in strings)
            {
                if (sentence == null || !sentence.IsEmitting)
                {
                    continue;
                }

                if (Vector2.Distance(origin, sentence.WorldOrigin) > PerceptionRadius)
                {
                    continue;
                }

                foreach (var rune in sentence.Sequence)
                {
                    AddUnique(seen, rune);
                }
            }

            seen.Sort(CompareFieldOrder);
            return seen;
        }

        public bool TryPick(Vector3 world, out RuneId rune)
        {
            TapestryStrand best = null;
            var bestDistance = PickRadius;
            for (var i = 0; i < _strands.Count; i++)
            {
                var strand = _strands[i];
                if (strand == null || strand.Dying)
                {
                    continue;
                }

                var distance = Vector2.Distance(world, strand.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = strand;
                }
            }

            if (best == null)
            {
                rune = RuneId.None;
                return false;
            }

            rune = best.Rune;
            return true;
        }

        void LateUpdate()
        {
            var still = _director != null && _director.Mode == PlayMode.Charter ? 1f : 0f;
            _sampleIn -= Time.deltaTime;
            if (_sampleIn <= 0f)
            {
                _sampleIn = SampleInterval;
                Resample();
            }

            for (var i = _strands.Count - 1; i >= 0; i--)
            {
                var strand = _strands[i];
                if (strand == null)
                {
                    _strands.RemoveAt(i);
                    continue;
                }

                if (strand.Tick(still, Time.deltaTime))
                {
                    Destroy(strand.gameObject);
                    _strands.RemoveAt(i);
                }
            }

            UpdateStringThreads();
        }

        void Resample()
        {
            var origin = ReaderOrigin();
            GatherSources();
            BuildWells(origin);
            ReconcileStrands();
            RefreshReading();
        }

        Vector3 ReaderOrigin()
        {
            var adept = AdeptAvatar.Find();
            return adept != null ? adept.transform.position : transform.position;
        }

        void GatherSources()
        {
            _sources.Clear();
            if (_grid != null)
            {
                foreach (var tile in _grid.All)
                {
                    if (tile != null)
                    {
                        _sources.Add(tile);
                    }
                }
            }

            if (_locks != null)
            {
                foreach (var encounter in _locks)
                {
                    if (encounter is IRuneSource source)
                    {
                        _sources.Add(source);
                    }
                }
            }

            var strings = Object.FindObjectsByType<RuneStringSource>(FindObjectsSortMode.None);
            foreach (var sentence in strings)
            {
                if (sentence != null)
                {
                    _sources.Add(sentence);
                }
            }
        }

        void BuildWells(Vector3 origin)
        {
            _wells.Clear();
            _tears.Clear();
            _weights.Clear();

            var clusters = new Dictionary<(int, int, RuneId), Cluster>(32);
            var stringWells = new List<Well>(8);

            for (var i = 0; i < _sources.Count; i++)
            {
                var source = _sources[i];
                if (source == null)
                {
                    continue;
                }

                if (source is WorldTile tile && tile.Def.TearsTapestry)
                {
                    if (Vector2.Distance(origin, tile.WorldOrigin) <= PerceptionRadius + 1.5f)
                    {
                        _tears.Add(tile.WorldOrigin);
                    }

                    continue;
                }

                if (!source.IsEmitting || Vector2.Distance(origin, source.WorldOrigin) > PerceptionRadius)
                {
                    continue;
                }

                _emission.Clear();
                source.Collect(_emission);
                if (source.SourceKind == RuneSourceKind.String && source is RuneStringSource sentence)
                {
                    LayString(sentence, stringWells);
                    continue;
                }

                for (var r = 0; r < _emission.Count; r++)
                {
                    var rune = _emission[r];
                    if (rune == RuneId.None)
                    {
                        continue;
                    }

                    AddWeight(rune, source.VoiceWeight);
                    var cell = ((int)Mathf.Floor(source.WorldOrigin.x / CellSize),
                        (int)Mathf.Floor(source.WorldOrigin.y / CellSize), rune);
                    if (!clusters.TryGetValue(cell, out var cluster))
                    {
                        cluster = new Cluster { Weight = 0f, Sum = Vector3.zero };
                    }

                    cluster.Weight += source.VoiceWeight;
                    cluster.Sum += source.WorldOrigin * source.VoiceWeight;
                    clusters[cell] = cluster;
                }
            }

            BirthJoins(clusters);

            var ranked = new List<KeyValuePair<(int, int, RuneId), Cluster>>(clusters);
            ranked.Sort((a, b) => b.Value.Weight.CompareTo(a.Value.Weight));
            for (var i = 0; i < ranked.Count; i++)
            {
                var pair = ranked[i];
                var cluster = pair.Value;
                if (cluster.Weight < 0.4f)
                {
                    continue;
                }

                var center = cluster.Sum / Mathf.Max(0.001f, cluster.Weight);
                var count = Mathf.Clamp(Mathf.RoundToInt(Mathf.Sqrt(cluster.Weight) * 1.6f), 1, MaxPerRune);
                for (var n = 0; n < count && _wells.Count < MaxStrands - 8; n++)
                {
                    var scatter = (Vector3)(Random.insideUnitCircle * (0.85f + Mathf.Min(1.4f, cluster.Weight * 0.08f)));
                    _wells.Add(new Well
                    {
                        Rune = pair.Key.Item3,
                        Position = PushFromTears(center + scatter),
                        FromString = false,
                        StringId = 0,
                        StringIndex = -1
                    });
                }
            }

            for (var i = 0; i < stringWells.Count; i++)
            {
                _wells.Add(stringWells[i]);
            }

            if (_wells.Count > MaxStrands)
            {
                _wells.RemoveRange(MaxStrands, _wells.Count - MaxStrands);
            }
        }

        void LayString(RuneStringSource sentence, List<Well> into)
        {
            var sequence = sentence.Sequence;
            var origin = sentence.WorldOrigin;
            var step = sentence.Heading * 0.7f;
            var start = origin - step * ((sequence.Length - 1) * 0.5f);
            for (var i = 0; i < sequence.Length; i++)
            {
                if (sequence[i] == RuneId.None)
                {
                    continue;
                }

                AddWeight(sequence[i], sentence.VoiceWeight);
                into.Add(new Well
                {
                    Rune = sequence[i],
                    Position = PushFromTears(start + step * i),
                    FromString = true,
                    StringId = sentence.StringId,
                    StringIndex = i
                });
            }
        }

        struct Cluster
        {
            public float Weight;
            public Vector3 Sum;
        }

        void BirthJoins(Dictionary<(int, int, RuneId), Cluster> clusters)
        {
            var cells = new Dictionary<(int, int), List<RuneId>>(16);
            foreach (var key in clusters.Keys)
            {
                var cell = (key.Item1, key.Item2);
                if (!cells.TryGetValue(cell, out var runes))
                {
                    runes = new List<RuneId>(4);
                    cells[cell] = runes;
                }

                if (!runes.Contains(key.Item3))
                {
                    runes.Add(key.Item3);
                }
            }

            foreach (var pair in cells)
            {
                var runes = pair.Value;
                for (var i = 0; i < runes.Count; i++)
                {
                    for (var j = i + 1; j < runes.Count; j++)
                    {
                        if (!MaterialTree.TryBlend(runes[i], runes[j], out var blend) ||
                            !IsTeachingJoin(blend.Result))
                        {
                            continue;
                        }

                        var a = clusters[(pair.Key.Item1, pair.Key.Item2, runes[i])];
                        var b = clusters[(pair.Key.Item1, pair.Key.Item2, runes[j])];
                        if (a.Weight < 2.1f || b.Weight < 2.1f)
                        {
                            continue;
                        }

                        var joinKey = (pair.Key.Item1, pair.Key.Item2, blend.Result);
                        if (!clusters.TryGetValue(joinKey, out var join))
                        {
                            join = new Cluster { Weight = 0f, Sum = Vector3.zero };
                        }

                        var weight = Mathf.Min(a.Weight, b.Weight) * 0.45f;
                        join.Weight += weight;
                        join.Sum += (a.Sum / a.Weight + b.Sum / b.Weight) * 0.5f * weight;
                        clusters[joinKey] = join;
                        AddWeight(blend.Result, weight);
                    }
                }
            }
        }

        static bool IsTeachingJoin(RuneId rune)
        {
            switch (rune)
            {
                case RuneId.Spark:
                case RuneId.Cloud:
                case RuneId.Mud:
                case RuneId.Lava:
                case RuneId.Steam:
                case RuneId.Dust:
                case RuneId.Ash:
                case RuneId.Lightning:
                    return true;
                default:
                    return false;
            }
        }

        void AddWeight(RuneId rune, float weight)
        {
            _weights.TryGetValue(rune, out var current);
            _weights[rune] = current + weight;
        }

        Vector3 PushFromTears(Vector3 point)
        {
            for (var i = 0; i < _tears.Count; i++)
            {
                var delta = (Vector2)(point - _tears[i]);
                var distance = delta.magnitude;
                if (distance < 0.95f)
                {
                    var away = distance < 0.001f ? Vector2.right : delta.normalized;
                    point = _tears[i] + (Vector3)(away * 0.95f);
                }
            }

            return point;
        }

        void ReconcileStrands()
        {
            var assigned = new bool[_wells.Count];
            for (var i = 0; i < _strands.Count; i++)
            {
                var strand = _strands[i];
                if (strand == null)
                {
                    continue;
                }

                var best = -1;
                var bestScore = float.MaxValue;
                for (var w = 0; w < _wells.Count; w++)
                {
                    if (assigned[w] || _wells[w].Rune != strand.Rune)
                    {
                        continue;
                    }

                    var score = Vector2.Distance(strand.Home, _wells[w].Position);
                    if (_wells[w].FromString != strand.FromString)
                    {
                        score += 4f;
                    }

                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = w;
                    }
                }

                if (best < 0)
                {
                    strand.BeginFade();
                    continue;
                }

                assigned[best] = true;
                var well = _wells[best];
                strand.Retarget(well.Position, well.FromString, well.StringId, well.StringIndex);
            }

            for (var w = 0; w < _wells.Count && _strands.Count < MaxStrands; w++)
            {
                if (assigned[w])
                {
                    continue;
                }

                SpawnStrand(_wells[w]);
            }
        }

        void SpawnStrand(Well well)
        {
            var host = new GameObject($"Strand_{RuneCatalog.GlyphOf(well.Rune)}");
            host.transform.SetParent(transform, false);
            var strand = host.AddComponent<TapestryStrand>();
            strand.Bind(well.Rune, well.Position, well.FromString, well.StringId, well.StringIndex);
            _strands.Add(strand);
        }

        void RefreshReading()
        {
            _reading.Clear();
            foreach (var pair in _weights)
            {
                _reading.Add((pair.Key, pair.Value));
            }

            _reading.Sort((a, b) => b.Weight.CompareTo(a.Weight));
            if (_reading.Count == 0)
            {
                _readingText = _tears.Count > 0 ? "the weave thins — a tear" : "the field is quiet";
                return;
            }

            var take = Mathf.Min(6, _reading.Count);
            var parts = new string[take];
            for (var i = 0; i < take; i++)
            {
                parts[i] = RuneCatalog.GlyphOf(_reading[i].Rune);
            }

            _readingText = string.Join(" · ", parts);
        }

        void UpdateStringThreads()
        {
            var groups = new Dictionary<int, List<TapestryStrand>>(4);
            for (var i = 0; i < _strands.Count; i++)
            {
                var strand = _strands[i];
                if (strand == null || !strand.FromString || strand.Dying)
                {
                    continue;
                }

                var key = strand.StringId;
                if (!groups.TryGetValue(key, out var list))
                {
                    list = new List<TapestryStrand>(4);
                    groups[key] = list;
                }

                list.Add(strand);
            }

            var needed = 0;
            foreach (var list in groups.Values)
            {
                if (list.Count >= 2)
                {
                    needed++;
                }
            }

            while (_stringLines.Count < needed)
            {
                _stringLines.Add(MakeThread());
            }

            var lineIndex = 0;
            foreach (var list in groups.Values)
            {
                if (list.Count < 2)
                {
                    continue;
                }

                list.Sort((a, b) => a.StringIndex.CompareTo(b.StringIndex));
                var line = _stringLines[lineIndex++];
                line.enabled = true;
                line.positionCount = list.Count;
                for (var i = 0; i < list.Count; i++)
                {
                    line.SetPosition(i, list[i].transform.position);
                }
            }

            for (var i = lineIndex; i < _stringLines.Count; i++)
            {
                _stringLines[i].enabled = false;
            }
        }

        LineRenderer MakeThread()
        {
            var host = new GameObject("StringThread");
            host.transform.SetParent(transform, false);
            var line = host.AddComponent<LineRenderer>();
            line.positionCount = 2;
            line.widthMultiplier = 0.035f;
            line.numCapVertices = 4;
            line.useWorldSpace = true;
            line.sortingOrder = 7;
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                line.material = new Material(shader);
            }

            line.startColor = new Color(0.86f, 0.8f, 0.62f, 0.45f);
            line.endColor = new Color(0.86f, 0.8f, 0.62f, 0.45f);
            return line;
        }

        static void AddUnique(List<RuneId> seen, RuneId rune)
        {
            if (rune == RuneId.None || seen.Contains(rune))
            {
                return;
            }

            seen.Add(rune);
        }

        static int CompareFieldOrder(RuneId left, RuneId right)
        {
            return Rank(left).CompareTo(Rank(right));
        }

        static int Rank(RuneId rune)
        {
            if (RuneCatalog.IsAspect(rune))
            {
                return 200 + (int)rune;
            }

            if (rune == RuneId.Vita || rune == RuneId.Mors)
            {
                return 300 + (int)rune;
            }

            if (rune == RuneId.Lumen || rune == RuneId.Umbra || rune == RuneId.Animus || rune == RuneId.Anima)
            {
                return 400 + (int)rune;
            }

            if (rune == RuneId.Aether)
            {
                return 500;
            }

            switch (rune)
            {
                case RuneId.Fire: return 10;
                case RuneId.Air: return 11;
                case RuneId.Earth: return 12;
                case RuneId.Water: return 13;
                default: return 50 + (int)rune;
            }
        }
    }
}
