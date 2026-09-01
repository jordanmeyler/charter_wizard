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
        Inventory,
        Paused
    }

    public sealed class SanctumDirector : MonoBehaviour
    {
        public SpellComposer Composer { get; } = new();
        public Grimoire Grimoire { get; } = new();
        public RuneMemory Memory { get; } = new();
        public CastLedger Ledger { get; } = new();
        public ISpellLock CurrentTarget { get; private set; }
        public RoomInfo CurrentRoom { get; private set; }

        public RoomInfo RoomAt(Vector3 world)
        {
            if (_rooms != null)
            {
                for (var i = 0; i < _rooms.Length; i++)
                {
                    if (_rooms[i] != null && _rooms[i].Contains(world))
                    {
                        return _rooms[i];
                    }
                }
            }

            return CurrentRoom;
        }

        public WorldTile Underfoot { get; private set; }
        public string LastLog { get; private set; } = "WASD to walk. Space opens the Charter. Charter Cast, Store, or Free Cast.";
        public DeathCause LastDeath { get; private set; }
        public float LastDeathAt { get; private set; }
        public bool DeathNoticeUp =>
            LastDeath.Exists && Time.unscaledTime - LastDeathAt < 7.5f;
        public string SightLine { get; private set; } = "You see the room. Space opens the Charter.";
        public float Taint { get; private set; }
        public WorldGrid Grid { get; private set; }
        public AdeptPack Pack { get; } = new();
        public PlayMode Mode { get; private set; } = PlayMode.Exploring;
        public FreeAttunement Attunement { get; } = new();
        public StoredSpell Held { get; private set; } = StoredSpell.Empty;
        public RuneTapestry Tapestry { get; private set; }
        public string FieldReading { get; private set; } = string.Empty;
        public IReadOnlyList<RuneId> VisibleRunes => BuildWallRunes();
        public IReadOnlyList<SpellShape> AvailableShapes { get; private set; } = System.Array.Empty<SpellShape>();
        public SpellShape ChosenShape { get; private set; }
        public string PendingPreview { get; private set; } = string.Empty;
        public string AimHint { get; private set; } = string.Empty;
        public CastingStance PendingStance { get; private set; }
        public SpellId PendingSpell { get; private set; }
        public bool HasSpanStart => _spanStart.HasValue;
        public bool Busy { get; private set; }
        public bool Started { get; private set; }
        public bool CanMove =>
            (Mode == PlayMode.Exploring || Mode == PlayMode.Aiming || Mode == PlayMode.Charter)
            && !Busy && !GameHud.EditingName;

        readonly CastResolver _resolver = new();
        ISpellLock[] _locks;
        RoomInfo[] _rooms;
        Vector3 _safePoint;
        Vector3 _spawnPoint;
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
        readonly HashSet<int> _hunted = new();

        public void Begin(SanctumBuild build)
        {
            Grid = build.Grid;
            _locks = build.Locks;
            _rooms = build.Rooms;
            _safePoint = build.Spawn;
            _spawnPoint = build.Spawn;
            var crystal = FindFirstObjectByType<SpawnCrystal>();
            if (crystal == null)
            {
                SpawnCrystal.Spawn(build.Spawn);
            }
            else
            {
                crystal.EnsureBound();
            }

            CurrentRoom = _rooms != null && _rooms.Length > 0 ? _rooms[0] : null;
            Started = true;
            var broken = SpellCodex.Validate();
            if (!string.IsNullOrEmpty(broken))
            {
                Debug.LogWarning("Spell catalog failed to bind: " + broken);
            }
        }

        public void BindPlayer(GameObject player)
        {
            _player = player != null ? player.transform : null;
            var host = StatusHost.On(player != null ? player.transform : null);
            if (host != null)
            {
                host.OnFatal = id =>
                {
                    if (!VitalLaw.IsMeter(id))
                    {
                        return;
                    }

                    KillPlayer(DeathCause.Plain(VitalLaw.FatalNote(id, string.Empty, true)));
                };
            }
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
            UpdateSight();
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

            if (Underfoot != null && Underfoot.IsSafeStand)
            {
                _safePoint = WorldGrid.Center(Underfoot.Coord.x, Underfoot.Coord.y);
            }

            if (Mode == PlayMode.Paused || GameHud.EditingName)
            {
                FieldReading = Tapestry != null ? Tapestry.Reading : string.Empty;
                return;
            }

            var adept = player.GetComponent<AdeptAvatar>();
            if ((adept == null || !adept.IsAirborne) && Underfoot == null)
            {
                FallInPit(player);
            }

            if (Underfoot != null && Underfoot.IsDeepWater && (adept == null || !adept.IsAirborne))
            {
                FallInPit(player);
            }

            if (Underfoot != null && Underfoot.HasMiasma && (adept == null || !adept.IsAirborne))
            {
                var host = StatusHost.On(player);
                if (host == null || !host.Fends(Essence.Poison))
                {
                    PlacePlayer(player, _safePoint, "The breath is foul. Send air through it.");
                    FieldReading = Tapestry != null ? Tapestry.Reading : string.Empty;
                    return;
                }
            }
            else if (Underfoot != null && Underfoot.IsPoisonWater && (adept == null || !adept.IsAirborne))
            {
                var host = StatusHost.On(player);
                if (host == null || !host.Fends(Essence.Poison))
                {
                    host?.Apply(StatusId.Poisoned, VitalLaw.AdeptPoisonSeconds);
                }
            }

            if (Underfoot != null && Underfoot.IsSafeStand)
            {
                var host = StatusHost.On(player);
                var inFire = Underfoot.IsBurning;
                if (inFire)
                {
                    host?.Apply(StatusId.Burning, VitalLaw.AdeptBurnSeconds);
                    var warded = host != null && host.Fends(Essence.Fire);
                    if (Underfoot.Kindled && !warded && (adept == null || !adept.IsAirborne))
                    {
                        KillPlayer(DeathCause.Plain(
                            "The flaming hall finds you. Wear a water ward, or throw yield first."));
                    }
                }

                if (Underfoot.Material == MaterialId.Lava && (host == null || !host.Fends(Essence.Fire)))
                {
                    KillPlayer(DeathCause.Plain("Hungry earth finds you."));
                }
            }
            if (Underfoot != null && WorldWork.BurnsOccupants(Underfoot) && (adept == null || !adept.IsAirborne))
            {
                var host = StatusHost.On(player);
                if (host == null || !host.Fends(Essence.Fire))
                {
                    KillPlayer(DeathCause.OfSpell(SpellId.FlamePillar, "A standing flame finds you."));
                }
            }

            BurnLocksOnPillars();

            FieldReading = Tapestry != null ? Tapestry.Reading : string.Empty;
        }

        void HandleInput()
        {
            if (GameHud.EditingName)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    GameHud.CancelNaming();
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                ToggleSight();
                return;
            }

            if (Input.GetKeyDown(KeyCode.K)
                && Mode != PlayMode.Paused && Mode != PlayMode.Inventory)
            {
                YieldSelf();
                return;
            }

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
                else if (Mode == PlayMode.Inventory)
                {
                    CloseInventory();
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

            if (Input.GetKeyDown(KeyCode.I) && Mode != PlayMode.Charter && Mode != PlayMode.Aiming)
            {
                if (Mode == PlayMode.Inventory)
                {
                    CloseInventory();
                }
                else if (Mode != PlayMode.Paused)
                {
                    OpenInventory();
                }

                return;
            }

            if (Mode == PlayMode.Inventory)
            {
                HandleInventoryInput();
                return;
            }

            if (Mode == PlayMode.Paused || Mode == PlayMode.Grimoire || Busy)
            {
                return;
            }

            if (PlayerBlocksAction())
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Return)
                    || Input.GetMouseButtonDown(0))
                {
                    if (Mode == PlayMode.Aiming)
                    {
                        CancelAim();
                    }
                    else if (Mode == PlayMode.Charter)
                    {
                        CloseCharter();
                    }
                    else
                    {
                        Log("A held mind will not write.");
                    }
                }

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
            Log(GlyphView.Speak(
                "The screen unrolls. You can walk. Draw only what is in view; what you have already strung stays until you cast or close.",
                "The screen unrolls. You can walk. Draw marks from the weave. What you have already strung stays until you cast or close."));
        }

        public void CloseCharter(bool releaseString = true)
        {
            if (Mode != PlayMode.Charter)
            {
                return;
            }

            var released = false;
            if (releaseString && !Composer.IsEmpty)
            {
                Composer.Clear();
                released = true;
            }

            Mode = PlayMode.Exploring;
            if (released)
            {
                Log(Held.Occupied
                    ? GlyphView.Speak(
                        $"The wall folds. The string is released. You still hold {Held.Name}.",
                        "The wall folds. The string is released. You still hold a working.")
                    : "The wall folds. The string is released.");
            }
            else if (string.IsNullOrEmpty(LastLog) ||
                LastLog.StartsWith("The field stands still") ||
                LastLog.StartsWith("The weave stills") ||
                LastLog.StartsWith("The room unrolls") ||
                LastLog.StartsWith("The screen unrolls"))
            {
                Log(Held.Occupied
                    ? GlyphView.Speak(
                        $"The wall folds. You still hold {Held.Name}.",
                        "The wall folds. You still hold a working.")
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

        public void PauseForNaming()
        {
            Time.timeScale = 0f;
        }

        public void ResumeFromNaming()
        {
            if (Mode != PlayMode.Paused)
            {
                Time.timeScale = 1f;
            }
        }

        public void OpenGrimoire()
        {
            if (Mode == PlayMode.Paused || Busy)
            {
                return;
            }

            if (Mode == PlayMode.Charter)
            {
                CloseCharter();
            }
            else if (Mode == PlayMode.Inventory)
            {
                Mode = PlayMode.Exploring;
            }

            Mode = PlayMode.Grimoire;
            Log(GlyphView.Speak(
                "The Grimoire. Every written chain and join. Click a name to string it if those runes are in view. Kept workings are marked.",
                "Your book. Workings you have kept. Click a page to send it if those marks are in view."));
        }

        public void CloseGrimoire()
        {
            if (Mode != PlayMode.Grimoire)
            {
                return;
            }

            Mode = PlayMode.Exploring;
        }

        public void OpenInventory()
        {
            if (Mode == PlayMode.Paused || Busy)
            {
                return;
            }

            if (Mode == PlayMode.Aiming)
            {
                CancelAim();
            }

            if (Mode == PlayMode.Charter)
            {
                CloseCharter();
            }
            else if (Mode == PlayMode.Grimoire)
            {
                Mode = PlayMode.Exploring;
            }

            Mode = PlayMode.Inventory;
            if (Pack.Empty)
            {
                Log("The pack is empty. Stones and other keys will sit here. Esc or I closes.");
                return;
            }

            if (Pack.Selected == null)
            {
                Pack.Select(0);
            }

            Log(AdeptPack.LookText(Pack.Selected));
        }

        public void CloseInventory()
        {
            if (Mode != PlayMode.Inventory)
            {
                return;
            }

            Mode = PlayMode.Exploring;
        }

        public void SelectPack(int index)
        {
            if (!Pack.Select(index))
            {
                return;
            }

            Log(AdeptPack.LookText(Pack.Selected));
        }

        void HandleInventoryInput()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                if (Pack.Nudge(-1))
                {
                    Log(AdeptPack.LookText(Pack.Selected));
                }

                return;
            }

            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                if (Pack.Nudge(1))
                {
                    Log(AdeptPack.LookText(Pack.Selected));
                }
            }
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
                Log(OffScreenNote(rune));
                return;
            }

            Composer.TryAdd(rune, out var note);
            Log(note);
        }

        public void WeaveFromField(RuneId rune)
        {
            if (Busy || Mode == PlayMode.Aiming || Mode == PlayMode.Paused || Mode == PlayMode.Grimoire ||
                Mode == PlayMode.Inventory)
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
                Log(OffScreenNote(rune));
                return;
            }

            Composer.TryAdd(rune, out var note);
            Log(GlyphView.Speak(
                $"You draw {RuneCatalog.NameOf(rune)} from the weave. {note}",
                $"You draw a mark from the weave. {note}"));
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
            var name = CallWorking(composition);
            var overwritten = Held.Occupied;
            Held = new StoredSpell(composition, CastingStance.Charter, name);
            Composer.Clear();
            Log(overwritten
                ? GlyphView.Speak(
                    $"The held Charter form is rewritten. You now carry {name}. Free cannot be stored.",
                    "The held working is rewritten. Free cannot be stored.")
                : GlyphView.Speak(
                    $"{name} is held as Charter. Free is wild — it cannot be stored.",
                    "The working is held as Charter. Free cannot be stored."));
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
                ? GlyphView.Speak(
                    $"The cast is withheld. You still hold {Held.Name}.",
                    "The cast is withheld. You still hold a working.")
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

            if (WorldWork.NeedsSpan(SpellCodex.WorkOf(PendingSpell)) && !_spanStart.HasValue)
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
                CloseCharter(releaseString: false);
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
                    RecordWorking(composition, CastingStance.Free, false, SpellId.None);
                    Log($"Free finds no spell that {CastResolver.FillWords(Attunement.FillBudget)} would complete.");
                    return;
                }

                var call = CallWorking(composition);
                LockAim(pick.Shape, AimPreview(call, pick.Shape), pick.Spell, pick);
                var clash = ChainBook.CollectForFree(composition, SpellShape.None, Attunement.FillBudget).Count > 1
                    ? " Attunement chose this sentence, including how it lands."
                    : string.Empty;
                Log(GlyphView.Speak(
                    $"{call} is {SpellFormations.NameOf(pick.Shape)} — the chain writes the form.{clash} {AimHint} Esc cancels.",
                    $"The chain writes how it lands.{clash} {AimHint} Esc cancels."));
                return;
            }

            if (ChainBook.TryMatch(composition, SpellShape.None, out var written))
            {
                var call = CallWorking(composition);
                LockAim(written.Shape, AimPreview(call, written.Shape), written.Spell);
                Log(GlyphView.Speak(
                    $"{call} is {SpellFormations.NameOf(written.Shape)} — the chain writes the form. {AimHint} Esc cancels.",
                    $"The chain writes how it lands. {AimHint} Esc cancels."));
                return;
            }

            PendingPreview = CallWorking(composition);
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
            spell = SpellCodex.WorkOf(spell);
            if (WorldWork.IsHop(spell))
            {
                return "Click where you want to land. Breath given a body carries you over a hollow.";
            }

            if (WorldWork.IsFlight(spell))
            {
                return "Click to keep the breath on you. Pits will not take you while it lasts.";
            }

            if (WorldWork.IsTimeStop(spell))
            {
                return "Click to confirm. The instant stands around you. Motion leaves the living.";
            }

            if (WorldWork.NeedsSpan(spell))
            {
                return spell == SpellId.IceWall
                    ? "Click the near end, then the far end. Across a pit the span is two tiles wide and must find floor or wall at each end, or it falls. Ice freezes water without banks. Fire thaws ice."
                    : spell == SpellId.MetalWall
                    ? "Click the near end, then the far end. Across a pit the span is two tiles wide. Iron needs no far rest, and it will stand on water."
                    : spell == SpellId.WoodWall
                    ? "Click the near end, then the far end. A line of trees. Across a pit the span is two tiles wide and must find floor or wall at each end, or it falls. On water it grows a walkable cover without banks. Hunger eats the wood."
                    : spell == SpellId.ObsidianWall
                    ? "Click the near end, then the far end. Across a pit the span is two tiles wide. Black glass needs no far rest. Melt, Shatter, and hunger's thaw will not take it."
                    : "Click the near end, then the far end. Across a pit the span is two tiles wide and must find floor or wall at each end, or it falls. Water takes mud, not a bridge.";
            }

            if (WorldWork.IsSkyStrike(spell))
            {
                return "Click where it should fall. A spark given form comes from the sky. Walls will not hide them.";
            }

            if (WorldWork.IsPush(spell) && spell == SpellId.Push)
            {
                return "Click through a person. Breath given a body finds them, and they go.";
            }

            if (WorldWork.IsPillar(spell))
            {
                return spell == SpellId.Tree
                    ? "Click the ground. A tree stands there until hunger eats it. Over a pit it must join two floors, or it falls. On water it grows a walkable cover without banks."
                    : "Click the ground. A column stands there until another element unmakes it. Over a pit it follows the same law as a wall: two tiles wide, and basic earth or ice must join two floors. Ice freezes water without banks. Standing fire unmakes flesh.";
            }

            if (WorldWork.LaysVeil(spell))
            {
                return WorldWork.IsPoisonVeil(spell)
                    ? "Click to confirm. A sick mist hangs until fire or breath tears it."
                    : "Click to confirm. Fog hangs until breath, fire, or light tears it.";
            }

            return SpellFormations.Get(shape).Hint;
        }

        public void LoadBirth(RuneId rune)
        {
            if (!ChainBook.TryBirth(rune, out var sources) || sources.Count == 0)
            {
                Log("That join is not written.");
                return;
            }

            if (Mode == PlayMode.Grimoire)
            {
                CloseGrimoire();
            }

            if (Mode != PlayMode.Charter)
            {
                OpenCharter();
            }

            Composer.Load(sources);
            var name = RuneCatalog.NameOf(rune);
            var recipe = ChainBook.BirthNameText(rune);
            var extras = ChainBook.ExtraRoles(sources);
            var extra = extras.Length == 0
                ? string.Empty
                : $"  ({extras})";
            if (FieldOffers(sources))
            {
                Log(GlyphView.Speak(
                    $"{name} is {recipe}{extra}. The sentence is strung.",
                    "The join is strung."));
                return;
            }

            Log(GlyphView.Speak(
                $"{name} is {recipe}{extra}, but those runes are not all in this view. Walk until they speak.",
                "The join is strung, but those marks are not all in this view."));
        }

        public void LoadCodex(int number)
        {
            if (!SpellCodex.TryGet(number, out var entry))
            {
                Log("That page is blank.");
                return;
            }

            var stance = entry.FreeOnly ? CastingStance.Free : CastingStance.Charter;
            if (TryCastPrepared(entry.RecipeRunes, entry.ViaRunes, stance))
            {
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
            var call = CallWorking(entry.RecipeRunes);
            Log(GlyphView.Speak(
                $"{call} is strung, but those runes are not all in this view. Walk until they speak, then Charter Cast.{gate}",
                "A sentence is strung, but those marks are not all in this view."));
        }

        public void CastRecent(int index)
        {
            if (Busy || index < 0 || index >= Ledger.Recent.Count)
            {
                return;
            }

            SendWorking(Ledger.Recent[index].Runes, Ledger.Recent[index].Stance, Ledger.Recent[index].Worked);
        }

        public void CastKept(int index)
        {
            if (Busy || !Grimoire.TryGetKept(index, out var kept))
            {
                return;
            }

            SendWorking(kept.Runes, kept.Stance, worked: true);
        }

        public void KeepRecent(int index, string givenName)
        {
            if (!Ledger.TryKeep(index, givenName))
            {
                Log("Only a working that held can be kept.");
                return;
            }

            var attempt = Ledger.Recent[index];
            Grimoire.KeepWorking(attempt.Stance, attempt.Runes, attempt.Spell, attempt.GivenName);
            Grimoire.Names.Remember(attempt.Runes, attempt.GivenName);
            var label = CallWorking(attempt.Runes);
            if (Held.Occupied && WorkingNames.SameComposition(Held.Composition.Sequence, attempt.Runes))
            {
                Held = new StoredSpell(Held.Composition, Held.Stance, label);
            }

            Log(GlyphView.Speak(
                $"{label} is kept in the Grimoire for that same writing. Spark is not Fire · Air — only this composition carries the name.",
                "The working is kept in your book. The name holds for that same writing."));
        }

        void SendWorking(IReadOnlyList<RuneId> runes, CastingStance stance, bool worked)
        {
            if (!worked || runes == null || runes.Count == 0)
            {
                Log("That working did not hold. There is nothing to send again.");
                return;
            }

            if (TryCastPrepared(runes, null, stance))
            {
                return;
            }

            Log(GlyphView.Speak(
                "Those runes are not in this view. Walk until they speak, then send it again.",
                "Those marks are not in this view."));
        }

        bool TryCastPrepared(IReadOnlyList<RuneId> runes, IReadOnlyList<RuneId> via, CastingStance stance)
        {
            if (Busy)
            {
                return false;
            }

            IReadOnlyList<RuneId> chosen = null;
            if (FieldOffers(runes))
            {
                chosen = runes;
            }
            else if (via != null && via.Count > 0 && FieldOffers(via))
            {
                chosen = via;
            }

            if (chosen == null)
            {
                return false;
            }

            if (Mode == PlayMode.Grimoire)
            {
                CloseGrimoire();
            }

            BeginAim(Composition.FromSequence(chosen), stance, fromHeld: false);
            return true;
        }

        public bool FieldOffers(IReadOnlyList<RuneId> runes)
        {
            if (runes == null || runes.Count == 0)
            {
                return false;
            }

            var seen = new HashSet<RuneId>();
            var perceived = RuneTapestry.Perceive(CasterPosition(), Grid, _locks);
            for (var i = 0; i < perceived.Count; i++)
            {
                RememberField(seen, perceived[i]);
            }

            if (Tapestry != null)
            {
                Tapestry.Resample();
                var vicinity = Tapestry.Vicinity;
                for (var i = 0; i < vicinity.Count; i++)
                {
                    RememberField(seen, vicinity[i]);
                }
            }

            for (var i = 0; i < runes.Count; i++)
            {
                if (!FieldHas(seen, runes[i]))
                {
                    return false;
                }
            }

            return true;
        }

        static void RememberField(HashSet<RuneId> seen, RuneId rune)
        {
            if (rune == RuneId.None || !seen.Add(rune))
            {
                return;
            }

            if (ChainBook.TryBirth(rune, out var sources))
            {
                for (var i = 0; i < sources.Count; i++)
                {
                    RememberField(seen, sources[i]);
                }
            }
        }

        static bool FieldHas(HashSet<RuneId> seen, RuneId need)
        {
            if (need == RuneId.None || seen.Contains(need))
            {
                return true;
            }

            if (!ChainBook.TryBirth(need, out var sources) || sources.Count == 0)
            {
                return false;
            }

            for (var i = 0; i < sources.Count; i++)
            {
                if (!FieldHas(seen, sources[i]))
                {
                    return false;
                }
            }

            return true;
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
            return Composer.IsEmpty ? "empty string" : CallWorking(Composer.Snapshot());
        }

        public void ToggleSight()
        {
            GlyphView.Toggle();
            RefreshVisibleRunes();
            Log(GlyphView.IsDevelop
                ? "Develop sight. Names, letters, colours, and the written book are shown."
                : "Play sight. Only marks. The wall holds what you keep.");
        }

        public void RememberRune(RuneId rune)
        {
            if (rune == RuneId.None)
            {
                return;
            }

            if (Memory.Knows(rune))
            {
                Memory.TryForget(rune, out var forgotten);
                RefreshVisibleRunes();
                Log(forgotten);
                return;
            }

            Memory.TryKeep(rune, out var kept);
            RefreshVisibleRunes();
            Log(kept);
        }

        static string OffScreenNote(RuneId rune)
        {
            return GlyphView.Speak(
                $"{RuneCatalog.NameOf(rune)} is not on the screen. Walk until it is in view. Marks already in the string stay.",
                "That mark is not on the screen. Walk until it is in view. Marks already in the string stay.");
        }

        static string AimPreview(string name, SpellShape shape)
        {
            return $"{name} · {SpellFormations.NameOf(shape)}";
        }

        void RefreshVisibleRunes()
        {
            if (Tapestry != null)
            {
                Tapestry.Resample();
            }
        }

        /// <summary>
        /// Develop: the eleven, then every wrought elemental the room
        /// is speaking (Spark, Ice, Plant…). Play: kept marks, then
        /// those same spoken joins so a fire or vein can be drawn.
        /// </summary>
        List<RuneId> BuildWallRunes()
        {
            var list = new List<RuneId>(16);
            var seen = new HashSet<RuneId>();
            void Offer(RuneId rune)
            {
                if (rune != RuneId.None && seen.Add(rune))
                {
                    list.Add(rune);
                }
            }

            if (GlyphView.IsDevelop)
            {
                for (var i = 0; i < RuneCatalog.BasicRunes.Length; i++)
                {
                    Offer(RuneCatalog.BasicRunes[i]);
                }

                for (var i = 0; i < RuneCatalog.ElementalJoins.Length; i++)
                {
                    Offer(RuneCatalog.ElementalJoins[i]);
                }
            }
            else
            {
                var kept = Memory.Wall(RuneCatalog.BasicRunes);
                for (var i = 0; i < kept.Count; i++)
                {
                    Offer(kept[i]);
                }
            }

            var vicinity = Tapestry != null ? Tapestry.Vicinity : null;
            if (vicinity == null)
            {
                return list;
            }

            for (var i = 0; i < vicinity.Count; i++)
            {
                if (RuneCatalog.OffersOnWall(vicinity[i]))
                {
                    Offer(vicinity[i]);
                }
            }

            return list;
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
                ? WorldPhysics.Distance(clicked, world)
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
            if (player != null && WorldPhysics.Distance(clicked, player.position) > 3.6f)
            {
                Log($"Walk closer to look, or Store a form and aim from here. {Sight.YouSee(Sight.OfLock(clicked))}");
                return;
            }

            OpenCharter();
            Log($"{Sight.YouSee(Sight.OfLock(clicked))} String a key, then Charter Cast, Store, or Free Cast.");
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
                CloseCharter(releaseString: false);
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
                target = ResolveCastLock(shape, origin, aim, potency, spell)
                    ?? ResolveCastLock(shape, origin, requested, potency, spell)
                    ?? target;
                if (WorldWork.IsHop(SpellCodex.WorkOf(spell)))
                {
                    target = FindLockNear(aim, 1.8f) ?? FindLockNear(origin, 2.4f) ?? target;
                }

                var caption = GlyphView.IsPlay
                    ? string.Empty
                    : outcome.Spell != SpellId.None
                        ? CallWorking(composition)
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
                    SpellFx.Play(fxFrom, aim, material, shape, caption, () => finished = true, potency, SpellCodex.WorkOf(spell));
                }
            }
            catch (System.Exception exception)
            {
                Debug.LogWarning("Cast failed: " + exception.Message);
                Log("The spell fizzled. Try again — the lock still holds.");
                setupFailed = true;
                RecordWorking(composition, stance, false, SpellId.None);
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
                    var work = SpellCodex.WorkOf(outcome.Spell);
                    if (WorldWork.StopsOnWalls(work))
                    {
                        aim = WorldWork.ClampShot(Grid, origin, aim);
                        if (target != null && WorldPhysics.Occluded(Grid, work, origin, target))
                        {
                            target = null;
                        }
                    }

                    var workFrom = spanFrom ?? aim;
                    try
                    {
                        workNote = WorldWork.Apply(Grid, work, outcome.Material, origin, workFrom, aim, shape);
                        if (work == SpellId.Wall)
                        {
                            CombatActor.NoticePlayerSpell(work, origin, aim);
                        }
                    }
                    catch (System.Exception exception)
                    {
                        Debug.LogWarning("Terrain work failed: " + exception.Message);
                    }

                    yield return CarryCaster(SpellCodex.WorkOf(outcome.Spell), origin, requested);
                }

                var impactNote = string.Empty;
                try
                {
                    if (!outcome.Fizzled && outcome.Spell != SpellId.None)
                    {
                        var work = SpellCodex.WorkOf(outcome.Spell);
                        FocusLaw.Release(AdeptAvatar.Find(), work, composition, SpellVerb.Of(work, shape).Status);
                        var impact = SpellImpact.Apply(Grid, _locks, work, shape, origin, aim, outcome.Potency, composition);
                        impactNote = impact.Note;
                        if (impact.Locks.Count > 0)
                        {
                            for (var i = 0; i < impact.Locks.Count; i++)
                            {
                                var hit = impact.Locks[i];
                                if (!LockAlive(hit))
                                {
                                    continue;
                                }

                                if (!SpellVerb.HoldsMind(outcome.Spell))
                                {
                                    MarkHunted(hit);
                                }

                                if (SpellVerb.HoldsMind(outcome.Spell))
                                {
                                    Grimoire.LearnInterpretation(hit.FormulaId);
                                    workNote = FirstNote(impactNote, workNote);
                                    continue;
                                }

                                if (!Accepts(hit, outcome.Spell))
                                {
                                    continue;
                                }

                                ForgetHunted(hit);
                                Grimoire.LearnInterpretation(hit.FormulaId);
                                var flavor = hit.Resolve(outcome.Spell);
                                OpenDoorFor(hit);
                                if (_focus == hit)
                                {
                                    _focus = null;
                                }

                                CurrentTarget = null;
                                workNote = FirstNote(flavor, workNote);
                            }
                        }
                        else if (outcome.Resolved && LockAlive(target) && !SpellVerb.HoldsMind(outcome.Spell))
                        {
                            MarkHunted(target);
                            ForgetHunted(target);
                            Grimoire.LearnInterpretation(target.FormulaId);
                            var flavor = target.Resolve(outcome.Spell);
                            OpenDoorFor(target);
                            if (_focus == target)
                            {
                                _focus = null;
                            }

                            CurrentTarget = null;
                            workNote = FirstNote(flavor, workNote);
                        }

                        if (WorldWork.IsPush(work))
                        {
                            var shoved = ShoveHits(impact.Locks, target, origin);
                            workNote = FirstNote(shoved, workNote);
                        }
                    }
                    else if (outcome.Resolved && LockAlive(target) && !SpellVerb.HoldsMind(outcome.Spell))
                    {
                        Grimoire.LearnInterpretation(target.FormulaId);
                        var flavor = target.Resolve(outcome.Spell);
                        OpenDoorFor(target);
                        if (_focus == target)
                        {
                            _focus = null;
                        }

                        CurrentTarget = null;
                        workNote = FirstNote(flavor, workNote);
                    }

                    var worked = !outcome.Fizzled && outcome.Spell != SpellId.None;
                    RecordWorking(composition, stance, worked, outcome.Spell);
                    Log(GlyphView.IsDevelop
                        ? FirstNote(workNote, impactNote, outcome.Log)
                        : FirstNote(workNote, GlyphView.WorkLog(outcome)));
                    CheckFinished();
                }
                catch (System.Exception exception)
                {
                    Debug.LogWarning("Cast failed: " + exception.Message);
                    Log("The spell fizzled. Try again — the lock still holds.");
                    RecordWorking(composition, stance, false, outcome.Spell);
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
            spell = SpellCodex.WorkOf(spell);
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

            var point = SpellFormations.ClampPoint(shape, origin, requested, potency);
            if (WorldWork.StopsOnWalls(spell))
            {
                return WorldWork.ClampShot(Grid, origin, point);
            }

            return point;
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

            if (WorldWork.IsTimeStop(spell) && adept != null)
            {
                adept.HoldWorld(WorldWork.TimeStopSeconds);
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
            return ResolveCastLock(shape, CasterPosition(), mouse, 1f, PendingSpell);
        }

        ISpellLock ResolveCastLock(
            SpellShape shape,
            Vector3 origin,
            Vector3 requested,
            float potency = 1f,
            SpellId spell = SpellId.None)
        {
            var clicked = FindLockNear(requested, 1.85f);
            if (clicked != null && !WorldPhysics.Occluded(Grid, spell, origin, clicked))
            {
                return clicked;
            }

            var aimed = LockAtAim(shape, origin, requested, potency, spell);
            if (aimed != null)
            {
                return aimed;
            }

            if (LockAlive(_focus) && !WorldPhysics.Occluded(Grid, spell, origin, _focus))
            {
                return _focus;
            }

            return null;
        }

        ISpellLock LockAtAim(SpellShape shape, Vector3 origin, Vector3 requested, float potency = 1f, SpellId spell = SpellId.None)
        {
            var scale = potency <= 0f ? 1f : potency;
            var point = SpellFormations.ClampPoint(shape, origin, requested, scale);
            if (WorldWork.StopsOnWalls(spell))
            {
                point = WorldWork.ClampShot(Grid, origin, point);
            }

            var radius = Mathf.Max(
                SpellFormations.Get(shape).LockRadius * scale,
                WorldPhysics.WidthOf(spell, shape, scale));
            if (shape == SpellShape.Shot)
            {
                return FindLockAlong(origin, point, radius, spell);
            }

            if (shape == SpellShape.Spread || shape == SpellShape.Self)
            {
                return FindLockNear(origin, radius);
            }

            return FindLockNear(point, radius);
        }

        ISpellLock FindLockAlong(Vector3 from, Vector3 to, float radius, SpellId spell = SpellId.None)
        {
            return WorldPhysics.FirstAlong(_locks, from, to, Mathf.Max(radius, 1.15f), Grid, spell);
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
            var work = SpellCodex.WorkOf(PendingSpell);
            var drawLine = show && (ChosenShape == SpellShape.Shot || WorldWork.NeedsSpan(work) || WorldWork.IsHop(work));
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
            var mark = shape == SpellShape.Spread && !WorldWork.IsHop(SpellCodex.WorkOf(PendingSpell))
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
                    WorldPhysics.Distance(_focus, player.position) > 6.5f)
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

                var distance = WorldPhysics.Distance(encounter, point);
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

            var show = LockAlive(CurrentTarget) && Mode != PlayMode.Paused && Mode != PlayMode.Inventory &&
                Mode != PlayMode.Grimoire;
            _targetRing.gameObject.SetActive(show);
            if (!show)
            {
                return;
            }

            var mark = PlayerTransform() != null ? PlayerTransform().position : CurrentTarget.WorldPosition;
            _targetRing.transform.position = WorldPhysics.ClosestPoint(CurrentTarget, mark) + new Vector3(0f, -0.05f, 0f);
            var pulse = 0.95f + Mathf.Sin(Time.time * 6f) * 0.12f;
            _targetRing.transform.localScale = Vector3.one * pulse;
            _targetRing.color = Mode == PlayMode.Aiming
                ? new Color(0.45f, 0.92f, 0.95f, 0.95f)
                : new Color(1f, 0.86f, 0.35f, 0.95f);
        }

        public void MarkHunted(ISpellLock encounter)
        {
            if (encounter is not MonoBehaviour body || body == null)
            {
                return;
            }

            _hunted.Add(body.GetInstanceID());
        }

        public void ForgetHunted(ISpellLock encounter)
        {
            if (encounter is MonoBehaviour body && body != null)
            {
                _hunted.Remove(body.GetInstanceID());
            }
        }

        public bool IsHunted(ISpellLock encounter)
        {
            return encounter is MonoBehaviour body && body != null && _hunted.Contains(body.GetInstanceID());
        }

        public EncounterLock NearestHunted(Vector3 from, Component except = null)
        {
            EncounterLock best = null;
            var bestDistance = 9f;
            if (_locks == null)
            {
                return null;
            }

            for (var i = 0; i < _locks.Length; i++)
            {
                if (_locks[i] is not EncounterLock other || !LockAlive(other) || !IsHunted(other))
                {
                    continue;
                }

                if (except != null && other.gameObject == except.gameObject)
                {
                    continue;
                }

                var host = StatusHost.On(other);
                if (host != null && host.Has(StatusId.Charmed))
                {
                    continue;
                }

                var distance = Vector2.Distance(from, other.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = other;
                }
            }

            return best;
        }

        string ShoveHits(IReadOnlyList<ISpellLock> hits, ISpellLock fallback, Vector3 origin)
        {
            var pushed = 0;
            if (hits != null)
            {
                for (var i = 0; i < hits.Count; i++)
                {
                    if (ShoveLock(hits[i], origin))
                    {
                        pushed++;
                    }
                }
            }

            if (pushed == 0 && ShoveLock(fallback, origin))
            {
                pushed++;
            }

            if (pushed == 0)
            {
                return string.Empty;
            }

            return pushed == 1
                ? "Breath given a body finds them. They go."
                : "The wind takes them. They go.";
        }

        bool ShoveLock(ISpellLock encounter, Vector3 origin)
        {
            if (!LockAlive(encounter) || encounter is not MonoBehaviour body || body == null)
            {
                return false;
            }

            var land = WorldWork.PushLanding(Grid, origin, encounter.WorldPosition);
            if (Vector2.Distance(land, encounter.WorldPosition) < 0.2f)
            {
                return false;
            }

            var combat = body.GetComponent<CombatActor>();
            if (combat != null)
            {
                combat.PlaceAt(land);
            }
            else
            {
                body.transform.position = land;
            }

            return true;
        }

        void BurnLocksOnPillars()
        {
            if (_locks == null || Grid == null)
            {
                return;
            }

            for (var i = 0; i < _locks.Length; i++)
            {
                var encounter = _locks[i];
                if (!LockAlive(encounter) || encounter is not MonoBehaviour body || body == null)
                {
                    continue;
                }

                var tile = Grid.TileAtWorld(encounter.WorldPosition);
                var host = StatusHost.On(body);
                if (tile != null && tile.IsBurning)
                {
                    host?.Apply(StatusId.Burning, VitalLaw.FleshBurnSeconds);
                }

                if (!WorldWork.BurnsOccupants(tile))
                {
                    continue;
                }

                host?.Apply(StatusId.Burning, VitalLaw.FleshBurnSeconds);
                if (Accepts(encounter, SpellId.FlamePillar) || Accepts(encounter, SpellId.LavaPillar))
                {
                    UnmakeLock(encounter, $"{encounter.DisplayName} cannot stand in the flame. They fall.");
                    return;
                }
            }
        }

        public void TurnLock(ISpellLock encounter)
        {
            if (!LockAlive(encounter))
            {
                return;
            }

            ForgetHunted(encounter);
            Grimoire.LearnInterpretation(encounter.FormulaId);
            var flavor = encounter.Resolve(SpellId.None);
            OpenDoorFor(encounter);
            if (_focus == encounter)
            {
                _focus = null;
            }

            CurrentTarget = null;
            Log(flavor);
            CheckFinished();
        }

        public void UnmakeLock(ISpellLock encounter, string flavor)
        {
            if (!LockAlive(encounter))
            {
                return;
            }

            ForgetHunted(encounter);
            Grimoire.LearnInterpretation(encounter.FormulaId);
            encounter.Resolve(SpellId.None);
            OpenDoorFor(encounter);
            if (_focus == encounter)
            {
                _focus = null;
            }

            CurrentTarget = null;
            Log(string.IsNullOrEmpty(flavor) ? "The lock falls." : flavor);
            CheckFinished();
        }

        public void FallInPit(Transform player)
        {
            var wet = Underfoot != null && Underfoot.Material == MaterialId.Water;
            KnockBack(player, wet
                ? "You cannot swim. Freeze it, span it, dry it, or give breath a body and cross."
                : "The pit takes you. Raise a column, draw a wall across, or give breath a body and leap.");
        }

        public void KnockBack(Transform player, string message)
        {
            PlacePlayer(player, _safePoint, message);
        }

        public void KillPlayer(string message)
        {
            KillPlayer(DeathCause.Plain(message));
        }

        public void KillPlayer(DeathCause cause)
        {
            var player = PlayerTransform();
            if (player == null)
            {
                return;
            }

            if (!cause.Exists)
            {
                cause = DeathCause.Plain("You fall. The work you stood forgets itself.");
            }

            SweepOwnWork(CurrentRoom, player.position);
            player.GetComponent<AdeptAvatar>()?.ClearWork();
            StatusHost.On(player)?.Clear();
            if (Mode == PlayMode.Aiming)
            {
                CancelAim();
            }

            LastDeath = cause;
            LastDeathAt = Time.unscaledTime;
            PlacePlayer(player, _spawnPoint, cause.LogLine);
        }

        public void YieldSelf()
        {
            KillPlayer(GlyphView.Speak(
                "You yield. Pillars, walls, and hanging work in this room fall. Stones and artifacts stay with you.",
                "You yield. The work you stood in this room forgets itself. What you carry stays."));
        }

        void SweepOwnWork(RoomInfo room, Vector3 origin)
        {
            if (Grid != null)
            {
                foreach (var tile in Grid.All)
                {
                    if (tile == null || !tile.IsConjured)
                    {
                        continue;
                    }

                    if (room != null)
                    {
                        if (!room.Contains(tile.transform.position))
                        {
                            continue;
                        }
                    }
                    else if (Vector2.Distance(origin, tile.transform.position) > 10f)
                    {
                        continue;
                    }

                    tile.RestoreFoundation();
                }
            }

            if (room != null)
            {
                VeilField.ClearInBounds(room.Bounds);
            }
            else
            {
                VeilField.ClearNear(Grid, origin, VeilKind.None, 8);
            }
        }

        public string PlayerStatuses()
        {
            var player = PlayerTransform();
            var host = StatusHost.On(player);
            return host != null ? host.Summary() : string.Empty;
        }

        public string ConcentrationLine()
        {
            var player = PlayerTransform();
            return player != null ? StatusHost.HeldBy(player) : string.Empty;
        }

        bool PlayerBlocksAction()
        {
            var host = StatusHost.On(PlayerTransform());
            return host != null && host.BlocksAction;
        }

        static bool Accepts(ISpellLock encounter, SpellId spell)
        {
            if (encounter is BarrierLock barrier && barrier.YieldsTo(spell))
            {
                return true;
            }

            if (encounter is ChargeGate charge && charge.YieldsTo(spell))
            {
                return true;
            }

            if (encounter?.AcceptedKeys == null)
            {
                return false;
            }

            for (var i = 0; i < encounter.AcceptedKeys.Length; i++)
            {
                if (encounter.AcceptedKeys[i] == spell)
                {
                    return true;
                }
            }

            return false;
        }

        void PlacePlayer(Transform player, Vector3 point, string message)
        {
            if (player == null)
            {
                return;
            }

            var body = player.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                body.position = point;
            }

            player.position = point;
            if (!string.IsNullOrEmpty(message))
            {
                Log(message);
            }
        }

        void CheckFinished()
        {
            if (_finished || _locks == null)
            {
                return;
            }

            var hasFloorGate = false;
            foreach (var encounter in _locks)
            {
                if ((encounter is SocketGate socket && socket.FinishesFloor)
                    || (encounter is ChargeGate charge && charge.FinishesFloor))
                {
                    hasFloorGate = true;
                    if (encounter.Resolved)
                    {
                        _finished = true;
                        Log("The sockets of this floor are seated. The way down stands open.");
                        return;
                    }
                }
            }

            if (hasFloorGate)
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

        public string CallWorking(Composition composition) => Grimoire.Names.Call(composition);

        public string CallWorking(IReadOnlyList<RuneId> runes) => Grimoire.Names.Call(runes);

        void RecordWorking(Composition composition, CastingStance stance, bool worked, SpellId spell)
        {
            var given = Grimoire.Names.SavedOrEmpty(composition.Sequence);
            Ledger.Record(composition, stance, worked, spell, given, saved: !string.IsNullOrEmpty(given));
        }

        void UpdateSight()
        {
            if (!GameHud.PointerOverChrome(Mode) && TryMouseWorld(out var world))
            {
                var lookable = Lookables.Nearest(world);
                if (lookable != null)
                {
                    SightLine = Sight.YouSee(lookable.LookText);
                    return;
                }

                var hovered = FindLockNear(world, 0.9f);
                if (hovered != null)
                {
                    SightLine = Sight.YouSee(Sight.OfLock(hovered));
                    return;
                }

                if (Grid != null)
                {
                    var tile = Grid.TileAtWorld(world);
                    if (tile != null)
                    {
                        SightLine = Sight.YouSee(Sight.OfTile(tile));
                        return;
                    }
                }
            }

            if (LockAlive(CurrentTarget))
            {
                SightLine = Sight.YouSee(Sight.OfLock(CurrentTarget));
                return;
            }

            var room = CurrentRoom != null ? CurrentRoom.Name : "the sanctum";
            SightLine = Sight.YouSee(room + ".");
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
            if (RuneStele.TryPick(world, out var rune) || RuneStringSource.TryPick(world, out rune) ||
                CoverCatalog.TryPick(world, out rune))
            {
                WeaveFromField(rune);
                return true;
            }

            if (Tapestry == null || !Tapestry.TryPick(world, out rune))
            {
                return false;
            }

            WeaveFromField(rune);
            return true;
        }
    }
}
