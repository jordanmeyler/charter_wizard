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
        public TileDef(TileKind kind, RuneId element)
        {
            Kind = kind;
            Element = element;
        }

        public TileKind Kind { get; }
        public RuneId Element { get; }

        public bool BlocksMovement => Kind == TileKind.Wall || Kind == TileKind.Door;
        public bool IsHazard => Kind == TileKind.Pit;

        public string DisplayName
        {
            get
            {
                switch (Kind)
                {
                    case TileKind.Pit:
                        return "Pit (no floor)";
                    case TileKind.Bridge:
                        return "Earth bridge";
                    case TileKind.Door:
                        return Element == RuneId.Plant ? "Timber door" : "Stone door";
                    case TileKind.Wall:
                        return Element == RuneId.Plant ? "Timber wall" : "Stone wall";
                    default:
                        return Element switch
                        {
                            RuneId.Fire => "Fire-warmed stone",
                            RuneId.Plant => "Wood floor",
                            RuneId.Spark => "Spark-veined stone",
                            RuneId.Air => "Air-scoured stone",
                            RuneId.Water => "Damp stone",
                            RuneId.Earth => "Earth stone",
                            _ => "Stone floor"
                        };
                }
            }
        }
    }
}
