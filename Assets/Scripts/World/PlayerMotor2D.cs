using UnityEngine;

namespace RuneMagic
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        public float moveSpeed = 5.2f;
        public Vector2 Facing { get; private set; } = Vector2.right;
        public bool Moving { get; private set; }

        Rigidbody2D _body;
        SpriteRenderer _sprite;
        SanctumDirector _director;
        AdeptAvatar _adept;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _body.gravityScale = 0f;
            _body.freezeRotation = true;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        void Update()
        {
            BindDirector();
            if (_director != null && !_director.CanMove)
            {
                Halt();
            }
        }

        void FixedUpdate()
        {
            BindDirector();
            if (_director != null && !_director.CanMove)
            {
                Halt();
                return;
            }

            if (_adept == null)
            {
                _adept = GetComponent<AdeptAvatar>();
            }

            // Hop is a scripted leap. Flight is walkable airborne and
            // must not go through IsHopping, or WASD freezes for the
            // whole Flight clock.
            if (_adept != null && _adept.IsHopping)
            {
                Halt();
                return;
            }

            var host = StatusHost.On(this);
            if (host != null && host.BlocksMove)
            {
                Halt();
                return;
            }

            var input = ReadMove();
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            Moving = input.sqrMagnitude > 0.01f;
            if (Moving)
            {
                Facing = input;
                if (_sprite != null && Mathf.Abs(Facing.x) > 0.15f)
                {
                    _sprite.flipX = Facing.x < 0f;
                }
            }

            _body.MovePosition(_body.position + input * moveSpeed * Time.fixedDeltaTime);
        }

        static Vector2 ReadMove()
        {
            var keyboard = Vector2.zero;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                keyboard.x -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                keyboard.x += 1f;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                keyboard.y -= 1f;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                keyboard.y += 1f;
            }

            if (keyboard.sqrMagnitude > 0f)
            {
                return keyboard.sqrMagnitude > 1f ? keyboard.normalized : keyboard;
            }

            // Joystick / leftover analog. A noisy stick or an OS axis
            // that sits slightly off zero walks the adept forever.
            var analog = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            const float dead = 0.25f;
            if (Mathf.Abs(analog.x) < dead)
            {
                analog.x = 0f;
            }

            if (Mathf.Abs(analog.y) < dead)
            {
                analog.y = 0f;
            }

            return analog.sqrMagnitude > 1f ? analog.normalized : analog;
        }

        void BindDirector()
        {
            if (_director == null)
            {
                _director = FindFirstObjectByType<SanctumDirector>();
            }
        }

        void Halt()
        {
            Moving = false;
            if (_body != null)
            {
                _body.linearVelocity = Vector2.zero;
            }
        }
    }
}
