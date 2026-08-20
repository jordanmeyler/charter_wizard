using UnityEngine;

namespace RuneMagic
{
    public static class BuiltinFont
    {
        static Font _cached;
        static bool _looked;

        public static Font Get()
        {
            if (_looked)
            {
                return _cached;
            }

            _looked = true;
            var names = new[] { "LegacyRuntime.ttf", "Arial.ttf", "LegacyRuntime.otf" };
            for (var i = 0; i < names.Length; i++)
            {
                try
                {
                    var font = Resources.GetBuiltinResource<Font>(names[i]);
                    if (font != null)
                    {
                        _cached = font;
                        return _cached;
                    }
                }
                catch (System.Exception)
                {
                }
            }

            try
            {
                _cached = Font.CreateDynamicFontFromOSFont(
                    new[] { "Arial", "Helvetica", "DejaVu Sans", "Liberation Sans", "Verdana" },
                    16);
            }
            catch (System.Exception)
            {
                _cached = null;
            }

            return _cached;
        }
    }
}
