using ScienceJam.Content.Tiles.SunflowerStagesOfGrowth;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ScienceJam.Content.Items.Placeable
{
    internal class SunflowerSeedItem : ModItem
    {
        public SproutData sproutData;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SproutTile>());
            Item.width = 30;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.value = 1000;
            Item.consumable = true;
        }

        public override void UpdateInventory(Player player)
        {
            if (sproutData.DryLove == 0)
            {
                Random random = new Random();
                sproutData.DryLove = random.Next(0, 100);
            }
            base.UpdateInventory(player);
        }

        public override bool CanStack(Item source)
        {
            return sproutData.DryLove == ((SunflowerSeedItem)source.ModItem).sproutData.DryLove;
        }

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(sproutData.DryLove)] = sproutData.DryLove;
        }

        public override void LoadData(TagCompound tag)
        {
            sproutData.DryLove = tag.GetInt(nameof(sproutData.DryLove));
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine tooltip = new TooltipLine(Mod, "Info: ", $"{sproutData.DryLove}") { OverrideColor = Color.Red };
            tooltips.Add(tooltip);
        }
    }
}
