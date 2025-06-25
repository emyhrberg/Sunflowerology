using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ScienceJam.Common.Configs;
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
    internal class SaplingTile : PlantStageTile<SaplingEntity>
    {
        protected override int WidthInTiles => 2;
        protected override int HeightInTiles => 3;
        protected override bool IsSpecialHook => false;
        protected override bool HaveGlow => true;
        protected override int[] Heights => [16, 16, 18];
    }
    public class SaplingEntity : PlantStageEntity<SunflowerWithSeedsEntity>
    {
        protected override int TileType => ModContent.TileType<SaplingTile>();
        protected override int NextTileType => ModContent.TileType<SunflowerWithSeedsTile>();
    }

}
