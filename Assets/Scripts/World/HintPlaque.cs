using UnityEngine;

namespace RuneMagic
{
    public sealed class HintPlaque : MonoBehaviour
    {
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
            var renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Plaque();
            renderer.sortingOrder = 2;
            WorldLabel.Attach(transform, text, new Vector3(0f, 0.55f, 0f),
                new Color(0.92f, 0.86f, 0.72f), 12);
        }
    }
}
