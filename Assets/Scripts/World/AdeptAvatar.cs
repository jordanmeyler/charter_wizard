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
        Color _baseColor = Color.white;
        Vector3 _restScale = Vector3.one;

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

        public void ClearWork()
        {
            _airborneUntil = 0f;
            _stillUntil = 0f;
            WorldHeld = false;
        }

        void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _restScale = transform.localScale;
            if (_sprite != null)
            {
                _baseColor = _sprite.color;
            }
        }

        void OnDisable()
        {
            WorldHeld = false;
        }

        void Update()
        {
            WorldHeld = Time.time < _stillUntil;
            var bob = WorldHeld ? 1f : 1f + Mathf.Sin(Time.time * 2.4f) * 0.018f;
            transform.localScale = new Vector3(_restScale.x, _restScale.y * bob, _restScale.z);
            if (_sprite == null)
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
