using UnityEngine;

namespace RuneMagic
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMotor2D : MonoBehaviour
    {
        public float moveSpeed = 5.2f;

        Rigidbody2D _body;
        SanctumDirector _director;

        void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
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

            if (_director != null && _director.Mode != PlayMode.Exploring)
            {
                return;
            }

            var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            _body.MovePosition(_body.position + input * moveSpeed * Time.fixedDeltaTime);
        }
    }
}
