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
        public TileDef(TileKind kind, TileSubstance substance)
        {
            Kind = kind;
            Substance = substance;
        }

        public TileDef(TileKind kind, RuneId element)
            : this(kind, TileSubstances.FromElement(element))
        {
        }

        public TileKind Kind { get; }
        public TileSubstance Substance { get; }
        public RuneId Element => TileSubstances.Primary(Substance);
        public System.Collections.Generic.IReadOnlyList<RuneId> Emission => TileSubstances.EmissionOf(Substance);

        public bool BlocksMovement => Kind == TileKind.Wall || Kind == TileKind.Door;
        public bool IsHazard => Kind == TileKind.Pit;
        public bool TearsTapestry => Kind == TileKind.Pit || Substance == TileSubstance.Void;

        public string DisplayName => TileSubstances.DisplayName(Kind, Substance);
    }
}
