using System;
using System.Collections.Generic;
using ScienceJam.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace ScienceJam.Content.Items.SunflowerSeeds
{
    internal class DrySunflowerSeedItem : ModItem
    {
        public SeedData sproutData = new();
        public int Dry = 5;//Dry, Water, Wild, Sun, Cave, Hot, Cold, Evil, Good, Honey
        public int Water = 30;
        public int Wild = 10;
        public int Sun = 50;
        public int Cave = 10;
        public int Hot = 5;
        public int Cold = 5;
        public int Evil = 0;
        public int Good = 0;
        public int Honey = 1;
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SproutTile>());
            Item.width = 22;
            Item.height = 22;
            Item.maxStack = 9999;
            Item.value = 1000;
            Item.consumable = true;
        }

        public override bool CanRightClick()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        public override void RightClick(Player player)
        {
#if DEBUG
            Random rand = new Random();
            Dry = rand.Next(0, 100);
            Water = rand.Next(0, 100);
            Wild = rand.Next(0, 100);
            Sun = rand.Next(0, 100);
            Cave = rand.Next(0, 100);
            Hot = rand.Next(0, 100);
            Cold = rand.Next(0, 100);
            Evil = rand.Next(0, 100);
            Good = rand.Next(0, 100);
            Honey = rand.Next(0, 100);
#endif
        }

        public override void UpdateInventory(Player player)
        {
            int[] intl = [Dry, Water, Wild, Sun, Cave, Hot, Cold, Evil, Good, Honey];
            for (int i = 0; i < 10; i++)
            {
                sproutData[SeedTags.AllTags[i]] = intl[i];
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
