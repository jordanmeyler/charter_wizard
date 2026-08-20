using UnityEngine;

namespace RuneMagic
{
    public sealed class FollowCamera2D : MonoBehaviour
    {
        public Transform Target;
        public float damp = 8f;
        bool _snapped;

        void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            var desired = Target.position;
            desired.z = -10f;
            if (!_snapped)
            {
                transform.position = desired;
                _snapped = true;
                return;
            }

            var next = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-damp * Time.deltaTime));
            next.z = -10f;
            transform.position = next;
        }
    }
}
