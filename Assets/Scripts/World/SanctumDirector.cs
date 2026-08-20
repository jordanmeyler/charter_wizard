using UnityEngine;

namespace RuneMagic
{
    public sealed class SanctumDirector : MonoBehaviour
    {
        public SpellComposer Composer { get; } = new();
        public Grimoire Grimoire { get; } = new();
        public ISpellLock CurrentTarget { get; private set; }
        public RoomInfo CurrentRoom { get; private set; }
        public WorldTile Underfoot { get; private set; }
        public string LastLog { get; private set; } = "The field is already here. Read it. Compose from what flows.";
        public float Taint { get; private set; }
        public WorldGrid Grid { get; private set; }

        readonly CastResolver _resolver = new();
        ISpellLock[] _locks;
        RoomInfo[] _rooms;
        Vector3 _safePoint;
        bool _finished;

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
            if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Q))
            {
                Composer.ToggleStance();
                Log(Composer.Stance == CastingStance.Charter
                    ? "Bound stance. Reliable. The Charter siphons a little light."
                    : "Unbound stance. Higher magnitude, variance-loaded. Never the required key.");
            }

            if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.Backspace))
            {
                Composer.Clear();
                Log("The composition is released back into the field.");
            }

            if (Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.Return))
            {
                Cast();
            }

            for (var i = 0; i < RuneField.StartingStream.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i))
                {
                    var field = FindFirstObjectByType<RuneField>();
                    field.Select(RuneField.StartingStream[i]);
                }
            }

            if (Input.GetMouseButtonDown(0))
            {
                TryClickRune();
            }
        }

        void TryClickRune()
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
                var orb = hit.GetComponent<RuneOrb>();
                if (orb != null)
                {
                    FindFirstObjectByType<RuneField>().Select(orb.Rune);
                    return;
                }
            }
        }

        void Cast()
        {
            var accepted = CurrentTarget != null && !CurrentTarget.Resolved
                ? CurrentTarget.AcceptedKeys
                : System.Array.Empty<SpellId>();

            var outcome = _resolver.Resolve(Composer.Snapshot(), Composer.Stance, accepted, Grimoire);
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

            Composer.Clear();
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
