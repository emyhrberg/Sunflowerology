using ScienceJam.Content.Tiles.SunflowerStagesOfGrowth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ScienceJam.Content.Items.Placeable
{
    internal class SunflowerSeedItem : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SproutTile>());
            Item.width = 30;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.value = 1000;
            Item.consumable = true;
        }
    }
}
