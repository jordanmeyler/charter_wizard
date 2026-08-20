using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// The on-screen weave, read only in the Charter. Exploring shows tiles,
    /// not glyphs. The sentence is everything in the camera view, laid
    /// boustrophedon and scrolled sideways. Off-screen runes cannot be used.
    /// </summary>
    public sealed class RuneTapestry : MonoBehaviour
    {
        public const float PerceptionRadius = 8.2f;
        public const int Rows = 4;
        public const int Cols = 12;
        const float ScrollSpeed = 0.55f;

        SanctumDirector _director;
        WorldGrid _grid;
        ISpellLock[] _locks;
        readonly List<WeaveGlyph> _sequence = new();
        readonly HashSet<RuneId> _vicinity = new();
        readonly List<RuneId> _vicinityList = new();
        int _viewKey = int.MinValue;
        string _readingText = string.Empty;

        public string Reading => _readingText;
        public IReadOnlyList<WeaveGlyph> Sequence => _sequence;
        public IReadOnlyList<RuneId> Vicinity => _vicinityList;
        public float Scroll { get; private set; }
        public bool Showing => _director != null && _director.Mode == PlayMode.Charter && _sequence.Count > 0;

        public void Bind(SanctumDirector director, SanctumBuild build)
        {
            _director = director;
            _grid = build != null ? build.Grid : null;
            _locks = build != null ? build.Locks : null;
            Scroll = 0f;
        }

        public bool InVicinity(RuneId rune)
        {
            return rune != RuneId.None && _vicinity.Contains(rune);
        }

        public void Resample()
        {
            var view = FieldView.OnScreen();
            Rebuild(view, FieldView.Key(view));
        }

        public static List<RuneId> Perceive(Vector3 origin, WorldGrid grid, ISpellLock[] locks)
        {
            var view = FieldView.OnScreen();
            var strings = Object.FindObjectsByType<RuneStringSource>(FindObjectsSortMode.None);
            var sequence = RoomSentence.Read(grid, locks, strings, view);
            var seen = new List<RuneId>();
            for (var i = 0; i < sequence.Count; i++)
            {
                var rune = sequence[i].Rune;
                if (rune == RuneId.None || seen.Contains(rune))
                {
                    continue;
                }

                seen.Add(rune);
            }

            return seen;
        }

        public bool TryPick(Vector3 world, out RuneId rune)
        {
            rune = RuneId.None;
            return false;
        }

        public WeaveGlyph Cell(int row, int col)
        {
            if (_sequence.Count == 0 || row < 0 || row >= Rows || col < 0 || col >= Cols)
            {
                return new WeaveGlyph(RuneId.None, MaterialId.None, WeaveKind.Tear);
            }

            var along = row % 2 == 0 ? col : Cols - 1 - col;
            var visual = row * Cols + along;
            var index = Mod(Mathf.FloorToInt(Scroll) + visual, _sequence.Count);
            return _sequence[index];
        }

        void LateUpdate()
        {
            if (_director == null || _director.Mode != PlayMode.Charter)
            {
                _readingText = string.Empty;
                return;
            }

            var view = FieldView.OnScreen();
            var key = FieldView.Key(view);
            if (key != _viewKey || _sequence.Count == 0)
            {
                Rebuild(view, key);
            }

            if (_sequence.Count == 0)
            {
                _readingText = "nothing on the screen speaks";
                return;
            }

            Scroll += Time.unscaledDeltaTime * ScrollSpeed;
            if (Scroll >= _sequence.Count)
            {
                Scroll -= _sequence.Count;
            }

            RefreshReading();
        }

        void Rebuild(Rect view, int key)
        {
            _viewKey = key;
            _sequence.Clear();
            _vicinity.Clear();
            _vicinityList.Clear();
            Scroll = 0f;

            var strings = Object.FindObjectsByType<RuneStringSource>(FindObjectsSortMode.None);
            var read = RoomSentence.Read(_grid, _locks, strings, view);
            for (var i = 0; i < read.Count; i++)
            {
                _sequence.Add(read[i]);
                var rune = read[i].Rune;
                if (rune != RuneId.None && _vicinity.Add(rune))
                {
                    _vicinityList.Add(rune);
                }
            }
        }

        void RefreshReading()
        {
            if (_sequence.Count == 0)
            {
                _readingText = "nothing on the screen speaks";
                return;
            }

            var take = Mathf.Min(10, _sequence.Count);
            var start = Mod(Mathf.FloorToInt(Scroll), _sequence.Count);
            var parts = new List<string>(take);
            for (var i = 0; i < take; i++)
            {
                var glyph = _sequence[Mod(start + i, _sequence.Count)];
                parts.Add(glyph.IsTear ? "—" : RuneCatalog.GlyphOf(glyph.Rune));
            }

            _readingText = "on screen  ·  " + string.Join(" · ", parts);
        }

        static int Mod(int value, int modulus)
        {
            if (modulus <= 0)
            {
                return 0;
            }

            var wrapped = value % modulus;
            return wrapped < 0 ? wrapped + modulus : wrapped;
        }
    }
}
