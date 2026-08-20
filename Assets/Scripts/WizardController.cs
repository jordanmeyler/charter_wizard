using UnityEngine;

namespace CharterWizard
{
    /// <summary>
    /// WASD / arrow keys to run, Space to hop. Tweak the numbers in the Inspector.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class WizardController : MonoBehaviour
    {
        public float moveSpeed = 7f;
        public float jumpSpeed = 7.5f;
        public float gravity = -22f;
        public float turnSpeed = 12f;

        CharacterController _controller;
        float _verticalVelocity;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            var input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (input.sqrMagnitude > 1f)
            {
                input.Normalize();
            }

            if (input.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(input, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }

            if (_controller.isGrounded)
            {
                _verticalVelocity = -1f;
                if (Input.GetButtonDown("Jump"))
                {
                    _verticalVelocity = jumpSpeed;
                }
            }
            else
            {
                _verticalVelocity += gravity * Time.deltaTime;
            }

            var motion = input * moveSpeed;
            motion.y = _verticalVelocity;
            _controller.Move(motion * Time.deltaTime);
        }
    }
}
