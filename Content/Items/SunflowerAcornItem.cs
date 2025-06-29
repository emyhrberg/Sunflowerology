using Sunflowerology.Content.Tiles.SunflowerTree;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Items
{
    public class SunflowerAcornItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SunflowerSaplingTile>());
            Item.useAnimation = Item.useTime;
            Item.createTile = ModContent.TileType<SunflowerSaplingTile>();
            Item.width = 22;
            Item.height = 22;
        }
    }
}