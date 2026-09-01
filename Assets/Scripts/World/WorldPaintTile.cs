using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// A palette tile you paint in the Scene view. Kind is the walk
    /// family — Floor only if you stamped Floor or used a Floor brush.
    /// A look with Kind = None is never a floor, on any layer.
    /// Extra Floor / Tiles children merge; each Floor stamp still counts.
    /// Cover is the overlay: look, gameplay, and what the cell answers.
    /// Older Aura stamps still map onto Cover.
    /// </summary>
    [CreateAssetMenu(menuName = "Rune Magic/Map Tile", fileName = "MapTile")]
    public sealed class WorldPaintTile : Tile
    {
        [Tooltip("What the cell is made of. Play bakes this into the live grid.")]
        public MaterialId material = MaterialId.Stone;
        [Tooltip("Walk family. Floor is walkable ground. None is look only — not a floor, on any layer.")]
        public TileKind kind = TileKind.Floor;

        public bool StampsWalk => kind != TileKind.None;

        public bool StampsFloor => kind == TileKind.Floor;
        [Tooltip("Legacy veil stamp. Fire aura is a kindled hall. Prefer Cover for the Fire mark.")]
        public TileAura aura;
        [Tooltip("Ice / fire / miasma / fog over the walk tile. Look, work, and the same catalog mark as an inscription. Fire cover only marks hunger so the weave speaks Fire — it does not kindle a hall.")]
        public TileCover cover;
        [Tooltip("On Environment Details, this cell blocks walking. Drag-stamp a cluster of tables or statues.")]
        public bool blocks;
        [Tooltip("Cover tint. 0 means automatic: miasma and fog are see-through.")]
        [Range(0f, 1f)]
        public float opacity;

        public bool HasOverlay => ResolvedCover() != TileCover.None || material == MaterialId.Miasma;

        public TileCover ResolvedCover()
        {
            if (cover != TileCover.None)
            {
                return cover;
            }

            return CoverFromAura(aura);
        }

        public TileAura ResolvedAura()
        {
            if (aura != TileAura.None)
            {
                return aura;
            }

            return AuraFromCover(ResolvedCover() != TileCover.None ? ResolvedCover() : CoverFromMaterial(material));
        }

        public float ResolvedOpacity()
        {
            if (opacity > 0.001f)
            {
                return Mathf.Clamp01(opacity);
            }

            var shown = ResolvedCover();
            if (shown == TileCover.None && material == MaterialId.Miasma)
            {
                shown = TileCover.Miasma;
            }

            if (shown == TileCover.Miasma || shown == TileCover.Fog)
            {
                return 0.42f;
            }

            return 1f;
        }

        public string CoverId()
        {
            var shown = ResolvedCover();
            return shown == TileCover.None ? null : shown.ToString().ToLowerInvariant();
        }

        public static TileCover CoverFromAura(TileAura aura)
        {
            switch (aura)
            {
                case TileAura.Miasma:
                    return TileCover.Miasma;
                case TileAura.Fog:
                    return TileCover.Fog;
                case TileAura.Fire:
                    return TileCover.Fire;
                default:
                    return TileCover.None;
            }
        }

        /// <summary>
        /// Veils map back to an aura. Fire cover is a mark, not a
        /// kindled hall — only an explicit Fire aura kindles.
        /// </summary>
        public static TileAura AuraFromCover(TileCover cover)
        {
            switch (cover)
            {
                case TileCover.Miasma:
                    return TileAura.Miasma;
                case TileCover.Fog:
                    return TileAura.Fog;
                default:
                    return TileAura.None;
            }
        }

        public static TileCover CoverFromMaterial(MaterialId material)
        {
            switch (material)
            {
                case MaterialId.Miasma:
                    return TileCover.Miasma;
                case MaterialId.Cloud:
                case MaterialId.Steam:
                    return TileCover.Fog;
                case MaterialId.Ice:
                case MaterialId.Snow:
                case MaterialId.Glacier:
                    return TileCover.Ice;
                case MaterialId.Water:
                case MaterialId.Rain:
                    return TileCover.Water;
                case MaterialId.Mud:
                    return TileCover.Mud;
                case MaterialId.Ember:
                case MaterialId.Hearth:
                case MaterialId.Lava:
                    return TileCover.Fire;
                case MaterialId.Plant:
                case MaterialId.Grove:
                    return TileCover.Vine;
                case MaterialId.Vein:
                    return TileCover.Lightning;
                default:
                    return TileCover.None;
            }
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
                case TileKind.None:
                    return sprite != null ? sprite : TileAtlas.Get("floor-stone");
                default:
                    return TileAtlas.Floor(material, x, y) ?? TileAtlas.Get("floor-stone");
            }
        }
    }
}
