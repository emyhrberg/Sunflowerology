using Sunflowerology.Content.Tiles.SunflowerStagesOfGrowth;
using Terraria.ID;

namespace Sunflowerology.Content.Tiles.Sunflower
{
    internal class SnowflowerTile : FlowerTile
    {
        protected override int RangeOfEffectInTiles => 20;

        protected override int EffectBuffID => BuffID.Warmth;

        protected override int EffetDuration => 30;

        protected override TypeOfSunflower TypeOfSunflower => TypeOfSunflower.Snowflower;
    }
}