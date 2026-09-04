using UnityEngine;

namespace RuneMagic
{
    public enum CombatKind
    {
        None,
        Golem,
        Wizard,
        Archer
    }

    public enum ShotAllegiance
    {
        Hostile,
        Allied,
        Wild
    }

    /// <summary>
    /// A lock that can strike back. Slots cover close, mid, and long.
    /// Modes hold, hunt, or keep distance. Gambits answer the room
    /// the way a fire mage answers a wall with a stood flame.
    /// Mind ailments rewrite who they hunt, and whether they stand still.
    /// </summary>
    public sealed class CombatActor : MonoBehaviour
    {
        public CombatKind Kind { get; private set; }
        public CombatMode Mode { get; private set; }
        public float CastSeconds { get; private set; } = 2f;
        public Vector2 Facing { get; private set; } = Vector2.left;
        public RuneId[] CastRecipe => _castRecipe;
        public bool AlliedWithAdept => _status != null && _status.Has(StatusId.Charmed);

        ISpellLock _lock;
        StatusHost _status;
        WorldGrid _grid;
        Collider2D _hit;
        bool _baseTrigger;
        float _windup;
        bool _casting;
        Vector2 _committed;
        SpriteRenderer _sprite;
        SpriteAnim _anim;
        TextMesh _castChip;
        RuneId[] _castRecipe = System.Array.Empty<RuneId>();
        float _reach = 1.2f;
        float _sight = 8.2f;
        float _oilBulk;
        float _walk = 2.6f;
        Vector3 _restScale = Vector3.one;
        Vector3 _idleOrigin;
        float _phase;
        float _wanderUntil;
        Vector2 _wanderDir = Vector2.left;
        float _confusedUntil;
        Transform _confusedMark;
        SanctumDirector _director;
        CombatPlan _plan;
        CombatSlot _active;
        CombatGambit _pending;
        bool[] _gambitSpent = System.Array.Empty<bool>();
        float _close = CombatBook.DefaultClose;
        float _mid = CombatBook.DefaultMid;
        float _long = CombatBook.DefaultLong;
        float _actionSeconds = 2f;
        ICarryable _carried;
        Sprite[] _idleFrames;
        Sprite[] _attackFrames;
        string _idleClip = string.Empty;
        string _attackClip = string.Empty;
        const float FetchReach = 1.15f;

        public void Bind(CombatKind kind, float castSeconds, WorldGrid grid, RuneId[] castRecipe = null)
        {
            Bind(CombatBook.PlanFromLegacy(kind, castSeconds, castRecipe), grid);
        }

        public void Bind(CombatPlan plan, WorldGrid grid)
        {
            _plan = plan ?? CombatBook.PlanFromLegacy(CombatKind.None, 2f, null);
            Kind = _plan.Kind;
            Mode = _plan.Mode;
            CastSeconds = Mathf.Max(0.35f, _plan.CastSeconds);
            _close = _plan.CloseRange;
            _mid = _plan.MidRange;
            _long = _plan.LongRange;
            _reach = _close;
            _sight = _long;
            _grid = grid;
            _lock = GetComponent<ISpellLock>();
            _status = GetComponent<StatusHost>();
            _sprite = GetComponent<SpriteRenderer>();
            _hit = GetComponent<Collider2D>();
            _baseTrigger = _hit != null && _hit.isTrigger;
            _anim = GetComponent<SpriteAnim>() ?? SpriteAnim.On(gameObject, _sprite);
            _restScale = transform.localScale;
            _oilBulk = 0f;
            _idleOrigin = transform.position;
            _castRecipe = FirstRecipe(_plan);
            _gambitSpent = _plan.Gambits != null && _plan.Gambits.Length > 0
                ? new bool[_plan.Gambits.Length]
                : System.Array.Empty<bool>();
            _castChip = WorldLabel.Attach(transform, "", new Vector3(0f, 1.62f, 0f),
                new Color(1f, 0.72f, 0.28f), DrawDepth.CastChip);
            if (_castChip != null)
            {
                _castChip.characterSize = 0.05f;
            }
        }

        /// <summary>
        /// Unity sprites on the EncounterLock win. Clip ids are the
        /// fallback (pack enemy-011, or the generated fire-golem / warden).
        /// </summary>
        public void BindLooks(Sprite[] idleFrames, Sprite[] attackFrames, string idleClip, string attackClip)
        {
            _idleFrames = idleFrames;
            _attackFrames = attackFrames;
            _idleClip = idleClip ?? string.Empty;
            _attackClip = attackClip ?? string.Empty;
        }

        public void FeedOil()
        {
            if (_status == null || _status.Nature != CreatureNature.Fire)
            {
                return;
            }

            _oilBulk = Mathf.Min(1.6f, _oilBulk + 0.35f);
            _restScale = Vector3.one * (1f + _oilBulk * 0.45f);
            _reach = _close + _oilBulk * 0.55f;
            transform.localScale = _restScale;
        }

        float SightNow()
        {
            if (_grid != null && WorldPhysics.AuraAt(_grid, transform.position, out var kind) && kind == VeilKind.Darkness)
            {
                return 0.35f;
            }

            return _long > 0.05f ? _long : _sight;
        }

        static RuneId[] FirstRecipe(CombatPlan plan)
        {
            if (plan?.Slots == null)
            {
                return System.Array.Empty<RuneId>();
            }

            for (var i = 0; i < plan.Slots.Length; i++)
            {
                var slot = plan.Slots[i];
                if (slot == null)
                {
                    continue;
                }

                var runes = CombatBook.ParseRecipe(slot.Recipe);
                if (runes != null && runes.Length > 0)
                {
                    return runes;
                }

                if (slot.Spell != SpellId.None)
                {
                    return CombatBook.RecipeOf(slot.Spell);
                }
            }

            return System.Array.Empty<RuneId>();
        }

        void Update()
        {
            if (Kind == CombatKind.None && _status == null)
            {
                return;
            }

            if (_lock == null || _lock.Resolved)
            {
                DropCarried();
                ClearCastChip();
                return;
            }

            if (AdeptAvatar.WorldHeld)
            {
                ClearCastChip();
                return;
            }

            if (_carried != null && (_status == null || !_status.Has(StatusId.Charmed)))
            {
                DropCarried();
            }

            SyncPassage();
            if (_status != null && _status.BlocksAction)
            {
                CancelWindup();
                if (_status.Has(StatusId.Sleeping))
                {
                    transform.localScale = new Vector3(_restScale.x, _restScale.y * 0.55f, 1f);
                }

                ShowMindChip();
                return;
            }

            if (_windup <= 0f && !_casting)
            {
                transform.localScale = _restScale;
            }

            var mind = _status != null ? _status.MindAilment : StatusId.None;
            if (mind == StatusId.None && Kind == CombatKind.None && Mode == CombatMode.Wander
                && (_plan == null || _plan.Slots == null || _plan.Slots.Length == 0)
                && (_plan == null || _plan.Gambits == null || _plan.Gambits.Length == 0))
            {
                TickIdle();
                return;
            }

            var player = AdeptAvatar.Find();
            if (mind == StatusId.Frightened)
            {
                Flee(player);
                return;
            }

            if (mind == StatusId.Charmed)
            {
                TickCharm(player);
                return;
            }

            var mark = PickMark(mind, player);
            if (mark == null)
            {
                if (mind == StatusId.Confused)
                {
                    Wander();
                    return;
                }

                if (mind == StatusId.Raging)
                {
                    TickSelfTurn();
                    return;
                }

                Wander();
                return;
            }

            DriveToward(mark, player, mind != StatusId.None);
        }

        void TickIdle()
        {
            CancelWindup();
            _phase += Time.deltaTime;
            transform.position = _idleOrigin + new Vector3(Mathf.Sin(_phase * 0.7f) * 0.2f, Mathf.Cos(_phase * 0.45f) * 0.12f, 0f);
        }

        void DriveToward(Transform mark, AdeptAvatar player, bool chase)
        {
            var toMark = (Vector2)(mark.position - transform.position);
            var distance = toMark.magnitude;
            if (distance > 0.05f)
            {
                Face(toMark);
            }

            var slot = PickSlot(distance, mark);
            if (slot != null)
            {
                BeginAction(slot);
                if (slot.Strike == CombatStrike.Slam)
                {
                    TickMelee(mark, player, distance, chase);
                    return;
                }

                TickRanged(mark, toMark, distance, slot);
                return;
            }

            CancelWindup();
            MoveForMode(mark, distance, chase);
        }

        void BeginAction(CombatSlot slot)
        {
            if (slot == null)
            {
                return;
            }

            if (!SameAction(_active, slot))
            {
                CancelWindup();
            }

            _active = slot;
            var runes = CombatBook.ParseRecipe(slot.Recipe);
            if (runes == null || runes.Length == 0)
            {
                runes = slot.Spell != SpellId.None
                    ? CombatBook.RecipeOf(slot.Spell)
                    : System.Array.Empty<RuneId>();
            }

            _castRecipe = runes;
            _actionSeconds = CombatBook.SecondsOf(slot, CastSeconds);
        }

        static bool SameAction(CombatSlot a, CombatSlot b)
        {
            if (a == null || b == null)
            {
                return a == b;
            }

            return a.Strike == b.Strike && a.Spell == b.Spell && a.Range == b.Range;
        }

        CombatSlot PickSlot(float distance, Transform mark)
        {
            var gambit = MatchGambit(distance, mark);
            if (gambit != null)
            {
                return CombatBook.SlotFromGambit(gambit);
            }

            var slots = _plan != null ? _plan.Slots : null;
            if (slots == null || slots.Length == 0)
            {
                return null;
            }

            var band = CombatBook.BandOf(distance, _close, _mid, _long);
            CombatSlot bandMatch = null;
            CombatSlot usable = null;
            for (var i = 0; i < slots.Length; i++)
            {
                var slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                var strike = slot.Strike;
                if (strike == CombatStrike.None && slot.Spell != SpellId.None)
                {
                    strike = CombatBook.StrikeOf(slot.Spell);
                }

                if (strike == CombatStrike.None)
                {
                    continue;
                }

                if (slot.Range == band)
                {
                    bandMatch ??= slot;
                }

                var max = CombatBook.MaxOf(slot.Range, _close, _mid, _long);
                if (distance <= max + 0.15f)
                {
                    usable ??= slot;
                }
            }

            return bandMatch ?? usable;
        }

        CombatGambit MatchGambit(float distance, Transform mark)
        {
            if (_pending != null)
            {
                return _pending;
            }

            var gambits = _plan != null ? _plan.Gambits : null;
            if (gambits == null)
            {
                return null;
            }

            for (var i = 0; i < gambits.Length; i++)
            {
                if (i < _gambitSpent.Length && _gambitSpent[i])
                {
                    continue;
                }

                var gambit = gambits[i];
                if (gambit == null || !Matches(gambit, distance, mark))
                {
                    continue;
                }

                if (gambit.Once && i < _gambitSpent.Length)
                {
                    _gambitSpent[i] = true;
                }

                return gambit;
            }

            return null;
        }

        bool Matches(CombatGambit gambit, float distance, Transform mark)
        {
            var band = CombatBook.BandOf(distance, _close, _mid, _long);
            switch (gambit.When)
            {
                case GambitWhen.Always:
                    return true;
                case GambitWhen.InCloseRange:
                    return band == CombatRange.Close;
                case GambitWhen.InMidRange:
                    return band == CombatRange.Mid;
                case GambitWhen.InLongRange:
                    return band == CombatRange.Long && distance <= _long;
                case GambitWhen.AllyNearby:
                    var ally = NearestLock(includeCharmed: true);
                    return ally != null && Vector2.Distance(transform.position, ally.position) <= _mid;
                case GambitWhen.SelfHasStatus:
                    return _status != null && gambit.WhenStatus != StatusId.None && _status.Has(gambit.WhenStatus);
                case GambitWhen.TargetHasStatus:
                    var host = mark != null ? StatusHost.On(mark) : null;
                    return host != null && gambit.WhenStatus != StatusId.None && host.Has(gambit.WhenStatus);
                default:
                    return false;
            }
        }

        void MoveForMode(Transform mark, float distance, bool chase)
        {
            var mode = _plan != null ? _plan.Mode : Mode;
            if (!chase && (mode == CombatMode.Guard || mode == CombatMode.Caster))
            {
                PlayMotion(false, 5f);
                ShowMindChip();
                return;
            }

            if (!chase && mode == CombatMode.Wander)
            {
                Wander();
                return;
            }

            if (!chase && mode == CombatMode.Skirmish)
            {
                var prefer = PreferredDistance();
                var away = (Vector2)(transform.position - mark.position);
                if (distance < prefer - 0.6f && away.sqrMagnitude > 0.01f)
                {
                    Face(away);
                    StepToward(transform.position + (Vector3)away.normalized);
                }
                else if (distance > prefer + 0.6f)
                {
                    StepToward(mark.position);
                }

                PlayMotion(false, 5f);
                ShowMindChip();
                return;
            }

            if (distance > _reach + 0.15f)
            {
                StepToward(mark.position);
            }

            PlayMotion(false, 5f);
            ShowMindChip();
        }

        float PreferredDistance()
        {
            var slots = _plan != null ? _plan.Slots : null;
            if (slots != null)
            {
                for (var i = 0; i < slots.Length; i++)
                {
                    if (slots[i] == null)
                    {
                        continue;
                    }

                    if (slots[i].Range == CombatRange.Long)
                    {
                        return (_mid + _long) * 0.5f;
                    }

                    if (slots[i].Range == CombatRange.Mid)
                    {
                        return (_close + _mid) * 0.5f;
                    }
                }
            }

            return _mid;
        }

        void TickCharm(AdeptAvatar player)
        {
            if (_carried != null && _carried.Collected)
            {
                _carried = null;
            }

            if (_carried != null)
            {
                Follow(player);
                if (player != null && Vector2.Distance(transform.position, player.transform.position) < 1.85f)
                {
                    var name = _carried.CarryName;
                    if (_carried.DeliverTo(player))
                    {
                        Director()?.Log($"{(_lock != null ? _lock.DisplayName : "They")} lay {name} at your feet.");
                    }

                    _carried = null;
                }

                return;
            }

            var prize = NearestPrize();
            if (prize != null)
            {
                Fetch(prize);
                return;
            }

            var hunted = Director()?.NearestHunted(transform.position, this);
            if (hunted != null)
            {
                DriveToward(hunted.transform, player, true);
                return;
            }

            Follow(player);
        }

        void Fetch(ICarryable prize)
        {
            CancelWindup();
            var to = (Vector2)(prize.WorldPosition - transform.position);
            var distance = to.magnitude;
            if (distance > 0.05f)
            {
                Face(to);
            }

            if (distance <= FetchReach)
            {
                if (prize.TryCarry(transform))
                {
                    _carried = prize;
                    ShowCast("fetches…");
                }

                return;
            }

            StepToward(prize.WorldPosition);
            ShowCast("fetches…");
            PlayMotion(false, 5f);
        }

        ICarryable NearestPrize()
        {
            ICarryable best = null;
            var bestDistance = SightNow();
            Consider(FindObjectsByType<WorldItem>(FindObjectsSortMode.None), ref best, ref bestDistance);
            Consider(FindObjectsByType<FreeCharm>(FindObjectsSortMode.None), ref best, ref bestDistance);
            return best;
        }

        void Consider<T>(T[] found, ref ICarryable best, ref float bestDistance) where T : Component, ICarryable
        {
            if (found == null)
            {
                return;
            }

            for (var i = 0; i < found.Length; i++)
            {
                var item = found[i];
                if (item == null || !item.CanFetch)
                {
                    continue;
                }

                var distance = Vector2.Distance(transform.position, item.WorldPosition);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = item;
                }
            }
        }

        static bool CanSeeAdept(AdeptAvatar player)
        {
            if (player == null)
            {
                return false;
            }

            var host = StatusHost.On(player);
            return host == null || !host.IsHidden;
        }

        static Transform SeenAdept(AdeptAvatar player)
        {
            return CanSeeAdept(player) ? player.transform : null;
        }

        Transform PickMark(StatusId mind, AdeptAvatar player)
        {
            switch (mind)
            {
                case StatusId.Charmed:
                    return NearestLock(includeCharmed: false);
                case StatusId.Raging:
                    return NearestCreature(player, preferLocks: true);
                case StatusId.Confused:
                    return ConfusedMark(player);
                default:
                    return SeenAdept(player);
            }
        }

        Transform ConfusedMark(AdeptAvatar player)
        {
            if (Time.time >= _confusedUntil || _confusedMark == null || !MarkAlive(_confusedMark))
            {
                _confusedUntil = Time.time + Random.Range(1.1f, 2.2f);
                _confusedMark = RandomMark(player);
                _wanderDir = Random.insideUnitCircle.normalized;
                if (_wanderDir.sqrMagnitude < 0.01f)
                {
                    _wanderDir = Vector2.right;
                }
            }

            return _confusedMark;
        }

        Transform RandomMark(AdeptAvatar player)
        {
            var roll = Random.value;
            if (roll < 0.35f)
            {
                return null;
            }

            if (roll < 0.6f && CanSeeAdept(player))
            {
                return player.transform;
            }

            return NearestLock(includeCharmed: true);
        }

        Transform NearestCreature(AdeptAvatar player, bool preferLocks)
        {
            var other = NearestLock(includeCharmed: true);
            var seen = SeenAdept(player);
            if (preferLocks && other != null)
            {
                if (seen == null)
                {
                    return other;
                }

                var toLock = Vector2.Distance(transform.position, other.position);
                var toPlayer = Vector2.Distance(transform.position, seen.position);
                return toLock <= toPlayer + 0.85f ? other : seen;
            }

            if (other == null)
            {
                return seen;
            }

            if (seen == null)
            {
                return other;
            }

            var lockDistance = Vector2.Distance(transform.position, other.position);
            var playerDistance = Vector2.Distance(transform.position, seen.position);
            return lockDistance < playerDistance ? other : seen;
        }

        Transform NearestLock(bool includeCharmed)
        {
            EncounterLock best = null;
            var bestDistance = SightNow();
            var found = FindObjectsByType<EncounterLock>(FindObjectsSortMode.None);
            for (var i = 0; i < found.Length; i++)
            {
                var other = found[i];
                if (other == null || other.Resolved || other.gameObject == gameObject)
                {
                    continue;
                }

                if (!includeCharmed)
                {
                    var host = StatusHost.On(other);
                    if (host != null && host.Has(StatusId.Charmed))
                    {
                        continue;
                    }
                }

                var distance = Vector2.Distance(transform.position, other.transform.position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = other;
                }
            }

            return best != null ? best.transform : null;
        }

        static bool MarkAlive(Transform mark)
        {
            if (mark == null)
            {
                return false;
            }

            var encounter = mark.GetComponent<EncounterLock>();
            return encounter == null || !encounter.Resolved;
        }

        void TickMelee(Transform mark, AdeptAvatar player, float distance, bool chase)
        {
            var reach = _active != null
                ? CombatBook.MaxOf(_active.Range, _close, _mid, _long)
                : _reach;
            if (distance > reach + 0.15f)
            {
                _windup = 0f;
                transform.localScale = _restScale;
                MoveForMode(mark, distance, chase);
                return;
            }

            _windup += Time.deltaTime;
            ShowCast(MindVerb());
            PlayMotion(true, 6f);
            var need = _actionSeconds > 0.35f ? _actionSeconds : CastSeconds;
            var wind = Mathf.Clamp01(_windup / need);
            transform.localScale = new Vector3(_restScale.x * (1f + wind * 0.12f), _restScale.y * (1f - wind * 0.12f), 1f);
            if (_windup < need)
            {
                return;
            }

            _windup = 0f;
            transform.localScale = _restScale;
            PlayMotion(false, 5f);
            ClearCastChip();
            FinishPending();
            LandBlow(mark, player);
        }

        void TickSelfTurn()
        {
            _windup += Time.deltaTime;
            ShowCast("turns…");
            var wind = Mathf.Clamp01(_windup / CastSeconds);
            transform.localScale = new Vector3(_restScale.x * (1f + wind * 0.12f), _restScale.y * (1f - wind * 0.12f), 1f);
            if (_windup < CastSeconds)
            {
                return;
            }

            _windup = 0f;
            transform.localScale = _restScale;
            Director()?.TurnLock(_lock);
        }

        void TickRanged(Transform mark, Vector2 toMark, float distance, CombatSlot slot)
        {
            var reach = CombatBook.MaxOf(slot.Range, _close, _mid, _long);
            if (!_casting)
            {
                if (distance > Mathf.Max(reach, SightNow()) + 0.15f)
                {
                    return;
                }

                _casting = true;
                _windup = 0f;
                _committed = toMark.sqrMagnitude > 0.01f ? toMark.normalized : Facing;
            }

            var need = _actionSeconds > 0.35f ? _actionSeconds : CastSeconds;
            _windup += Time.deltaTime;
            var left = Mathf.Max(0f, need - _windup);
            var mind = _status != null ? _status.MindAilment : StatusId.None;
            if (mind != StatusId.None)
            {
                ShowCast($"{MindVerb()} {left:0.0}");
            }
            else
            {
                ShowCast(_castRecipe.Length > 0 ? string.Empty : $"casting… {left:0.0}");
                ShowRunes();
            }

            PlayMotion(true, 5f);
            if (_windup < need)
            {
                return;
            }

            _casting = false;
            _windup = 0f;
            PlayMotion(false, 4f);
            ClearCastChip();
            FinishPending();
            var spell = CombatBook.SpellOf(slot);
            if (spell == SpellId.None)
            {
                spell = CombatBook.SpellFromRecipe(_castRecipe);
            }

            var strike = slot.Strike == CombatStrike.None ? CombatBook.StrikeOf(spell) : slot.Strike;
            var aim = mark != null ? mark.position : transform.position + (Vector3)_committed;
            if (strike == CombatStrike.Pillar || WorldWork.IsPillar(spell) || WorldWork.NeedsSpan(spell))
            {
                if (spell == SpellId.None)
                {
                    spell = SpellId.FlamePillar;
                }

                EnemyPillar.Cast(_grid, transform.position, aim, spell, this, ShotOf(), _castRecipe, _committed);
                return;
            }

            var shot = CombatBook.ShotKind(spell, _castRecipe);
            var origin = transform.position + (Vector3)(_committed * 0.45f);
            WorldProjectile.Spawn(origin, _committed, shot, _grid, shot == ProjectileKind.Fireball ? 6.4f : 7.4f, this, ShotOf(), _castRecipe);
        }

        public static void NoticePlayerSpell(SpellId spell, Vector3 origin, Vector3 aim)
        {
            if (spell == SpellId.None)
            {
                return;
            }

            var director = Object.FindFirstObjectByType<SanctumDirector>();
            var here = director != null ? director.RoomAt(origin) : null;
            var there = director != null ? director.RoomAt(aim) : null;
            var found = Object.FindObjectsByType<CombatActor>(FindObjectsSortMode.None);
            for (var i = 0; i < found.Length; i++)
            {
                var actor = found[i];
                if (actor == null || actor._lock == null || actor._lock.Resolved)
                {
                    continue;
                }

                if (!SameRoom(director, actor, here, there, origin, aim))
                {
                    continue;
                }

                actor.NoticeSpell(spell);
            }
        }

        static bool SameRoom(
            SanctumDirector director,
            CombatActor actor,
            RoomInfo here,
            RoomInfo there,
            Vector3 origin,
            Vector3 aim)
        {
            if (director == null || (here == null && there == null))
            {
                return Vector2.Distance(actor.transform.position, origin) <= 12f
                    || Vector2.Distance(actor.transform.position, aim) <= 12f;
            }

            var room = director.RoomAt(actor.transform.position);
            return room != null && (room == here || room == there);
        }

        void NoticeSpell(SpellId spell)
        {
            var gambits = _plan != null ? _plan.Gambits : null;
            if (gambits != null)
            {
                for (var i = 0; i < gambits.Length; i++)
                {
                    if (i < _gambitSpent.Length && _gambitSpent[i])
                    {
                        continue;
                    }

                    var gambit = gambits[i];
                    if (gambit == null)
                    {
                        continue;
                    }

                    var wall = gambit.When == GambitWhen.PlayerRaisesWall && spell == SpellId.Wall;
                    var cast = gambit.When == GambitWhen.PlayerCasts
                        && (gambit.WhenSpell == SpellId.None || gambit.WhenSpell == spell);
                    if (!wall && !cast)
                    {
                        continue;
                    }

                    _pending = gambit;
                    if (gambit.Once && i < _gambitSpent.Length)
                    {
                        _gambitSpent[i] = true;
                    }

                    CancelWindup();
                    var who = _lock != null ? _lock.DisplayName : "They";
                    var then = CombatBook.NameOf(gambit.ThenSpell, gambit.ThenStrike);
                    Director()?.Log(spell == SpellId.Wall && gambit.ThenSpell == SpellId.FlamePillar
                        ? $"The wall stands. {who} writes hunger and asks it to rest."
                        : $"{who} answers. They write {then}.");
                    return;
                }
            }

            if (spell == SpellId.Wall)
            {
                AnswerWall();
            }
        }

        void AnswerWall()
        {
            if (_pending != null || !WritesFire() || !InMixedCourt())
            {
                return;
            }

            _pending = CombatBook.WallToFlamePillar();
            CancelWindup();
            var who = _lock != null ? _lock.DisplayName : "The adept";
            Director()?.Log($"The wall stands. {who} writes hunger and asks it to rest.");
        }

        bool WritesFire()
        {
            if (Kind != CombatKind.Wizard)
            {
                return false;
            }

            if (CombatBook.WritesFire(_castRecipe))
            {
                return true;
            }

            var slots = _plan != null ? _plan.Slots : null;
            if (slots == null)
            {
                return false;
            }

            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null && CombatBook.WritesFire(CombatBook.ParseRecipe(slots[i].Recipe)))
                {
                    return true;
                }
            }

            return false;
        }

        bool InMixedCourt()
        {
            var room = Director()?.RoomAt(transform.position);
            return room != null && (room.Id == "arena" || room.Name == "The Mixed Court");
        }

        void FinishPending()
        {
            _pending = null;
        }

        void LandBlow(Transform mark, AdeptAvatar player)
        {
            if (mark == null)
            {
                return;
            }

            if (player != null && mark == player.transform)
            {
                if (AlliedWithAdept)
                {
                    return;
                }

                if (player.IsAirborne)
                {
                    Director()?.Log("The slam passes under you.");
                    return;
                }

                var host = StatusHost.On(player);
                if (host != null && host.Fends(Essence.Physical))
                {
                    Director()?.Log($"{host.FendingName(Essence.Physical)} takes the blow.");
                    return;
                }

                var who = _lock != null ? _lock.DisplayName : string.Empty;
                Director()?.KillPlayer(DeathCause.Plain(string.IsNullOrEmpty(who)
                    ? "A blow finds you."
                    : $"{who}'s blow finds you."));
                return;
            }

            var encounter = mark.GetComponent<EncounterLock>();
            if (encounter != null && !encounter.Resolved)
            {
                Director()?.Log($"{_lock.DisplayName} turns on {encounter.DisplayName}.");
                Director()?.TurnLock(encounter);
            }
        }

        void Follow(AdeptAvatar player)
        {
            CancelWindup();
            if (player == null)
            {
                ShowMindChip();
                return;
            }

            var toPlayer = (Vector2)(player.transform.position - transform.position);
            var distance = toPlayer.magnitude;
            if (distance > 1.7f)
            {
                Face(toPlayer);
                StepToward(player.transform.position);
            }
            else if (distance < 1.05f && toPlayer.sqrMagnitude > 0.01f)
            {
                Face(-toPlayer);
                StepToward(transform.position - (Vector3)toPlayer.normalized);
            }

            ShowMindChip();
        }

        void Flee(AdeptAvatar player)
        {
            CancelWindup();
            if (player == null)
            {
                ShowMindChip();
                return;
            }

            var away = (Vector2)(transform.position - player.transform.position);
            if (away.sqrMagnitude < 0.01f)
            {
                away = Vector2.right;
            }

            Face(away);
            StepToward(transform.position + (Vector3)away.normalized, _walk + 0.8f);
            ShowCast("flees…");
        }

        void Wander()
        {
            CancelWindup();
            if (Time.time >= _wanderUntil || _wanderDir.sqrMagnitude < 0.01f)
            {
                _wanderUntil = Time.time + Random.Range(0.7f, 1.6f);
                _wanderDir = Random.insideUnitCircle.normalized;
                if (_wanderDir.sqrMagnitude < 0.01f)
                {
                    _wanderDir = Vector2.left;
                }
            }

            Face(_wanderDir);
            if (!StepToward(transform.position + (Vector3)_wanderDir))
            {
                _wanderUntil = 0f;
            }

            ShowMindChip();
        }

        public void PlaceAt(Vector3 world)
        {
            transform.position = world;
            _idleOrigin = world;
        }

        bool StepToward(Vector3 world, float speed = 0f)
        {
            if (_status != null && _status.BlocksMove)
            {
                return false;
            }

            var delta = (Vector2)(world - transform.position);
            if (delta.sqrMagnitude < 0.0004f)
            {
                return true;
            }

            var pace = speed > 0f ? speed : _walk;
            pace *= WorldWork.TerrainWalkScale(_grid, transform.position, false, _status);
            var step = delta.normalized * pace * Time.deltaTime;
            var next = (Vector2)transform.position + step;
            if (Blocked(next))
            {
                var slideX = new Vector2(((Vector2)transform.position).x + step.x, transform.position.y);
                if (Mathf.Abs(delta.x) > 0.01f && !Blocked(slideX))
                {
                    transform.position = slideX;
                    _idleOrigin = transform.position;
                    return true;
                }

                var slideY = new Vector2(transform.position.x, ((Vector2)transform.position).y + step.y);
                if (Mathf.Abs(delta.y) > 0.01f && !Blocked(slideY))
                {
                    transform.position = slideY;
                    _idleOrigin = transform.position;
                    return true;
                }

                return false;
            }

            transform.position = next;
            _idleOrigin = transform.position;
            return true;
        }

        bool Blocked(Vector2 point)
        {
            if (_grid == null)
            {
                return false;
            }

            var tile = _grid.TileAtWorld(point);
            return WorldWork.BlocksCell(WorldWork.CoordOf(point), tile);
        }

        void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Facing = direction.normalized;
            if (_sprite != null && Mathf.Abs(Facing.x) > 0.12f)
            {
                _sprite.flipX = Facing.x > 0f;
            }
        }

        void SyncPassage()
        {
            if (_hit == null)
            {
                return;
            }

            _hit.isTrigger = _baseTrigger || (_status != null && _status.YieldsPassage);
        }

        void CancelWindup()
        {
            _casting = false;
            _windup = 0f;
            transform.localScale = _restScale;
            ClearCastChip();
        }

        ShotAllegiance ShotOf()
        {
            if (_status == null)
            {
                return ShotAllegiance.Hostile;
            }

            if (_status.Has(StatusId.Charmed))
            {
                return ShotAllegiance.Allied;
            }

            if (_status.Has(StatusId.Raging) || _status.Has(StatusId.Confused))
            {
                return ShotAllegiance.Wild;
            }

            return ShotAllegiance.Hostile;
        }

        string MindVerb()
        {
            var ranged = Kind == CombatKind.Wizard || Kind == CombatKind.Archer
                || (_active != null && CombatBook.IsRanged(_active.Strike));
            if (_status == null)
            {
                return ranged ? "casting…" : "slam…";
            }

            if (_status.Has(StatusId.Raging))
            {
                return "raging…";
            }

            if (_status.Has(StatusId.Charmed))
            {
                if (_carried != null)
                {
                    return "brings…";
                }

                return ranged ? "serves…" : "fetches…";
            }

            if (_status.Has(StatusId.Confused))
            {
                return "confused…";
            }

            return ranged ? "casting…" : "slam…";
        }

        void ShowMindChip()
        {
            var mind = _status != null ? _status.MindAilment : StatusId.None;
            if (mind == StatusId.None)
            {
                ClearCastChip();
                return;
            }

            ShowCast(StatusSpec.Of(mind).Name);
        }

        void PlayMotion(bool attacking, float fps)
        {
            if (_anim == null)
            {
                return;
            }

            if (attacking)
            {
                if (_attackFrames != null && _attackFrames.Length > 0)
                {
                    var clip = string.IsNullOrEmpty(_attackClip) ? "attack" : _attackClip;
                    if (_anim.Clip != clip)
                    {
                        _anim.Play(_attackFrames, fps, true, clip);
                    }

                    return;
                }

                var named = !string.IsNullOrEmpty(_attackClip) ? _attackClip : DefaultAttackClip();
                if (!string.IsNullOrEmpty(named))
                {
                    _anim.Play(named, fps);
                }

                return;
            }

            if (_idleFrames != null && _idleFrames.Length > 0)
            {
                var clip = string.IsNullOrEmpty(_idleClip) ? "idle" : _idleClip;
                if (_anim.Clip != clip)
                {
                    _anim.Play(_idleFrames, fps, true, clip);
                }

                return;
            }

            _anim.Play(!string.IsNullOrEmpty(_idleClip) ? _idleClip : IdleClip(), fps);
        }

        string DefaultAttackClip()
        {
            switch (Kind)
            {
                case CombatKind.Golem:
                    return "fire-golem-slam";
                case CombatKind.Wizard:
                case CombatKind.Archer:
                    return "warden-cast";
                default:
                    return IdleClip();
            }
        }

        string IdleClip()
        {
            if (!string.IsNullOrEmpty(_idleClip))
            {
                return _idleClip;
            }

            if (Kind == CombatKind.Golem)
            {
                return gameObject.name.ToLowerInvariant().Contains("stone") ? "stone-man" : "fire-golem";
            }

            if (Kind == CombatKind.Wizard || Kind == CombatKind.Archer)
            {
                return "warden";
            }

            return gameObject.name.ToLowerInvariant().Contains("stone") ? "stone-man" : "ash-mite";
        }

        void DropCarried()
        {
            if (_carried != null && !_carried.Collected)
            {
                _carried.Drop();
            }

            _carried = null;
        }

        void OnDisable()
        {
            DropCarried();
        }

        SanctumDirector Director()
        {
            return _director != null ? _director : _director = FindFirstObjectByType<SanctumDirector>();
        }

        void ShowCast(string text)
        {
            if (_castChip != null)
            {
                _castChip.text = text;
            }
        }

        void ShowRunes()
        {
            if (_castRecipe == null || _castRecipe.Length == 0)
            {
                CastSign.Hide(transform);
                return;
            }

            CastSign.Show(transform, _castRecipe, new Vector3(0f, 1.95f, 0f));
        }

        void ClearCastChip()
        {
            if (_castChip != null)
            {
                _castChip.text = string.Empty;
            }

            CastSign.Hide(transform);
        }
    }
}
