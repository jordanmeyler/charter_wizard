using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Marks the player without relying on the Player tag existing in the project.
    /// The adept’s recipe is mind, body, and soul — always in the weave.
    /// Motion is a Unity Animator on Hero_22 (Idle / Walk / Cast / Hop).
    /// </summary>
    public sealed class AdeptAvatar : MonoBehaviour
    {
        public const string DisplayTitle = "Adept";
        public const RuneId Wash = RuneId.Mercury;
        public static readonly RuneId[] Formula =
        {
            RuneId.Sulphur,
            RuneId.Salt,
            RuneId.Mercury
        };

        public const float FlameKillSeconds = VitalLaw.AdeptBurnSeconds;
        public const string AnimatorResource = "Animations/Adept";
        public const string MovingParam = "Moving";
        public const string CastingParam = "Casting";
        public const string AirborneParam = "Airborne";

        float _hopUntil;
        float _flightUntil;
        float _floatUntil;
        float _stillUntil;
        float _flameStand;
        SpriteRenderer _sprite;
        SpriteRenderer _glow;
        Animator _animator;
        SpriteAnim _anim;
        PlayerMotor2D _motor;
        StatusHost _status;
        SanctumDirector _director;
        Color _baseColor = Color.white;
        Vector3 _restScale = Vector3.one;
        bool _casting;
        bool _usesAnimator;

        /// <summary>
        /// Scripted hop: the motor yields so CarryCaster can lerp the body.
        /// Flight must not use this — walking is the point of Flight.
        /// </summary>
        public bool IsHopping => Time.time < _hopUntil;
        public bool IsFlying => Time.time < _flightUntil;
        public bool IsFloating => Time.time < _floatUntil;
        public bool Flies
        {
            get
            {
                if (IsFlying || IsFloating)
                {
                    return true;
                }

                if (_status == null)
                {
                    _status = GetComponent<StatusHost>();
                }

                return _status != null && _status.Flies;
            }
        }
        /// <summary>
        /// Float hangs without logos. WASD is a crawl. Flight and
        /// cloud-form still walk.
        /// </summary>
        public bool Drifts
        {
            get
            {
                if (!IsFloating || IsFlying)
                {
                    return false;
                }

                if (_status == null)
                {
                    _status = GetComponent<StatusHost>();
                }

                return _status == null || !_status.Flies;
            }
        }
        public bool IsAirborne => IsHopping || Flies;
        public static bool WorldHeld { get; private set; }

        public static AdeptAvatar Find()
        {
            return FindFirstObjectByType<AdeptAvatar>();
        }

        public static bool IsAdept(Component other)
        {
            return other != null && other.GetComponent<AdeptAvatar>() != null;
        }

        public void KeepAirborne(float seconds)
        {
            if (seconds <= 0f)
            {
                return;
            }

            _hopUntil = Mathf.Max(_hopUntil, Time.time + seconds);
        }

        public void KeepFlying(float seconds)
        {
            if (seconds <= 0f)
            {
                return;
            }

            _flightUntil = Mathf.Max(_flightUntil, Time.time + seconds);
        }

        public void KeepFloating(float seconds)
        {
            if (seconds <= 0f)
            {
                return;
            }

            _floatUntil = Mathf.Max(_floatUntil, Time.time + seconds);
        }

        /// <summary>
        /// Recast the same breath to land. True if the hold now stands.
        /// </summary>
        public bool ToggleFlying(float seconds)
        {
            if (IsFlying)
            {
                _flightUntil = 0f;
                return false;
            }

            KeepFlying(seconds);
            return true;
        }

        /// <summary>
        /// Recast the same breath to land. True if the hold now stands.
        /// </summary>
        public bool ToggleFloating(float seconds)
        {
            if (IsFloating)
            {
                _floatUntil = 0f;
                return false;
            }

            KeepFloating(seconds);
            return true;
        }

        public void HoldWorld(float seconds)
        {
            if (seconds <= 0f)
            {
                return;
            }

            _stillUntil = Mathf.Max(_stillUntil, Time.time + seconds);
        }

        public void SetCasting(bool casting)
        {
            _casting = casting;
        }

        public void ClearWork()
        {
            _hopUntil = 0f;
            _flightUntil = 0f;
            _floatUntil = 0f;
            _stillUntil = 0f;
            _flameStand = 0f;
            _casting = false;
            WorldHeld = false;
        }

        public bool TickFlame(bool inFire, bool warded)
        {
            if (!inFire || warded)
            {
                _flameStand = 0f;
                return false;
            }

            _flameStand += Time.deltaTime;
            return _flameStand >= FlameKillSeconds;
        }

        void Awake()
        {
            _restScale = transform.localScale;
            BindView();
        }

        void Start()
        {
            BindView();
            if (_usesAnimator && _animator != null)
            {
                _animator.Rebind();
                _animator.Update(0f);
            }
        }

        void BindView()
        {
            if (_sprite == null)
            {
                _sprite = GetComponent<SpriteRenderer>();
                if (_sprite != null)
                {
                    _baseColor = _sprite.color;
                }
            }

            if (_motor == null)
            {
                _motor = GetComponent<PlayerMotor2D>();
            }

            if (_status == null)
            {
                _status = GetComponent<StatusHost>();
            }

            if (_director == null)
            {
                _director = FindFirstObjectByType<SanctumDirector>();
            }

            if (_glow == null)
            {
                var child = transform.Find("Glow");
                if (child != null)
                {
                    _glow = child.GetComponent<SpriteRenderer>();
                }
            }

            BindAnimator();
        }

        void BindAnimator()
        {
            if (_usesAnimator && _animator != null)
            {
                return;
            }

            // Mecanim cannot bind SpriteRenderer.sprite until the renderer
            // exists. Binding earlier (Awake, before Bootstrap adds it) is
            // why toggling Write Defaults in Play suddenly made clips show:
            // that toggle rebinds against the live renderer.
            if (_sprite == null)
            {
                return;
            }

            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
            }

            var controller = _animator != null ? _animator.runtimeAnimatorController : null;
            if (controller == null)
            {
                controller = Resources.Load<RuntimeAnimatorController>(AnimatorResource);
            }

            if (controller == null)
            {
                if (_anim == null)
                {
                    _anim = SpriteAnim.On(gameObject, _sprite);
                    _anim.Play("adept-idle", 5f);
                }

                return;
            }

            if (_animator == null)
            {
                _animator = gameObject.AddComponent<Animator>();
            }

            if (_sprite.sprite == null)
            {
                _sprite.sprite = SpriteFactory.Named("adept");
            }

            ApplyController(_animator, controller);
            _animator.Rebind();
            _animator.Update(0f);
            _usesAnimator = true;
            if (_anim != null)
            {
                _anim.enabled = false;
            }
        }

        public static void ApplyController(Animator animator, RuntimeAnimatorController controller)
        {
            if (animator == null || controller == null)
            {
                return;
            }

            animator.runtimeAnimatorController = controller;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.applyRootMotion = false;
        }

        void OnDisable()
        {
            WorldHeld = false;
        }

        void Update()
        {
            BindView();
            WorldHeld = Time.time < _stillUntil;
            var moving = _motor != null && _motor.Moving && (_director == null || _director.CanMove);
            var aiming = _casting
                || (_director != null && (_director.IsCasting || _director.Busy));
            if (_usesAnimator && _animator != null)
            {
                _animator.speed = GameHud.HoldsPlay ? 0f : 1f;
                _animator.SetBool(MovingParam, moving);
                _animator.SetBool(CastingParam, WorldHeld || aiming);
                _animator.SetBool(AirborneParam, IsHopping);
            }
            else
            {
                var clip = "adept-idle";
                var fps = 5f;
                if (WorldHeld || aiming)
                {
                    clip = "adept-cast";
                    fps = 4f;
                }
                else if (IsHopping)
                {
                    clip = "adept-hop";
                    fps = 7f;
                }
                else if (moving)
                {
                    clip = "adept-walk";
                    fps = 8f;
                }

                _anim?.Play(clip, fps);
                var bob = moving || IsHopping || IsFlying || IsFloating || aiming
                    ? 1f
                    : 1f + Mathf.Sin(Time.time * 2.4f) * 0.018f;
                transform.localScale = new Vector3(_restScale.x, _restScale.y * bob, _restScale.z);
            }
        }

        void LateUpdate()
        {
            var hidden = _status != null && _status.IsHidden;
            if (_glow != null)
            {
                if (hidden)
                {
                    _glow.color = new Color(0.78f, 0.55f, 1f, 0f);
                }
                else
                {
                    var pulse = 0.72f + Mathf.Sin(Time.time * 3.2f) * 0.16f;
                    _glow.color = new Color(0.78f, 0.55f, 1f, pulse);
                    _glow.transform.localScale = Vector3.one * (0.95f + Mathf.Sin(Time.time * 2.1f) * 0.08f);
                }
            }

            if (_sprite == null)
            {
                return;
            }

            if (hidden)
            {
                var fade = _baseColor;
                fade.a = 0.18f;
                _sprite.color = fade;
                return;
            }

            if (_status != null && !string.IsNullOrEmpty(_status.Summary()))
            {
                return;
            }

            if (WorldHeld)
            {
                _sprite.color = Color.Lerp(_baseColor, new Color(0.42f, 0.28f, 0.62f, 0.92f), 0.6f);
                return;
            }

            _sprite.color = IsAirborne
                ? Color.Lerp(_baseColor, new Color(0.75f, 0.92f, 1f, 0.92f), 0.55f)
                : _baseColor;
        }
    }
}
