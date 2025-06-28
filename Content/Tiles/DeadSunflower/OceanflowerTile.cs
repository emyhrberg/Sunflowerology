using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ID;

namespace Sunflowerology.Content.Tiles.DeadSunflower
{
    internal class OceanflowerTile : FlowerTile
    {
        protected override int RangeOfEffectInTiles => 20;

        protected override int EffectBuffID => BuffID.Gills;

        protected override int EffetDuration => 30;

        protected override TypeOfSunflower TypeOfSunflower => TypeOfSunflower.Oceanflower;
    }
}