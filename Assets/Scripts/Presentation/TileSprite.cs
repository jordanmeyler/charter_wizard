using System.Collections.Generic;
using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Pack tiles are sliced with Tight meshes and 1px extrude. Dirt
    /// sits next to brown cave wall on the cavern sheet, so Play was
    /// sampling that wall as a hairline on the tile. Rebuild the
    /// sprite as a full rect with no extrude.
    /// </summary>
    public static class TileSprite
    {
        static readonly Dictionary<int, Sprite> SolidOf = new();

        public static Sprite Solid(Sprite source)
        {
            if (source == null)
            {
                return null;
            }

            var id = source.GetInstanceID();
            if (SolidOf.TryGetValue(id, out var cached) && cached != null)
            {
                return cached;
            }

            var texture = source.texture;
            if (texture == null)
            {
                return source;
            }

            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            var rect = source.rect;
            var pivot = new Vector2(
                rect.width > 0.01f ? source.pivot.x / rect.width : 0.5f,
                rect.height > 0.01f ? source.pivot.y / rect.height : 0.5f);
            var ppu = source.pixelsPerUnit > 0f ? source.pixelsPerUnit : 16f;
            var solid = Sprite.Create(texture, rect, pivot, ppu, 0, SpriteMeshType.FullRect);
            solid.name = source.name;
            SolidOf[id] = solid;
            return solid;
        }

        public static void Audit(System.Collections.Generic.List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (Solid(null) != null)
            {
                broken.Add("A missing tile sprite must stay missing");
            }
        }
    }
}
