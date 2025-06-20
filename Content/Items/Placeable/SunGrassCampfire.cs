using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CotlimsCoolMod.Content.Items.Placeable
{
	public class SunGrassCampfire : ModItem
	{
		public override void SetDefaults() {
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunGrassCampfire>(), 0);
		}

		public override void AddRecipes() {
			CreateRecipe()
				.AddRecipeGroup(RecipeGroupID.Wood, 10)
				.AddIngredient<SunGrassTorch>(5)
				.Register();
		}
	}
}
