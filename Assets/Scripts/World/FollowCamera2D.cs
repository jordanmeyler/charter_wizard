using UnityEngine;

namespace RuneMagic
{
    public sealed class FollowCamera2D : MonoBehaviour
    {
        public Transform Target;
        public float damp = 8f;
        public float lookAhead = 0.85f;
        public bool pixelSnap = true;
        public float pixelsPerUnit = 32f;
        bool _snapped;
        Vector3 _look;

        void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            var motor = Target.GetComponent<PlayerMotor2D>();
            var ahead = motor != null ? (Vector3)(motor.Facing * lookAhead) : Vector3.zero;
            _look = Vector3.Lerp(_look, ahead, 1f - Mathf.Exp(-4f * Time.deltaTime));
            var desired = Target.position + _look;
            desired.z = -10f;
            if (!_snapped)
            {
                transform.position = desired;
                _snapped = true;
                return;
            }

            var next = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-damp * Time.deltaTime));
            next.z = -10f;
            if (pixelSnap && pixelsPerUnit > 0f)
            {
                next.x = Mathf.Round(next.x * pixelsPerUnit) / pixelsPerUnit;
                next.y = Mathf.Round(next.y * pixelsPerUnit) / pixelsPerUnit;
            }

            transform.position = next;
        }
    }
}
