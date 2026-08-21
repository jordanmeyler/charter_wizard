using UnityEngine;

namespace RuneMagic
{
    /// <summary>
    /// Coloured work-signs. The strokes themselves live in
    /// <see cref="RuneMark"/> — the old triangles, bars, and circles.
    /// This only tints them so Play and Develop share one set of marks.
    /// </summary>
    public static class RuneGlyphs
    {
        public static Sprite Build(RuneId id)
        {
            return RuneMark.AsSprite(id, RunePalette.MarkInk(id));
        }
    }
}
