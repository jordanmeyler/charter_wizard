using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// An enemy flame-pillar. The floor takes the standing hunger first;
    /// a breath later the column rises and strikes whoever still stands there.
    /// The adept's own pillars rise at once.
    /// </summary>
    public sealed class EnemyPillar : MonoBehaviour
    {
        public const float TelegraphSeconds = 1.6f;

        WorldGrid _grid;
        WorldTile _tile;
        Vector3 _origin;
        Vector3 _target;
        SpellId _spell = SpellId.FlamePillar;
        CombatActor _source;
        ShotAllegiance _allegiance = ShotAllegiance.Hostile;
        float _left;
        SpriteRenderer _glow;
        GameObject _linger;
        bool _done;

        RuneId[] _recipe = System.Array.Empty<RuneId>();

        Vector3 _spanFrom;
        Vector3 _spanTo;

        public static void Cast(
            WorldGrid grid,
            Vector3 origin,
            Vector3 target,
            SpellId spell,
            CombatActor source,
            ShotAllegiance allegiance,
            RuneId[] recipe = null)
        {
            Cast(grid, origin, target, spell, source, allegiance, recipe, Vector2.zero);
        }

        public static void Cast(
            WorldGrid grid,
            Vector3 origin,
            Vector3 target,
            SpellId spell,
            CombatActor source,
            ShotAllegiance allegiance,
            RuneId[] recipe,
            Vector2 facing)
        {
            if (spell == SpellId.None)
            {
                spell = SpellId.FlamePillar;
            }

            var host = new GameObject("EnemyPillar");
            var strike = host.AddComponent<EnemyPillar>();
            strike.Begin(grid, origin, target, spell, source, allegiance, recipe, facing);
        }

        void Begin(
            WorldGrid grid,
            Vector3 origin,
            Vector3 target,
            SpellId spell,
            CombatActor source,
            ShotAllegiance allegiance,
            RuneId[] recipe,
            Vector2 facing)
        {
            _grid = grid;
            _origin = origin;
            _target = target;
            _spell = spell;
            _source = source;
            _allegiance = allegiance;
            _recipe = recipe ?? System.Array.Empty<RuneId>();
            _left = TelegraphSeconds;
            if (WorldWork.NeedsSpan(spell))
            {
                CombatBook.WallSpan(origin, target, facing, out _spanFrom, out _spanTo);
                _target = (_spanFrom + _spanTo) * 0.5f;
            }
            else
            {
                _spanFrom = target;
                _spanTo = target;
            }

            var element = CombatBook.ElementOf(spell, _recipe);
            _tile = grid != null ? grid.TileAtWorld(_target) : null;
            if (_tile != null)
            {
                _target = _tile.transform.position;
                if (!WorldWork.NeedsSpan(spell))
                {
                    _spanFrom = _target;
                    _spanTo = _target;
                }

                _tile.BeginTelegraph(WorldWork.MaterialFor(element, spell));
            }

            transform.position = _target;
            var look = ElementLook.For(element, spell);
            _glow = gameObject.AddComponent<SpriteRenderer>();
            _glow.sprite = SpriteFactory.Glow(look.Glow);
            _glow.color = look.Glow;
            _glow.sortingOrder = 8;
            _linger = ElementFx.Linger(transform, look, 0.95f, new Vector3(0f, 0.08f, 0f));
            ElementFx.Burst(_target, look, SpellShape.Remote, 0.75f);
        }

        void Update()
        {
            if (_done)
            {
                return;
            }

            if (AdeptAvatar.WorldHeld)
            {
                return;
            }

            _left -= Time.deltaTime;
            if (_glow != null)
            {
                var pulse = 0.45f + Mathf.Abs(Mathf.Sin(Time.time * 8f)) * 0.5f;
                var color = _glow.color;
                color.a = pulse;
                _glow.color = color;
                transform.localScale = Vector3.one * (0.85f + pulse * 0.35f);
            }

            if (_left > 0f)
            {
                return;
            }

            Erupt();
        }

        void Erupt()
        {
            _done = true;
            _tile?.EndTelegraph();
            if (_linger != null)
            {
                Destroy(_linger);
                _linger = null;
            }

            var element = CombatBook.ElementOf(_spell, _recipe);
            SpellFx.Play(_origin, _target, element, SpellShape.Pillar, string.Empty, null, 1f, _spell);
            var adeptHeld = StrikeOccupants();
            if (!adeptHeld && _grid != null)
            {
                WorldWork.Apply(_grid, _spell, element, _origin, _spanFrom, _spanTo, SpellShape.Pillar);
            }

            Destroy(gameObject);
        }

        bool StrikeOccupants()
        {
            if (WorldWork.NeedsSpan(_spell) || StrikeLaw.Of(_spell).Power <= 0)
            {
                return false;
            }

            var director = FindFirstObjectByType<SanctumDirector>();
            var tile = WorldWork.CoordOf(_target);
            var adeptHeld = false;
            var player = AdeptAvatar.Find();
            if (player != null && WorldWork.CoordOf(player.transform.position) == tile)
            {
                adeptHeld = StrikeAdept(player, director);
            }

            var found = FindObjectsByType<EncounterLock>(FindObjectsSortMode.None);
            for (var i = 0; i < found.Length; i++)
            {
                var encounter = found[i];
                if (encounter == null || encounter.Resolved)
                {
                    continue;
                }

                if (_source != null && encounter.gameObject == _source.gameObject)
                {
                    continue;
                }

                if (WorldWork.CoordOf(encounter.WorldPosition) != tile)
                {
                    continue;
                }

                director?.Log($"{CasterName()} turns hunger on {encounter.DisplayName}.");
                director?.TurnLock(encounter);
            }

            return adeptHeld;
        }

        bool StrikeAdept(AdeptAvatar player, SanctumDirector director)
        {
            if (_allegiance == ShotAllegiance.Allied)
            {
                return true;
            }

            if (player.IsAirborne)
            {
                director?.Log("The column rises under you.");
                return false;
            }

            var host = StatusHost.On(player);
            var incoming = ElementalLaw.Of(_spell);
            if (incoming == Essence.None)
            {
                incoming = Essence.Fire;
            }

            if (host != null && host.Fends(incoming))
            {
                director?.Log($"The column stands, and breaks on the {host.FendingName(incoming)}.");
                return true;
            }

            director?.KillPlayer(DeathCause.OfSpell(_spell,
                _spell == SpellId.FlamePillar || _spell == SpellId.FirePillar
                    ? "A column of hunger finds you."
                    : "A column finds you."));
            return false;
        }

        string CasterName()
        {
            if (_source == null)
            {
                return "Hunger";
            }

            var encounter = _source.GetComponent<EncounterLock>();
            return encounter != null ? encounter.DisplayName : "The adept";
        }

        void OnDestroy()
        {
            if (!_done)
            {
                _tile?.EndTelegraph();
            }
        }
    }
}
