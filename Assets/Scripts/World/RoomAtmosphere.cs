using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Room tint and camera wash so each chamber reads as its own material.
    /// </summary>
    public sealed class RoomAtmosphere : MonoBehaviour
    {
        SanctumDirector _director;
        Camera _camera;
        Color _background = new(0.04f, 0.045f, 0.07f);
        string _roomId;

        public void Bind(SanctumDirector director)
        {
            _director = director;
            _camera = Camera.main;
        }

        void LateUpdate()
        {
            if (_director == null)
            {
                return;
            }

            var room = _director.CurrentRoom;
            var id = room != null ? room.Id : string.Empty;
            if (id != _roomId)
            {
                _roomId = id;
                _background = TintFor(id);
            }

            if (_camera == null)
            {
                _camera = Camera.main;
            }

            var wash = _background;
            var adept = AdeptAvatar.Find();
            if (adept != null && VeilField.Covering(adept.transform.position, out var veil))
            {
                wash = Color.Lerp(_background, VeilField.Wash(veil), 0.72f);
            }

            if (_camera != null)
            {
                _camera.backgroundColor = Color.Lerp(_camera.backgroundColor, wash, 1f - Mathf.Exp(-3.5f * Time.deltaTime));
            }
        }

        public static Color TintFor(string roomId)
        {
            switch (roomId)
            {
                case "ash-court":
                    return new Color(0.08f, 0.04f, 0.035f);
                case "wick-chapel":
                    return new Color(0.07f, 0.05f, 0.03f);
                case "the-drop":
                    return new Color(0.03f, 0.03f, 0.05f);
                case "storm-cell":
                    return new Color(0.04f, 0.05f, 0.09f);
                default:
                    return new Color(0.04f, 0.045f, 0.07f);
            }
        }
    }
}
