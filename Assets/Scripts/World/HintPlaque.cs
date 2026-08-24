using UnityEngine;

namespace RuneMagic
{
    public sealed class HintPlaque : MonoBehaviour, ILookable
    {
        [Header("Authoring")]
        [SerializeField] string text;
        [SerializeField] string spriteId = "plaque";
        [SerializeField] Sprite portrait;

        string _text;
        bool _wired;

        public Vector3 WorldPosition => transform.position;
        public float LookRadius => 0.7f;
        public bool CanLook => true;
        public string LookText => Sight.OfPlaque(_text);

        public static HintPlaque Spawn(Vector3 position, string text)
        {
            var plaque = new GameObject("Plaque");
            plaque.transform.position = position;
            var view = plaque.AddComponent<HintPlaque>();
            view.Bind(text);
            return view;
        }

        public void Bind(string text)
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            _text = string.IsNullOrEmpty(text) ? this.text : text;
            AuthoringUtil.ApplyLook(gameObject, 2, string.IsNullOrEmpty(spriteId) ? "plaque" : spriteId, portrait, null, 1f);
            WorldLabel.Attach(transform, _text, new Vector3(0f, 0.55f, 0f),
                new Color(0.92f, 0.86f, 0.72f), 12);
            Lookables.Register(this);
        }

        public void EnsureBound()
        {
            Bind(_text ?? text);
        }

        void OnDisable()
        {
            Lookables.Unregister(this);
        }
    }
}
