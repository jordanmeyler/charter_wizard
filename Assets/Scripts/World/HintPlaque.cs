using UnityEngine;

namespace RuneMagic
{
    public sealed class HintPlaque : MonoBehaviour, ILookable
    {
        string _text;

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

        void Bind(string text)
        {
            _text = text;
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Named("plaque");
            renderer.sortingOrder = 2;
            WorldLabel.Attach(transform, text, new Vector3(0f, 0.55f, 0f),
                new Color(0.92f, 0.86f, 0.72f), 12);
            Lookables.Register(this);
        }

        void OnDisable()
        {
            Lookables.Unregister(this);
        }
    }
}
