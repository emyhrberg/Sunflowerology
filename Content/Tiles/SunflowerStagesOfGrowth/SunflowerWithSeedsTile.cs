using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;
using Terraria.ObjectData;

namespace ScienceJam.Content.Tiles.SunflowerStagesOfGrowth
{
    internal class SunflowerWithSeedsTile : PlantStageTile<SunflowerWithSeedsEntity>
    {
        protected override int WidthInTiles => 2;
        protected override int HeightInTiles => 4;
        protected override bool IsSpecialHook => false;
        protected override bool HaveGlow => true;
        protected override int[] Heights => [16, 16, 16, 18];

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
