using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    /// <summary>
    /// A palette tile you paint in the Scene view. Kind is the walk
    /// family — Floor only if you stamped Floor or used a Floor brush.
    /// A look with Kind = None is never a floor, on any layer.
    /// Extra Floor / Tiles children merge; each Floor stamp still counts.
    /// Cover is the overlay: look and what the cell answers.
    /// Stamps do not start a reaction. Older Aura stamps still map onto Cover.
    /// </summary>
    [CreateAssetMenu(menuName = "Rune Magic/Map Tile", fileName = "MapTile")]
    public sealed class WorldPaintTile : Tile
    {
        [Tooltip("What the cell is made of. Play bakes this into the live grid.")]
        public MaterialId material = MaterialId.Stone;
        [Tooltip("Walk family. Floor is walkable ground. None is look only — not a floor, on any layer.")]
        public TileKind kind = TileKind.Floor;

        /// <summary>
        /// Cover-* / Aura-* brushes. They sit on the walk tile.
        /// Pack art on those brushes is a sheen, not a new floor.
        /// </summary>
        public bool IsOverlayBrush => IsOverlayBrushOf(name, cover, aura);

        /// <summary>
        /// Floor / wall / pit / door / bridge. Overlay brushes never
        /// stamp walk, even when an older Cover-Ice asset still says
        /// Kind = Floor.
        /// </summary>
        public bool StampsWalk => StampsWalkOf(name, kind, cover, aura);

        public bool StampsFloor => StampsWalk && kind == TileKind.Floor;
        [Tooltip("Legacy veil stamp. Fire aura is a kindled hall. Prefer Cover for the Fire mark.")]
        public TileAura aura;
        [Tooltip("Ice / fire / miasma / poison / fog / ash over the walk tile. Covers are the live layer: they can catch, melt, and interact once a spell starts work. Floor and wall stamps stay at rest. Fire cover is tinder when hunger is live. Aura-Fire still kindles a hall. Poison is a liquid slick; miasma is the airborne cloud.")]
        public TileCover cover;
        [Tooltip("On Environment Details, this cell blocks walking. Drag-stamp a cluster of tables or statues.")]
        public bool blocks;
        [Tooltip("Cover tint. 0 means automatic: miasma and fog are see-through.")]
        [Range(0f, 1f)]
        public float opacity;

        public bool HasOverlay =>
            ResolvedCover() != TileCover.None
            || material == MaterialId.Miasma
            || material == MaterialId.Acid;

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

            return AutomaticOpacity(shown);
        }

        /// <summary>
        /// Covers sit on the walk tile. Fire, lightning, and veils
        /// are a sheen. Ice is thicker. A full 1 hides the floor.
        /// </summary>
        public static float AutomaticOpacity(TileCover cover)
        {
            switch (cover)
            {
                case TileCover.Miasma:
                case TileCover.Fog:
                case TileCover.Fire:
                case TileCover.Lightning:
                    return 0.42f;
                case TileCover.Ice:
                case TileCover.Water:
                    return 0.7f;
                case TileCover.None:
                    return 1f;
                default:
                    return 0.55f;
            }
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
                case MaterialId.Acid:
                    return TileCover.Poison;
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
                case MaterialId.Hearth:
                case MaterialId.Lava:
                    return TileCover.Fire;
                case MaterialId.Vein:
                    return TileCover.Lightning;
                case MaterialId.Ash:
                    return TileCover.Ash;
                default:
                    return TileCover.None;
            }
        }

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            base.GetTileData(position, tilemap, ref tileData);
            if (tileData.sprite == null && !KeepsPaintedLook)
            {
                tileData.sprite = PreviewSprite(position.x, position.y);
            }

            tileData.colliderType = ColliderType.None;
            var alpha = ResolvedOpacity();
            tileData.color = new Color(1f, 1f, 1f, alpha);
            color = tileData.color;
            tileData.flags = TileFlags.LockColor | TileFlags.LockTransform;
        }

        /// <summary>
        /// Floor and wall stamps add qualities over the tile you
        /// painted. Pack art on Floor-Stone / Floor-Plant is only a
        /// chip preview — it must not replace the tileset already on
        /// that cell. Fire is walk matter at rest, like stone.
        /// Cover brushes keep that same tileset and draw on top.
        /// </summary>
        public bool IsQualityStamp =>
            IsOverlayBrush || IsQualityStampOf(kind, material);

        /// <summary>
        /// A later stamp must not throw away the tileset already on
        /// the cell. Floor, wall, and overlay brushes all keep it.
        /// </summary>
        public bool KeepsExistingLook => IsQualityStamp || IsOverlayBrush;

        public static bool IsOverlayBrushOf(string name, TileCover cover, TileAura aura)
        {
            if (cover == TileCover.None && aura == TileAura.None)
            {
                return false;
            }

            return IsOverlayBrushName(name);
        }

        public static bool IsOverlayBrushName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            return name.StartsWith("Cover-", System.StringComparison.OrdinalIgnoreCase)
                || name.StartsWith("Aura-", System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool StampsWalkOf(string name, TileKind kind, TileCover cover, TileAura aura)
        {
            return kind != TileKind.None && !IsOverlayBrushOf(name, cover, aura);
        }

        public static bool IsQualityStampOf(TileKind kind, MaterialId material)
        {
            if (kind == TileKind.Floor || kind == TileKind.Wall)
            {
                return true;
            }

            switch (material)
            {
                case MaterialId.Water:
                case MaterialId.Rain:
                case MaterialId.Plant:
                case MaterialId.Grove:
                case MaterialId.Moss:
                case MaterialId.Timber:
                case MaterialId.Fire:
                    return true;
                default:
                    return false;
            }
        }

        public static void Audit(System.Collections.Generic.List<string> broken)
        {
            if (broken == null)
            {
                return;
            }

            if (!IsQualityStampOf(TileKind.Floor, MaterialId.Stone)
                || !IsQualityStampOf(TileKind.Floor, MaterialId.Dirt)
                || !IsQualityStampOf(TileKind.Floor, MaterialId.Ice)
                || !IsQualityStampOf(TileKind.Floor, MaterialId.Plant)
                || !IsQualityStampOf(TileKind.Floor, MaterialId.Fire)
                || !IsQualityStampOf(TileKind.Wall, MaterialId.Stone)
                || !IsQualityStampOf(TileKind.Wall, MaterialId.Ice)
                || !IsQualityStampOf(TileKind.Wall, MaterialId.Fire))
            {
                broken.Add("Floor and wall stamps must keep the tileset sprite they sit on");
            }

            if (AutomaticOpacity(TileCover.Fire) >= 0.95f
                || AutomaticOpacity(TileCover.Lightning) >= 0.95f
                || AutomaticOpacity(TileCover.Miasma) >= 0.95f
                || AutomaticOpacity(TileCover.None) < 0.99f)
            {
                broken.Add("Fire and veil covers must be a sheen; they must not hide the walk tile");
            }

            if (!IsOverlayBrushOf("Cover-Ice", TileCover.Ice, TileAura.None)
                || !IsOverlayBrushOf("Aura-Fire", TileCover.Fire, TileAura.Fire)
                || !IsOverlayBrushName("Cover-Water")
                || IsOverlayBrushOf("Floor-Ice", TileCover.Ice, TileAura.None)
                || IsOverlayBrushOf("Floor_Stone_Ice", TileCover.Ice, TileAura.None))
            {
                broken.Add("Cover-* / Aura-* are overlay brushes; Floor-* and authored tiles are not");
            }

            if (StampsWalkOf("Cover-Ice", TileKind.Floor, TileCover.Ice, TileAura.None)
                || StampsWalkOf("Aura-Fire", TileKind.Floor, TileCover.Fire, TileAura.Fire)
                || !StampsWalkOf("Floor-Stone", TileKind.Floor, TileCover.None, TileAura.None)
                || !StampsWalkOf("Floor_Stone_Ice", TileKind.Floor, TileCover.Ice, TileAura.None)
                || !StampsWalkOf("Wall-Ice", TileKind.Wall, TileCover.None, TileAura.None))
            {
                broken.Add("Cover brushes must not stamp walk; Floor / Wall and authored Floor+Cover still do");
            }

            if (CoverFromMaterial(MaterialId.Ice) != TileCover.Ice
                || CoverFromMaterial(MaterialId.Stone) != TileCover.None
                || CoverFromMaterial(MaterialId.Fire) != TileCover.None
                || CoverFromMaterial(MaterialId.Ember) != TileCover.None
                || CoverFromMaterial(MaterialId.Plant) != TileCover.None)
            {
                broken.Add("CoverFromMaterial is Cover-layer inference — Fire, ember, and Plant walk stamps are not covers");
            }
        }

        /// <summary>
        /// Stamps add qualities over the tile you painted. They must
        /// not invent a new floor graphic when the painted sprite is
        /// missing.
        /// </summary>
        public bool KeepsPaintedLook => true;

        public Sprite PreviewSprite(int x = 0, int y = 0)
        {
            if (sprite != null)
            {
                return sprite;
            }

            if (KeepsPaintedLook)
            {
                return null;
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
