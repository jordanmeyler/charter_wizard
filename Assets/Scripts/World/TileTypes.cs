namespace RuneMagic
{
    public enum TileKind
    {
        Floor,
        Wall,
        Pit,
        Bridge,
        Door
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
        public bool IsHazard => Kind == TileKind.Pit;
        public bool TearsTapestry => Kind == TileKind.Pit || WorldMaterial.TearsTheWeave;

        public string DisplayName => MaterialCatalog.DisplayName(Kind, Material);
    }
}
