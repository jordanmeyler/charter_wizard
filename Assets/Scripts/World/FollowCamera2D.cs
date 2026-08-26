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

        void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            var motor = Target.GetComponent<PlayerMotor2D>();
            var ahead = motor != null && motor.Moving
                ? (Vector3)(motor.Facing * lookAhead)
                : Vector3.zero;
            _look = Vector3.Lerp(_look, ahead, 1f - Mathf.Exp(-4f * Time.deltaTime));
            var desired = TargetPoint() + _look;
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

        Vector3 TargetPoint()
        {
            var body = Target.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                return new Vector3(body.position.x, body.position.y, 0f);
            }

            return Target.position;
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
