using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public enum PlayMode
    {
        Exploring,
        Charter,
        Grimoire,
        Paused
    }

    public sealed class SanctumDirector : MonoBehaviour
    {
        public SpellComposer Composer { get; } = new();
        public Grimoire Grimoire { get; } = new();
        public ISpellLock CurrentTarget { get; private set; }
        public RoomInfo CurrentRoom { get; private set; }
        public WorldTile Underfoot { get; private set; }
        public string LastLog { get; private set; } = "WASD to walk. Space opens the Charter. Store a spell, then click a lock or press F.";
        public float Taint { get; private set; }
        public WorldGrid Grid { get; private set; }
        public PlayMode Mode { get; private set; } = PlayMode.Exploring;
        public StoredSpell Held { get; private set; } = StoredSpell.Empty;
        public IReadOnlyList<RuneId> VisibleRunes { get; private set; } = System.Array.Empty<RuneId>();
        public bool Busy { get; private set; }
        public bool CanMove => Mode == PlayMode.Exploring && !Busy;

        readonly CastResolver _resolver = new();
        ISpellLock[] _locks;
        RoomInfo[] _rooms;
        Vector3 _safePoint;
        bool _finished;
        PlayMode _modeBeforePause = PlayMode.Exploring;
        ISpellLock _focus;
        SpriteRenderer _targetRing;

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
            CurrentTarget = ResolveFocus();
            UpdateTargetRing();
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
                if (Mode == PlayMode.Grimoire)
                {
                    CloseGrimoire();
                }
                else if (Mode == PlayMode.Charter)
                {
                    CloseCharter();
                }
                else if (Mode == PlayMode.Paused)
                {
                    TogglePause();
                }
                else
                {
                    OpenGrimoire();
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.G) && Mode != PlayMode.Charter)
            {
                if (Mode == PlayMode.Grimoire)
                {
                    CloseGrimoire();
                }
                else if (Mode != PlayMode.Paused)
                {
                    OpenGrimoire();
                }

                return;
            }

            if (Mode == PlayMode.Paused || Mode == PlayMode.Grimoire || Busy)
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
                return;
            }

            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Return))
            {
                CastHeld();
            }

            if (Input.GetMouseButtonDown(0))
            {
                HandleWorldClick();
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
            Log("The field stands still. String runes, then Cast or Store.");
        }

        public void CloseCharter()
        {
            if (Mode != PlayMode.Charter)
            {
                return;
            }

            Mode = PlayMode.Exploring;
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
                return;
            }

            _modeBeforePause = Mode;
            Mode = PlayMode.Paused;
            Time.timeScale = 0f;
        }

        public void OpenGrimoire()
        {
            if (Mode == PlayMode.Paused || Busy)
            {
                return;
            }

            if (Mode == PlayMode.Charter)
            {
                Mode = PlayMode.Exploring;
            }

            Mode = PlayMode.Grimoire;
            Log("The Grimoire. Every written Charter recipe is listed here.");
        }

        public void CloseGrimoire()
        {
            if (Mode != PlayMode.Grimoire)
            {
                return;
            }

            Mode = PlayMode.Exploring;
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
            if (Busy)
            {
                return;
            }

            if (Composer.IsEmpty)
            {
                Log("Nothing is strung. Choose runes, or release a held form with F.");
                return;
            }

            var composition = Composer.Snapshot();
            var stance = Composer.Stance;
            Composer.Clear();
            CloseCharter();
            Release(composition, stance);
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
            if (Busy)
            {
                return;
            }

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

        void HandleWorldClick()
        {
            if (GameHud.PointerOverChrome(Mode))
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var world = camera.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            var clicked = FindLockNear(world, 0.95f);
            if (clicked == null)
            {
                return;
            }

            _focus = clicked;
            CurrentTarget = clicked;

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && Vector2.Distance(player.transform.position, clicked.WorldPosition) > 3.6f)
            {
                Log($"Walk closer to the {clicked.DisplayName} before you cast.");
                return;
            }

            if (Held.Occupied)
            {
                CastHeld();
                return;
            }

            OpenCharter();
            Log($"{clicked.DisplayName} is a lock. String a key, then Cast or Store.");
        }

        void Release(Composition composition, CastingStance stance)
        {
            if (Busy)
            {
                return;
            }

            StartCoroutine(ReleaseRoutine(composition, stance));
        }

        System.Collections.IEnumerator ReleaseRoutine(Composition composition, CastingStance stance)
        {
            Busy = true;
            var target = CurrentTarget != null && !CurrentTarget.Resolved ? CurrentTarget : null;
            var accepted = target != null ? target.AcceptedKeys : System.Array.Empty<SpellId>();
            var outcome = _resolver.Resolve(composition, stance, accepted, Grimoire);
            var aim = AimPoint(target);
            var origin = CasterPosition();
            var caption = outcome.Spell != SpellId.None
                ? _resolver.PreviewName(composition)
                : "unformed surge";

            composition.TryFoldMaterials(out var material, out _);
            var aspect = composition.Aspect;
            if (material == RuneId.None && outcome.Spell != SpellId.None &&
                SpellGrammar.TryGetBySpell(outcome.Spell, out var recipe))
            {
                material = recipe.Material;
                aspect = recipe.Aspect;
            }

            var finished = false;
            SpellFx.Play(origin, aim, material, aspect, caption, () => finished = true);
            while (!finished)
            {
                yield return null;
            }

            Taint = Mathf.Clamp01(Taint + outcome.TaintDelta);

            if (outcome.Resolved && target != null && !target.Resolved)
            {
                Grimoire.LearnInterpretation(target.FormulaId);
                var flavor = target.Resolve(outcome.Spell);
                OpenDoorFor(target);
                if (_focus == target)
                {
                    _focus = null;
                }

                CurrentTarget = null;
                Log(string.IsNullOrEmpty(flavor) ? outcome.Log : flavor);
            }
            else
            {
                Log(outcome.Log);
            }

            CheckFinished();
            Busy = false;
        }

        Vector3 CasterPosition()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            return player != null ? player.transform.position : _safePoint;
        }

        Vector3 AimPoint(ISpellLock target)
        {
            if (target != null)
            {
                return target.WorldPosition;
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            var facing = Vector2.right;
            if (player != null)
            {
                var motor = player.GetComponent<PlayerMotor2D>();
                if (motor != null)
                {
                    facing = motor.Facing;
                }

                return player.transform.position + (Vector3)(facing.normalized * 2.1f);
            }

            return _safePoint + Vector3.right * 2.1f;
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

        ISpellLock ResolveFocus()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (_focus != null)
            {
                if (_focus.Resolved || player == null ||
                    Vector2.Distance(player.transform.position, _focus.WorldPosition) > 6.5f)
                {
                    _focus = null;
                }
                else
                {
                    return _focus;
                }
            }

            return FindNearestLock();
        }

        ISpellLock FindNearestLock()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return null;
            }

            return FindLockNear(player.transform.position, 3.4f);
        }

        ISpellLock FindLockNear(Vector3 point, float radius)
        {
            ISpellLock best = null;
            var bestDistance = radius;
            if (_locks == null)
            {
                return null;
            }

            foreach (var encounter in _locks)
            {
                if (encounter == null || encounter.Resolved)
                {
                    continue;
                }

                var distance = Vector2.Distance(point, encounter.WorldPosition);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = encounter;
                }
            }

            return best;
        }

        void UpdateTargetRing()
        {
            if (_targetRing == null)
            {
                var ring = new GameObject("TargetRing");
                _targetRing = ring.AddComponent<SpriteRenderer>();
                _targetRing.sprite = SpriteFactory.TargetRing();
                _targetRing.sortingOrder = 16;
            }

            var show = CurrentTarget != null && !CurrentTarget.Resolved && Mode != PlayMode.Paused;
            _targetRing.gameObject.SetActive(show);
            if (!show)
            {
                return;
            }

            _targetRing.transform.position = CurrentTarget.WorldPosition + new Vector3(0f, -0.05f, 0f);
            var pulse = 0.95f + Mathf.Sin(Time.time * 6f) * 0.12f;
            _targetRing.transform.localScale = Vector3.one * pulse;
            _targetRing.color = new Color(1f, 0.86f, 0.35f, 0.95f);
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
