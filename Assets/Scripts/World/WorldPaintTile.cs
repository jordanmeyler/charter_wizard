using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// A palette tile you paint in the Scene view. Kind + material become
    /// a live <see cref="WorldTile"/> at Play. Assign a sprite or leave it
    /// blank to use the atlas slice for that material.
    /// </summary>
    [CreateAssetMenu(menuName = "Rune Magic/Map Tile", fileName = "MapTile")]
    public sealed class WorldPaintTile : Tile
    {
        public MaterialId material = MaterialId.Stone;
        public TileKind kind = TileKind.Floor;
        [Tooltip("fire, miasma, or fog — applied when the tile is baked.")]
        public string aura;
        [Tooltip("ice, fire, lightning, water, vine — covering, not the walk family.")]
        public string cover;

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            if (tileData.sprite == null)
            {
                tileData.sprite = PreviewSprite(position.x, position.y);
            }

            tileData.colliderType = ColliderType.None;
            tileData.flags = TileFlags.LockTransform;
        }

        public Sprite PreviewSprite(int x = 0, int y = 0)
        {
            if (sprite != null)
            {
                return sprite;
            }

            TileAtlas.Ensure();
            switch (kind)
            {
                case TileKind.Wall:
                    return TileAtlas.Wall(material, x, y) ?? TileAtlas.Get("wall");
                case TileKind.Pit:
                    return TileAtlas.Get("pit");
                case TileKind.Door:
                    return TileAtlas.Door(false, true) ?? TileAtlas.Get("door");
                case TileKind.Bridge:
                    return TileAtlas.Get("bridge") ?? TileAtlas.Floor(material, x, y);
                default:
                    return TileAtlas.Floor(material, x, y) ?? TileAtlas.Get("floor-stone");
            }
        }
    }
}
