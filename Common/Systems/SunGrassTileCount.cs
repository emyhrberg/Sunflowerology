using ScienceJam.Content.Tiles;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace ScienceJam.Common.Systems
{
    public class SunGrassTileCount : ModSystem
    {
        private static int sunGrassTileCount = 0;
        private static float sunGrassTileInfluence = 0f;

        public override void TileCountsAvailable(ReadOnlySpan<int> tileCounts)
        {
            sunGrassTileCount = tileCounts[ModContent.TileType<SunGrassTile>()];
        }

        public static float SunGrassTileInfluence => sunGrassTileInfluence;

        public static void UpdateInfluence()
        {
            if (!Main.gameMenu)
            {
                sunGrassTileInfluence = MathHelper.Lerp(sunGrassTileInfluence, Math.Clamp(sunGrassTileCount, 0f, 40f) / 40f, 0.05f);
            }
            else
            {
                sunGrassTileInfluence = MathHelper.Lerp(sunGrassTileInfluence, 0f, 0.05f);
            }
        }

        public override void PostUpdateEverything()
        {
            UpdateInfluence();
        }
    }
}
