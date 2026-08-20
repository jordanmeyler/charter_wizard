using UnityEngine;

namespace RuneMagic
{
    public static class RunePalette
    {
        public static Color Of(RuneId id)
        {
            switch (id)
            {
                case RuneId.Fire: return new Color(0.92f, 0.38f, 0.16f);
                case RuneId.Air: return new Color(0.72f, 0.88f, 0.95f);
                case RuneId.Earth: return new Color(0.55f, 0.38f, 0.22f);
                case RuneId.Water: return new Color(0.22f, 0.48f, 0.86f);
                case RuneId.Spark: return new Color(0.98f, 0.86f, 0.28f);
                case RuneId.Cloud: return new Color(0.78f, 0.82f, 0.9f);
                case RuneId.Mud: return new Color(0.4f, 0.32f, 0.2f);
                case RuneId.Lava: return new Color(0.85f, 0.28f, 0.1f);
                case RuneId.Steam: return new Color(0.82f, 0.86f, 0.88f);
                case RuneId.Dust: return new Color(0.7f, 0.62f, 0.42f);
                case RuneId.Plant: return new Color(0.38f, 0.62f, 0.28f);
                case RuneId.Salt: return new Color(0.93f, 0.93f, 0.9f);
                case RuneId.Mercury: return new Color(0.55f, 0.78f, 0.62f);
                case RuneId.Sulphur: return new Color(0.93f, 0.78f, 0.22f);
                case RuneId.Aether: return new Color(0.78f, 0.62f, 0.95f);
                default: return new Color(0.7f, 0.7f, 0.75f);
            }
        }
    }
}
