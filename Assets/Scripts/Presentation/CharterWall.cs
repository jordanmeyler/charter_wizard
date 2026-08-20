using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// World-space Charter wall. Runes stand over the room so the adept
    /// stays visible through a light veil.
    /// </summary>
    public sealed class CharterWall : MonoBehaviour
    {
        readonly List<GameObject> _spawned = new();

        public bool IsShowing { get; private set; }

        public void Show(IReadOnlyList<RuneId> runes, Camera camera)
        {
            Hide();
            if (camera == null || runes == null || runes.Count == 0)
            {
                return;
            }

            IsShowing = true;
            SpawnVeil(camera);
            SpawnCards(runes, camera);
        }

        public void Hide()
        {
            IsShowing = false;
            for (var i = 0; i < _spawned.Count; i++)
            {
                if (_spawned[i] != null)
                {
                    Destroy(_spawned[i]);
                }
            }

            _spawned.Clear();
        }

        void SpawnVeil(Camera camera)
        {
            var veil = new GameObject("CharterVeil");
            veil.transform.SetParent(camera.transform, false);
            veil.transform.localPosition = new Vector3(0f, 0f, 1f);
            var renderer = veil.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.Square(new Color(0.04f, 0.05f, 0.08f), 16);
            renderer.color = new Color(1f, 1f, 1f, 0.38f);
            renderer.sortingOrder = 14;
            FitToCamera(veil.transform, camera, 1.15f);
            _spawned.Add(veil);
        }

        void SpawnCards(IReadOnlyList<RuneId> runes, Camera camera)
        {
            var columns = Mathf.Min(runes.Count, 7);
            var rows = Mathf.CeilToInt(runes.Count / (float)columns);
            const float size = 1.15f;
            const float gap = 0.22f;
            var stride = size + gap;
            var totalWidth = columns * stride - gap;
            var totalHeight = rows * stride - gap;
            var origin = camera.transform.position;
            var top = origin.y + camera.orthographicSize * 0.42f;
            var startX = origin.x - totalWidth * 0.5f + size * 0.5f;
            var startY = top - size * 0.5f;
            if (rows > 1)
            {
                startY = origin.y + (totalHeight * 0.15f);
            }

            for (var i = 0; i < runes.Count; i++)
            {
                var col = i % columns;
                var row = i / columns;
                var position = new Vector3(startX + col * stride, startY - row * stride, 0f);
                var card = CharterRune.Spawn(runes[i], position, size);
                card.transform.SetParent(camera.transform, true);
                _spawned.Add(card);
            }
        }

        static void FitToCamera(Transform target, Camera camera, float pad)
        {
            var height = camera.orthographicSize * 2f * pad;
            var width = height * camera.aspect;
            target.localScale = new Vector3(width, height, 1f);
        }
    }

    public sealed class CharterRune : MonoBehaviour
    {
        public RuneId Rune { get; private set; }

        public static GameObject Spawn(RuneId rune, Vector3 position, float size)
        {
            var card = new GameObject($"Charter_{RuneCatalog.NameOf(rune)}");
            card.transform.position = position;
            var view = card.AddComponent<CharterRune>();
            view.Bind(rune, size);
            return card;
        }

        void Bind(RuneId rune, float size)
        {
            Rune = rune;
            var body = gameObject.AddComponent<SpriteRenderer>();
            body.sprite = SpriteFactory.Circle(RunePalette.Of(rune), 48);
            body.sortingOrder = 16;
            transform.localScale = Vector3.one * size;

            var hit = gameObject.AddComponent<CircleCollider2D>();
            hit.radius = 0.48f;
            hit.isTrigger = true;

            WorldLabel.Attach(transform, RuneCatalog.NameOf(rune), new Vector3(0f, -0.42f, 0f),
                Color.white, 17);
            var glyph = WorldLabel.Attach(transform, RuneCatalog.GlyphOf(rune), Vector3.zero,
                new Color(0.08f, 0.06f, 0.06f), 17);
            glyph.characterSize = 0.11f;
            glyph.fontSize = 36;
        }
    }
}
