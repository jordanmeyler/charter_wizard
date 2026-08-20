using UnityEngine;

namespace CharterWizard
{
    /// <summary>
    /// A floating charter rune. Walk into it to collect it.
    /// </summary>
    public class RunePickup : MonoBehaviour
    {
        public float spinDegreesPerSecond = 90f;
        public float bobHeight = 0.25f;
        public float bobSpeed = 2.2f;

        Vector3 _restPosition;
        GameDirector _director;
        bool _collected;

        public void Bind(GameDirector director)
        {
            _director = director;
            _restPosition = transform.position;
        }

        void Update()
        {
            if (_collected)
            {
                return;
            }

            transform.Rotate(0f, spinDegreesPerSecond * Time.deltaTime, 0f, Space.World);
            var bob = Mathf.Sin(Time.time * bobSpeed + _restPosition.x) * bobHeight;
            transform.position = _restPosition + Vector3.up * bob;
        }

        void OnTriggerEnter(Collider other)
        {
            if (_collected || !other.CompareTag("Player"))
            {
                return;
            }

            _collected = true;
            _director.CollectRune();
            Destroy(gameObject);
        }
    }
}
