using Sunflowerology.Content.Tiles;
using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Items
{
    internal class ZenithflowerItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<ZenithflowerTile>());
            Item.width = 30;
            Item.height = 28;
            Item.scale = 2f;
            Item.maxStack = 9999;
            Item.value = 1000;
        }

        public override void AddRecipes()
        {
            var recipe = CreateRecipe();
            for (int i = 0; i < 10; i++)
            {
                recipe.AddIngredient(NatureData.TypeOfSunflowerToItemId[(TypeOfSunflower)i], 1);
            }
            recipe.AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
