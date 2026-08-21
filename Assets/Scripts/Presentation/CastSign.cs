using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Rune marks above a caster's head while a sentence is being written.
    /// </summary>
    public static class CastSign
    {
        const string ChildName = "CastSign";

        public static void Show(Transform parent, RuneId[] runes, Vector3 offset)
        {
            if (parent == null)
            {
                return;
            }

            var host = Host(parent, create: true);
            if (host == null)
            {
                return;
            }

            host.localPosition = offset;
            ClearChildren(host);
            if (runes == null || runes.Length == 0)
            {
                host.gameObject.SetActive(false);
                return;
            }

            host.gameObject.SetActive(true);
            var count = 0;
            for (var i = 0; i < runes.Length; i++)
            {
                if (runes[i] != RuneId.None)
                {
                    count++;
                }
            }

            if (count == 0)
            {
                host.gameObject.SetActive(false);
                return;
            }

            var stride = 0.42f;
            var start = -(count - 1) * stride * 0.5f;
            var placed = 0;
            for (var i = 0; i < runes.Length; i++)
            {
                if (runes[i] == RuneId.None)
                {
                    continue;
                }

                var mark = new GameObject("Rune");
                mark.transform.SetParent(host, false);
                mark.transform.localPosition = new Vector3(start + placed * stride, 0f, 0f);
                mark.transform.localScale = Vector3.one * 0.38f;
                var renderer = mark.AddComponent<SpriteRenderer>();
                renderer.sprite = RuneMark.AsSprite(runes[i], RunePalette.MarkInk(runes[i]));
                renderer.sortingOrder = 22;
                placed++;
            }
        }

        public static void Hide(Transform parent)
        {
            var host = Host(parent, create: false);
            if (host != null)
            {
                host.gameObject.SetActive(false);
            }
        }

        static Transform Host(Transform parent, bool create)
        {
            var existing = parent.Find(ChildName);
            if (existing != null)
            {
                return existing;
            }

            if (!create)
            {
                return null;
            }

            var host = new GameObject(ChildName);
            host.transform.SetParent(parent, false);
            return host.transform;
        }

        static void ClearChildren(Transform host)
        {
            for (var i = host.childCount - 1; i >= 0; i--)
            {
                Object.Destroy(host.GetChild(i).gameObject);
            }
        }
    }
}
