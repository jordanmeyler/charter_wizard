using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    public enum LevelTileSource
    {
        Tilemap = 0,
        NamedMap,
        RoomShell,
        SceneGrid
    }

    /// <summary>
    /// Drop this in the scene to control how Play builds the floor.
    /// Default is the painted Tilemap. Named JSON maps are leftover.
    /// Scene objects (items, locks, plaques) are always picked up.
    /// </summary>
    public sealed class LevelAuthoring : MonoBehaviour
    {
        [Header("Tiles")]
        public LevelTileSource tiles = LevelTileSource.Tilemap;
        public string mapId;
        public bool includeJsonProps;
        [Tooltip("Walk tiles — floor, wall, pit, door. A Floor or Tiles child works.")]
        public Tilemap tilemap;
        [Tooltip("Optional Walls Tilemap. Merged into the walk grid on Play.")]
        public Tilemap walls;
        [Tooltip("Cover and aura only. A Cover or Coverings child works.")]
        public Tilemap overlays;
        [Tooltip("Environment Details — plants, rugs, furniture. Own material and optional collision. Burns to an ash pile; fire can run onto a flammable floor.")]
        public Tilemap decor;

        [Header("Spawn")]
        public Transform spawnPoint;

        [Header("Room shell")]
        public string roomName = "Authored Room";
        public int roomWidth = 13;
        public int roomHeight = 11;
        public MaterialId wall = MaterialId.Stone;
        public MaterialId floor = MaterialId.Stone;

        void OnDrawGizmos()
        {
            var origin = spawnPoint != null ? spawnPoint.position : transform.position;
            Gizmos.color = new Color(0.72f, 0.55f, 1f, 0.85f);
            Gizmos.DrawWireSphere(origin, 0.28f);
            if (tiles != LevelTileSource.RoomShell)
            {
                return;
            }

            var w = Mathf.Max(3, roomWidth);
            var h = Mathf.Max(3, roomHeight);
            Gizmos.color = new Color(0.85f, 0.72f, 0.35f, 0.35f);
            var center = new Vector3(w * 0.5f, h * 0.5f, 0f);
            Gizmos.DrawWireCube(center, new Vector3(w, h, 0.1f));
        }
    }
}
