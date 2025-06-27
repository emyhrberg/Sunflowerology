using Terraria;
using Terraria.ModLoader;
using ScienceJam.Content.Tiles.SunflowerTree;

namespace ScienceJam.Common.Systems
{
    public class SpawnSunflowerTree : ModSystem
    {
        public override void PostWorldGen()
        {
            int x = Main.spawnTileX;
            int y = Main.spawnTileY;

            // walk downward until we stand on solid ground
            while (y < Main.maxTilesY - 50 && !WorldGen.SolidTile(x, y))
                y++;

            // place an *invisible* sunflower sapling style-0
            WorldGen.PlaceObject(x, y - 1,
                ModContent.TileType<SunflowerSapling>(),
                mute: true, style: 0);

            // grow it instantly into the full tree
            WorldGen.GrowTree(x, y - 1);

            // sync for multiplayer & map
            NetMessage.SendTileSquare(-1, x, y - 1, 16);
        }
    }
}
