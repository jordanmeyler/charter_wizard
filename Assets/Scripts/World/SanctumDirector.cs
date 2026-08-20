using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    public enum PlayMode
    {
        Exploring,
        Charter,
        Aiming,
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
        public string LastLog { get; private set; } = "WASD to walk. Space opens the Charter. Cast chooses a form, then you aim.";
        public float Taint { get; private set; }
        public WorldGrid Grid { get; private set; }
        public PlayMode Mode { get; private set; } = PlayMode.Exploring;
        public StoredSpell Held { get; private set; } = StoredSpell.Empty;
        public RuneTapestry Tapestry { get; private set; }
        public string FieldReading { get; private set; } = string.Empty;
        public IReadOnlyList<RuneId> VisibleRunes { get; private set; } = System.Array.Empty<RuneId>();
        public IReadOnlyList<SpellShape> AvailableShapes { get; private set; } = System.Array.Empty<SpellShape>();
        public SpellShape ChosenShape { get; private set; }
        public string PendingPreview { get; private set; } = string.Empty;
        public CastingStance PendingStance { get; private set; }
        public bool Busy { get; private set; }
        public bool CanMove => (Mode == PlayMode.Exploring || Mode == PlayMode.Aiming) && !Busy;

        readonly CastResolver _resolver = new();
        ISpellLock[] _locks;
        RoomInfo[] _rooms;
        Vector3 _safePoint;
        bool _finished;
        PlayMode _modeBeforePause = PlayMode.Exploring;
        ISpellLock _focus;
        SpriteRenderer _targetRing;
        SpriteRenderer _aimMark;
        LineRenderer _aimLine;
        Transform _player;
        Composition _pendingComposition;
        CastingStance _pendingStance;
        bool _pendingFromHeld;

        public void Begin(SanctumBuild build)
        {
            Grid = build.Grid;
            _locks = build.Locks;
            _rooms = build.Rooms;
            _safePoint = build.Spawn;
            CurrentRoom = _rooms != null && _rooms.Length > 0 ? _rooms[0] : null;
            var broken = SpellCodex.Validate();
            if (!string.IsNullOrEmpty(broken))
            {
                Debug.LogWarning("Spell catalog failed to bind: " + broken);
            }
        }

        public void BindPlayer(GameObject player)
        {
            _player = player != null ? player.transform : null;
        }

        public void BindTapestry(RuneTapestry tapestry)
        {
            Tapestry = tapestry;
        }

        Transform PlayerTransform()
        {
            if (_player != null)
            {
                return _player;
            }

            var avatar = AdeptAvatar.Find();
            if (avatar != null)
            {
                _player = avatar.transform;
            }

            return _player;
        }

        static bool LockAlive(ISpellLock encounter)
        {
            return encounter is MonoBehaviour body && body != null && !encounter.Resolved;
        }

        void Update()
        {
            TrackPlayer();
            CurrentTarget = Mode == PlayMode.Aiming ? ResolveAimFocus() : ResolveFocus();
            UpdateTargetRing();
            UpdateAimGhost();
            HandleInput();
        }

        void OnDisable()
        {
            Time.timeScale = 1f;
        }

        void TrackPlayer()
        {
            var player = PlayerTransform();
            if (player == null || Grid == null)
            {
                return;
            }

            Underfoot = Grid.TileAtWorld(player.position);
            if (_rooms != null)
            {
                foreach (var room in _rooms)
                {
                    if (room.Contains(player.position))
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

            FieldReading = Tapestry != null ? Tapestry.Reading : string.Empty;
        }

        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (Mode == PlayMode.Aiming)
                {
                    CancelAim();
                }
                else if (Mode == PlayMode.Grimoire)
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

            if (Input.GetKeyDown(KeyCode.G) && Mode != PlayMode.Charter && Mode != PlayMode.Aiming)
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
                if (Mode == PlayMode.Aiming)
                {
                    CancelAim();
                    return;
                }

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

            if (Mode == PlayMode.Aiming)
            {
                HandleAimingInput();
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

            if (Input.GetMouseButtonDown(1) && Mode == PlayMode.Aiming)
            {
                CancelAim();
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

            if (Input.GetMouseButtonDown(0) && !GameHud.BlocksWorldPick(Mode))
            {
                TryWeaveFromPointer();
            }
        }

        public void OpenCharter()
        {
            Mode = PlayMode.Charter;
            RefreshVisibleRunes();
            Log("The weave stills. Draw from the wall, or click a glyph in the world.");
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
            Log("The Grimoire. All fifty written chains. Click a name to string it.");
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

        public void WeaveFromField(RuneId rune)
        {
            if (Busy || Mode == PlayMode.Aiming || Mode == PlayMode.Paused || Mode == PlayMode.Grimoire)
            {
                return;
            }

            if (rune == RuneId.None)
            {
                return;
            }

            if (Mode != PlayMode.Charter)
            {
                OpenCharter();
            }

            Composer.TryAdd(rune, out var note);
            Log($"You draw {RuneCatalog.NameOf(rune)} from the weave. {note}");
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

            BeginAim(Composer.Snapshot(), Composer.Stance, fromHeld: false);
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

            BeginAim(Held.Composition, Held.Stance, fromHeld: true);
        }

        public void ChooseShape(SpellShape shape)
        {
            if (Mode != PlayMode.Aiming)
            {
                return;
            }

            ChosenShape = shape;
            var def = SpellFormations.Get(shape);
            PendingPreview = _resolver.PreviewName(_pendingComposition, shape);
            Log($"{def.Name}. {def.Hint}");
        }

        public void CancelAim()
        {
            if (Mode != PlayMode.Aiming)
            {
                return;
            }

            Mode = PlayMode.Exploring;
            ChosenShape = SpellShape.None;
            AvailableShapes = System.Array.Empty<SpellShape>();
            Log(_pendingFromHeld
                ? $"The cast is withheld. You still hold {Held.Name}."
                : "The cast is withheld. The string is still on the Charter.");
        }

        public void ConfirmAimAt(Vector3 worldPoint)
        {
            if (Mode != PlayMode.Aiming || Busy)
            {
                return;
            }

            var shapes = AvailableShapes;
            if (shapes.Count > 0 && ChosenShape == SpellShape.None)
            {
                Log("Pick a formation first — Shot, Pillar, Spread, Remote, or Self.");
                return;
            }

            var shape = ChosenShape;
            if (shapes.Count == 0)
            {
                shape = SpellShape.Spread;
            }

            var composition = _pendingComposition;
            var stance = _pendingStance;
            if (_pendingFromHeld)
            {
                Held = StoredSpell.Empty;
            }
            else
            {
                Composer.Clear();
            }

            Mode = PlayMode.Exploring;
            ChosenShape = SpellShape.None;
            AvailableShapes = System.Array.Empty<SpellShape>();
            Release(composition, stance, shape, worldPoint);
        }

        void BeginAim(Composition composition, CastingStance stance, bool fromHeld)
        {
            if (Mode == PlayMode.Charter)
            {
                CloseCharter();
            }

            if (composition.Sequence == null || composition.Sequence.Length == 0)
            {
                Log("Nothing is strung. Choose runes, or release a held form with F.");
                return;
            }

            composition.TryFoldMaterials(out var material, out _);

            _pendingComposition = composition;
            _pendingStance = stance;
            _pendingFromHeld = fromHeld;
            PendingStance = stance;
            PendingPreview = _resolver.PreviewName(composition);
            ChosenShape = SpellShape.None;

            var shapes = ChainBook.ShapesFor(composition);
            if (shapes.Count == 0)
            {
                var aspect = composition.Aspect;
                if (material != RuneId.None && RuneCatalog.IsFormAspect(aspect))
                {
                    shapes = SpellFormations.Available(material, aspect);
                }
            }

            if (shapes.Count == 0 && stance == CastingStance.Free)
            {
                AvailableShapes = new[] { SpellShape.Shot, SpellShape.Pillar, SpellShape.Spread, SpellShape.Remote, SpellShape.Self };
                Mode = PlayMode.Aiming;
                Log("No natural form. Pick any formation — Free will borrow a written spell of that type. Esc cancels.");
                return;
            }

            AvailableShapes = shapes;
            Mode = PlayMode.Aiming;

            if (shapes.Count == 0)
            {
                Log("Those runes have no written form. Click the world to fizzle, or Esc to keep the string.");
                return;
            }

            if (shapes.Count == 1)
            {
                ChooseShape(shapes[0]);
                return;
            }

            Log($"{PendingPreview}. Choose how it aims, then click the world. Esc cancels.");
        }

        public void LoadCodex(int number)
        {
            if (!SpellCodex.TryGet(number, out var entry))
            {
                Log("That page is blank.");
                return;
            }

            Composer.Load(entry.RecipeRunes);
            if (Mode == PlayMode.Grimoire)
            {
                CloseGrimoire();
            }

            if (Mode != PlayMode.Charter)
            {
                OpenCharter();
            }

            var gate = entry.FreeOnly ? " Free only — Charter will fizzle." : string.Empty;
            Log($"Testing {entry.Name}: {entry.Recipe}.{gate} Cast to aim.");
        }

        void HandleAimingInput()
        {
            if (Input.GetMouseButtonDown(1))
            {
                CancelAim();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                if (GameHud.PointerOverChrome(Mode))
                {
                    return;
                }

                if (!TryMouseWorld(out var world))
                {
                    return;
                }

                ConfirmAimAt(world);
            }
        }

        public string DraftPreview()
        {
            return Composer.IsEmpty ? "empty string" : _resolver.PreviewName(Composer.Snapshot());
        }

        void RefreshVisibleRunes()
        {
            var player = PlayerTransform();
            var origin = player != null ? player.position : _safePoint;
            VisibleRunes = RuneField.Perceive(origin, Grid, _locks);
        }

        void HandleWorldClick()
        {
            if (GameHud.BlocksWorldPick(Mode))
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
            var clicked = FindLockNear(world, 1.2f);
            var lockDistance = clicked != null
                ? Vector2.Distance(world, clicked.WorldPosition)
                : float.MaxValue;
            if (lockDistance > 0.7f && TryWeaveAt(world))
            {
                return;
            }

            if (clicked == null)
            {
                return;
            }

            _focus = clicked;
            CurrentTarget = clicked;

            if (Held.Occupied)
            {
                CastHeld();
                return;
            }

            var player = PlayerTransform();
            if (player != null && Vector2.Distance(player.position, clicked.WorldPosition) > 3.6f)
            {
                Log($"Walk closer to the {clicked.DisplayName} to read it, or Store a form and aim from here.");
                return;
            }

            OpenCharter();
            Log($"{clicked.DisplayName} is a lock. String a key, then Cast or Store.");
        }

        void Release(Composition composition, CastingStance stance, SpellShape shape, Vector3 requested)
        {
            if (Busy)
            {
                return;
            }

            StartCoroutine(ReleaseRoutine(composition, stance, shape, requested));
        }

        System.Collections.IEnumerator ReleaseRoutine(
            Composition composition,
            CastingStance stance,
            SpellShape shape,
            Vector3 requested)
        {
            if (Mode == PlayMode.Charter)
            {
                CloseCharter();
            }

            Busy = true;

            ISpellLock target = null;
            CastOutcome outcome = default;
            var finished = false;
            var setupFailed = false;
            var origin = CasterPosition();
            var aim = SpellFormations.ClampPoint(shape, origin, requested);

            try
            {
                target = LockAtAim(shape, origin, requested);
                CurrentTarget = target;
                var accepted = target != null ? target.AcceptedKeys : System.Array.Empty<SpellId>();
                outcome = _resolver.Resolve(composition, stance, shape, accepted, Grimoire);
                if (outcome.Shape != SpellShape.None)
                {
                    shape = outcome.Shape;
                    aim = SpellFormations.ClampPoint(shape, origin, requested);
                    target = LockAtAim(shape, origin, requested);
                }

                var caption = outcome.Spell != SpellId.None
                    ? _resolver.PreviewName(composition, shape)
                    : "unformed surge";

                var material = outcome.Material;
                if (material == RuneId.None)
                {
                    composition.TryFoldMaterials(out material, out _);
                }

                if (outcome.Fizzled || outcome.Spell == SpellId.None)
                {
                    SpellFx.PlayFizzle(shape == SpellShape.Spread ? origin : aim, () => finished = true);
                }
                else
                {
                    SpellFx.Play(origin, aim, material, shape, caption, () => finished = true);
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Cast failed: " + exception.Message);
                Log("The spell fizzled. Try again — the lock still holds.");
                setupFailed = true;
            }

            if (!setupFailed)
            {
                var timeout = 1.25f;
                while (!finished && timeout > 0f)
                {
                    timeout -= Time.unscaledDeltaTime;
                    yield return null;
                }

                try
                {
                    Taint = Mathf.Clamp01(Taint + outcome.TaintDelta);

                    if (outcome.Resolved && LockAlive(target))
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
                }
                catch (System.Exception exception)
                {
                    Debug.LogWarning("Cast failed: " + exception.Message);
                    Log("The spell fizzled. Try again — the lock still holds.");
                }
            }

            Busy = false;
        }

        Vector3 CasterPosition()
        {
            var player = PlayerTransform();
            return player != null ? player.position : _safePoint;
        }

        ISpellLock ResolveAimFocus()
        {
            if (!TryMouseWorld(out var mouse))
            {
                return null;
            }

            var shape = ChosenShape == SpellShape.None ? SpellShape.Shot : ChosenShape;
            return LockAtAim(shape, CasterPosition(), mouse);
        }

        ISpellLock LockAtAim(SpellShape shape, Vector3 origin, Vector3 requested)
        {
            var point = SpellFormations.ClampPoint(shape, origin, requested);
            var radius = SpellFormations.Get(shape).LockRadius;
            if (shape == SpellShape.Shot)
            {
                return FindLockAlong(origin, point, radius);
            }

            if (shape == SpellShape.Spread || shape == SpellShape.Self)
            {
                return FindLockNear(origin, radius);
            }

            return FindLockNear(point, radius);
        }

        ISpellLock FindLockAlong(Vector3 from, Vector3 to, float radius)
        {
            ISpellLock best = null;
            var bestAlong = float.MaxValue;
            if (_locks == null)
            {
                return null;
            }

            var a = (Vector2)from;
            var b = (Vector2)to;
            var span = b - a;
            var lengthSq = span.sqrMagnitude;
            foreach (var encounter in _locks)
            {
                if (!LockAlive(encounter))
                {
                    continue;
                }

                var point = (Vector2)encounter.WorldPosition;
                var t = lengthSq < 0.0001f ? 0f : Mathf.Clamp01(Vector2.Dot(point - a, span) / lengthSq);
                var closest = a + span * t;
                if (Vector2.Distance(point, closest) > radius)
                {
                    continue;
                }

                if (t < bestAlong)
                {
                    bestAlong = t;
                    best = encounter;
                }
            }

            return best;
        }

        bool TryMouseWorld(out Vector3 world)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                world = default;
                return false;
            }

            world = camera.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            return true;
        }

        void UpdateAimGhost()
        {
            var show = Mode == PlayMode.Aiming && !Busy;
            EnsureAimGhost();
            _aimMark.gameObject.SetActive(show);
            _aimLine.enabled = show && ChosenShape == SpellShape.Shot;
            if (!show)
            {
                return;
            }

            if (!TryMouseWorld(out var mouse))
            {
                return;
            }

            var origin = CasterPosition();
            var shape = ChosenShape == SpellShape.None ? SpellShape.Shot : ChosenShape;
            var point = SpellFormations.ClampPoint(shape, origin, mouse);
            _aimMark.transform.position = shape == SpellShape.Spread ? origin : point;
            var pulse = 0.9f + Mathf.Sin(Time.time * 8f) * 0.1f;
            _aimMark.transform.localScale = Vector3.one * pulse;
            _aimMark.color = ChosenShape == SpellShape.None
                ? new Color(0.7f, 0.72f, 0.8f, 0.55f)
                : new Color(0.55f, 0.92f, 0.95f, 0.9f);

            if (_aimLine.enabled)
            {
                _aimLine.SetPosition(0, origin + new Vector3(0f, 0.1f, 0f));
                _aimLine.SetPosition(1, point + new Vector3(0f, 0.1f, 0f));
            }
        }

        void EnsureAimGhost()
        {
            if (_aimMark == null)
            {
                var mark = new GameObject("AimMark");
                _aimMark = mark.AddComponent<SpriteRenderer>();
                _aimMark.sprite = SpriteFactory.TargetRing();
                _aimMark.sortingOrder = 17;
            }

            if (_aimLine != null)
            {
                return;
            }

            var lineObject = new GameObject("AimLine");
            _aimLine = lineObject.AddComponent<LineRenderer>();
            _aimLine.positionCount = 2;
            _aimLine.widthMultiplier = 0.06f;
            _aimLine.numCapVertices = 4;
            _aimLine.useWorldSpace = true;
            _aimLine.sortingOrder = 16;
            var shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                _aimLine.material = new Material(shader);
            }

            _aimLine.startColor = new Color(0.75f, 0.95f, 1f, 0.85f);
            _aimLine.endColor = new Color(0.75f, 0.95f, 1f, 0.2f);
            _aimLine.enabled = false;
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
            var player = PlayerTransform();
            if (_focus != null)
            {
                if (!LockAlive(_focus) || player == null ||
                    Vector2.Distance(player.position, _focus.WorldPosition) > 6.5f)
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
            var player = PlayerTransform();
            if (player == null)
            {
                return null;
            }

            return FindLockNear(player.position, 3.4f);
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
                if (!LockAlive(encounter))
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

            var show = LockAlive(CurrentTarget) && Mode != PlayMode.Paused;
            _targetRing.gameObject.SetActive(show);
            if (!show)
            {
                return;
            }

            _targetRing.transform.position = CurrentTarget.WorldPosition + new Vector3(0f, -0.05f, 0f);
            var pulse = 0.95f + Mathf.Sin(Time.time * 6f) * 0.12f;
            _targetRing.transform.localScale = Vector3.one * pulse;
            _targetRing.color = Mode == PlayMode.Aiming
                ? new Color(0.45f, 0.92f, 0.95f, 0.95f)
                : new Color(1f, 0.86f, 0.35f, 0.95f);
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
                if (LockAlive(encounter))
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

        void TryWeaveFromPointer()
        {
            if (!TryMouseWorld(out var world))
            {
                return;
            }

            TryWeaveAt(world);
        }

        bool TryWeaveAt(Vector3 world)
        {
            if (Tapestry == null || !Tapestry.TryPick(world, out var rune))
            {
                return false;
            }

            WeaveFromField(rune);
            return true;
        }
    }
}
