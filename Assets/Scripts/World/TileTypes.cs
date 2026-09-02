namespace RuneMagic
{
    public enum TileKind
    {
        Floor,
        Wall,
        Pit,
        Bridge,
        Door,
        /// <summary>
        /// Look only. Play will not treat this cell as walkable floor.
        /// </summary>
        None
    }

    /// <summary>
    /// Atmosphere stamped when a painted cell is baked. Not the walk family.
    /// Fire is a kindled hall. Cover-Fire is the hunger mark and does
    /// not set this.
    /// </summary>
    public enum TileAura
    {
        None = 0,
        Fire,
        Miasma,
        Fog
    }

    /// <summary>
    /// Overlay on a walk tile. Ice / fire / lightning are never the floor itself.
    /// Fog stays before later covers so older Cover values stay stable.
    /// </summary>
    public enum TileCover
    {
        None = 0,
        Ice,
        Fire,
        Lightning,
        Water,
        Vine,
        Miasma,
        Cracks,
        Seal,
        Fog,
        Mud,
        Ash,
        /// <summary>
        /// Liquid poison on the walk. Contact only; yield washes it.
        /// Miasma is the airborne cloud.
        /// </summary>
        Poison,
        /// <summary>
        /// What withholding leaves of a vegetable body. Speaks Death.
        /// </summary>
        Wither
    }

    /// <summary>
    /// How a conjured body stands. Architectural tiles stay None.
    /// A pillar is one column. A wall is masonry. A span is a walkable fill.
    /// </summary>
    public enum RaisedForm
    {
        None,
        Wall,
        Pillar,
        Span
    }

    public readonly struct TileDef
    {
        public TileDef(TileKind kind, MaterialId material)
        {
            Kind = kind;
            Material = material;
        }

        public TileDef(TileKind kind, TileSubstance substance)
            : this(kind, MaterialCatalog.FromLegacy(substance))
        {
        }

        public TileDef(TileKind kind, RuneId element)
            : this(kind, MaterialCatalog.FromElement(element))
        {
        }

        public TileKind Kind { get; }
        public MaterialId Material { get; }
        public TileSubstance Substance => MaterialCatalog.ToLegacy(Material);
        public WorldMaterial WorldMaterial => MaterialCatalog.Of(Material);
        public RuneId Element => WorldMaterial.Primary;
        public System.Collections.Generic.IReadOnlyList<RuneId> Emission => WorldMaterial.Signature;

        public bool BlocksMovement => Kind == TileKind.Wall || Kind == TileKind.Door;
        public bool IsHazard =>
            Kind == TileKind.Pit || (Kind == TileKind.Floor && Material == MaterialId.Water);
        public bool TearsTapestry => Kind == TileKind.Pit || WorldMaterial.TearsTheWeave;

        public string DisplayName => MaterialCatalog.DisplayName(Kind, Material);
    }
}
