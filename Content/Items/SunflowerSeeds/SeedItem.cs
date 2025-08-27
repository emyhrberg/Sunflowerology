using System.Collections.Generic;
using System.IO;
using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Sunflowerology.Content.Items.SunflowerSeeds
{
    internal abstract class SeedItem : ModItem
    {
        public NatureData seedData = new();
        protected abstract TypeOfSunflower TypeOfSunflower { get; }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SproutTile>());
            Item.width = 20;
            Item.height = 26;
            Item.value = 10;
            Item.placeStyle = 9 * (int)TypeOfSunflower;
            seedData = NatureData.TypeOfSunflowerToData[TypeOfSunflower];
        }

        public override bool CanStack(Item source)
        {
            foreach (var tag in NatureTags.AllTags)
            {
                if (seedData[tag] != ((SeedItem)source.ModItem).seedData[tag])
                {
                    return false;
                }
            }
            return true;
        }

        public override void SaveData(TagCompound tag)
        {
            foreach (var seedTag in NatureTags.AllTags)
            {
                tag[seedTag] = seedData[seedTag];
            }
        }

        public override void LoadData(TagCompound tag)
        {
            var newSeedData = new NatureData();
            foreach (var seedTag in NatureTags.AllTags)
            {
                try
                {
                    if (tag.TryGet(seedTag, out int val))
                    {
                        newSeedData[seedTag] = val;
                    }
                    else
                    {
                        newSeedData[seedTag] = 0;
                    }
                }
                catch
                {
                    newSeedData[seedTag] = 0; // If the tag is not found or fails, set it to 0
                }
            }
            seedData = newSeedData;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            foreach (var tag in NatureTags.AllTags)
            {
                TooltipLine tooltip = new TooltipLine(Mod, "Info: ", $"{tag}: {seedData[tag]}") { OverrideColor = Color.Yellow };
                tooltips.Add(tooltip);
            }

        }

        public override void NetSend(BinaryWriter writer)
        {
            foreach (var seedTag in NatureTags.AllTags)
            {
                writer.Write(seedData[seedTag]);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            var newSeedData = new NatureData();
            foreach (var seedTag in NatureTags.AllTags)
            {
                newSeedData[seedTag] = reader.ReadInt32();
            }
            seedData = newSeedData;
        }
    }
}
