using UnityEngine;

namespace RuneMagic
{
    public sealed class FollowCamera2D : MonoBehaviour
    {
        public Transform Target;
        public float damp = 8f;
        public float lookAhead = 0.85f;
        public bool pixelSnap = true;
        public float pixelsPerUnit = 16f;
        bool _snapped;
        Vector3 _look;
        Transform _bound;
        PlayerMotor2D _motor;

        void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            if (_bound != Target)
            {
                _bound = Target;
                _motor = Target.GetComponent<PlayerMotor2D>();
            }

            var ahead = _motor != null && _motor.Moving
                ? (Vector3)(_motor.Facing * lookAhead)
                : Vector3.zero;
            _look = Vector3.Lerp(_look, ahead, 1f - Mathf.Exp(-4f * Time.deltaTime));
            // Follow the interpolated transform, not Rigidbody2D.position.
            // The body only steps at the physics tick, which made the
            // dungeon hitch even though the adept was sliding smoothly.
            var desired = Target.position + _look;
            desired.z = -10f;
            if (!_snapped)
            {
                transform.position = Snap(desired);
                _snapped = true;
                return;
            }

            // Lerp in world space. Rounding every frame after the lerp
            // stair-steps the camera down a pixel when the target sits
            // between snap points (or the rigidbody interpolates).
            var next = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-damp * Time.deltaTime));
            next.z = -10f;
            if ((next - desired).sqrMagnitude < 0.0004f)
            {
                next = Snap(desired);
            }

            transform.position = next;
        }

        Vector3 Snap(Vector3 point)
        {
            if (!pixelSnap || pixelsPerUnit <= 0f)
            {
                return point;
            }

            point.x = Mathf.Round(point.x * pixelsPerUnit) / pixelsPerUnit;
            point.y = Mathf.Round(point.y * pixelsPerUnit) / pixelsPerUnit;
            return point;
        }
    }
}
