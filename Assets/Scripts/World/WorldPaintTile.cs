using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// A palette tile you paint in the Scene view. On Floor / Walls, kind
    /// + material become the walk cell. On Environment Details they
    /// become a detail stamp — material, optional collision, and a
    /// plant can burn off and leave the stone.
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
        [Tooltip("On Environment Details, this cell blocks walking. Drag-stamp a cluster of tables or statues.")]
        public bool blocks;
        [Tooltip("Cover / aura tint. 0 means automatic: miasma and fog are see-through.")]
        [Range(0f, 1f)]
        public float opacity;

        public bool HasOverlay => aura != TileAura.None || cover != TileCover.None;

        public float ResolvedOpacity()
        {
            if (opacity > 0.001f)
            {
                return Mathf.Clamp01(opacity);
            }

            if (aura == TileAura.Miasma || aura == TileAura.Fog)
            {
                return 0.42f;
            }

            return 1f;
        }

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
            var alpha = ResolvedOpacity();
            tileData.color = new Color(1f, 1f, 1f, alpha);
            color = tileData.color;
            tileData.flags = TileFlags.LockColor | TileFlags.LockTransform;
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
