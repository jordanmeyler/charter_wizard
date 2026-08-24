using UnityEngine;
using UnityEngine.Tilemaps;

namespace RuneMagic
{
    public enum LevelTileSource
    {
        StartupMap,
        NamedMap,
        RoomShell,
        SceneGrid,
        Tilemap
    }

    /// <summary>
    /// Drop this in the scene to control how Play builds the floor.
    /// Leave it off to keep the JSON startup map and still pick up
    /// any items / locks you placed by hand.
    /// </summary>
    public sealed class LevelAuthoring : MonoBehaviour
    {
        [Header("Tiles")]
        public LevelTileSource tiles = LevelTileSource.StartupMap;
        public string mapId;
        public bool includeJsonProps = true;
        public Tilemap tilemap;

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
