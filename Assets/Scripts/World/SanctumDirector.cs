using UnityEngine;

namespace RuneMagic
{
    public sealed class SanctumDirector : MonoBehaviour
    {
        public SpellComposer Composer { get; } = new();
        public Grimoire Grimoire { get; } = new();
        public EncounterLock CurrentTarget { get; private set; }
        public string LastLog { get; private set; } = "The field is already here. Read it. Compose from what flows.";
        public float Taint { get; private set; }

        readonly CastResolver _resolver = new();
        EncounterLock[] _locks;
        bool _finished;

        public void Begin(EncounterLock[] locks)
        {
            _locks = locks;
        }

        void Update()
        {
            CurrentTarget = FindNearestLock();
            HandleInput();
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
            var hit = Physics2D.OverlapPoint(world);
            if (hit == null)
            {
                return;
            }

            var orb = hit.GetComponent<RuneOrb>();
            if (orb != null)
            {
                FindFirstObjectByType<RuneField>().Select(orb.Rune);
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
                CurrentTarget.Resolve();
                CurrentTarget = null;
            }

            Log(outcome.Log);
            Composer.Clear();
            CheckFinished();
        }

        EncounterLock FindNearestLock()
        {
            EncounterLock best = null;
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

                var distance = Vector2.Distance(player.transform.position, encounter.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = encounter;
                }
            }

            return best;
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
            Log("The sanctum is read. Both locks turned. The trees are yours to grow.");
        }

        public void Log(string message)
        {
            LastLog = message;
        }
    }
}
