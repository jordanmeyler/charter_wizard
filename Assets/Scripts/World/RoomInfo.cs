using UnityEngine;

namespace RuneMagic
{
    public sealed class RoomInfo
    {
        public RoomInfo(string id, string name, RectInt bounds, Vector3 entrance)
        {
            Id = id;
            Name = name;
            Bounds = bounds;
            Entrance = entrance;
        }

        public string Id { get; }
        public string Name { get; }
        public RectInt Bounds { get; }
        public Vector3 Entrance { get; }
        public WorldTile[] ExitDoors { get; set; }
        public ISpellLock Lock { get; set; }

        public bool Contains(Vector3 world)
        {
            var x = Mathf.FloorToInt(world.x);
            var y = Mathf.FloorToInt(world.y);
            return Bounds.Contains(new Vector2Int(x, y));
        }
    }
}
