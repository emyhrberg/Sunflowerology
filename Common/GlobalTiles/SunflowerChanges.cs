using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Sunflowerology.Content.Tiles.SunGrass;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace Sunflowerology.Common.GlobalTiles
{
    public class SunflowerChanges : GlobalTile
    {
        private static int[] ogAnchors;
        public override void SetStaticDefaults()
        {
            TileObjectData sunflowerObjectData = TileObjectData.GetTileData(TileID.Sunflower, 0);
            ogAnchors = sunflowerObjectData.AnchorValidTiles.ToArray();
            sunflowerObjectData.AnchorValidTiles = sunflowerObjectData.AnchorValidTiles.Concat(NatureData.TilesToData.Keys.ToArray()).Distinct().ToArray();
        }

        public override void Unload()
        {
            TileObjectData sunflowerObjectData = TileObjectData.GetTileData(TileID.Sunflower, 0);
            sunflowerObjectData.AnchorValidTiles = ogAnchors;
        }
    }
}