using UnityEngine;

namespace RuneMagic
{
    public static class WorldLabel
    {
        public static TextMesh Attach(Transform parent, string text, Vector3 offset, Color color, int order = 12)
        {
            var font = BuiltinFont.Get();
            if (font == null)
            {
                return null;
            }

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = offset;

            var mesh = labelObject.AddComponent<TextMesh>();
            mesh.font = font;
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.fontSize = 28;
            mesh.characterSize = 0.07f;
            mesh.color = color;

            var renderer = labelObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                if (font.material != null)
                {
                    renderer.sharedMaterial = font.material;
                }

                renderer.sortingOrder = order;
            }

            return mesh;
        }
    }
}
