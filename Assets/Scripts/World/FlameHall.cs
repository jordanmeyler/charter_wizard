using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// The mouth of a kindled hall. Looking names the ward sentence;
    /// walking close speaks it once so the first fire is a lesson.
    /// </summary>
    public sealed class FlameHall : MonoBehaviour, ILookable
    {
        bool _told;
        bool _wired;

        public Vector3 WorldPosition => transform.position;
        public float LookRadius => 1.15f;
        public bool CanLook => true;

        public string LookText => GlyphView.Speak(
            "a hall of hunger. Water ward is Water · Salt · Sulphur — wear it and walk. Yield thrown also puts the flame out, but there is no water here to throw.",
            "a hall of hunger. Yield given a body, then the mind holds it on you. The three marks write that sentence. Wear it and walk. Yield thrown also forgets the flame — but yield has no vessel here.");

        public static FlameHall Spawn(Vector3 position)
        {
            var host = new GameObject("FlameHall");
            host.transform.position = position;
            var hall = host.AddComponent<FlameHall>();
            hall.Bind();
            return hall;
        }

        public void BindFromAuthoring()
        {
            Bind();
        }

        void Bind()
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            var renderer = AuthoringUtil.GetOrAdd<SpriteRenderer>(gameObject);
            renderer.sprite = SpriteFactory.Named("plaque");
            renderer.sortingOrder = 2;
            renderer.color = new Color(1f, 0.55f, 0.22f, 0.95f);
            WorldLabel.Attach(transform,
                GlyphView.Speak("Water · Salt · Sulphur", "Wear yield. The marks write the ward."),
                new Vector3(0f, 0.62f, 0f),
                new Color(1f, 0.72f, 0.38f), 12);
            Lookables.Register(this);
        }

        void Update()
        {
            if (_told)
            {
                return;
            }

            var player = AdeptAvatar.Find();
            if (player == null || Vector2.Distance(player.transform.position, transform.position) > 2.4f)
            {
                return;
            }

            _told = true;
            FindFirstObjectByType<SanctumDirector>()?.Log(GlyphView.Speak(
                "Hunger holds the walk. Water · Salt · Sulphur is a water ward — wear it and the hall will not take you. Douse also works, if you can throw yield.",
                "Hunger holds the walk. The three marks are a ward: yield given a body, then the mind holds it on you. Wear that and walk. Yield thrown also forgets the flame."));
        }

        void OnDisable()
        {
            Lookables.Unregister(this);
        }
    }
}
