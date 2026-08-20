using UnityEngine;

namespace RuneMagic
{
    public sealed class FollowCamera2D : MonoBehaviour
    {
        public Transform Target;
        public float damp = 8f;

        void LateUpdate()
        {
            if (Target == null)
            {
                return;
            }

            var next = Vector3.Lerp(transform.position, Target.position, damp * Time.deltaTime);
            next.z = -10f;
            transform.position = next;
        }
    }
}
