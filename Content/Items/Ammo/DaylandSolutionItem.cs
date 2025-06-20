using CotlimsCoolMod.Content.Projectiles;
using CotlimsCoolMod.Content.Tiles;
using CotlimsCoolMod.Content.Walls;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CotlimsCoolMod.Content.Items.Ammo
{
    public class DaylandSolutionItem : ModItem
    {

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.DefaultToSolution(ModContent.ProjectileType<DaylandSolutionProjectile>());
            Item.value = Item.buyPrice(0, 0, 25);
            Item.rare = ItemRarityID.Orange;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.Solutions;
        }

        // Please see Content/ExampleRecipes.cs for a detailed explanation of recipe creation.
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ItemID.DirtBlock)
                .Register();
        }
    }

    
}