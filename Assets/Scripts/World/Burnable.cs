using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A stood thing that can take hunger and become ash.
    /// Creatures use StatusHost meters; timber and plant use this.
    /// </summary>
    public sealed class Burnable : MonoBehaviour
    {
        public float Capacity { get; private set; }
        public float Remaining { get; private set; }
        public bool Alight { get; private set; }
        public bool Spent { get; private set; }

        System.Action<string> _onAsh;
        SpriteRenderer _sprite;
        Color _baseColor = Color.white;
        WorldGrid _grid;

        public static Burnable On(Component other)
        {
            return other != null ? other.GetComponent<Burnable>() : null;
        }

        public static bool CanBurn(Component other)
        {
            var burnable = On(other);
            return burnable != null && !burnable.Spent && burnable.Capacity > 0f;
        }

        public void Bind(float seconds, System.Action<string> onAsh)
        {
            Capacity = Mathf.Max(0f, seconds);
            Remaining = Capacity;
            _onAsh = onAsh;
            _sprite = GetComponent<SpriteRenderer>();
            if (_sprite != null)
            {
                _baseColor = _sprite.color;
            }
        }

        public bool Ignite()
        {
            if (Spent || Capacity <= 0f)
            {
                return false;
            }

            if (!Alight)
            {
                Remaining = Capacity;
            }

            Alight = true;
            KindleUnderfoot();
            return true;
        }

        void KindleUnderfoot()
        {
            if (_grid == null)
            {
                _grid = FindFirstObjectByType<WorldGrid>();
            }

            var tile = _grid != null ? _grid.TileAtWorld(transform.position) : null;
            if (tile != null && tile.Fire < 0.12f)
            {
                tile.Ignite(0.4f);
            }
        }

        public void Douse()
        {
            Alight = false;
            if (_sprite != null && !Spent)
            {
                _sprite.color = _baseColor;
            }
        }

        void Update()
        {
            if (Spent || AdeptAvatar.WorldHeld)
            {
                return;
            }

            if (!Alight)
            {
                CatchUnderfoot();
                return;
            }

            Remaining -= Time.deltaTime;
            KindleUnderfoot();
            if (_sprite != null)
            {
                _sprite.color = Color.Lerp(_baseColor, new Color(1f, 0.42f, 0.12f), 0.55f);
            }

            if (Remaining > 0f)
            {
                return;
            }

            Spent = true;
            Alight = false;
            _onAsh?.Invoke("Hunger finishes the timber. Ash is what remains.");
        }

        void CatchUnderfoot()
        {
            if (_grid == null)
            {
                _grid = FindFirstObjectByType<WorldGrid>();
            }

            var tile = _grid != null ? _grid.TileAtWorld(transform.position) : null;
            if (tile != null && tile.IsBurning)
            {
                Ignite();
            }
        }
    }
}
