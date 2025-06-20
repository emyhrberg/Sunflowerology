using ScienceJam.Content.Tiles;
using ScienceJam.Content.Tiles.SandSnow;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ScienceJam.Content.Items.Placeable
{
    public class SandSnowCompound : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;

            ItemID.Sets.SandgunAmmoProjectileData[Type] = new(ModContent.ProjectileType<Projectiles.SandSnowCompoundBallGunProjectile>(), 10);
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SandSnowCompoundTile>());
            Item.width = 12;
            Item.height = 12;
            Item.ammo = AmmoID.Sand;
            Item.notAmmo = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.SandBlock)
                .AddIngredient(ItemID.SnowBlock)
                .AddTile(TileID.WorkBenches)
                .Register();
        }
    }
}