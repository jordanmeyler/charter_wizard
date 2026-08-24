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
        [Tooltip("What the cell is made of. Play bakes this into the live grid.")]
        public MaterialId material = MaterialId.Stone;
        [Tooltip("Floor, wall, pit, door, or bridge.")]
        public TileKind kind = TileKind.Floor;
        [Tooltip("Fire, miasma, or fog on this cell.")]
        public TileAura aura;
        [Tooltip("Ice / fire / lightning / vine over the walk tile.")]
        public TileCover cover;

        public bool HasOverlay => aura != TileAura.None || cover != TileCover.None;

        public string CoverId()
        {
            return cover == TileCover.None ? null : cover.ToString().ToLowerInvariant();
        }

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
