using ScienceJam.Content.Items.SunflowerSeeds;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace ScienceJam.Content.Tiles.SunflowerStagesOfGrowth
{
    internal class SunflowerWithSeedsTile : PlantStageTile<SunflowerWithSeedsEntity>
    {
        protected override int WidthInTiles => 2;
        protected override int HeightInTiles => 4;
        protected override bool IsSpecialHook => false;
        protected override bool HaveGlow => true;
        protected override int[] Heights => [16, 16, 16, 18];

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            int id = ModContent.GetInstance<SunflowerWithSeedsEntity>().Find(i, j);
            if (TileEntity.ByID.TryGetValue(id, out TileEntity te) && te is SunflowerWithSeedsEntity ste)
            {
                var r = new Random();
                int seedItemIndex = Item.NewItem(
                    new EntitySource_TileBreak(i, j),
                    i * 16, j * 16, 16, 16,
                    NatureData.TypeOfSunflowerToSeedItemId[ste.typeOfSunflower],
                    r.Next(5, 11)

                );
                int flowerItemIndex = Item.NewItem(
                    new EntitySource_TileBreak(i, j),
                    i * 16, j * 16, 16, 16,
                    NatureData.TypeOfSunflowerToItemId[ste.typeOfSunflower],
                    1
                );

                (Main.item[seedItemIndex].ModItem as SeedItem).seedData = ste.plantData;
            }

            base.KillMultiTile(i, j, frameX, frameY);
        }
        protected override string GetmouseOverPlantText(int i, int j)
        {
            if (TileEntity.TryGet(i, j, out SunflowerWithSeedsEntity tileEntity))
            {
                string res = $"";
                foreach (var seedTag in NatureTags.AllTags)
                {
                    res += $"\n{seedTag}: {tileEntity.plantData[seedTag]}";
                }
                return res;
            }
            else
            {
                return "No tile entity found.";
            }
        }
    }
    public class SunflowerWithSeedsEntity : FinalPlantStageEntity
    {
        protected override int TileType => ModContent.TileType<SunflowerWithSeedsTile>();


    }

}
