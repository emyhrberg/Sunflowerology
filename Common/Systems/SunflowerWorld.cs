using System;
using ScienceJam.Content.Tiles.SunflowerTree;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ScienceJam.Common.Systems
{
    public class SunflowerWorld : ModSystem
    {
        public override void PostWorldGen()
        {
            int radius = 300;   // tiles left/right of spawn to scan (~600-tile strip)
            int odds = 4;     // 1-in-4 saplings become sunflower (25 %)

            int lx = Math.Max(10, Main.spawnTileX - radius);
            int rx = Math.Min(Main.maxTilesX - 10, Main.spawnTileX + radius);
            int surface = (int)Main.worldSurface;

            for (int x = lx; x <= rx; x++)
                for (int y = 0; y <= surface; y++)
                {
                    Tile t = Main.tile[x, y];
                    if (t.TileType == TileID.Saplings && t.TileFrameY == 0 &&
                        WorldGen.genRand.NextBool(odds))
                    {
                        WorldGen.KillTile(x, y, noItem: true);
                        WorldGen.PlaceObject(x, y, ModContent.TileType<SunflowerSapling>());
                    }
                }
        }

    }
}
