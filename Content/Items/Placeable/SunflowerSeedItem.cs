using ScienceJam.Content.Tiles.SunflowerStagesOfGrowth;
using StructureHelper.Content.GUI;
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
        public SeedData sproutData = new();
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
            if (sproutData[SeedTags.Dry] == 0)
            {
                Random random = new Random();
                foreach (var tag in SeedTags.AllTags)
                {
                    sproutData[tag] = random.Next(0, 100);
                }
            }
            base.UpdateInventory(player);
        }

        public override bool CanStack(Item source)
        {
            foreach (var tag in SeedTags.AllTags)
            {
                if (sproutData[tag] != ((SunflowerSeedItem)source.ModItem).sproutData[tag])
                {
                    return false; // If any tag does not match, do not stack
                }
            }
            return true;
        }

        public override void SaveData(TagCompound tag)
        {
            foreach (var seedTag in SeedTags.AllTags)
            {
                tag[seedTag] = sproutData[seedTag];
            }
        }

        public override void LoadData(TagCompound tag)
        {
            foreach (var seedTag in SeedTags.AllTags)
            {
                try
                {
                    if (tag.TryGet(seedTag, out int val))
                    {
                        sproutData[seedTag] = val;
                    }
                    else
                    {
                        sproutData[seedTag] = 0;
                    }
                }
                catch
                {
                    sproutData[seedTag] = 0; // If the tag is not found or fails, set it to 0
                }
                
            }
        }


        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (var tag in SeedTags.AllTags)
            {
                TooltipLine tooltip = new TooltipLine(Mod, "Info: ", $"{tag}: {sproutData[tag]}") { OverrideColor = Color.Yellow };
                tooltips.Add(tooltip);
            }

        }

        public override bool? UseItem(Player player)
        {
            SproutEntity.transferData = sproutData;
            return base.UseItem(player);
        }
    }
}
