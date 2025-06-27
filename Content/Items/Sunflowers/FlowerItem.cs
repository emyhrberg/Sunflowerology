using System;
using Sunflowerology.Content.Tiles.DeadSunflower;
using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ModLoader;

namespace Sunflowerology.Content.Items.Sunflowers
{
    internal abstract class FlowerItem : ModItem
    {
        protected abstract TypeOfSunflower TypeOfSunflower { get; }
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(NatureData.TypeOfSunflowerToTileId[TypeOfSunflower]);
            Item.width = 30;
            Item.height = 28;
            Item.maxStack = 9999;
            Item.value = 1000;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = Terraria.ID.ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.placeStyle = 3 * (int)TypeOfSunflower;
        }
    }
}
