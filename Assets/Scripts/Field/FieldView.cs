using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// The player's vicinity is whatever the camera can see.
    /// </summary>
    public static class FieldView
    {
        const float Pad = 0.2f;

        public static Rect OnScreen()
        {
            return OnScreen(Camera.main);
        }

        public static Rect OnScreen(Camera camera)
        {
            if (camera == null)
            {
                return new Rect(0f, 0f, 0f, 0f);
            }

            var height = camera.orthographic
                ? camera.orthographicSize * 2f
                : 10.8f;
            var width = height * Mathf.Max(0.5f, camera.aspect);
            var center = camera.transform.position;
            return new Rect(
                center.x - width * 0.5f - Pad,
                center.y - height * 0.5f - Pad,
                width + Pad * 2f,
                height + Pad * 2f);
        }

        public static bool ContainsWorld(Rect view, Vector3 world)
        {
            return view.width > 0f && view.Contains(new Vector2(world.x, world.y));
        }

        public static bool ContainsTile(Rect view, int x, int y)
        {
            if (view.width <= 0f)
            {
                return false;
            }

            var tile = new Rect(x, y, 1f, 1f);
            return view.Overlaps(tile);
        }

        public static int Key(Rect view)
        {
            return (Mathf.FloorToInt(view.xMin) * 73856093)
                ^ (Mathf.FloorToInt(view.yMin) * 19349663)
                ^ (Mathf.FloorToInt(view.xMax) * 83492791)
                ^ (Mathf.FloorToInt(view.yMax) * 39916801);
        }
    }
}
