using System.Linq;
using Sunflowerology.Content.Tiles.SunGrass;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Sunflowerology.Common.GlobalTiles
{
    public class SunflowerChanges : GlobalTile
    {
        public override void SetStaticDefaults()
        {
            TileObjectData sunflowerObjectData = TileObjectData.GetTileData(TileID.Sunflower, 0);
            sunflowerObjectData.AnchorValidTiles = sunflowerObjectData.AnchorValidTiles.Append(ModContent.TileType<SunGrassTile>()).ToArray();
        }

        public override void Unload()
        {
            TileObjectData sunflowerObjectData = TileObjectData.GetTileData(TileID.Sunflower, 0);
            sunflowerObjectData.AnchorValidTiles = sunflowerObjectData.AnchorValidTiles.Except([ModContent.TileType<SunGrassTile>()]).ToArray();
        }
    }
}