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
        public string LastLog { get; private set; } = "WASD to walk. Space opens the Charter. Charter Cast, Store, or Free Cast.";
        public float Taint { get; private set; }
        public WorldGrid Grid { get; private set; }
        public PlayMode Mode { get; private set; } = PlayMode.Exploring;
        public FreeAttunement Attunement { get; } = new();
        public StoredSpell Held { get; private set; } = StoredSpell.Empty;
        public RuneTapestry Tapestry { get; private set; }
        public string FieldReading { get; private set; } = string.Empty;
        public IReadOnlyList<RuneId> VisibleRunes { get; private set; } = System.Array.Empty<RuneId>();
        public IReadOnlyList<SpellShape> AvailableShapes { get; private set; } = System.Array.Empty<SpellShape>();
        public SpellShape ChosenShape { get; private set; }
        public string PendingPreview { get; private set; } = string.Empty;
        public string AimHint { get; private set; } = string.Empty;
        public CastingStance PendingStance { get; private set; }
        public SpellId PendingSpell { get; private set; }
        public bool HasSpanStart => _spanStart.HasValue;
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
        CodexEntry _pendingFree;
        bool _pendingFromHeld;
        Vector3? _spanStart;

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

            if (Underfoot != null && (Underfoot.Kind == TileKind.Floor || Underfoot.Kind == TileKind.Bridge))
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
                    CastCharter();
                }
                else if (Held.Occupied)
                {
                    CastHeld();
                }
                else
                {
                    Log("Nothing is strung. Choose runes, then Charter Cast, Store, or Free Cast.");
                }
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                CastFree();
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
            Log("The screen unrolls. Only runes in view can be drawn. Charter Cast, Store, or Free Cast.");
        }

        public void CloseCharter()
        {
            if (Mode != PlayMode.Charter)
            {
                return;
            }

            Mode = PlayMode.Exploring;
            if (string.IsNullOrEmpty(LastLog) ||
                LastLog.StartsWith("The field stands still") ||
                LastLog.StartsWith("The weave stills") ||
                LastLog.StartsWith("The room unrolls"))
            {
                Log(Held.Occupied
                    ? $"The wall folds. You still hold {Held.Name}."
                    : "The wall folds. The room's weave waits for the Charter.");
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

        public bool InVicinity(RuneId rune)
        {
            return Tapestry != null && Tapestry.InVicinity(rune);
        }

        public void AddRune(RuneId rune)
        {
            if (Mode != PlayMode.Charter)
            {
                return;
            }

            if (!InVicinity(rune))
            {
                Log($"{RuneCatalog.NameOf(rune)} is not on the screen. Walk until it is in view.");
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

            if (!InVicinity(rune))
            {
                Log($"{RuneCatalog.NameOf(rune)} is not on the screen. Walk until it is in view.");
                return;
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

        public void CastCharter()
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

            BeginAim(Composer.Snapshot(), CastingStance.Charter, fromHeld: false);
        }

        public void CastFree()
        {
            if (Busy)
            {
                return;
            }

            if (Composer.IsEmpty)
            {
                Log("Free has nothing to complete. String at least one rune.");
                return;
            }

            BeginAim(Composer.Snapshot(), CastingStance.Free, fromHeld: false);
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
            Held = new StoredSpell(composition, CastingStance.Charter, name);
            Composer.Clear();
            Log(overwritten
                ? $"The held Charter form is rewritten. You now carry {name}. Free cannot be stored."
                : $"{name} is held as Charter. Free is wild — it cannot be stored.");
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

        public void CancelAim()
        {
            if (Mode != PlayMode.Aiming)
            {
                return;
            }

            Mode = PlayMode.Exploring;
            ChosenShape = SpellShape.None;
            AvailableShapes = System.Array.Empty<SpellShape>();
            PendingSpell = SpellId.None;
            AimHint = string.Empty;
            _pendingFree = default;
            _spanStart = null;
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

            var shape = ChosenShape;
            if (shape == SpellShape.None && _pendingFree.Spell != SpellId.None)
            {
                shape = _pendingFree.Shape;
            }

            if (WorldWork.NeedsSpan(PendingSpell) && !_spanStart.HasValue)
            {
                var origin = CasterPosition();
                _spanStart = SpellFormations.ClampPoint(SpellShape.Remote, origin, worldPoint);
                AimHint = "Start is set. Click the far end. Across a pit it is a span; on the floor it is a barrier.";
                Log("The near end is marked. Click the far end, or Esc to withhold.");
                return;
            }

            var composition = _pendingComposition;
            var stance = _pendingStance;
            var spanFrom = _spanStart;
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
            var lockedFree = _pendingFree;
            var spell = PendingSpell;
            _pendingFree = default;
            PendingSpell = SpellId.None;
            AimHint = string.Empty;
            _spanStart = null;
            Release(composition, stance, shape, worldPoint, lockedFree, spanFrom, spell);
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

            _pendingComposition = composition;
            _pendingStance = stance;
            _pendingFromHeld = fromHeld;
            _pendingFree = default;
            _spanStart = null;
            PendingStance = stance;
            PendingSpell = SpellId.None;
            ChosenShape = SpellShape.None;
            AvailableShapes = System.Array.Empty<SpellShape>();
            AimHint = string.Empty;

            if (stance == CastingStance.Free)
            {
                if (!_resolver.TryChooseFree(composition, Attunement, out var pick))
                {
                    Log($"Free finds no spell that {CastResolver.FillWords(Attunement.FillBudget)} would complete.");
                    return;
                }

                LockAim(pick.Shape, $"{pick.Name} · {SpellFormations.NameOf(pick.Shape)}", pick.Spell, pick);
                var clash = ChainBook.CollectForFree(composition, SpellShape.None, Attunement.FillBudget).Count > 1
                    ? " Attunement chose this sentence, including how it lands."
                    : string.Empty;
                Log($"{pick.Name} is {SpellFormations.NameOf(pick.Shape)} — the chain writes the form.{clash} {AimHint} Esc cancels.");
                return;
            }

            if (ChainBook.TryMatch(composition, SpellShape.None, out var written))
            {
                LockAim(written.Shape, $"{written.Name} · {SpellFormations.NameOf(written.Shape)}", written.Spell);
                Log($"{written.Name} is {SpellFormations.NameOf(written.Shape)} — the chain writes the form. {AimHint} Esc cancels.");
                return;
            }

            PendingPreview = _resolver.PreviewName(composition);
            AimHint = "The chain did not write a form. Click the world to fizzle, or Esc to keep the string.";
            Mode = PlayMode.Aiming;
            Log("Those runes are not a written sentence. The form is in the chain, not a later choice. Click to fizzle, or Esc to keep the string.");
        }

        void LockAim(SpellShape shape, string preview, SpellId spell, CodexEntry freePick = default)
        {
            _pendingFree = freePick;
            ChosenShape = shape;
            PendingPreview = preview;
            PendingSpell = spell;
            AimHint = HintFor(spell, shape);
            AvailableShapes = shape == SpellShape.None
                ? System.Array.Empty<SpellShape>()
                : new[] { shape };
            Mode = PlayMode.Aiming;
        }

        static string HintFor(SpellId spell, SpellShape shape)
        {
            if (WorldWork.IsHop(spell))
            {
                return "Click where you want to land. Breath given a body carries you over a hollow.";
            }

            if (WorldWork.IsFlight(spell))
            {
                return "Click to keep the breath on you. Pits will not take you while it lasts.";
            }

            if (WorldWork.NeedsSpan(spell))
            {
                return "Click the near end, then the far end. Across a pit it is a span; on the floor it is a barrier.";
            }

            if (WorldWork.IsPillar(spell))
            {
                return "Click the ground. A column rises there. A hollow takes it and holds.";
            }

            return SpellFormations.Get(shape).Hint;
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
            Log($"Testing {entry.Name}: {entry.Recipe} · {entry.Form}.{gate} The form is in the sentence. Charter Cast or Free Cast to aim.");
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
            VisibleRunes = RuneCatalog.BasicRunes;
            if (Tapestry != null)
            {
                Tapestry.Resample();
            }
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
            Log($"{clicked.DisplayName} is a lock. String a key, then Charter Cast, Store, or Free Cast.");
        }

        void Release(
            Composition composition,
            CastingStance stance,
            SpellShape shape,
            Vector3 requested,
            CodexEntry lockedFree = default,
            Vector3? spanFrom = null,
            SpellId pendingSpell = SpellId.None)
        {
            if (Busy)
            {
                return;
            }

            StartCoroutine(ReleaseRoutine(composition, stance, shape, requested, lockedFree, spanFrom, pendingSpell));
        }

        System.Collections.IEnumerator ReleaseRoutine(
            Composition composition,
            CastingStance stance,
            SpellShape shape,
            Vector3 requested,
            CodexEntry lockedFree,
            Vector3? spanFrom,
            SpellId pendingSpell)
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
            var spell = pendingSpell;
            var aim = AimPoint(spell, shape, origin, requested);

            try
            {
                target = ResolveCastLock(shape, origin, requested);
                CurrentTarget = target;
                var accepted = target != null ? target.AcceptedKeys : System.Array.Empty<SpellId>();
                outcome = _resolver.Resolve(composition, stance, shape, accepted, Grimoire, Attunement, lockedFree);
                var potency = outcome.Potency;
                if (outcome.Shape != SpellShape.None)
                {
                    shape = outcome.Shape;
                }

                if (outcome.Spell != SpellId.None)
                {
                    spell = outcome.Spell;
                }

                aim = AimPoint(spell, shape, origin, requested, potency);
                target = ResolveCastLock(shape, origin, requested, potency) ?? target;
                if (WorldWork.IsHop(spell))
                {
                    target = FindLockNear(aim, 1.8f) ?? FindLockNear(origin, 2.4f) ?? target;
                }

                var caption = outcome.Spell != SpellId.None && SpellCodex.TryGet(outcome.Spell, out var named)
                    ? named.Name
                    : outcome.Spell != SpellId.None
                        ? _resolver.PreviewName(composition, shape)
                        : "unformed surge";

                var material = outcome.Material;
                if (material == RuneId.None)
                {
                    composition.TryFoldMaterials(out material, out _);
                }

                if (outcome.Fizzled || outcome.Spell == SpellId.None)
                {
                    SpellFx.PlayFizzle(shape == SpellShape.Spread || shape == SpellShape.Self ? origin : aim, () => finished = true);
                }
                else
                {
                    var fxFrom = spanFrom ?? origin;
                    SpellFx.Play(fxFrom, aim, material, shape, caption, () => finished = true, potency);
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

                Taint = Mathf.Clamp01(Taint + outcome.TaintDelta);

                var workNote = string.Empty;
                if (!outcome.Fizzled && outcome.Spell != SpellId.None)
                {
                    var workFrom = spanFrom ?? aim;
                    try
                    {
                        workNote = WorldWork.Apply(Grid, outcome.Spell, outcome.Material, origin, workFrom, aim);
                    }
                    catch (System.Exception exception)
                    {
                        Debug.LogWarning("Terrain work failed: " + exception.Message);
                    }

                    yield return CarryCaster(outcome.Spell, origin, requested);
                }

                try
                {
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
                        Log(FirstNote(flavor, workNote, outcome.Log));
                    }
                    else
                    {
                        Log(FirstNote(workNote, outcome.Log));
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

        Vector3 AimPoint(SpellId spell, SpellShape shape, Vector3 origin, Vector3 requested, float potency = 1f)
        {
            if (WorldWork.IsHop(spell))
            {
                var facing = Vector2.right;
                var motor = PlayerTransform() != null ? PlayerTransform().GetComponent<PlayerMotor2D>() : null;
                if (motor != null)
                {
                    facing = motor.Facing;
                }

                return WorldWork.HopLanding(Grid, origin, requested, facing);
            }

            if (WorldWork.NeedsSpan(spell))
            {
                return SpellFormations.ClampPoint(SpellShape.Remote, origin, requested, potency);
            }

            return SpellFormations.ClampPoint(shape, origin, requested, potency);
        }

        System.Collections.IEnumerator CarryCaster(SpellId spell, Vector3 origin, Vector3 requested)
        {
            var player = PlayerTransform();
            var adept = player != null ? player.GetComponent<AdeptAvatar>() : null;
            if (WorldWork.IsFlight(spell) && adept != null)
            {
                adept.KeepAirborne(WorldWork.FlightSeconds);
                yield break;
            }

            if (!WorldWork.IsHop(spell) || player == null)
            {
                yield break;
            }

            var land = AimPoint(spell, SpellShape.Self, origin, requested);
            if (adept != null)
            {
                adept.KeepAirborne(0.45f);
            }

            var body = player.GetComponent<Rigidbody2D>();
            var elapsed = 0f;
            const float duration = 0.28f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var next = Vector3.Lerp(origin, land, Mathf.Clamp01(elapsed / duration));
                if (body != null)
                {
                    body.position = next;
                }

                player.position = next;
                yield return null;
            }

            if (body != null)
            {
                body.position = land;
            }

            player.position = land;
        }

        static string FirstNote(params string[] notes)
        {
            if (notes == null)
            {
                return string.Empty;
            }

            for (var i = 0; i < notes.Length; i++)
            {
                if (!string.IsNullOrEmpty(notes[i]))
                {
                    return notes[i];
                }
            }

            return string.Empty;
        }

        ISpellLock ResolveAimFocus()
        {
            if (!TryMouseWorld(out var mouse))
            {
                return null;
            }

            var shape = ChosenShape == SpellShape.None ? SpellShape.Shot : ChosenShape;
            return ResolveCastLock(shape, CasterPosition(), mouse);
        }

        ISpellLock ResolveCastLock(SpellShape shape, Vector3 origin, Vector3 requested, float potency = 1f)
        {
            var clicked = FindLockNear(requested, 1.85f);
            if (clicked != null)
            {
                return clicked;
            }

            var aimed = LockAtAim(shape, origin, requested, potency);
            if (aimed != null)
            {
                return aimed;
            }

            return LockAlive(_focus) ? _focus : null;
        }

        ISpellLock LockAtAim(SpellShape shape, Vector3 origin, Vector3 requested, float potency = 1f)
        {
            var scale = potency <= 0f ? 1f : potency;
            var point = SpellFormations.ClampPoint(shape, origin, requested, scale);
            var radius = SpellFormations.Get(shape).LockRadius * scale;
            if (shape == SpellShape.Shot)
            {
                return FindLockAlong(origin, point, Mathf.Max(radius, 1.35f));
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
            var drawLine = show && (ChosenShape == SpellShape.Shot || WorldWork.NeedsSpan(PendingSpell) || WorldWork.IsHop(PendingSpell));
            _aimLine.enabled = drawLine;
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
            var point = AimPoint(PendingSpell, shape, origin, mouse);
            var mark = shape == SpellShape.Spread && !WorldWork.IsHop(PendingSpell)
                ? origin
                : point;
            _aimMark.transform.position = mark;
            var pulse = 0.9f + Mathf.Sin(Time.time * 8f) * 0.1f;
            _aimMark.transform.localScale = Vector3.one * pulse;
            _aimMark.color = ChosenShape == SpellShape.None
                ? new Color(0.7f, 0.72f, 0.8f, 0.55f)
                : new Color(0.55f, 0.92f, 0.95f, 0.9f);

            if (_aimLine.enabled)
            {
                var lineFrom = _spanStart ?? origin;
                _aimLine.SetPosition(0, lineFrom + new Vector3(0f, 0.1f, 0f));
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
            Log("The pit takes you. Raise a column, draw a wall across, or give breath a body and leap.");
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
