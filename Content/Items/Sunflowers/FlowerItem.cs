using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Items.Sunflowers
{
    internal abstract class FlowerItem : ModItem
    {
        public abstract TypeOfSunflower TypeOfSunflower { get; }
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SunflowerWithSeedsTile>());
            Item.width = 30;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.value = 1000;
            Item.placeStyle = 3 * (int)TypeOfSunflower;
        }
    }
}
