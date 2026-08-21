using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Marks the player without relying on the Player tag existing in the project.
    /// The adept’s recipe is mind, body, and soul — always in the weave.
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

        float _airborneUntil;
        float _stillUntil;
        SpriteRenderer _sprite;
        SpriteRenderer _glow;
        SpriteAnim _anim;
        PlayerMotor2D _motor;
        StatusHost _status;
        SanctumDirector _director;
        Color _baseColor = Color.white;
        Vector3 _restScale = Vector3.one;
        bool _casting;

        public bool IsAirborne => Time.time < _airborneUntil;
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

            _airborneUntil = Mathf.Max(_airborneUntil, Time.time + seconds);
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
            _airborneUntil = 0f;
            _stillUntil = 0f;
            _casting = false;
            WorldHeld = false;
        }

        void Awake()
        {
            _restScale = transform.localScale;
        }

        void Start()
        {
            BindView();
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

            if (_anim == null && _sprite != null)
            {
                _anim = SpriteAnim.On(gameObject, _sprite);
                _anim.Play("adept-idle", 5f);
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
        }

        void OnDisable()
        {
            WorldHeld = false;
        }

        void Update()
        {
            BindView();
            WorldHeld = Time.time < _stillUntil;
            var moving = _motor != null && _motor.Moving;
            var aiming = _casting || (_director != null && _director.Mode == PlayMode.Aiming);
            var clip = "adept-idle";
            var fps = 5f;
            if (WorldHeld || aiming)
            {
                clip = "adept-cast";
                fps = 4f;
            }
            else if (IsAirborne)
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

            var bob = moving || IsAirborne || aiming
                ? 1f
                : 1f + Mathf.Sin(Time.time * 2.4f) * 0.018f;
            transform.localScale = new Vector3(_restScale.x, _restScale.y * bob, _restScale.z);
            if (_glow != null)
            {
                var pulse = 0.72f + Mathf.Sin(Time.time * 3.2f) * 0.16f;
                _glow.color = new Color(0.78f, 0.55f, 1f, pulse);
                _glow.transform.localScale = Vector3.one * (0.95f + Mathf.Sin(Time.time * 2.1f) * 0.08f);
            }

            if (_sprite == null)
            {
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
