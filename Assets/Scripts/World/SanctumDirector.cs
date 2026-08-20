using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public enum PlayMode
    {
        Exploring,
        Charter,
        Paused
    }

    public sealed class SanctumDirector : MonoBehaviour
    {
        public SpellComposer Composer { get; } = new();
        public Grimoire Grimoire { get; } = new();
        public ISpellLock CurrentTarget { get; private set; }
        public RoomInfo CurrentRoom { get; private set; }
        public WorldTile Underfoot { get; private set; }
        public string LastLog { get; private set; } = "Walk. Space reads the field. Esc lists every written recipe.";
        public float Taint { get; private set; }
        public WorldGrid Grid { get; private set; }
        public PlayMode Mode { get; private set; } = PlayMode.Exploring;
        public StoredSpell Held { get; private set; } = StoredSpell.Empty;
        public IReadOnlyList<RuneId> VisibleRunes { get; private set; } = System.Array.Empty<RuneId>();

        readonly CastResolver _resolver = new();
        CharterWall _wall;
        ISpellLock[] _locks;
        RoomInfo[] _rooms;
        Vector3 _safePoint;
        bool _finished;
        PlayMode _modeBeforePause = PlayMode.Exploring;

        public void BindWall(CharterWall wall)
        {
            _wall = wall;
        }

        public void Begin(SanctumBuild build)
        {
            Grid = build.Grid;
            _locks = build.Locks;
            _rooms = build.Rooms;
            _safePoint = build.Spawn;
            CurrentRoom = _rooms != null && _rooms.Length > 0 ? _rooms[0] : null;
        }

        void Update()
        {
            TrackPlayer();
            CurrentTarget = FindNearestLock();
            HandleInput();
        }

        void OnDisable()
        {
            Time.timeScale = 1f;
        }

        void TrackPlayer()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null || Grid == null)
            {
                return;
            }

            Underfoot = Grid.TileAtWorld(player.transform.position);
            if (_rooms != null)
            {
                foreach (var room in _rooms)
                {
                    if (room.Contains(player.transform.position))
                    {
                        CurrentRoom = room;
                        break;
                    }
                }
            }

            if (Underfoot != null && Underfoot.Kind == TileKind.Floor)
            {
                _safePoint = WorldGrid.Center(Underfoot.Coord.x, Underfoot.Coord.y);
            }
        }

        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
                return;
            }

            if (Mode == PlayMode.Paused)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (Mode == PlayMode.Charter)
                {
                    CloseCharter();
                }
                else
                {
                    OpenCharter();
                }

                return;
            }

            if (Mode == PlayMode.Charter)
            {
                HandleCharterInput();
                if (Input.GetMouseButtonDown(0))
                {
                    TryClickCharterRune();
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Return))
            {
                CastHeld();
            }
        }

        void HandleCharterInput()
        {
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Q))
            {
                Composer.ToggleStance();
                Log(Composer.Stance == CastingStance.Charter
                    ? "Bound stance. Reliable. The Charter siphons a little light."
                    : "Unbound stance. Higher magnitude, variance-loaded. Never the required key.");
            }

            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Backspace))
            {
                if (Composer.IsEmpty)
                {
                    Log("The string is already empty.");
                    return;
                }

                Composer.TryRemoveAt(Composer.Count - 1, out var note);
                Log(note);
            }

            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Return))
            {
                if (!Composer.IsEmpty)
                {
                    CastDraft();
                }
                else if (Held.Occupied)
                {
                    CastHeld();
                    CloseCharter();
                }
                else
                {
                    Log("Nothing is strung. Choose runes, or store a form first.");
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                StoreDraft();
            }
        }

        public void OpenCharter()
        {
            Mode = PlayMode.Charter;
            RefreshVisibleRunes();
            _wall?.Show(VisibleRunes, Camera.main);
            Log("The field stands still. String runes, then Cast or Store.");
        }

        public void CloseCharter()
        {
            if (Mode != PlayMode.Charter)
            {
                return;
            }

            Mode = PlayMode.Exploring;
            _wall?.Hide();
            if (string.IsNullOrEmpty(LastLog) || LastLog.StartsWith("The field stands still"))
            {
                Log(Held.Occupied
                    ? $"The wall folds. You still hold {Held.Name}."
                    : "The wall folds. The field keeps moving.");
            }
        }

        public void TogglePause()
        {
            if (Mode == PlayMode.Paused)
            {
                Time.timeScale = 1f;
                Mode = _modeBeforePause;
                if (Mode == PlayMode.Charter)
                {
                    _wall?.Show(VisibleRunes, Camera.main);
                }

                return;
            }

            _modeBeforePause = Mode;
            Mode = PlayMode.Paused;
            _wall?.Hide();
            Time.timeScale = 0f;
        }

        void TryClickCharterRune()
        {
            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var world = camera.ScreenToWorldPoint(Input.mousePosition);
            var hits = Physics2D.OverlapPointAll(world);
            foreach (var hit in hits)
            {
                var card = hit.GetComponent<CharterRune>();
                if (card != null)
                {
                    AddRune(card.Rune);
                    return;
                }
            }
        }

        public void AddRune(RuneId rune)
        {
            if (Mode != PlayMode.Charter)
            {
                return;
            }

            Composer.TryAdd(rune, out var note);
            Log(note);
        }

        public void RemoveDraftFrom(int index)
        {
            if (Mode != PlayMode.Charter)
            {
                return;
            }

            Composer.TryRemoveAt(index, out var note);
            Log(note);
        }

        public void ClearDraft()
        {
            Composer.Clear();
            Log("The composition is released back into the field.");
        }

        public void CastDraft()
        {
            if (Composer.IsEmpty)
            {
                Log("Nothing is strung. Choose runes, or release a held form with F.");
                return;
            }

            Release(Composer.Snapshot(), Composer.Stance);
            Composer.Clear();
            CloseCharter();
        }

        public void StoreDraft()
        {
            if (Composer.IsEmpty)
            {
                Log("Nothing to hold. String at least one rune.");
                return;
            }

            var composition = Composer.Snapshot();
            var name = _resolver.PreviewName(composition);
            var overwritten = Held.Occupied;
            Held = new StoredSpell(composition, Composer.Stance, name);
            Composer.Clear();
            Log(overwritten
                ? $"The held form is rewritten. You now carry {name}."
                : $"{name} is held. One form only — Store again to replace it.");
        }

        public void CastHeld()
        {
            if (!Held.Occupied)
            {
                Log("No form is held. Space opens the Charter to compose one.");
                return;
            }

            var held = Held;
            Held = StoredSpell.Empty;
            Release(held.Composition, held.Stance);
        }

        public string DraftPreview()
        {
            return Composer.IsEmpty ? "empty string" : _resolver.PreviewName(Composer.Snapshot());
        }

        void RefreshVisibleRunes()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var origin = player != null ? player.transform.position : _safePoint;
            VisibleRunes = RuneField.Perceive(origin, Grid, _locks);
        }

        void Release(Composition composition, CastingStance stance)
        {
            var accepted = CurrentTarget != null && !CurrentTarget.Resolved
                ? CurrentTarget.AcceptedKeys
                : System.Array.Empty<SpellId>();

            var outcome = _resolver.Resolve(composition, stance, accepted, Grimoire);
            Taint = Mathf.Clamp01(Taint + outcome.TaintDelta);

            if (outcome.Resolved && CurrentTarget != null)
            {
                Grimoire.LearnInterpretation(CurrentTarget.FormulaId);
                var flavor = CurrentTarget.Resolve(outcome.Spell);
                OpenDoorFor(CurrentTarget);
                CurrentTarget = null;
                Log(string.IsNullOrEmpty(flavor) ? outcome.Log : flavor);
            }
            else
            {
                Log(outcome.Log);
            }

            CheckFinished();
        }

        void OpenDoorFor(ISpellLock resolved)
        {
            if (_rooms == null)
            {
                return;
            }

            foreach (var room in _rooms)
            {
                if (room.Lock == resolved && room.ExitDoors != null)
                {
                    foreach (var door in room.ExitDoors)
                    {
                        door?.OpenDoor();
                    }
                }
            }
        }

        ISpellLock FindNearestLock()
        {
            ISpellLock best = null;
            var bestDistance = 3.4f;
            if (_locks == null)
            {
                return null;
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return null;
            }

            foreach (var encounter in _locks)
            {
                if (encounter == null || encounter.Resolved)
                {
                    continue;
                }

                var distance = Vector2.Distance(player.transform.position, encounter.WorldPosition);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = encounter;
                }
            }

            return best;
        }

        public void FallInPit(Transform player)
        {
            var body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.position = _safePoint;
            }

            player.position = _safePoint;
            Log("The pit takes you. Earth is missing here — throw some across.");
        }

        void CheckFinished()
        {
            if (_finished || _locks == null)
            {
                return;
            }

            foreach (var encounter in _locks)
            {
                if (encounter != null && !encounter.Resolved)
                {
                    return;
                }
            }

            _finished = true;
            Log("The four rooms are read. Flesh and terrain use the same keys.");
        }

        public void Log(string message)
        {
            LastLog = message;
        }
    }
}
