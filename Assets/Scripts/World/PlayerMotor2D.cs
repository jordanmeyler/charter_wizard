using UnityEngine;

namespace RuneMagic
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        public float moveSpeed = 5.2f;
        public Vector2 Facing { get; private set; } = Vector2.right;

        Rigidbody2D _body;
        SpriteRenderer _sprite;
        SanctumDirector _director;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _sprite = GetComponent<SpriteRenderer>();
            _body.gravityScale = 0f;
            _body.freezeRotation = true;
            _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        void FixedUpdate()
        {
            if (_director == null)
            {
                _director = FindFirstObjectByType<SanctumDirector>();
            }

            if (_director != null && !_director.CanMove)
            {
                return;
            }

            var input = ReadMove();
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            if (input.sqrMagnitude > 0.01f)
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
            var x = 0f;
            var y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                x -= 1f;
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                x += 1f;
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                y -= 1f;
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                y += 1f;
            }

            if (x == 0f && y == 0f)
            {
                return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            }

            return new Vector2(x, y);
        }
    }
}
