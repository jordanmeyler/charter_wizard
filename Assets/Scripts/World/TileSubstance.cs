using System.Collections.Generic;

namespace RuneMagic
{
    /// <summary>
    /// Legacy names for the first sanctum slice. New maps should stamp
    /// <see cref="MaterialId"/> from <see cref="MaterialCatalog"/>.
    /// </summary>
    public enum TileSubstance
    {
        Stone,
        Ash,
        Timber,
        Hearth,
        Ember,
        Damp,
        Vein,
        Scoured,
        Moss,
        Metal,
        SaltCrust,
        Void
    }

    public static class TileSubstances
    {
        public static WorldMaterial Get(TileSubstance substance) =>
            MaterialCatalog.Of(MaterialCatalog.FromLegacy(substance));

        public static RuneId Primary(TileSubstance substance) => Get(substance).Primary;

        public static IReadOnlyList<RuneId> EmissionOf(TileSubstance substance) =>
            Get(substance).Signature;

        public static TileSubstance FromElement(RuneId element) =>
            MaterialCatalog.ToLegacy(MaterialCatalog.FromElement(element));

        public static string DisplayName(TileKind kind, TileSubstance substance) =>
            MaterialCatalog.DisplayName(kind, MaterialCatalog.FromLegacy(substance));
    }
}
