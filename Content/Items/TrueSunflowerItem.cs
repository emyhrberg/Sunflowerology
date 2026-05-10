using Sunflowerology.Content.Tiles;
using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ID;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Items
{
    internal class TrueSunflowerItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<TrueSunflowerTile>());
            Item.width = 30;
            Item.height = 28;
            Item.scale = 2f;
            Item.maxStack = 1;
            Item.value = Terraria.Item.buyPrice(platinum: 5);
            Item.consumable = false;

        }

        public override void AddRecipes()
        {
            var recipe = CreateRecipe();
            for (int i = 0; i < 10; i++)
            {
                recipe.AddIngredient(NatureData.TypeOfSunflowerToItemId[(TypeOfSunflower)i], 99);
            }
            recipe.AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}
