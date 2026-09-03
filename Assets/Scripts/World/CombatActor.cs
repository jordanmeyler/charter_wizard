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
    /// A lock that can strike back. Golems slam. Wizards write a sentence
    /// and send it. A beginner fireball takes two seconds.
    /// In the Mixed Court a fire mage answers a wall with a stood flame.
    /// Mind ailments rewrite who they hunt, and whether they stand still.
    /// </summary>
    public sealed class CombatActor : MonoBehaviour
    {
        static readonly RuneId[] FlamePillarRecipe =
        {
            RuneId.Fire,
            RuneId.Salt,
            RuneId.Earth
        };

        public CombatKind Kind { get; private set; }
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
        bool _pillarReply;
        ICarryable _carried;
        Sprite[] _idleFrames;
        Sprite[] _attackFrames;
        string _idleClip = string.Empty;
        string _attackClip = string.Empty;
        const float FetchReach = 1.15f;

        public void Bind(CombatKind kind, float castSeconds, WorldGrid grid, RuneId[] castRecipe = null)
        {
            Kind = kind;
            CastSeconds = Mathf.Max(0.35f, castSeconds);
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
            _castRecipe = RecipeOf(kind, castRecipe);
            _castChip = WorldLabel.Attach(transform, "", new Vector3(0f, 1.62f, 0f),
                new Color(1f, 0.72f, 0.28f), 14);
            if (_castChip != null)
            {
                _castChip.characterSize = 0.05f;
            }

            if (kind == CombatKind.Golem || kind == CombatKind.None)
            {
                _reach = 1.25f;
                if (kind == CombatKind.Golem || castSeconds <= 2.01f)
                {
                    CastSeconds = Mathf.Max(0.7f, castSeconds <= 2.01f ? 0.85f : castSeconds);
                }
            }

            if (kind == CombatKind.Archer)
            {
                CastSeconds = Mathf.Max(0.45f, castSeconds <= 2.01f ? 1.15f : castSeconds);
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
            _reach = 1.25f + _oilBulk * 0.55f;
            transform.localScale = _restScale;
        }

        float SightNow()
        {
            if (_grid != null && WorldPhysics.AuraAt(_grid, transform.position, out var kind) && kind == VeilKind.Darkness)
            {
                return 0.35f;
            }

            return _sight;
        }

        ProjectileKind ShotKind()
        {
            if (WritesWood())
            {
                return ProjectileKind.Wood;
            }

            if (Kind == CombatKind.Archer)
            {
                return ProjectileKind.Wood;
            }

            return ProjectileKind.Fireball;
        }

        bool WritesWood()
        {
            if (_castRecipe == null)
            {
                return false;
            }

            var plant = false;
            var mercury = false;
            var life = false;
            var death = false;
            var fire = false;
            for (var i = 0; i < _castRecipe.Length; i++)
            {
                switch (_castRecipe[i])
                {
                    case RuneId.Plant:
                        plant = true;
                        break;
                    case RuneId.Mercury:
                        mercury = true;
                        break;
                    case RuneId.Vita:
                        life = true;
                        break;
                    case RuneId.Mors:
                        death = true;
                        break;
                    case RuneId.Fire:
                        fire = true;
                        break;
                }
            }

            return plant && mercury && !life && !death && !fire;
        }

        static RuneId[] RecipeOf(CombatKind kind, RuneId[] written)
        {
            if (written != null && written.Length > 0)
            {
                return written;
            }

            switch (kind)
            {
                case CombatKind.Wizard:
                    return new[] { RuneId.Fire, RuneId.Mercury };
                case CombatKind.Archer:
                    return new[] { RuneId.Plant, RuneId.Salt, RuneId.Mercury };
                default:
                    return System.Array.Empty<RuneId>();
            }
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
                ShowMindChip();
                return;
            }

            var mind = _status != null ? _status.MindAilment : StatusId.None;
            if (mind == StatusId.None && Kind == CombatKind.None)
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

            if (Kind == CombatKind.Wizard || Kind == CombatKind.Archer)
            {
                TickCaster(mark, toMark, distance);
                return;
            }

            TickMelee(mark, player, distance, chase);
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
            if (distance > _reach + 0.15f)
            {
                _windup = 0f;
                transform.localScale = _restScale;
                if (chase)
                {
                    StepToward(mark.position);
                }

                PlayMotion(false, 5f);
                ShowMindChip();
                return;
            }

            _windup += Time.deltaTime;
            ShowCast(MindVerb());
            PlayMotion(true, 6f);
            var wind = Mathf.Clamp01(_windup / CastSeconds);
            transform.localScale = new Vector3(_restScale.x * (1f + wind * 0.12f), _restScale.y * (1f - wind * 0.12f), 1f);
            if (_windup < CastSeconds)
            {
                return;
            }

            _windup = 0f;
            transform.localScale = _restScale;
            PlayMotion(false, 5f);
            ClearCastChip();
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

        void TickCaster(Transform mark, Vector2 toMark, float distance)
        {
            if (!_casting)
            {
                if (distance > SightNow())
                {
                    return;
                }

                _casting = true;
                _windup = 0f;
                _committed = toMark.sqrMagnitude > 0.01f ? toMark.normalized : Facing;
            }

            _windup += Time.deltaTime;
            var left = Mathf.Max(0f, CastSeconds - _windup);
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
            if (_windup < CastSeconds)
            {
                return;
            }

            _casting = false;
            _windup = 0f;
            PlayMotion(false, 4f);
            ClearCastChip();
            if (_pillarReply)
            {
                var aim = mark != null ? mark.position : transform.position + (Vector3)_committed;
                EnemyPillar.Cast(_grid, transform.position, aim, SpellId.FlamePillar, this, ShotOf());
                return;
            }

            var shot = ShotKind();
            var origin = transform.position + (Vector3)(_committed * 0.45f);
            WorldProjectile.Spawn(origin, _committed, shot, _grid, shot == ProjectileKind.Fireball ? 6.4f : 7.4f, this, ShotOf());
        }

        public static void NoticePlayerSpell(SpellId spell, Vector3 origin, Vector3 aim)
        {
            if (spell != SpellId.Wall)
            {
                return;
            }

            var director = Object.FindFirstObjectByType<SanctumDirector>();
            if (director == null)
            {
                return;
            }

            var here = director.RoomAt(origin);
            var there = director.RoomAt(aim);
            var room = IsMixedCourt(here) ? here : IsMixedCourt(there) ? there : null;
            if (room == null)
            {
                return;
            }

            var found = Object.FindObjectsByType<CombatActor>(FindObjectsSortMode.None);
            for (var i = 0; i < found.Length; i++)
            {
                var actor = found[i];
                if (actor == null || actor._lock == null || actor._lock.Resolved)
                {
                    continue;
                }

                if (!room.Contains(actor.transform.position))
                {
                    continue;
                }

                actor.AnswerWall();
            }
        }

        void AnswerWall()
        {
            if (_pillarReply || !WritesFire())
            {
                return;
            }

            _pillarReply = true;
            _castRecipe = FlamePillarRecipe;
            CancelWindup();
            var who = _lock != null ? _lock.DisplayName : "The adept";
            Director()?.Log($"The wall stands. {who} writes hunger and asks it to rest.");
        }

        bool WritesFire()
        {
            if (Kind != CombatKind.Wizard || _castRecipe == null)
            {
                return false;
            }

            for (var i = 0; i < _castRecipe.Length; i++)
            {
                if (_castRecipe[i] == RuneId.Spark)
                {
                    return false;
                }

                if (_castRecipe[i] == RuneId.Fire)
                {
                    return true;
                }
            }

            return false;
        }

        static bool IsMixedCourt(RoomInfo room)
        {
            return room != null && (room.Id == "arena" || room.Name == "The Mixed Court");
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

            var step = delta.normalized * (speed > 0f ? speed : _walk) * Time.deltaTime;
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
            var ranged = Kind == CombatKind.Wizard || Kind == CombatKind.Archer;
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
