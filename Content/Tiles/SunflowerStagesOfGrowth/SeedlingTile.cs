using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModLiquidLib.Utils;
using Sunflowerology.Common.Configs;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI;
using Terraria.ObjectData;

namespace Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth
{
    internal class SeedlingTile : PlantStageTile<SeedlingEntity>
    {
        protected override int WidthInTiles => 2;
        protected override int HeightInTiles => 2;

        protected override bool IsSpecialHook => false;

        protected override bool HaveGlow => false;

        protected override int[] Heights => [16, 18];
    }
    public class SeedlingEntity : PlantStageEntity<SaplingEntity>
    {
        protected override int TileType => ModContent.TileType<SeedlingTile>();

        protected override int NextTileType => ModContent.TileType<SaplingTile>();
    }

}
