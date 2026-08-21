using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// A slow hover so stones, charms, and keys read as pickups.
    /// </summary>
    public sealed class PropBob : MonoBehaviour
    {
        Vector3 _rest;
        float _phase;
        float _height = 0.07f;
        float _spin;

        public static PropBob Attach(Transform host, float height = 0.07f, float spin = 0f)
        {
            if (host == null)
            {
                return null;
            }

            var bob = host.GetComponent<PropBob>();
            if (bob == null)
            {
                bob = host.gameObject.AddComponent<PropBob>();
            }

            bob._rest = host.localPosition;
            bob._height = height;
            bob._spin = spin;
            bob._phase = host.position.x * 0.7f + host.position.y * 0.4f;
            return bob;
        }

        void LateUpdate()
        {
            _phase += Time.deltaTime;
            transform.localPosition = _rest + new Vector3(0f, Mathf.Sin(_phase * 2.1f) * _height, 0f);
            if (_spin > 0.01f)
            {
                transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(_phase * _spin) * 6f);
            }
        }
    }
}
